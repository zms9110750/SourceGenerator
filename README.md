# SourceGenerator

Roslyn 源生成器项目集合。编译时生成代码，减少手写样板，提升开发效率。

## 项目一览

### [`src/StaticMethodAsExtensionGenerator`](src/StaticMethodAsExtensionGenerator/Readme.md)

**静态方法 → 扩展方法生成器**

编译时扫描引用程序集中的静态方法，自动生成扩展方法。将 `String.IsNullOrEmpty(str)` 变为 `str.IsNullOrEmpty()`。

- 可配置扫描范围（System / SystemAll / BCL / Microsoft / NuGet）
- 支持 `public` / `internal` 访问修饰符配置
- 智能过滤：跳过 `[Obsolete]`、`[EditorBrowsable(Never)]`、泛型、ref/in/out 参数、实例冲突
- 增量生成，不影响编译性能

### [`src/InterfaceImplAsExtensionGenerator`](src/InterfaceImplAsExtensionGenerator/README.md)

**接口实现 → 扩展方法生成器**

编译时自动为接口成员生成扩展方法（C# 14+ extension blocks 兼容）。

- 多种配置方式：`[ExtensionFor]`、`[ExtensionSource]`、`[ExtensionGlobalConfig]`
- 细粒度成员控制：`[ExtensionInclude]` / `[ExtensionIgnore]`
- 配置优先级：程序集级 → 接口级 → 类级 → 成员级

### [`src/MetaSourceGenerator.AttributeFactory`](src/MetaSourceGenerator.AttributeFactory/README.md)

**特性工厂元生成器**

元源代码生成器 —— 从特性类声明自动生成基于 `AttributeData` 的工厂方法。

- 只需标记 `[FromAttributeData]` 即可让特性类拥有 `Create(AttributeData)` 工厂方法
- 自动处理构造器参数映射、`Type` → `ITypeSymbol` 转换
- 无运行时反射，完全编译时解析

## 诊断 ID

| ID | 项目 | 说明 |
|----|------|------|
| ZMS001 | AttributeFactory | `FromAttributeData` 只能作用于特性类 |
| ZMS002 | AttributeFactory | 不能作用于抽象类 |
| ZMS003 | AttributeFactory | `Type` 参数构造器缺少 `ITypeSymbol` 重载 |
| ZMS005 | InterfaceImplAsExtension | 枚举默认值冲突 |
| ZMS006 | InterfaceImplAsExtension | `ExtensionForAttribute` 只允许作用于顶级非泛型静态类 |
| ZMS007 | InterfaceImplAsExtension | `ExtensionForAttribute` 的 `AppendType` 必须为接口 |

## 目录结构

```
SourceGenerator/
├── src/
│   ├── InterfaceImplAsExtensionGenerator/   # 接口→扩展生成器
│   ├── MetaSourceGenerator.AttributeFactory/ # 特性工厂元生成器
│   └── StaticMethodAsExtensionGenerator/     # 静态→扩展方法生成器
├── samples/
│   ├── AttributeFactoryDemo/
│   ├── InterfaceExtensionsDemo/
│   └── StaticMethodExtensionDemo/
├── test/
│   ├── SourceGeneratorTests/
│   └── StaticMethodAsExtensionGeneratorTests/
├── SourceGenerator.slnx
└── README.md
```

## 构建与测试

```bash
dotnet build SourceGenerator.slnx
dotnet test  SourceGenerator.slnx
```
