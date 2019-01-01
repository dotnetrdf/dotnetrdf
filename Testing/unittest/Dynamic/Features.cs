namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF.Parsing;
    using Xunit;

    public class Features
    {
        [Fact]
        public void Handles_nested_lists1()
        {
            var g = new Graph();
            g.LoadFromString(@"
@prefix : <urn:> .

:s :p (""o1"" (""o2"") ""o3"") .
");

            var s = g.CreateUriNode(":s");
            var p = g.CreateUriNode(":p");
            var r = g.GetTriplesWithSubjectPredicate(s, p).Single().Object;
            var l = new DynamicCollectionList(r);

            Assert.Contains("o1", l);
            Assert.Contains("o3", l);
            Assert.DoesNotContain("o2", l);
            Assert.IsAssignableFrom<IList<object>>(l[1]);
            Assert.Contains("o2", l[1] as IList<object>);
        }

        [Fact]
        public void Handles_nested_lists2()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
@prefix : <urn:> .

:s :p (""o1"" (""o2"") ""o3"") .
");

            var d = new DynamicGraph(null, UriFactory.Create("urn:"));
            d["s"] = new
            {
                p = new RdfCollection(
                    "o1" as object,
                    new RdfCollection(
                        "o2"
                    ),
                    "o3"
                )
            };

            Assert.Equal<IGraph>(expected, d);
        }

        [Fact]
        public void How_IGraph_HasSubraph_works_with_lists()
        {
            var factory = new NodeFactory();
            var n1 = factory.CreateUriNode(UriFactory.Create("urn:n1"));
            var n2 = factory.CreateUriNode(UriFactory.Create("urn:n2"));
            var n3 = factory.CreateUriNode(UriFactory.Create("urn:n3"));

            // same list items are same graph
            var g1 = new Graph();
            g1.AssertList(new[] {
                n1,
                n2,
                n3
            });

            var g2 = new Graph();
            g2.AssertList(new[] {
                n1,
                n2,
                n3
            });

            Assert.Equal(g1, g2);
            Assert.True(g1.HasSubGraph(g2));
            Assert.True(g2.IsSubGraphOf(g1));
            Assert.True(g2.HasSubGraph(g1));
            Assert.True(g1.IsSubGraphOf(g2));

            // subset of list items is not subgraph
            g1 = new Graph();
            g1.AssertList(new[] {
                n1
            });

            g2 = new Graph();
            g2.AssertList(new[] {
                n1,
                n2
            });

            Assert.NotEqual(g1, g2);
            Assert.False(g1.HasSubGraph(g2));
            Assert.False(g2.IsSubGraphOf(g1));
            Assert.False(g2.HasSubGraph(g1));
            Assert.False(g1.IsSubGraphOf(g2));

            // blank list items are different lists
            g1 = new Graph();
            g1.AssertList(new[] {
                g1.CreateBlankNode()
            });

            g2 = new Graph();
            g2.AssertList(new[] {
                g1.CreateBlankNode()
            });

            Assert.Equal(g1, g2);
            Assert.False(g1.HasSubGraph(g2));
            Assert.False(g2.IsSubGraphOf(g1));
            Assert.False(g2.HasSubGraph(g1));
            Assert.False(g1.IsSubGraphOf(g2));

            // trivial
            g1 = new Graph();
            g1.AssertList(new[] {
                n1,
                n2,
                n3
            });

            var listRoot = g1.GetTriplesWithPredicateObject(g1.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListFirst)), n1).Single().Subject;
            Assert.True(listRoot.IsListRoot(g1));

            g1.RetractList(listRoot);
            Assert.True(g1.IsEmpty);

            // can't retract from other graph
            g1 = new Graph();
            g1.AssertList(new[] {
                n1,
                n2,
                n3
            });

            g2 = new Graph();
            g2.AssertList(new[] {
                n1,
                n2,
                n3
            });

            listRoot = g2.GetTriplesWithPredicateObject(g1.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListFirst)), n1).Single().Subject;

            Assert.Throws<RdfException>(() => g1.RetractList(listRoot));
        }

        public IEnumerable<INode> FindListRoots(IGraph g, IEnumerable<INode> items)
        {
            return ListRoots(g).Where(n => SameList(n, items));
        }

        private bool SameList(INode root, IEnumerable<INode> items)
        {
            if (root.Graph is null)
            {
                throw new InvalidOperationException("Root node must have Graph");
            }

            var originalList = root.Graph.GetListItems(root).GetEnumerator();
            var list = items.GetEnumerator();

            while (true)
            {
                var originalListMoved = originalList.MoveNext();
                var listMoved = list.MoveNext();

                // different list lengths
                if (originalListMoved != listMoved)
                {
                    return false;
                }

                // both finished
                if (!originalListMoved)
                {
                    return true;
                }

                // items differ
                if (!originalList.Current.Equals(list.Current))
                {
                    return false;
                }
            }
        }

        public static IEnumerable<INode> ListRoots(IGraph g)
        {
            return g.Triples.SubjectNodes.BlankNodes().Where(n => n.IsListRoot(g));
        }
    }
}
