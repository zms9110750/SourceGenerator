using Microsoft.CodeAnalysis.Text;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;
using zms9110750.MetaSourceGenerator.AttributeFactory.DiagnosticDefine;

namespace zms9110750.MetaSourceGenerator.AttributeFactory.Builder;

internal class BuildDispatcher(GeneratorAttributeSyntaxContext ctx)
{
    public const string TargetAttributeFullName = "zms9110750.MetaSourceGenerator.AttributeFactory.FromAttributeDataAttribute";
    protected INamedTypeSymbol? AttributeSymbol { get; } = ctx.TargetSymbol as INamedTypeSymbol;

    public static bool FilterTarget(SyntaxNode node, CancellationToken token)
    {
        return node is ClassDeclarationSyntax;
    }

    public static BuildDispatcher Create(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        return new BuildDispatcher(context);
    }
    public bool Valid(SourceProductionContext context)
    {
        var att = AttributeSymbol!.GetAttributes().First(item
               => item.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ==
                   "global::zms9110750.MetaSourceGenerator.AttributeFactory.FromAttributeDataAttribute");

        Location location = att.ApplicationSyntaxReference!.GetSyntax().GetLocation();

        bool isAttributeDerived = false;
        for (INamedTypeSymbol? currentType = AttributeSymbol;
             currentType != null;
             currentType = currentType.BaseType)
        {
            if (currentType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ==
                "global::System.Attribute")
            {
                isAttributeDerived = true;
                break;
            }
        }

        if (!isAttributeDerived)
        {
            context.ReportDiagnostic(Diagnostic.Create(FromAttributeDiagnostic.ZMS001, location));
            return false;
        }

        if (AttributeSymbol.IsAbstract)
        {
            context.ReportDiagnostic(Diagnostic.Create(FromAttributeDiagnostic.ZMS002, location));
            return false;

        }
        return true;
    }

    public void GenerateSource(SourceProductionContext context)
    {
        if (AttributeSymbol == null)
        {
            return;
        }
        if (!Valid(context))
        {
            return;
        }

        using var buffer = new StringWriter(new StringBuilder());
        using var writer = new IndentedTextWriter(buffer);

        var fileName = AttributeSymbol.Name + "+" + AttributeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))
                            .Replace("<", "{")
                            .Replace(">", "}")
                            + ".g.cs";
        ClassBuilder classBuilder = new ClassBuilder(AttributeSymbol, writer, context.ReportDiagnostic);
        classBuilder.GenerateSource();
        context.AddSource(fileName, buffer.ToString());
    }

    public static void GenerateSource(SourceProductionContext context, BuildDispatcher generator)
    {
        try
        {
            generator.GenerateSource(context);
        }
        catch (Exception ex)
        {
            context.AddSource($"Error_{generator.AttributeSymbol?.Name ?? "Unknown"}", "/*" + ex.ToString() + "*/");
        }
    }
}
