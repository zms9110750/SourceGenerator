using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace StaticMethodAsExtensionGeneratorTests;

internal static class GeneratorTestHelper
{
    public static GeneratorResult RunGenerator<TGenerator>(
        string sourceCode,
        LanguageVersion languageVersion = LanguageVersion.Preview,
        IEnumerable<MetadataReference>? extraReferences = null)
        where TGenerator : IIncrementalGenerator, new()
    {
        var parseOpts = new CSharpParseOptions(languageVersion);
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode, parseOpts);

        var refs = new List<MetadataReference>();

        // Load only the key reference assemblies the generator needs.
        // Load ALL from the ref directory to match a real build environment.
        var refDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget", "packages", "microsoft.netcore.app.ref", "10.0.8",
            "ref", "net10.0");

        if (Directory.Exists(refDir))
        {
            foreach (var dll in Directory.GetFiles(refDir, "*.dll"))
            {
                try { refs.Add(MetadataReference.CreateFromFile(dll)); }
                catch { }
            }
        }
        else
        {
            // Fallback
            refs.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            refs.Add(MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location));
            var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
            if (runtimeDir != null)
            {
                foreach (var asm in new[] { "System.Runtime.dll", "System.Console.dll",
                                             "System.Collections.dll", "System.Linq.dll" })
                {
                    var p = Path.Combine(runtimeDir, asm);
                    if (File.Exists(p)) refs.Add(MetadataReference.CreateFromFile(p));
                }
            }
        }

        refs.Add(MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location));
        refs.Add(MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location));
        refs.Add(MetadataReference.CreateFromFile(
            typeof(System.ComponentModel.EditorBrowsableAttribute).Assembly.Location));

        // Include generator dependency assemblies
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.StartsWith("zms9110750.") == true))
        {
            try { refs.Add(MetadataReference.CreateFromFile(asm.Location)); }
            catch { }
        }

        if (extraReferences != null)
            refs.AddRange(extraReferences);

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new TGenerator();
        var driver = CSharpGeneratorDriver.Create(
            new[] { generator.AsSourceGenerator() },
            parseOptions: parseOpts);

        driver.RunGeneratorsAndUpdateCompilation(
            compilation, out var outputCompilation, out var diagnostics);

        var result = new Dictionary<string, string>();
        foreach (var tree in outputCompilation.SyntaxTrees)
        {
            if (tree.FilePath != "" && tree.FilePath != syntaxTree.FilePath)
                result[tree.FilePath] = tree.GetText().ToString();
        }
        return new GeneratorResult(result, diagnostics);
    }
}

internal readonly record struct GeneratorResult(
    IReadOnlyDictionary<string, string> GeneratedSources,
    ImmutableArray<Diagnostic> Diagnostics)
{
    public string AllOutput => string.Join("\n", GeneratedSources.Values);
    public bool IsEmpty => GeneratedSources.Count == 0;
    public bool HasErrors => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
}
