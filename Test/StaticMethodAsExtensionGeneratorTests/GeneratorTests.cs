using Xunit;
using zms9110750.StaticMethodAsExtensionGenerator.Builder;
using zms9110750.StaticMethodAsExtensionGenerator.Gener;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.CodeDom.Compiler;
using System.Text;

namespace StaticMethodAsExtensionGeneratorTests;

public class StaticMethodExtensionGeneratorTests
{
    [Fact]
    public void Generator_DoesNotProduceErrorDiagnostics()
    {
        var source = """
            using zms9110750.StaticMethodAsExtensionGenerator;
            [assembly: StaticMethodExtensions(StaticMethodExtensionScope.System)]
            """;

        var result = GeneratorTestHelper.RunGenerator<StaticMethodExtensionGenerator>(source);

        Assert.False(result.HasErrors,
            "Generator produced error diagnostics:\n"
            + string.Join("\n", result.Diagnostics));
    }

    [Fact]
    public void NamespaceBuilder_GeneratesValidSyntax()
    {
        var writer = new StringWriter();
        var indented = new IndentedTextWriter(writer);
        var builder = new StaticNamespaceBuilder(indented);

        // Create a simple test type info
        var source = """
            namespace Test { public class MyClass {
                public static void Hello(MyClass self, int value) { }
                public static int Add(MyClass self, int a, int b) => a + b;
            } }
            """;

        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview));
        var compilation = CSharpCompilation.Create("test",
            new[] { tree },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });

        var type = compilation.GetTypeByMetadataName("Test.MyClass")!;

        var methods = type.GetMembers().OfType<IMethodSymbol>()
            .Where(m => m.IsStatic && m.DeclaredAccessibility == Accessibility.Public)
            .ToArray();

        var info = new TypeExtensionInfo(type, methods);
        builder.GenerateAll("Test", [info]);

        var output = writer.ToString();
        Assert.Contains("internal static class MyClassExtensions", output);
        Assert.Contains("this ", output);
    }

    [Fact]
    public void NamespaceBuilder_GeneratesMethodBody()
    {
        var writer = new StringWriter();
        var indented = new IndentedTextWriter(writer);
        var builder = new StaticNamespaceBuilder(indented);

        var source = """
            namespace Test { public class MyClass {
                public static void Hello(MyClass self, int value) { }
            } }
            """;

        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview));
        var compilation = CSharpCompilation.Create("test",
            new[] { tree },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });

        var type = compilation.GetTypeByMetadataName("Test.MyClass")!;
        var methods = type.GetMembers().OfType<IMethodSymbol>()
            .Where(m => m.IsStatic && m.DeclaredAccessibility == Accessibility.Public)
            .ToArray();

        var info = new TypeExtensionInfo(type, methods);
        builder.GenerateAll("Test", [info]);

        var output = writer.ToString();
        Assert.Contains("Test.MyClass.Hello(self, value)", output);
    }
}
