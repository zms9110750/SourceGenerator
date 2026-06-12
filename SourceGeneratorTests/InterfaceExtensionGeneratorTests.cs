using Microsoft.CodeAnalysis.CSharp;
using zms9110750.InterfaceImplAsExtensionGenerator.Gener;
using Xunit;

namespace SourceGeneratorTests;

public class InterfaceExtensionGeneratorTests
{
    [Fact]
    public void ExtensionMethodSyntax()
    {
        var source = """
            using zms9110750.InterfaceImplAsExtensionGenerator;

            [ExtensionSource]
            public interface IMyInterface
            {
                int Value { get; set; }
                void DoSomething();
            }
            """;

        var result = GeneratorTestHelper.RunGenerator<InterfaceExtensionGenerator>(source);
        Assert.False(result.IsEmpty, "Expected at least one generated file");

        var output = result.AllOutput;

        // With Latest/Preview language version, should generate something
        Assert.Contains("IMyInterface", output);
        Assert.Contains("DoSomething", output);
    }

    [Fact]
    public void RefReturn()
    {
        var source = """
            using zms9110750.InterfaceImplAsExtensionGenerator;

            [ExtensionSource]
            public interface IRefReturn
            {
                ref int GetRef();
                ref readonly int GetRefReadonly();
            }
            """;

        var result = GeneratorTestHelper.RunGenerator<InterfaceExtensionGenerator>(source);
        var output = result.AllOutput;

        Assert.Contains("ref ", output);
        Assert.Contains("ref readonly", output);
        Assert.Contains("GetRef", output);
        Assert.Contains("GetRefReadonly", output);
    }

    [Fact]
    public void GenericConstraints()
    {
        var source = """
            using zms9110750.InterfaceImplAsExtensionGenerator;

            [ExtensionSource]
            public interface IGeneric<T1, T2>
                where T1 : class, new()
                where T2 : struct
            {
                T1 Create(T2 input);
                void Process<T3>(T3 item) where T3 : class;
            }
            """;

        var result = GeneratorTestHelper.RunGenerator<InterfaceExtensionGenerator>(source);
        var output = result.AllOutput;

        Assert.Contains("where", output);
        Assert.Contains("class", output);
        Assert.Contains("new()", output);
    }

    [Fact]
    public void KeywordParamsAndModifiers()
    {
        var source = """
            using zms9110750.InterfaceImplAsExtensionGenerator;

            [ExtensionSource]
            public interface IEdgeCases
            {
                void Process(ref int @class, out string @event, in double @object, int value = 42);
            }
            """;

        var result = GeneratorTestHelper.RunGenerator<InterfaceExtensionGenerator>(source);
        var output = result.AllOutput;

        Assert.Contains("@class", output);
        Assert.Contains("@event", output);
        Assert.Contains("@object", output);
        Assert.Contains("ref ", output);
        Assert.Contains("out ", output);
        Assert.Contains("in ", output);
        Assert.Contains("42", output);
    }

    [Fact]
    public void ExtensionIgnore()
    {
        var source = """
            using zms9110750.InterfaceImplAsExtensionGenerator;

            [ExtensionSource]
            public interface IWithIgnore
            {
                void ShouldGenerate();
                [ExtensionIgnore]
                void ShouldNotGenerate();
            }
            """;

        var result = GeneratorTestHelper.RunGenerator<InterfaceExtensionGenerator>(source);
        var output = result.AllOutput;

        Assert.Contains("ShouldGenerate", output);
        Assert.DoesNotContain("ShouldNotGenerate", output);
    }

    [Fact]
    public void ExtensionForAttribute()
    {
        var source = """
            using zms9110750.InterfaceImplAsExtensionGenerator;

            public interface IExternalInterface
            {
                void Hello();
                string Name { get; }
            }

            [ExtensionFor(typeof(IExternalInterface))]
            public static partial class MyExtensions
            {
            }
            """;

        var result = GeneratorTestHelper.RunGenerator<InterfaceExtensionGenerator>(source);

        Assert.False(result.IsEmpty,
            "ExtensionFor should generate output. Diagnostics:\n"
            + string.Join("\n", result.Diagnostics));
    }

    [Fact]
    public void ParamsParameter()
    {
        var source = """
            using zms9110750.InterfaceImplAsExtensionGenerator;

            [ExtensionSource]
            public interface IWithParams
            {
                int Sum(params int[] numbers);
            }
            """;

        var result = GeneratorTestHelper.RunGenerator<InterfaceExtensionGenerator>(source);
        var output = result.AllOutput;

        Assert.Contains("params", output);
    }

    [Fact]
    public void EventsNotGeneratedByDefault()
    {
        var source = """
            using zms9110750.InterfaceImplAsExtensionGenerator;

            [ExtensionSource]
            public interface IWithEvent
            {
                event EventHandler MyEvent;
            }
            """;

        var result = GeneratorTestHelper.RunGenerator<InterfaceExtensionGenerator>(source);
        var output = result.AllOutput;

        Assert.DoesNotContain("MyEvent", output);
    }
}
