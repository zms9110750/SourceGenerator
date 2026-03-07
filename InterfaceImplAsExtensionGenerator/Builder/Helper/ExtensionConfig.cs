using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace zms9110750.InterfaceImplAsExtensionGenerator.Builder.Helper;

internal class ExtensionConfig
{
    private static readonly ConditionalWeakTable<IAssemblySymbol, ExtensionGlobalConfigAttribute> _cache = new();

    private ExtensionGlobalConfigAttribute ExtensionGlobalConfig { get; }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    private ExtensionConfig(IAssemblySymbol assemblySymbol)
    {
        ExtensionGlobalConfig = _cache.GetValue(assemblySymbol, ass =>
        {
            return ass.GetAttributes().Select(ExtensionGlobalConfigAttribute.Create).FirstOrDefault(att => att != null) ?? new ExtensionGlobalConfigAttribute();
        });
        UseSyntax = ExtensionGlobalConfig.UseSyntax;
    }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    /// <summary>
    /// 为一个接口生成扩展
    /// </summary>
    /// <param name="typeInternal"></param> 
    public ExtensionConfig(INamedTypeSymbol typeInternal) : this(typeInternal.ContainingAssembly)
    {
        _ = typeInternal ?? throw new ArgumentNullException(nameof(typeInternal));
        var attribute = typeInternal.GetAttributes().Select(ExtensionSourceAttribute.Create).FirstOrDefault(att => att != null);
        _ = attribute ?? throw new ArgumentException("The specified type does not have a valid ExtensionSourceAttribute.", nameof(typeInternal));
        TypeSymbol = typeInternal;
        ClassName = attribute.ExtensionClassName ?? typeInternal.Name + ExtensionGlobalConfig.TypeNameSuffix;
        NamespaceName = attribute.ExtensionClassNamespace
            ?? (typeInternal.ContainingNamespace.IsGlobalNamespace, ExtensionGlobalConfig.NamespaceSuffix) switch
            {
                (true, null) => "",
                (true, not null) => ExtensionGlobalConfig.NamespaceSuffix,
                (false, null) => typeInternal.ContainingNamespace.ToDisplayString(),
                (false, not null) => typeInternal.ContainingNamespace.ToDisplayString() + "." + ExtensionGlobalConfig.NamespaceSuffix,
            };

        InstanceName = attribute.InstanceParameterName ?? ExtensionGlobalConfig.InstanceParameterName;
        GenerateMembers = attribute.DefaultGenerateMembers.IfDefaultThen(ExtensionGlobalConfig.DefaultGenerateMembers);
        UsePublic = attribute.UsePublic.IfDefaultThen(ExtensionGlobalConfig.UsePublic) switch
        {
            PublicModifier.Always => true,
            PublicModifier.FollowInterface => typeInternal.DeclaredAccessibility == Accessibility.Public,
            _ => false
        };
    }
    /// <summary>
    /// 为一个类以及他指定的接口配置生成扩展
    /// </summary>
    /// <param name="typeClass"></param>
    /// <param name="attribute"></param>
    public ExtensionConfig(INamedTypeSymbol typeClass, ExtensionForAttribute attribute) : this(typeClass.ContainingAssembly)
    {
        _ = typeClass ?? throw new ArgumentNullException(nameof(typeClass));
        _ = attribute ?? throw new ArgumentNullException(nameof(attribute));
        TypeSymbol = attribute.AppendTypeSymbol;
        ClassName = typeClass.Name;
        NamespaceName = typeClass.ContainingNamespace.IsGlobalNamespace ? "" : typeClass.ContainingNamespace.ToDisplayString();
        InstanceName = attribute.InstanceParameterName ?? ExtensionGlobalConfig.InstanceParameterName;
        GenerateMembers = attribute.DefaultGenerateMembers.IfDefaultThen(ExtensionGlobalConfig.DefaultGenerateMembers);
    }

    /// <summary>
    /// 是否使用旧语法（扩展方法形式）生成扩展
    /// </summary>
    public ExtensionSyntaxVersion UseSyntax { get; }

    /// <summary>
    /// 要追加成员的接口类型
    /// </summary>
    public INamedTypeSymbol TypeSymbol { get; }

    /// <summary>
    /// 扩展类的名称
    /// </summary> 
    public string ClassName { get; }

    /// <summary>
    /// 扩展类所在的命名空间
    /// </summary> 
    public string NamespaceName { get; }

    /// <summary>
    /// 实例参数的名称
    /// </summary> 
    public string InstanceName { get; }

    /// <summary>
    /// 为当前接口默认生成的成员类型（按位枚举）
    /// </summary> 
    public GenerateMembers GenerateMembers { get; }

    /// <summary>
    /// 生成扩展类使用 public 修饰
    /// </summary>  
    public bool UsePublic { get; }

    public ExtensionMemberConfig WithSymbol(ISymbol symbol)
    {
        return new ExtensionMemberConfig(this, symbol);
    }
}
