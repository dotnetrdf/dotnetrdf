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
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class BindingsTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();

        [TestMethod]
        public void SparqlBindingsSimple()
        {
            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
            query.CommandText = "SELECT ?subj WHERE { ?subj a ex:Car }";

            SparqlParameterizedString bindingsQuery = new SparqlParameterizedString();
            bindingsQuery.Namespaces = query.Namespaces;
            bindingsQuery.CommandText = "SELECT ?subj WHERE { ?subj a ?type } VALUES ?type { ex:Car }";

            this.TestBindings(this.GetTestData(), bindingsQuery, query);
        }

        [TestMethod]
        public void SparqlBindingsSimple2()
        {
            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
            query.CommandText = "SELECT ?subj WHERE { ?subj a ex:Car }";

            SparqlParameterizedString bindingsQuery = new SparqlParameterizedString();
            bindingsQuery.Namespaces = query.Namespaces;
            bindingsQuery.CommandText = "SELECT ?subj WHERE { ?subj a ?type } VALUES ?type { ex:Car ex:Plane }";

            this.TestBindings(this.GetTestData(), bindingsQuery, query);
        }
        [TestMethod]
        public void SparqlBindingsSimple3()
        {
            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
            query.CommandText = "SELECT ?subj WHERE { ?subj a ex:Car }";

            SparqlParameterizedString bindingsQuery = new SparqlParameterizedString();
            bindingsQuery.Namespaces = query.Namespaces;
            bindingsQuery.CommandText = "SELECT ?subj WHERE { ?subj a ?type } VALUES ( ?subj ?type ) { (ex:FordFiesta ex:Car) }";

            this.TestBindings(this.GetTestData(), bindingsQuery, query);
        }


        private void TestBindings(ISparqlDataset data, SparqlParameterizedString queryWithBindings, SparqlParameterizedString queryWithoutBindings)
        {
            this.TestBindings(data, queryWithBindings.ToString(), queryWithoutBindings.ToString());
        }

        private void TestBindings(ISparqlDataset data, String queryWithBindings, String queryWithoutBindings)
        {
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(data);
            SparqlQuery bindingsQuery = this._parser.ParseFromString(queryWithBindings);
            SparqlQuery noBindingsQuery = this._parser.ParseFromString(queryWithoutBindings);

            SparqlResultSet bindingsResults = processor.ProcessQuery(bindingsQuery) as SparqlResultSet;
            SparqlResultSet noBindingsResults = processor.ProcessQuery(noBindingsQuery) as SparqlResultSet;

            if (bindingsResults == null) Assert.Fail("Did not get a SPARQL Result Set for the Bindings Query");
            if (noBindingsResults == null) Assert.Fail("Did not get a SPARQL Result Set for the Non-Bindings Query");

            Console.WriteLine("Bindings Results");
            TestTools.ShowResults(bindingsResults);
            Console.WriteLine();
            Console.WriteLine("Non-Bindings Results");
            TestTools.ShowResults(noBindingsResults);
            Console.WriteLine();

            Assert.AreEqual(noBindingsResults, bindingsResults, "Result Sets should have been equal");
        }

        private void TestBindings(ISparqlDataset data, String queryWithBindings)
        {
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(data);
            SparqlQuery bindingsQuery = this._parser.ParseFromString(queryWithBindings);

            SparqlResultSet bindingsResults = processor.ProcessQuery(bindingsQuery) as SparqlResultSet;

            if (bindingsResults == null) Assert.Fail("Did not get a SPARQL Result Set for the Bindings Query");

            Console.WriteLine("Bindings Results");
            TestTools.ShowResults(bindingsResults);
            Console.WriteLine();

            Assert.IsTrue(bindingsResults.IsEmpty, "Result Set should be empty");
        }

        private ISparqlDataset GetTestData()
        {
            InMemoryDataset dataset = new InMemoryDataset();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            dataset.AddGraph(g);

            return dataset;
        }
    }
}
