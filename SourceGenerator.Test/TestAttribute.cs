using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using zms9110750.MetaSourceGenerator.AttributeFactory;

namespace zms9110750.SourceGenerator.Test
{
    public partial class Test
    {
        [FromAttributeData]
        public partial TestAttribute Creat(AttributeData attribute);
    }

    [FromAttributeData]
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public partial class TestAttribute : Attribute
    {
        public TestAttribute()
        {
        }
        public TestAttribute(Type type)
        {
        }
        internal TestAttribute(Type[] type)
        {
        }
        internal TestAttribute(INamedTypeSymbol type)
        {
        }
        public TestAttribute(INamedTypeSymbol[] type)
        {
        }

        public TestAttribute(int[] type)
        {
        }
        public TestAttribute(int age, string name)
        {
            Age = age;
            Name = name;
        }

        public int Age { get; set; } = 999;
        public string Name { get; set; } = "hello";
        public Type Type { get; set; }

        internal int ACE { get; set; }
        public Type[] Types { get; set; }
        public Color Color { get; set; }
        public override string ToString()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .OrderBy(p => p.Name);

            var propertyStrings = new List<string>();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(this);
                if (value == null)
                    continue;

                string valueString;

                if (prop.PropertyType.IsArray)
                {
                    var array = (Array)value;
                    var elements = new List<string>();
                    for (int i = 0; i < array.Length; i++)
                    {
                        elements.Add(array.GetValue(i)?.ToString() ?? "null");
                    }
                    valueString = $"[{string.Join(", ", elements)}]";
                }
                else if (value is string str)
                {
                    valueString = $"\"{str}\"";
                }
                else
                {
                    valueString = value.ToString();
                }

                propertyStrings.Add($"{prop.Name}={valueString}");

            }

            return $"[{GetType().FullName}({string.Join(", ", propertyStrings)})]";
        }
    }
}
