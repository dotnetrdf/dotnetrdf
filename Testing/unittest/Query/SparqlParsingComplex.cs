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
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update;

using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{

    public class SparqlParsingComplex
    {
        [Fact]
        public void SparqlParsingNestedGraphPatternFirstItem()
        {
                SparqlQueryParser parser = new SparqlQueryParser();
                SparqlQuery q = parser.ParseFromFile("resources\\childgraphpattern.rq");

                Console.WriteLine(q.ToString());
                Console.WriteLine();
                Console.WriteLine(q.ToAlgebra().ToString());
        }

        [Fact]
        public void SparqlParsingNestedGraphPatternFirstItem2()
        {
                SparqlQueryParser parser = new SparqlQueryParser();
                SparqlQuery q = parser.ParseFromFile("resources\\childgraphpattern2.rq");

                Console.WriteLine(q.ToString());
                Console.WriteLine();
                Console.WriteLine(q.ToAlgebra().ToString());
         }

        [Fact]
        public void SparqlParsingSubQueryWithLimitAndOrderBy()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");

            String query = "SELECT * WHERE { { SELECT * WHERE {?s ?p ?o} ORDER BY ?p ?o LIMIT 2 } }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);

                Assert.True(rset.All(r => r.HasValue("s") && r.HasValue("p") && r.HasValue("o")), "All Results should have had ?s, ?p and ?o variables");
            }
            else
            {
                Assert.True(false, "Expected a SPARQL Result Set");
            }
        }

        //[Fact]
        //public void SparqlNonNormalizedUris()
        //{
        //    try
        //    {
        //        //Options.UriNormalization = false;

        //        SparqlQueryParser parser = new SparqlQueryParser();
        //        SparqlRdfParser rdfParser = new SparqlRdfParser(new TurtleParser());

        //        foreach (String file in Directory.GetFiles(Environment.CurrentDirectory))
        //        {
        //            if (Path.GetFileName(file).StartsWith("normalization") && Path.GetExtension(file).Equals(".ttl") && !Path.GetFileName(file).EndsWith("-results.ttl"))
        //            {
        //                QueryableGraph g = new QueryableGraph();
        //                FileLoader.Load(g, file);

        //                Console.WriteLine("Testing " + Path.GetFileName(file));

        //                SparqlQuery query = parser.ParseFromFile(Path.GetFileNameWithoutExtension(file) + ".rq");

        //                Object results = g.ExecuteQuery(query);
        //                if (results is SparqlResultSet)
        //                {
        //                    SparqlResultSet rset = (SparqlResultSet)results;

        //                    SparqlResultSet expected = new SparqlResultSet();
        //                    rdfParser.Load(expected, Path.GetFileNameWithoutExtension(file) + "-results.ttl");

        //                    if (!rset.Equals(expected))
        //                    {
        //                        Console.WriteLine("Expected Results");
        //                        Console.WriteLine();
        //                        foreach (SparqlResult r in expected)
        //                        {
        //                            Console.WriteLine(r.ToString());
        //                        }
        //                        Console.WriteLine();
        //                        Console.WriteLine("Actual Results");
        //                        Console.WriteLine();
        //                        foreach (SparqlResult r in rset)
        //                        {
        //                            Console.WriteLine(r.ToString());
        //                        }
        //                        Console.WriteLine();
        //                    }
        //                    Assert.Equal(rset, expected);
        //                }
        //                else
        //                {
        //                    Assert.True(false, "Didn't get a SPARQL Result Set as expected");
        //                }
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        //Options.UriNormalization = true;
        //    }
        //}

        [Fact]
        public void SparqlParsingDescribeHangingWhere()
        {
            List<String> valid = new List<string>()
            {
                "DESCRIBE ?s WHERE { ?s a ?type }",
                "DESCRIBE <http://example.org/>",
                "PREFIX ex: <http://example.org/> DESCRIBE ex:"
            };

            List<String> invalid = new List<string>()
            {
                "DESCRIBE ?s WHERE"
            };

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlFormatter formatter = new SparqlFormatter();
            foreach (String v in valid)
            {
                    SparqlQuery q = parser.ParseFromString(v);
                    Console.WriteLine(formatter.Format(q));
                    Console.WriteLine();
            }

            foreach (String iv in invalid)
            {
                try
                {
                    SparqlQuery q = parser.ParseFromString(iv);
                    Assert.True(false, "Should have thrown a Parsing Error");
                }
                catch (RdfParseException parseEx)
                {
                    Console.WriteLine("Errored as expected");
                    TestTools.ReportError("Parsing Error", parseEx);
                }
            }
        }

        [Fact]
        public void SparqlParsingConstructShortForm()
        {
            List<String> valid = new List<string>()
            {
                "CONSTRUCT WHERE {?s ?p ?o }",
                "CONSTRUCT WHERE {?s a ?type }",
            };

            List<String> invalid = new List<string>()
            {
                "CONSTRUCT {?s ?p ?o}",
                "CONSTRUCT WHERE { ?s ?p ?o . FILTER(ISLITERAL(?o)) }",
                "CONSTRUCT WHERE { GRAPH ?g { ?s ?p ?o } }",
                "CONSTRUCT WHERE { ?s ?p ?o . OPTIONAL {?s a ?type}}",
                "CONSTRUCT WHERE { ?s a ?type . BIND (<http://example.org> AS ?thing) }",
                "CONSTRUCT WHERE { {SELECT * WHERE { ?s ?p ?o } } }"
            };

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlFormatter formatter = new SparqlFormatter();
            foreach (String v in valid)
            {
                Console.WriteLine("Valid Input: " + v);
                    SparqlQuery q = parser.ParseFromString(v);
                    Console.WriteLine(formatter.Format(q));
                Console.WriteLine();
            }

            foreach (String iv in invalid)
            {
                Console.WriteLine("Invalid Input: " + iv);
                try
                {
                    SparqlQuery q = parser.ParseFromString(iv);
                    Assert.True(false, "Should have thrown a Parsing Error");
                }
                catch (RdfParseException parseEx)
                {
                    Console.WriteLine("Errored as expected");
                    TestTools.ReportError("Parsing Error", parseEx);
                }
                Console.WriteLine();
            }
        }

        [Fact]
        public void SparqlEvaluationMultipleOptionals()
        {
            TripleStore store = new TripleStore();
            store.LoadFromFile("resources\\multiple-options.trig");

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery query = parser.ParseFromFile("resources\\multiple-optionals.rq");

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(store);
            Object results = processor.ProcessQuery(query);
            if (results is SparqlResultSet)
            {
                TestTools.ShowResults(results);
            }
            else
            {
                Assert.True(false, "Expected a SPARQL Result Set");
            }
        }

        [Fact]
        public void SparqlEvaluationMultipleOptionals2()
        {
            TripleStore store = new TripleStore();
            store.LoadFromFile("resources\\multiple-options.trig");

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery query = parser.ParseFromFile("resources\\multiple-optionals-alternate.rq");

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(store);
            Object results = processor.ProcessQuery(query);
            if (results is SparqlResultSet)
            {
                TestTools.ShowResults(results);
            }
            else
            {
                Assert.True(false, "Expected a SPARQL Result Set");
            }
        }

        [Fact]
        public void SparqlParsingSubqueries1()
        {
            String query = "SELECT * WHERE { { SELECT * WHERE { ?s ?p ?o } } . }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);
            Console.WriteLine("Parsed original input OK");

            String query2 = q.ToString();
            Assert.Throws<RdfParseException>(() => parser.ParseFromString(query2));
            Console.WriteLine("Parsed reserialized input OK");
        }

        [Fact]
        public void SparqlParsingSubqueries2()
        {
            String query = "SELECT * WHERE { { SELECT * WHERE { ?s ?p ?o } } }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);
            Console.WriteLine("Parsed original input OK");

            String query2 = q.ToString();
            SparqlQuery q2 = parser.ParseFromString(query2);
            Console.WriteLine("Parsed reserialized input OK");
        }

        [Fact]
        public void SparqlParsingSubqueries3()
        {
            String query = "SELECT * WHERE { { SELECT * WHERE { ?s ?p ?o } } . }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);
            Console.WriteLine("Parsed original input OK");

            SparqlFormatter formatter = new SparqlFormatter();
            String query2 = formatter.Format(q);
            Assert.Throws<RdfParseException>(() => parser.ParseFromString(query2));
            Console.WriteLine("Parsed reserialized input OK");
        }

        [Fact]
        public void SparqlParsingSubqueries4()
        {
            String query = "SELECT * WHERE { { SELECT * WHERE { ?s ?p ?o } } }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);
            Console.WriteLine("Parsed original input OK");

            SparqlFormatter formatter = new SparqlFormatter();
            String query2 = formatter.Format(q);
            SparqlQuery q2 = parser.ParseFromString(query2);
            Console.WriteLine("Parsed reserialized input OK");
        }

        [Fact]
        public void SparqlParsingUpdateWithDeleteWhereIllegal()
        {
            const string update = "WITH <http://graph> DELETE WHERE { ?s ?p ?o }";
            SparqlUpdateParser parser = new SparqlUpdateParser();
            Assert.Throws<RdfParseException>(() => parser.ParseFromString(update));
        }

        [Fact]
        public void SparqlParsingToStringRoundTripCore458()
        {
            const String query = @"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
SELECT * 
WHERE 
{
  ?s ?p ?o .
  FILTER(?p = rdf:type) 
}";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);
            String output = q.ToString();
            SparqlQuery q2 = parser.ParseFromString(output);
        }
    }
}
