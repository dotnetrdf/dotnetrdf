namespace VDS.RDF.Dynamic.Experimental
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using VDS.RDF.Writing;

    public class Class1
    {
        public string SingleStringProperty { get; set; }

        public Class1 SingleClassProperty { get; set; }

        public IEnumerable<string> EnumerableStringProperty { get; set; }

        public IEnumerable<Class1> EnumerableClassProperty { get; set; }
    }

    [TestClass]
    public class RecursiveObjectTranslation
    {
        [TestMethod]
        public void MyTestMethod1()
        {
            var c1 = new Class1
            {
                SingleStringProperty = "c1",
                SingleClassProperty = new Class1
                {
                    SingleStringProperty = "c2",
                    SingleClassProperty = new Class1
                    {
                        SingleStringProperty = "c3"
                    }
                }
            };
            c1.SingleClassProperty.SingleClassProperty.SingleClassProperty = c1;

            var g = new Graph();
            process(g, g.CreateUriNode(new Uri("http://example.com/c1")), c1);
            g.SaveToStream(Console.Out, new NTriplesWriter());
        }

        [TestMethod]
        public void MyTestMethod2()
        {
            var c1 = new Class1
            {
                SingleStringProperty = "c1"
            };
            c1.SingleClassProperty = c1;

            var g = new Graph();
            process(g, g.CreateUriNode(new Uri("http://example.com/c1")), c1);
            g.SaveToStream(Console.Out, new NTriplesWriter());
        }

        [TestMethod]
        public void MyTestMethod3()
        {
            var c1 = new Class1
            {
                EnumerableStringProperty = new[] {
                    "s1",
                    "s2"
                }
            };

            var g = new Graph();
            process(g, g.CreateUriNode(new Uri("http://example.com/c1")), c1);
            g.SaveToStream(Console.Out, new NTriplesWriter());
        }

        [TestMethod]
        public void MyTestMethod4()
        {
            var c1 = new Class1
            {
                EnumerableClassProperty = new[] {
                    new Class1 {
                        SingleStringProperty = "s1"
                    },
                    new Class1 {
                        SingleStringProperty = "s2"
                    }
                }
            };

            var g = new Graph();
            process(g, g.CreateUriNode(new Uri("http://example.com/c1")), c1);
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

                    foreach (var x in Enumerate(propertyValue))
                    {
                        if (x is string || x.GetType().IsPrimitive)
                        {
                            var objectNode = g.CreateLiteralNode(x.ToString());
                            g.Assert(subject, predicate, objectNode);
                        }
                        else
                        {
                            if (!seen.TryGetValue(x, out var bnode))
                            {
                                bnode = seen[x] = g.CreateBlankNode();
                                process(g, bnode, x, seen);
                            }

                            g.Assert(subject, predicate, bnode);
                        }
                    }
                }
            }
        }

        private static IEnumerable<object> Enumerate(object value)
        {
            if (value is string || !(value is IEnumerable enumerableValue)) // Strings are enumerable but not for our case
            {
                enumerableValue = value.AsEnumerable(); // When they're not enumerable, wrap them in an enumerable of one
            }

            foreach (var item in enumerableValue)
            {
                yield return item;
            }
        }
    }
}
