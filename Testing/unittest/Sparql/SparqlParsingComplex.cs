using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update;

using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class SparqlParsingComplex
    {
        [TestMethod]
        public void SparqlParsingNestedGraphPatternFirstItem()
        {
            try
            {
                SparqlQueryParser parser = new SparqlQueryParser();
                SparqlQuery q = parser.ParseFromFile("childgraphpattern.rq");

                Console.WriteLine(q.ToString());
                Console.WriteLine();
                Console.WriteLine(q.ToAlgebra().ToString());
            }
            catch (RdfParseException parseEx)
            {
                TestTools.ReportError("Parsing Error", parseEx, true);
            }
        }

        [TestMethod]
        public void SparqlParsingNestedGraphPatternFirstItem2()
        {
            try
            {
                SparqlQueryParser parser = new SparqlQueryParser();
                SparqlQuery q = parser.ParseFromFile("childgraphpattern2.rq");

                Console.WriteLine(q.ToString());
                Console.WriteLine();
                Console.WriteLine(q.ToAlgebra().ToString());
            }
            catch (RdfParseException parseEx)
            {
                TestTools.ReportError("Parsing Error", parseEx, true);
            }
        }

        [TestMethod]
        public void SparqlParsingSubQueryWithLimitAndOrderBy()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            String query = "SELECT * WHERE { { SELECT * WHERE {?s ?p ?o} ORDER BY ?p ?o LIMIT 2 } }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);

                Assert.IsTrue(rset.All(r => r.HasValue("s") && r.HasValue("p") && r.HasValue("o")), "All Results should have had ?s, ?p and ?o variables");
            }
            else
            {
                Assert.Fail("Expected a SPARQL Result Set");
            }
        }

        //[TestMethod]
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
        //                    Assert.AreEqual(rset, expected, "Result Sets should be equal");
        //                }
        //                else
        //                {
        //                    Assert.Fail("Didn't get a SPARQL Result Set as expected");
        //                }
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        //Options.UriNormalization = true;
        //    }
        //}

        [TestMethod]
        public void SparqlEvaluationComplexOptionalGraphUnion()
        {
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromFile("q-opt-complex-4.rq");

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "complex-data-2.ttl");
            store.Add(g);
            Graph h = new Graph();
            FileLoader.Load(h, "complex-data-1.ttl");
            store.Add(h);

            InMemoryDataset dataset = new InMemoryDataset(store);
            q.AddDefaultGraph(g.BaseUri);
            q.AddNamedGraph(h.BaseUri);

            SparqlFormatter formatter = new SparqlFormatter(q.NamespaceMap);
            Object results;

            //Examine limited parts of the Query to see why it doesn't work properly
            SparqlParameterizedString unionClause = new SparqlParameterizedString();
            unionClause.Namespaces = q.NamespaceMap;
            unionClause.CommandText = "SELECT * WHERE { ?person foaf:name ?name . { ?person ex:healthplan ?plan . } UNION { ?person ex:department ?dept . } }";
            SparqlQuery unionQuery = parser.ParseFromString(unionClause);

            Console.WriteLine("UNION Clause Only");
            Console.WriteLine(formatter.Format(unionQuery));

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
            results = processor.ProcessQuery(unionQuery);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);
            }
            else
            {
                Assert.Fail("Didn't get a Result Set as expected");
            }
            Console.WriteLine();

            //Try the Optional Clause
            SparqlParameterizedString optionalClause = new SparqlParameterizedString();
            optionalClause.Namespaces = q.NamespaceMap;
            optionalClause.CommandText = "SELECT * WHERE { OPTIONAL { ?person a foaf:Person . GRAPH ?g { [] foaf:depiction ?img ; foaf:name ?name } } }";
            SparqlQuery optionalQuery = parser.ParseFromString(optionalClause);

            Console.WriteLine("OPTIONAL Clause Only");
            Console.WriteLine(formatter.Format(optionalQuery));

            results = processor.ProcessQuery(optionalQuery);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);
            }
            else
            {
                Assert.Fail("Didn't get a Result Set as expected");
            }
            Console.WriteLine();

            //Try the full Query with a SELECT * to examine all the values
            SparqlParameterizedString fullQuery = new SparqlParameterizedString();
            fullQuery.Namespaces = q.NamespaceMap;
            fullQuery.CommandText = "SELECT * WHERE { ?person foaf:name ?name . { ?person ex:healthplan ?plan . } UNION { ?person ex:department ?dept . } OPTIONAL { ?person a foaf:Person . GRAPH ?g { [] foaf:depiction ?img ; foaf:name ?name } } }";
            SparqlQuery q2 = parser.ParseFromString(fullQuery);

            Console.WriteLine("Full Query as a SELECT *");
            Console.WriteLine(formatter.Format(q2));

            results = processor.ProcessQuery(q2);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);
            }
            else
            {
                Assert.Fail("Didn't get a Result Set as expected");
            }
            Console.WriteLine();

            //Try the full Query
            Console.WriteLine("Full Query");
            Console.WriteLine(formatter.Format(q));

            results = processor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);

                SparqlRdfParser resultsParser = new SparqlRdfParser(new TurtleParser());
                SparqlResultSet expected = new SparqlResultSet();
                resultsParser.Load(expected, "result-opt-complex-4.ttl");

                Console.WriteLine();
                Console.WriteLine("Expected Results");
                TestTools.ShowResults(expected);

                Assert.AreEqual(rset, expected, "Result Sets should be equal");
            } 
            else 
            {
                Assert.Fail("Didn't get a Result Set as expected");
            }
        }

        [TestMethod]
        public void SparqlEvaluationGraphUnion()
        {
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromFile("graph-11.rq");
            q.BaseUri = new Uri("file:///" + Environment.CurrentDirectory.Replace('\\', '/') + "/");

            TripleStore store = new TripleStore();
            Graph g = new Graph();

            FileLoader.Load(g, "data-g1.ttl");
            store.Add(g);
            Graph h = new Graph();
            FileLoader.Load(h, "data-g2.ttl");
            store.Add(h);
            Graph i = new Graph();
            FileLoader.Load(i, "data-g3.ttl");
            store.Add(i);
            Graph j = new Graph();
            FileLoader.Load(j, "data-g4.ttl");
            store.Add(j);

            InMemoryDataset dataset = new InMemoryDataset(store);
            q.AddDefaultGraph(g.BaseUri);
            q.AddNamedGraph(g.BaseUri);
            q.AddNamedGraph(h.BaseUri);
            q.AddNamedGraph(i.BaseUri);
            q.AddNamedGraph(j.BaseUri);

            SparqlFormatter formatter = new SparqlFormatter(q.NamespaceMap);
            Object results;

            //Try the full Query
            Console.WriteLine("Full Query");
            Console.WriteLine(formatter.Format(q));

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
            results = processor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);

                //SparqlRdfParser resultsParser = new SparqlRdfParser(new TurtleParser());
                //SparqlResultSet expected = new SparqlResultSet();
                //resultsParser.Load(expected, "graph-11.ttl");

                //Console.WriteLine();
                //Console.WriteLine("Expected Results");
                //TestTools.ShowResults(expected);

                //Assert.AreEqual(rset, expected, "Result Sets should be equal");
            }
            else
            {
                Assert.Fail("Didn't get a Result Set as expected");
            }
        }

        [TestMethod]
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
                try
                {
                    SparqlQuery q = parser.ParseFromString(v);
                    Console.WriteLine(formatter.Format(q));
                    Console.WriteLine();
                }
                catch (RdfParseException parseEx)
                {
                    Console.WriteLine("Failed to parse valid Query");
                    TestTools.ReportError("Parsing Error", parseEx, true);
                }
            }

            foreach (String iv in invalid)
            {
                try
                {
                    SparqlQuery q = parser.ParseFromString(iv);
                    Assert.Fail("Should have thrown a Parsing Error");
                }
                catch (RdfParseException parseEx)
                {
                    Console.WriteLine("Errored as expected");
                    TestTools.ReportError("Parsing Error", parseEx, false);
                }
            }
        }

        [TestMethod]
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
                try
                {
                    SparqlQuery q = parser.ParseFromString(v);
                    Console.WriteLine(formatter.Format(q));
                }
                catch (RdfParseException parseEx)
                {
                    Console.WriteLine("Failed to parse valid Query");
                    TestTools.ReportError("Parsing Error", parseEx, true);
                }
                Console.WriteLine();
            }

            foreach (String iv in invalid)
            {
                Console.WriteLine("Invalid Input: " + iv);
                try
                {
                    SparqlQuery q = parser.ParseFromString(iv);
                    Assert.Fail("Should have thrown a Parsing Error");
                }
                catch (RdfParseException parseEx)
                {
                    Console.WriteLine("Errored as expected");
                    TestTools.ReportError("Parsing Error", parseEx, false);
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void SparqlEvaluationMultipleOptionals()
        {
            TripleStore store = new TripleStore();
            store.LoadFromFile("multiple-options.trig");

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery query = parser.ParseFromFile("multiple-optionals.rq");

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(store);
            Object results = processor.ProcessQuery(query);
            if (results is SparqlResultSet)
            {
                TestTools.ShowResults(results);
            }
            else
            {
                Assert.Fail("Expected a SPARQL Result Set");
            }
        }

        [TestMethod]
        public void SparqlEvaluationMultipleOptionals2()
        {
            TripleStore store = new TripleStore();
            store.LoadFromFile("multiple-options.trig");

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery query = parser.ParseFromFile("multiple-optionals-alternate.rq");

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(store);
            Object results = processor.ProcessQuery(query);
            if (results is SparqlResultSet)
            {
                TestTools.ShowResults(results);
            }
            else
            {
                Assert.Fail("Expected a SPARQL Result Set");
            }
        }

        [TestMethod,ExpectedException(typeof(RdfParseException))]
        public void SparqlParsingSubqueries1()
        {
            String query = "SELECT * WHERE { { SELECT * WHERE { ?s ?p ?o } } . }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);
            Console.WriteLine("Parsed original input OK");

            String query2 = q.ToString();
            SparqlQuery q2 = parser.ParseFromString(query2);
            Console.WriteLine("Parsed reserialized input OK");
        }

        [TestMethod]
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

        [TestMethod,ExpectedException(typeof(RdfParseException))]
        public void SparqlParsingSubqueries3()
        {
            String query = "SELECT * WHERE { { SELECT * WHERE { ?s ?p ?o } } . }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);
            Console.WriteLine("Parsed original input OK");

            SparqlFormatter formatter = new SparqlFormatter();
            String query2 = formatter.Format(q);
            SparqlQuery q2 = parser.ParseFromString(query2);
            Console.WriteLine("Parsed reserialized input OK");
        }

        [TestMethod]
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
    }
}
