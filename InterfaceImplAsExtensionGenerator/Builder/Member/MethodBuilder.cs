using System.CodeDom.Compiler;
using System.Globalization;
using System.Runtime.InteropServices.ComTypes;
using zms9110750.InterfaceImplAsExtensionGenerator.Builder.Helper;
using zms9110750.InterfaceImplAsExtensionGenerator.DiagnosticDefine;

namespace zms9110750.InterfaceImplAsExtensionGenerator.Builder.Member;

class MethodBuilder : MemberBuilder
{
    public new IMethodSymbol Symbol { get; }

    public MethodBuilder(IMethodSymbol symbol, ExtensionConfig config, IndentedTextWriter writer, Action<Diagnostic> reportDiagnostic) : base(symbol, config, writer, reportDiagnostic)
    {
        Symbol = symbol;
        TypeBuild = new TypeParameterBuild(config.TypeSymbol, symbol);
        if (symbol.MethodKind != MethodKind.Ordinary)
        {
            IsValid = false;
        }
    }
    public override void GenerateMemberExtensionBlock()
    {
        WriteInheritdoc();
        GenerateMemberExtensionMethod(false);
    }

    public override void GenerateMemberExtensionMethod()
    {
        WriteInheritdoc();
        GenerateMemberExtensionMethod(true);
    }
    void GenerateMemberExtensionMethod(bool extensionMethod = true)
    {
        Writer.Write(MemberConfig.IsPublic ? "public " : "internal ");
        if (extensionMethod)
        {
            Writer.Write("static ");
        }
        Writer.Write(Symbol switch
        {
            { ReturnsByRef: true } => "ref ",
            { ReturnsByRefReadonly: true } => "ref readonly ",
            _ => null
        });
        Writer.Write(Symbol.ReturnType.ToGlobalDisplayString());
        Writer.Write(" ");
        Writer.Write(MemberConfig.MemberName);
        if (extensionMethod)
        {
            if (TypeBuild.GetTypeParametersName().Any())
            {
                Writer.Write("<");
                Writer.Write(string.Join(", ", TypeBuild.GetTypeParametersName()));
                Writer.Write(">");
            }
        }
        else
        {
            if (TypeBuild.GetTypeParametersNameMethodOnly().Any())
            {
                Writer.Write("<");
                Writer.Write(string.Join(", ", TypeBuild.GetTypeParametersNameMethodOnly()));
                Writer.Write(">");
            }
        }


        Writer.Write("(");
        if (extensionMethod)
        {
            // 开始参数列表
            Writer.Write("this ");
            Writer.Write(TypeBuild.FullyQualifiedFormat());
            Writer.Write(" ");
            Writer.Write(Config.InstanceName);
            if (!Symbol.Parameters.IsEmpty)
            {
                Writer.Write(", ");
            }
        }
        for (int i = 0; i < Symbol.Parameters.Length; i++)
        {
            if (i > 0)
            {
                Writer.Write(", ");
            }
            var para = Symbol.Parameters[i];
            Writer.Write(para switch
            {
                { RefKind: RefKind.Ref } => "ref ",
                { RefKind: RefKind.Out } => "out ",
                { RefKind: RefKind.In } => "in ",
                { IsParams: true } => "params ",
                _ => null
            });
            Writer.Write(TypeBuild.FullyQualifiedFormat(para.Type));
            Writer.Write(" ");
            Writer.Write(para.Name.EscapeKeywords());
            if (para.HasExplicitDefaultValue)
            {
                Writer.Write(" = ");
                if (para.Type.TypeKind == TypeKind.Enum && para.ExplicitDefaultValue != null)
                {
                    if (para.Type is INamedTypeSymbol enumType)
                    {
                        var matchingMembers = enumType.GetMembers().OfType<IFieldSymbol>().Where(f => para.ExplicitDefaultValue.Equals(f.ConstantValue)).ToImmutableArray();
                        if (matchingMembers.Length == 1)
                        {
                            Writer.Write(enumType.ToGlobalDisplayString());
                            Writer.Write(".");
                            Writer.Write(matchingMembers.First().Name.EscapeKeywords());
                        }
                        else
                        {
                            Writer.Write("(");
                            Writer.Write(enumType.ToGlobalDisplayString());
                            Writer.Write(")");
                            Writer.Write(Convert.ToInt64(para.ExplicitDefaultValue));
                            if (matchingMembers.Length > 1)
                            { 
                                if (para.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()?.GetLocation() is { } location)
                                {
                                    ReportDiagnostic(Diagnostic.Create(ExtensionDiagnostic.ZMS005, location));
                                }
                                Writer.Write(" /* CA1069 */ ");
                            }
                        }
                    }
                }
                else
                {
                    Writer.Write(para.ExplicitDefaultValue switch
                    {
                        null => "null",
                        true => "true",
                        false => "false",
                        string s => $"@\"{s.Replace("\"", "\"\"")}\"",
                        char c => $"\'{c}\'",
                        float f => f.ToString(CultureInfo.InvariantCulture) + "f",
                        double d => d.ToString(CultureInfo.InvariantCulture) + "d",
                        decimal dec => dec.ToString(CultureInfo.InvariantCulture) + "m",
                        _ => para.ExplicitDefaultValue.ToString()
                    });
                }
            }
        }
        Writer.WriteLine(")");
        Writer.Indent++;
        // 添加泛型约束
        if (extensionMethod)
        {
            Writer.WriteSources(TypeBuild.AppendConstraintClauses().ToString());
        }
        else
        {
            Writer.WriteSources(TypeBuild.AppendConstraintClausesMethodOnly().ToString());
        }
        Writer.Indent--;

        using DeferredActionScope deferredActionScope = new DeferredActionScope();
        Writer.AppendOpenBracket(deferredActionScope);
        if (Symbol.ReturnType.SpecialType != SpecialType.System_Void)
        {
            Writer.Write("return ");
        }
        if (Symbol.ReturnsByRef || Symbol.ReturnsByRefReadonly)
        {
            Writer.Write("ref ");
        }
        Writer.Write(Config.InstanceName);
        switch (Symbol.AssociatedSymbol)
        {
            case IPropertySymbol { IsIndexer: true }:
                Writer.Write("[");
                var parameters = Symbol.Parameters;
                if (Symbol.MethodKind == MethodKind.PropertySet)
                {
                    parameters = parameters.Take(parameters.Length - 1).ToImmutableArray();
                }
                Writer.Write(string.Join(", ", parameters.Select(p => p.Name.EscapeKeywords())));
                Writer.Write("]");
                if (Symbol.MethodKind == MethodKind.PropertySet)
                {
                    Writer.Write(" = value");
                }
                break;
            case IPropertySymbol symbol:
                Writer.Write(".");
                Writer.Write(symbol.Name.EscapeKeywords());
                if (Symbol.MethodKind == MethodKind.PropertySet)
                {
                    Writer.Write(" = ");
                    Writer.Write("value");
                }
                break;
            case IEventSymbol symbol:
                Writer.Write(".");
                Writer.Write(symbol.Name.EscapeKeywords());
                Writer.Write(" ");
                Writer.Write(Symbol.MethodKind == MethodKind.EventAdd ? "+" : "-");
                Writer.Write("= value");
                break;

            default:
                Writer.Write(".");
                Writer.Write(Symbol.Name.EscapeKeywords());
                if (Symbol.IsGenericMethod)
                {
                    Writer.Write("<");
                    // 使用 TypeBuild.RenameMap 获取重命名后的类型参数名
                    Writer.Write(string.Join(", ", Symbol.TypeArguments.Select(ta =>
                    {
                        return ta is ITypeParameterSymbol typeParam ? TypeBuild.GetTypeParameterRenameOrDefault(typeParam) : ta.ToGlobalDisplayString();
                    })));
                    Writer.Write(">");
                }
                Writer.Write("(");

                bool firstParam = true;
                foreach (var param in Symbol.Parameters)
                {
                    if (!firstParam)
                    {
                        Writer.Write(", ");
                    }

                    Writer.Write(param switch
                    {
                        { RefKind: RefKind.Ref } => "ref ",
                        { RefKind: RefKind.Out } => "out ",
                        { RefKind: RefKind.In } => "in ",
                        _ => null
                    });

                    Writer.Write(param.Name.EscapeKeywords());
                    firstParam = false;
                }

                Writer.Write(")");
                break;
        }
        Writer.WriteLine(";");
    }
}
