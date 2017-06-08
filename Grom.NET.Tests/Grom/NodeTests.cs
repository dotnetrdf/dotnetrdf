namespace Grom.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;
    using System.Reflection;
    using VDS.RDF;
    using VDS.RDF.Parsing;

    [TestClass]
    public class NodeTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Cant_find_other_index()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 0 .
";

            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));
                dynamic s1 = engine.s1;

                var value = s1[DateTime.Now];
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Cant_get_by_multi_index()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 0 .
";

            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));
                dynamic s1 = engine.s1;

                var value = s1[0, 0];
            }
        }

        [TestMethod]
        public void Get_by_node_index()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 0 .
";
            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));
                dynamic s1 = engine.s1;

                var prtedicateNode = graph.Triples.First().Predicate;

                var zeroes = s1[prtedicateNode];

                Assert.IsNotNull(zeroes);
            }
        }

        [TestMethod]
        public void Get_by_string_index()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 0 .
";
            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));
                dynamic s1 = engine.s1;

                var prtedicateUriString = "http://example.com/p1";

                var zeroes = s1[prtedicateUriString];

                Assert.IsNotNull(zeroes);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Cant_find_by_name_without_prefix()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 0 .
";
            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                var engine = new Engine(graph);
                dynamic s1 = engine.First();

                var noNode = s1.p1;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Cant_get_by_illegal_string_index()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 0 .
";
            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));
                dynamic s1 = engine.s1;

                var zeroes = s1["ASDF"];
            }
        }

        [TestMethod]
        public void Get_by_uri_index()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 0 .
";
            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));
                dynamic s1 = engine.s1;

                var predicateUri = (graph.Triples.First().Predicate as IUriNode).Uri;

                var zeroes = s1[predicateUri];

                Assert.IsNotNull(zeroes);
            }
        }

        [TestMethod]
        public void Instances_are_the_same()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 :s1 .
";

            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));
                dynamic s1 = engine.s1;

                var value = s1.p1[0];

                Assert.AreSame(s1, value);
            }
        }

        [TestMethod]
        public void Instances_are_the_same_longer()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 :s2 .
:s2 :p2 :s1 .
";

            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));

                dynamic s1 = engine.s1;
                dynamic s2 = s1.p1[0];
                dynamic otherS1 = s2.p2[0];

                Assert.AreSame(s1, otherS1);
            }
        }

        [TestMethod]
        public void Get_dynamic_names_with_prefix()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 0 . # p1
:s1 :p1 0 . # p1 should only appear once
:s1 :p1 1 . # Still only once
:s1 :p2 0 . # p2
:s1 a :s2 . # This cannot be made relative to prefix
";
            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));
                var s1 = engine.s1 as Node;

                var actual = s1.GetDynamicMemberNames();
                var expected = new string[] {
                    "p1",
                    "p2",
                    "http://www.w3.org/1999/02/22-rdf-syntax-ns#type"
                };

                CollectionAssert.AreEquivalent(expected.ToList(), actual.ToList());
            }
        }

        [TestMethod]
        public void Get_dynamic_names_without_prefix()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 0 . # p1
:s1 :p1 0 . # p1 should only appear once
:s1 :p1 1 . # Still only once
:s1 :p2 0 . # p2
:s1 a :s2 . # This cannot be made relative to prefix
";
            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph);
                var s1 = engine["http://example.com/s1"] as Node;

                var actual = s1.GetDynamicMemberNames();
                var expected = new string[] {
                    "http://example.com/p1",
                    "http://example.com/p2",
                    "http://www.w3.org/1999/02/22-rdf-syntax-ns#type"
                };

                CollectionAssert.AreEquivalent(expected.ToList(), actual.ToList());
            }
        }

        [TestMethod]
        public void Correct_type_for_int()
        {
            var turtle = @"
@prefix : <http://example.com/> .
@prefix xsd: <http://www.w3.org/2001/XMLSchema#> .

:s1 :p1 ""0""^^xsd:int .
";

            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));

                dynamic s1 = engine.s1;
                dynamic p1 = s1.p1[0];

                Assert.IsInstanceOfType(p1, typeof(int));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Unrecognized_node_cannot_be_converted()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 0 .
";

            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));

                var s1 = engine.s1;

                var node = new PrivateObject(s1);

                try
                {
                    node.Invoke("Convert", graph.CreateVariableNode("variable"));
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }
        }

        [TestMethod]
        public void Correct_type_for_unknown()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 """"^^:unknown .
";

            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));

                dynamic s1 = engine.s1;
                dynamic p1 = s1.p1[0];

                Assert.IsInstanceOfType(p1, typeof(string));
            }
        }

        [TestMethod]
        public void Correct_type_for_invalid()
        {
            var turtle = @"
@prefix : <http://example.com/> .
@prefix xsd: <http://www.w3.org/2001/XMLSchema#> .

:s1 :p1 ""a""^^xsd:int .
:s1 :p1 ""a""^^xsd:integer .
:s1 :p1 ""a""^^xsd:date .
:s1 :p1 ""a""^^xsd:boolean .
:s1 :p1 ""a""^^xsd:long .
:s1 :p1 ""a""^^xsd:decimal .
:s1 :p1 ""a""^^xsd:double .
";

            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));

                dynamic s1 = engine.s1;
                var p1 = s1.p1;

                Assert.IsNotInstanceOfType(p1[0], typeof(int));
                Assert.IsNotInstanceOfType(p1[1], typeof(int));
                Assert.IsNotInstanceOfType(p1[2], typeof(DateTime));
                Assert.IsNotInstanceOfType(p1[3], typeof(bool));
                Assert.IsNotInstanceOfType(p1[4], typeof(long));
                Assert.IsNotInstanceOfType(p1[5], typeof(decimal));
                Assert.IsNotInstanceOfType(p1[6], typeof(double));
            }
        }

        [TestMethod]
        public void Correct_type_for_string()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 """" .
";

            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));

                dynamic s1 = engine.s1;
                dynamic p1 = s1.p1[0];

                Assert.IsInstanceOfType(p1, typeof(string));
            }
        }

        [TestMethod]
        public void Correct_type_for_integer()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 0 .
";

            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));

                dynamic s1 = engine.s1;
                dynamic p1 = s1.p1[0];

                Assert.IsInstanceOfType(p1, typeof(int));
            }
        }

        [TestMethod]
        public void Correct_type_for_long()
        {
            var turtle = @"
@prefix : <http://example.com/> .
@prefix xsd: <http://www.w3.org/2001/XMLSchema#> .

:s1 :p1 ""0""^^xsd:long .
";

            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));

                dynamic s1 = engine.s1;
                dynamic p1 = s1.p1[0];

                Assert.IsInstanceOfType(p1, typeof(long));
            }
        }

        [TestMethod]
        public void Decimal_is_correct_type()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 4.5 .
";

            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));

                dynamic s1 = engine.s1;
                dynamic p1 = s1.p1[0];

                Assert.IsInstanceOfType(p1, typeof(decimal));
            }
        }

        [TestMethod]
        public void Correct_type_for_double()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 4E5 .
";

            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));

                dynamic s1 = engine.s1;
                dynamic p1 = s1.p1[0];

                Assert.IsInstanceOfType(p1, typeof(double));
            }
        }

        [TestMethod]
        public void Correct_type_for_bool()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 true .
";

            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));

                dynamic s1 = engine.s1;
                dynamic p1 = s1.p1[0];

                Assert.IsInstanceOfType(p1, typeof(bool));
            }
        }

        [TestMethod]
        public void Correct_type_for_datetime()
        {
            var turtle = @"
@prefix : <http://example.com/> .
@prefix xsd: <http://www.w3.org/2001/XMLSchema#> .

:s1 :p1 ""01-01-1900""^^xsd:date.
";

            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));

                dynamic s1 = engine.s1;
                dynamic p1 = s1.p1[0];

                Assert.IsInstanceOfType(p1, typeof(DateTime));
            }
        }
    }
}