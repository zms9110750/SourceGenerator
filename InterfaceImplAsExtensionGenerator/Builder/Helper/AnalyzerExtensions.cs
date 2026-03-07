using System.Linq;

namespace zms9110750.InterfaceImplAsExtensionGenerator.Builder.Helper;

static class SourceGeneratorExtensions
{
    /// <summary>
    /// 获取类型的全局完全限定名
    /// </summary>
    public static string ToGlobalDisplayString(this ISymbol typeSymbol)
    {
        return typeSymbol == null
            ? string.Empty
            : typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
    public static TValue? GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue>? dictionary, TKey key, TValue? defaultValue = default)
    {
        return dictionary != null && dictionary.TryGetValue(key, out TValue? value) ? value : defaultValue;
    }
}