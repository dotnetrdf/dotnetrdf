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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{
    /// <summary>
    /// Summary description for SparqlNewFunctions
    /// </summary>
    [TestClass]
    public class SparqlNewFunctions
    {
        [TestMethod]
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

            Object results = store.ExecuteQuery(q);

            Assert.IsTrue(results is SparqlResultSet, "Result should be a SPARQL Result Set");
            TestTools.ShowResults(results);
        }

        [TestMethod]
        public void SparqlFunctionsNow()
        {
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromFile("now01.rq");

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
                Assert.IsTrue(results.Result, "Result should be true");
            }
            else
            {
                Assert.Fail("Expected a non-null result");
            }
        }

        [TestMethod]
        public void SparqlFunctionsRand()
        {
            String query = "SELECT ?s (RAND() AS ?rand) WHERE { ?s ?p ?o } ORDER BY ?rand";
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");

            Object results = g.ExecuteQuery(query);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);
            }
            else
            {
                Assert.Fail("Did not get a SPARQL Result Set as expected");
            }
        }

        [TestMethod]
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
                    Assert.Fail("Did not get a SPARQL Result Set as expected");
                }
            }
        }
    }
}
