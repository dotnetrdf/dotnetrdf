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
using NUnit.Framework;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing.Handlers
{
    [TestFixture]
    public class GraphHandlerTests
    {
        [Test]
        public void ParsingGraphHandlerImplicitBaseUriPropogation()
        {
            try
            {
                Options.UriLoaderCaching = false;

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
                Options.UriLoaderCaching = true;
            }
        }

        [Test]
        public void ParsingGraphHandlerImplicitBaseUriPropogation2()
        {
            try
            {
                Options.UriLoaderCaching = false;

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
                Options.UriLoaderCaching = true;
            }
        }

        [Test]
        public void ParsingGraphHandlerImplicitTurtle()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
            Assert.IsTrue(g.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
        }

        #region Explicit GraphHandler Usage

        public void ParsingUsingGraphHandlerExplicitTest(String tempFile, IRdfReader parser, bool nsCheck)
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
            g.SaveToFile(tempFile);

            Graph h = new Graph();
            GraphHandler handler = new GraphHandler(h);
            parser.Load(handler, tempFile);

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in h.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
            Assert.IsTrue(g.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
            Assert.IsFalse(h.IsEmpty, "Graph should not be empty");
            if (nsCheck) Assert.IsTrue(h.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        [Test]
        public void ParsingGraphHandlerExplicitNTriples()
        {
            this.ParsingUsingGraphHandlerExplicitTest("temp.nt", new NTriplesParser(), false);
        }

        [Test]
        public void ParsingGraphHandlerExplicitTurtle()
        {
            this.ParsingUsingGraphHandlerExplicitTest("temp.ttl", new TurtleParser(), true);
        }

        [Test]
        public void ParsingGraphHandlerExplicitNotation3()
        {
            this.ParsingUsingGraphHandlerExplicitTest("temp.n3", new Notation3Parser(), true);
        }

        [Test]
        public void ParsingGraphHandlerExplicitRdfXml()
        {
            this.ParsingUsingGraphHandlerExplicitTest("temp.rdf", new RdfXmlParser(), true);
        }

        [Test]
        public void ParsingGraphHandlerExplicitRdfA()
        {
            this.ParsingUsingGraphHandlerExplicitTest("temp.html", new RdfAParser(), false);
        }

        [Test]
        public void ParsingGraphHandlerExplicitRdfJson()
        {
            this.ParsingUsingGraphHandlerExplicitTest("temp.json", new RdfJsonParser(), false);
        }

        #endregion

        [Test]
        public void ParsingGraphHandlerExplicitMerging()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
            g.SaveToFile("temp.ttl");

            Graph h = new Graph();
            GraphHandler handler = new GraphHandler(h);

            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "temp.ttl");

            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
            Assert.IsTrue(g.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
            Assert.IsFalse(h.IsEmpty, "Graph should not be empty");
            Assert.IsTrue(h.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
            Assert.AreEqual(g, h, "Graphs should be equal");

            parser.Load(handler, "temp.ttl");
            Assert.AreEqual(g.Triples.Count + 2, h.Triples.Count, "Triples count should now be 2 higher due to the merge which will have replicated the 2 triples containing Blank Nodes");
            Assert.AreNotEqual(g, h, "Graphs should no longer be equal");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in h.Triples.Where(x => !x.IsGroundTriple))
            {
                Console.WriteLine(t.ToString(formatter));
            }
        }

        [Test]
        public void ParsingGraphHandlerImplicitMerging()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
            g.SaveToFile("temp.ttl");

            Graph h = new Graph();

            TurtleParser parser = new TurtleParser();
            parser.Load(h, "temp.ttl");

            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
            Assert.IsTrue(g.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
            Assert.IsFalse(h.IsEmpty, "Graph should not be empty");
            Assert.IsTrue(h.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
            Assert.AreEqual(g, h, "Graphs should be equal");

            parser.Load(h, "temp.ttl");
            Assert.AreEqual(g.Triples.Count + 2, h.Triples.Count, "Triples count should now be 2 higher due to the merge which will have replicated the 2 triples containing Blank Nodes");
            Assert.AreNotEqual(g, h, "Graphs should no longer be equal");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in h.Triples.Where(x => !x.IsGroundTriple))
            {
                Console.WriteLine(t.ToString(formatter));
            }
        }

        [Test]
        public void ParsingGraphHandlerImplicitInitialBaseUri()
        {
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/");

            String fragment = "<subject> <predicate> <object> .";
            TurtleParser parser = new TurtleParser();
            parser.Load(g, new StringReader(fragment));

            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
            Assert.AreEqual(1, g.Triples.Count, "Expected 1 Triple to be parsed");
        }

        [Test]
        public void ParsingGraphHandlerExplicitInitialBaseUri()
        {
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/");

            String fragment = "<subject> <predicate> <object> .";
            TurtleParser parser = new TurtleParser();
            GraphHandler handler = new GraphHandler(g);
            parser.Load(handler, new StringReader(fragment));

            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
            Assert.AreEqual(1, g.Triples.Count, "Expected 1 Triple to be parsed");
        }
    }
}
