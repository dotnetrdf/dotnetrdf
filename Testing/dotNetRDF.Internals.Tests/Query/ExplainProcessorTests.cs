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
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{

    [Trait("category", "explicit")]
    public class ExplainProcessorTests
    {
        private ExplainQueryProcessor _processor;
        private readonly SparqlQueryParser _parser = new SparqlQueryParser();
        private readonly SparqlFormatter _formatter = new SparqlFormatter();

        private void TestExplainProcessor(String query)
        {
            if (this._processor == null)
            {
                TripleStore store = new TripleStore();
                Graph g = new Graph();
                g.LoadFromFile("resources\\InferenceTest.ttl");
                g.BaseUri = null;
                store.Add(g);

                this._processor = new ExplainQueryProcessor(store);
            }

            SparqlQuery q = this._parser.ParseFromString(query);
            Object results;
            Console.WriteLine("Input Query:");
            Console.WriteLine(this._formatter.Format(q));
            Console.WriteLine();

            Console.WriteLine("Explanation with Default Options (Simulated):");
            this._processor.ExplanationLevel = ExplanationLevel.DefaultSimulation;
            results = this._processor.ProcessQuery(q);

            Console.WriteLine();
            Console.WriteLine("Explanation with Default Options:");
            this._processor.ExplanationLevel = ExplanationLevel.Default;
            results = this._processor.ProcessQuery(q);

            Console.WriteLine();
            Console.WriteLine("Explanation with Full Options:");
            this._processor.ExplanationLevel = ExplanationLevel.Full;
            results = this._processor.ProcessQuery(q);
        }

        [Fact]
        public void SparqlExplainProcessor01()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o }");
        }

        [Fact]
        public void SparqlExplainProcessor02()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o OPTIONAL { ?s a ?type } }");
        }

        [Fact]
        public void SparqlExplainProcessor03()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o MINUS { ?s a ?type } }");
        }

        [Fact]
        public void SparqlExplainProcessor04()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o . OPTIONAL { ?s a ?type } FILTER(!BOUND(?type)) }");
        }

        [Fact]
        public void SparqlExplainProcessor05()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o . ?x ?y ?z }");
        }

        [Fact]
        public void SparqlExplainProcessor06()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o . ?s a ?type }");
        }

        [Fact]
        public void SparqlExplainProcessor07()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o . ?s ?p ?o2 }");
        }

        [Fact]
        public void SparqlExplainProcessor08()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o MINUS { ?x ?y ?z } }");
        }

        [Fact]
        public void SparqlExplainProcessor09()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o . FILTER (!SAMETERM(?s, <ex:nothing>)) . BIND(IsLiteral(?o) AS ?hasLiteral) . ?s a ?type }");
        }

        [Fact]
        public void SparqlExplainProcessor10()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o . FILTER (!SAMETERM(?s, <ex:nothing>)) . ?s a ?type . BIND(IsLiteral(?o) AS ?hasLiteral)}");
        }

        [Fact]
        public void SparqlExplainProcessor11()
        {
            this.TestExplainProcessor("SELECT * WHERE { GRAPH <http://graph> { ?s ?p ?o } }");
        }

        [Fact]
        public void SparqlExplainProcessor12()
        {
            this.TestExplainProcessor("SELECT * WHERE { GRAPH ?g { ?s ?p ?o } }");
        }

        [Fact]
        public void SparqlExplainProcessor13()
        {
            this.TestExplainProcessor("SELECT * WHERE { BIND(<http://graph> AS ?g) GRAPH ?g { ?s ?p ?o } }");
        }

        [Fact]
        public void SparqlExplainProcessor14()
        {
            this.TestExplainProcessor("SELECT * FROM <http://default> WHERE { GRAPH ?g { ?s ?p ?o } }");
        }

        [Fact]
        public void SparqlExplainProcessor15()
        {
            this.TestExplainProcessor("SELECT * FROM NAMED <http://graph> WHERE { GRAPH ?g { ?s ?p ?o } }");
        }
    }
}
