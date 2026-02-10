

using System.Collections;

namespace zms9110750.MetaSourceGenerator.AttributeFactory.Builder
{
    abstract class BaseBuilder:IEnumerable<Diagnostic>
    {
        protected readonly string AttributeFullName;
        protected readonly INamedTypeSymbol AttributeSymbol;
        protected readonly ImmutableArray<IMethodSymbol> NoHasTypeConstructors = ImmutableArray<IMethodSymbol>.Empty;
        protected readonly ImmutableArray<IPropertySymbol> NoHasTypeWritableProperties = ImmutableArray<IPropertySymbol>.Empty;
        protected readonly ImmutableArray<IMethodSymbol> HasTypeConstructors = ImmutableArray<IMethodSymbol>.Empty;
        protected readonly ImmutableArray<IPropertySymbol> HasTypeWritableProperties = ImmutableArray<IPropertySymbol>.Empty;
        public BaseBuilder(INamedTypeSymbol attributeSymbol)
        {
            AttributeSymbol = attributeSymbol;
            AttributeFullName = AttributeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            var constructors = AttributeSymbol.Constructors
                .Where(IsValidMember);
            var property = AttributeSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(IsValidMember);

            foreach (var group in constructors.GroupBy(s => s.Parameters.Any(p => IsTypeNamedType(p.Type))))
            {
                switch (group.Key)
                {
                    case true:
                        HasTypeConstructors = group.ToImmutableArray();
                        break;
                    case false:
                        NoHasTypeConstructors = group.ToImmutableArray();
                        break;
                }
            }
            foreach (var group in property.GroupBy(s => IsTypeNamedType(s.Type)))
            {
                switch (group.Key)
                {
                    case true:
                        HasTypeWritableProperties = group.ToImmutableArray();
                        break;
                    case false:
                        NoHasTypeWritableProperties = group.ToImmutableArray();
                        break;
                }
            }
        }
        public abstract CompilationUnitSyntax Generate();

        public bool ShouldGenerateMethod(IMethodSymbol methodSymbol)
        {
            if (methodSymbol.MethodKind != MethodKind.Constructor)
            {
                return false;
            }
            var format = SymbolDisplayFormat.FullyQualifiedFormat;
            var constructors = methodSymbol.ContainingType.Constructors;

            return constructors.Any(constructor => AreParametersCompatible(methodSymbol, constructor, format));

            bool AreParametersCompatible(IMethodSymbol source, IMethodSymbol target, SymbolDisplayFormat format)
            {
                if (!IsValidMemberAccessibility(target.DeclaredAccessibility))
                {
                    return false;
                }
                if (target.IsStatic)
                {
                    return false;
                }
                if (source.Parameters.Length != target.Parameters.Length)
                {
                    return false;
                }

                for (int i = 0; i < source.Parameters.Length; i++)
                {
                    var sourceType = source.Parameters[i].Type.ToDisplayString(format);
                    var targetType = target.Parameters[i].Type.ToDisplayString(format);

                    switch (sourceType)
                    {
                        case "global::System.Type" when targetType != "global::Microsoft.CodeAnalysis.INamedTypeSymbol":
                        case "global::System.Type[]" when targetType != "global::Microsoft.CodeAnalysis.INamedTypeSymbol[]":
                            return false;
                        case "global::System.Type" or "global::System.Type[]":
                            continue;
                        default:
                            if (sourceType != targetType)
                            {
                                return false;
                            }
                            continue;
                    }
                }

                return true;
            }
        }
        public abstract string? GetPropertyNameIfShouldGenerate(IPropertySymbol propertySymbol);

        protected string GenerateMethodBody()
        {
            return $$"""
        {
        if (data == null)
            {
                return null;
            }
            var format = global::Microsoft.CodeAnalysis.SymbolDisplayFormat.FullyQualifiedFormat;
            if (data.AttributeClass.ToDisplayString(format) != "{{AttributeFullName}}")
            {
                return null;
            }

            {{AttributeFullName}} value = null;
            switch (string.Join("|", global::System.Linq.Enumerable.Select(data.AttributeConstructor.Parameters, p => p.Type.ToDisplayString(format))))
            {
                {{string.Join("\n", NoHasTypeConstructors.Select(CaseMethod))}}
                {{string.Join("\n", HasTypeConstructors.Where(ShouldGenerateMethod).Select(CaseMethod))}}
                default:
                    return null;
            }
            foreach (var item in data.NamedArguments)
            {
                switch (item.Key)
                {
                    {{string.Join("\n", NoHasTypeWritableProperties.Select(CaseProperty))}}
                    {{string.Join("\n", HasTypeWritableProperties.Where(p => GetPropertyNameIfShouldGenerate(p) != null).Select(CaseProperty))}}
                }
            }
            return value;
        }
        """;
        }

        string CaseMethod(IMethodSymbol symbol)
        {
            var parameters = string.Join("|", symbol.Parameters.Select(p => p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));

            int index = 0;
            return $$"""
            case "{{parameters}}":
                value = new {{AttributeFullName}}({{string.Join(", ", symbol.Parameters.Select(p => ConstructorParameters(p, index++)))}});
                break;
            """;
            string ConstructorParameters(IParameterSymbol symbol, int index)
            {
                if (symbol.Type is IArrayTypeSymbol arr)
                {
                    var typeFullName = arr.ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    if (typeFullName == "global::System.Type")
                    {
                        typeFullName = "global::Microsoft.CodeAnalysis.INamedTypeSymbol";
                    }
                    return $$"""
                        System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(data.ConstructorArguments[{{index}}].Values, s => ({{typeFullName}})s.Value))
                        """;
                }
                else
                {
                    var typeFullName = symbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    if (typeFullName == "global::System.Type")
                    {
                        typeFullName = "global::Microsoft.CodeAnalysis.INamedTypeSymbol";
                    }
                    return $$"""
                        ({{typeFullName}})data.ConstructorArguments[{{index}}].Value
                        """;
                }
            }
        }

        string CaseProperty(IPropertySymbol symbol)
        {
            bool isArray = false;
            var name = symbol.Name;
            var typeFullName = symbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (symbol.Type is IArrayTypeSymbol arrayType)
            {
                typeFullName = arrayType.ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                isArray = true;
            }
            if (typeFullName == "global::System.Type")
            {
                name = GetPropertyNameIfShouldGenerate(symbol);
                if (name == null)
                {
                    return "";
                }
                typeFullName = "global::Microsoft.CodeAnalysis.INamedTypeSymbol";
            }
            string valueExpression = isArray
                ? $"System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(item.Value.Values, s => ({typeFullName})s.Value))"
                : $"({typeFullName})item.Value.Value";
            return $$"""
                    case "{{symbol.Name}}":
                        value.{{name}} = {{valueExpression}};
                        break;
                    """;
        }

        public static bool IsValidAttributeParameterType(ITypeSymbol type)
        {
            if (type is IArrayTypeSymbol arrayType)
            {
                return arrayType.ElementType is not IArrayTypeSymbol && IsValidAttributeParameterType(arrayType.ElementType);
            }

            switch (type.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_Byte:
                case SpecialType.System_SByte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_String:
                case SpecialType.System_Char:
                    return true;
            }

            if (type.TypeKind == TypeKind.Enum)
                return true;

            if (type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.Type")
                return true;

            return false;
        }
        public static bool IsTypeNamedType(ITypeSymbol type)
        {
            if (type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.Type")
            {
                return true;
            }
            else if (type is IArrayTypeSymbol { ElementType: not IArrayTypeSymbol and var eleType })
            {
                return IsTypeNamedType(eleType);
            }
            return false;
        }
        public bool IsValidMember(ISymbol symbol)
        {
            if (!IsValidMemberAccessibility(symbol.DeclaredAccessibility))
            {
                return false;
            }
            if (symbol.IsStatic || symbol.IsAbstract)
            {
                return false;
            }
            return symbol switch
            {
                IMethodSymbol { MethodKind: MethodKind.Constructor } methodSymbol => methodSymbol.Parameters.All(s => IsValidAttributeParameterType(s.Type)),
                IPropertySymbol { SetMethod.IsInitOnly: false } propertySymbol => IsValidAttributeParameterType(propertySymbol.Type),
                _ => false
            };
        }
        public abstract bool IsValidMemberAccessibility(Accessibility accessibility);
        public abstract IEnumerator<Diagnostic> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract string? FileName { get; }
    }
}