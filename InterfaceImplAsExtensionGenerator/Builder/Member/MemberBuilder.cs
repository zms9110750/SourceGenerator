using System.CodeDom.Compiler;
using zms9110750.InterfaceImplAsExtensionGenerator.Builder.Helper;

namespace zms9110750.InterfaceImplAsExtensionGenerator.Builder.Member;

abstract class MemberBuilder
{
    protected static SymbolDisplayFormat? MemberFormat { get; } = new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                memberOptions: SymbolDisplayMemberOptions.IncludeContainingType |
                               SymbolDisplayMemberOptions.IncludeParameters,
                parameterOptions: SymbolDisplayParameterOptions.IncludeType |
                                  SymbolDisplayParameterOptions.IncludeModifiers,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
                            SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);
    public TypeParameterBuild TypeBuild { get; protected set; }
    public virtual bool IsValid { get; protected set; }
    public ISymbol Symbol { get; }
    public ExtensionConfig Config { get; }
    public ExtensionMemberConfig MemberConfig { get; }
    public IndentedTextWriter Writer { get; }
    public Action<Diagnostic> ReportDiagnostic { get; }
    public MemberBuilder(ISymbol symbol, ExtensionConfig config, IndentedTextWriter writer, Action<Diagnostic> reportDiagnostic)
    {
        TypeBuild = new TypeParameterBuild(config.TypeSymbol);
        Symbol = symbol;
        Config = config;
        Writer = writer;
        ReportDiagnostic = reportDiagnostic;
        MemberConfig = config.WithSymbol(symbol);
        if (symbol.IsStatic || symbol.IsImplicitlyDeclared)
        {
            return;
        }
        if (!ValidAccessibility(symbol.DeclaredAccessibility))
        {
            return;
        }
        if (!MemberConfig.Include)
        {
            return;
        }
        IsValid = true;
    }
    public static bool ValidAccessibility(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Internal => true,
            Accessibility.ProtectedOrInternal => true,
            Accessibility.Public => true,
            _ => false
        };
    }
    public static MemberBuilder Creat(InterfaceBuilder interfaceBuilder, ISymbol symbol)
    {
        return symbol switch
        {
            IEventSymbol eventSymbol => new EventBuilder(eventSymbol, interfaceBuilder.Config, interfaceBuilder.Writer, interfaceBuilder.ReportDiagnostic),
            IPropertySymbol { IsIndexer: true } indexer => new IndexerBuilder(indexer, interfaceBuilder.Config, interfaceBuilder.Writer, interfaceBuilder.ReportDiagnostic),
            IPropertySymbol property => new PropertyBuilder(property, interfaceBuilder.Config, interfaceBuilder.Writer, interfaceBuilder.ReportDiagnostic),
            IMethodSymbol { MethodKind: MethodKind.Ordinary } method => new MethodBuilder(method, interfaceBuilder.Config, interfaceBuilder.Writer, interfaceBuilder.ReportDiagnostic),
            _ => new ErrorBuilder(symbol, interfaceBuilder.Config, interfaceBuilder.Writer, interfaceBuilder.ReportDiagnostic)
        };
    }

    /// <summary>
    /// 生成文档注释
    /// </summary> 
    public void WriteInheritdoc()
    {
        var symbol = Symbol;
        if (symbol is IMethodSymbol { AssociatedSymbol: { } asymbol })
        {
            symbol = asymbol;
        }

        Writer.WriteLine($"/// <inheritdoc cref=\"{symbol.ToDisplayString(MemberFormat).Replace("<", "{").Replace(">", "}").Replace("params ", "")}\" />");
    }
    /// <summary>
    /// 生成扩展块
    /// </summary>
    public abstract void GenerateMemberExtensionBlock();
    /// <summary>
    /// 生成扩展方法
    /// </summary>
    public abstract void GenerateMemberExtensionMethod();

}
class ErrorBuilder : MemberBuilder
{

    public ErrorBuilder(ISymbol symbol, ExtensionConfig config, IndentedTextWriter writer, Action<Diagnostic> reportDiagnostic) : base(symbol, config, writer, reportDiagnostic)
    {
        IsValid = false;
    }

    public override void GenerateMemberExtensionBlock()
    {
        Writer.WriteLine($"// Unsupported member name:{Symbol.Name} type: {Symbol.GetType().FullName}");
    }

    public override void GenerateMemberExtensionMethod()
    {
        Writer.WriteLine($"// Unsupported member name:{Symbol.Name} type: {Symbol.GetType().FullName}");

    }
}
