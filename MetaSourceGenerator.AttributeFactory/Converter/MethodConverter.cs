using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using zms9110750.MetaSourceGenerator.AttributeFactory.Gener;

namespace zms9110750.MetaSourceGenerator.AttributeFactory.Converter
{
    class MethodConverter : BaseConverter
    {
        private readonly IMethodSymbol _methodSymbol;
        private readonly MethodDeclarationSyntax _methodSyntax;
        private readonly string _attributeFullName;
        private readonly string _parameterName;
        public MethodConverter(IMethodSymbol methodSymbol, MethodDeclarationSyntax methodSyntax) : base((INamedTypeSymbol)methodSymbol.ReturnType)
        {
            _methodSymbol = methodSymbol;
            _methodSyntax = methodSyntax;
            _attributeFullName = _methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            _parameterName = _methodSymbol.Parameters[0].Name;
        }

        public override CompilationUnitSyntax Generate()
        {
            if (_methodSymbol.ContainingNamespace.IsGlobalNamespace)
            {
                return SyntaxFactory.CompilationUnit().AddMembers(Class());
            }
            else
            {
                return SyntaxFactory.CompilationUnit().AddMembers(
                    SyntaxFactory.NamespaceDeclaration(
                        SyntaxFactory.ParseName(_methodSymbol.ContainingNamespace.ToDisplayString())
                    )
                    .AddMembers(Class())
                );
            }
        }
        ClassDeclarationSyntax Class()
        {
            return SyntaxFactory.ClassDeclaration(_methodSymbol.ContainingType.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(ConvertToFullQualified());
        }


        public MethodDeclarationSyntax ConvertToFullQualified()
        {
            // 返回类型完全限定名
            var returnType = SyntaxFactory.ParseTypeName(
                _methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            );

            // 参数完全限定名
            var paramType = SyntaxFactory.ParseTypeName(
                _methodSymbol.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            );

            var parameter = SyntaxFactory.Parameter(
                _methodSyntax.ParameterList.Parameters[0].Identifier
            ).WithType(paramType)
            .WithModifiers(_methodSyntax.ParameterList.Parameters[0].Modifiers)
            ;

            return SyntaxFactory.MethodDeclaration(
                returnType,
                _methodSyntax.Identifier
            )
            .WithModifiers(_methodSyntax.Modifiers)  // 复制原修饰符
            .WithParameterList(
                SyntaxFactory.ParameterList(
                    SyntaxFactory.SingletonSeparatedList(parameter)
                )
            )
           .WithBody(CreateFactoryMethodBody());
        }
        public BlockSyntax CreateFactoryMethodBody()
        {
            string code = $$"""
                {
                return Creat({{_parameterName}});
                {{_attributeFullName}} Creat(global::Microsoft.CodeAnalysis.AttributeData data)
                {{GenerateMethodBody()}}
                }
                """;
            // 解析为语法块
            return (BlockSyntax)SyntaxFactory.ParseStatement(code);
        }

        public override string? GetPropertyNameIfShouldGenerate(IPropertySymbol propertySymbol)
        {
            return null;
        }

        public override IEnumerable<DiagnosticDescriptor> Diagnostics()
        {
            List<DiagnosticDescriptor> list = new List<DiagnosticDescriptor>();
            if (HasTypeConstructors.Where(p => !ShouldGenerateMethod(p)).Any())
            {
                list.Add(FromAttributeDiagnostic.ZMS022);
            }
            if (!HasTypeWritableProperties.IsEmpty)
            {
                list.Add(FromAttributeDiagnostic.ZMS007);
            }
            return list;
        }
        public override bool IsValidMemberAccessibility(Accessibility accessibility)
        {
            return accessibility switch
            {
                Accessibility.Public => true, 
                _ => false
            };
        }
#if false
        DescriptionAttribute Creat(AttributeData data)
        {
            if (data == null)
            {
                return null;
            }
            var format = global::Microsoft.CodeAnalysis.SymbolDisplayFormat.FullyQualifiedFormat;
            if (data.AttributeClass.ToDisplayString(format) != "global::System.ComponentModel.DescriptionAttribute")
            {
                return null;
            }

            DescriptionAttribute? value = null;
            switch (string.Join("|", global::System.Linq.Enumerable.Select(data.AttributeConstructor.Parameters, p => p.Type.ToDisplayString(format))))
            {
                case "":
                    value = new DescriptionAttribute();
                    break;
                case "int":
                    value = new DescriptionAttribute((int)data.ConstructorArguments[0].Value, (int[])System.Linq.Enumerable.ToArray(data.ConstructorArguments[1].Values));
                    break;
                default:
                    return null;
            }
            foreach (var item in data.NamedArguments)
            {
                switch (item.Key)
                {
                    case "Description":
                        value.Description = (string)item.Value.Value;
                        break;
                    case "Name":
                        var a = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(item.Value.Values, s => (int)s.Value));

                        break;
                }
            }
            return value;
        }
#endif
    }
}