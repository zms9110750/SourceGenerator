namespace zms9110750.MetaSourceGenerator.AttributeFactory.DiagnosticDefine
{
    static class FromAttributeDiagnostic
    {
        private const string HelpLink = "https://github.com/zms9110750/SourceGenerator/blob/main/MetaSourceGenerator.AttributeFactory/DiagnosticDefine/AnalyzerReleases.Shipped.md";

        /// <summary>
        /// 作用于类时，只能作用于特性类
        /// </summary>
        public readonly static DiagnosticDescriptor ZMS001 = new DiagnosticDescriptor(
            id: nameof(ZMS001),
            title: "FromAttributeDataAttribute 不会工作",
            messageFormat: "FromAttributeDataAttribute作用于类时，只能作用于特性类",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink);

        /// <summary>
        /// 作用于类时，不能作用于抽象类
        /// </summary>
        public readonly static DiagnosticDescriptor ZMS002 = new DiagnosticDescriptor(
            id: nameof(ZMS002),
            title: "FromAttributeDataAttribute 不会工作",
            messageFormat: "FromAttributeDataAttribute作用于类时，不能作用于抽象类",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink);

        /// <summary>
        /// 特性构造器包含Type或Type[]参数，但没有对应的INamedTypeSymbol或INamedTypeSymbol[]参数的重载构造器
        /// </summary>
        public readonly static DiagnosticDescriptor ZMS003 = new DiagnosticDescriptor(
            id: nameof(ZMS003),
            title: "Type参数构造器缺少重载",
            messageFormat: "特性构造器包含Type或Type[]参数，但没有对应的INamedTypeSymbol或INamedTypeSymbol[]参数的重载构造器。源码生成器无法通过这些构造器创建实例。",
            category: "Usage",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink);
    }
}