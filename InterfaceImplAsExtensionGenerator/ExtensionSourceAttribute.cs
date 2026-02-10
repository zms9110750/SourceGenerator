namespace zms9110750.InterfaceImplAsExtensionGenerator
{
    [AttributeUsage(AttributeTargets.Interface)]
    [FromAttributeData]
    public partial class ExtensionSourceAttribute : Attribute
    {

        /// <summary>
        /// 扩展类的名称
        /// </summary>
        /// <remarks>
        /// 未设置时，使用接口名 + <see cref="ExtensionGlobalConfigAttribute.TypeNameSuffix"/>生成。
        /// </remarks>
        public string? ExtensionClassName { get => string.IsNullOrWhiteSpace(field) ? null : field; set; }

        /// <summary>
        /// 扩展类所在的命名空间
        /// </summary>
        /// <remarks>
        /// 未设置时，使用接口原命名空间 + <see cref="ExtensionGlobalConfigAttribute.NamespaceSuffix"/>生成。
        /// </remarks>
        public string? ExtensionClassNamespace { get => string.IsNullOrWhiteSpace(field) ? null : field; set; }

        /// <summary>
        /// 实例参数的名称
        /// </summary>
        /// <remarks>
        /// 未设置时使用<see cref="ExtensionGlobalConfigAttribute.InstanceParameterName"/>
        /// </remarks>
        public string? InstanceParameterName { get => string.IsNullOrWhiteSpace(field) ? null : field; set; }

        /// <summary>
        /// 为当前接口默认生成的成员类型（按位枚举）
        /// </summary>
        /// <remarks>
        /// 为当前接口生成的成员类型组合。未设置时使用<see cref="ExtensionGlobalConfigAttribute.DefaultGenerateMembers"/>
        /// </remarks>
        public GenerateMembers DefaultGenerateMembers { get; set; }

        /// <summary>
        /// 生成扩展类使用 public 修饰
        /// </summary> 
        /// <remarks>
        /// 未设置时使用<see cref="ExtensionGlobalConfigAttribute.UsePublic"/>
        /// </remarks>
        public PublicModifier UsePublic { get; set; }
    }
}