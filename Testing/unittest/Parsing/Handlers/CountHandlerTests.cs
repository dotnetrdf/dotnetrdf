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
    public class CountHandlerTests
    {
        private void ParsingUsingCountHandler(String tempFile, IRdfReader parser)
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
            g.SaveToFile(tempFile);

            CountHandler handler = new CountHandler();
            parser.Load(handler, tempFile);

            Console.WriteLine("Counted " + handler.Count + " Triples");
            Assert.AreEqual(g.Triples.Count, handler.Count, "Counts should have been equal");
        }

        [TestMethod]
        public void ParsingCountHandlerNTriples()
        {
            this.ParsingUsingCountHandler("test.nt", new NTriplesParser());
        }

        [TestMethod]
        public void ParsingCountHandlerTurtle()
        {
            this.ParsingUsingCountHandler("test.ttl", new TurtleParser());
        }

        [TestMethod]
        public void ParsingCountHandlerNotation3()
        {
            this.ParsingUsingCountHandler("temp.n3", new Notation3Parser());
        }

        [TestMethod]
        public void ParsingCountHandlerRdfXml()
        {
            this.ParsingUsingCountHandler("test.rdf", new RdfXmlParser());
        }

        [TestMethod]
        public void ParsingCountHandlerRdfA()
        {
            this.ParsingUsingCountHandler("test.html", new RdfAParser());
        }

        [TestMethod]
        public void ParsingCountHandlerRdfJson()
        {
            this.ParsingUsingCountHandler("test.json", new RdfJsonParser());
        }
    }
}
