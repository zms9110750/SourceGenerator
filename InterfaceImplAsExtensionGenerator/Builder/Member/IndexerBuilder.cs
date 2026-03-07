using System.CodeDom.Compiler;
using zms9110750.InterfaceImplAsExtensionGenerator.Builder.Helper;

namespace zms9110750.InterfaceImplAsExtensionGenerator.Builder.Member;

class IndexerBuilder : PropertyOrIndexerBuilder
{

    public IndexerBuilder(IPropertySymbol symbol, ExtensionConfig config, IndentedTextWriter writer, Action<Diagnostic> reportDiagnostic) : base(symbol, config, writer, reportDiagnostic)
    {
        if (!symbol.IsIndexer)
        {
            throw new InvalidOperationException("Invalid symbol type for IndexerBuilder.");
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

        Writer.Write(" this[");
        for (int i = 0; i < Symbol.Parameters.Length; i++)
        {
            var parameter = Symbol.Parameters[i];
            if (i > 0)
            {
                Writer.Write(", ");
            }
            Writer.Write(parameter.RefKind switch
            {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.In => "in ",
                _ => null
            });
            Writer.Write(TypeBuild.FullyQualifiedFormat(parameter.Type));
            Writer.Write(" " + parameter.Name.EscapeKeywords());
        }
        Writer.WriteLine("]");
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
            Writer.Write(Config.InstanceName);
            Writer.Write("[");
            Writer.Write(string.Join(", ", Symbol.Parameters.Select(p => p.Name)));
            Writer.WriteLine("];");

        }
        if (HasSet)
        {
            if (SetInternal)
            {
                Writer.Write("internal ");
            }
            Writer.Write("set => ");
            if (Symbol.ReturnsByRef || Symbol.ReturnsByRefReadonly)
            {
                Writer.Write("ref ");
            }
            Writer.Write(Config.InstanceName);
            Writer.Write("[");
            Writer.Write(string.Join(", ", Symbol.Parameters.Select(p => p.Name)));
            Writer.WriteLine("] = value;");
        }
        Writer.AppendCloseBracket();
    }

}
