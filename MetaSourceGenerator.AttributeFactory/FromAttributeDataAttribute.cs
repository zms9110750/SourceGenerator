using Microsoft.CodeAnalysis;
using System;

namespace zms9110750.MetaSourceGenerator.AttributeFactory
{
    /// <summary>
    /// 这个特性标记的特性类会自动生成一个静态方法用于从 <see cref="AttributeData"/> 创建该特性实例。
    /// <br/>或一个以<see cref="AttributeData"/>为唯一参数，返回值为特性的方法会自动生成方法体用于从 <see cref="AttributeData"/> 创建该特性实例。
    /// </summary>
    /// <remarks><see cref="AttributeData"/>是生成器包才存在的东西，这个特性只用于辅助生成器构建。</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class FromAttributeDataAttribute : Attribute
    {
    }
}
