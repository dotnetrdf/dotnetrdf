/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
