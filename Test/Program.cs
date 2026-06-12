using System;
using zms9110750.Extensions.Generator.System;
using zms9110750.Extensions.Generator.System.IO;
using zms9110750.StaticMethodAsExtensionGenerator;

[assembly: StaticMethodExtensions(StaticMethodExtensionScope.SystemAll)]

Console.WriteLine($"string.IsNullOrEmpty: {"hello".IsNullOrEmpty()}");
Console.WriteLine($"int.Abs: {Int32Extensions.Abs(-5)}");
Console.WriteLine($"decimal.Add: {DecimalExtensions.Add(1.5m, 2.3m)}");
Console.WriteLine($"stream.CanRead: {new System.IO.MemoryStream().CanRead}");
Console.WriteLine("All OK!");
