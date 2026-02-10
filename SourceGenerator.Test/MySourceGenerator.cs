using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

[Generator]
public class MySourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methodData = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "System.ComponentModel.DescriptionAttribute",
                (node, _) => node is MethodDeclarationSyntax,
                (ctx, _) => (
                    Syntax: (MethodDeclarationSyntax)ctx.TargetNode,
                    Symbol: (IMethodSymbol)ctx.TargetSymbol
                ))
            .Where(data => data.Syntax != null && data.Symbol != null);

        context.RegisterSourceOutput(methodData, (ctx, data) =>
        {
            var sourceCode = "/*\n";
            sourceCode += "// 生成的代码\n";
            sourceCode += "// 方法: " + data.Symbol.Name + "\n";

            var processedMethod = Process(data.Symbol, data.Syntax);
            sourceCode += processedMethod.ToFullString() + "\n";
            sourceCode += "*/\n";

            ctx.AddSource($"{data.Symbol.Name}_generated.g.cs", sourceCode);
        });
    }

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

    static MethodDeclarationSyntax Process(IMethodSymbol symbol, MethodDeclarationSyntax syntax)
    {
        var newClauses = new List<TypeParameterConstraintClauseSyntax>();
         var constraints = symbol.TypeParameters
            .Join(syntax.ConstraintClauses,
                tp => tp.Name,
                clause => clause.Name.Identifier.Text,
                (tp, clause) => (tp, clause));

        foreach (var (typeParam, clause) in constraints)
        {
            if (!typeParam.ConstraintTypes.Any(ct => ct is INamedTypeSymbol and not IErrorTypeSymbol))
            {
                newClauses.Add(clause);
                continue;
            }

            bool replaced = false;
            var newConstraints = new List<TypeParameterConstraintSyntax>();
            var validTypes = typeParam.ConstraintTypes.Select(constraintType => constraintType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).ToImmutableHashSet();

            foreach (var constraint in clause.Constraints)
            { 

                if (constraint is TypeConstraintSyntax typeConstraint && typeConstraint.Type.ToString() is not "unmanaged" or "notnull")
                {
                    if (replaced)
                    {
                        continue;
                    }
                    replaced = true;
                    newConstraints.AddRange(validTypes.Select(validType => SyntaxFactory.TypeConstraint(SyntaxFactory.ParseTypeName(validType))));
                }
                else
                {
                    newConstraints.Add(constraint);
                }
            }

            newClauses.Add(clause.WithConstraints(SyntaxFactory.SeparatedList(newConstraints)));
        }

        return syntax.WithConstraintClauses(SyntaxFactory.List(newClauses));
    }
}