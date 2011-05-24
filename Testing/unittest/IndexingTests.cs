using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test
{
    /// <summary>
    /// Summary description for IndexingTests
    /// </summary>
    [TestClass]
    public class IndexingTests
    {
        private Random _rnd = new Random();
        private IGraph _g;

        private IGraph EnsureTestData()
        {
            if (this._g == null)
            {
                this._g = new Graph();
                this._g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            }
            return this._g;
        }

        private void TestBinarySearch(IComparer<Triple> comparer, INode s, INode p, INode o, Func<IGraph,Triple,List<Triple>> getExpected)
        {
            IGraph g = this.EnsureTestData();

            ITripleFormatter formatter = new UncompressedNotation3Formatter();
            List<Triple> index = new List<Triple>(g.Triples);
            index.Sort(comparer);

            for (int i = 1; i <= 10000; i++)
            {
                Console.WriteLine("Run #" + i);
                Console.WriteLine();

                INode subj, pred, obj;
                if (s != null)
                {
                    subj = s;
                }
                else
                {
                    subj = g.Triples.Skip(this._rnd.Next(0, g.Triples.Count - 1)).First().Subject;
                }
                if (p != null)
                {
                    pred = p;
                }
                else
                {
                    pred = g.Triples.Skip(this._rnd.Next(0, g.Triples.Count - 1)).First().Predicate;
                }
                if (o != null)
                {
                    obj = o;
                }
                else
                {
                    obj = g.Triples.Skip(this._rnd.Next(0, g.Triples.Count - 1)).First().Object;
                }

                Triple search = new Triple(subj, pred, obj);
                Console.WriteLine("Searching For " + search.ToString(formatter));
                Console.WriteLine();

                List<Triple> expected = getExpected(g, search);
                expected.Sort();
                Console.WriteLine("Expecting " + expected.Count + " Triple(s)");

                List<Triple> actual = index.SearchIndex<Triple>(comparer, search).ToList();
                actual.Sort();
                Console.WriteLine("Got " + actual.Count + " Triple(s)");

                Graph x = new Graph();
                x.Assert(expected);
                Graph y = new Graph();
                y.Assert(actual);

                GraphDiffReport report = x.Difference(y);
                if (!report.AreEqual)
                {
                    Console.WriteLine("Run #" + i + " Failed!");

                    Console.WriteLine("Expected Triples:");
                    foreach (Triple t in x.Triples)
                    {
                        Console.WriteLine(t.ToString(formatter));
                    }
                    Console.WriteLine();

                    Console.WriteLine("Actual Triples:");
                    foreach (Triple t in y.Triples)
                    {
                        Console.WriteLine(t.ToString(formatter));
                    }

                    TestTools.ShowDifferences(report);
                }

                Assert.AreEqual(expected.Count, actual.Count, "Failed Count Check on Run #" + i);
                Assert.IsTrue(expected.All(t => actual.Contains(t)), "Failed Equality Check on Run #" + i);
                Console.WriteLine("Run #" + i + " Passed OK");

                Console.WriteLine();
            }
        }

        [TestMethod]
        public void IndexingBinarySearchSubject()
        {
            IGraph g = this.EnsureTestData();

            this.TestBinarySearch(new SComparer(), null, g.CreateVariableNode("p"), g.CreateVariableNode("o"), ((g2, t) => g2.GetTriplesWithSubject(t.Subject).ToList()));
        }

        [TestMethod]
        public void IndexingBinarySearchPredicate()
        {
            IGraph g = this.EnsureTestData();

            this.TestBinarySearch(new PComparer(), g.CreateVariableNode("s"), null, g.CreateVariableNode("o"), ((g2, t) => g2.GetTriplesWithPredicate(t.Predicate).ToList()));
        }

        [TestMethod]
        public void IndexingBinarySearchObject()
        {
            IGraph g = this.EnsureTestData();

            this.TestBinarySearch(new OComparer(), g.CreateVariableNode("s"), g.CreateVariableNode("p"), null, ((g2, t) => g2.GetTriplesWithObject(t.Object).ToList()));
        }

        [TestMethod]
        public void IndexingBinarySearchSubjectPredicate()
        {
            IGraph g = this.EnsureTestData();

            this.TestBinarySearch(new SPComparer(), null, null, g.CreateVariableNode("o"), ((g2, t) => g2.GetTriplesWithSubjectPredicate(t.Subject, t.Predicate).ToList()));
        }

        [TestMethod]
        public void IndexingBinarySearchSubjectObject()
        {
            IGraph g = this.EnsureTestData();

            this.TestBinarySearch(new OSComparer(), null, g.CreateVariableNode("p"), null, ((g2, t) => g2.GetTriplesWithSubjectObject(t.Subject, t.Object).ToList()));
        }

        [TestMethod]
        public void IndexingBinarySearchPredicateObject()
        {
            IGraph g = this.EnsureTestData();

            this.TestBinarySearch(new POComparer(), g.CreateVariableNode("s"), null, null, ((g2, t) => g2.GetTriplesWithPredicateObject(t.Predicate, t.Object).ToList()));
        }
    }
}
