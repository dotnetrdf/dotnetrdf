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
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing.Handlers
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
            g.LoadFromFile("temp.ttl");

            TestTools.ShowGraph(g);
            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
        }

        [TestMethod]
        public void ParsingFileLoaderGraphHandlerExplicitTurtle()
        {
            EnsureTestData("temp.ttl");
            Graph g = new Graph();
            GraphHandler handler = new GraphHandler(g);
#if PORTABLE
            using (var input = File.OpenRead("temp.ttl"))
            {
                StreamLoader.Load(handler, "temp.ttl", input);
            }
#else
            FileLoader.Load(handler, "temp.ttl");
#endif

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
#if PORTABLE
            using (var input = File.OpenRead("temp.ttl"))
            {
                StreamLoader.Load(handler, "temp.ttl", input);
            }
#else
            FileLoader.Load(handler, "temp.ttl");
#endif

            Assert.AreEqual(orig.Triples.Count, handler.Count);
        }
    }
}
