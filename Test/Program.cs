using System;
using System.Reflection;

Console.WriteLine("=== Enum 静态方法 ===");
foreach (var m in typeof(Enum).GetMethods(BindingFlags.Public | BindingFlags.Static))
{
    var ps = m.GetParameters();
    if (ps.Length > 0 && ps[0].ParameterType == typeof(Enum))
        Console.WriteLine($"  {m.Name}({string.Join(", ", ps.Select(p => p.ParameterType.Name))})");
}
if (!typeof(Enum).GetMethods(BindingFlags.Public | BindingFlags.Static)
    .Any(m => m.GetParameters().Length > 0 && m.GetParameters()[0].ParameterType == typeof(Enum)))
    Console.WriteLine("  (无 —— 第一个参数都不是 Enum 自身)");

Console.WriteLine("\n=== ValueType 静态方法 ===");
foreach (var m in typeof(ValueType).GetMethods(BindingFlags.Public | BindingFlags.Static))
{
    var ps = m.GetParameters();
    if (ps.Length > 0 && ps[0].ParameterType == typeof(ValueType))
        Console.WriteLine($"  {m.Name}({string.Join(", ", ps.Select(p => p.ParameterType.Name))})");
}
if (!typeof(ValueType).GetMethods(BindingFlags.Public | BindingFlags.Static)
    .Any(m => m.GetParameters().Length > 0 && m.GetParameters()[0].ParameterType == typeof(ValueType)))
    Console.WriteLine("  (无 —— 第一个参数都不是 ValueType 自身)");
