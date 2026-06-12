namespace zms9110750.StaticMethodAsExtensionGenerator.Builder;

class TypeExtensionInfo(INamedTypeSymbol type, IMethodSymbol[] methods)
{
    public INamedTypeSymbol Type { get; } = type;
    public IMethodSymbol[] Methods { get; } = methods;
}
