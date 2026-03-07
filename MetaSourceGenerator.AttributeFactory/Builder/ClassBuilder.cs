using System.CodeDom.Compiler;
using System.Reflection;
using System.Reflection.Metadata;
using zms9110750.MetaSourceGenerator.AttributeFactory.Builder.Helper;

namespace zms9110750.MetaSourceGenerator.AttributeFactory.Builder;

class ClassBuilder(INamedTypeSymbol attributeSymbol, IndentedTextWriter writer, Action<Diagnostic> reportDiagnostic) : BaseBuilder(writer, reportDiagnostic)
{
    public INamedTypeSymbol AttributeSymbol { get; } = attributeSymbol ?? throw new ArgumentNullException(nameof(attributeSymbol));
    public string AttributeFullName => AttributeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    public void GenerateSource()
    {
        using DeferredActionScope deferredActionScope = new DeferredActionScope();
        if (!AttributeSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            Writer.WriteLine($"namespace {AttributeSymbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))}");
            Writer.AppendOpenBracket(deferredActionScope);
        }
        Writer.WriteLine($"partial class {AttributeSymbol.Name}");
        Writer.AppendOpenBracket(deferredActionScope);
        PropertiesBuilder propertiesBuilder = new(this);
        Writer.WriteLine($"internal const string FullName = \"{AttributeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))}\";");
        Writer.WriteLine();

        propertiesBuilder.GenerateSourceProperties();


        ConstructorsBuilder constructorsBuild = new(this);
        Writer.WriteLine($"public static {AttributeFullName} Create(global::Microsoft.CodeAnalysis.AttributeData data)");
        Writer.AppendOpenBracket(deferredActionScope);
        Writer.WriteSources($$"""
        if (data == null)
        {
            return null;
        }
        var format = global::Microsoft.CodeAnalysis.SymbolDisplayFormat.FullyQualifiedFormat;
        if (data.AttributeClass.ToDisplayString(format) != "{{AttributeFullName}}")
        {
            return null;
        }

        {{AttributeFullName}} value = null;
        switch (string.Join("|", global::System.Linq.Enumerable.Select(data.AttributeConstructor.Parameters, p => p.Type.ToDisplayString(format))))
        {
        """);
        constructorsBuild.GenerateSource();

        Writer.WriteLine("default:");
        Writer.WriteLine(1, "return null;");
        Writer.WriteLine("}");
        Writer.WriteLineNoTabs("#pragma warning disable CS1522");
        Writer.WriteSources($$""" 
        foreach (var symbol in data.NamedArguments)
        {
            switch (symbol.Key)
            {
        """);
        propertiesBuilder.GenerateSource();
        Writer.WriteSources($$"""
            }
        }
        return value;
        """);
    }
}
