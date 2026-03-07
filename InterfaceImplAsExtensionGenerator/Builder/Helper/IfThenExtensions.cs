using System.Runtime.CompilerServices;

namespace zms9110750.InterfaceImplAsExtensionGenerator.Builder.Helper;

internal static class IfThenExtensions
{
    public static string EscapeKeywords(this string identifier)
    {
        return identifier.IfThen(SyntaxFacts.GetKeywordKind(identifier) != SyntaxKind.None, "@" + identifier);
    }

    // 1. 条件选择：根据布尔值选择返回原始值或指定值
    public static T IfDefaultThen<T>(this T value, T alternativeValue)
    {
        return EqualityComparer<T>.Default.Equals(value, default!) ? alternativeValue : value;
    }

    /* // 1. 条件选择：根据布尔值选择返回原始值或指定值
     public static TB IfThen<T, TB>(this T value, bool condition, TB alternativeValue)
         where T : TB
     {
         return condition ? alternativeValue : value;
     }*/

    // 2. 条件选择：根据谓词函数选择返回原始值或指定值
    public static TB IfThen<T, TB>(this T value, Func<T, bool> predicate, TB alternativeValue)
        where T : TB
    {
        return predicate(value) ? alternativeValue : value;
    }

    // 3. 条件选择：根据谓词函数选择返回原始值或计算值
    public static TB IfThen<T, TB>(this T value, Func<T, bool> predicate, Func<T, TB> valueFactory)
        where T : TB
    {
        return predicate(value) ? valueFactory(value) : value;
    }

    // 4. 条件选择：根据布尔值选择返回原始值或计算值
    public static TB IfThen<T, TB>(this T value, bool condition, Func<T, TB> valueFactory)
        where T : TB
    {
        return condition ? valueFactory(value) : value;
    }

    // 1. 条件选择：根据布尔值选择返回原始值或指定值
    public static T IfThen<T>(this T value, bool condition, T alternativeValue)
    {
        return condition ? alternativeValue : value;
    }

    // 2. 条件选择：根据谓词函数选择返回原始值或指定值
    public static T IfThen<T>(this T value, Func<T, bool> predicate, T alternativeValue)
    {
        return predicate(value) ? alternativeValue : value;
    }
    /*
        // 3. 条件选择：根据谓词函数选择返回原始值或计算值
        public static T IfThen<T>(this T value, Func<T, bool> predicate, Func<T, T> valueFactory)
        {
            return predicate(value) ? valueFactory(value) : value;
        }

        // 4. 条件选择：根据布尔值选择返回原始值或计算值
        public static T IfThen<T>(this T value, bool condition, Func<T, T> valueFactory)
        {
            return condition ? valueFactory(value) : value;
        }*/
}
