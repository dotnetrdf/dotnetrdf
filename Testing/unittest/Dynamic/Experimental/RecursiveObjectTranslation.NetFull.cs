/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

namespace VDS.RDF.Dynamic.Experimental
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using VDS.RDF.Writing;
    using Xunit;
    using Xunit.Abstractions;

    public class Class1
    {
        public string SingleStringProperty { get; set; }

        public Class1 SingleClassProperty { get; set; }

        public IEnumerable<string> EnumerableStringProperty { get; set; }

        public IEnumerable<Class1> EnumerableClassProperty { get; set; }
    }

    public class RecursiveObjectTranslation
    {
        private readonly ITestOutputHelper output;

        public RecursiveObjectTranslation(ITestOutputHelper output)
        {
            this.output = output;
        }

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

            this.output.WriteLine(StringWriter.Write(g, new NTriplesWriter()));
        }

        public void MyTestMethod2()
        {
            var c1 = new Class1
            {
                SingleStringProperty = "c1"
            };
            c1.SingleClassProperty = c1;

            var g = new Graph();
            process(g, g.CreateUriNode(new Uri("http://example.com/c1")), c1);
            this.output.WriteLine(StringWriter.Write(g, new NTriplesWriter()));
        }

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
            this.output.WriteLine(StringWriter.Write(g, new NTriplesWriter()));
        }

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
            this.output.WriteLine(StringWriter.Write(g, new NTriplesWriter()));
        }

        private static readonly Random r = new Random();

        private static void process(IGraph g, INode subject, object value, Dictionary<object, INode> seen = null)
        {
            seen = seen ?? new Dictionary<object, INode>();

            seen[value] = subject;

            var properties = value.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !p.GetIndexParameters().Any());

            foreach (var property in properties)
            {
                var propertyValue = property.GetValue(value);

                if (propertyValue != null)
                {
                    // TODO: Proper predicate conversion
                    var predicate = g.CreateUriNode(new Uri(new Uri("http://example.com/"), property.Name));

                    foreach (var instance in Enumerate(propertyValue))
                    {
                        if (instance is string || instance.GetType().IsPrimitive)
                        {
                            // TODO: Proper object conversion
                            var literalNode = g.CreateLiteralNode(instance.ToString());
                            g.Assert(subject, predicate, literalNode);
                        }
                        else
                        {
                            if (!seen.TryGetValue(instance, out var blankNode))
                            {
                                blankNode = seen[instance] = g.CreateBlankNode();
                                process(g, blankNode, instance, seen);
                            }

                            g.Assert(subject, predicate, blankNode);
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
