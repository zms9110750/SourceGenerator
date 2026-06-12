using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;

namespace zms9110750.StaticMethodAsExtensionGenerator.Gener;

[Generator]
internal sealed class StaticMethodExtensionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider, static (spc, comp) =>
        {
            try
            {
                var scope = ReadScope(comp);
                var types = CollectTargetTypes(comp, scope);
                foreach (var type in types)
                    GenerateSource(spc, type);
            }
            catch (Exception ex)
            {
                spc.AddSource("_error.g.cs", $"/* StaticMethodExtensionGenerator error: {ex} */");
            }
        });
    }

    private static StaticMethodExtensionScope ReadScope(Compilation comp)
    {
        foreach (var attr in comp.Assembly.GetAttributes())
        {
            if (attr.AttributeClass?.Name == "StaticMethodExtensionsAttribute" &&
                attr.ConstructorArguments.Length == 1 &&
                attr.ConstructorArguments[0].Value is int val)
            {
                return (StaticMethodExtensionScope)val;
            }
        }
        return StaticMethodExtensionScope.SystemAll; // 默认
    }

    private static List<INamedTypeSymbol> CollectTargetTypes(Compilation comp, StaticMethodExtensionScope scope)
    {
        var core = comp.GetTypeByMetadataName("System.Object")?.ContainingAssembly;
        if (core == null) return [];

        var results = new List<INamedTypeSymbol>();
        var visited = new HashSet<string>();

        foreach (var asm in comp.SourceModule.ReferencedAssemblySymbols)
        {
            if (!ShouldScanAssembly(asm, scope)) continue;
            foreach (var root in asm.GlobalNamespace.GetMembers())
            {
                if (root is INamespaceSymbol ns && NamespaceMatches(ns, asm, scope))
                    CollectAll(ns, asm, results, visited, scope);
            }
        }

        return results;
    }

    private static bool ShouldScanAssembly(IAssemblySymbol asm, StaticMethodExtensionScope scope)
    {
        var name = asm.Name;
        // 总是跳过工具程序集
        if (name.StartsWith("Microsoft.CodeAnalysis") || name == "Microsoft.CSharp")
            return false;
        // BCL 程序集
        if (IsBclAssembly(name))
            return scope.HasFlag(StaticMethodExtensionScope.BCL) || scope.HasFlag(StaticMethodExtensionScope.System) || scope.HasFlag(StaticMethodExtensionScope.SystemAll);
        // Microsoft NuGet
        if (name.StartsWith("Microsoft."))
            return scope.HasFlag(StaticMethodExtensionScope.Microsoft);
        // 其他 NuGet
        return scope.HasFlag(StaticMethodExtensionScope.NuGet);
    }

    private static bool IsBclAssembly(string name)
    {
        return name.StartsWith("System.")
            || name is "System" or "mscorlib" or "netstandard"
            or "System.Private.CoreLib" or "System.Runtime"
            or "Microsoft.Win32.Primitives" or "Microsoft.Win32.Registry"
            or "Microsoft.VisualBasic" or "Microsoft.VisualBasic.Core";
    }

    private static bool NamespaceMatches(INamespaceSymbol ns, IAssemblySymbol asm, StaticMethodExtensionScope scope)
    {
        var n = ns.ToDisplayString();
        if (scope.HasFlag(StaticMethodExtensionScope.System) && n == "System") return true;
        if (scope.HasFlag(StaticMethodExtensionScope.SystemAll) && (n == "System" || n.StartsWith("System."))) return true;
        if (scope.HasFlag(StaticMethodExtensionScope.BCL) && IsBclAssembly(asm.Name) && n != "System" && !n.StartsWith("System.")) return true;
        if (scope.HasFlag(StaticMethodExtensionScope.Microsoft) && !IsBclAssembly(asm.Name) && n.StartsWith("Microsoft.")) return true;
        if (scope.HasFlag(StaticMethodExtensionScope.NuGet) && !IsBclAssembly(asm.Name) && !n.StartsWith("Microsoft.")) return true;
        return false;
    }

    private static void CollectAll(INamespaceSymbol ns, IAssemblySymbol asm, List<INamedTypeSymbol> results, HashSet<string> visited, StaticMethodExtensionScope scope)
    {
        foreach (var m in ns.GetMembers())
        {
            if (m is INamespaceSymbol child && NamespaceMatches(child, asm, scope))
                CollectAll(child, asm, results, visited, scope);
            else if (m is INamedTypeSymbol t && IsWantedType(t))
            {
                var k = t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                if (visited.Add(k)) results.Add(t);
            }
        }
    }

    private static bool IsWantedType(INamedTypeSymbol t)
    {
        if (t.IsGenericType) return false;
        if (t.TypeKind is not TypeKind.Class and not TypeKind.Struct) return false;
        if (t.DeclaredAccessibility != Accessibility.Public) return false;
        if (t.IsStatic) return false;
        if (t.SpecialType == SpecialType.System_Object) return false;
        return true;
    }

    private static void GenerateSource(SourceProductionContext spc, INamedTypeSymbol type)
    {
        var methods = type.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => IsWantedMethod(m, type))
            .ToArray();

        if (methods.Length == 0) return;

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine();

        var ns = type.ContainingNamespace.ToDisplayString();
        sb.AppendLine($"namespace {ns}.Extensions;");
        sb.AppendLine();
        sb.AppendLine($"internal static class {type.Name}Extensions");
        sb.AppendLine("{");

        foreach (var m in methods) WriteMethod(sb, m, type);

        sb.AppendLine("}");
        spc.AddSource($"_{type.Name}Extensions.g.cs", sb.ToString());
    }

    private static bool IsWantedMethod(IMethodSymbol m, INamedTypeSymbol owner)
    {
        if (m is not { IsStatic: true, MethodKind: MethodKind.Ordinary, Parameters.Length: > 0 })
            return false;
        if (m.DeclaredAccessibility != Accessibility.Public) return false;
        if (m.IsGenericMethod) return false;
        // 跳过 [Obsolete]
        if (m.GetAttributes().Any(a => a.AttributeClass?.Name == "ObsoleteAttribute"))
            return false;

        // 只跳过 [EditorBrowsable(Never)]，保留 Always/Advanced
        if (m.GetAttributes().Any(a =>
            a.AttributeClass?.Name == "EditorBrowsableAttribute" &&
            a.ConstructorArguments.Length > 0 &&
            a.ConstructorArguments[0].Value is int state &&
            state == 0))  // 0 = EditorBrowsableState.Never (枚举值)
            return false;
        if (m.Parameters.Any(p => p.RefKind != RefKind.None)) return false;
        if (!SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, owner)) return false;
        if (HasInstanceConflict(m, owner)) return false;
        return true;
    }

    private static bool HasInstanceConflict(IMethodSymbol m, INamedTypeSymbol t)
    {
        foreach (var member in t.GetMembers(m.Name))
        {
            if (member is IMethodSymbol { IsStatic: false } inst &&
                inst.Parameters.Length == m.Parameters.Length - 1)
            {
                bool match = true;
                for (int i = 1; i < m.Parameters.Length; i++)
                    if (!SymbolEqualityComparer.Default.Equals(m.Parameters[i].Type, inst.Parameters[i - 1].Type))
                    { match = false; break; }
                if (match) return true;
            }
        }
        return false;
    }

    private static void WriteMethod(StringBuilder sb, IMethodSymbol m, INamedTypeSymbol owner)
    {
        var ps = m.Parameters;
        sb.AppendLine($"    /// <inheritdoc cref=\"{Cref(m)}\"/>");
        sb.Append($"    internal static {Fmt(m.ReturnType)} {m.Name}(this {Fmt(ps[0].Type)} {Esc(ps[0].Name)}");
        for (int i = 1; i < ps.Length; i++)
        {
            sb.Append(", ");
            var mod = ps[i].RefKind switch
            {
                RefKind.Ref => "ref ", RefKind.Out => "out ", RefKind.In => "in ",
                _ => ps[i].IsParams ? "params " : ""
            };
            sb.Append($"{mod}{Fmt(ps[i].Type)} {Esc(ps[i].Name)}");
        }
        sb.AppendLine(")");
        sb.AppendLine("    {");
        sb.Append($"        {(m.ReturnsVoid ? "" : "return ")}{FmtShort(owner)}.{m.Name}(");
        for (int i = 0; i < ps.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            var mod = ps[i].RefKind switch
            {
                RefKind.Ref => "ref ", RefKind.Out => "out ", RefKind.In => "in ",
                _ => ""
            };
            sb.Append($"{mod}{Esc(ps[i].Name)}");
        }
        sb.AppendLine(");");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static string Esc(string name) => SyntaxFacts.IsKeywordKind(SyntaxFacts.GetKeywordKind(name)) ? "@" + name : name;

    private static string Fmt(ITypeSymbol t) => t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    private static string FmtShort(INamedTypeSymbol t) => t.ToDisplayString();
    private static string Cref(IMethodSymbol m)
    {
        var t = m.ContainingType.ToDisplayString();
        return $"{t}.{m.Name}({string.Join(", ", m.Parameters.Select(p => p.Type.ToDisplayString()))})";
    }
}
