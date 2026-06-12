using zms9110750.StaticMethodAsExtensionGenerator.Builder;

namespace zms9110750.StaticMethodAsExtensionGenerator.Gener;

[Generator]
internal class StaticMethodExtensionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider, StaticBuildDispatcher.GenerateSource);
    }
}
