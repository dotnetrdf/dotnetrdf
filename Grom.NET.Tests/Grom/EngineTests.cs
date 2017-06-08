namespace Grom.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Parsing;

    [TestClass]
    public class EngineTests
    {
        [TestMethod]
        public void Correct_number_of_nodes()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 0 .   # 1: Only subject is uri
:s1 :p1 0 .   # 1: Same triple as above
:s1 :p1 :s2 . # 2: Object is new uri
[ :p1 0 ] .   # 3
[ :p1 0 ] .   # 4: Anonymous blank nodes are distinct
_:b1 :p1 0 .  # 5: Subject is new blank
_:b1 :p1 0 .  # 5: Named blank nodes are the same
";

            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                var engine = new Engine(graph, new Uri("http://example.com/"));

                Assert.AreEqual(engine.Count(), 5);
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

                var s1GraphNode = new NodeFactory().CreateUriNode(new Uri("http://example.com/s1"));

                var s1GromNode = engine[s1GraphNode];

                Assert.IsNotNull(s1GromNode);
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

                var s1GraphNodeUri = new Uri("http://example.com/s1");
                var s1GromNode = engine[s1GraphNodeUri];

                Assert.IsNotNull(s1GromNode);
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

                var s1GraphNodeUriString = "http://example.com/s1";
                var s1GromNode = engine[s1GraphNodeUriString];

                Assert.IsNotNull(s1GromNode);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Cant_find_other_index()
        {
            using (var graph = new Graph())
            {
                dynamic engine = new Engine(graph, new Uri("http://example.com/"));

                var noNode = engine[DateTime.Now];
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Cant_get_by_illegal_string_index()
        {
            using (var graph = new Graph())
            {
                dynamic engine = new Engine(graph, new Uri("http://example.com/"));

                var noNode = engine["ASDF"];
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CantFindMissingUri()
        {
            using (var graph = new Graph())
            {
                dynamic engine = new Engine(graph, new Uri("http://example.com/"));

                var noNode = engine["http://example.com/WHATEVER"];
            }
        }

        [TestMethod]
        public void GetByName()
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

                Assert.IsNotNull(s1);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CantFindMissingName()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 0 .
";
            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                dynamic engine = new Engine(graph, new Uri("http://example.com/"));

                var s1GromNode = engine.FAKE;

                Assert.IsNotNull(s1GromNode);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Cant_find_by_name_without_prefix()
        {
            using (var graph = new Graph())
            {
                dynamic engine = new Engine(graph);

                var noNode = engine.x;
            }
        }

        [TestMethod]
        public void Get_dynamic_names_with_prefix()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 0 .                      # s1
:s1 :p1 0 .                      # s1 should only show once
:s1 :p1 :s2 .                    # s1 should only show once, s2 is new
<http://example2.com/s1> :p1 0 . # This cannot be made relative to prefix
[ :p1 0 ] .                      # Blanks don't show
_:b1 :p1 0 .                     # Named blanks don't show either
";
            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                var engine = new Engine(graph, new Uri("http://example.com/"));

                var actual = engine.GetDynamicMemberNames();
                var expected = new string[] {
                    "s1",
                    "s2",
                    "http://example2.com/s1"
                };

                CollectionAssert.AreEquivalent(expected.ToList(), actual.ToList());
            }
        }

        [TestMethod]
        public void Get_dynamic_names_without_prefix()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 0 .                      # s1
:s1 :p1 0 .                      # s1 should only show once
:s1 :p1 :s2 .                    # s1 should only show once, s2 is new
<http://example2.com/s1> :p1 0 . # This cannot be made relative to prefix
[ :p1 0 ] .                      # Blanks don't show
_:b1 :p1 0 .                     # Named blanks don't show either
";
            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                var engine = new Engine(graph);

                var actual = engine.GetDynamicMemberNames();
                var expected = new string[] {
                    "http://example.com/s1",
                    "http://example.com/s2",
                    "http://example2.com/s1"
                };

                CollectionAssert.AreEquivalent(expected.ToList(), actual.ToList());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Cant_get_by_multi_index()
        {
            using (var graph = new Graph())
            {
                dynamic engine = new Engine(graph);

                var noNode = engine[0, 0];
            }
        }

        [TestMethod]
        public void Is_enumerable()
        {
            var turtle = @"
@prefix : <http://example.com/> .

:s1 :p1 0 .
";
            using (var graph = new Graph())
            {
                StringParser.Parse(graph, turtle);

                IEnumerable engine = new Engine(graph, new Uri("http://example.com/"));
                var enumerator = engine.GetEnumerator();

                Assert.IsTrue(enumerator.MoveNext());
                Assert.IsNotNull(enumerator.Current);
                Assert.IsFalse(enumerator.MoveNext());
            }
        }
    }
}