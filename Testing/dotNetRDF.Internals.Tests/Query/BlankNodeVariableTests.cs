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
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{

    public class BlankNodeVariableTests
    {
        private LeviathanQueryProcessor _processor;
        private TripleStore _data;
        private SparqlQueryParser _parser = new SparqlQueryParser();

        private void EnsureTestData()
        {
            if (this._data == null)
            {
                this._data = new TripleStore();
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                this._data.Add(g);
            }
            if (this._processor == null)
            {
                this._processor = new LeviathanQueryProcessor(new InMemoryDataset(this._data, this._data.Graphs.First().BaseUri));
            }
        }

        [Fact]
        public void SparqlBlankNodeVariables1()
        {
            this.EnsureTestData();

            SparqlQuery q = this._parser.ParseFromString("SELECT ?o WHERE { _:s ?p1 ?o1 FILTER(ISURI(?o1)) ?o1 ?p2 ?o . FILTER(ISLITERAL(?o)) }");
            q.AlgebraOptimisers = new IAlgebraOptimiser[] { new StrictAlgebraOptimiser() };
            SparqlFormatter formatter = new SparqlFormatter();
            Console.WriteLine(formatter.Format(q));
            Console.WriteLine(q.ToAlgebra().ToString());

            SparqlResultSet results = this._processor.ProcessQuery(q) as SparqlResultSet;
            if (results == null) Assert.True(false, "Did not get a SPARQL Result Set as expected");
            Assert.False(results.Count == 0, "Result Set should not be empty");

            Assert.True(results.All(r => r.HasValue("o") && r["o"] != null && r["o"].NodeType == NodeType.Literal), "All results should be literals");
        }
    }
}
