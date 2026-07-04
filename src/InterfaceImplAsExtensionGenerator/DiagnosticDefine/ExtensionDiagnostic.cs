namespace zms9110750.InterfaceImplAsExtensionGenerator.DiagnosticDefine;

internal static class ExtensionDiagnostic
{
    private const string HelpLink = "https://github.com/zms9110750/SourceGenerator/blob/main/InterfaceImplAsExtensionGenerator/DiagnosticDefine/AnalyzerReleases.Shipped.md";

    /// <summary>
    /// 枚举值不唯一
    /// </summary>
    public readonly static DiagnosticDescriptor ZMS005 = new DiagnosticDescriptor(
        id: nameof(ZMS005),
        title: "枚举值不唯一",
        messageFormat: "这个方法的参数默认值，有多个枚举值与当前默认值相同。",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: HelpLink);

    /// <summary>
    /// 只能作用于顶级无泛型静态类
    /// </summary>
    public readonly static DiagnosticDescriptor ZMS006 = new DiagnosticDescriptor(
        id: nameof(ZMS006),
        title: "只允许作用于顶级非泛型静态类",
        messageFormat: "ExtensionForAttribute 仅能作用于顶级非泛型静态类。",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpLink);

    /// <summary>
    /// 只能作用于接口类型
    /// </summary>
    public readonly static DiagnosticDescriptor ZMS007 = new DiagnosticDescriptor(
        id: nameof(ZMS007),
        title: "只允许作用于接口类型",
        messageFormat: "ExtensionForAttribute 仅能为接口生成扩展，类型参数必须是接口。",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpLink);
}
