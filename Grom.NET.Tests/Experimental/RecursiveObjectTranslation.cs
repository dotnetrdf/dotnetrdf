namespace Experimental
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using VDS.RDF;
    using VDS.RDF.Writing;

    public class c
    {
        public string s { get; set; }
        public c c1 { get; set; }
    }

    [TestClass]
    public class RecursiveObjectTranslation
    {
        [TestMethod]
        public void MyTestMethod1()
        {
            var a = new c
            {
                s = "a",
                c1 = new c
                {
                    s = "b",
                    c1 = new c
                    {
                        s = "c"
                    }
                }
            };
            a.c1.c1.c1 = a;

            var g = new Graph();
            process(g, g.CreateUriNode(new Uri("http://example.com/a")), a);
            g.SaveToStream(Console.Out, new NTriplesWriter());
        }

        [TestMethod]
        public void MyTestMethod2()
        {
            var a = new c
            {
                s = "a"
            };
            a.c1 = a;

            var g = new Graph();
            process(g, g.CreateUriNode(new Uri("http://example.com/a")), a);
            g.SaveToStream(Console.Out, new NTriplesWriter());
        }

        private static readonly Random r = new Random();

        private static void process(IGraph g, INode subject, object value, Dictionary<object, INode> seen = null)
        {
            seen = seen ?? new Dictionary<object, INode>();

            seen[value] = subject;

            var properties = value.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .Where(p => !p.GetIndexParameters().Any());

            foreach (var property in properties)
            {
                var propertyValue = property.GetValue(value);

                if (propertyValue != null)
                {
                    var predicate = g.CreateUriNode(new Uri(new Uri("http://example.com/"), property.Name));
                    if (propertyValue is string || propertyValue.GetType().IsPrimitive)
                    {
                        var objectNode = g.CreateLiteralNode(propertyValue.ToString());
                        g.Assert(subject, predicate, objectNode);
                    }
                    else
                    {
                        if (seen.TryGetValue(propertyValue, out INode bnode))
                        {
                            g.Assert(subject, predicate, bnode);
                        }
                        else
                        {
                            bnode = seen[propertyValue] = g.CreateBlankNode();
                            g.Assert(subject, predicate, bnode);
                            process(g, bnode, propertyValue, seen);
                        }
                    }
                }
            }
        }
    }
}
