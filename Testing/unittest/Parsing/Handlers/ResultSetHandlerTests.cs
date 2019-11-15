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
using System.IO;
using System.Linq;
using Xunit;
using VDS.RDF.Query;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing.Handlers
{
    public class ResultSetHandlerTests
    {
        private void EnsureTestData(String file)
        {
            var writer = MimeTypesHelper.GetSparqlWriterByFileExtension(Path.GetExtension(file));
            EnsureTestData(file, writer);
        }

        private void EnsureTestData(String file, ISparqlResultsWriter writer)
        {
            if (!File.Exists(file))
            {
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.Retract(g.Triples.Where(t => !t.IsGroundTriple).ToList());
                SparqlResultSet results = g.ExecuteQuery("SELECT * WHERE { ?s ?p ?o }") as SparqlResultSet;
                if (results == null) Assert.True(false, "Failed to generate sample SPARQL Results");

                writer.Save(results, file);
            }
        }

        [Fact]
        public void ParsingResultSetHandlerImplicitSparqlXml()
        {
            this.EnsureTestData("test.srx");

            SparqlXmlParser parser = new SparqlXmlParser();
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(results, "test.srx");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.False(results.IsEmpty, "Result Set should not be empty");
            Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        }

        [Fact]
        public void ParsingResultSetHandlerImplicitSparqlJson()
        {
            this.EnsureTestData("test.srj");

            SparqlJsonParser parser = new SparqlJsonParser();
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(results, "test.srj");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.False(results.IsEmpty, "Result Set should not be empty");
            Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        }

        [Fact]
        public void ParsingResultSetHandlerImplicitSparqlRdfNTriples()
        {
            this.EnsureTestData("test.sparql.nt", new SparqlRdfWriter(new NTriplesWriter()));

            SparqlRdfParser parser = new SparqlRdfParser(new NTriplesParser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(results, "test.sparql.nt");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.False(results.IsEmpty, "Result Set should not be empty");
            Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        }

        [Fact]
        public void ParsingResultSetHandlerImplicitSparqlRdfTurtle()
        {
            this.EnsureTestData("test.sparql.ttl", new SparqlRdfWriter(new CompressingTurtleWriter()));

            SparqlRdfParser parser = new SparqlRdfParser(new TurtleParser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(results, "test.sparql.ttl");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.False(results.IsEmpty, "Result Set should not be empty");
            Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        }

        [Fact]
        public void ParsingResultSetHandlerImplicitSparqlRdfNotation3()
        {
            this.EnsureTestData("test.sparql.n3", new SparqlRdfWriter(new Notation3Writer()));

            SparqlRdfParser parser = new SparqlRdfParser(new Notation3Parser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(results, "test.sparql.n3");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.False(results.IsEmpty, "Result Set should not be empty");
            Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        }

        [Fact]
        public void ParsingResultSetHandlerImplicitSparqlRdfJson()
        {
            this.EnsureTestData("test.sparql.json", new SparqlRdfWriter(new RdfJsonWriter()));

            SparqlRdfParser parser = new SparqlRdfParser(new RdfJsonParser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(results, "test.sparql.json");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.False(results.IsEmpty, "Result Set should not be empty");
            Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        }

        [Fact]
        public void ParsingResultSetHandlerExplicitSparqlXml()
        {
            this.EnsureTestData("test.srx");

            SparqlXmlParser parser = new SparqlXmlParser();
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(new ResultSetHandler(results), "test.srx");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.False(results.IsEmpty, "Result Set should not be empty");
            Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        }

        [Fact]
        public void ParsingResultSetHandlerExplicitSparqlJson()
        {
            this.EnsureTestData("test.srj");

            SparqlJsonParser parser = new SparqlJsonParser();
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(new ResultSetHandler(results), "test.srj");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.False(results.IsEmpty, "Result Set should not be empty");
            Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        }

        [Fact]
        public void ParsingResultSetHandlerExplicitSparqlRdfNTriples()
        {
            this.EnsureTestData("test.sparql.nt", new SparqlRdfWriter(new NTriplesWriter()));

            SparqlRdfParser parser = new SparqlRdfParser(new NTriplesParser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(new ResultSetHandler(results), "test.sparql.nt");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.False(results.IsEmpty, "Result Set should not be empty");
            Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        }

        [Fact]
        public void ParsingResultSetHandlerExplicitSparqlRdfTurtle()
        {
            this.EnsureTestData("test.sparql.ttl", new SparqlRdfWriter(new CompressingTurtleWriter()));

            SparqlRdfParser parser = new SparqlRdfParser(new TurtleParser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(new ResultSetHandler(results), "test.sparql.ttl");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.False(results.IsEmpty, "Result Set should not be empty");
            Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        }

        [Fact]
        public void ParsingResultSetHandlerExplicitSparqlRdfNotation3()
        {
            this.EnsureTestData("test.sparql.n3", new SparqlRdfWriter(new Notation3Writer()));

            SparqlRdfParser parser = new SparqlRdfParser(new Notation3Parser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(new ResultSetHandler(results), "test.sparql.n3");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.False(results.IsEmpty, "Result Set should not be empty");
            Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        }

        [Fact]
        public void ParsingResultSetHandlerExplicitSparqlRdfJson()
        {
            this.EnsureTestData("test.sparql.json", new SparqlRdfWriter(new RdfJsonWriter()));

            SparqlRdfParser parser = new SparqlRdfParser(new RdfJsonParser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(new ResultSetHandler(results), "test.sparql.json");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.False(results.IsEmpty, "Result Set should not be empty");
            Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        }

        [Fact]
        public void ParsingMergingResultSetHandler()
        {
            this.EnsureTestData("test.srx", new SparqlXmlWriter());

            SparqlXmlParser parser = new SparqlXmlParser();
            SparqlResultSet results = new SparqlResultSet();
            MergingResultSetHandler handler = new MergingResultSetHandler(results);
            parser.Load(handler, "test.srx");

            Assert.False(results.IsEmpty, "Result Set should not be empty");
            Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);

            int count = results.Count;

            //Load again
            parser.Load(handler, "test.srx");

            Assert.Equal(count * 2, results.Count);
        }

        [Fact]
        public void ParsingResultsWriteThroughHandlerSparqlXml()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Query.Optimisation.OptimiserStats.ttl");
            SparqlResultSet original = g.ExecuteQuery("SELECT * WHERE { ?s ?p ?o }") as SparqlResultSet;
            SparqlXmlWriter sparqlWriter = new SparqlXmlWriter();
            sparqlWriter.Save(original, "test.custom.srx");

            SparqlXmlParser parser = new SparqlXmlParser();
            System.IO.StringWriter writer = new System.IO.StringWriter();
            ResultWriteThroughHandler handler = new ResultWriteThroughHandler(new SparqlXmlFormatter(), writer, false);
            parser.Load(handler, "test.custom.srx");

            Console.WriteLine(writer.ToString());

            SparqlResultSet results = new SparqlResultSet();
            parser.Load(results, new StringReader(writer.ToString()));

            Console.WriteLine("Original Result Count: " + original.Count);
            Console.WriteLine("Round Trip Result Count: " + results.Count);

            Assert.Equal(original, results);
        }

        [Fact]
        public void ParsingResultSetHandlerImplicitSparqlRdfXml()
        {
            this.EnsureTestData("test.sparql.rdf", new SparqlRdfWriter(new RdfXmlWriter()));

            SparqlRdfParser parser = new SparqlRdfParser(new RdfXmlParser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(results, "test.sparql.rdf");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.False(results.IsEmpty, "Result Set should not be empty");
            Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        }

        [Fact]
        public void ParsingResultSetHandlerExplicitSparqlRdfXml()
        {
            this.EnsureTestData("test.sparql.rdf", new SparqlRdfWriter(new RdfXmlWriter()));

            SparqlRdfParser parser = new SparqlRdfParser(new RdfXmlParser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(new ResultSetHandler(results), "test.sparql.rdf");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.False(results.IsEmpty, "Result Set should not be empty");
            Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
        }
    }
}
