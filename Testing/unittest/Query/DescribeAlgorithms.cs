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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Describe;

namespace VDS.RDF.Query
{
    [TestClass]
    public class DescribeAlgorithms
    {
        private const String DescribeQuery = "PREFIX foaf: <http://xmlns.com/foaf/0.1/> DESCRIBE ?x WHERE {?x foaf:name \"Dave\" }";

        private SparqlQueryParser _parser;
        private InMemoryDataset _data;
        private LeviathanQueryProcessor _processor;

        [TestInitialize]
        public void Setup()
        {
            this._parser = new SparqlQueryParser();
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "describe-algos.ttl");
            store.Add(g);
            this._data = new InMemoryDataset(store);
            this._processor = new LeviathanQueryProcessor(this._data);
        }

        [TestCleanup]
        public void Cleanup()
        {
            this._parser = null;
            this._data = null;
        }

        private SparqlQuery GetQuery()
        {
            return this._parser.ParseFromString(DescribeQuery);
        }

        private Graph RunDescribeTest(ISparqlDescribe describer)
        {
            SparqlQuery q = this.GetQuery();
            q.Describer = describer;
            Object results = this._processor.ProcessQuery(q);
            if (results is Graph)
            {
                TestTools.ShowResults(results);
                return (Graph)results;
            }
            else 
            {
                Assert.Fail("Expected a Graph as the Result");
                return null;
            }
        }

        [TestMethod]
        public void SparqlDescribeCBD()
        {
            this.RunDescribeTest(new ConciseBoundedDescription());
        }

        [TestMethod]
        public void SparqlDescribeSCBD()
        {
            this.RunDescribeTest(new SymmetricConciseBoundedDescription());
        }

        [TestMethod]
        public void SparqlDescribeSubject()
        {
            this.RunDescribeTest(new SimpleSubjectDescription());
        }

        [TestMethod]
        public void SparqlDescribeSubjectObject()
        {
            this.RunDescribeTest(new SimpleSubjectObjectDescription());
        }

        [TestMethod]
        public void SparqlDescribeMSG()
        {
            this.RunDescribeTest(new MinimalSpanningGraph());
        }

        [TestMethod]
        public void SparqlDescribeLabelled()
        {
            this.RunDescribeTest(new LabelledDescription());
        }
    }
}
