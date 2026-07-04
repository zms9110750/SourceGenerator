using System.CodeDom.Compiler;
using zms9110750.InterfaceImplAsExtensionGenerator.Builder.Helper;

namespace zms9110750.InterfaceImplAsExtensionGenerator.Builder.Member;

class EventBuilder : MemberBuilder
{
    public new IEventSymbol Symbol { get; }
    public EventBuilder(IEventSymbol symbol, ExtensionConfig config, IndentedTextWriter writer, Action<Diagnostic> reportDiagnostic) : base(symbol, config, writer, reportDiagnostic)
    {
        Symbol = symbol;
    }

    public override void GenerateMemberExtensionBlock()
    {
        WriteInheritdoc(); 
        Writer.Write(MemberConfig.IsPublic ? "public " : "internal ");
        Writer.Write("event ");
        Writer.Write(Symbol.Type.ToDisplayString(MemberFormat));
        Writer.Write(" "); 
        Writer.WriteLine(MemberConfig.MemberName);
        Writer.AppendOpenBracket();
        Writer.WriteLine($"add => {Config.InstanceName}.{Symbol.Name.EscapeKeywords()} += value;");
        Writer.WriteLine($"remove => {Config.InstanceName}.{Symbol.Name.EscapeKeywords()} -= value;");
        Writer.AppendCloseBracket();
    }

    public override void GenerateMemberExtensionMethod()
    {
        new MethodBuilder(Symbol.AddMethod!, Config, Writer, ReportDiagnostic).GenerateMemberExtensionMethod();
        new MethodBuilder(Symbol.RemoveMethod!, Config, Writer, ReportDiagnostic).GenerateMemberExtensionMethod();
    }
}
