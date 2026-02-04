using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using zms9110750.MetaSourceGenerator.AttributeFactory.Gener;

namespace zms9110750.MetaSourceGenerator.AttributeFactory.Converter
{
    class ClassConverter : BaseConverter
    {

        public ClassConverter(INamedTypeSymbol classSymbol, ClassDeclarationSyntax classSyntax) : base(classSymbol)
        {
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

        public override IEnumerable<DiagnosticDescriptor> Diagnostics()
        {
            if (HasTypeConstructors.Where(p => !ShouldGenerateMethod(p)).Any())
            {
                return [FromAttributeDiagnostic.ZMS022];
            }
            return [];
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
    }
}