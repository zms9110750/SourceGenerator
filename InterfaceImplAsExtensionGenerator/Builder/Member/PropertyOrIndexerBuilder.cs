using System.CodeDom.Compiler;
using zms9110750.InterfaceImplAsExtensionGenerator.Builder.Helper;

namespace zms9110750.InterfaceImplAsExtensionGenerator.Builder.Member;

abstract class PropertyOrIndexerBuilder : MemberBuilder
{
    public new IPropertySymbol Symbol { get; }
    protected bool HasGet { get; }
    protected bool HasSet { get; }
    protected bool GetInternal { get; }
    protected bool SetInternal { get; }
    public PropertyOrIndexerBuilder(IPropertySymbol symbol, ExtensionConfig config, IndentedTextWriter writer, Action<Diagnostic> reportDiagnostic) : base(symbol, config, writer, reportDiagnostic)
    {
        Symbol = symbol;
        if (symbol.GetMethod != null && ValidAccessibility(symbol.GetMethod.DeclaredAccessibility))
        {
            HasGet = true;
        }
        if (symbol.SetMethod != null && ValidAccessibility(symbol.SetMethod.DeclaredAccessibility) && !symbol.SetMethod.IsInitOnly)
        {
            HasSet = true;
        }
        if (!HasGet && !HasSet)
        {
            IsValid = false;
        }
        if (MemberConfig.IsPublic)
        {
            if (symbol.GetMethod is { DeclaredAccessibility: not Accessibility.Public })
            {
                GetInternal = true;
            }
            if (symbol.SetMethod is { DeclaredAccessibility: not Accessibility.Public })
            {
                SetInternal = true;
            }
        }
    }
    public override void GenerateMemberExtensionMethod()
    {
        if (HasGet)
        {
            new MethodBuilder(Symbol.GetMethod!, Config, Writer, ReportDiagnostic).GenerateMemberExtensionMethod();
        }
        if (HasSet)
        {
            new MethodBuilder(Symbol.SetMethod!, Config, Writer, ReportDiagnostic).GenerateMemberExtensionMethod();
        }
    }
}
