# MetaSourceGenerator.AttributeFactory —— 元源代码生成器：从特性声明生成工厂

## 这是什么？

**MetaSourceGenerator.AttributeFactory** 是一个 Roslyn **元源代码生成器**（meta-source-generator）。  
它的作用是：**自动从特性类（Attribute class）的声明生成基于 `AttributeData` 的工厂方法**，供其他源代码生成器方便地在编译时从 `AttributeData` 创建特性实例。

简而言之：你写一个特性类，标记 `[FromAttributeData]`，它就会为你自动生成：
- 一个 `FullName` 常量（用于 `ForAttributeWithMetadataName`）
- 一个 `Create(AttributeData)` 静态工厂方法
- 对 `Type` 类型的属性/参数自动生成对应的 `ITypeSymbol` 属性

## 工作原理

1. **标记**：在特性类上添加 `[FromAttributeData]`
2. **检测**：`AutoFactoryClassGenerator`（`IIncrementalGenerator`）扫描所有类语法节点，查找标记了目标特性的类
3. **验证**：`BuildDispatcher.Valid()` 检查该类是特性类（继承自 `System.Attribute`）且非抽象
4. **生成**：`ClassBuilder` 生成 partial 类，包含：
   - `internal const string FullName = "..."` — 特性的完全限定名
   - 对每个 `Type` 类型的属性，生成 `ITypeSymbol XxxSymbol { get; set; }` 属性
   - `public static XxxAttribute Create(AttributeData data)` — 从 Roslyn 的 `AttributeData` 创建实例
5. **构造器匹配**：`ConstructorsBuilder` 为每个符合参数类型规则的构造器生成 `case` 分支，从 `data.ConstructorArguments` 提取参数值
6. **命名参数**：`PropertiesBuilder` 为每个可写属性生成 `case` 分支，从 `data.NamedArguments` 赋值

## `[FromAttributeData]` 特性详解

```csharp
[AttributeUsage(AttributeTargets.Class)]
public sealed class FromAttributeDataAttribute : Attribute
{
    /// <summary>
    /// 生成的常量字段名。默认 "FullName"。
    /// </summary>
    public string FullName { get; set; } = "FullName";

    /// <summary>
    /// 生成的工厂方法名。默认 "Create"。
    /// </summary>
    public string Create { get; set; } = "Create";
}
```

### 属性说明

| 属性 | 默认值 | 说明 |
|------|--------|------|
| `FullName` | `"FullName"` | 自定义生成的常量字段名（可避免与已有成员冲突） |
| `Create` | `"Create"` | 自定义生成的工厂方法名（可避免与已有方法冲突） |

## 支持的功能

### 1. 构造器参数映射

生成器会遍历特性类的所有非静态、非抽象、可访问（public/internal/protected internal）的构造器，为每个构造器的参数列表生成一个 `case` 分支：

```csharp
// 假设特性有构造器: MyAttribute(int a, string b)
// 生成:
case "global::System.Int32|global::System.String":
    value = new MyAttribute((int)data.ConstructorArguments[0].Value,
                            (string)data.ConstructorArguments[1].Value);
    break;
```

### 2. `Type` → `ITypeSymbol` 自动转换

当构造器参数或属性的类型为 `System.Type`（或 `Type[]`）时，生成器会自动改用 `ITypeSymbol`（或 `ITypeSymbol[]`）进行类型转换，因为 Roslyn 的 `AttributeData` 中 `Type` 类型参数实际以 `ITypeSymbol` 形式存储。

- 对于 **属性**：还会自动生成一个 `{Name}Symbol` 属性，类型为 `ITypeSymbol`（或 `ITypeSymbol[]`）
- 对于 **构造器**：需要一个对应的 `ITypeSymbol` 重载构造器，否则会报诊断 `ZMS003`

### 3. `FullName` 常量

生成的常量包含特性的完全限定名，可直接用于 `ForAttributeWithMetadataName`：

```csharp
// 生成:
internal const string FullName = "MyNamespace.MyAttribute";
// 使用:
context.SyntaxProvider.ForAttributeWithMetadataName(MyAttribute.FullName, ...)
```

### 4. 冲突避让

通过 `[FromAttributeData(FullName = "MyFullName", Create = "MyCreate")]` 可以自定义生成的常量名和方法名，避免与特性类中已有的成员冲突。

## 诊断 ID

| ID | 严重性 | 说明 |
|----|--------|------|
| `ZMS001` | Error | `[FromAttributeData]` 只能作用于继承自 `System.Attribute` 的特性类 |
| `ZMS002` | Error | `[FromAttributeData]` 不能作用于抽象类 |
| `ZMS003` | Info | 特性构造器包含 `Type`/`Type[]` 参数，但没有对应的 `ITypeSymbol`/`ITypeSymbol[]` 参数重载构造器。源代码生成器无法通过这些构造器创建实例 |

## 使用示例

```csharp
// 1. 定义一个特性类，标记 [FromAttributeData]
using zms9110750.MetaSourceGenerator.AttributeFactory;

[FromAttributeData]
public partial class MyAttribute : Attribute
{
    public Type? TargetType { get; }
    public string? Name { get; set; }
    public int Priority { get; set; }

    public MyAttribute(Type targetType)
    {
        TargetType = targetType;
    }

    // 如果希望生成器能通过构造器创建实例，
    // 需要提供一个 ITypeSymbol 重载：
    internal MyAttribute(ITypeSymbol targetTypeSymbol)
    {
        TargetTypeSymbol = targetTypeSymbol;
    }
}

// 2. 生成器自动生成的 partial 代码（示意）：
#if false // 自动生成
partial class MyAttribute
{
    internal const string FullName = "MyApp.MyAttribute";

    internal global::Microsoft.CodeAnalysis.ITypeSymbol TargetTypeSymbol { get; set; }

    public static global::MyApp.MyAttribute Create(global::Microsoft.CodeAnalysis.AttributeData data)
    {
        // 验证 data.AttributeClass 是否匹配
        // 根据构造器参数类型匹配 case
        // 从 data.ConstructorArguments / data.NamedArguments 提取值
        // 返回实例
    }
}
#endif

// 3. 在另一个源代码生成器中使用：
internal class MyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider
            .ForAttributeWithMetadataName(MyAttribute.FullName, ...)
            .Select((ctx, _) =>
            {
                var attr = MyAttribute.Create(ctx.Attributes[0]);
                // 使用 attr.TargetTypeSymbol, attr.Name, attr.Priority ...
                return attr;
            });
    }
}
```

## 项目结构

```
src/MetaSourceGenerator.AttributeFactory/
├── MetaSourceGenerator.AttributeFactory.csproj   # 项目文件 (netstandard2.0, Roslyn组件)
├── FromAttributeDataAttribute.cs                 # [FromAttributeData] 特性定义
├── Readme.md                                     # 原有英文文档
├── README.md                                     # 本文档
├── DiagnosticDefine/
│   ├── AnalyzerReleases.Shipped.md               # 已发布诊断列表
│   ├── AnalyzerReleases.Unshipped.md             # 未发布诊断列表
│   └── FromAttributeDiagnostic.cs                # 诊断定义 (ZMS001-ZMS003)
├── Gener/
│   └── AutoFactoryClassGenerator.cs              # 入口：IIncrementalGenerator
├── Builder/
│   ├── BaseBuilder.cs                            # 构建器基类
│   ├── BuildDispatcher.cs                        # 语法节点分发 + 验证
│   ├── ClassBuilder.cs                           # 主构建器（生成 partial 类框架）
│   ├── ConstructorsBuilder.cs                    # 构造器分支生成
│   ├── PropertiesBuilder.cs                      # 命名属性分支生成 + Symbol属性生成
│   └── Helper/
│       ├── DeferredActionScope.cs                # 延迟执行作用域（括号闭合）
│       ├── IndentedTextWriterHelpers.cs          # 代码写入帮助
│       └── ValidAttributeParameterType.cs        # 特性参数类型验证
```

## 与 InterfaceImplAsExtensionGenerator 的关系

本项目的 NuGet 包 `zms9110750.MetaSourceGenerator.AttributeFactory` 是 `InterfaceImplAsExtensionGenerator` 的**编译时依赖**（参考其 `.csproj`）：

```xml
<PackageReference Include="zms9110750.MetaSourceGenerator.AttributeFactory"
                  Version="0.1.1"
                  OutputItemType="Analyzer"
                  PrivateAssets="all" />
```

`InterfaceImplAsExtensionGenerator` 中的 `ExtensionForAttribute`、`ExtensionSourceAttribute`、`ExtensionGlobalConfigAttribute`、`ExtensionIncludeAttribute`、`ExtensionIgnoreAttribute` 都标记了 `[FromAttributeData]`，构建时会由本生成器自动生成它们的 `FullName` 常量和 `Create(AttributeData)` 工厂方法。

---

*该生成器由 [zms9110750](https://github.com/zms9110750) 开发，基于 MIT 许可证开源。*
