using zms9110750.MetaSourceGenerator.AttributeFactory.Builder.Helper;

namespace zms9110750.MetaSourceGenerator.AttributeFactory.Builder;

class PropertiesBuilder(ClassBuilder classBuilder) : BaseBuilder(classBuilder.Writer, classBuilder.ReportDiagnostic)
{
    IPropertySymbol[] Properties { get; } =
      classBuilder.AttributeSymbol.GetMembers()
      .OfType<IPropertySymbol>()
      .Where(IsValidMember)
      .ToArray();

    public void GenerateSource()
    {
        foreach (var symbol in Properties)
        {
            Writer.WriteLine($"""case "{symbol.Name}":""");
            Writer.Indent++;
            Writer.Write($"value.{symbol.Name}");
            bool toArray = true;
            if (symbol.Type is not IArrayTypeSymbol { ElementType: var paraType })
            {
                paraType = symbol.Type;
                toArray = false;
            }
            var typeFullName = paraType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (typeFullName == "global::System.Type")
            {
                typeFullName = "global::Microsoft.CodeAnalysis.ITypeSymbol";
                Writer.Write($"Symbol");
            }
            Writer.Write($" = ");
            if (toArray)
            {
                Writer.WriteLine($"System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(symbol.Value.Values, s => ({typeFullName})s.Value));");
            }
            else
            {
                Writer.WriteLine($"({typeFullName})symbol.Value.Value;");
            }

            Writer.WriteLine("break;");
            Writer.Indent--;
        }
    }
    internal void GenerateSourceProperties()
    {
        foreach (var item in Properties)
        {
            if (item.Type.IsTypeNamedType())
            {
                Writer.WriteLine("/// <summary>");
                Writer.WriteLine($"""/// 自动生成。为<see cref="{item.Name}"/>在<see cref="AttributeData"/>中的<see cref="ITypeSymbol"/>表现形式""");
                Writer.WriteLine("/// </summary>");


                Writer.Write("internal global::Microsoft.CodeAnalysis.ITypeSymbol");
                if (item.Type is IArrayTypeSymbol)
                {
                    Writer.Write("[]");
                }
                Writer.Write($" {item.Name}Symbol");
                Writer.WriteLine("{ get; set; }");
                Writer.WriteLine();
            }
        }
    }

    public static bool IsValidMember(IPropertySymbol symbol)
    {
        if (symbol.IsStatic || symbol.IsIndexer)
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