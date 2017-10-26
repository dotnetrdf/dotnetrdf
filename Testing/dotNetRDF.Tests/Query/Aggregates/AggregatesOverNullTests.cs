/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Aggregates
{

    public class AggregatesOverNullTests
    {
        private INodeFactory _factory = new NodeFactory();
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private LeviathanQueryProcessor _processor = new LeviathanQueryProcessor(new InMemoryDataset());

        [Fact]
        public void SparqlAggregatesOverNullCount1()
        {
            SparqlQuery q = this._parser.ParseFromString("SELECT (COUNT(*) AS ?Count) WHERE { ?s ?p ?o }");
            SparqlResultSet results = this._processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(1, results.Count);

            SparqlResult r = results.First();
            Assert.Equal((0).ToLiteral(this._factory), r["Count"]);
        }

        [Fact]
        public void SparqlAggregatesOverNullCount2()
        {
            SparqlQuery q = this._parser.ParseFromString("SELECT (COUNT(?s) AS ?Count) WHERE { ?s ?p ?o }");
            SparqlResultSet results = this._processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(1, results.Count);

            SparqlResult r = results.First();
            Assert.Equal((0).ToLiteral(this._factory), r["Count"]);
        }

        [Fact]
        public void SparqlAggregatesOverNullCount3()
        {
            SparqlQuery q = this._parser.ParseFromString("SELECT (COUNT(*) AS ?Count) WHERE { ?s ?p ?o } GROUP BY ?s");
            SparqlResultSet results = this._processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(1, results.Count);

            SparqlResult r = results.First();
            Assert.Equal((0).ToLiteral(this._factory), r["Count"]);
        }

        [Fact]
        public void SparqlAggregatesOverNullCount4()
        {
            SparqlQuery q = this._parser.ParseFromString("SELECT (COUNT(?s) AS ?Count) WHERE { ?s ?p ?o } GROUP BY ?s");
            SparqlResultSet results = this._processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(1, results.Count);

            SparqlResult r = results.First();
            Assert.Equal((0).ToLiteral(this._factory), r["Count"]);
        }

        [Fact]
        public void SparqlAggregatesOverNullSum1()
        {
            SparqlQuery q = this._parser.ParseFromString("SELECT (SUM(?s) AS ?Sum) WHERE { ?s ?p ?o }");
            SparqlResultSet results = this._processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(1, results.Count);

            SparqlResult r = results.First();
            Assert.Equal((0).ToLiteral(this._factory), r["Sum"]);
        }

        [Fact]
        public void SparqlAggregatesOverNullSum2()
        {
            SparqlQuery q = this._parser.ParseFromString("SELECT (SUM(?s) AS ?Sum) WHERE { ?s ?p ?o } GROUP BY ?s");
            SparqlResultSet results = this._processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(1, results.Count);

            SparqlResult r = results.First();
            Assert.Equal((0).ToLiteral(this._factory), r["Sum"]);
        }

        [Fact]
        public void SparqlAggregatesOverNullAvg1()
        {
            SparqlQuery q = this._parser.ParseFromString("SELECT (AVG(?s) AS ?Avg) WHERE { ?s ?p ?o }");
            SparqlResultSet results = this._processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(1, results.Count);

            SparqlResult r = results.First();
            Assert.Equal((0).ToLiteral(this._factory), r["Avg"]);
        }

        [Fact]
        public void SparqlAggregatesOverNullAvg2()
        {
            SparqlQuery q = this._parser.ParseFromString("SELECT (AVG(?s) AS ?Avg) WHERE { ?s ?p ?o } GROUP BY ?s");
            SparqlResultSet results = this._processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(1, results.Count);

            SparqlResult r = results.First();
            Assert.Equal((0).ToLiteral(this._factory), r["Avg"]);
        }

        [Fact]
        public void SparqlAggregatesOverNullMax1()
        {
            SparqlQuery q = this._parser.ParseFromString("SELECT (MAX(?s) AS ?Max) WHERE { ?s ?p ?o }");
            SparqlResultSet results = this._processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(1, results.Count);

            SparqlResult r = results.First();
            Assert.Null(r["Max"]);
        }

        [Fact]
        public void SparqlAggregatesOverNullMax2()
        {
            SparqlQuery q = this._parser.ParseFromString("SELECT (MAX(?s) AS ?Max) WHERE { ?s ?p ?o } GROUP BY ?s");
            SparqlResultSet results = this._processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(1, results.Count);

            SparqlResult r = results.First();
            Assert.Null(r["Max"]);
        }

        [Fact]
        public void SparqlAggregatesOverNullMin1()
        {
            SparqlQuery q = this._parser.ParseFromString("SELECT (MIN(?s) AS ?Min) WHERE { ?s ?p ?o }");
            SparqlResultSet results = this._processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(1, results.Count);

            SparqlResult r = results.First();
            Assert.Null(r["Min"]);
        }

        
        [Fact]
        public void SparqlAggregatesOverNullMin2()
        {
            SparqlQuery q = this._parser.ParseFromString("SELECT (MIN(?s) AS ?Min) WHERE { ?s ?p ?o } GROUP BY ?s");
            SparqlResultSet results = this._processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(1, results.Count);

            SparqlResult r = results.First();
            Assert.Null(r["Min"]);
        }

        [Fact]
        public void SparqlAggregatesOverNullGroupConcat1()
        {
            SparqlQuery q = this._parser.ParseFromString("SELECT (GROUP_CONCAT(?s) AS ?GroupConcat) WHERE { ?s ?p ?o }");
            SparqlResultSet results = this._processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(1, results.Count);

            SparqlResult r = results.First();
            Assert.Equal(this._factory.CreateLiteralNode(String.Empty), r["GroupConcat"]);
        }

        [Fact]
        public void SparqlAggregatesOverNullGroupConcat2()
        {
            SparqlQuery q = this._parser.ParseFromString("SELECT (GROUP_CONCAT(?s) AS ?GroupConcat) WHERE { ?s ?p ?o } GROUP BY ?s");
            SparqlResultSet results = this._processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(1, results.Count);

            SparqlResult r = results.First();
            Assert.Equal(this._factory.CreateLiteralNode(String.Empty), r["GroupConcat"]);
        }
    }
}
