# StaticMethodAsExtensionGenerator

编译时为 BCL 类型自动生成实例扩展方法的 Roslyn 源生成器。

## 动机

`string.IsNullOrEmpty("text")` 这种写法在链式调用里很别扭：

```csharp
// 要拆变量才能用
var text = GetResult();
var empty = string.IsNullOrEmpty(text);
```

有了扩展方法就可以直接链下去：

```csharp
// 一句话完成
var empty = GetResult().IsNullOrEmpty();
```

传统的 T4 模板方案只能反射设计时程序集，对不同 .NET 版本（.NET 5/6/7/8/9/10）的 API 差异只能手工维护。
本源生成器在编译时直接读取 Roslyn 编译上下文，自动为目标框架生成正确的扩展方法。

## 快速开始

### 1. 引用包

```xml
<PackageReference Include="zms9110750.StaticMethodAsExtensionGenerator" Version="0.1.0.0"
                  PrivateAssets="all" OutputItemType="Analyzer" />
```

### 2. 配置扫描范围（可选）

默认扫描 `System.*` 全部子命名空间。可通过程序集级特性调整：

```csharp
using zms9110750.StaticMethodAsExtensionGenerator;

// 只扫 System 命名空间本身（不含 System.IO、System.Text 等）
[assembly: StaticMethodExtensions(StaticMethodExtensionScope.System)]

// System 及其子命名空间全部（默认）
[assembly: StaticMethodExtensions(StaticMethodExtensionScope.SystemAll)]

// 整个基类库
[assembly: StaticMethodExtensions(StaticMethodExtensionScope.BCL)]

// Microsoft 开头的 NuGet 包
[assembly: StaticMethodExtensions(StaticMethodExtensionScope.Microsoft)]

// 所有第三方 NuGet 包
[assembly: StaticMethodExtensions(StaticMethodExtensionScope.NuGet)]

// 支持组合
[assembly: StaticMethodExtensions(StaticMethodExtensionScope.SystemAll | StaticMethodExtensionScope.Microsoft)]
```

### 3. 使用

```csharp
using System.Extensions;
using System.IO.Extensions;

// String
bool empty = "hello".IsNullOrEmpty();                // → false
string text = "Hello, ".Concat("World!");            // → "Hello, World!"

// 数值（.NET 7+ 自动可见）
int abs = Int32Extensions.Abs(-5);                    // → 5
int clamped = Int32Extensions.Clamp(10, 0, 5);        // → 5

// Char
bool isDigit = 'A'.IsDigit();                         // → false
bool isUpper = 'A'.IsUpper();                         // → true

// Array
int idx = ArrayExtensions.BinarySearch(new[] { 1, 3, 5 }, 3);  // → 1

// Stream
bool canRead = new MemoryStream().CanRead;            // → true
```

## 跨框架适配

生成器在编译时自动识别目标框架，只生成该框架上存在的方法。

| 目标框架 | `int.Abs` 扩展 | `StringExtensions` |
|---------|---------------|-------------------|
| .NET 5  | ❌ 不存在 | ✅ |
| .NET 7  | ✅ 新增 | ✅ |
| .NET 8  | ✅ | ✅ |
| .NET 10 | ✅ | ✅ |

> 代码里可以放心写 `Int32Extensions.Abs(x)`——如果目标框架没有 `Int32.Abs(int)`，生成器不会生成它，
> 编译器会报告 `CS0103`，你就知道这个 API 在当前框架不可用。

## 过滤规则

| 规则 | 说明 |
|------|------|
| 类型 | public、非泛型、非静态类、class/struct |
| 方法 | `public static`、非泛型、非 `[Obsolete]`、非 `[EditorBrowsable(Never)]` |
| 参数 | 无 `ref`/`in`/`out`、第一个参数必须是该方法所在的类型 |
| 冲突 | 不与实例成员同名同签名 |
| 关键词 | 参数名为 C# 关键字的自动加 `@` 前缀 |

## 生成器如何工作

1. 通过 `GetTypeByMetadataName("System.Object")` 找到真实的核心程序集（`System.Runtime`）
2. 根据配置的 `StaticMethodExtensionScope` 遍历目标命名空间
3. 对每个类型筛选符合条件的静态方法
4. 为每个类型生成 `{TypeName}Extensions.g.cs`，包含所有扩展方法
5. 生成的类和方法均为 `internal`，不暴露到公共 API

## 项目结构

```
├── StaticMethodAsExtensionGenerator.csproj        # netstandard2.0, IsRoslynComponent
├── StaticMethodExtensionsAttribute.cs             # 配置特性定义
├── Gener/
│   └── StaticMethodExtensionGenerator.cs          # 核心生成器
└── Readme.md
```

## 许可证

MIT
