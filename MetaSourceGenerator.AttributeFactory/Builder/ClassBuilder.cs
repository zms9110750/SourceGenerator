using Microsoft.CodeAnalysis;
using zms9110750.MetaSourceGenerator.AttributeFactory.DiagnosticDefine;

namespace zms9110750.MetaSourceGenerator.AttributeFactory.Builder
{
    class ClassBuilder : BaseBuilder
    {
        public override string? FileName { get; }
        readonly ClassDeclarationSyntax _classSyntax;
        public ClassBuilder(INamedTypeSymbol classSymbol, ClassDeclarationSyntax classSyntax) : base(classSymbol)
        {
            _classSyntax = classSyntax;

            FileName = classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))
                        .Replace("<", "{")
                        .Replace(">", "}")
                        + ".g.cs";
            if (this.Any(s => s.Descriptor.DefaultSeverity == DiagnosticSeverity.Error))
            {
                FileName = null;
            }
        }
        public override CompilationUnitSyntax Generate()
        {
            if (AttributeSymbol.ContainingNamespace.IsGlobalNamespace)
            {
                return SyntaxFactory.CompilationUnit().AddMembers(Class());
            }
            else
            {
                return SyntaxFactory.CompilationUnit().AddMembers(
                    SyntaxFactory.NamespaceDeclaration(
                        SyntaxFactory.ParseName(AttributeSymbol.ContainingNamespace.ToDisplayString())
                    )
                    .AddMembers(Class())
                );
            }
        }


        ClassDeclarationSyntax Class()
        {
            return SyntaxFactory.ClassDeclaration(AttributeSymbol.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(CreateFactoryMethod())
                .AddMembers(CreateProperty());
        }
        internal PropertyDeclarationSyntax[] CreateProperty()
        {
            if (!HasTypeWritableProperties.Any())
            {
                return Array.Empty<PropertyDeclarationSyntax>();
            }

            return HasTypeWritableProperties.Select(s => (PropertyDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration($"internal Microsoft.CodeAnalysis.INamedTypeSymbol{(s.Type is IArrayTypeSymbol ? "[]" : null)} {s.Name}Symbol {{ get; set; }}")!).ToArray()!;
        }


        public MethodDeclarationSyntax CreateFactoryMethod()
        {

            // 生成方法体代码
            string methodBody = GenerateMethodBody();

            // 返回类型 - 类自身
            var returnType = SyntaxFactory.ParseTypeName(AttributeFullName);

            // 参数类型
            var paramType = SyntaxFactory.ParseTypeName("global::Microsoft.CodeAnalysis.AttributeData");
            var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("data"))
                .WithType(paramType);

            return SyntaxFactory.MethodDeclaration(
                    returnType,
                    SyntaxFactory.Identifier("Creat")
                )
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                )
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList(parameter)
                    )
                )
                .WithBody(SyntaxFactory.ParseStatement(methodBody) as BlockSyntax);
        }

        public override string? GetPropertyNameIfShouldGenerate(IPropertySymbol propertySymbol)
        {
            return propertySymbol.Name + "Symbol";
        }


        public override bool IsValidMemberAccessibility(Accessibility accessibility)
        {
            return accessibility switch
            {
                Accessibility.Public => true,
                Accessibility.Internal => true,
                Accessibility.ProtectedOrInternal => true,
                _ => false
            };
        }
        public override IEnumerator<Diagnostic> GetEnumerator()
        {
            Location location = _classSyntax!.GetLocation();
            // 查找 FromAttributeDataAttribute 的位置
            foreach (var item in AttributeSymbol!.GetAttributes())
            {
                if (item.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ==
                    "global::zms9110750.MetaSourceGenerator.AttributeFactory.FromAttributeDataAttribute")
                {
                    location = item.ApplicationSyntaxReference!.GetSyntax().GetLocation();
                    break;
                }
            }
            bool isAttributeDerived = false;
            for (INamedTypeSymbol? currentType = AttributeSymbol;
                 currentType != null;
                 currentType = currentType.BaseType)
            {
                if (currentType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ==
                    "global::System.Attribute")
                {
                    isAttributeDerived = true;
                    break;
                }
            }

            if (!isAttributeDerived)
            {
                yield return Diagnostic.Create(FromAttributeDiagnostic.ZMS020, location);
            }

            if (AttributeSymbol.IsAbstract)
            {
                yield return Diagnostic.Create(FromAttributeDiagnostic.ZMS021, location);
            }

            if (HasTypeConstructors.Where(p => !ShouldGenerateMethod(p)).Any())
            {
                yield return Diagnostic.Create(FromAttributeDiagnostic.ZMS022, location);
            }
        }
    }
}