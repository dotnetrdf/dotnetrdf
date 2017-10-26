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
using VDS.RDF.Writing;

namespace VDS.RDF.Writing
{

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
                g.LoadFromFile("resources\\InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/graph");
                store.Add(g);

                this._dataset = new InMemoryDataset(store);
                this._processor = new LeviathanQueryProcessor(this._dataset);
            }
        }

        [Theory]
        [InlineData("SELECT * WHERE { ?s a ?type }")]
        [InlineData("SELECT * WHERE { ?s a ?type . ?s ex:Speed ?speed }")]
        [InlineData("SELECT * WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } }")]
        [InlineData("SELECT * WHERE { ?s <http://example.org/noSuchThing> ?o }")]
        [InlineData("SELECT * WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } ?s ?p ?o }")]
        [InlineData("SELECT ?s (ISLITERAL(?o) AS ?LiteralObject) WHERE { ?s ?p ?o }")]
        public void TestTsvRoundTrip(String query)
        {
            this.EnsureTestData();

            SparqlParameterizedString queryString = new SparqlParameterizedString(query);
            queryString.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
            SparqlQuery q = this._parser.ParseFromString(queryString);

            SparqlResultSet original = this._processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(original);

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

            Assert.Equal(original, results);
            
        }
    }
}
