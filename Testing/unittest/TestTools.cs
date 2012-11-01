/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
