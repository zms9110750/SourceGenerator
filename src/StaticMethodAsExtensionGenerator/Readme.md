# StaticMethodAsExtensionGenerator

> **编译时为 BCL 类型自动生成实例扩展方法的 Roslyn 源生成器。**  
> 将 `String.IsNullOrEmpty(str)` 书写为 `str.IsNullOrEmpty()`，让链式调用更流畅。

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
![.NET](https://img.shields.io/badge/.NET-netstandard2.0-512BD4)
![Roslyn](https://img.shields.io/badge/Roslyn-Incremental%20Generator-7B1FA2)

---

## 目录

- [解决的问题](#解决的问题)
- [快速开始](#快速开始)
- [配置说明](#配置说明)
- [过滤规则](#过滤规则)
- [工作原理](#工作原理)
- [生成的代码示例](#生成的代码示例)
- [跨框架适配](#跨框架适配)
- [项目结构](#项目结构)
- [许可证](#许可证)

---

## 解决的问题

在 C# 中，许多实用的静态方法（如 `string.IsNullOrEmpty`、`char.IsDigit`、`Path.Combine`）只能通过类名调用，无法在链式表达式中直接使用：

```csharp
// 传统写法 —— 需要中间变量，破坏流畅性
var text = GetResult();
var empty = string.IsNullOrEmpty(text);
var combined = Path.Combine(baseDir, fileName);
```

利用源生成器在**编译时**自动为这些静态方法生成对应的扩展方法，就能写出自然的链式调用：

```csharp
// 扩展方法风格 —— 一句完成
var empty = GetResult().IsNullOrEmpty();
var combined = baseDir.Combine(fileName);
```

---

## 快速开始

### 1. 引用 NuGet 包

在目标项目中添加以下 `PackageReference`：

```xml
<ItemGroup>
  <PackageReference Include="zms9110750.StaticMethodAsExtensionGenerator" Version="0.1.4.0"
                    PrivateAssets="all" OutputItemType="Analyzer" />
</ItemGroup>
```

> `PrivateAssets="all"` 确保生成器不会传递到依赖项目；  
> `OutputItemType="Analyzer"` 告知 MSBuild 将其作为分析器/生成器运行。

### 2. 配置扫描范围（可选）

默认扫描 `System.*` 全部子命名空间。可通过**程序集级特性**自定义范围：

```csharp
using zms9110750.StaticMethodAsExtensionGenerator;

// 只扫描 System 命名空间本身（不含 System.IO、System.Text 等子命名空间）
[assembly: StaticMethodExtensions(StaticMethodExtensionScope.System)]

// System 及其所有子命名空间（默认值）
[assembly: StaticMethodExtensions(StaticMethodExtensionScope.SystemAll)]

// 整个基类库（包括 System.* 及 Microsoft.Win32 等内建程序集）
[assembly: StaticMethodExtensions(StaticMethodExtensionScope.BCL)]

// Microsoft 开头的 NuGet 包（Microsoft.Extensions.* 等）
[assembly: StaticMethodExtensions(StaticMethodExtensionScope.Microsoft)]

// 所有第三方 NuGet 包
[assembly: StaticMethodExtensions(StaticMethodExtensionScope.NuGet)]

// 支持按位组合
[assembly: StaticMethodExtensions(StaticMethodExtensionScope.SystemAll | StaticMethodExtensionScope.Microsoft)]
```

### 3. 使用生成的扩展方法

编译完成后，在代码中引用对应命名空间即可：

```csharp
using zms9110750.Extensions.Generator.System;
using zms9110750.Extensions.Generator.System.IO;
using zms9110750.Extensions.Generator.System.Text;

// String 扩展
bool empty = "hello".IsNullOrEmpty();                    // → false
string text = "Hello, ".Concat("World!");                // → "Hello, World!"

// Char 扩展
bool isDigit = 'A'.IsDigit();                            // → false
bool isUpper = 'A'.IsUpper();                            // → true

// 数值扩展（.NET 7+ 自动可见）
int abs = Int32Extensions.Abs(-5);                       // → 5
int clamped = Int32Extensions.Clamp(10, 0, 5);           // → 5

// 数组扩展
int idx = ArrayExtensions.BinarySearch(new[] { 1, 3, 5 }, 3);  // → 1

// Stream 扩展
bool canRead = new MemoryStream().CanRead;               // → true

// 路径组合
string fullPath = @"C:\Base".Combine("sub", "file.txt"); // → "C:\Base\sub\file.txt"
```

> **注意**：生成的扩展类和方法默认均为 `internal`，仅在当前程序集内可见。  
> 若需要公开给外部调用方，请设置 `Public = true`（见下文）。

---

## 配置说明

### `StaticMethodExtensionsAttribute`

标记在程序集上的配置特性，定义生成器的扫描行为。

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Scope` | `StaticMethodExtensionScope` | `SystemAll` | 扫描范围标志，支持位组合 |
| `Public` | `bool` | `false` | 生成的扩展类和方法是否为 `public`；`false` 时为 `internal` |

### `StaticMethodExtensionScope` 标志

| 枚举值 | 值 | 说明 |
|--------|----|------|
| `System` | `1` | 仅 `System` 命名空间（不含子命名空间） |
| `SystemAll` | `2` | `System` 及其所有子命名空间（默认） |
| `BCL` | `4` | 整个基类库，包括 `System.*` 及内建程序集 |
| `Microsoft` | `8` | `Microsoft.*` 开头的 NuGet 包 |
| `NuGet` | `16` | 所有第三方 NuGet 包 |

示例 — 扫描 `System` 所有子命名空间 + `Microsoft` 开头的包：

```csharp
[assembly: StaticMethodExtensions(StaticMethodExtensionScope.SystemAll | StaticMethodExtensionScope.Microsoft)]
```

示例 — 公开生成的扩展 API：

```csharp
[assembly: StaticMethodExtensions(StaticMethodExtensionScope.BCL, Public = true)]
```

此时生成的扩展类和方法为 `public`，可供其他程序集调用。

---

## 过滤规则

生成器在扫描过程中会跳过不符合条件的类型和方法，确保生成的代码安全、无歧义。

### 类型过滤

| 条件 | 是否生成扩展 |
|------|------------|
| `public` 类型 | ✅ |
| 非泛型类型 | ✅ |
| `class` 或 `struct` | ✅ |
| 静态类（`static class`） | ❌ 跳过 |
| `internal` / `private` 类型 | ❌ 跳过 |
| 泛型类型 | ❌ 跳过 |
| 嵌套类型 | ❌ 跳过 |
| `System.Object` 自身 | ❌ 跳过 |

### 方法过滤

| 条件 | 是否生成扩展 |
|------|------------|
| `public static` 方法 | ✅ |
| 第一个参数类型 == 所在类型 | ✅ |
| 普通方法（`MethodKind.Ordinary`） | ✅ |
| 标记 `[Obsolete]` | ❌ 跳过 |
| 标记 `[EditorBrowsable(EditorBrowsableState.Never)]` | ❌ 跳过 |
| 泛型方法 | ❌ 跳过 |
| 含 `ref` / `in` / `out` 参数 | ❌ 跳过 |
| 与同类型实例方法签名冲突 | ❌ 跳过 |
| 参数名为 C# 关键字 | ✅ 自动添加 `@` 前缀 |

### 冲突检测

若某静态方法与同类型中已有的实例方法具有**相同名称**且**参数列表匹配**（忽略第一个参数），则跳过该静态方法，避免调用歧义。

---

## 工作原理

```
┌─────────────────────────────────────────────────────────┐
│                    编译时 (Roslyn)                        │
│                                                         │
│  1. StaticMethodExtensionGenerator                      │
│     (IIncrementalGenerator)                             │
│         ↓                                              │
│  2. StaticBuildDispatcher                               │
│     - 读取 [assembly: StaticMethodExtensions] 特性      │
│     - 通过 System.Object 定位核心运行时程序集           │
│         ↓                                              │
│  3. AssemblyScanner                                     │
│     - 遍历所有引用程序集                                │
│     - 根据 Scope 判断是否扫描                          │
│     - 递归扫描匹配的命名空间                            │
│     - 筛选符合条件的类型和方法                          │
│         ↓                                              │
│  4. StaticNamespaceBuilder                              │
│     - 按命名空间分组生成 .g.cs 文件                     │
│     - 使用 IndentedTextWriter 管理缩进                  │
│     - 生成 <inheritdoc cref="..."/> 文档注释            │
│         ↓                                              │
│  5. 输出 Source Production                              │
│     - System.g.cs                                       │
│     - System.IO.g.cs                                    │
│     - System.Text.g.cs                                  │
│     - ...                                               │
└─────────────────────────────────────────────────────────┘
```

### 详细步骤

1. **入口**：`StaticMethodExtensionGenerator` 实现 `IIncrementalGenerator`，注册 `CompilationProvider`。

2. **调度**：`StaticBuildDispatcher` 从编译上下文的程序集特性中读取 `StaticMethodExtensionsAttribute`，获取扫描范围（`Scope`）和可见性（`Public`）。

3. **定位运行时**：通过 `compilation.GetTypeByMetadataName("System.Object")?.ContainingAssembly` 找到真正的核心运行时程序集（如 `System.Runtime`），用于后续判断 BCL 程序集。

4. **扫描**：`AssemblyScanner` 遍历所有引用程序集：
   - 跳过 `Microsoft.CodeAnalysis` 和 `Microsoft.CSharp` 等生成器自身依赖。
   - 根据 `Scope` 判断每个程序集是否需要扫描（BCL / Microsoft / NuGet）。
   - 递归进入匹配的命名空间，扫描其中的 `public` 非泛型非静态的 `class` / `struct`。
   - 对每个类型筛选符合条件的方法，存入 `Dictionary<命名空间, List<类型扩展信息>>`。

5. **生成**：`StaticNamespaceBuilder` 对每个命名空间生成一个 `.g.cs` 文件：
   - 命名空间形如 `zms9110750.Extensions.Generator.System`。
   - 每个类型生成一个 `{TypeName}Extensions` 类（`internal` 或 `public`）。
   - 每个扩展方法添加 `<inheritdoc cref="..." />` 文档注释，指向原始静态方法。
   - 扩展方法体直接转发调用：`return TypeName.MethodName(args);`。

6. **错误处理**：若生成过程中抛出异常，会输出一个 `_error.g.cs` 文件包含异常信息，方便调试。

---

## 生成的代码示例

假设 `System.String` 中有以下静态方法：

```csharp
public static bool IsNullOrEmpty(string value);
public static string Concat(string str0, string str1);
```

生成器将输出类似如下的代码（在 `zms9110750.Extensions.Generator.System` 命名空间下）：

```csharp
// <auto-generated />

namespace zms9110750.Extensions.Generator.System
{
    internal static class StringExtensions
    {
        /// <inheritdoc cref="string.IsNullOrEmpty(string)" />
        internal static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <inheritdoc cref="string.Concat(string, string)" />
        internal static string Concat(this string str0, string str1)
        {
            return string.Concat(str0, str1);
        }
    }
}
```

若设置了 `Public = true`，则 `internal` 变为 `public`。

---

## 跨框架适配

生成器在编译时自动识别目标框架，只生成目标框架上真实存在的 API。不同 .NET 版本之间的 API 差异无需手动维护。

| 目标框架 | `Int32.Abs(int)` 是否存在 | `StringExtensions` 是否生成 |
|---------|--------------------------|---------------------------|
| .NET 5  | ❌ 不存在                | ✅                        |
| .NET 7  | ✅ 新增                  | ✅                        |
| .NET 8  | ✅                       | ✅                        |
| .NET 10 | ✅                       | ✅                        |

> 代码中可以放心使用 `Int32Extensions.Abs(x)`——若当前框架没有对应 API，生成器不会生成该扩展方法，编译器会报告 `CS0103` 错误，让开发者第一时间感知到 API 不可用。

---

## 项目结构

```
StaticMethodAsExtensionGenerator/
├── StaticMethodAsExtensionGenerator.csproj   # netstandard2.0, IsRoslynComponent
├── StaticMethodExtensionsAttribute.cs        # [assembly: StaticMethodExtensions] 特性定义
├── Gener/
│   └── StaticMethodExtensionGenerator.cs     # IIncrementalGenerator 入口
├── Builder/
│   ├── StaticBuildDispatcher.cs              # 读取配置、调度扫描与生成
│   ├── AssemblyScanner.cs                    # 扫描引用程序集，筛选类型和方法
│   ├── StaticNamespaceBuilder.cs             # 按命名空间生成扩展类代码
│   ├── TypeExtensionInfo.cs                  # 类型扩展信息的数据包装
│   └── Helper/
│       ├── DeferredActionScope.cs            # 延迟执行作用域（用于大括号管理）
│       └── IndentedTextWriterHelpers.cs      # IndentedTextWriter 扩展方法
└── README.md                                 # 本文件
```

### 关键依赖

| 包名 | 用途 |
|------|------|
| `Microsoft.CodeAnalysis.CSharp` (5.3.0) | Roslyn 编译平台 |
| `Microsoft.CodeAnalysis.Analyzers` | 分析器 SDK |
| `zms9110750.MetaSourceGenerator.AttributeFactory` | 特性构造器自动解析（消除手动解析 `AttributeData` 的样板代码） |

---

## 许可证

本项目基于 [MIT](LICENSE) 许可证开源。

---

*StaticMethodAsExtensionGenerator — 让静态方法享受扩展方法的便利，编译时生成，零运行时开销。*
