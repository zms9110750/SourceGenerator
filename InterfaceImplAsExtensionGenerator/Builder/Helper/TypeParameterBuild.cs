using System.Runtime.CompilerServices;

namespace zms9110750.InterfaceImplAsExtensionGenerator.Builder.Helper;


internal class TypeParameterBuild
{
    public INamedTypeSymbol TypeSymbol { get; }
    public IMethodSymbol? MethodSymbol { get; }
    public Dictionary<ITypeParameterSymbol, string> RenameMap { get; } = new(SymbolEqualityComparer.Default);
    public TypeParameterBuild(INamedTypeSymbol typeSymbol, IMethodSymbol? methodSymbol = null)
    {
        TypeSymbol = typeSymbol;
        MethodSymbol = methodSymbol;
        HashSet<string> usedNames = new();
        foreach (var param in GetTypeParameters())
        {
            string name = param.Name.EscapeKeywords(); 
            for (int i = 0; !usedNames.Add(name); i++)
            {
                name = param.Name + "_" + ToBase62(i);
            }
            RenameMap[param] = name;
        }
    }
    public string GetTypeParameterRenameOrDefault(ITypeParameterSymbol typeParameter)
    {
        return RenameMap.GetOrDefault(typeParameter, typeParameter.Name)!;
    }
    private static IEnumerable<INamedTypeSymbol> GetContainingTypes(INamedTypeSymbol typeSymbol)
    {
        for (var symbol = typeSymbol; symbol != null; symbol = symbol.ContainingType)
        {
            yield return symbol;
        }
    }

    public IEnumerable<ITypeParameterSymbol> GetTypeParametersClassOnly()
    {
        return GetContainingTypes(TypeSymbol)
            .SelectMany(type => type.TypeArguments)
            .OfType<ITypeParameterSymbol>();
    }

    public IEnumerable<ITypeParameterSymbol> GetTypeParametersMethodOnly()
    {
        if (MethodSymbol == null)
        {
            return Enumerable.Empty<ITypeParameterSymbol>();
        }

        return MethodSymbol.TypeArguments.OfType<ITypeParameterSymbol>();
    }

    public IEnumerable<ITypeParameterSymbol> GetTypeParameters()
    {
        return GetTypeParametersClassOnly()
            .Concat(GetTypeParametersMethodOnly());
    }

    public IEnumerable<string> GetTypeParametersNameClassOnly()
    {
        return GetTypeParametersClassOnly().Select(GetTypeParameterRenameOrDefault);
    }

    public IEnumerable<string> GetTypeParametersNameMethodOnly()
    {
        return GetTypeParametersMethodOnly().Select(GetTypeParameterRenameOrDefault);
    }

    public IEnumerable<string> GetTypeParametersName()
    {
        return GetTypeParametersNameClassOnly()
            .Concat(GetTypeParametersNameMethodOnly());
    }
    public StringBuilder FullyQualifiedFormat(StringBuilder? builder = null)
    {
        return FullyQualifiedFormat(TypeSymbol, builder ?? new StringBuilder());
    }
    public StringBuilder FullyQualifiedFormat(ITypeSymbol typeSymbol, StringBuilder? builder = null)
    {
        builder ??= new StringBuilder();
        if (typeSymbol is not INamedTypeSymbol namedType || !GetContainingTypes(namedType).SelectMany(t => t.TypeArguments).Any(t => t is ITypeParameterSymbol))
        {
            return builder.Append(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        }

        if (typeSymbol.ContainingType != null)
        {
            FullyQualifiedFormat(typeSymbol.ContainingType, builder);
            builder.Append(".");
        }
        else
        {
            if (typeSymbol.ContainingNamespace.IsGlobalNamespace)
            {
                builder.Append("global::");
            }
            else
            {
                builder.Append(typeSymbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                builder.Append(".");
            }
        }

        builder.Append(namedType.Name.EscapeKeywords());
        if (namedType.TypeArguments.Length > 0)
        {
            builder.Append("<");
            for (int i = 0; i < namedType.TypeArguments.Length; i++)
            {
                var argum = namedType.TypeArguments[i];
                if (i > 0)
                {
                    builder.Append(", ");
                }
                if (argum is ITypeParameterSymbol typeParameter)
                {
                    builder.Append(GetTypeParameterRenameOrDefault(typeParameter));
                }
                else
                {
                    builder.Append(argum.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                }
            }
            builder.Append(">");
        }

        return builder;
    }
    public StringBuilder AppendConstraintClauses(StringBuilder? builder = null)
    {
        return AppendConstraintClausesInternal(builder ?? new StringBuilder(), GetTypeParameters());
    }

    public StringBuilder AppendConstraintClausesClassOnly(StringBuilder? builder = null)
    {
        return AppendConstraintClausesInternal(builder ?? new StringBuilder(), GetTypeParametersClassOnly());
    }

    public StringBuilder AppendConstraintClausesMethodOnly(StringBuilder? builder = null)
    {
        return AppendConstraintClausesInternal(builder ?? new StringBuilder(), GetTypeParametersMethodOnly());
    }

    private StringBuilder AppendConstraintClausesInternal(
        StringBuilder builder,
        IEnumerable<ITypeParameterSymbol> typeParameters)
    {
        foreach (var typeParam in typeParameters)
        {
            AppendConstraintClause(typeParam, builder);
        }
        return builder;
    }

    private void AppendConstraintClause(ITypeParameterSymbol typeParam, StringBuilder builder)
    {
        var constraints = new List<string>();

        // 1. 类约束 (class)
        if (typeParam.HasReferenceTypeConstraint)
        {
            constraints.Add("class");
        }

        // 2. 结构体约束 (struct)
        if (typeParam.HasValueTypeConstraint)
        {
            constraints.Add("struct");
        }

        // 3. 非托管约束 (unmanaged)
        if (typeParam.HasUnmanagedTypeConstraint)
        {
            constraints.Add("unmanaged");
        }

        // 4. notnull 约束
        if (typeParam.HasNotNullConstraint)
        {
            constraints.Add("notnull");
        }

        // 5. 类型约束
        foreach (var constraintType in typeParam.ConstraintTypes)
        { 
            constraints.Add(FullyQualifiedFormat(constraintType).ToString());
        }

        // 6. 允许 ref 结构约束
        if (typeParam.AllowsRefLikeType)
        {
            constraints.Add("allows ref struct");
        }

        // 7. 构造函数约束
        if (typeParam.HasConstructorConstraint)
        {
            constraints.Add("new()");
        }

        if (constraints.Count == 0)
        {
            return;
        }

        // 每个 where 子句单独一行
        builder.Append("where ").Append(GetTypeParameterRenameOrDefault(typeParam)).Append(" : ");
        builder.Append(string.Join(", ", constraints));
        builder.AppendLine();
    }
    private static string ToBase62(int number)
    {
        const string Base62Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        if (number < 0)
            throw new ArgumentOutOfRangeException(nameof(number));
        if (number < 62)
        {
            return Base62Chars[number].ToString();
        }

        var result = new StringBuilder();
        while (number > 0)
        {
            result.Insert(0, Base62Chars[number % 62]);
            number /= 62;
        }
        return result.ToString();
    }
}