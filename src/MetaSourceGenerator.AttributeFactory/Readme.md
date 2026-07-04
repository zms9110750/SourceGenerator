# 简介

为特性类生成基于`AttributeData`的创建实例


## 主要API
引入命名空间`zms9110750.MetaSourceGenerator.AttributeFactory`

- `FromAttributeDataAttribute`:为附着的特性生成工厂方法
 
## 注意事项

- 方法名始终为`Create`，没有避让措施。
- 有可访问的类型为`Type`或`Type[]`字段时，会自动生成`ITypeSymbol`或`ITypeSymbol[]`属性。属性为源属性名后加`Symbol`后缀。
  - 没有避让措施。
- 在源数据中，特性参数里的`Type`和`Type[]`在分析器中都会解析为`ITypeSymbol`或`ITypeSymbol[]`
  - 为属性赋值时会自动转为对`Symbol`赋值
  - 但构造器不会自动生成。仅在有对应重载的构造器时，才会被调用。
- 会创建一个`FullName`名子的常量。用于`ForAttributeWithMetadataName`方法使用。没有避让措施。

## 示例

```csharp
[FromAttributeData]
public partial class ExtensionForAttribute : Attribute
{
    public Type? AppendType { get; }

    public string? InstanceParameterName { get; set; }

    public GenerateMembers DefaultGenerateMembers { get; set; }

    public ExtensionForAttribute(Type appendType)
    {
        AppendType = appendType;
    }
    internal ExtensionForAttribute(INamedTypeSymbol symbol)
    {
        AppendTypeSymbol = symbol.IfThen(symbol.IsUnboundGenericType, symbol.OriginalDefinition); 
    }
}
```
生成
```csharp
partial class ExtensionForAttribute
{
    internal const string FullName = "zms9110750.InterfaceImplAsExtensionGenerator.ExtensionForAttribute";
    
    /// <summary>
    /// 自动生成。为<see cref="AppendType"/>在<see cref="AttributeData"/>中的<see cref="ITypeSymbol"/>表现形式
    /// </summary>
    internal global::Microsoft.CodeAnalysis.ITypeSymbol AppendTypeSymbol{ get; set; }
    
    public static global::zms9110750.InterfaceImplAsExtensionGenerator.ExtensionForAttribute Create(global::Microsoft.CodeAnalysis.AttributeData data)
    {
        if (data == null)
        {
            return null;
        }
        var format = global::Microsoft.CodeAnalysis.SymbolDisplayFormat.FullyQualifiedFormat;
        if (data.AttributeClass.ToDisplayString(format) != "global::zms9110750.InterfaceImplAsExtensionGenerator.ExtensionForAttribute")
        {
            return null;
        }
        
        global::zms9110750.InterfaceImplAsExtensionGenerator.ExtensionForAttribute value = null;
        switch (string.Join("|", global::System.Linq.Enumerable.Select(data.AttributeConstructor.Parameters, p => p.Type.ToDisplayString(format))))
        {
            case "global::System.Type":
                return null;
            default:
                return null;
            }
            foreach (var symbol in data.NamedArguments)
            {
                switch (symbol.Key)
                {
                    case "AppendType":
                        value.AppendTypeSymbol = (global::Microsoft.CodeAnalysis.ITypeSymbol)symbol.Value.Value;
                        break;
                    case "InstanceParameterName":
                        value.InstanceParameterName = (string)symbol.Value.Value;
                        break;
                    case "DefaultGenerateMembers":
                        value.DefaultGenerateMembers = (global::zms9110750.InterfaceImplAsExtensionGenerator.Config.GenerateMembers)symbol.Value.Value;
                        break;
                }
            }
            return value;
        }
    }
}
```