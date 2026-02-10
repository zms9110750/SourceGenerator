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

                try
                {
                    var converter = new MethodBuilder(symbol!, syntax!);
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
                    spc.AddSource($"Error_{symbol!.ContainingType.Name}_{symbol.Name}_{Guid.NewGuid():N}", ex.ToString());
                }
            });
        }
    }
}