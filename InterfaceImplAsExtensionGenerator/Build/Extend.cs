namespace zms9110750.InterfaceImplAsExtensionGenerator.Build;

internal static class Extend
{
    public static StringBuilder AppendJoin<T>(this StringBuilder sb, string? separator, IEnumerable<T?> values)
    {
        bool noFirst = false;
        foreach (var item in values)
        {
            if (noFirst)
            {
                sb.Append(separator);
            }
            else
            {
                noFirst = true;
            }
            sb.Append(item);
        }
        return sb;
    }
    /// <summary>
    /// 生成泛型约束代码
    /// </summary>
    /// <param name="typeParameters">泛型参数的迭代器</param>
    /// <param name="constraintClauses">泛型约束的声明的迭代器</param>
    /// <returns></returns>
    public static IEnumerable<TypeParameterConstraintClauseSyntax> ProcessConstraints(
     IEnumerable<ITypeParameterSymbol> typeParameters,
     IEnumerable<TypeParameterConstraintClauseSyntax> constraintClauses)
    {
        var constraints = typeParameters
            .Join(constraintClauses,
                tp => tp.Name,
                clause => clause.Name.Identifier.Text,
                (tp, clause) => (TypeParameter: tp, ConstraintClause: clause));

        foreach (var (typeParam, clause) in constraints)
        {
            if (!typeParam.ConstraintTypes.Any(ct => ct is INamedTypeSymbol and not IErrorTypeSymbol))
            {
                yield return clause;
                continue;
            }

            bool replaced = false;
            var newConstraints = new List<TypeParameterConstraintSyntax>();
            var validTypes = typeParam.ConstraintTypes
                .Select(constraintType => constraintType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                .ToImmutableHashSet();

            foreach (var constraint in clause.Constraints)
            {
                if (constraint is TypeConstraintSyntax typeConstraint && typeConstraint.Type.ToString() is not ("unmanaged" or "notnull"))
                {
                    if (replaced)
                    {
                        continue;
                    }
                    newConstraints.AddRange(validTypes.Select(validType => SyntaxFactory.TypeConstraint(SyntaxFactory.ParseTypeName(validType))));
                    replaced = true;
                }
                else
                {
                    newConstraints.Add(constraint);
                }
            }

            yield return clause.WithConstraints(SyntaxFactory.SeparatedList(newConstraints));

        }
    }
    /// <summary>
    /// 从泛型参数符号生成约束子句
    /// </summary>
    /// <param name="typeParameters">泛型参数符号</param>
    /// <returns>生成的约束子句列表</returns>
    public static IEnumerable<TypeParameterConstraintClauseSyntax> CreateConstraintClauses(
        IEnumerable<ITypeParameterSymbol> typeParameters)
    {
        foreach (var typeParam in typeParameters)
        {
            var constraints = new List<TypeParameterConstraintSyntax>();

            // 1. 类约束 (class)
            if (typeParam.HasReferenceTypeConstraint)
            {
                constraints.Add(
                    SyntaxFactory.ClassOrStructConstraint(SyntaxKind.ClassConstraint));
            }
            // 2. 结构体约束 (struct)
            if (typeParam.HasValueTypeConstraint)
            {
                constraints.Add(
                    SyntaxFactory.ClassOrStructConstraint(SyntaxKind.StructConstraint));
            }
            // 3. 非托管约束 (unmanaged)
            if (typeParam.HasUnmanagedTypeConstraint)
            {
                constraints.Add(
                    SyntaxFactory.TypeConstraint(
                        SyntaxFactory.ParseTypeName("unmanaged")));
            }
            // 4. notnull 约束
            if (typeParam.HasNotNullConstraint)
            {
                constraints.Add(
                    SyntaxFactory.TypeConstraint(
                        SyntaxFactory.ParseTypeName("notnull")));
            }

            // 5. 类型约束 (where T : BaseType, IInterface)
            foreach (var constraintType in typeParam.ConstraintTypes)
            {
                constraints.Add(
                      SyntaxFactory.TypeConstraint(
                          SyntaxFactory.ParseTypeName(
                              constraintType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))));
            }
            if (typeParam.AllowsRefLikeType)
            {
                constraints.Add(SyntaxFactory.AllowsConstraintClause(SyntaxFactory.SingletonSeparatedList<AllowsConstraintSyntax>(  SyntaxFactory.RefStructConstraint())));
            }
            // 6. 构造函数约束 (new())
            if (typeParam.HasConstructorConstraint) // 结构体默认就有构造函数
            { 
                constraints.Add(SyntaxFactory.ConstructorConstraint());
            }

            // 7. 默认约束 (default)
            if (typeParam.HasUnmanagedTypeConstraint)
            {
                constraints.Add(SyntaxFactory.DefaultConstraint());
            }

            // 如果有约束，创建约束子句
            if (constraints.Count > 0)
            {
                yield return SyntaxFactory.TypeParameterConstraintClause(
                    SyntaxFactory.IdentifierName(typeParam.Name))
                    .WithConstraints(
                        SyntaxFactory.SeparatedList(constraints));
            }
        }
    }
}
