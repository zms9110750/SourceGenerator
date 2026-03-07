using System;
using System.Collections.Generic;
using System.Drawing;
using zms9110750.InterfaceImplAsExtensionGenerator;
using IList = System.Int32;

Console.WriteLine();

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
[ExtensionSource]
public interface IHello
{
    string GetSet { get; set; }
#if NET10_0
    string GetInit { get; init; }
    string PropertyGet { protected get; set; }
#endif
    int this[string index] { get; }
    void Hello();
    string Hello(string t);
    public void Hello<T1, T2, T3>(out T1 t)
        where T1 : class, IList<int>, new()
        where T2 : struct, ISet<int>
        where T3 : class, IList<int>, new();

    ref string Hello(out string t, params int[] i);

    void Collection<T>(ref int a, int b = 10, string s = "hello\t" + @"  {你好}""{哈哈}  ", bool d = true | true ^ true, float f = 34, A<T>.B<T>.Color r = A<T>.B<T>.Color.B, params int[] p);
}
/*
<LangVersion >preview</LangVersion >
*/
public class @params { }
public class A<T>
{
    public class B<T>
    {
        public enum Color
        {
            Red = 1, Green = 2, Blue = 4, Ambient = 8, Cyan = 16, DarkBlue = 32,
            A = 33,
            B = 33
        }
    }
    internal protected class V { }
}
[ExtensionFor(typeof(List<int>))]

static partial class PC { }
