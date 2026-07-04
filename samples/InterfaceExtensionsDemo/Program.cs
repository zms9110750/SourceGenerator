using System;
using zms9110750.InterfaceImplAsExtensionGenerator;
using zms9110750.Extensions.Generator.InterfaceExtensionsDemo;

namespace InterfaceExtensionsDemo;

/// <summary>定义接口，生成器会为其成员生成扩展方法</summary>
[ExtensionSource]
public interface ICalculator
{
    int Add(int a, int b);
    int Subtract(int a, int b);
    int Multiply(int a, int b);
    double Divide(int a, int b);
    string GetName();
}

/// <summary>实现类 — 显式实现接口成员</summary>
public class Calculator : ICalculator
{
    int ICalculator.Add(int a, int b) => a + b;
    int ICalculator.Subtract(int a, int b) => a - b;
    int ICalculator.Multiply(int a, int b) => a * b;
    double ICalculator.Divide(int a, int b) =>
        b == 0 ? throw new DivideByZeroException() : (double)a / b;
    string ICalculator.GetName() => "MyCalculator";
}

public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== 接口扩展方法演示 ===");
        Console.WriteLine();

        // 生成器将 ICalculator 的显式实现成员转为扩展方法
        ICalculator calc = new Calculator();

        Console.WriteLine($"  名称:       {calc.GetName()}");
        Console.WriteLine($"  10 + 3 =    {calc.Add(10, 3)}");
        Console.WriteLine($"  10 - 3 =    {calc.Subtract(10, 3)}");
        Console.WriteLine($"  10 × 3 =    {calc.Multiply(10, 3)}");
        Console.WriteLine($"  10 ÷ 3 =    {calc.Divide(10, 3):F2}");

        Console.WriteLine("\n所有演示完成!");
    }
}
