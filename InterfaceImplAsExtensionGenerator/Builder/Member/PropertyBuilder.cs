using System.CodeDom.Compiler;
using zms9110750.InterfaceImplAsExtensionGenerator.Builder.Helper;

namespace zms9110750.InterfaceImplAsExtensionGenerator.Builder.Member;

class PropertyBuilder : PropertyOrIndexerBuilder
{
    public PropertyBuilder(IPropertySymbol symbol, ExtensionConfig config, IndentedTextWriter writer, Action<Diagnostic> reportDiagnostic) : base(symbol, config, writer, reportDiagnostic)
    {
        if (symbol.IsIndexer)
        {
            throw new InvalidOperationException("Invalid symbol type for PropertyBuilder.");
        }
    }
    public override void GenerateMemberExtensionBlock()
    {
        WriteInheritdoc();
        Writer.Write(MemberConfig.IsPublic ? "public " : "internal ");
        Writer.Write(Symbol switch
        {
            { ReturnsByRef: true } => "ref ",
            { ReturnsByRefReadonly: true } => "ref readonly ",
            _ => null
        });
        Writer.Write(TypeBuild.FullyQualifiedFormat(Symbol.Type)); 
        Writer.Write(" ");
        Writer.WriteLine(MemberConfig.MemberName);

        Writer.AppendOpenBracket();
        if (HasGet)
        {
            if (GetInternal)
            {
                Writer.Write("internal ");
            }
            Writer.Write("get => ");
            if (Symbol.ReturnsByRef || Symbol.ReturnsByRefReadonly)
            {
                Writer.Write("ref ");
            }
            Writer.WriteLine($"{Config.InstanceName}.{Symbol.Name.EscapeKeywords()};");
        }
        if (HasSet)
        {
            if (SetInternal)
            {
                Writer.Write("internal ");
            }
            Writer.WriteLine($"set => {Config.InstanceName}.{Symbol.Name.EscapeKeywords()} = value;");
        }
        Writer.AppendCloseBracket();
    }
}