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
using System.Data;
using System.Linq;
using System.Text;
using System.Net;
using Xunit;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;
using VDS.RDF.XunitExtensions;
using VDS.RDF.Data.DataTables;

namespace VDS.RDF
{
    public partial class BasicTests2 : BaseTest
    {
        [SkippableFact]
        public void GraphEquality() {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing))
            {
                throw new SkipTestException("Test Config marks Remote Parsing as unavailable, test cannot be run");
            }

            try
            {
                Options.UriLoaderCaching = false;
                Console.WriteLine("Going to get two copies of a Graph from DBPedia and compare");
                Console.WriteLine("Using the DBPedia Graph for Barack Obama");

                Graph g = new Graph();
                Graph h = new Graph();
                Uri target = new Uri("http://dbpedia.org/resource/Barack_Obama");

                VDS.RDF.Parsing.UriLoader.Load(g, target);
                Console.WriteLine("Loaded first copy OK - " + g.Triples.Count + " Triples");
                VDS.RDF.Parsing.UriLoader.Load(h, target);
                Console.WriteLine("Loaded second copy OK - " + h.Triples.Count + " Triples");

                //Should have same Base Uri
                Assert.Equal(g.BaseUri, h.BaseUri);

                //Do equality check
                Console.WriteLine("Checking the Equality of the Graphs");
                //TestTools.CompareGraphs(g, h, true);
                Dictionary<INode, INode> mapping;
                bool equals = g.Equals(h, out mapping);
                Assert.True(equals, "Graphs should have been equal");
                if (mapping != null)
                {
                    Console.WriteLine("Blank Node Mapping was:");
                    foreach (KeyValuePair<INode, INode> pair in mapping)
                    {
                        Console.WriteLine(pair.Key.ToString() + " => " + pair.Value.ToString());
                    }
                }
                Console.WriteLine();

                //Get a third graph of something different
                Console.WriteLine("Going to get a third Graph of something different and check it is non-equal");
                Uri target2 = new Uri("http://dbpedia.org/resource/Nottingham");
                Graph i = new Graph();
                VDS.RDF.Parsing.UriLoader.Load(i, target2);

                //Should have different Base URIs and be non-equal
                Assert.Equal(g.BaseUri, i.BaseUri);
                Assert.Equal(h.BaseUri, i.BaseUri);
                Assert.False(g.Equals(i));
                Assert.False(h.Equals(i));
                //TestTools.CompareGraphs(g, i, false);
                //TestTools.CompareGraphs(h, i, false);

            }
            catch (WebException webEx)
            {
                TestTools.ReportError("Web Exception", webEx);
                Console.WriteLine();
                Console.WriteLine("Unable to retrieve the Graphs from the Web successfully!");
                throw new SkipTestException("Unable to retrieve the graphs from the web successfully.");
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [Fact]
        public void GraphSubGraphMatching()
        {
            Graph parent = new Graph();
            FileLoader.Load(parent, "resources\\InferenceTest.ttl");
            Graph subgraph = new Graph();
            subgraph.NamespaceMap.Import(parent.NamespaceMap);
            subgraph.Assert(parent.GetTriplesWithSubject(parent.CreateUriNode("eg:FordFiesta")));

            //Check method calls
            Dictionary<INode, INode> mapping;
            Console.WriteLine("Doing basic sub-graph matching with no BNode tests");
            Assert.True(parent.HasSubGraph(subgraph, out mapping), "Failed to match the sub-graph as expected");
            Assert.False(parent.IsSubGraphOf(subgraph, out mapping), "Parent should not be a sub-graph of the sub-graph");
            Assert.False(subgraph.HasSubGraph(parent, out mapping), "Sub-graph should not have parent as its sub-graph");
            Assert.True(subgraph.IsSubGraphOf(parent, out mapping), "Failed to match the sub-graph as expected");
            Console.WriteLine("OK");
            Console.WriteLine();

            //Add an extra triple into the Graph which will cause it to no longer be a sub-graph
            Console.WriteLine("Adding an extra Triple so the sub-graph is no longer such");
            subgraph.Assert(new Triple(subgraph.CreateUriNode("eg:Rocket"), subgraph.CreateUriNode("rdf:type"), subgraph.CreateUriNode("eg:AirVehicle")));
            Assert.False(parent.HasSubGraph(subgraph, out mapping), "Sub-graph should no longer be considered a sub-graph");
            Assert.False(subgraph.IsSubGraphOf(parent, out mapping), "Sub-graph should no longer be considered a sub-graph");
            Console.WriteLine("OK");
            Console.WriteLine();

            //Reset the sub-graph
            Console.WriteLine("Resetting the sub-graph");
            subgraph = new Graph();
            subgraph.NamespaceMap.Import(parent.NamespaceMap);
            subgraph.Assert(parent.GetTriplesWithSubject(parent.CreateUriNode("eg:FordFiesta")));
            Console.WriteLine("Adding additional information to the parent Graph, this should not affect the fact that the sub-graph is a sub-graph of it");
            Assert.True(parent.HasSubGraph(subgraph, out mapping), "Failed to match the sub-graph as expected");
            Assert.False(parent.IsSubGraphOf(subgraph, out mapping), "Parent should not be a sub-graph of the sub-graph");
            Assert.False(subgraph.HasSubGraph(parent, out mapping), "Sub-graph should not have parent as its sub-graph");
            Assert.True(subgraph.IsSubGraphOf(parent, out mapping), "Failed to match the sub-graph as expected");
            Console.WriteLine("OK");
            Console.WriteLine();

            //Remove stuff from parent graph so it won't match any more
            Console.WriteLine("Removing stuff from parent graph so that it won't have the sub-graph anymore");
            parent.Retract(parent.GetTriplesWithSubject(parent.CreateUriNode("eg:FordFiesta")).ToList());
            Assert.False(parent.HasSubGraph(subgraph, out mapping), "Parent should no longer contian the sub-graph");
            Assert.False(subgraph.IsSubGraphOf(parent, out mapping), "Parent should no longer contain the sub-graph");
            Console.WriteLine("OK");
            Console.WriteLine();
        }

        [Fact]
        public void GraphSubGraphMatchingWithBNodes()
        {
            Graph parent = new Graph();
            FileLoader.Load(parent, "resources\\Turtle.ttl");
            Graph subgraph = new Graph();
            subgraph.Assert(parent.Triples.Where(t => !t.IsGroundTriple));

            //Check method calls
            Dictionary<INode, INode> mapping;
            Console.WriteLine("Doing basic sub-graph matching with BNode tests");
            Assert.True(parent.HasSubGraph(subgraph, out mapping), "Failed to match the sub-graph as expected");
            Assert.False(parent.IsSubGraphOf(subgraph, out mapping), "Parent should not be a sub-graph of the sub-graph");
            Assert.False(subgraph.HasSubGraph(parent, out mapping), "Sub-graph should not have parent as its sub-graph");
            Assert.True(subgraph.IsSubGraphOf(parent, out mapping), "Failed to match the sub-graph as expected");
            Console.WriteLine("OK");
            Console.WriteLine();

            //Eliminate some of the Triples from the sub-graph
            Console.WriteLine("Eliminating some Triples from the sub-graph and seeing if the mapping still computes OK");
            subgraph.Retract(subgraph.Triples.Skip(2).Take(5).ToList());
            Assert.True(parent.HasSubGraph(subgraph, out mapping), "Failed to match the sub-graph as expected");
            Assert.False(parent.IsSubGraphOf(subgraph, out mapping), "Parent should not be a sub-graph of the sub-graph");
            Assert.False(subgraph.HasSubGraph(parent, out mapping), "Sub-graph should not have parent as its sub-graph");
            Assert.True(subgraph.IsSubGraphOf(parent, out mapping), "Failed to match the sub-graph as expected");
            Console.WriteLine("OK");
            Console.WriteLine();

            Console.WriteLine("Eliminating Blank Nodes from the parent Graph to check that the sub-graph is no longer considered as such afterwards");
            parent.Retract(parent.Triples.Where(t => !t.IsGroundTriple).ToList());
            Assert.False(parent.HasSubGraph(subgraph), "Sub-graph should no longer be considered as such");
            Assert.False(subgraph.IsSubGraphOf(parent), "Sub-graph should no longer be considered as such");

        }

        [SkippableFact]
        public void ParsingUriLoader()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing))
            {
                throw new SkipTestException("Test Config marks Remote Parsing as unavailable, test cannot be run");
            }

            int defaultTimeout = Options.UriLoaderTimeout;
            try
            {
                Options.UriLoaderCaching = false;
                Options.UriLoaderTimeout = 45000;

                List<Uri> testUris = new List<Uri>() {
                    new Uri("http://www.bbc.co.uk/programmes/b0080bbs#programme"),
                    new Uri("http://dbpedia.org/resource/Southampton"),
                    new Uri("file:///resources\\MergePart1.ttl"),
                    new Uri("http://www.dotnetrdf.org/configuration#")
                };

                Console.WriteLine("## URI Loader Test Suite");

                foreach (Uri u in testUris)
                {
                    Console.WriteLine("# Testing URI '" + u.AbsoluteUri + "'");

                    //Load the Test RDF
                    Graph g = new Graph();
                    Assert.NotNull(g);
                    VDS.RDF.Parsing.UriLoader.Load(g, u);

                    if (!u.IsFile)
                    {
                        Assert.Equal(u, g.BaseUri);
                    }

                    Console.WriteLine();

                    Console.WriteLine("Following Triples were generated");
                    foreach (Triple t in g.Triples)
                    {
                        Console.WriteLine(t.ToString());
                    }
                    Console.WriteLine();
                }

            }
            finally
            {
                Options.UriLoaderCaching = true;
                Options.UriLoaderTimeout = defaultTimeout;
            }
        }

        [Fact]
        public void GraphToDataTable()
        {
            Graph g = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");

            DataTable table = g.ToDataTable();

            Assert.Equal(g.Triples.Count, table.Rows.Count);
            Assert.Equal(3, table.Columns.Count);

            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    Console.Write(col.ColumnName + " = " + row[col].ToString() + " , ");
                }
                Console.WriteLine();
            }
        }

        [Fact]
        public void GraphToDataTable2()
        {
            Graph g = new Graph();

            DataTable table = g.ToDataTable();

            Assert.Equal(g.Triples.Count, table.Rows.Count);
            Assert.Equal(3, table.Columns.Count);

            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    Console.Write(col.ColumnName + " = " + row[col].ToString() + " , ");
                }
                Console.WriteLine();
            }
        }
    }
}
