namespace zms9110750.InterfaceImplAsExtensionGenerator
{
    /// <summary>
    /// 接口成员级别的扩展生成规则
    /// </summary>
    /// <remarks>
    /// 作用于接口的具体成员（方法、属性等），未设置的属性继承接口级或全局配置。<br/>
    /// 使用这个特性会强制生成这个成员。即便不包含在配置中。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event)]
    [FromAttributeData]
    public partial class ExtensionIncludeAttribute : Attribute
    {
        /// <summary>
        /// 生成的扩展成员的替代名称
        /// </summary>
        /// <remarks>
        /// 用于替换原成员名称的扩展成员名。不可为 null 或空字符串。
        /// 未设置时使用原成员名称。
        /// </remarks>
        public string? ReplacementMemberName { get; set; }

        /// <summary>
        /// 扩展方法实例参数的名称
        /// </summary>
        /// <remarks>
        /// 未使用扩展方法语法时，此属性不生效
        /// 未设置时继承接口级或程序集级 InstanceParameterName。
        /// </remarks>
        public string? InstanceParameterName { get; set; }
    }
}