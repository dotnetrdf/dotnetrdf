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
    public class ChainedHandlerTests
    {
        private void EnsureTestData()
        {
            if (!System.IO.File.Exists("temp.ttl"))
            {
                Graph g = new Graph();
                EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
                g.SaveToFile("temp.ttl");
            }
        }
        
        [Test]
        public void ParsingChainedHandlerBadInstantiation()
        {
            Assert.Throws<ArgumentException>(() => new ChainedHandler(Enumerable.Empty<IRdfHandler>()));
        }

        [Test]
        public void ParsingChainedHandlerBadInstantiation2()
        {
            Assert.Throws<ArgumentNullException>(() => new ChainedHandler(null));
        }

        [Test]
        public void ParsingChainedHandlerBadInstantiation3()
        {
            GraphHandler h = new GraphHandler(new Graph());
            Assert.Throws<ArgumentException>(() => new ChainedHandler(new IRdfHandler[] { h, h }));
        }

        [Test]
        public void ParsingChainedHandlerTwoGraphs()
        {
            EnsureTestData();

            Graph g = new Graph();
            Graph h = new Graph();

            GraphHandler handler1 = new GraphHandler(g);
            GraphHandler handler2 = new GraphHandler(h);

            ChainedHandler handler = new ChainedHandler(new IRdfHandler[] { handler1, handler2 });

            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "temp.ttl");

            Assert.AreEqual(g.Triples.Count, h.Triples.Count, "Expected same number of Triples");
            Assert.AreEqual(g, h, "Expected Graphs to be equal");
        }

        [Test]
        public void ParsingChainedHandlerGraphAndPaging()
        {
            EnsureTestData();

            Graph g = new Graph();
            Graph h = new Graph();

            GraphHandler handler1 = new GraphHandler(g);
            PagingHandler handler2 = new PagingHandler(new GraphHandler(h), 100);

            ChainedHandler handler = new ChainedHandler(new IRdfHandler[] { handler1, handler2 });

            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "temp.ttl");

            Assert.AreEqual(101, g.Triples.Count, "Triples should have been limited to 101 (1st Graph)");
            Assert.AreEqual(100, h.Triples.Count, "Triples should have been limited to 100 (2nd Graph)");
            Assert.AreNotEqual(g.Triples.Count, h.Triples.Count, "Expected different number of Triples");
            Assert.AreNotEqual(g, h, "Expected Graphs to not be equal");
        }

        [Test]
        public void ParsingChainedHandlerGraphAndPaging2()
        {
            EnsureTestData();

            Graph g = new Graph();
            Graph h = new Graph();

            GraphHandler handler1 = new GraphHandler(g);
            PagingHandler handler2 = new PagingHandler(new GraphHandler(h), 100);

            ChainedHandler handler = new ChainedHandler(new IRdfHandler[] { handler2, handler1 });

            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "temp.ttl");

            Assert.AreEqual(100, g.Triples.Count, "Triples should have been limited to 100 (1st Graph)");
            Assert.AreEqual(100, h.Triples.Count, "Triples should have been limited to 100 (2nd Graph)");
            Assert.AreEqual(g.Triples.Count, h.Triples.Count, "Expected same number of Triples");
            Assert.AreEqual(g, h, "Expected Graphs to be equal");
        }
        
        [Test]
        public void ParsingChainedHandlerGraphAndCount()
        {
            EnsureTestData();

            Graph g = new Graph();
            GraphHandler handler1 = new GraphHandler(g);

            CountHandler handler2 = new CountHandler();

            ChainedHandler handler = new ChainedHandler(new IRdfHandler[] { handler1, handler2 });

            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "temp.ttl");

            Assert.AreEqual(g.Triples.Count, handler2.Count, "Expected Counts to be the same");
 
        }

        [Test]
        public void ParsingChainedHandlerGraphAndNull()
        {
            EnsureTestData();

            Graph g = new Graph();
            GraphHandler handler1 = new GraphHandler(g);

            NullHandler handler2 = new NullHandler();

            ChainedHandler handler = new ChainedHandler(new IRdfHandler[] { handler1, handler2 });

            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "temp.ttl");
        }
    }
}
