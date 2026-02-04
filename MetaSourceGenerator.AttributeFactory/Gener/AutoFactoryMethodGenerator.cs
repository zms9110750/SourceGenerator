using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using zms9110750.MetaSourceGenerator.AttributeFactory.Converter;

namespace zms9110750.MetaSourceGenerator.AttributeFactory.Gener
{
    [Generator]
    class AutoFactoryMethodGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var methodProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
            "zms9110750.MetaSourceGenerator.AttributeFactory.FromAttributeDataAttribute",
            predicate: static (node, token) => node is MethodDeclarationSyntax,
            transform: static (ctx, token) => (Syntax: ctx.TargetNode as MethodDeclarationSyntax, Symbol: ctx.TargetSymbol as IMethodSymbol))
             .Where(static x => x.Syntax != null && x.Symbol != null);

            context.RegisterSourceOutput(methodProvider, (spc, tuple) =>
            {
                var (syntax, symbol) = tuple;
                Location location = syntax!.GetLocation();
                foreach (var item in symbol!.GetAttributes())
                {
                    if (item.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::zms9110750.MetaSourceGenerator.AttributeFactory.FromAttributeDataAttribute")
                    {
                        location = item.ApplicationSyntaxReference!.GetSyntax().GetLocation();
                        break;
                    }
                }
                if (FromAttributeDiagnostic.IsValidFromAttribute(symbol!) is { } descriptor)
                {


                    var diagnostic = Diagnostic.Create(descriptor, location: location);
                    spc.ReportDiagnostic(diagnostic);
                    return;
                }
                try
                { 
                    var sourceName = tuple.Symbol!.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))
                        .Replace("<", "{").Replace(">", "}") + "_" + tuple.Symbol.Name + ".g.cs";
                    var converter = new MethodConverter(symbol!, syntax!);

                    spc.AddSource(sourceName, converter.Generate().NormalizeWhitespace().ToFullString());

                    foreach (var item in converter.Diagnostics())
                    {
                        var diagnostic = Diagnostic.Create(item, location: location);
                        spc.ReportDiagnostic(diagnostic);
                    }
                }
                catch (System.Exception ex)
                {
                    spc.AddSource($"Error_{symbol!.ContainingType.Name}_{symbol.Name}_{Guid.NewGuid():N}", ex.ToString());
                }
            });
        }
    }
}