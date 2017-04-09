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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing.Handlers
{

    public class GraphHandlerTests
    {
#if !NO_SYNC_HTTP
        [Fact]
        public void ParsingGraphHandlerImplicitBaseUriPropogation()
        {
            try
            {
#if NET40
                Options.UriLoaderCaching = false;
#endif

                Graph g = new Graph();
                UriLoader.Load(g, new Uri("http://wiki.rkbexplorer.com/id/void"));
                NTriplesFormatter formatter = new NTriplesFormatter();
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
            }
            finally
            {
#if NET40
                Options.UriLoaderCaching = true;
#endif
            }
        }

        [Fact]
        public void ParsingGraphHandlerImplicitBaseUriPropogation2()
        {
            try
            {
#if NET40
                Options.UriLoaderCaching = false;
#endif

                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                UriLoader.Load(g, new Uri("http://wiki.rkbexplorer.com/id/void"));
                NTriplesFormatter formatter = new NTriplesFormatter();
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
            }
            finally
            {
#if NET40
                Options.UriLoaderCaching = true;
#endif
            }
        }
#endif

        [Fact]
        public void ParsingGraphHandlerImplicitTurtle()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.False(g.IsEmpty, "Graph should not be empty");
            Assert.True(g.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
        }

        #region Explicit GraphHandler Usage

        public void ParsingUsingGraphHandlerExplicitTest(String tempFile, IRdfReader parser, bool nsCheck)
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
            g.SaveToFile(tempFile);

            Graph h = new Graph();
            GraphHandler handler = new GraphHandler(h);
#if PORTABLE
            using (var reader = new StreamReader(tempFile))
            {
                parser.Load(handler, reader);
            }
#else
            parser.Load(handler, tempFile);
#endif

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in h.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.False(g.IsEmpty, "Graph should not be empty");
            Assert.True(g.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
            Assert.False(h.IsEmpty, "Graph should not be empty");
            if (nsCheck) Assert.True(h.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
            Assert.Equal(g, h);
        }

        [Fact]
        public void ParsingGraphHandlerExplicitNTriples()
        {
            this.ParsingUsingGraphHandlerExplicitTest("graph_handler_tests_temp.nt", new NTriplesParser(), false);
        }

        [Fact]
        public void ParsingGraphHandlerExplicitTurtle()
        {
            this.ParsingUsingGraphHandlerExplicitTest("graph_handler_tests_temp.ttl", new TurtleParser(), true);
        }

        [Fact]
        public void ParsingGraphHandlerExplicitNotation3()
        {
            this.ParsingUsingGraphHandlerExplicitTest("graph_handler_tests_temp.n3", new Notation3Parser(), true);
        }

#if !NO_XMLENTITIES
        [Fact]
        public void ParsingGraphHandlerExplicitRdfXml()
        {
            this.ParsingUsingGraphHandlerExplicitTest("graph_handler_tests_temp.rdf", new RdfXmlParser(), true);
        }
#endif

        [Fact]
        public void ParsingGraphHandlerExplicitRdfA()
        {
            this.ParsingUsingGraphHandlerExplicitTest("graph_handler_tests_temp.html", new RdfAParser(), false);
        }

        [Fact]
        public void ParsingGraphHandlerExplicitRdfJson()
        {
            this.ParsingUsingGraphHandlerExplicitTest("graph_handler_tests_temp.json", new RdfJsonParser(), false);
        }

        #endregion

        [Fact]
        public void ParsingGraphHandlerExplicitMerging()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
            g.SaveToFile("graph_handler_tests_temp.ttl");

            Graph h = new Graph();
            GraphHandler handler = new GraphHandler(h);

            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "graph_handler_tests_temp.ttl");

            Assert.False(g.IsEmpty, "Graph should not be empty");
            Assert.True(g.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
            Assert.False(h.IsEmpty, "Graph should not be empty");
            Assert.True(h.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
            Assert.Equal(g, h);

            parser.Load(handler, "graph_handler_tests_temp.ttl");
            Assert.Equal(g.Triples.Count + 2, h.Triples.Count);
            Assert.NotEqual(g, h);

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in h.Triples.Where(x => !x.IsGroundTriple))
            {
                Console.WriteLine(t.ToString(formatter));
            }
        }

        [Fact]
        public void ParsingGraphHandlerImplicitMerging()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
            g.SaveToFile("graph_handler_tests_temp.ttl");

            Graph h = new Graph();

            TurtleParser parser = new TurtleParser();
            parser.Load(h, "graph_handler_tests_temp.ttl");

            Assert.False(g.IsEmpty, "Graph should not be empty");
            Assert.True(g.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
            Assert.False(h.IsEmpty, "Graph should not be empty");
            Assert.True(h.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
            Assert.Equal(g, h);

            parser.Load(h, "graph_handler_tests_temp.ttl");
            Assert.Equal(g.Triples.Count + 2, h.Triples.Count);
            Assert.NotEqual(g, h);

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in h.Triples.Where(x => !x.IsGroundTriple))
            {
                Console.WriteLine(t.ToString(formatter));
            }
        }

        [Fact]
        public void ParsingGraphHandlerImplicitInitialBaseUri()
        {
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/");

            String fragment = "<subject> <predicate> <object> .";
            TurtleParser parser = new TurtleParser();
            parser.Load(g, new StringReader(fragment));

            Assert.False(g.IsEmpty, "Graph should not be empty");
            Assert.Equal(1, g.Triples.Count);
        }

        [Fact]
        public void ParsingGraphHandlerExplicitInitialBaseUri()
        {
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/");

            String fragment = "<subject> <predicate> <object> .";
            TurtleParser parser = new TurtleParser();
            GraphHandler handler = new GraphHandler(g);
            parser.Load(handler, new StringReader(fragment));

            Assert.False(g.IsEmpty, "Graph should not be empty");
            Assert.Equal(1, g.Triples.Count);
        }
    }
}
