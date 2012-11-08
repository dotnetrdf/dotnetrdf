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
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Parsing.Handlers
{
    [TestClass]
    public class WriteThroughHandlerTests
    {
        private void ParsingUsingWriteThroughHandler(ITripleFormatter formatter)
        {
            if (!System.IO.File.Exists("temp.ttl"))
            {
                Graph g = new Graph();
                EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
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
                EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
                g.SaveToFile("temp.ttl");
            }

            WriteThroughHandler handler = new WriteThroughHandler(formatterType, Console.Out, false);
            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "temp.ttl");
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerNTriples()
        {
            this.ParsingUsingWriteThroughHandler(new NTriplesFormatter());
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerTurtle()
        {
            this.ParsingUsingWriteThroughHandler(new TurtleFormatter());
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerNotation3()
        {
            this.ParsingUsingWriteThroughHandler(new Notation3Formatter());
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerNQuads()
        {
            this.ParsingUsingWriteThroughHandler(new NQuadsFormatter());
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerUncompressedNotation3()
        {
            this.ParsingUsingWriteThroughHandler(new UncompressedNotation3Formatter());
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerUncompressedTurtle()
        {
            this.ParsingUsingWriteThroughHandler(new UncompressedTurtleFormatter());
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerCsv()
        {
            this.ParsingUsingWriteThroughHandler(new CsvFormatter());
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerTsv()
        {
            this.ParsingUsingWriteThroughHandler(new TsvFormatter());
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerRdfXml()
        {
            this.ParsingUsingWriteThroughHandler(new RdfXmlFormatter());
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerRdfXml2()
        {
            this.ParsingUsingWriteThroughHandler(typeof(RdfXmlFormatter));
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerSparql()
        {
            this.ParsingUsingWriteThroughHandler(new SparqlFormatter());
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerNTriples2()
        {
            this.ParsingUsingWriteThroughHandler(typeof(NTriplesFormatter));
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerTurtle2()
        {
            this.ParsingUsingWriteThroughHandler(typeof(TurtleFormatter));
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerNotation32()
        {
            this.ParsingUsingWriteThroughHandler(typeof(Notation3Formatter));
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerNQuads2()
        {
            this.ParsingUsingWriteThroughHandler(typeof(NQuadsFormatter));
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerUncompressedNotation32()
        {
            this.ParsingUsingWriteThroughHandler(typeof(UncompressedNotation3Formatter));
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerUncompressedTurtle2()
        {
            this.ParsingUsingWriteThroughHandler(typeof(UncompressedTurtleFormatter));
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerCsv2()
        {
            this.ParsingUsingWriteThroughHandler(typeof(CsvFormatter));
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerTsv2()
        {
            this.ParsingUsingWriteThroughHandler(typeof(TsvFormatter));
        }

        [TestMethod]
        public void ParsingWriteThroughHandlerSparql2()
        {
            this.ParsingUsingWriteThroughHandler(typeof(SparqlFormatter));
        }
    }
}
