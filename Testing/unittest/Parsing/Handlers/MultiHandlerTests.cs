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
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing.Handlers
{

    public class MultiHandlerTests
    {
        private void EnsureTestData()
        {
            if (!System.IO.File.Exists("multi_handler_tests_temp.ttl"))
            {
                Graph g = new Graph();
                EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
                g.SaveToFile("multi_handler_tests_temp.ttl");
            }
        }

        [Fact]
        public void ParsingMultiHandlerBadInstantiation()
        {
            Assert.Throws<ArgumentException>(() => new MultiHandler(Enumerable.Empty<IRdfHandler>()));
        }

        [Fact]
        public void ParsingMultiHandlerBadInstantiation2()
        {
            Assert.Throws<ArgumentNullException>(() => new MultiHandler(null));
        }

        [Fact]
        public void ParsingMultiHandlerBadInstantiation3()
        {
            GraphHandler h = new GraphHandler(new Graph());

            Assert.Throws<ArgumentException>(() => new MultiHandler(new IRdfHandler[] { h, h }));
        }

        [Fact]
        public void ParsingMultiHandlerBadInstantiation4()
        {
            GraphHandler h = new GraphHandler(new Graph());

            Assert.Throws<ArgumentNullException>(() => new MultiHandler(new IRdfHandler[] { h }, null));
        }

        [Fact]
        public void ParsingMultiHandlerTwoGraphs()
        {
            EnsureTestData();

            Graph g = new Graph();
            Graph h = new Graph();

            GraphHandler handler1 = new GraphHandler(g);
            GraphHandler handler2 = new GraphHandler(h);

            MultiHandler handler = new MultiHandler(new IRdfHandler[] { handler1, handler2 });

            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "multi_handler_tests_temp.ttl");

            Assert.Equal(g.Triples.Count, h.Triples.Count);
            Assert.Equal(g, h);
        }

        [Fact]
        public void ParsingMultiHandlerGraphAndPaging()
        {
            EnsureTestData();

            Graph g = new Graph();
            Graph h = new Graph();

            GraphHandler handler1 = new GraphHandler(g);
            PagingHandler handler2 = new PagingHandler(new GraphHandler(h), 100);

            MultiHandler handler = new MultiHandler(new IRdfHandler[] { handler1, handler2 });

            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "multi_handler_tests_temp.ttl");

            Assert.Equal(101, g.Triples.Count);
            Assert.Equal(100, h.Triples.Count);
            Assert.NotEqual(g.Triples.Count, h.Triples.Count);
            Assert.NotEqual(g, h);
        }

        [Fact]
        public void ParsingMultiHandlerGraphAndPaging2()
        {
            EnsureTestData();

            Graph g = new Graph();
            Graph h = new Graph();

            GraphHandler handler1 = new GraphHandler(g);
            PagingHandler handler2 = new PagingHandler(new GraphHandler(h), 100);

            MultiHandler handler = new MultiHandler(new IRdfHandler[] { handler2, handler1 });

            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "multi_handler_tests_temp.ttl");

            Assert.Equal(101, g.Triples.Count);
            Assert.Equal(100, h.Triples.Count);
            Assert.NotEqual(g.Triples.Count, h.Triples.Count);
            Assert.NotEqual(g, h);
        }

        [Fact]
        public void ParsingMultiHandlerGraphAndCount()
        {
            EnsureTestData();

            Graph g = new Graph();
            GraphHandler handler1 = new GraphHandler(g);

            CountHandler handler2 = new CountHandler();

            MultiHandler handler = new MultiHandler(new IRdfHandler[] { handler1, handler2 });

            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "multi_handler_tests_temp.ttl");

            Assert.Equal(g.Triples.Count, handler2.Count);

        }

        [Fact]
        public void ParsingMultiHandlerGraphAndNull()
        {
            EnsureTestData();

            Graph g = new Graph();
            GraphHandler handler1 = new GraphHandler(g);

            NullHandler handler2 = new NullHandler();

            MultiHandler handler = new MultiHandler(new IRdfHandler[] { handler1, handler2 });

            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "multi_handler_tests_temp.ttl");
        }

        [Fact]
        public void ParsingMultiHandlerGraphWithFactory()
        {
            EnsureTestData();

            Graph g = new Graph();
            GraphHandler h = new GraphHandler(g);

            MultiHandler handler = new MultiHandler(new IRdfHandler[] { h }, g);

            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "multi_handler_tests_temp.ttl");

            Assert.Same(g, g.Triples.First().Graph);
        }
    }
}
