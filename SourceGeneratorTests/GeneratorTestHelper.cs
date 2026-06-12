using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceGeneratorTests;

internal static class GeneratorTestHelper
{
    /// <summary>Run a generator with basic references (handles InterfaceExtensionGenerator).</summary>
    public static GeneratorResult RunGenerator<TGenerator>(
        string sourceCode,
        LanguageVersion languageVersion = LanguageVersion.Preview,
        IEnumerable<MetadataReference>? extraReferences = null)
        where TGenerator : IIncrementalGenerator, new()
    {
        var parseOpts = new CSharpParseOptions(languageVersion);
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode, parseOpts);

        var references = new List<MetadataReference>();

        // Use runtime implementation assemblies for core types.
        references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location));
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
        if (runtimeDir != null)
        {
            foreach (var asm in new[] { "System.Runtime.dll", "System.Console.dll",
                                         "System.Collections.dll" })
            {
                var path = Path.Combine(runtimeDir, asm);
                if (File.Exists(path))
                    references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        // These assemblies are from the host runtime (not corefx), keep as-is
        references.Add(MetadataReference.CreateFromFile(typeof(System.ComponentModel.EditorBrowsableAttribute).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(System.Collections.Immutable.ImmutableArray).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(TGenerator).Assembly.Location));

        if (extraReferences != null)
            references.AddRange(extraReferences);

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new TGenerator();
        var driver = CSharpGeneratorDriver.Create(
            new[] { generator.AsSourceGenerator() },
            parseOptions: parseOpts);

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var result = new Dictionary<string, string>();
        foreach (var tree in outputCompilation.SyntaxTrees)
        {
            if (tree.FilePath != "" && tree.FilePath != syntaxTree.FilePath)
                result[tree.FilePath] = tree.GetText().ToString();
        }

        return new GeneratorResult(result, diagnostics);
    }

    /// <summary>Run a generator with netstandard refs (for generator targeting netstandard2.0).</summary>
    public static GeneratorResult RunGeneratorWithNetstandard<TGenerator>(
        string sourceCode,
        LanguageVersion languageVersion = LanguageVersion.Preview)
        where TGenerator : IIncrementalGenerator, new()
    {
        var extraRefs = new List<MetadataReference>();

        // Add netstandard — the generator was compiled against netstandard2.0,
        // and resolves types like System.Attribute through netstandard forwarding.
        var netstd = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget", "packages", "netstandard.library", "2.0.3",
            "build", "netstandard2.0", "ref", "netstandard.dll");
        if (File.Exists(netstd))
            extraRefs.Add(MetadataReference.CreateFromFile(netstd));

        // Also add System.Runtime from the same runtime dir for completeness
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
        if (runtimeDir != null)
        {
            var rt = Path.Combine(runtimeDir, "System.Runtime.dll");
            if (File.Exists(rt))
                extraRefs.Add(MetadataReference.CreateFromFile(rt));
        }

        return RunGenerator<TGenerator>(sourceCode, languageVersion, extraRefs);
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
