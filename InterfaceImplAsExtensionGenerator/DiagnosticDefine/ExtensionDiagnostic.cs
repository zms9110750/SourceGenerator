using System;
using System.Collections.Generic;
using System.Text;

namespace zms9110750.InterfaceImplAsExtensionGenerator.DiagnosticDefine;

internal class ExtensionDiagnostic
{
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
        helpLinkUri: "https://learn.microsoft.com/zh-cn/dotnet/fundamentals/code-analysis/quality-rules/ca1069"
        );

    /// <summary>
    /// 只能作用于顶级无泛型静态类
    /// </summary>
    public readonly static DiagnosticDescriptor ZMS006 = new DiagnosticDescriptor(
        id: nameof(ZMS006),
        title: "只能作用于顶级非泛型静态类",
        messageFormat: "这个特性用于生成扩展方法。作用类必须满足扩展方法容器条件。必须不是嵌套的，非泛型的，静态类",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
        );

    /// <summary>
    /// 只能作用于接口类型
    /// </summary>
    public readonly static DiagnosticDescriptor ZMS007 = new DiagnosticDescriptor(
        id: nameof(ZMS007),
        title: "只能作用于接口类型",
        messageFormat: "这个特性仅能为接口生成扩展方法。不能传入其他类型的类型参数。",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
        );
}
