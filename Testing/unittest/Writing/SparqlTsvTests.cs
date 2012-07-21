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
using VDS.RDF.Writing;

namespace VDS.RDF.Test.Writing
{
    [TestClass]
    public class SparqlTsvTests
    {
        private InMemoryDataset _dataset;
        private LeviathanQueryProcessor _processor;
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlTsvParser _tsvParser = new SparqlTsvParser();
        private SparqlTsvWriter _tsvWriter = new SparqlTsvWriter();

        private void EnsureTestData()
        {
            if (this._dataset == null)
            {
                TripleStore store = new TripleStore();
                Graph g = new Graph();
                g.LoadFromFile("InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/graph");
                store.Add(g);

                this._dataset = new InMemoryDataset(store);
                this._processor = new LeviathanQueryProcessor(this._dataset);
            }
        }

        private void TestTsvRoundTrip(String query)
        {
            this.EnsureTestData();

            SparqlParameterizedString queryString = new SparqlParameterizedString(query);
            queryString.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
            SparqlQuery q = this._parser.ParseFromString(queryString);

            SparqlResultSet original = this._processor.ProcessQuery(q) as SparqlResultSet;
            if (original == null) Assert.Fail("Did not get a SPARQL Result Set as expected");

            Console.WriteLine("Original Results:");
            TestTools.ShowResults(original);

            System.IO.StringWriter writer = new System.IO.StringWriter();
            this._tsvWriter.Save(original, writer);
            Console.WriteLine("Serialized TSV Results:");
            Console.WriteLine(writer.ToString());
            Console.WriteLine();

            SparqlResultSet results = new SparqlResultSet();
            this._tsvParser.Load(results, new StringReader(writer.ToString()));
            Console.WriteLine("Parsed Results:");
            TestTools.ShowResults(results);

            Assert.AreEqual(original, results, "Result Sets should be equal");
            
        }

        [TestMethod]
        public void WritingSparqlTsv1()
        {
            this.TestTsvRoundTrip("SELECT * WHERE { ?s a ?type }");
        }

        [TestMethod]
        public void WritingSparqlTsv2()
        {
            this.TestTsvRoundTrip("SELECT * WHERE { ?s a ?type . ?s ex:Speed ?speed }");
        }

        [TestMethod]
        public void WritingSparqlTsv3()
        {
            this.TestTsvRoundTrip("SELECT * WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } }");
        }

        [TestMethod]
        public void WritingSparqlTsv4()
        {
            this.TestTsvRoundTrip("SELECT * WHERE { ?s <http://example.org/noSuchThing> ?o }");
        }

        [TestMethod]
        public void WritingSparqlTsv5()
        {
            this.TestTsvRoundTrip("SELECT * WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } ?s ?p ?o }");
        }

        [TestMethod]
        public void WritingSparqlTsv6()
        {
            this.TestTsvRoundTrip("SELECT ?s (ISLITERAL(?o) AS ?LiteralObject) WHERE { ?s ?p ?o }");
        }
    }
}
