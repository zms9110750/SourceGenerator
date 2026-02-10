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

                try
                {
                    var converter = new ClassBuilder(symbol!, syntax!);
                    if (converter.FileName != null)
                    {
                        spc.AddSource(converter.FileName, converter.Generate().NormalizeWhitespace().ToFullString());
                    }
                    foreach (var item in converter)
                    {
                        spc.ReportDiagnostic(item);
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