using Microsoft.CodeAnalysis;

namespace zms9110750.MetaSourceGenerator.AttributeFactory.Gener
{
    static class FromAttributeDiagnostic
    {
        public readonly static DiagnosticDescriptor ZMS001 = new DiagnosticDescriptor(
            id: nameof(ZMS001),
            title: "FromAttributeDataAttribute 不会工作",
            messageFormat: "FromAttributeDataAttribute只能用于只有一个参数的方法",
            category: "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public readonly static DiagnosticDescriptor ZMS002 = new DiagnosticDescriptor(
            id: nameof(ZMS002),
            title: "FromAttributeDataAttribute 不会工作",
            messageFormat: "FromAttributeDataAttribute不能用于有ref,in,out,params参数的方法",
            category: "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public readonly static DiagnosticDescriptor ZMS003 = new DiagnosticDescriptor(
            id: nameof(ZMS003),
            title: "FromAttributeDataAttribute 不会工作",
            messageFormat: "FromAttributeDataAttribute不能用于泛型方法",
            category: "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public readonly static DiagnosticDescriptor ZMS004 = new DiagnosticDescriptor(
            id: nameof(ZMS004),
            title: "FromAttributeDataAttribute 不会工作",
            messageFormat: "FromAttributeDataAttribute只能作用与返回值是特性的方法",
            category: "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public readonly static DiagnosticDescriptor ZMS005 = new DiagnosticDescriptor(
            id: nameof(ZMS005),
            title: "FromAttributeDataAttribute 不会工作",
            messageFormat: "FromAttributeDataAttribute不能作用返回值是抽象类的方法",
            category: "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public readonly static DiagnosticDescriptor ZMS006 = new DiagnosticDescriptor(
            id: nameof(ZMS006),
            title: "FromAttributeDataAttribute 不会工作",
            messageFormat: "FromAttributeDataAttribute只能作用于参数类型是AttributeData的方法",
            category: "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);


        public readonly static DiagnosticDescriptor ZMS007 = new DiagnosticDescriptor(
            id: nameof(ZMS007),
            title: "Type类型属性无法生成代码",
            messageFormat: "特性包含Type或Type[]类型的属性，源码生成器无法为此类属性生成赋值代码。",
            category: "Usage",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);



        public readonly static DiagnosticDescriptor ZMS020 = new DiagnosticDescriptor(
            id: nameof(ZMS020),
            title: "FromAttributeDataAttribute 不会工作",
            messageFormat: "FromAttributeDataAttribute作用于类时，只能作用于特性类",
            category: "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public readonly static DiagnosticDescriptor ZMS021 = new DiagnosticDescriptor(
            id: nameof(ZMS021),
            title: "FromAttributeDataAttribute 不会工作",
            messageFormat: "FromAttributeDataAttribute作用于类时，不能作用于抽象类",
            category: "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public readonly static DiagnosticDescriptor ZMS022 = new DiagnosticDescriptor(
            id: nameof(ZMS022),
            title: "Type参数构造器缺少重载",
            messageFormat: "特性构造器包含Type或Type[]参数，但没有对应的INamedTypeSymbol或INamedTypeSymbol[]参数的重载构造器。源码生成器无法通过这些构造器创建实例。",
            category: "Usage",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);


        public static DiagnosticDescriptor? IsValidFromAttribute(IMethodSymbol methodSymbol)
        {
            // 1. 检查方法是否只有一个参数
            if (methodSymbol.Parameters.Length != 1)
            {
                return ZMS001;
            }

            // 2. 检查参数类型是否为 AttributeData
            var paramType = methodSymbol.Parameters[0].Type;
            if (paramType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) !=
                "global::Microsoft.CodeAnalysis.AttributeData")
            {
                return ZMS006;
            }

            // 3. 检查是否有 ref/in/out/params 参数
            var parameters = methodSymbol.Parameters;
            foreach (var parameter in parameters)
            {
                if (parameter.RefKind != RefKind.None ||
                    parameter.IsParams)
                {
                    return ZMS002;
                }
            }

            // 4. 检查是否为泛型方法
            if (methodSymbol.IsGenericMethod)
            {
                return ZMS003;
            }

            // 5. 检查返回值是否为 Attribute 或其派生类
            var returnType = methodSymbol.ReturnType;

            // 检查是否为抽象类
            if (returnType.IsAbstract)
            {
                return ZMS005;
            }

            // 检查是否继承自 System.Attribute
            bool isAttributeDerived = false;
            for (ITypeSymbol? currentType = returnType;
                 currentType != null;
                 currentType = currentType.BaseType)
            {
                if (currentType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ==
                    "global::System.Attribute")
                {
                    isAttributeDerived = true;
                    break;
                }
            }

            if (!isAttributeDerived)
            {
                return ZMS004;
            }

            // 所有验证通过，返回 null
            return null;
        }
        public static DiagnosticDescriptor? IsValidFromAttribute(INamedTypeSymbol classSymbol)
        {
            // 1. 检查是否为 Attribute 的派生类
            bool isAttributeDerived = false;
            for (INamedTypeSymbol? currentType = classSymbol;
                 currentType != null;
                 currentType = currentType.BaseType)
            {
                if (currentType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ==
                    "global::System.Attribute")
                {
                    isAttributeDerived = true;
                    break;
                }
            }

            if (!isAttributeDerived)
            {
                return ZMS020; // 必须作用于特性类
            }

            if (classSymbol.IsAbstract)
            {
                return ZMS021; // 不能是抽象类
            }


            return null;
        }
    }
}