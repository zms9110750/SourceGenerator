global using IList = System.Int32;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

var p = SyntaxFactory.ParseMemberDeclaration("   public   void Hello3<T, H, E, G>() where T : class, E, new() where E : notnull, IList<E>, IEquatable<E> where H : unmanaged, allows ref struct where G : unmanaged\r\n    {\r\n    }") as MethodDeclarationSyntax;
// 6. 输出



// 这是正确的写法
var condition = SyntaxFactory.IdentifierName("NET10_0_OR_GREATER");
var ifNet10 = SyntaxFactory.IfDirectiveTrivia(condition, true, true, true);
var ifNotNet10 = ifNet10.WithCondition(SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, condition));
var endif = SyntaxFactory.EndIfDirectiveTrivia(true);
Console.WriteLine(SyntaxFactory.EndIfDirectiveTrivia(true).NormalizeWhitespace());

var extensionBlock = SyntaxFactory.ExtensionBlockDeclaration(
    // 特性列表
    attributeLists: SyntaxFactory.List<AttributeListSyntax>(), 
    modifiers: SyntaxFactory.TokenList(), 
    keyword: SyntaxFactory.Token(SyntaxKind.ExtensionKeyword), 
    typeParameterList: null, 
    parameterList: SyntaxFactory.ParameterList(
        SyntaxFactory.SingletonSeparatedList(
            SyntaxFactory.Parameter(
                SyntaxFactory.Identifier("i"))
                .WithType(SyntaxFactory.PredefinedType(
                    SyntaxFactory.Token(SyntaxKind.IntKeyword))))), 
    constraintClauses: SyntaxFactory.List<TypeParameterConstraintClauseSyntax>(), 
    openBraceToken: SyntaxFactory.Token(SyntaxKind.OpenBraceToken), 
    members: SyntaxFactory.List(new MemberDeclarationSyntax[] { }), 
    closeBraceToken: SyntaxFactory.Token(SyntaxKind.CloseBraceToken), 
    semicolonToken: default
);

Console.WriteLine(extensionBlock.NormalizeWhitespace());
static class OL
{ 
}
partial class CC
{
    [Description]
    public void Hello3<T, H, E, G>()
        where T : class, E, new()
        where E : notnull, IList<E>, IEquatable<E>, new()
        where H : unmanaged, allows ref struct
        where G : unmanaged
    {
    }
    [Description]
    public void Hello4<T, H, E, G>()
        where T : class, E, new()
        where E : notnull, global::System.Collections.Generic.IList<E>, global::System.IEquatable<E>, new()
        where H : unmanaged, allows ref struct
        where G : unmanaged
    {
    }
    [Description]
    public void Hello5<T, H, E, G>()
       where E : Random, new()
       where G : class, IEquatable<G>, new()
       where H : unmanaged, IEquatable<G>, allows ref struct
    {
    }
}
/*
// 生成的代码
// 方法: Hello3
    [Description]
    public void Hello3<T, H, E, G>()
        where T : class,E,new()
        where E : notnull,global::System.Collections.Generic.IList<E>,global::System.IEquatable<E>,new()
        where H : unmanaged,allows ref struct
        where G : unmanaged
    {
    }

*/
/*
// 生成的代码
// 方法: Hello4
    [Description]
    public void Hello4<T, H, E, G>()
        where T : class,E,new()
        where E : notnull,global::System.Collections.Generic.IList<E>,global::System.IEquatable<E>,new()
        where H : unmanaged,allows ref struct
        where G : unmanaged
    {
    }

*/
/*
// 生成的代码
// 方法: Hello5
    [Description]
    public void Hello5<T, H, E, G>()
       where E : global::System.Random,new()
       where G : class,global::System.IEquatable<G>,new()
       where H : unmanaged,global::System.IEquatable<G>,allows ref struct
    {
    }

*/
