# InterfaceImplAsExtensionGenerator —— 接口成员生成扩展方法

## 这是什么？

**InterfaceImplAsExtensionGenerator** 是一个 Roslyn 源代码生成器（Incremental Generator），用于为接口中的成员自动生成**扩展方法**（或 C# 14 的**扩展块**语法）。  
它解决的核心问题是：当接口包含**默认实现**，或实现类包含**显式实现**（explicit implementation）且不存在对应的类成员时，你可以通过自动生成的扩展方法直接调用接口成员，而无需手动编写转发代码。  
生成的扩展方法优先级低于类本身的成员，不会造成冲突。

## 工作原理

1. **标记阶段**：通过在接口上使用 `[ExtensionSource]`，或在静态类上使用 `[ExtensionFor]`，声明哪些接口需要生成扩展。
2. **收集阶段**：Roslyn 增量生成器在编译过程中扫描带有上述特性的语法节点，提取接口的符号信息。
3. **构建阶段**：`InterfaceBuildDispatcher` / `ClassBuildDispatcher` 根据提取的信息创建 `InterfaceBuilder` / `ClassBuilder`。
4. **生成阶段**：`InterfaceBuilder` 遍历接口的所有成员（方法、属性、索引器、事件），根据配置生成对应的扩展方法/扩展块源代码。
5. **输出阶段**：生成的 `.g.cs` 文件被加入到编译中。

### 两种生成语法

| 模式 | 说明 |
|------|------|
| `ExtensionMethod` | 传统 `this T instance` 扩展方法语法 |
| `ExtensionBlock` (C# 14+) | 新的 `extension(T instance)` 块语法 |
| `Auto` | 根据编译器语言版本自动选择（>= C# 14 用块，否则用方法） |
| `Both` | 同时生成两种，通过 `#if NET10_0_OR_GREATER` 条件编译切换 |

## 配置方式 —— `ExtensionForAttribute`

`[ExtensionFor(typeof(IInterface))]` 可多次作用于**顶级非泛型静态类**，将指定接口的成员以扩展方法形式追加到该类中。

```csharp
[ExtensionFor(typeof(IList<int>))]
[ExtensionFor(typeof(IList<>))]
public static partial class MyExtensions
{
}
```

### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `AppendType` | `Type?` | 要追加成员的接口类型（必填，构造函数传入） |
| `InstanceParameterName` | `string?` | 扩展方法中的实例参数名 |
| `DefaultGenerateMembers` | `GenerateMembers` | 默认为该接口生成的成员类型（按位枚举） |

## 配置方式 —— `ExtensionSourceAttribute`

`[ExtensionSource]` 直接作用于**接口**，为该接口本身的成员生成扩展。

```csharp
[ExtensionSource]
public interface IMyInterface
{
    void DoWork();
}
```

### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `ExtensionClassName` | `string?` | 生成的扩展类名（默认：接口名 + "Extension"） |
| `ExtensionClassNamespace` | `string?` | 生成的扩展类所在命名空间 |
| `InstanceParameterName` | `string?` | 实例参数名 |
| `DefaultGenerateMembers` | `GenerateMembers` | 默认生成的成员类型 |
| `UsePublic` | `PublicModifier` | 是否添加 `public` 修饰符 |

## 全局配置 —— `ExtensionGlobalConfigAttribute`

作用于**程序集**，设置全局默认行为：

```csharp
[assembly: ExtensionGlobalConfig(
    NamespacePrefix = "MyApp.Extensions",
    TypeNameSuffix = "Extensions",
    InstanceParameterName = "self",
    DefaultGenerateMembers = GenerateMembers.Method | GenerateMembers.Property,
    UseSyntax = ExtensionSyntaxVersion.Auto,
    UsePublic = PublicModifier.FollowInterface
)]
```

| 属性 | 默认值 | 说明 |
|------|--------|------|
| `NamespacePrefix` | `"zms9110750.Extensions.Generator"` | 所有生成扩展类的命名空间前缀 |
| `TypeNameSuffix` | `"Extension"` | 扩展类名后缀 |
| `NamespaceSuffix` | `null` | 命名空间后缀（追加在原命名空间后） |
| `InstanceParameterName` | `"instance"` | 实例参数默认名称 |
| `DefaultGenerateMembers` | `Property \| Method` | 默认生成的成员类型 |
| `UseSyntax` | `Auto` | 扩展语法版本 |
| `UsePublic` | `FollowInterface` | 是否添加 `public` |

## 成员级配置

| 特性 | 作用 | 说明 |
|------|------|------|
| `[ExtensionInclude]` | 方法/属性/事件 | **强制包含**该成员，即使不在 `DefaultGenerateMembers` 中；可设置 `ReplacementMemberName` 和 `InstanceParameterName` |
| `[ExtensionIgnore]` | 方法/属性/事件 | **强制排除**该成员 |

> **注意**：同时使用 `Include` 和 `Ignore` 时效果未定义。

## 过滤器规则

生成器仅对符合以下条件的成员生成扩展：

1. **非静态**成员（`IsStatic == false`）
2. **非隐式声明**成员（`IsImplicitlyDeclared == false`）
3. **可访问性**为 `public`、`internal` 或 `protected internal`
4. 未被 `[ExtensionIgnore]` 排除
5. 被 `[ExtensionInclude]` 包含 **或** 其成员类型在 `DefaultGenerateMembers` 中启用

### `GenerateMembers` 按位枚举

| 值 | 说明 |
|------|------|
| `Property` | 属性 |
| `Indexer` | 索引器 |
| `Event` | 事件 |
| `Method` | 方法 |
| `All` | 全部 |

### `PublicModifier` 枚举

| 值 | 说明 |
|------|------|
| `NoModifier` | 始终不加 `public` |
| `FollowInterface` | 跟随接口的访问级别 |
| `Always` | 总是加 `public` |

## 配置优先级（从低到高）

```
程序集级 (ExtensionGlobalConfig)
  → 接口级 (ExtensionSourceAttribute)
    → 类级 (ExtensionForAttribute)
      → 成员级 (ExtensionIncludeAttribute)
```

未设置的属性会继承上一级的值。

## 使用示例

```csharp
// 1. 全局配置（可选）
[assembly: ExtensionGlobalConfig(
    NamespacePrefix = "MyApp.Extensions",
    TypeNameSuffix = "Extensions"
)]

// 2. 定义接口，标记 [ExtensionSource]
[ExtensionSource]
public interface IMyService
{
    string GetName();
    void SetValue(int x);
    int this[string key] { get; set; }
}

// 3. 生成后，可以在任意代码中通过扩展调用：
//    var service = ...; // 实现了 IMyService 的对象
//    string name = service.GetName();  // 自动生成的扩展方法
//    service.SetValue(42);

// ---- 或者使用 ExtensionFor ----

[ExtensionFor(typeof(IMyService))]
public static partial class MyStaticExtensions
{
}
```

## 项目结构

```
src/InterfaceImplAsExtensionGenerator/
├── InterfaceImplAsExtensionGenerator.csproj   # 项目文件 (netstandard2.0, Roslyn组件)
├── ExtensionForAttribute.cs                   # [ExtensionFor] 特性定义
├── ExtensionGlobalConfigAttribute.cs          # [ExtensionGlobalConfig] 程序集级配置
├── ExtensionIgnoreAttribute.cs                # [ExtensionIgnore] 排除标记
├── ExtensionIncludeAttribute.cs               # [ExtensionInclude] 包含标记
├── ExtensionSourceAttribute.cs                # [ExtensionSource] 接口标记
├── Readme.md                                  # 原英文文档
├── README.md                                  # 本文档
├── Config/
│   ├── ExtensionSyntaxVersion.cs              # 语法版本枚举
│   ├── GenerateMembers.cs                     # 成员类型按位枚举
│   └── PublicModifier.cs                      # 访问修饰符枚举
├── DiagnosticDefine/
│   ├── AnalyzerReleases.Shipped.md            # 已发布诊断列表
│   ├── AnalyzerReleases.Unshipped.md          # 未发布诊断列表
│   └── ExtensionDiagnostic.cs                 # 诊断定义 (ZMS005-ZMS007)
├── Gener/
│   └── InterfaceExtensionGenerator.cs         # 入口：IIncrementalGenerator
├── Builder/
│   ├── ClassBuildDispatcher.cs                # [ExtensionFor] 的分发器
│   ├── ClassBuilder.cs                        # 类级扩展构建器
│   ├── InterfaceBuildDispatcher.cs            # [ExtensionSource] 的分发器
│   ├── InterfaceBuilder.cs                    # 接口扩展构建器（主逻辑）
│   ├── Helper/
│   │   ├── AnalyzerExtensions.cs              # 符号扩展方法
│   │   ├── DeferredActionScope.cs             # 延迟执行作用域（用于括号闭合）
│   │   ├── ExtensionConfig.cs                 # 配置聚合（层级继承）
│   │   ├── ExtensionMemberConfig.cs           # 成员级配置
│   │   ├── IfThenExtensions.cs                # 条件选择扩展
│   │   ├── IndentedTextWriterHelpers.cs       # 代码写入帮助
│   │   ├── TypeParameterBuild.cs              # 泛型参数重命名与约束
│   │   └── ValidAttributeParameterType.cs     # 特性参数类型验证
│   └── Member/
│       ├── MemberBuilder.cs                   # 成员构建器基类 + 工厂
│       ├── MethodBuilder.cs                   # 方法扩展生成
│       ├── PropertyBuilder.cs                 # 属性扩展生成
│       ├── PropertyOrIndexerBuilder.cs        # 属性/索引器基类
│       ├── IndexerBuilder.cs                  # 索引器扩展生成
│       └── EventBuilder.cs                    # 事件扩展生成
```

## 诊断 ID

| ID | 严重性 | 说明 |
|----|--------|------|
| `ZMS005` | Warning | 枚举参数默认值有多个匹配项 |
| `ZMS006` | Error | `[ExtensionFor]` 只能用于顶级非泛型静态类 |
| `ZMS007` | Error | `[ExtensionFor]` 的 `AppendType` 必须是接口类型 |

---

*该生成器由 [zms9110750](https://github.com/zms9110750) 开发，基于 MIT 许可证开源。*
