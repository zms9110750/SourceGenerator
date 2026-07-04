; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 0.3.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
ZMS005 | Usage | Warning | 枚举默认值冲突
ZMS006 | Usage | Error | ExtensionForAttribute 只允许作用于顶级非泛型静态类
ZMS007 | Usage | Error | ExtensionForAttribute 的 AppendType 必须为接口
