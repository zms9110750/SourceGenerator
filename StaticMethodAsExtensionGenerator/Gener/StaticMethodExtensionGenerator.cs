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

                // 按命名空间分组，每个命名空间生成一个文件
                var nsGroups = new Dictionary<string, List<(INamedTypeSymbol type, IMethodSymbol[] methods)>>();
                foreach (var type in types)
                {
                    var methods = type.GetMembers()
                        .OfType<IMethodSymbol>()
                        .Where(m => IsWantedMethod(m, type))
                        .ToArray();
                    if (methods.Length == 0) continue;

                    var ns = type.ContainingNamespace.ToDisplayString();
                    var key = ns.Length > 0 ? ns : "Global";
                    if (!nsGroups.TryGetValue(key, out var list))
                        nsGroups[key] = list = [];
                    list.Add((type, methods));
                }

                foreach (var kv in nsGroups)
                    spc.AddSource($"{kv.Key}.g.cs", BuildNamespaceSource(kv.Key, kv.Value.ToArray()));
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

        var runtimeAsm = core; // System.Runtime 作为 BCL 基准

        var results = new List<INamedTypeSymbol>();
        // 按生成的扩展类全名（命名空间.类名）去重
        var visited = new HashSet<string>();

        foreach (var asm in comp.SourceModule.ReferencedAssemblySymbols)
        {
            if (!ShouldScanAssembly(asm, scope, runtimeAsm)) continue;
            foreach (var root in asm.GlobalNamespace.GetMembers())
            {
                if (root is INamespaceSymbol ns && NamespaceMatches(ns, asm, scope, runtimeAsm))
                    CollectAll(ns, asm, results, visited, scope, runtimeAsm);
            }
        }

        return results;
    }

    private static bool ShouldScanAssembly(IAssemblySymbol asm, StaticMethodExtensionScope scope, IAssemblySymbol runtimeAsm)
    {
        var name = asm.Name;
        // 总是跳过工具程序集
        if (name.StartsWith("Microsoft.CodeAnalysis") || name == "Microsoft.CSharp")
            return false;
        // BCL 程序集
        if (IsBclAssembly(asm, runtimeAsm))
            return scope.HasFlag(StaticMethodExtensionScope.BCL) || scope.HasFlag(StaticMethodExtensionScope.System) || scope.HasFlag(StaticMethodExtensionScope.SystemAll);
        // Microsoft NuGet（名字以 Microsoft 开头但不是 BCL）
        if (name.StartsWith("Microsoft."))
            return scope.HasFlag(StaticMethodExtensionScope.Microsoft);
        // 其他 NuGet
        return scope.HasFlag(StaticMethodExtensionScope.NuGet);
    }

    private static bool IsBclAssembly(IAssemblySymbol asm, IAssemblySymbol? runtimeAsm)
    {
        // 用公钥令牌判断是否 Microsoft 签名的程序集
        if (runtimeAsm == null) return false;
        var key = asm.Identity.PublicKeyToken;
        var runtimeKey = runtimeAsm.Identity.PublicKeyToken;
        if (key == null || runtimeKey == null) return false;
        if (!key.SequenceEqual(runtimeKey)) return false;

        // 版本号与 System.Runtime 大致对齐的，是 BCL 程序集
        // 版本差距大的（如 Microsoft.Extensions.* 版本号不同）是 NuGet
        var v = asm.Identity.Version;
        var rv = runtimeAsm.Identity.Version;
        return v.Major == rv.Major && v.Minor == rv.Minor;
    }

    private static bool NamespaceMatches(INamespaceSymbol ns, IAssemblySymbol asm, StaticMethodExtensionScope scope, IAssemblySymbol runtimeAsm)
    {
        var n = ns.ToDisplayString();
        if (scope.HasFlag(StaticMethodExtensionScope.System) && n == "System") return true;
        if (scope.HasFlag(StaticMethodExtensionScope.SystemAll) && (n == "System" || n.StartsWith("System."))) return true;
        if (scope.HasFlag(StaticMethodExtensionScope.BCL) && IsBclAssembly(asm, runtimeAsm) && n != "System" && !n.StartsWith("System.")) return true;
        if (scope.HasFlag(StaticMethodExtensionScope.Microsoft) && !IsBclAssembly(asm, runtimeAsm) && n.StartsWith("Microsoft.")) return true;
        if (scope.HasFlag(StaticMethodExtensionScope.NuGet) && !IsBclAssembly(asm, runtimeAsm) && !n.StartsWith("Microsoft.")) return true;
        return false;
    }

    private static void CollectAll(INamespaceSymbol ns, IAssemblySymbol asm, List<INamedTypeSymbol> results, HashSet<string> visited, StaticMethodExtensionScope scope, IAssemblySymbol runtimeAsm)
    {
        foreach (var m in ns.GetMembers())
        {
            if (m is INamespaceSymbol child && NamespaceMatches(child, asm, scope, runtimeAsm))
                CollectAll(child, asm, results, visited, scope, runtimeAsm);
            else if (m is INamedTypeSymbol t && IsWantedType(t))
            {
                // 用生成的扩展类全名去重，而不是类型标识符
                // 同一命名空间下同名类（来自不同程序集）只会生成一份
                var clsName = $"{t.ContainingNamespace.ToDisplayString()}.Extensions.{t.Name}Extensions";
                if (visited.Add(clsName)) results.Add(t);
            }
        }
    }

    private static bool IsWantedType(INamedTypeSymbol t) => t is
    {
        IsGenericType: false,
        IsStatic: false,
        ContainingType: null,                              // 跳过嵌套类
        DeclaredAccessibility: Accessibility.Public,
        TypeKind: TypeKind.Class or TypeKind.Struct,
        SpecialType: not SpecialType.System_Object,
    };

    private static string BuildNamespaceSource(string nsName, (INamedTypeSymbol type, IMethodSymbol[] methods)[] groups)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine();

        var genNs = nsName.Length > 0 ? $"zms9110750.Extensions.Generator.{nsName}" : "zms9110750.Extensions.Generator";
        sb.AppendLine($"namespace {genNs};");
        sb.AppendLine();

        foreach (var (type, methods) in groups)
        {
            sb.AppendLine($"internal static class {type.Name}Extensions");
            sb.AppendLine("{");
            foreach (var m in methods) WriteMethod(sb, m, type);
            sb.AppendLine("}");
            sb.AppendLine();
        }

        return sb.ToString();
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
        sb.Append($"        {(m.ReturnsVoid ? "" : "return ")}{Fmt(owner)}.{m.Name}(");
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
        var t = Fmt(m.ContainingType);
        return $"{t}.{m.Name}({string.Join(", ", m.Parameters.Select(p => Fmt(p.Type)))})";
    }
}
