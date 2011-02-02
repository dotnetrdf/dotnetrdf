using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class SparqlParsingComplex
    {
        [TestMethod]
        public void SparqlNestedGraphPatternFirstItem()
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
        public void SparqlNestedGraphPatternFirstItem2()
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
        public void SparqlSubQueryWithLimitAndOrderBy()
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
        public void SparqlComplexOptionalGraphUnion()
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
            unionClause.QueryText = "SELECT * WHERE { ?person foaf:name ?name . { ?person ex:healthplan ?plan . } UNION { ?person ex:department ?dept . } }";
            SparqlQuery unionQuery = parser.ParseFromString(unionClause);

            Console.WriteLine("UNION Clause Only");
            Console.WriteLine(formatter.Format(unionQuery));

            results = unionQuery.Evaluate(dataset);
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
            optionalClause.QueryText = "SELECT * WHERE { OPTIONAL { ?person a foaf:Person . GRAPH ?g { [] foaf:depiction ?img ; foaf:name ?name } } }";
            SparqlQuery optionalQuery = parser.ParseFromString(optionalClause);

            Console.WriteLine("OPTIONAL Clause Only");
            Console.WriteLine(formatter.Format(optionalQuery));

            results = optionalQuery.Evaluate(dataset);
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
            fullQuery.QueryText = "SELECT * WHERE { ?person foaf:name ?name . { ?person ex:healthplan ?plan . } UNION { ?person ex:department ?dept . } OPTIONAL { ?person a foaf:Person . GRAPH ?g { [] foaf:depiction ?img ; foaf:name ?name } } }";
            SparqlQuery q2 = parser.ParseFromString(fullQuery);

            Console.WriteLine("Full Query as a SELECT *");
            Console.WriteLine(formatter.Format(q2));

            results = q2.Evaluate(dataset);
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

            results = q.Evaluate(dataset);
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
    }
}
