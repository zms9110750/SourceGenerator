

using System;
using System.Collections.Generic;
using System.ComponentModel;
using zms9110750.InterfaceImplAsExtensionGenerator;
[assembly: ExtensionGlobalConfig()]
#if false


class C<T>
{
    public class B
    {
        public class A { }
    }
}


[ExtensionSource]
interface HZ<T2>
{
    C<T2>.B.A Hello();
    C<int>.B.A Hello2();

    event Action Action;
    int this[int index] { get; set; }
    int Age { get; internal set; }
    ref int Name { get; }
    ref int AA(string s = "\r\t\n\u6959" + @"hello	 {你好}""{哈哈} asdasdasd ");

    interface IHZ<T2>
    {

        [ExtensionSource]
        interface IHZE<T2>
        {
            /// <summary>
            /// 对的对的
            /// </summary>
            /// <typeparam name="T2"></typeparam>
            void BB<T2>(out int t) where T2 : class;
        }
    }
}

namespace Hello
{
    [ExtensionFor(typeof(IHP3<,>))]
    [ExtensionFor(typeof(IHP3<string, int>))]
    public static partial class World { }

    [ExtensionFor(typeof(List<>))]
    [ExtensionFor(typeof(IDictionary<string, string>))]
    public static partial class SayHello { }
}
namespace Hello404
{
    [ExtensionSource]
    public interface IHP3<T, G> where T : class
    {
        ///<inheritdoc cref = "List{T}.AddRange(IEnumerable{T})"/>

        void Hello<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
            where T1 : class
            where T2 : class, new()
            where T3 : class
            where T4 : class, new()
            where T5 : class, new()
            where T6 : class, new()
            where T7 : class
            where T8 : class, new()
            where T9 : class, new()
            where T10 : class
            ;

        public void Hello<T1, T2, T3>(out T1 t)
            where T1 : class, IList<int>, new()
            where T2 : struct, ISet<int>
            where T3 : class, IList<int>, new();

        ref int Hello(ref string t);
        void Collection(ref int a, int b = 10, bool d = true | true ^ true, float f = 34, Color r = Color.B, params int[] p);
        void AA(string s = "\r\t\n\u6959" + @"hello	 {你好}""{哈哈} asdasdasd ");
        /// <summary>
        /// 对的对的
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        void BB<T>() where T : class;
    }
}
public

enum Color
{
    Red = 1, Green = 2, Blue = 4, Ambient = 8, Cyan = 16, DarkBlue = 32,
    A = 33,
    B = 33
}
#if NET10_0
[ExtensionSource]

interface IHello
{
    string GetSet { get; set; }
    string GetInit { get; init; }
    ref readonly string PropertyGet { get; }
    ref int this[in string index] { get; }
    void Hello();
    string Hello(string t);
    public void Hello<T1, T2, T3>(out T1 t)
        where T1 : class, IList<int>, new()
        where T2 : struct, ISet<int>
        where T3 : class, IList<int>, new();

    void Hello(ref string t, params int[] @params);

    void Collection(ref int a, int b = 10, string s = "hello\t" + @"  {你好}""{哈哈}  ", bool d = true | true ^ true, float f = 34, Color r = Color.Red, params int[] p);
}
#endif
#endif