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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Summary description for SparqlNewFunctions
    /// </summary>

    public class SparqlNewFunctions
    {
        [Fact]
        public void SparqlFunctionsIsNumeric()
        {
            Graph g = new Graph();
            IUriNode subj = g.CreateUriNode(new Uri("http://example.org/subject"));
            IUriNode pred = g.CreateUriNode(new Uri("http://example.org/predicate"));

            g.Assert(subj, pred, (12).ToLiteral(g));
            g.Assert(subj, pred, g.CreateLiteralNode("12"));
            g.Assert(subj, pred, g.CreateLiteralNode("12", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNonNegativeInteger)));
            g.Assert(subj, pred, g.CreateLiteralNode("12", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNonPositiveInteger)));
            g.Assert(subj, pred, g.CreateLiteralNode("1200", new Uri(XmlSpecsHelper.XmlSchemaDataTypeByte)));
            g.Assert(subj, pred, ((byte)50).ToLiteral(g));
            g.Assert(subj, pred, g.CreateLiteralNode("-50", new Uri(XmlSpecsHelper.XmlSchemaDataTypeByte)));
            g.Assert(subj, pred, g.CreateLiteralNode("-50", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte)));
            g.Assert(subj, pred, g.CreateUriNode(new Uri("http://example.org")));

            TripleStore store = new TripleStore();
            store.Add(g);

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT ?obj (IsNumeric(?obj) AS ?IsNumeric) WHERE { ?s ?p ?obj }");

            var processor = new LeviathanQueryProcessor(store);
            var results = processor.ProcessQuery(q);

            Assert.True(results is SparqlResultSet, "Result should be a SPARQL Result Set");
            TestTools.ShowResults(results);
        }

        [Fact]
        public void SparqlFunctionsNow()
        {
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromFile("resources\\now01.rq");

            Console.WriteLine("ToString Output:");
            Console.WriteLine(q.ToString());
            Console.WriteLine();

            SparqlFormatter formatter = new SparqlFormatter();
            Console.WriteLine("SparqlFormatter Output:");
            Console.WriteLine(formatter.Format(q));

            TripleStore store = new TripleStore();
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(store);
            SparqlResultSet results = processor.ProcessQuery(q) as SparqlResultSet;
            if (results != null)
            {
                Assert.True(results.Result, "Result should be true");
            }
            else
            {
                Assert.True(false, "Expected a non-null result");
            }
        }

        [Fact]
        public void SparqlFunctionsRand()
        {
            String query = "SELECT ?s (RAND() AS ?rand) WHERE { ?s ?p ?o } ORDER BY ?rand";
            Graph g = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");

            Object results = g.ExecuteQuery(query);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);
            }
            else
            {
                Assert.True(false, "Did not get a SPARQL Result Set as expected");
            }
        }

        [Fact]
        public void SparqlOrderByNonDeterministic()
        {
            String query = "SELECT * WHERE { ?s ?p ?o } ORDER BY " + SparqlSpecsHelper.SparqlKeywordRand + "()";
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            for (int i = 0; i < 50; i++)
            {
                Object results = g.ExecuteQuery(query);
                if (results is SparqlResultSet)
                {
                    Console.WriteLine("Run #" + (i+1) + " OK");
                }
                else
                {
                    Assert.True(false, "Did not get a SPARQL Result Set as expected");
                }
            }
        }
    }
}
