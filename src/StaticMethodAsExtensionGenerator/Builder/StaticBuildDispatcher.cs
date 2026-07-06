using System.CodeDom.Compiler;
using System.Text;

namespace zms9110750.StaticMethodAsExtensionGenerator.Builder;

internal class StaticBuildDispatcher(Compilation compilation)
{
    private StaticMethodExtensionsAttribute Attribute { get; } = 
        compilation.Assembly.GetAttributes()
            .Select(StaticMethodExtensionsAttribute.Create)
            .FirstOrDefault(att => att != null)
            ?? StaticMethodExtensionsAttribute.Default;

    public StaticMethodExtensionScope Scope => Attribute.Scope;

    public bool Public => Attribute.Public;

    private IAssemblySymbol? RuntimeAssembly { get; } =
        compilation.GetTypeByMetadataName("System.Object")?.ContainingAssembly;

    public static void GenerateSource(SourceProductionContext context, Compilation compilation)
    {
        try
        {
            new StaticBuildDispatcher(compilation).GenerateSource(context);
        }
        catch (Exception ex)
        {
            context.AddSource("_error.g.cs", "/*" + ex.ToString() + "*/");
        }
    }

    public void GenerateSource(SourceProductionContext context)
    {
        if (RuntimeAssembly == null)
        {
            return;
        }

        var nsMap = new Dictionary<string, List<TypeExtensionInfo>>();
        var visited = new HashSet<string>();

        foreach (var asm in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            if (asm == null) continue;
            try
            {
                var scanner = new AssemblyScanner(asm, RuntimeAssembly);
                if (!scanner.ShouldScan(Scope))
                {
                    continue;
                }

                foreach (var root in asm.GlobalNamespace.GetMembers())
                {
                    if (root is INamespaceSymbol ns && scanner.NamespaceMatches(ns, Scope))
                    {
                        scanner.ScanNamespace(ns, Scope, nsMap, visited);
                    }
                }
            }
            catch
            {
                // Skip assemblies that can't be scanned
            }
        }

        foreach (var group in nsMap)
        {
            using var buffer = new StringWriter(new StringBuilder());
            using var writer = new IndentedTextWriter(buffer);

            var builder = new StaticNamespaceBuilder(writer, Public);
            builder.GenerateAll(group.Key, group.Value);

            context.AddSource(group.Key + ".g.cs", buffer.ToString());
        }
    }
}
