# 简介

这个包可以为接口API生成扩展方法。

让显式实现的方法也可以调用。


## 主要API
引入命名空间`zms9110750.InterfaceImplAsExtensionGenerator`

- `ExtensionGlobalConfig`:全局配置，作用于程序集
- `ExtensionSourceAttribute`:接口配置，作用于接口
- `ExtensionForAttribute`:类配置，作用于顶级非泛型静态类
- `ExtensionIncludeAttribute`:成员配置，并强制包含该成员生成扩展
- `ExtensionIgnoreAttribute`:强制不包含该成员

## 配置项

效果|全局配置|接口配置|类配置|成员配置|默认设置|备注
-|-|-|-|-|-|-
使用扩展语法|`UseSyntax`|-|-|-|自动|-
附带`public`修饰|`UsePublic`|`UsePublic(bool)`|-|-|跟随接口|全局配置使用枚举。接口配置使用`bool`，类配置自行为声明类添加修饰符
作用目标接口|-|特性附着的接口|构造器参数|-|-|-
类名|`TypeNameSuffix`|`ExtensionClassName`|特性附着类|-|`Extension`|全局配置仅设置后缀
类所在命名空间|`NamespaceSuffix`|`ExtensionClassNamespace`|特性附着类|-|-|全局配置仅设置后缀。后缀名即子命名空间。
实例参数名|`InstanceParameterName`|`InstanceParameterName`|`InstanceParameterName`|`InstanceParameterName`|`instance`|扩展方法的实例变量名可被成员配置影响。扩展块实例名仅能被接口配置和类配置影响。
成员名|-|-|-|`ReplacementMemberName`|跟随成员名|扩展方法版本的访问器会有`get_`,`set_`,`add_`,`remove_`前缀。索引器总是`get_Item`和`set_Item`
包含的成员|`DefaultGenerateMembers`|`DefaultGenerateMembers`|`DefaultGenerateMembers`|附着成员强制包含|属性和方法|目前扩展块语法不支持索引器和事件。在支持的时候可以自行启用
 
## 配置枚举项

### `PublicModifier`

全局配置的`UsePublic`成员枚举。

- `NoModifier`：始终不添加
- `FollowInterface`:跟随接口
- `Always`:总是添加
 
### `GenerateMembers`

所有配置的`DefaultGenerateMembers`成员枚举。
指示为哪些成员生成扩展。按位枚举。

- `Property`：属性
- `Indexer`:索引器
- `Event`：事件
- `Method`：方法
- `All`：全部
   
### `ExtensionSyntaxVersion`

全局配置的`UseSyntax`成员枚举。
- `Auto`：如果当前项目语言>=c#14，以扩展块生成。否则以扩展方法生成。
- `ExtensionBlock`：扩展块
- `ExtensionMethod`：扩展方法
- `Both`：生成基于`#if NET10_OR_GREATER`的条件编译。

## 边缘测试

- 方法或属性返回`ref`和`ref readonly`类型
- 参数或成员名为关键字
- 泛型名与声明处有同名泛型（泛型方法和泛型接口，泛型接口和嵌套泛型接口）
- 参数有`ref`,`in`,`out`,`params`修饰
- 泛型约束
- 访问器独立访问权限修饰符（仅在`public`和`internal`时区分）
- 属性没有有效访问器（仅有`init`访问器。`get`访问器不存在或访问权限过低）
- `internal protected`视为`internal`
- 区分有返回值方法和`void`方法
- 参数默认值
- 没有对应字段或多个对应字段的枚举默认值
- 生成对源成员的文档注释引用


## 示例

```csharp
namespace Hello.World
{
    [ExtensionFor(typeof(IList<int>))]
    [ExtensionFor(typeof(IList<>))]
    public static partial class Apple
    {
        [ExtensionSource]
        public interface IOrange
        {
            string GetSet { get; set; }
#if NET10_0
            string GetInit { get; init; }
            string PropertyGet { protected get; set; }
#endif
            int this[string index] { get; }
            internal void Hello<T1, T2, T3>(out T1 t)
                 where T1 : class, IList<int>, new()
                 where T2 : struct, ISet<T2>
                 where T3 : class, IDog<T3>, new();

            ref string Hello(out string t, params int[] i);

            internal void Collection<T>(ref int a, int b = 10, string s = "hello\t" + @"  {你好}""{哈哈}  ", bool d = true | true ^ true, float f = 34, IDog<T>.Cat<T>.Color c = IDog<T>.Cat<T>.Color.B, params int[] p);
        }
    }
    interface IDog<T>
    {
        public interface Cat<T>
        {
            @class @event<@int>(@int @string);
            public enum Color
            {
                Red = 1, Green = 2, Blue = 4, Ambient = 8, Cyan = 16, DarkBlue = 32,
                A = 33,
                B = 33
            }
        }
    }
    class @class { }
}
```