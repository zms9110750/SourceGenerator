using System.Runtime.CompilerServices;
using zms9110750.InterfaceImplAsExtensionGenerator.Build;

namespace zms9110750.InterfaceImplAsExtensionGenerator.Gener;

// 第二个生成器：处理 ExtensionForAttribute（类特性）
[Generator]
class ClassExtensionForGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var analyzers = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                 "zms9110750.InterfaceImplAsExtensionGenerator.ExtensionForAttribute",
                (node, _) => true,
                (ctx, _) =>
                {
                    return (INamedTypeSymbol)ctx.TargetSymbol;
                });
        context.RegisterSourceOutput(analyzers, (ctx, classSymbol) =>
        {
            try
            {
                var analyzer = new ClassBuild(classSymbol);
                foreach (var item in analyzer)
                {
                    ctx.ReportDiagnostic(item);
                }
                foreach (var item in analyzer.InterfaceBuilds)
                {
                    try
                    {
                        if (item.FileName != null)
                        {
                            ctx.AddSource(item.FileName, item.Generate().NormalizeWhitespace().ToFullString());
                        }
                        foreach (var item2 in item)
                        {
                            ctx.ReportDiagnostic(item2);
                        }
                    }
                    catch (Exception ex)
                    {
                        ctx.AddSource($"Error_Class_{Guid.NewGuid():N}.g.cs", $"/* Error: {ex} */");
                    }
                }
            }
            catch (Exception ex2)
            {
                ctx.AddSource($"Error_Class_{Guid.NewGuid():N}.g.cs", $"/* Error: {ex2} */");
            }
        });
    }
}