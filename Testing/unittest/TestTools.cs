using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Test
{
    public class TestTools
    {
        public static void ReportError(String title, Exception ex, bool fail)
        {
            Console.WriteLine(title);
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);

            if (ex.InnerException != null)
            {
                ReportError("Inner Exception", ex.InnerException, fail);
            }

            if (fail)
            {
                Assert.Fail("An Exception occurred in the Test");
            }
        }

        public static void CompareNodes(UriNode a, UriNode b, bool expectEquality)
        {
            Console.WriteLine("URI Node A has String form: " + a.ToString());
            Console.WriteLine("URI Node B has String form: " + b.ToString());
            Console.WriteLine();
            Console.WriteLine("URI Node A has Hash Code: " + a.GetHashCode());
            Console.WriteLine("URI Node B has Hash Code: " + b.GetHashCode());
            Console.WriteLine();
            Console.WriteLine("Nodes are Equal? " + a.Equals(b));
            Console.WriteLine("Hash Codes are Equal? " + a.GetHashCode().Equals(b.GetHashCode()));
            Console.WriteLine();

            if (expectEquality)
            {
                Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
                Assert.AreEqual(a, b);
            }
            else
            {
                Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
                Assert.AreNotEqual(a, b);
            }
        }

        public static void CompareGraphs(IGraph g, IGraph h, bool expectEquality)
        {
            if (expectEquality)
            {
                //Triple Counts must be identical
                Assert.AreEqual(g.Triples.Count, h.Triples.Count);

                //Each Triple in g must be in h
                foreach (Triple t in g.Triples)
                {
                    Assert.IsTrue(h.Triples.Contains(t), "Second Graph must contain Triple " + t.ToString());
                }
            }
            else
            {
                if (g.Triples.Count != h.Triples.Count)
                {
                    //Different number of Triples so must be non-equal
                    //We know this Assertion should succeed based on our previous IF but should Assert anyway
                    Assert.AreNotEqual(g.Triples.Count, h.Triples.Count, "Two non-equivalent Graphs should have different numbers of Triples");
                }
                else
                {
                    //Not every Triple in g is in h and not every Triple in h is in g
                    Assert.IsFalse(g.Triples.All(t => h.Triples.Contains(t)) && h.Triples.All(t => g.Triples.Contains(t)), "Graphs contain the same Triples when they were expected to be different");
                }
            }
        }

        public static void ShowResults(Object results)
        {
            if (results is Graph)
            {
                ShowGraph((Graph)results);
            }
            else if (results is SparqlResultSet)
            {
                SparqlResultSet resultSet = (SparqlResultSet)results;
                Console.WriteLine("Result: " + resultSet.Result);
                Console.WriteLine(resultSet.Results.Count + " Results");
                foreach (SparqlResult r in resultSet.Results)
                {
                    Console.WriteLine(r.ToString());
                }
            }
            else
            {
                throw new ArgumentException("Expected a Graph or a SPARQLResultSet");
            }
        }

        public static void ShowGraph(IGraph g)
        {
            Console.Write("Graph URI: ");
            if (g.BaseUri != null)
            {
                Console.WriteLine(g.BaseUri.ToString());
            }
            else
            {
                Console.WriteLine("NULL");
            }
            Console.WriteLine(g.Triples.Count + " Triples");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }
        }

        public static void WarningPrinter(String message)
        {
            Console.WriteLine(message);
        }
    }
}
