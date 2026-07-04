namespace zms9110750.StaticMethodAsExtensionGenerator.Builder;

class AssemblyScanner(IAssemblySymbol asm, IAssemblySymbol runtimeAsm)
{
    private bool? _isBcl;
    private bool IsBcl
    {
        get
        {
            if (_isBcl.HasValue)
            {
                return _isBcl.Value;
            }
            var key = asm.Identity.PublicKeyToken;
            var runtimeKey = runtimeAsm.Identity.PublicKeyToken;
            if (key.IsDefault || runtimeKey.IsDefault)
            {
                _isBcl = false;
                return false;
            }
            if (!key.SequenceEqual(runtimeKey))
            {
                return false;
            }
            var v = asm.Identity.Version;
            var rv = runtimeAsm.Identity.Version;
            _isBcl = v.Major == rv.Major && v.Minor == rv.Minor;
            return _isBcl.Value;
        }
    }

    public bool ShouldScan(StaticMethodExtensionScope scope)
    {
        var name = asm.Name;
        if (name.StartsWith("Microsoft.CodeAnalysis") || name == "Microsoft.CSharp")
        {
            return false;
        }
        if (IsBcl)
        {
            return scope.HasFlag(StaticMethodExtensionScope.BCL)
                || scope.HasFlag(StaticMethodExtensionScope.System)
                || scope.HasFlag(StaticMethodExtensionScope.SystemAll);
        }
        if (name.StartsWith("Microsoft."))
        {
            return scope.HasFlag(StaticMethodExtensionScope.Microsoft);
        }
        return scope.HasFlag(StaticMethodExtensionScope.NuGet);
    }

    public bool NamespaceMatches(INamespaceSymbol ns, StaticMethodExtensionScope scope)
    {
        var n = ns.ToDisplayString();
        if (scope.HasFlag(StaticMethodExtensionScope.System) && n == "System")
        {
            return true;
        }
        if (scope.HasFlag(StaticMethodExtensionScope.SystemAll) && (n == "System" || n.StartsWith("System.")))
        {
            return true;
        }
        if (scope.HasFlag(StaticMethodExtensionScope.BCL) && IsBcl && n != "System" && !n.StartsWith("System."))
        {
            return true;
        }
        if (scope.HasFlag(StaticMethodExtensionScope.Microsoft) && !IsBcl && n.StartsWith("Microsoft."))
        {
            return true;
        }
        if (scope.HasFlag(StaticMethodExtensionScope.NuGet) && !IsBcl && !n.StartsWith("Microsoft.") && !n.StartsWith("System."))
        {
            return true;
        }
        return false;
    }

    public void ScanNamespace(
        INamespaceSymbol ns,
        StaticMethodExtensionScope scope,
        Dictionary<string, List<TypeExtensionInfo>> nsMap,
        HashSet<string> visited)
    {
        foreach (var member in ns.GetMembers())
        {
            if (member is INamespaceSymbol child && NamespaceMatches(child, scope))
            {
                ScanNamespace(child, scope, nsMap, visited);
            }
            else if (member is INamedTypeSymbol type && type is
            {
                IsGenericType: false,
                IsStatic: false,
                ContainingType: null,
                DeclaredAccessibility: Accessibility.Public,
                TypeKind: TypeKind.Class or TypeKind.Struct,
                SpecialType: not SpecialType.System_Object,
            })
            {
                var clsName = $"{type.ContainingNamespace.ToDisplayString()}.Extensions.{type.Name}Extensions";
                if (!visited.Add(clsName))
                {
                    continue;
                }

                var methods = new List<IMethodSymbol>();
                foreach (var m in type.GetMembers().OfType<IMethodSymbol>())
                {
                    if (m is not { IsStatic: true, MethodKind: MethodKind.Ordinary, Parameters.Length: > 0 })
                    {
                        continue;
                    }
                    if (m.DeclaredAccessibility != Accessibility.Public)
                    {
                        continue;
                    }
                    if (m.IsGenericMethod)
                    {
                        continue;
                    }
                    if (m.GetAttributes().Any(a => a.AttributeClass?.Name == "ObsoleteAttribute"))
                    {
                        continue;
                    }
                    if (m.GetAttributes().Any(a =>
                        a.AttributeClass?.Name == "EditorBrowsableAttribute" &&
                        a.ConstructorArguments.Length > 0 &&
                        a.ConstructorArguments[0].Value is int state &&
                        state == 0))
                    {
                        continue;
                    }
                    if (m.Parameters.Any(p => p.RefKind != RefKind.None))
                    {
                        continue;
                    }
                    if (!SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, type))
                    {
                        continue;
                    }
                    {
                        var hasConflict = false;
                        foreach (var candidate in type.GetMembers(m.Name))
                        {
                            if (candidate is IMethodSymbol { IsStatic: false } inst &&
                                inst.Parameters.Length == m.Parameters.Length - 1)
                            {
                                var match = true;
                                for (int i = 1; i < m.Parameters.Length; i++)
                                {
                                    if (!SymbolEqualityComparer.Default.Equals(m.Parameters[i].Type, inst.Parameters[i - 1].Type))
                                    {
                                        match = false;
                                        break;
                                    }
                                }
                                if (match)
                                {
                                    hasConflict = true;
                                    break;
                                }
                            }
                        }
                        if (hasConflict)
                        {
                            continue;
                        }
                    }
                    methods.Add(m);
                }

                if (methods.Count == 0)
                {
                    continue;
                }

                var nsKey = type.ContainingNamespace.ToDisplayString();
                if (nsKey.Length == 0)
                {
                    nsKey = "Global";
                }
                if (!nsMap.TryGetValue(nsKey, out var list))
                {
                    nsMap[nsKey] = list = [];
                }
                list.Add(new TypeExtensionInfo(type, methods.ToArray()));
            }
        }
    }

}
