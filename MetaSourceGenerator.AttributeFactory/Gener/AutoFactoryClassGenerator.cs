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
    class AutoFactoryClassGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classProvider = context.SyntaxProvider
             .ForAttributeWithMetadataName(
             "zms9110750.MetaSourceGenerator.AttributeFactory.FromAttributeDataAttribute",
                 predicate: static (node, token) => node is ClassDeclarationSyntax,
                 transform: static (ctx, token) => (Syntax: ctx.TargetNode as ClassDeclarationSyntax, Symbol: ctx.TargetSymbol as INamedTypeSymbol))
              .Where(static x => x.Syntax != null && x.Symbol != null);

            context.RegisterSourceOutput(classProvider, (spc, tuple) =>
            {
                var (syntax, symbol) = tuple;
                Location location = syntax!.GetLocation();
                // 查找 FromAttributeDataAttribute 的位置
                foreach (var item in symbol!.GetAttributes())
                {
                    if (item.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ==
                        "global::zms9110750.MetaSourceGenerator.AttributeFactory.FromAttributeDataAttribute")
                    {
                        location = item.ApplicationSyntaxReference!.GetSyntax().GetLocation();
                        break;
                    }
                }
                // 调用类的验证方法
                if (FromAttributeDiagnostic.IsValidFromAttribute(symbol!) is { } descriptor)
                {


                    var diagnostic = Diagnostic.Create(descriptor, location: location);
                    spc.ReportDiagnostic(diagnostic);
                    return;
                }

                try
                {
                    // 生成源文件名
                    var sourceName = symbol!.ToDisplayString(
                        SymbolDisplayFormat.FullyQualifiedFormat
                            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))
                        .Replace("<", "{")
                        .Replace(">", "}")
                        + ".g.cs";
                    var converter = new ClassConverter(symbol!, syntax!); 
                    spc.AddSource(sourceName, converter.Generate().NormalizeWhitespace().ToFullString());
                    foreach (var item in converter.Diagnostics())
                    {
                        var diagnostic = Diagnostic.Create(item, location: location);
                        spc.ReportDiagnostic(diagnostic);
                    }

                }
                catch (System.Exception ex)
                {
                    spc.AddSource($"Error_{symbol!.Name}_{Guid.NewGuid():N}", ex.ToString());
                }
            });
        }
    }
}