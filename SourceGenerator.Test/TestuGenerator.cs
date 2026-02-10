using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
namespace zms9110750.SourceGenerator.Test
{
    [Generator]
    class TestuGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var methodProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    "zms9110750.SourceGenerator.Test.TestAttribute",
                    predicate: static (node, token) => node is MethodDeclarationSyntax,
                    transform: static (ctx, token) => (Syntax: ctx.TargetNode as MethodDeclarationSyntax,
                        Symbol: ctx.TargetSymbol as IMethodSymbol,
                        Attributes: ctx.Attributes))
                .Where(static x => x.Syntax != null && x.Symbol != null && x.Attributes.Any());

            context.RegisterSourceOutput(methodProvider, (spc, tuple) =>
            {
                var (syntax, symbol, attributes) = tuple;

                try
                {
                    // 构建所有特性的字符串
                    var stringBuilder = new System.Text.StringBuilder();

                    // 添加注释开头
                    stringBuilder.AppendLine("// 文件由 TestuGenerator 生成");
                    stringBuilder.AppendLine();

                    // 处理每个特性
                    foreach (var attribute in attributes)
                    {
                        var testAttribute = TestAttribute.Creat(attribute);
                        var content = testAttribute?.ToString() ?? "null";

                        // 每个特性的内容用/**/包围
                        stringBuilder.AppendLine("/*");
                        stringBuilder.AppendLine(content);
                        stringBuilder.AppendLine("*/");

                        // 换行两行
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine();
                    }

                    // 生成文件名
                    var fileName = GenerateFileName(symbol!);
                    spc.AddSource(fileName, stringBuilder.ToString());
                }
                catch (InvalidCastException ex)
                {


                }
                catch (Exception ex)
                {
                    var errorFileName = $"Error_{symbol!.ContainingType.Name}_{symbol.Name}_{Guid.NewGuid():N}";
                    var errorContent = $"// 生成错误\n{ex}";
                    spc.AddSource(errorFileName, errorContent);
                }
            });
        }

        private static string GenerateFileName(IMethodSymbol methodSymbol)
        {

            var typeName = methodSymbol.ContainingType
                .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
                    .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))
                .Replace("<", "{")
                .Replace(">", "}");

            var methodName = methodSymbol.Name;

            if (methodSymbol.IsGenericMethod)
            {
                methodName += $"_{methodSymbol.TypeParameters.Length}";
            }

            return $"{typeName}_{methodName}.g.cs";
        }
    }
}
