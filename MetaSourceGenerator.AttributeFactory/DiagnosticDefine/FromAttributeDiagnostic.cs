namespace zms9110750.MetaSourceGenerator.AttributeFactory.DiagnosticDefine
{
    static class FromAttributeDiagnostic
    {
        /// <summary>
        /// 只能用于只有一个参数的方法
        /// </summary>
        public readonly static DiagnosticDescriptor ZMS001 = new DiagnosticDescriptor(
            id: nameof(ZMS001),
            title: "FromAttributeDataAttribute 不会工作",
            messageFormat: "FromAttributeDataAttribute只能用于只有一个参数的方法",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// 不能用于有ref,in,out,params参数的方法
        /// </summary>
        public readonly static DiagnosticDescriptor ZMS002 = new DiagnosticDescriptor(
            id: nameof(ZMS002),
            title: "FromAttributeDataAttribute 不会工作",
            messageFormat: "FromAttributeDataAttribute不能用于有ref,in,out,params参数的方法",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// 不能用于泛型方法
        /// </summary>
        public readonly static DiagnosticDescriptor ZMS003 = new DiagnosticDescriptor(
            id: nameof(ZMS003),
            title: "FromAttributeDataAttribute 不会工作",
            messageFormat: "FromAttributeDataAttribute不能用于泛型方法",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// 只能作用与返回值是特性的方法
        /// </summary>
        public readonly static DiagnosticDescriptor ZMS004 = new DiagnosticDescriptor(
            id: nameof(ZMS004),
            title: "FromAttributeDataAttribute 不会工作",
            messageFormat: "FromAttributeDataAttribute只能作用与返回值是特性的方法",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// 不能作用返回值是抽象类的方法
        /// </summary>
        public readonly static DiagnosticDescriptor ZMS005 = new DiagnosticDescriptor(
            id: nameof(ZMS005),
            title: "FromAttributeDataAttribute 不会工作",
            messageFormat: "FromAttributeDataAttribute不能作用返回值是抽象类的方法",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// 只能作用于参数类型是AttributeData的方法
        /// </summary>
        public readonly static DiagnosticDescriptor ZMS006 = new DiagnosticDescriptor(
            id: nameof(ZMS006),
            title: "FromAttributeDataAttribute 不会工作",
            messageFormat: "FromAttributeDataAttribute只能作用于参数类型是AttributeData的方法",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// 特性包含Type或Type[]类型的属性，源码生成器无法为此类属性生成赋值代码。
        /// </summary>
        public readonly static DiagnosticDescriptor ZMS007 = new DiagnosticDescriptor(
            id: nameof(ZMS007),
            title: "Type类型属性无法生成代码",
            messageFormat: "特性包含Type或Type[]类型的属性，源码生成器无法为此类属性生成赋值代码。",
            category: "Usage",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);


        /// <summary>
        /// 作用于类时，只能作用于特性类
        /// </summary>
        public readonly static DiagnosticDescriptor ZMS020 = new DiagnosticDescriptor(
            id: nameof(ZMS020),
            title: "FromAttributeDataAttribute 不会工作",
            messageFormat: "FromAttributeDataAttribute作用于类时，只能作用于特性类",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// 作用于类时，不能作用于抽象类
        /// </summary>
        public readonly static DiagnosticDescriptor ZMS021 = new DiagnosticDescriptor(
            id: nameof(ZMS021),
            title: "FromAttributeDataAttribute 不会工作",
            messageFormat: "FromAttributeDataAttribute作用于类时，不能作用于抽象类",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// 特性构造器包含Type或Type[]参数，但没有对应的INamedTypeSymbol或INamedTypeSymbol[]参数的重载构造器
        /// </summary>
        public readonly static DiagnosticDescriptor ZMS022 = new DiagnosticDescriptor(
            id: nameof(ZMS022),
            title: "Type参数构造器缺少重载",
            messageFormat: "特性构造器包含Type或Type[]参数，但没有对应的INamedTypeSymbol或INamedTypeSymbol[]参数的重载构造器。源码生成器无法通过这些构造器创建实例。",
            category: "Usage",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);


    }
}