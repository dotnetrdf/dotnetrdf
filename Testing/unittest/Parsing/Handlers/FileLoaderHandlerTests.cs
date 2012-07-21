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
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Test.Parsing.Handlers
{
    [TestClass]
    public class FileLoaderHandlerTests
    {
        private void EnsureTestData(String testFile)
        {
            if (!File.Exists(testFile))
            {
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.SaveToFile(testFile);
            }
        }

        [TestMethod]
        public void ParsingFileLoaderGraphHandlerImplicitTurtle()
        {
            EnsureTestData("temp.ttl");

            Graph g = new Graph();
            FileLoader.Load(g, "temp.ttl");

            TestTools.ShowGraph(g);
            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
        }

        [TestMethod]
        public void ParsingFileLoaderGraphHandlerExplicitTurtle()
        {
            EnsureTestData("temp.ttl");
            Graph g = new Graph();
            GraphHandler handler = new GraphHandler(g);
            FileLoader.Load(handler, "temp.ttl");

            TestTools.ShowGraph(g);
            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
        }

        [TestMethod]
        public void ParsingFileLoaderCountHandlerTurtle()
        {
            EnsureTestData("temp.ttl");
            Graph orig = new Graph();
            orig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            CountHandler handler = new CountHandler();
            FileLoader.Load(handler, "temp.ttl");

            Assert.AreEqual(orig.Triples.Count, handler.Count);
        }
    }
}
