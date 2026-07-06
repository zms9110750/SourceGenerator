using zms9110750.MetaSourceGenerator.AttributeFactory;

namespace zms9110750.StaticMethodAsExtensionGenerator;

/// <summary>
/// 配置静态方法扩展生成器的扫描范围。标记在程序集上。
/// </summary>
/// <example>
/// [assembly: StaticMethodExtensions(StaticMethodExtensionScope.SystemAll | StaticMethodExtensionScope.Microsoft)]
/// </example>
/// <remarks>创建配置实例</remarks>
/// <param name="scope">要扫描的范围，支持位组合</param>
[AttributeUsage(AttributeTargets.Assembly)]
[FromAttributeData]
public partial class StaticMethodExtensionsAttribute(StaticMethodExtensionScope scope) : Attribute
{
    /// <summary>扫描范围标志组合</summary>
    public StaticMethodExtensionScope Scope { get; set; } = scope;

    /// <summary>生成的扩展类和方法是否公开（public），默认为 false（internal）</summary>
    public bool Public { get; set; } = false;

    /// <summary>默认配置实例（扫描 System.* 全部子命名空间）</summary>
    public static StaticMethodExtensionsAttribute Default { get; } = new(StaticMethodExtensionScope.SystemAll);
}

/// <summary>
/// 扫描范围标志
/// </summary>
[Flags]
public enum StaticMethodExtensionScope
{
    /// <summary>System 命名空间本身（不进入子命名空间）</summary>
    System = 1,

    /// <summary>System 及其所有子命名空间（System.Collections、System.IO、System.Text 等）</summary>
    SystemAll = 2,

    /// <summary>整个基类库（包括 System.*、Microsoft.Win32 等内建程序集）</summary>
    BCL = 4,

    /// <summary>Microsoft 开头的 NuGet 包（Microsoft.Extensions.* 等）</summary>
    Microsoft = 8,

    /// <summary>所有第三方 NuGet 包</summary>
    NuGet = 16,
}
