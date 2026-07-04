namespace zms9110750.MetaSourceGenerator.AttributeFactory.Gener
{
    [Generator]
    class AutoFactoryClassGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classProvider = context.SyntaxProvider
           .ForAttributeWithMetadataName(BuildDispatcher.TargetAttributeFullName, BuildDispatcher.FilterTarget, BuildDispatcher.Create);

            context.RegisterSourceOutput(classProvider, BuildDispatcher.GenerateSource);
        }
    } 
}