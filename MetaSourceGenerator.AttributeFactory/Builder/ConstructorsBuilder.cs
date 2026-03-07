using zms9110750.MetaSourceGenerator.AttributeFactory.Builder.Helper;
using zms9110750.MetaSourceGenerator.AttributeFactory.DiagnosticDefine;

namespace zms9110750.MetaSourceGenerator.AttributeFactory.Builder;

class ConstructorsBuilder(ClassBuilder classBuilder) : BaseBuilder(classBuilder.Writer, classBuilder.ReportDiagnostic)
{
    public IMethodSymbol[] Constructors { get; } =
           classBuilder.AttributeSymbol.Constructors
           .Where(IsValidMember)
           .Where(symbol => symbol.Parameters.All(s => s.Type.IsValidAttributeParameterType()))
           .ToArray();

    public void GenerateSource()
    {
        foreach (var symbol in Constructors)
        {
            Writer.WriteLine($"""case "{string.Join("|", symbol.Parameters.Select(p => p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))}":""");
            Writer.Indent++;

            if (!symbol.Parameters.Any(t => t.Type.IsTypeNamedType()) || ShouldGenerateMethod(symbol))
            {
                Writer.WriteLine("value = new " + classBuilder.AttributeFullName);
                Writer.Indent++;
                Writer.WriteLine("(");
                Writer.Indent++;
                int index = 0;
                foreach (var para in symbol.Parameters)
                {
                    if (index != 0)
                    {
                        Writer.WriteLine(",");
                    }
                    bool toArray = true;
                    if (para.Type is not IArrayTypeSymbol { ElementType: var paraType })
                    {
                        paraType = para.Type;
                        toArray = false;
                    }
                    var typeFullName = paraType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    if (typeFullName == "global::System.Type")
                    {
                        typeFullName = "global::Microsoft.CodeAnalysis.INamedTypeSymbol";
                    }
                    if (toArray)
                    {
                        Writer.Write($"System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(data.ConstructorArguments[{index}].Values, s => ({typeFullName})s.Value))");
                    }
                    else
                    {
                        Writer.Write($"({typeFullName})data.ConstructorArguments[{index}].Value");
                    }
                    index++;
                }
                Writer.WriteLine();
                Writer.Indent--;
                Writer.WriteLine(");");
                Writer.Indent--;
                Writer.WriteLine("break;");
            }
            else
            {
                Writer.WriteLine("return null;");
                var methodDeclaration = symbol.DeclaringSyntaxReferences[0].GetSyntax();
                var location = methodDeclaration! switch
                {
                    ConstructorDeclarationSyntax constructor => constructor.ParameterList.GetLocation(),
                    ClassDeclarationSyntax classDeclaration => classDeclaration.ParameterList!.GetLocation(),
                    _ => throw new System.InvalidOperationException("Unexpected syntax node type: " + methodDeclaration.GetType())
                };

                _ = location ?? throw new System.ArgumentNullException(nameof(location));
                ReportDiagnostic(Diagnostic.Create(FromAttributeDiagnostic.ZMS003, location));
            }
            Writer.Indent--;
        }
    }
    public bool ShouldGenerateMethod(IMethodSymbol methodSymbol)
    {
        return methodSymbol.ContainingType.Constructors.Any(constructor => AreParametersCompatible(methodSymbol, constructor));

        bool AreParametersCompatible(IMethodSymbol source, IMethodSymbol target)
        {
            if (!IsValidMember(target) || source.Parameters.Length != target.Parameters.Length)
            {
                return false;
            }
            for (int i = 0; i < source.Parameters.Length; i++)
            {
                var sourceType = source.Parameters[i].Type.ToDisplayString((SymbolDisplayFormat?)SymbolDisplayFormat.FullyQualifiedFormat);
                var targetType = target.Parameters[i].Type.ToDisplayString((SymbolDisplayFormat?)SymbolDisplayFormat.FullyQualifiedFormat);

                switch (sourceType)
                {
                    case "global::System.Type" when targetType != "global::Microsoft.CodeAnalysis.INamedTypeSymbol":
                    case "global::System.Type[]" when targetType != "global::Microsoft.CodeAnalysis.INamedTypeSymbol[]":
                        return false;
                    case "global::System.Type":
                    case "global::System.Type[]":
                        continue;
                    case var _ when sourceType != targetType:
                        return false;
                }
            }

            return true;
        }
    }
    public static bool IsValidMember(IMethodSymbol symbol)
    {
        if (symbol.IsStatic || symbol.IsAbstract)
        {
            return false;
        }
        switch (symbol.DeclaredAccessibility)
        {
            case Accessibility.Public:
            case Accessibility.Internal:
            case Accessibility.ProtectedOrInternal:
                break;
            default:
                return false;
        }
        return true;
    }
}
