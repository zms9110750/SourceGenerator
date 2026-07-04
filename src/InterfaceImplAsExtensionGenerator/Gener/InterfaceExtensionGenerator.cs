using System;
using System.Collections.Generic;
using System.Text;
using zms9110750.InterfaceImplAsExtensionGenerator.Builder;

namespace zms9110750.InterfaceImplAsExtensionGenerator.Gener;

[Generator]
class InterfaceExtensionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 获取语言版本
        var langVersionProvider = context.CompilationProvider
            .Select((comp, token) =>
            {
                return comp is CSharpCompilation csharpComp ? csharpComp.LanguageVersion : LanguageVersion.Default;
            });
        try
        {
            var classProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(ClassBuildDispatcher.TargetAttributeFullName, ClassBuildDispatcher.FilterTarget, ClassBuildDispatcher.Create)
                .Combine(langVersionProvider);
            context.RegisterSourceOutput(classProvider, ClassBuildDispatcher.GenerateSource);
        }
        catch (Exception ex)
        {
            context.RegisterSourceOutput(context.CompilationProvider, (ctx, compilation) =>
            {
                ctx.AddSource($"Error_Class_{Guid.NewGuid():N}.g.cs", $"/* Error: {ex} */");
            });
        }

        try
        {
            var interfaceProvider = context.SyntaxProvider
              .ForAttributeWithMetadataName(InterfaceBuildDispatcher.TargetAttributeFullName, InterfaceBuildDispatcher.FilterTarget, InterfaceBuildDispatcher.Create)
              .Combine(langVersionProvider);

            context.RegisterSourceOutput(interfaceProvider, InterfaceBuildDispatcher.GenerateSource);
        }
        catch (Exception ex)
        {
            context.RegisterSourceOutput(context.CompilationProvider, (ctx, compilation) =>
            {
                ctx.AddSource($"Error_Interface_{Guid.NewGuid():N}.g.cs", $"/* Error: {ex} */");
            });
        } 
    }
}

