using System.CodeDom.Compiler;
using System.Reflection;
using System.Reflection.Metadata;
using zms9110750.InterfaceImplAsExtensionGenerator.Builder.Helper;
using zms9110750.InterfaceImplAsExtensionGenerator.DiagnosticDefine;

namespace zms9110750.InterfaceImplAsExtensionGenerator.Builder;

class ClassBuilder(SourceProductionContext context, INamedTypeSymbol classSymbol, LanguageVersion languageVersion = default)
{
    (AttributeData, ExtensionForAttribute)[] ExtensionForAttributes { get; } = classSymbol.GetAttributes()
          .Select(attr => (attr, ExtensionForAttribute.Create(attr)))
          .Where(attr => attr.Item2 != null)
          .ToArray();

    public void GenerateSource()
    {
        foreach (var (data, attr) in ExtensionForAttributes)
        {
            try
            {
                if (attr.AppendTypeSymbol?.TypeKind != TypeKind.Interface)
                {
                    if (data.ApplicationSyntaxReference?.GetSyntax()?.GetLocation() is { } location)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(ExtensionDiagnostic.ZMS007, location)); 
                    }
                    continue;
                }

                using var buffer = new StringWriter(new StringBuilder());
                using var writer = new IndentedTextWriter(buffer);

                var fileName = classSymbol!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))
                    + "+" + attr.AppendTypeSymbol!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

                fileName = fileName.Replace("<", "{")
                                    .Replace(">", "}")
                                    + ".g.cs";

                InterfaceBuilder interfaceBuilder = new InterfaceBuilder(classSymbol, attr, writer, context.ReportDiagnostic, languageVersion);
                interfaceBuilder.GenerateSource();
                context.AddSource(fileName, buffer.ToString());
            }
            catch (Exception ex)
            {
                context.AddSource($"Error_{classSymbol?.Name ?? "Unknown"}+{attr.AppendTypeSymbol?.Name ?? "Unknown"}", "/*" + ex.ToString() + "*/");
            } 
        }
    }
}
