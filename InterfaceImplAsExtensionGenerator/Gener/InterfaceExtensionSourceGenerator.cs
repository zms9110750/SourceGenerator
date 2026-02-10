using System.Runtime.CompilerServices;
using zms9110750.InterfaceImplAsExtensionGenerator.Build;

namespace zms9110750.InterfaceImplAsExtensionGenerator.Gener;
// 第一个生成器：处理 ExtensionSourceAttribute（接口特性）
[Generator]
class InterfaceExtensionSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var analyzers = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "zms9110750.InterfaceImplAsExtensionGenerator.ExtensionSourceAttribute",
                (node, _) => true,
                (ctx, _) =>
                {
                    var att = ctx.Attributes.Select(ExtensionSourceAttribute.Creat).FirstOrDefault(att => att != null);
                    return att != null ? new InterfaceBuild(att, (INamedTypeSymbol)ctx.TargetSymbol) : null;
                })
            .Where(analyzer => analyzer != null);

        context.RegisterSourceOutput(analyzers, (ctx, analyzer) =>
        {
            try
            {
                if (analyzer.FileName != null)
                {
                    ctx.AddSource(analyzer.FileName, analyzer.Generate().NormalizeWhitespace().ToFullString());
                }
                foreach (var item in analyzer)
                {
                    ctx.ReportDiagnostic(item);
                }
            }
            catch (Exception ex)
            {
                ctx.AddSource($"Error_Interface_{Guid.NewGuid():N}.g.cs", $"/* Error: {ex} */");
            }
        });
    }
}
