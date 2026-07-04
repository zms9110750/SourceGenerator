namespace zms9110750.InterfaceImplAsExtensionGenerator.Builder.Helper;

internal static class AttributeParameterHelper
{
    /// <summary>
    /// 是否是有效的特性参数类型，特性参数类型必须是以下类型之一，或以下类型的数组：
    /// <list type="bullet"> 
    /// <item>布尔类型</item>
    /// <item>字节类型</item>
    /// <item>有符号字节类型</item>
    /// <item>短整型</item>
    /// <item>无符号短整型</item>
    /// <item>整型</item>
    /// <item>无符号整型</item>
    /// <item>长整型</item>
    /// <item>无符号长整型</item>
    /// <item>单精度浮点型</item>
    /// <item>双精度浮点型</item>
    /// <item>字符串</item>
    /// <item>字符</item>
    /// <item>枚举类型</item>
    /// <item>System.Type</item>
    /// </list>
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public static bool IsValidAttributeParameterType(this ITypeSymbol symbol)
    {
        if (symbol is IArrayTypeSymbol { ElementType: not IArrayTypeSymbol and var arrayType })
        {
            return IsValidAttributeParameterType(arrayType);
        }

        switch (symbol.SpecialType)
        {
            case SpecialType.System_Boolean:
            case SpecialType.System_Byte:
            case SpecialType.System_SByte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_String:
            case SpecialType.System_Char:
                return true;
        }

        if (symbol.TypeKind == TypeKind.Enum)
            return true;

        if (symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.Type")
            return true;

        return false;
    }
    /// <summary>
    /// 检查是否是 System.Type 或 System.Type 的数组
    /// </summary> 
    /// <param name="symbol"></param>
    /// <returns></returns>
    public static bool IsTypeNamedType(this ITypeSymbol symbol)
    {  
        if (symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.Type")
        {
            return true;
        }
        else if ((ITypeSymbol?)symbol is IArrayTypeSymbol { ElementType: not IArrayTypeSymbol and var eleType })
        {
            return IsTypeNamedType(eleType);
        }
        return false;
    }
}
