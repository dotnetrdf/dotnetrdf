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
using NUnit.Framework;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing.Handlers
{
    [TestFixture]
    public class WriteThroughHandlerTests
    {
        private void ParsingUsingWriteThroughHandler(ITripleFormatter formatter)
        {
            if (!System.IO.File.Exists("temp.ttl"))
            {
                Graph g = new Graph();
                EmbeddedResourceLoader.Load(g, "dotNetRDF.Configuration.configuration.ttl");
                g.SaveToFile("temp.ttl");
            }

            WriteThroughHandler handler = new WriteThroughHandler(formatter, Console.Out, false);
            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "temp.ttl");
        }

        private void ParsingUsingWriteThroughHandler(Type formatterType)
        {
            if (!System.IO.File.Exists("temp.ttl"))
            {
                Graph g = new Graph();
                EmbeddedResourceLoader.Load(g, "dotNetRDF.Configuration.configuration.ttl");
                g.SaveToFile("temp.ttl");
            }

            WriteThroughHandler handler = new WriteThroughHandler(formatterType, Console.Out, false);
            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "temp.ttl");
        }

        [Test]
        public void ParsingWriteThroughHandlerNTriples()
        {
            this.ParsingUsingWriteThroughHandler(new NTriplesFormatter());
        }

        [Test]
        public void ParsingWriteThroughHandlerTurtle()
        {
            this.ParsingUsingWriteThroughHandler(new TurtleFormatter());
        }

        [Test]
        public void ParsingWriteThroughHandlerNotation3()
        {
            this.ParsingUsingWriteThroughHandler(new Notation3Formatter());
        }

        [Test]
        public void ParsingWriteThroughHandlerNQuads()
        {
            this.ParsingUsingWriteThroughHandler(new NQuadsFormatter());
        }

        [Test]
        public void ParsingWriteThroughHandlerUncompressedNotation3()
        {
            this.ParsingUsingWriteThroughHandler(new UncompressedNotation3Formatter());
        }

        [Test]
        public void ParsingWriteThroughHandlerUncompressedTurtle()
        {
            this.ParsingUsingWriteThroughHandler(new UncompressedTurtleFormatter());
        }

        [Test]
        public void ParsingWriteThroughHandlerCsv()
        {
            this.ParsingUsingWriteThroughHandler(new CsvFormatter());
        }

        [Test]
        public void ParsingWriteThroughHandlerTsv()
        {
            this.ParsingUsingWriteThroughHandler(new TsvFormatter());
        }

        [Test]
        public void ParsingWriteThroughHandlerRdfXml()
        {
            this.ParsingUsingWriteThroughHandler(new RdfXmlFormatter());
        }

        [Test]
        public void ParsingWriteThroughHandlerRdfXml2()
        {
            this.ParsingUsingWriteThroughHandler(typeof(RdfXmlFormatter));
        }

        [Test]
        public void ParsingWriteThroughHandlerSparql()
        {
            this.ParsingUsingWriteThroughHandler(new SparqlFormatter());
        }

        [Test]
        public void ParsingWriteThroughHandlerNTriples2()
        {
            this.ParsingUsingWriteThroughHandler(typeof(NTriplesFormatter));
        }

        [Test]
        public void ParsingWriteThroughHandlerTurtle2()
        {
            this.ParsingUsingWriteThroughHandler(typeof(TurtleFormatter));
        }

        [Test]
        public void ParsingWriteThroughHandlerNotation32()
        {
            this.ParsingUsingWriteThroughHandler(typeof(Notation3Formatter));
        }

        [Test]
        public void ParsingWriteThroughHandlerNQuads2()
        {
            this.ParsingUsingWriteThroughHandler(typeof(NQuadsFormatter));
        }

        [Test]
        public void ParsingWriteThroughHandlerUncompressedNotation32()
        {
            this.ParsingUsingWriteThroughHandler(typeof(UncompressedNotation3Formatter));
        }

        [Test]
        public void ParsingWriteThroughHandlerUncompressedTurtle2()
        {
            this.ParsingUsingWriteThroughHandler(typeof(UncompressedTurtleFormatter));
        }

        [Test]
        public void ParsingWriteThroughHandlerCsv2()
        {
            this.ParsingUsingWriteThroughHandler(typeof(CsvFormatter));
        }

        [Test]
        public void ParsingWriteThroughHandlerTsv2()
        {
            this.ParsingUsingWriteThroughHandler(typeof(TsvFormatter));
        }

        [Test]
        public void ParsingWriteThroughHandlerSparql2()
        {
            this.ParsingUsingWriteThroughHandler(typeof(SparqlFormatter));
        }
    }
}
