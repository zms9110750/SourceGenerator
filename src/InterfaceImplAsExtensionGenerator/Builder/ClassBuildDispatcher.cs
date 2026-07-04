using zms9110750.InterfaceImplAsExtensionGenerator.DiagnosticDefine;

namespace zms9110750.InterfaceImplAsExtensionGenerator.Builder;

internal class ClassBuildDispatcher(GeneratorAttributeSyntaxContext ctx)
{
    public const string TargetAttributeFullName = ExtensionForAttribute.FullName;
    protected INamedTypeSymbol? ClassSymbol { get; } = ctx.TargetSymbol as INamedTypeSymbol;

    public static bool FilterTarget(SyntaxNode node, CancellationToken token)
    {
        return node is ClassDeclarationSyntax;
    }

    public static ClassBuildDispatcher Create(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        return new ClassBuildDispatcher(context);
    }
    public bool Valid(SourceProductionContext context)
    {
        if (ClassSymbol is not { TypeArguments.IsEmpty: true, ContainingType: null, IsStatic: true })
        {
            var location = (ClassSymbol?.GetAttributes())?.FirstOrDefault(t => ExtensionForAttribute.Create(t) != null)
                ?.ApplicationSyntaxReference?.GetSyntax()?.GetLocation();

            if (location != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(ExtensionDiagnostic.ZMS006, location));

            }
            return false;
        }


        return true;
    }

    public void GenerateSource(SourceProductionContext context, LanguageVersion languageVersion = default)
    {
        if (ClassSymbol == null)
        {
            return;
        }
        if (!Valid(context))
        {
            return;
        }

        ClassBuilder classBuilder = new ClassBuilder(context, ClassSymbol, languageVersion);
        classBuilder.GenerateSource();
    }

    public static void GenerateSource(SourceProductionContext context, ClassBuildDispatcher generator)
    {
        try
        {
            generator.GenerateSource(context);
        }
        catch (Exception ex)
        {
            context.AddSource($"Error_{generator.ClassSymbol?.Name ?? "Unknown"}", "/*" + ex.ToString() + "*/");
        }
    }
    public static void GenerateSource(SourceProductionContext context, (ClassBuildDispatcher Left, LanguageVersion Right) generator)
    {
        try
        {
            generator.Left.GenerateSource(context, generator.Right);
        }
        catch (Exception ex)
        {
            context.AddSource($"Error_{generator.Left.ClassSymbol?.Name ?? "Unknown"}", "/*" + ex.ToString() + "*/");
        }
    }
}
