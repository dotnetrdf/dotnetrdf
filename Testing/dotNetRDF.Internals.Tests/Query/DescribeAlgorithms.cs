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
using System.Linq;
using System.Text;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Describe;

namespace VDS.RDF.Query
{

    public class DescribeAlgorithms : IDisposable
    {
        private const String DescribeQuery = "PREFIX foaf: <http://xmlns.com/foaf/0.1/> DESCRIBE ?x WHERE {?x foaf:name \"Dave\" }";

        private SparqlQueryParser _parser;
        private InMemoryDataset _data;
        private LeviathanQueryProcessor _processor;

        public DescribeAlgorithms()
        {
            this._parser = new SparqlQueryParser();
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromFile("resources\\describe-algos.ttl");
            store.Add(g);
            this._data = new InMemoryDataset(store);
            this._processor = new LeviathanQueryProcessor(this._data);
        }

        public void Dispose()
        {
            this._parser = null;
            this._data = null;
        }

        private SparqlQuery GetQuery()
        {
            return this._parser.ParseFromString(DescribeQuery);
        }

        [Theory]
        [InlineData(typeof(ConciseBoundedDescription))]
        [InlineData(typeof(SymmetricConciseBoundedDescription))]
        [InlineData(typeof(SimpleSubjectDescription))]
        [InlineData(typeof(SimpleSubjectObjectDescription))]
        [InlineData(typeof(MinimalSpanningGraph))]
        [InlineData(typeof(LabelledDescription))]
        public void SparqlDescribeAlgorithms(Type describerType)
        {
            SparqlQuery q = this.GetQuery();
            q.Describer = (ISparqlDescribe) Activator.CreateInstance(describerType);
            Object results = this._processor.ProcessQuery(q);
            Assert.IsAssignableFrom<Graph>(results);
            if (results is Graph)
            {
                TestTools.ShowResults(results);
            }
        }

        [Fact]
        public void SparqlDescribeDefaultGraphHandling1()
        {
            InMemoryDataset dataset = new InMemoryDataset();

            IGraph g = new Graph();
            g.Assert(g.CreateUriNode(UriFactory.Create("http://subject")), g.CreateUriNode(UriFactory.Create("http://predicate")), g.CreateUriNode(UriFactory.Create("http://object")));
            g.BaseUri = UriFactory.Create("http://graph");
            dataset.AddGraph(g);

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
            IGraph description = processor.ProcessQuery(this._parser.ParseFromString("DESCRIBE ?s FROM <http://graph> WHERE { ?s ?p ?o }")) as IGraph;
            Assert.NotNull(description);
            Assert.False(description.IsEmpty);
        }

        [Fact]
        public void SparqlDescribeDefaultGraphHandling2()
        {
            InMemoryDataset dataset = new InMemoryDataset();

            IGraph g = new Graph();
            g.Assert(g.CreateUriNode(UriFactory.Create("http://subject")), g.CreateUriNode(UriFactory.Create("http://predicate")), g.CreateUriNode(UriFactory.Create("http://object")));
            g.BaseUri = UriFactory.Create("http://graph");
            dataset.AddGraph(g);

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
            IGraph description = processor.ProcessQuery(this._parser.ParseFromString("DESCRIBE ?s WHERE { GRAPH <http://graph> { ?s ?p ?o } }")) as IGraph;
            Assert.NotNull(description);
            Assert.True(description.IsEmpty);
        }

        [Fact]
        public void SparqlDescribeDefaultGraphHandling3()
        {
            InMemoryDataset dataset = new InMemoryDataset();

            IGraph g = new Graph();
            g.Assert(g.CreateUriNode(UriFactory.Create("http://subject")), g.CreateUriNode(UriFactory.Create("http://predicate")), g.CreateUriNode(UriFactory.Create("http://object")));
            g.BaseUri = UriFactory.Create("http://graph");
            dataset.AddGraph(g);

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
            IGraph description = processor.ProcessQuery(this._parser.ParseFromString("DESCRIBE ?s FROM <http://graph> WHERE { GRAPH <http://graph> { ?s ?p ?o } }")) as IGraph;
            Assert.NotNull(description);
            Assert.True(description.IsEmpty);
        }

        [Fact]
        public void SparqlDescribeDefaultGraphHandling4()
        {
            InMemoryDataset dataset = new InMemoryDataset();

            IGraph g = new Graph();
            g.Assert(g.CreateUriNode(UriFactory.Create("http://subject")), g.CreateUriNode(UriFactory.Create("http://predicate")), g.CreateUriNode(UriFactory.Create("http://object")));
            g.BaseUri = UriFactory.Create("http://graph");
            dataset.AddGraph(g);

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
            IGraph description = processor.ProcessQuery(this._parser.ParseFromString("DESCRIBE ?s FROM <http://graph> FROM NAMED <http://graph> WHERE { GRAPH <http://graph> { ?s ?p ?o } }")) as IGraph;
            Assert.NotNull(description);
            Assert.False(description.IsEmpty);
        }
    }
}
