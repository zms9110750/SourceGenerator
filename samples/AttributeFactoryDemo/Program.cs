using System;
using zms9110750.MetaSourceGenerator.AttributeFactory;

namespace AttributeFactoryDemo;

/// <summary>标记需要日志记录的方法/属性。</summary>
[FromAttributeData]
public partial class LogMethodAttribute : Attribute
{
    /// <summary>日志级别：0=Info, 1=Warning, 2=Error</summary>
    public int Level { get; set; }

    /// <summary>可选的日志分类名</summary>
    public string? Category { get; set; }

    /// <summary>构造函数参数</summary>
    public LogMethodAttribute(int level, string? category = null)
    {
        Level = level;
        Category = category;
    }
}

/// <summary>标记某个类型需要注册到 DI 容器。</summary>
[FromAttributeData]
public partial class RegisterServiceAttribute : Attribute
{
    /// <summary>服务生命周期</summary>
    public string Lifetime { get; set; }

    /// <summary>服务类型（源生成器会为其生成 ITypeSymbol 属性）</summary>
    public Type? ServiceType { get; set; }

    public RegisterServiceAttribute(string lifetime)
    {
        Lifetime = lifetime;
    }
}

public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== [FromAttributeData] 特性工厂演示 ===");
        Console.WriteLine();

        // [FromAttributeData] 生成器为每个 partial class Attribute 生成:
        //   • internal const string FullName — 可直接用于 ForAttributeWithMetadataName
        //   • static Create(AttributeData) — 从 Roslyn AttributeData 创建实例
        //   • 对于 Type 字段，生成 XxxSymbol (ITypeSymbol) 属性
        //
        // 以下展示运行时反射读取特性的效果（生产环境应在源生成器中使用生成的工厂方法）

        var method = typeof(Program).GetMethod("DemoMethod",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;

        Console.WriteLine($"  方法: {method.Name}");
        foreach (var attr in method.GetCustomAttributesData())
        {
            Console.WriteLine($"    特性: {attr.AttributeType.Name}");
            foreach (var arg in attr.ConstructorArguments)
                Console.WriteLine($"      构造参数: {arg.Value} ({arg.ArgumentType.Name})");
            foreach (var named in attr.NamedArguments)
                Console.WriteLine($"      命名参数: {named.MemberName} = {named.TypedValue.Value}");
        }

        Console.WriteLine();
        Console.WriteLine("  关键优势: 相比运行时反射，生成器产出的 Create(AttributeData)");
        Console.WriteLine("  可在编译时直接读取特性参数，用于源生成器上下文。");
        Console.WriteLine("  FullName 常量免去硬编码字符串，配合 Roslyn API 类型安全查询。");

        Console.WriteLine();
        Console.WriteLine("=== 演示完成 ===");
    }

    [LogMethod(1, Category = "Startup")]
    [RegisterService("Singleton", ServiceType = typeof(IDisposable))]
    private static void DemoMethod() { }
}
