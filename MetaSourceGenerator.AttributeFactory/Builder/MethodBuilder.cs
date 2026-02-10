using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using zms9110750.MetaSourceGenerator.AttributeFactory.DiagnosticDefine;

namespace zms9110750.MetaSourceGenerator.AttributeFactory.Builder
{
    class MethodBuilder : BaseBuilder
    {
        private readonly IMethodSymbol _methodSymbol;
        private readonly MethodDeclarationSyntax _methodSyntax;
        private readonly string _attributeFullName;
        private readonly string _parameterName;

        public override string? FileName { get; }

        public MethodBuilder(IMethodSymbol methodSymbol, MethodDeclarationSyntax methodSyntax) : base((INamedTypeSymbol)methodSymbol.ReturnType)
        {
            _methodSymbol = methodSymbol;
            _methodSyntax = methodSyntax;
            _attributeFullName = _methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            _parameterName = _methodSymbol.Parameters[0].Name;
            FileName = methodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))
                        .Replace("<", "{")
                        .Replace(">", "}")
                        + ".g.cs";

            if (this.Any(s => s.Descriptor.DefaultSeverity == DiagnosticSeverity.Error))
            {
                FileName = null;
            }
        }

        public override CompilationUnitSyntax Generate()
        {
            if (_methodSymbol.ContainingNamespace.IsGlobalNamespace)
            {
                return SyntaxFactory.CompilationUnit().AddMembers(Class());
            }
            else
            {
                return SyntaxFactory.CompilationUnit().AddMembers(
                    SyntaxFactory.NamespaceDeclaration(
                        SyntaxFactory.ParseName(_methodSymbol.ContainingNamespace.ToDisplayString())
                    )
                    .AddMembers(Class())
                );
            }
        }
        ClassDeclarationSyntax Class()
        {
            return SyntaxFactory.ClassDeclaration(_methodSymbol.ContainingType.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(ConvertToFullQualified());
        }


        public MethodDeclarationSyntax ConvertToFullQualified()
        {
            // 返回类型完全限定名
            var returnType = SyntaxFactory.ParseTypeName(
                _methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            );

            // 参数完全限定名
            var paramType = SyntaxFactory.ParseTypeName(
                _methodSymbol.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            );

            var parameter = SyntaxFactory.Parameter(
                _methodSyntax.ParameterList.Parameters[0].Identifier
            ).WithType(paramType)
            .WithModifiers(_methodSyntax.ParameterList.Parameters[0].Modifiers)
            ;

            return SyntaxFactory.MethodDeclaration(
                returnType,
                _methodSyntax.Identifier
            )
            .WithModifiers(_methodSyntax.Modifiers)  // 复制原修饰符
            .WithParameterList(
                SyntaxFactory.ParameterList(
                    SyntaxFactory.SingletonSeparatedList(parameter)
                )
            )
           .WithBody(CreateFactoryMethodBody());
        }
        public BlockSyntax CreateFactoryMethodBody()
        {
            string code = $$"""
                {
                return Creat({{_parameterName}});
                {{_attributeFullName}} Creat(global::Microsoft.CodeAnalysis.AttributeData data)
                {{GenerateMethodBody()}}
                }
                """;
            // 解析为语法块
            return (BlockSyntax)SyntaxFactory.ParseStatement(code);
        }

        public override string? GetPropertyNameIfShouldGenerate(IPropertySymbol propertySymbol)
        {
            return null;
        }

        public override bool IsValidMemberAccessibility(Accessibility accessibility)
        {
            return accessibility switch
            {
                Accessibility.Public => true,
                _ => false
            };
        }
         
        public override IEnumerator<Diagnostic> GetEnumerator()
        {
            Location location = _methodSyntax!.GetLocation();
            foreach (var item in _methodSymbol!.GetAttributes())
            {
                if (item.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::zms9110750.MetaSourceGenerator.AttributeFactory.FromAttributeDataAttribute")
                {
                    location = item.ApplicationSyntaxReference!.GetSyntax().GetLocation();
                    break;
                }
            }
            // 1. 检查方法是否只有一个参数
            if (_methodSymbol.Parameters.Length != 1)
            {
                yield return Diagnostic.Create(FromAttributeDiagnostic.ZMS001, location);
            }

            // 2. 检查参数类型是否为 AttributeData
            var paramType = _methodSymbol.Parameters[0].Type;
            if (paramType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) !=
                "global::Microsoft.CodeAnalysis.AttributeData")
            {
                yield return Diagnostic.Create(FromAttributeDiagnostic.ZMS006, location);
            }

            // 3. 检查是否有 ref/in/out/params 参数
            var parameters = _methodSymbol.Parameters;
            foreach (var parameter in parameters)
            {
                if (parameter.RefKind != RefKind.None || parameter.IsParams)
                {
                    yield return Diagnostic.Create(FromAttributeDiagnostic.ZMS002, location);
                }
            }

            // 4. 检查是否为泛型方法
            if (_methodSymbol.IsGenericMethod)
            {
                yield return Diagnostic.Create(FromAttributeDiagnostic.ZMS003, location);
            }

            // 5. 检查返回值是否为 Attribute 或其派生类
            var returnType = _methodSymbol.ReturnType;

            // 检查是否为抽象类
            if (returnType.IsAbstract)
            {
                yield return Diagnostic.Create(FromAttributeDiagnostic.ZMS005, location);
            }

            // 检查是否继承自 System.Attribute
            bool isAttributeDerived = false;
            for (ITypeSymbol? currentType = returnType; currentType != null; currentType = currentType.BaseType)
            {
                if (currentType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.Attribute")
                {
                    isAttributeDerived = true;
                    break;
                }
            }

            if (!isAttributeDerived)
            {
                yield return Diagnostic.Create(FromAttributeDiagnostic.ZMS004, location);
            }

            if (HasTypeConstructors.Where(p => !ShouldGenerateMethod(p)).Any())
            {
                yield return Diagnostic.Create(FromAttributeDiagnostic.ZMS022, location);
            }
            if (!HasTypeWritableProperties.IsEmpty)
            {
                yield return Diagnostic.Create(FromAttributeDiagnostic.ZMS007, location);
            }

        }
#if false
        DescriptionAttribute Creat(AttributeData data)
        {
            if (data == null)
            {
                return null;
            }
            var format = global::Microsoft.CodeAnalysis.SymbolDisplayFormat.FullyQualifiedFormat;
            if (data.AttributeClass.ToDisplayString(format) != "global::System.ComponentModel.DescriptionAttribute")
            {
                return null;
            }

            DescriptionAttribute? value = null;
            switch (string.Join("|", global::System.Linq.Enumerable.Select(data.AttributeConstructor.Parameters, p => p.Type.ToDisplayString(format))))
            {
                case "":
                    value = new DescriptionAttribute();
                    break;
                case "int":
                    value = new DescriptionAttribute((int)data.ConstructorArguments[0].Value, (int[])System.Linq.Enumerable.ToArray(data.ConstructorArguments[1].Values));
                    break;
                default:
                    return null;
            }
            foreach (var item in data.NamedArguments)
            {
                switch (item.Key)
                {
                    case "Description":
                        value.Description = (string)item.Value.Value;
                        break;
                    case "Name":
                        var a = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(item.Value.Values, s => (int)s.Value));

                        break;
                }
            }
            return value;
        }
#endif
    }
}