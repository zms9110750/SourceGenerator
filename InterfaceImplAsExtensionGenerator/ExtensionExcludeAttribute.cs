namespace zms9110750.InterfaceImplAsExtensionGenerator
{
    /// <summary>
    /// 不生成这个成员
    /// </summary> 
    /// <remarks>
    /// 同时使用<see cref="ExtensionIncludeAttribute"/>时，效果未定义。不要假设优先级稳定。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event)]
    [FromAttributeData]
    public partial class ExtensionExcludeAttribute : Attribute
    {
    }
}