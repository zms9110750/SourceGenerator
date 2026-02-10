namespace zms9110750.InterfaceImplAsExtensionGenerator.Config;

/// <summary>
/// 是否添加puclic修饰符的设定
/// </summary>
public enum PublicModifier
{
    /// <summary>
    /// 使用继承的或默认的行为
    /// </summary>
    InheritOrDefault = 0,

    /// <summary>
    /// 不添加 public
    /// </summary>
    NoModifier = 1,

    /// <summary>
    /// 与接口的访问修饰符相同
    /// </summary>
    FollowInterface = 2,

    /// <summary>
    /// 总是添加
    /// </summary>
    Always=3
}