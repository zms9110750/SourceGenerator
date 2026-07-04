; Shipped analyzer releases
; https://github.com/dotnet/roslyn/blob/main/src/RoslynAnalyzers/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 0.1.1

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
ZMS001 | Usage | Error | FromAttributeDataAttribute 只能作用于特性类
ZMS002 | Usage | Error | 不能作用于抽象类
ZMS003 | Usage | Info | Type 参数构造器缺少 ITypeSymbol 重载

