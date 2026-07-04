using System.CodeDom.Compiler;

namespace zms9110750.InterfaceImplAsExtensionGenerator.Builder;

internal class InterfaceBuildDispatcher(GeneratorAttributeSyntaxContext ctx)
{
    protected INamedTypeSymbol? InterfaceSymbol { get; } = ctx.TargetSymbol as INamedTypeSymbol;
    public const string TargetAttributeFullName = ExtensionSourceAttribute.FullName;

    public static bool FilterTarget(SyntaxNode node, CancellationToken token)
    {
        return node is InterfaceDeclarationSyntax;
    }

    public static InterfaceBuildDispatcher Create(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        return new InterfaceBuildDispatcher(context);
    }
    public bool Valid(SourceProductionContext context)
    {

        return true;
    }

    public void GenerateSource(SourceProductionContext context, LanguageVersion version = default)
    {
        if (InterfaceSymbol == null)
        {
            return;
        }
        if (!Valid(context))
        {
            return;
        }

        using var buffer = new StringWriter(new StringBuilder());
        using var writer = new IndentedTextWriter(buffer);

        var fileName = InterfaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))
                            .Replace("<", "{")
                            .Replace(">", "}")
                            + ".g.cs";
        InterfaceBuilder classBuilder = new InterfaceBuilder(InterfaceSymbol, writer, context.ReportDiagnostic, version);
        classBuilder.GenerateSource();
        context.AddSource(fileName, buffer.ToString());
    }

    public static void GenerateSource(SourceProductionContext context, InterfaceBuildDispatcher generator)
    {
        try
        {
            generator.GenerateSource(context);
        }
        catch (Exception ex)
        {
            context.AddSource($"Error_{generator.InterfaceSymbol?.Name ?? "Unknown"}", "/*" + ex.ToString() + "*/");
        }
    }
    public static void GenerateSource(SourceProductionContext context, (InterfaceBuildDispatcher Left, LanguageVersion Right) generator)
    {
        try
        {
            generator.Left.GenerateSource(context, generator.Right);
        }
        catch (Exception ex)
        {
            context.AddSource($"Error_{generator.Left.InterfaceSymbol?.Name ?? "Unknown"}", "/*" + ex.ToString() + "*/");
        }
    }
}
