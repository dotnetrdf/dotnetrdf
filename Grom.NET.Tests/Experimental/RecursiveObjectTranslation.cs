namespace Experimental
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class c
    {
        public string s { get; set; }
        public c c1 { get; set; }
    }

    [TestClass]
    public class RecursiveObjectTranslation
    {
        [TestMethod]
        public void MyTestMethod()
        {
            var a = new c { s = "a" };
            var b = new c { s = "b", c1 = a };
            a.c1 = a;

            process("b", a);
        }

        private static readonly Random r = new Random();

        private void process(object subject, object value, Dictionary<object, string> seen = null)
        {
            seen = seen ?? new Dictionary<object, string>();

            var properties = value.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .Where(p => !p.GetIndexParameters().Any());

            foreach (var property in properties)
            {
                var x = property.GetValue(value);

                if (x != null)
                {
                    if (x is string || x.GetType().IsPrimitive)
                    {
                        Console.WriteLine("{0} {1} {2}", subject, property.Name, x);
                    }
                    else
                    {
                        if (seen.TryGetValue(x, out string bnode))
                        {
                            Console.WriteLine("{0} {1} {2}", subject, property.Name, bnode);
                        }
                        else
                        {
                            bnode = seen[x] = r.Next().ToString();
                            Console.WriteLine("{0} {1} {2}", subject, property.Name, bnode);
                            process(bnode, x, seen);
                        }
                    }
                }
            }
        }
    }
}
