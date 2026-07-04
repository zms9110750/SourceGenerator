using zms9110750.MetaSourceGenerator.AttributeFactory.Gener;
using Xunit;

namespace SourceGeneratorTests;

public class AttributeFactoryTests
{
    [Fact]
    public void SimpleAttribute_GeneratesFactory()
    {
        var source = """
            using zms9110750.MetaSourceGenerator.AttributeFactory;

            [FromAttributeData]
            public class MyAttribute : Attribute
            {
                public string? Name { get; set; }
                public int Count { get; set; }

                public MyAttribute(string name)
                {
                    Name = name;
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator<AutoFactoryClassGenerator>(source);

        if (result.IsEmpty)
        {
            var allDiags = string.Join("\n---\n", result.Diagnostics.Select(d =>
                $"{d.Id} (sev={d.Severity}): {d.GetMessage()} at {d.Location.GetLineSpan()}"));
            Assert.Fail($"No generated output. All diagnostics:\n{allDiags}");
        }

        var output = result.AllOutput;

        Assert.Contains("FullName", output);
        Assert.Contains("Create", output);
        Assert.Contains("Name", output);
        Assert.Contains("Count", output);
    }

    [Fact]
    public void TypeParameter_GeneratesSymbolProperty()
    {
        var source = """
            using System;
            using zms9110750.MetaSourceGenerator.AttributeFactory;

            [FromAttributeData]
            public class TypeAttribute : Attribute
            {
                public Type? TargetType { get; set; }
                public Type[]? MultipleTypes { get; set; }

                public TypeAttribute(Type targetType)
                {
                    TargetType = targetType;
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator<AutoFactoryClassGenerator>(source);

        if (result.IsEmpty)
        {
            var allDiags = string.Join("\n---\n", result.Diagnostics.Select(d =>
                $"{d.Id} (sev={d.Severity}): {d.GetMessage()} at {d.Location.GetLineSpan()}"));
            Assert.Fail($"No generated output. All diagnostics:\n{allDiags}");
        }

        var output = result.AllOutput;

        Assert.Contains("TargetTypeSymbol", output);
        Assert.Contains("MultipleTypesSymbol", output);
        Assert.Contains("ITypeSymbol", output);
    }

    [Fact]
    public void AbstractAttribute_DoesNotGenerate()
    {
        var source = """
            using zms9110750.MetaSourceGenerator.AttributeFactory;

            [FromAttributeData]
            public abstract class AbstractAttribute : Attribute
            {
                public string? Name { get; set; }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator<AutoFactoryClassGenerator>(source);

        Assert.True(result.IsEmpty, "Abstract attribute should not generate any files");
    }

    [Fact]
    public void NonAttributeType_DoesNotGenerate()
    {
        var source = """
            using zms9110750.MetaSourceGenerator.AttributeFactory;

            [FromAttributeData]
            public class NotAnAttribute
            {
                public string? Name { get; set; }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator<AutoFactoryClassGenerator>(source);

        Assert.True(result.IsEmpty, "Non-Attribute type should not generate any files");
    }
}
