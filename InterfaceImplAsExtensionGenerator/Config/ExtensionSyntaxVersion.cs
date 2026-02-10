namespace zms9110750.InterfaceImplAsExtensionGenerator.Config;

/// <summary>
/// 扩展语法版本控制
/// </summary> 
public enum ExtensionSyntaxVersion
{ 
    /// <summary>
    /// 继承上级配置或使用默认值
    /// </summary>
    InheritOrDefault = 0,

    /// <summary>
    /// 只生成新版扩展语法
    /// </summary>
    /// <remarks>
    /// 仅生成新的扩展语法块形式
    /// </remarks>
    ExtensionBlock = 1,

    /// <summary>
    /// 只生成旧版扩展方法语法
    /// </summary>
    /// <remarks>
    /// 仅生成传统的扩展方法语法
    /// </remarks>
    ExtensionMethod = 2,

    /// <summary>
    /// 两者都生成（使用条件编译）
    /// </summary>
    /// <remarks>
    /// 生成两种语法形式，通过 #if NET10_OR_GREATER 条件编译区分
    /// </remarks>
    Both = ExtensionBlock | ExtensionMethod,
}