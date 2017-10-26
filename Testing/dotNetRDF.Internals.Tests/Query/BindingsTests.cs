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
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query
{

    public class BindingsTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();

        [Fact]
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

        [Fact]
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
        [Fact]
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

            if (bindingsResults == null) Assert.True(false, "Did not get a SPARQL Result Set for the Bindings Query");
            if (noBindingsResults == null) Assert.True(false, "Did not get a SPARQL Result Set for the Non-Bindings Query");

            Console.WriteLine("Bindings Results");
            TestTools.ShowResults(bindingsResults);
            Console.WriteLine();
            Console.WriteLine("Non-Bindings Results");
            TestTools.ShowResults(noBindingsResults);
            Console.WriteLine();

            Assert.Equal(noBindingsResults, bindingsResults);
        }

        private void TestBindings(ISparqlDataset data, String queryWithBindings)
        {
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(data);
            SparqlQuery bindingsQuery = this._parser.ParseFromString(queryWithBindings);

            SparqlResultSet bindingsResults = processor.ProcessQuery(bindingsQuery) as SparqlResultSet;

            if (bindingsResults == null) Assert.True(false, "Did not get a SPARQL Result Set for the Bindings Query");

            Console.WriteLine("Bindings Results");
            TestTools.ShowResults(bindingsResults);
            Console.WriteLine();

            Assert.True(bindingsResults.IsEmpty, "Result Set should be empty");
        }

        private ISparqlDataset GetTestData()
        {
            InMemoryDataset dataset = new InMemoryDataset();
            Graph g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");
            dataset.AddGraph(g);

            return dataset;
        }
    }
}
