namespace zms9110750.InterfaceImplAsExtensionGenerator.Builder.Helper;

internal class ExtensionMemberConfig
{
    public ExtensionMemberConfig(ExtensionConfig config, ISymbol symbol)
    {
        InstanceName = config.InstanceName;
        MemberName = symbol.Name;
        if (symbol.GetAttributes().Select(ExtensionIncludeAttribute.Create).FirstOrDefault(att => att != null) is { } include)
        {
            Include = true;
            if (include.InstanceParameterName != null)
            {
                InstanceName = include.InstanceParameterName!;
            }
            if (include.ReplacementMemberName != null)
            {
                MemberName = include.ReplacementMemberName;
                if (symbol is IMethodSymbol { MethodKind: not MethodKind.Ordinary and var kind })
                {
                    MemberName = kind switch
                    {
                        MethodKind.EventAdd => "add_" + MemberName,
                        MethodKind.EventRemove => "remove_" + MemberName,
                        MethodKind.PropertyGet => "get_" + MemberName,
                        MethodKind.PropertySet => "set_" + MemberName,
                        _ => MemberName
                    };
                }
            }
        }
        else
        {
            Include = symbol switch
            {
                IEventSymbol => config.GenerateMembers.HasFlag(GenerateMembers.Event),
                IPropertySymbol { IsIndexer: true } => config.GenerateMembers.HasFlag(GenerateMembers.Indexer),
                IPropertySymbol => config.GenerateMembers.HasFlag(GenerateMembers.Property),
                IMethodSymbol => config.GenerateMembers.HasFlag(GenerateMembers.Method),
                _ => false
            };
        }
        MemberName = MemberName.EscapeKeywords();
        IsPublic = symbol is { DeclaredAccessibility: Accessibility.Public, ContainingType.DeclaredAccessibility: Accessibility.Public };
        if (symbol.GetAttributes().Select(ExtensionIgnoreAttribute.Create).Any(att => att != null))
        {
            Include = false;
        }
    }
    /// <summary>
    /// 实例参数名称
    /// </summary>
    public string InstanceName { get; }
    /// <summary>
    /// 扩展成员的名称
    /// </summary>
    public string MemberName { get; }
    /// <summary>
    /// 是否具有public修饰符
    /// </summary>
    public bool IsPublic { get; }
    /// <summary>
    /// 包含这个成员
    /// </summary>
    public bool Include { get; }
}