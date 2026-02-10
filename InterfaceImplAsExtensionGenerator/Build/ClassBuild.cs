using System.Collections;
using System.Drawing;

namespace zms9110750.InterfaceImplAsExtensionGenerator.Build;

class ClassBuild : IEnumerable<Diagnostic>
{
    public InterfaceBuild[] InterfaceBuilds { get; }
    public ClassBuild(INamedTypeSymbol classSymbol)
    {
        InterfaceBuilds = classSymbol.GetAttributes().Select(ExtensionForAttribute.Creat).OfType<ExtensionForAttribute>()
                .Select(t => new InterfaceBuild(t, classSymbol)).ToArray();
    }

    public IEnumerator<Diagnostic> GetEnumerator()
    {
        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}