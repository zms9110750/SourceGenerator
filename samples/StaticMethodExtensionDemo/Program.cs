using System;
using System.IO;
using zms9110750.Extensions.Generator.System;
using zms9110750.Extensions.Generator.System.IO;
using zms9110750.StaticMethodAsExtensionGenerator;

// ====================================================
// 演示: BCL 静态方法 → 实例扩展方法
// ====================================================

// 配置扫描范围：扫描 System.* 全部子命名空间
[assembly: StaticMethodExtensions(StaticMethodExtensionScope.SystemAll)]

// 生成器在编译时将 BCL 的 public static 方法（如 string.IsNullOrEmpty）
// 转化为实例扩展方法（如 "hello".IsNullOrEmpty()）

Console.WriteLine("=== StaticMethodAsExtensionGenerator 演示 ===");
Console.WriteLine();

// ---- String ----
Console.WriteLine("--- String 扩展 ---");
string text = "Hello, World!";
Console.WriteLine($"  IsNullOrEmpty:       \"{text}\".IsNullOrEmpty() = {text.IsNullOrEmpty()}");
Console.WriteLine($"  Concat:              \"Hello, \".Concat(\"World!\") = {"Hello, ".Concat("World!")}");
Console.WriteLine($"  IsNullOrWhiteSpace:  \"   \".IsNullOrWhiteSpace() = {"   ".IsNullOrWhiteSpace()}");

// ---- 数值类型 ----
Console.WriteLine("\n--- 数值扩展 ---");
Console.WriteLine($"  Int32Extensions.Abs(-5):        {Int32Extensions.Abs(-5)}");
Console.WriteLine($"  Int32Extensions.Clamp(10, 0, 5): {Int32Extensions.Clamp(10, 0, 5)}");
Console.WriteLine($"  DecimalExtensions.Add(1.5m, 2.3m): {DecimalExtensions.Add(1.5m, 2.3m)}");

// ---- Char ----
Console.WriteLine("\n--- Char 扩展 ---");
Console.WriteLine($"  'A'.IsDigit() =    {'A'.IsDigit()}");
Console.WriteLine($"  'A'.IsUpper() =    {'A'.IsUpper()}");
Console.WriteLine($"  '5'.IsDigit() =    {'5'.IsDigit()}");
Console.WriteLine($"  'a'.IsLower() =    {'a'.IsLower()}");

// ---- Stream ----
Console.WriteLine("\n--- Stream 扩展 ---");
using var ms = new MemoryStream();
Console.WriteLine($"  new MemoryStream().CanRead:     {ms.CanRead}");
Console.WriteLine($"  new MemoryStream().CanWrite:    {ms.CanWrite}");
Console.WriteLine($"  new MemoryStream().CanSeek:     {ms.CanSeek}");

// ---- Array ----
Console.WriteLine("\n--- Array 扩展 ---");
int[] numbers = [3, 1, 4, 1, 5, 9];
Console.WriteLine($"  ArrayExtensions.BinarySearch: {ArrayExtensions.BinarySearch(numbers, 4)}");

Console.WriteLine("\n所有演示完成!");

