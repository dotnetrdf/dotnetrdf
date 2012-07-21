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
                SparqlQueryParser parser = new SparqlQueryParser();
                SparqlQuery q = parser.ParseFromFile("childgraphpattern.rq");

                Console.WriteLine(q.ToString());
                Console.WriteLine();
                Console.WriteLine(q.ToAlgebra().ToString());
        }

        [TestMethod]
        public void SparqlParsingNestedGraphPatternFirstItem2()
        {
                SparqlQueryParser parser = new SparqlQueryParser();
                SparqlQuery q = parser.ParseFromFile("childgraphpattern2.rq");

                Console.WriteLine(q.ToString());
                Console.WriteLine();
                Console.WriteLine(q.ToAlgebra().ToString());
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
                    Assert.Fail("Should have thrown a Parsing Error");
                }
                catch (RdfParseException parseEx)
                {
                    Console.WriteLine("Errored as expected");
                    TestTools.ReportError("Parsing Error", parseEx);
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
                    Assert.Fail("Should have thrown a Parsing Error");
                }
                catch (RdfParseException parseEx)
                {
                    Console.WriteLine("Errored as expected");
                    TestTools.ReportError("Parsing Error", parseEx);
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
