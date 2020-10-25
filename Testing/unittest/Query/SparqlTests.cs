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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using FluentAssertions;
using Xunit;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;
using VDS.RDF.Update;
using VDS.RDF.Writing;

namespace VDS.RDF.Query
{

    public class SparqlTests : IClassFixture<MockRemoteSparqlEndpointFixture>
    {
        private MockRemoteSparqlEndpointFixture _serverFixture;

        public SparqlTests(MockRemoteSparqlEndpointFixture serverFixture)
        {
            _serverFixture = serverFixture;
        }
        private object ExecuteQuery(IInMemoryQueryableStore store, string query)
        {
            var parser = new SparqlQueryParser();
            var q = parser.ParseFromString(query);
            var processor = new LeviathanQueryProcessor(store);
            return processor.ProcessQuery(q);
        }

        [Fact]
        public void SparqlJoinWithoutVars1()
        {
            var data = @"<http://s> <http://p> <http://o> .
<http://x> <http://y> <http://z> .";

            var g = new Graph();
            g.LoadFromString(data, new NTriplesParser());

            var results = g.ExecuteQuery("ASK WHERE { <http://s> <http://p> <http://o> . <http://x> <http://y> <http://z> }") as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(SparqlResultsType.Boolean, results.ResultsType);
            Assert.True(results.Result);
        }

        [Fact]
        public void SparqlJoinWithoutVars2()
        {
            var data = @"<http://s> <http://p> <http://o> .
<http://x> <http://y> <http://z> .";

            var g = new Graph();
            g.LoadFromString(data, new NTriplesParser());
            g.BaseUri = new Uri("http://example/graph");

            var results = g.ExecuteQuery("ASK WHERE { GRAPH <http://example/graph> { <http://s> <http://p> <http://o> . <http://x> <http://y> <http://z> } }") as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(SparqlResultsType.Boolean, results.ResultsType);
            Assert.True(results.Result);
        }

        [Fact]
        public void SparqlJoinWithoutVars3()
        {
            var data = @"<http://s> <http://p> <http://o> .
<http://x> <http://y> <http://z> .";

            var g = new Graph();
            g.LoadFromString(data, new NTriplesParser());
            g.BaseUri = new Uri("http://example/graph");

            var results = g.ExecuteQuery("SELECT * WHERE { GRAPH <http://example/graph> { <http://s> <http://p> <http://o> . <http://x> <http://y> <http://z> } }") as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
            Assert.Single(results.Results);
            Assert.Empty(results.Variables);
        }

        [Fact]
        public void SparqlParameterizedStringWithNulls()
        {
            var query = new SparqlParameterizedString
            {
                CommandText = "SELECT * WHERE { @s ?p ?o }"
            };

            //Set a URI to a valid value
            query.SetUri("s", new Uri("http://example.org"));
            Console.WriteLine(query.ToString());
            Console.WriteLine();

            //Set the parameter to a null
            query.SetParameter("s", null);
            Console.WriteLine(query.ToString());
        }

        [Fact]
        public void SparqlParameterizedStringWithNulls2()
        {
            var query = new SparqlParameterizedString
            {
                CommandText = "SELECT * WHERE { @s ?p ?o }"
            };

            //Set a URI to a valid value
            query.SetUri("s", new Uri("http://example.org"));
            Console.WriteLine(query.ToString());
            Console.WriteLine();

            //Set the URI to a null
            Assert.Throws<ArgumentNullException>(() => query.SetUri("s", null));
        }

        // TODO: Determine if this really is a problem that we should even be testing for rather than just documenting as a "feature"
        // The issue is that %2f is automatically decoded by the Uri class when you get the AbsolutePath property. A possible fix
        // is to use the OriginalString property in the BaseFormatter class instead, but this then breaks several other tests that
        // are testing for standards adherence unless we create a derived formatter specifically for this one use case.
        [Fact(Skip = "Not supported by the Uri class")]
        public void SparqlParameterizedStringShouldNotDecodeEncodedCharactersInUri()
        {
            var query = new SparqlParameterizedString("DESCRIBE @uri");
            query.SetUri("uri", new Uri("http://example.com/some%40encoded%2furi"));
            Assert.Equal("DESCRIBE <http://example.com/some%40encoded%2furi>", query.ToString());
        }

        [Fact]
        public void SparqlParameterizedStringShouldNotEncodeUri()
        {
            var query = new SparqlParameterizedString("DESCRIBE @uri");
            query.SetUri("uri", new Uri("http://example.com/some@encoded/uri"));
            Assert.Equal("DESCRIBE <http://example.com/some@encoded/uri>", query.ToString());
        }

        [Fact]
        public void SparqlResultSetEquality()
        {
            var parser = new SparqlXmlParser();
            var rdfparser = new SparqlRdfParser();
            var a = new SparqlResultSet();
            var b = new SparqlResultSet();

            parser.Load(a, "resources\\list-3.srx");
            parser.Load(b, "resources\\list-3.srx.out");

            a.Trim();
            b.Trim();
            Assert.True(a.Equals(b));

            a = new SparqlResultSet();
            b = new SparqlResultSet();
            parser.Load(a, "resources\\no-distinct-opt.srx");
            parser.Load(b, "resources\\no-distinct-opt.srx.out");

            a.Trim();
            b.Trim();
            Assert.True(a.Equals(b));

            a = new SparqlResultSet();
            b = new SparqlResultSet();
            rdfparser.Load(a, "resources\\result-opt-3.ttl");
            parser.Load(b, "resources\\result-opt-3.ttl.out");

            a.Trim();
            b.Trim();
            Assert.True(a.Equals(b));
        }

        [Fact]
        public void SparqlJsonResultSet()
        {
            Console.WriteLine("Tests that JSON Parser parses language specifiers correctly");

            var query = "PREFIX rdfs: <" + NamespaceMapper.RDFS + ">\nSELECT DISTINCT ?comment WHERE {?s rdfs:comment ?comment}";

            var store = new TripleStore();
            var g = new Graph();
            FileLoader.Load(g, "resources\\json.owl");
            store.Add(g);

            var results = ExecuteQuery(store, query);
            Assert.IsAssignableFrom<SparqlResultSet>(results);
            if (results is SparqlResultSet)
            {
                var rset = (SparqlResultSet)results;

                //Serialize to both XML and JSON Results format
                var xmlwriter = new SparqlXmlWriter();
                xmlwriter.Save(rset, "results.xml");
                var jsonwriter = new SparqlJsonWriter();
                jsonwriter.Save(rset, "results.json");

                //Read both back in
                var xmlparser = new SparqlXmlParser();
                var r1 = new SparqlResultSet();
                xmlparser.Load(r1, "results.xml");
                Console.WriteLine("Result Set after XML serialization and reparsing contains:");
                foreach (SparqlResult r in r1)
                {
                    Console.WriteLine(r.ToString());
                }
                Console.WriteLine();

                var jsonparser = new SparqlJsonParser();
                var r2 = new SparqlResultSet();
                jsonparser.Load(r2, "results.json");
                Console.WriteLine("Result Set after JSON serialization and reparsing contains:");
                foreach (SparqlResult r in r2)
                {
                    Console.WriteLine(r.ToString());
                }
                Console.WriteLine();

                Assert.Equal(r1, r2);

                Console.WriteLine("Result Sets were equal as expected");
            }
        }

        [Fact]
        public void SparqlInjection()
        {
            var baseQuery = @"PREFIX ex: <http://example.org/Vehicles/>
SELECT * WHERE {
    ?s a @type .
}";

            var query = new SparqlParameterizedString(baseQuery);
            var parser = new SparqlQueryParser();

            var injections = new List<string>()
            {
                "ex:Car ; ex:Speed ?speed",
                "ex:Plane ; ?prop ?value",
                "ex:Car . EXISTS {?s ex:Speed ?speed}",
                "ex:Car \u0022; ex:Speed ?speed"
            };

            foreach (var value in injections)
            {
                query.SetLiteral("type", value, false);
                query.SetLiteral("@type", value, false);
                Console.WriteLine(query.ToString());
                Console.WriteLine();

                SparqlQuery q = parser.ParseFromString(query.ToString());
                Assert.Single(q.Variables);
            }
        }

        [Fact]
        public void SparqlConflictingParamNames()
        {
            var baseQuery = @"SELECT * WHERE {
    ?s a @type ; a @type1 ; a @type2 .
}";
            var query = new SparqlParameterizedString(baseQuery);
            var parser = new SparqlQueryParser();

            query.SetUri("type", new Uri("http://example.org/type"));

            Console.WriteLine("Only one of the type parameters should be replaced here");
            Console.WriteLine(query.ToString());
            Console.WriteLine();

            query.SetUri("type1", new Uri("http://example.org/anotherType"));
            query.SetUri("type2", new Uri("http://example.org/yetAnotherType"));

            Console.WriteLine("Now all the type parameters should have been replaced");
            Console.WriteLine(query.ToString());
            Console.WriteLine();

            Assert.Throws<FormatException>(() =>
            {
                query.SetUri("not valid", new Uri("http://example.org/invalidParam"));
            });

            query.SetUri("is_valid", new Uri("http://example.org/validParam"));
        }

        [Fact]
        public void SparqlParameterizedString()
        {
            var test = @"INSERT DATA { GRAPH @graph {
                            <http://uri> <http://uri> <http://uri>
                            }}";
            var cmdString = new SparqlParameterizedString(test);
            cmdString.SetUri("graph", new Uri("http://example.org/graph"));

            var parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString(cmdString);
            cmds.ToString();
        }

        [Fact]
        public void SparqlEndpointWithExtensions()
        {
            //var endpoint = new SparqlConnector(new Uri(TestConfigManager.GetSetting(TestConfigManager.RemoteSparqlQuery)));
            var testQuery = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE {?s rdfs:label ?label . ?label bif:contains " + "\"London\" } LIMIT 1";

            _serverFixture.RegisterSelectQueryGetHandler(testQuery);
            var endpoint = new SparqlConnector(new Uri(_serverFixture.Server.Urls[0] + "/sparql"));

            Assert.Throws<RdfException>(() =>
            {
                var r = endpoint.Query(testQuery);
            });

            endpoint.SkipLocalParsing = true;

            var results = endpoint.Query(testQuery);
            results.Should().BeOfType<SparqlResultSet>().Which.Results.Should().NotBeEmpty();
            //TestTools.ShowResults(results);
        }

        [Fact]
        public void SparqlBNodeIDsInResults()
        {
            var xmlparser = new SparqlXmlParser();
            var results = new SparqlResultSet();
            xmlparser.Load(results, "resources\\bnodes.srx");

            TestTools.ShowResults(results);
            Assert.Single(results.Results.Distinct());

            var jsonparser = new SparqlJsonParser();
            results = new SparqlResultSet();
            jsonparser.Load(results, "resources\\bnodes.json");

            TestTools.ShowResults(results);
            Assert.Single(results.Results.Distinct());
        }

        [Fact]
        public void SparqlAnton()
        {
            var g = new Graph();
            FileLoader.Load(g, "resources\\anton.rdf");

            var parser = new SparqlQueryParser();
            SparqlQuery query = parser.ParseFromFile("resources\\anton.rq");

            var results = g.ExecuteQuery(query);
            Assert.IsAssignableFrom<SparqlResultSet>(results);
            if (results is SparqlResultSet)
            {
                var rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r);
                }
                Assert.Equal(1, rset.Count);
            }
        }

        [Fact]
        public void SparqlBindMultiple()
        {
            const string sourceGraphTurtle = @"<urn:cell:cell23>
        <urn:coords:X> 2 ;
        <urn:coords:Y> 3 .

<urn:cell:cell33>
        <urn:coords:X> 3 ;
        <urn:coords:Y> 3 .

<urn:cell:cell34>
        <urn:coords:X> 3 ;
        <urn:coords:Y> 4 .";
            const string query = @"PREFIX fn: <http://www.w3.org/2005/xpath-functions#>
CONSTRUCT
{
   ?newCell <urn:cell:neighbourOf> ?cell .
   ?newCell <urn:coords:X> ?newX .
   ?newCell <urn:coords:Y> ?newY .
}
WHERE
{
    ?cell <urn:coords:X> ?x .
    ?cell <urn:coords:Y> ?y .
    BIND(1 + ?x as ?newX) .
    BIND(1 + ?y as ?newY) .
    BIND(IRI(fn:concat(""urn:cell:cell"", str(1 + ?x), str(1 + ?y))) as
?newCell) .
}";
            const string expectedGraphTurtle = @"<urn:cell:cell34>       <urn:cell:neighbourOf>  <urn:cell:cell23> .
<urn:cell:cell34>       <urn:coords:X>  ""3""^^<http://www.w3.org/2001/XMLSchema#integer> .
<urn:cell:cell34>       <urn:coords:Y>  ""4""^^<http://www.w3.org/2001/XMLSchema#integer> .
<urn:cell:cell44>       <urn:cell:neighbourOf>  <urn:cell:cell33> .
<urn:cell:cell44>       <urn:coords:X>  ""4""^^<http://www.w3.org/2001/XMLSchema#integer> .
<urn:cell:cell44>       <urn:coords:Y>  ""4""^^<http://www.w3.org/2001/XMLSchema#integer> .
<urn:cell:cell45>       <urn:cell:neighbourOf>  <urn:cell:cell34> .
<urn:cell:cell45>       <urn:coords:X>  ""4""^^<http://www.w3.org/2001/XMLSchema#integer> .
<urn:cell:cell45>       <urn:coords:Y>  ""5""^^<http://www.w3.org/2001/XMLSchema#integer> .";

            IGraph sourceGraph = new Graph();
            sourceGraph.LoadFromString(sourceGraphTurtle, new TurtleParser());

            IGraph expectedGraph = new Graph();
            expectedGraph.LoadFromString(expectedGraphTurtle);

            for (var i = 0; i < 10; i++)
            {
                var actualGraph = (IGraph)sourceGraph.ExecuteQuery(query);
                Assert.True(expectedGraph.Difference(actualGraph).AreEqual);
            }
        }

        [Fact]
        public void SparqlSimpleQuery1()
        {
            var store = new TripleStore();
            FileLoader.Load(store, Path.Combine("resources", "czech-royals.ttl"));
            const string sparqlQuery = "SELECT * WHERE {?s ?p ?o}";
            var sparqlParser = new SparqlQueryParser();
            var query = sparqlParser.ParseFromString(sparqlQuery);
            var processor = new LeviathanQueryProcessor(store);
            var results = processor.ProcessQuery(query);
        }

        private readonly String[] _langSpecCaseQueries = new string[]
                {
                    @"SELECT * WHERE { ?s ?p 'example'@en-gb }",
                    @"SELECT * WHERE { ?s ?p 'example'@en-GB }",
                    @"SELECT * WHERE { ?s ?p 'example'@EN-GB }",
                    @"SELECT * WHERE { ?s ?p 'example'@EN-gb }",
                    @"SELECT * WHERE { ?s ?p 'example'@en-GB }",
                    @"SELECT * WHERE { ?s ?p ?o . FILTER(LANGMATCHES(LANG(?o), 'en-gb')) }",
                    @"SELECT * WHERE { ?s ?p ?o . FILTER(LANGMATCHES(LANG(?o), 'en-GB')) }",
                    @"SELECT * WHERE { ?s ?p ?o . FILTER(LANGMATCHES(LANG(?o), 'EN-GB')) }",
                    @"SELECT * WHERE { ?s ?p ?o . FILTER(LANGMATCHES(LANG(?o), 'EN-gb')) }",
                    @"SELECT * WHERE { ?s ?p ?o . FILTER(LANGMATCHES(LANG(?o), 'en')) }",
                    @"SELECT * WHERE { ?s ?p ?o . FILTER(LANGMATCHES(LANG(?o), 'EN')) }",
                    @"SELECT * WHERE { ?s ?p ?o . FILTER(LANG(?o) = 'en-gb') }"
                };

        private void TestLanguageSpecifierCase(IGraph g)
        {
            foreach (var query in _langSpecCaseQueries)
            {
                Console.WriteLine("Checking query:\n" + query);
                var results = g.ExecuteQuery(query) as SparqlResultSet;
                Assert.NotNull(results);
                Assert.Equal(1, results.Count);
                Console.WriteLine();
            }
        }

        [Fact]
        public void SparqlLanguageSpecifierCase1()
        {
            IGraph g = new Graph();
            ILiteralNode lit = g.CreateLiteralNode("example", "en-gb");
            INode s = g.CreateBlankNode();
            INode p = g.CreateUriNode(UriFactory.Create("http://predicate"));

            g.Assert(s, p, lit);
            TestLanguageSpecifierCase(g);
        }

        [Fact]
        public void SparqlLanguageSpecifierCase2()
        {
            IGraph g = new Graph();
            ILiteralNode lit = g.CreateLiteralNode("example", "en-GB");
            INode s = g.CreateBlankNode();
            INode p = g.CreateUriNode(UriFactory.Create("http://predicate"));

            g.Assert(s, p, lit);
            TestLanguageSpecifierCase(g);
        }

        [Fact]
        public void SparqlLanguageSpecifierCase3()
        {
            IGraph g = new Graph();
            ILiteralNode lit = g.CreateLiteralNode("example", "EN-gb");
            INode s = g.CreateBlankNode();
            INode p = g.CreateUriNode(UriFactory.Create("http://predicate"));

            g.Assert(s, p, lit);
            TestLanguageSpecifierCase(g);
        }

        [Fact]
        public void SparqlLanguageSpecifierCase4()
        {
            IGraph g = new Graph();
            ILiteralNode lit = g.CreateLiteralNode("example", "EN-GB");
            INode s = g.CreateBlankNode();
            INode p = g.CreateUriNode(UriFactory.Create("http://predicate"));

            g.Assert(s, p, lit);
            TestLanguageSpecifierCase(g);
        }

        [Fact]
        public void SparqlConstructEmptyWhereCore407()
        {
            const String queryStr = @"CONSTRUCT
{
<http://s> <http://p> <http://o> .
}
WHERE
{}";

            SparqlQuery q = new SparqlQueryParser().ParseFromString(queryStr);
            var dataset = new InMemoryDataset();
            var processor = new LeviathanQueryProcessor(dataset);

            var g = processor.ProcessQuery(q) as IGraph;
            Assert.NotNull(g);
            Assert.False(g.IsEmpty, "Graph should not be empty");
            Assert.Equal(1, g.Triples.Count);
        }

        [Fact]
        public void SparqlConstructFromSubqueryWithLimitCore420()
        {
            const string queryStr = @"CONSTRUCT
{
  ?s ?p ?o .
} WHERE {
  ?s ?p ?o .
  {
    SELECT ?s WHERE {
      ?s a <http://xmlns.com/foaf/0.1/Person> .
    } LIMIT 3
  }
}";
            SparqlQuery q = new SparqlQueryParser().ParseFromString(queryStr);
            IGraph g = new Graph();
            g.NamespaceMap.AddNamespace("rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"));
            g.NamespaceMap.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));
            var rdfType = g.CreateUriNode("rdf:type");
            var person = g.CreateUriNode("foaf:Person");
            var name = g.CreateUriNode("foaf:name");
            var age = g.CreateUriNode("foaf:age");
            for (var i = 0; i < 3; i++)
            {
                // Create 3 statements for each instance of foaf:Person (including rdf:type statement)
                var s = g.CreateUriNode(new Uri("http://example.com/people/" + i));
                g.Assert(new Triple(s, rdfType, person));
                g.Assert(new Triple(s, name, g.CreateLiteralNode("Person " + i)));
                g.Assert(new Triple(s, age, g.CreateLiteralNode((20 + i).ToString(CultureInfo.InvariantCulture))));
            }

            //ISparqlQueryProcessor processor = new ExplainQueryProcessor(new InMemoryDataset(g), ExplanationLevel.ShowAll | ExplanationLevel.AnalyseAll | ExplanationLevel.OutputToConsoleStdOut);
            var processor = new LeviathanQueryProcessor(new InMemoryDataset(g));
            var resultGraph = processor.ProcessQuery(q) as IGraph;

            Assert.NotNull(resultGraph);
            Assert.Equal(9, resultGraph.Triples.Count); // Returns 3 rather than 9
        }

        [Fact]
        public void SparqlParameterizedStringDefaultPrefixOrder()
        {
            // given
            const string queryString = @"
PREFIX : <http://id.ukpds.org/schema/>
PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
CONSTRUCT *
WHERE {
    ?s ?p ?o
}";

            // when
            var query = new SparqlParameterizedString(queryString);

            // then
            query.Namespaces.Prefixes.Should().HaveCount(2);
            query.Namespaces.GetNamespaceUri(string.Empty).Should().Be(new Uri("http://id.ukpds.org/schema/"));
        }


        [Theory]
        [InlineData("en")]
        [InlineData("de")]
        public void SparqlMathWithCultureVariation(string culture)
        {
            TestTools.ExecuteWithChangedCulture(new CultureInfo(culture), SparqlMathTest);
        }

        private void SparqlMathTest()
        {
            const string update = @"
DELETE { ?s ?p ?o. }
INSERT { ?s ?p ?a. }
WHERE  { ?s ?p ?o. BIND(?o / 2 AS ?a) }
";
            const string query = @"SELECT * WHERE { ?s ?p ?o.}";

            var graph = new Graph();
            graph.Assert(graph.CreateBlankNode(), graph.CreateBlankNode(), 0.5.ToLiteral(graph));
            Assert.Equal(0.5, graph.Triples.Select(t=>t.Object.AsValuedNode().AsDouble()).First());

            var store = new TripleStore();
            store.Add(graph);

            var queryProcessor = new LeviathanQueryProcessor(store);
            var queryParser = new SparqlQueryParser();
            var q = queryParser.ParseFromString(query);
            var result = q.Process(queryProcessor) as SparqlResultSet;
            var oNode = result.First()["o"];
            Assert.Equal("0.5", ((ILiteralNode)oNode).Value);

            q = queryParser.ParseFromString("SELECT ?a WHERE { ?s ?p ?o. BIND(?o / 2 AS ?a) }");
            //q = queryParser.ParseFromString("SELECT ?a WHERE { BIND (0.25 as ?a) }");
            result = q.Process(queryProcessor) as SparqlResultSet;
            oNode = result.First()["a"];
            var o = oNode.AsValuedNode().AsDouble();
            Assert.Equal(0.25, o);
            Assert.Equal("0.25", ((ILiteralNode)oNode).Value);


            var updateProcessor = new LeviathanUpdateProcessor(store);
            var updateParser = new SparqlUpdateParser();
            updateParser.ParseFromString(update).Process(updateProcessor);

            result = queryParser.ParseFromString(query).Process(queryProcessor) as SparqlResultSet;
            oNode = result.First()["o"];
            //Assert.Equal("0.25", ((ILiteralNode)oNode).Value);
            o = oNode.AsValuedNode().AsDouble();
            Assert.Equal(0.25, o);
            //@object is 25 when using German (Germany) regional format
            //@object is 0.25 when using English (United Kingdom) regional format
        }
    }
}
