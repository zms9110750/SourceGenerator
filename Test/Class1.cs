
using System;
using System.Collections.Generic;
using System.ComponentModel;
using zms9110750.InterfaceImplAsExtensionGenerator;


namespace MyNamespace
{
    [ExtensionSourceAttribute]
    public interface IHP2<T> where T : class, IEquatable<T>, IList<int>, new()
    {
        [DescriptionAttribute]
        void Hello();

    }
}
[ExtensionSourceAttribute]
public interface IHP1<T>where T:class
{
    [DescriptionAttribute]
    void Hello();
    [ExtensionSourceAttribute]
    public interface IHP369<G> where G : class, IEquatable<T>, IList<T>, new() 
    {
        [DescriptionAttribute]
        void Hello();

    }

    [ExtensionFor(typeof(IHP1<>))]
    [ExtensionFor(typeof(IHP1<string>))]
    static partial class LP
    {

    } 
}
