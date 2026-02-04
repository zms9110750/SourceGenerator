

#if true
using Microsoft.CodeAnalysis;
using System;
using System.ComponentModel;
using System.Numerics;
using Test;
using zms9110750.SourceGenerator.Test;
namespace Test
{
    //  [zms9110750.SourceGenerator.Shared.AttributeFactory.FromAttributeDataAttribute]
    [DescriptionAttribute]

    class Class6 : Attribute
    {
        public int Age { get; set; }
        public int[] Ages { get; set; }
        public string Name { get; set; }
        public string[] Names { get; set; }
        public string[][] Namess { get; set; }
        public ABC Abc { get; set; }

        public Random Random { get; set; }
        public Class6(int i) { }
        public Class6() { }
        public Class6(int i, int b) { }
        public Class6(Random i, string b, BigInteger bigInteger) { }
        public Class6(string op) { }
        public Class6(string[] op) { }
        public Class6(Type[] op) { }
        public Class6(Random i) { }
        public Class6(bool? b) { }
        public Class6(bool b) { }
    }
}

class ABC
{

    [Test(Age = 6)]
    [Test(Name = "小明", Types = [typeof(int), typeof(string), typeof(Type)])]
    [Test(4, "wocao", Type = typeof(int))]
    [Test(Color = Color.Yelloow | Color.Orange)]
    public void Hello() { }
}


[Class6(12, Age = 6, Name = "age")]
partial class BBC
{


};


namespace APP.VV
{
    namespace AP
    {

        static partial class BBC
        {
            /*
           [Testu(Age = 6)]
           [Testu(Name = "小明")]
           public static partial Class6 Creat(AttributeData data);*/
        };

    }
}
#endif