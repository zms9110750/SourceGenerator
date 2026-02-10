using System.Collections;
using System.Runtime.CompilerServices;

namespace zms9110750.InterfaceImplAsExtensionGenerator.Build;

class InterfaceBuild : IEnumerable<Diagnostic>
{
    private static readonly ConditionalWeakTable<IAssemblySymbol, ExtensionGlobalConfigAttribute> _cache = new();
    ExtensionGlobalConfigAttribute GlobalConfigAttribute { get; }


    /// <summary>
    /// 扩展的接口的语义声明
    /// </summary>
    public INamedTypeSymbol InterfaceSymbol { get; }
    /// <summary>
    /// 命名空间名
    /// </summary>
    string? NameSpaceName { get; }
    /// <summary>
    /// 类名
    /// </summary>
    string ClassName { get; }
    /// <summary>
    /// 实例参数名称
    /// </summary>
    string InstanceParameterName { get; set; }
    /// <summary>
    /// 为哪些成员生成扩展
    /// </summary>
    GenerateMembers GenerateMembers { get; set; }
    /// <summary>
    /// 是否附加public修饰符
    /// </summary>
    bool UsePublic { get; set; }
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    private InterfaceBuild(INamedTypeSymbol interfaceSymbol)
    {
        InterfaceSymbol = interfaceSymbol;
        GlobalConfigAttribute = _cache.GetValue(interfaceSymbol.ContainingAssembly, ass =>
        {
            return ass.GetAttributes().Select(ExtensionGlobalConfigAttribute.Creat).FirstOrDefault(att => att != null) ?? new ExtensionGlobalConfigAttribute();
        });
    }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    /// <summary>
    /// 从一个接口和他身上的特性构造一个生成器
    /// </summary>
    /// <param name="interfaceSymbol"></param>
    /// <param name="sourceAttribute"></param>
    public InterfaceBuild(ExtensionSourceAttribute sourceAttribute, INamedTypeSymbol interfaceSymbol) : this(interfaceSymbol)
    {
        NameSpaceName = sourceAttribute.ExtensionClassNamespace ??
            (GlobalConfigAttribute.NamespaceSuffix, interfaceSymbol.ContainingNamespace) switch
            {
                (null, { IsGlobalNamespace: true }) => null,
                (_, { IsGlobalNamespace: true }) => GlobalConfigAttribute.NamespaceSuffix,
                (null, { IsGlobalNamespace: false } containing) => containing.ToDisplayString(),
                (_, { IsGlobalNamespace: false } containing) => containing.ToDisplayString() + "." + GlobalConfigAttribute.NamespaceSuffix,
            };
        ClassName = sourceAttribute.ExtensionClassName ??
            interfaceSymbol.Name + GlobalConfigAttribute.TypeNameSuffix;
        InstanceParameterName = sourceAttribute.InstanceParameterName ?? GlobalConfigAttribute.InstanceParameterName;
        GenerateMembers = sourceAttribute.DefaultGenerateMembers;
        if (GenerateMembers == default)
        {
            GenerateMembers = GlobalConfigAttribute.DefaultGenerateMembers;
        }
        UsePublic = (sourceAttribute.UsePublic == default ? GlobalConfigAttribute.UsePublic : sourceAttribute.UsePublic) switch
        {
            PublicModifier.Always => true,
            PublicModifier.FollowInterface => interfaceSymbol.DeclaredAccessibility == Accessibility.Public,
            _ => false
        };
        FileName = NameSpaceName + "." + ClassName.Replace("<", "{").Replace(">", "}") + "." + interfaceSymbol.Name + ".cs";

    }
    /// <summary>
    /// 从静态类和他身上的特性构造一个生成器
    /// </summary> 
    /// <param name="forAttribute"></param>
    /// <param name="classtypeSymbol"></param>
    public InterfaceBuild(ExtensionForAttribute forAttribute, INamedTypeSymbol classtypeSymbol) : this(forAttribute.AppendTypeSymbol!)
    { 
        NameSpaceName = classtypeSymbol.ContainingNamespace is { IsGlobalNamespace: false } containing ? containing.ToDisplayString() : null;
        ClassName = classtypeSymbol.Name;
        InstanceParameterName = forAttribute.InstanceParameterName ?? GlobalConfigAttribute.InstanceParameterName;
        GenerateMembers = forAttribute.DefaultGenerateMembers;
        if (GenerateMembers == default)
        {
            GenerateMembers = GlobalConfigAttribute.DefaultGenerateMembers;
        }
        FileName = NameSpaceName + "." + ClassName.Replace("<", "{").Replace(">", "}") + "." + classtypeSymbol.Name + "."
            + forAttribute.AppendTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)).Replace("<", "{").Replace(">", "}") + ".cs";
        UsePublic = false;
    }

    /// <summary>
    /// 生成时，文件名。如果为null，则不生成文件
    /// </summary>
    public string? FileName { get; }

    public CompilationUnitSyntax Generate()
    {
        if (NameSpaceName == null)
        {
            return SyntaxFactory.CompilationUnit().AddMembers(Class());
        }
        else
        {
            return SyntaxFactory.CompilationUnit().AddMembers(
                SyntaxFactory.NamespaceDeclaration(
                    SyntaxFactory.ParseName(NameSpaceName)
                )
                .AddMembers(Class())
            );
        }
    }

    ClassDeclarationSyntax[] Class()
    {
        var condition = SyntaxFactory.IdentifierName("NET10_0_OR_GREATER");
        var ifNet10 = SyntaxFactory.IfDirectiveTrivia(condition, true, true, true);
        var ifNotNet10 = ifNet10.WithCondition(SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, condition));
        var endif = SyntaxFactory.EndIfDirectiveTrivia(true);
        var classDeclaration = SyntaxFactory.ClassDeclaration(ClassName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        if (UsePublic)
        {
            classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
        }
        return GlobalConfigAttribute.UseSyntax switch
        {
            ExtensionSyntaxVersion.ExtensionBlock => [classDeclaration.AddMembers(CreateExtensionBlock())],
            ExtensionSyntaxVersion.ExtensionMethod => [classDeclaration.AddMembers(CreateExtensionMethod())],
            _ => [classDeclaration
                    .WithLeadingTrivia(SyntaxFactory.Trivia(ifNet10))
                    .WithTrailingTrivia(SyntaxFactory.Trivia(endif))
                    .AddMembers(CreateExtensionBlock()),
              classDeclaration
                    .WithLeadingTrivia(SyntaxFactory.Trivia(ifNotNet10))
                    .WithTrailingTrivia(SyntaxFactory.Trivia(endif))
                    .AddMembers(CreateExtensionMethod())       ],
        };
    }

    private MemberDeclarationSyntax[] CreateExtensionBlock()
    {
        List<MemberDeclarationSyntax> list = [];
        foreach (var item in InterfaceSymbol.GetMembers())
        {

        }
        TypeParameterListSyntax? typeParameterList = null;
        List<ITypeSymbol> typeParameters = []; 
        for (var symbol = InterfaceSymbol; symbol != null; symbol = symbol.ContainingType)
        { 
            typeParameters.AddRange(symbol.TypeArguments); 
        }
        if (typeParameters.Any())
        {
            typeParameterList =
            SyntaxFactory.TypeParameterList(SyntaxFactory.SeparatedList(typeParameters.Select(tp => SyntaxFactory.TypeParameter(tp.Name))));
        }

        var extensionBlock = SyntaxFactory.ExtensionBlockDeclaration(
            attributeLists: SyntaxFactory.List<AttributeListSyntax>(),
            modifiers: SyntaxFactory.TokenList(),
            keyword: SyntaxFactory.Token(SyntaxKind.ExtensionKeyword),
            typeParameterList: typeParameterList,
            parameterList: SyntaxFactory.ParameterList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Parameter(
                        SyntaxFactory.Identifier(InstanceParameterName))
                        .WithType(SyntaxFactory.ParseTypeName(InterfaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))))),
            constraintClauses: SyntaxFactory.List(Extend.CreateConstraintClauses(typeParameters.OfType<ITypeParameterSymbol>())),
            openBraceToken: SyntaxFactory.Token(SyntaxKind.OpenBraceToken),
            members: SyntaxFactory.List(list),
            closeBraceToken: SyntaxFactory.Token(SyntaxKind.CloseBraceToken),
            semicolonToken: default
        );
        return [extensionBlock];
    }

    private MemberDeclarationSyntax[] CreateExtensionMethod()
    {
        foreach (var item in InterfaceSymbol.GetMembers())
        {

        }
        return [];
    }


    public IEnumerator<Diagnostic> GetEnumerator()
    {
        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}