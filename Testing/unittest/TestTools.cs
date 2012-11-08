/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test
{
    public class TestTools
    {
        public static void ReportError(String title, Exception ex)
        {
            Console.WriteLine(title);
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);

            if (ex.InnerException != null)
            {
                ReportError("Inner Exception", ex.InnerException);
            }
        }

        public static void CompareNodes(IUriNode a, IUriNode b, bool expectEquality)
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
            if (results is IGraph)
            {
                ShowGraph((IGraph)results);
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
                throw new ArgumentException("Expected a Graph or a SparqlResultSet");
            }
        }

        public static void ShowMultiset(BaseMultiset multiset)
        {
            if (multiset is NullMultiset)
            {
                Console.WriteLine("NULL");
            }
            else if (multiset is IdentityMultiset)
            {
                Console.WriteLine("IDENTITY");
            }
            else
            {
                foreach (ISet s in multiset.Sets)
                {
                    Console.WriteLine(s.ToString());
                }
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
            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
        }

        public static void ShowDifferences(GraphDiffReport report)
        {
            NTriplesFormatter formatter = new NTriplesFormatter();

            if (report.AreEqual)
            {
                Console.WriteLine("Graphs are Equal");
                Console.WriteLine();
                Console.WriteLine("Blank Node Mapping between Graphs:");
                foreach (KeyValuePair<INode, INode> kvp in report.Mapping)
                {
                    Console.WriteLine(kvp.Key.ToString(formatter) + " => " + kvp.Value.ToString(formatter));
                }
            }
            else
            {
                Console.WriteLine("Graphs are non-equal");
                Console.WriteLine();
                Console.WriteLine("Triples added to 1st Graph to give 2nd Graph:");
                foreach (Triple t in report.AddedTriples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }
                Console.WriteLine();
                Console.WriteLine("Triples removed from 1st Graph to given 2nd Graph:");
                foreach (Triple t in report.RemovedTriples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }
                Console.WriteLine();
                Console.WriteLine("Blank Node Mapping between Graphs:");
                foreach (KeyValuePair<INode, INode> kvp in report.Mapping)
                {
                    Console.WriteLine(kvp.Key.ToString(formatter) + " => " + kvp.Value.ToString(formatter));
                }
                Console.WriteLine();
                Console.WriteLine("MSGs added to 1st Graph to give 2nd Graph:");
                foreach (IGraph msg in report.AddedMSGs)
                {
                    foreach (Triple t in msg.Triples)
                    {
                        Console.WriteLine(t.ToString(formatter));
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.WriteLine("MSGs removed from 1st Graph to give 2nd Graph:");
                foreach (IGraph msg in report.RemovedMSGs)
                {
                    foreach (Triple t in msg.Triples)
                    {
                        Console.WriteLine(t.ToString(formatter));
                    }
                    Console.WriteLine();
                }
            }
        }

        public static void ShowMapping(GraphDiffReport report)
        {
            NTriplesFormatter formatter = new NTriplesFormatter();
            Console.WriteLine("Blank Node Mapping between Graphs:");
            foreach (KeyValuePair<INode, INode> kvp in report.Mapping)
            {
                Console.WriteLine(kvp.Key.ToString(formatter) + " => " + kvp.Value.ToString(formatter));
            }
        }

        public static void WarningPrinter(String message)
        {
            Console.WriteLine(message);
        }

        public static void TestInMTAThread(ThreadStart info)
        {
                Thread t = new Thread(info);
                t.SetApartmentState(ApartmentState.MTA);
                t.Start();
                t.Join();
        }

        public static void PrintEnumerable<T>(IEnumerable<T> items, String sep)
            where T : class
        {
            bool first = true;
            foreach (T item in items)
            {
                if (!first)
                {
                    Console.Write(sep);
                }
                else
                {
                    first = false;
                }
                Console.Write((item != null ? item.ToString() : String.Empty));
            }
        }

        public static void PrintEnumerableStruct<T>(IEnumerable<T> items, String sep)
            where T : struct
        {
            bool first = true;
            foreach (T item in items)
            {
                if (!first)
                {
                    Console.Write(sep);
                }
                else
                {
                    first = false;
                }
                Console.Write(item.ToString());
            }
        }
    }
}
