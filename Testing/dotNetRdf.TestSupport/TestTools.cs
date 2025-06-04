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
using System.Globalization;
using System.Linq;
using System.Threading;
using VDS.RDF.Parsing;
using Xunit;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF;

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
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
            Assert.Equal(a, b);
        }
        else
        {
            Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
            Assert.NotEqual(a, b);
        }
    }

    public static void CompareGraphs(IGraph g, IGraph h, bool expectEquality)
    {
        if (expectEquality)
        {
            //Triple Counts must be identical
            Assert.Equal(g.Triples.Count, h.Triples.Count);

            //Each Triple in g must be in h
            foreach (Triple t in g.Triples)
            {
                Assert.True(h.Triples.Contains(t), "Second Graph must contain Triple " + t.ToString());
            }
        }
        else
        {
            if (g.Triples.Count != h.Triples.Count)
            {
                //Different number of Triples so must be non-equal
                //We know this Assertion should succeed based on our previous IF but should Assert anyway
                Assert.Equal(g.Triples.Count, h.Triples.Count);
            }
            else
            {
                //Not every Triple in g is in h and not every Triple in h is in g
                Assert.False(g.Triples.All(t => h.Triples.Contains(t)) && h.Triples.All(t => g.Triples.Contains(t)), "Graphs contain the same Triples when they were expected to be different");
            }
        }
    }

    public static void ShowResults(Object results, ITestOutputHelper output = null)
    {
        if (output == null) return;
        if (results is IGraph graph)
        {
            ShowGraph(graph);
        }
        else if (results is SparqlResultSet resultSet)
        {
            output.WriteLine("Result: " + resultSet.Result);
            output.WriteLine(resultSet.Results.Count + " Results");
            foreach (SparqlResult r in resultSet.Results)
            {
                output.WriteLine(r.ToString());
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
        try
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
            var formatter = new NTriplesFormatter();
            foreach (Triple t in g.Triples.ToList())
            {
                Console.WriteLine(t.ToString(formatter));
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            // Weird error that happens on the AppVeyor CI build
            Console.WriteLine("ArgumentOutOfRangeException raused while formatting RDF graph");
        }
    }

    public static void AssertIsomorphic(IGraph expectGraph, IGraph actualGraph, ITestOutputHelper outputHelper)
    {
        GraphDiffReport diffs = expectGraph.Difference(actualGraph);
        if (!diffs.AreEqual)
        {
            ShowDifferences(diffs, "expected", "actual", outputHelper);
            Assert.Fail("Graphs are not isomorphic.");
        }
    }

    public static void ShowGraph(IGraph g, ITestOutputHelper _output)
    {
        try
        {
            if (g.BaseUri != null)
            {
                _output.WriteLine("Graph Name: " + g.Name.ToString());
            }
            else
            {
                _output.WriteLine("Graph Name: NULL");
            }
            _output.WriteLine(g.Triples.Count + " Triples");
            var formatter = new NTriplesFormatter();
            foreach (Triple t in g.Triples.ToList())
            {
                _output.WriteLine(t.ToString(formatter));
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            // Weird error that happens on the AppVeyor CI build
            _output.WriteLine("ArgumentOutOfRangeException raised while formatting RDF graph");
        }
    }

    public static void ShowDifferences(GraphDiffReport report)
    {
        ShowDifferences(report, "1st Graph", "2nd Graph");
    }

    public static void ShowDifferences(GraphDiffReport report, ITestOutputHelper output)
    {
        ShowDifferences(report, "1st Graph", "2nd Graph", output);
    }

    public static void ShowDifferences(GraphDiffReport report, String lhsName, String rhsName)
    {
        var formatter = new NTriplesFormatter(NTriplesSyntax.Rdf11Star);
        lhsName = String.IsNullOrEmpty(lhsName) ? "1st Graph" : lhsName;
        rhsName = String.IsNullOrEmpty(rhsName) ? "2nd Graph" : rhsName;

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
            Console.WriteLine("Triples added to " + lhsName + " to give " + rhsName + ":");
            foreach (Triple t in report.AddedTriples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
            Console.WriteLine();
            Console.WriteLine("Triples removed from " + lhsName + " to give " + rhsName + ":");
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
            if (report.AddedMSGs.Any())
            {
                Console.WriteLine("MSGs added to " + lhsName + " to give " + rhsName + ":");
                foreach (IGraph msg in report.AddedMSGs)
                {
                    Console.WriteLine(msg.Triples.Count + " Triple(s):");
                    foreach (Triple t in msg.Triples)
                    {
                        Console.WriteLine(t.ToString(formatter));
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
            if (report.RemovedMSGs.Any())
            {
                Console.WriteLine("MSGs removed from " + lhsName + " to give " + rhsName + ":");
                foreach (IGraph msg in report.RemovedMSGs)
                {
                    Console.WriteLine(msg.Triples.Count + " Triple(s):");
                    foreach (Triple t in msg.Triples)
                    {
                        Console.WriteLine(t.ToString(formatter));
                    }
                    Console.WriteLine();
                }
            }
        }
    }
    public static void ShowDifferences(GraphDiffReport report, String lhsName, String rhsName, ITestOutputHelper output)
    {
        var formatter = new NTriplesFormatter(NTriplesSyntax.Rdf11Star);
        lhsName = String.IsNullOrEmpty(lhsName) ? "1st Graph" : lhsName;
        rhsName = String.IsNullOrEmpty(rhsName) ? "2nd Graph" : rhsName;

        if (report.AreEqual)
        {
            output.WriteLine("Graphs are Equal");
            output.WriteLine();
            output.WriteLine("Blank Node Mapping between Graphs:");
            foreach (KeyValuePair<INode, INode> kvp in report.Mapping)
            {
                output.WriteLine(kvp.Key.ToString(formatter) + " => " + kvp.Value.ToString(formatter));
            }
        }
        else
        {
            output.WriteLine("Graphs are non-equal");
            output.WriteLine();
            output.WriteLine("Triples added to " + lhsName + " to give " + rhsName + ":");
            foreach (Triple t in report.AddedTriples)
            {
                output.WriteLine(t.ToString(formatter));
            }
            output.WriteLine();
            output.WriteLine("Triples removed from " + lhsName + " to give " + rhsName + ":");
            foreach (Triple t in report.RemovedTriples)
            {
                output.WriteLine(t.ToString(formatter));
            }
            output.WriteLine();
            output.WriteLine("Blank Node Mapping between Graphs:");
            foreach (KeyValuePair<INode, INode> kvp in report.Mapping)
            {
                output.WriteLine(kvp.Key.ToString(formatter) + " => " + kvp.Value.ToString(formatter));
            }
            output.WriteLine();
            if (report.AddedMSGs.Any())
            {
                output.WriteLine("MSGs added to " + lhsName + " to give " + rhsName + ":");
                foreach (IGraph msg in report.AddedMSGs)
                {
                    output.WriteLine(msg.Triples.Count + " Triple(s):");
                    foreach (Triple t in msg.Triples)
                    {
                        output.WriteLine(t.ToString(formatter));
                    }
                    output.WriteLine();
                }
                output.WriteLine();
            }
            if (report.RemovedMSGs.Any())
            {
                output.WriteLine("MSGs removed from " + lhsName + " to give " + rhsName + ":");
                foreach (IGraph msg in report.RemovedMSGs)
                {
                    output.WriteLine(msg.Triples.Count + " Triple(s):");
                    foreach (Triple t in msg.Triples)
                    {
                        output.WriteLine(t.ToString(formatter));
                    }
                    output.WriteLine();
                }
            }
        }
    }

    public static void ShowMapping(GraphDiffReport report)
    {
        var formatter = new NTriplesFormatter();
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

    /// <summary>
    /// Taken from StackOverflow: http://stackoverflow.com/questions/5983779/catch-exception-that-is-thrown-in-different-thread
    /// </summary>
    /// <param name="test">Test action</param>
    /// <param name="exception">Exception if thrown</param>
    private static void ThreadExecute(Action test, out Exception exception)
    {
        exception = null;

        try
        {
            test();
        }
        catch (Exception ex)
        {
            exception = ex;
        }
    }

    public static void PrintEnumerable<T>(IEnumerable<T> items, String sep)
        where T : class
    {
        var first = true;
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
        var first = true;
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

    /// <summary>
    /// Runs a test at a specified recursive depth
    /// </summary>
    /// <param name="i">Recursive depth</param>
    /// <param name="test">Test</param>
    public static void RunAtDepth(int i, Action test)
    {
        if (i > 0)
        {
            RunAtDepth(i - 1, test);
        }
        else
        {
            test();
        }
    }

    public static void TestInMTAThread(Action action)
    {
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform
                .Windows))
        {
            Exception ex = null;
            var t = new Thread(() => ThreadExecute(action, out ex));
            t.SetApartmentState(ApartmentState.MTA);
            t.Start();
            t.Join();
            if (ex != null) throw ex;
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
    }

    public static void ExecuteWithChangedCulture(CultureInfo cultureInfoOverride, Action test)
    {
        CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = cultureInfoOverride;

        try
        {
            test();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = currentCulture;
        }
    }

    public static void AssertEqual(ITripleStore expected, ITripleStore actual, ITestOutputHelper output)
    {
        var bnodeMap = new Dictionary<INode, INode>();

        foreach (IGraph expectGraph in expected.Graphs)
        {
            if (expectGraph.Name is IBlankNode)
            {
                // Has to match one unmapped graph in actual
                var match = actual.Graphs.Where(g => g.Name is IBlankNode && !bnodeMap.ContainsKey(g.Name))
                    .Select(candidateGraph => new {name=candidateGraph.Name, diffReport=candidateGraph.Difference(expectGraph)})
                    .FirstOrDefault(x => x.diffReport.AreEqual);
                Assert.True(match != null, $"Could not find a graph in the actual store that matches blank node named graph {expectGraph.Name} in the expected store.");
                bnodeMap[match.name] = expectGraph.Name;
            }
            else
            {
                Assert.True(actual.Graphs.Contains(expectGraph.Name));
                IGraph actualGraph = actual.Graphs[expectGraph.Name];
                GraphDiffReport diffReport = actualGraph.Difference(expectGraph);
                if (!diffReport.AreEqual) ShowDifferences(diffReport, output);
                Assert.True(diffReport.AreEqual);
            }
        }

        foreach (IGraph actualGraph in actual.Graphs)
        {
            Assert.True((actualGraph.Name != null && bnodeMap.ContainsKey(actualGraph.Name)) || 
                        expected.Graphs.Contains(actualGraph.Name) || actualGraph.IsEmpty, 
                $"Found an unexpected graph {actualGraph.Name} in the store after update.");
        }
    }
}