using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

   // < ProjectReference Include = "..\MetaSourceGenerator.AttributeFactory\MetaSourceGenerator.AttributeFactory.csproj" OutputItemType = "Analyzer" />
// 1. 第一部分：用字符串写 var x = 5;
var part1Text = "var      \n x     =     5;";
var part1 = SyntaxFactory.ParseStatement(part1Text);

// 2. 第二部分：用语法树拼 Console.WriteLine(x);
var part2 = SyntaxFactory.ExpressionStatement(
    SyntaxFactory.InvocationExpression(
        SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName("Console"),
            SyntaxFactory.IdentifierName("WriteLine")
        )
    )
    .WithArgumentList(
        SyntaxFactory.ArgumentList(
            SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Argument(
                    SyntaxFactory.IdentifierName("x")
                )
            )
        )
    )
);

// 3. 第三部分：用字符串写 return x;
var part3Text = "return                             x;";
var part3 = SyntaxFactory.ParseStatement(part3Text);

// 4. 创建方法体块
var methodBody = SyntaxFactory.Block(part1, part2, part3);

// 5. 创建方法
var method = SyntaxFactory.MethodDeclaration(
    SyntaxFactory.ParseTypeName("int"),
    "Creat"
)
.WithParameterList(
    SyntaxFactory.ParseParameterList("(int a)")
)
.WithBody(methodBody);

method= (MethodDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("int Creat(int a)\r\n{\r\n    var x = 5;\r\n    Console.WriteLine(x);\r\n    return x;\r\n}\r\n");

// 6. 输出
Console.WriteLine(method.NormalizeWhitespace());
