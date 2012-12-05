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
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Parsing
{
   
    [TestClass]
    public class Turtle11
        : BaseRdfParserSuite
    {
        public Turtle11()
            : base(new TurtleParser(TurtleSyntax.W3C), new NTriplesParser(), "turtle11\\") { }

        [TestMethod]
        public void ParsingTurtleW3CSuite()
        {
            //Run manifests
            this.RunManifest("turtle11/manifest.ttl", true);
            this.RunManifest("turtle11/manifest-bad.ttl", false);

            if (this.Count == 0) Assert.Fail("No tests found");

            Console.WriteLine(this.Count + " Tests - " + this.Passed + " Passed - " + this.Failed + " Failed");
            Console.WriteLine((((double)this.Passed / (double)this.Count) * 100) + "% Passed");

            if (this.Failed > 0) Assert.Fail(this.Failed + " Tests failed");
            if (this.Indeterminate > 0) Assert.Inconclusive(this.Indeterminate + " Tests are indeterminate");
        }

        [TestMethod]
        public void ParsingTurtleW3CBaseTurtleStyle1()
        {
            //Dot required
            String graph = "@base <http://example.org/> .";
            Graph g = new Graph();
            this._parser.Load(g, new StringReader(graph));

            Assert.AreEqual(new Uri("http://example.org"), g.BaseUri);
        }

        [TestMethod,ExpectedException(typeof(RdfParseException))]
        public void ParsingTurtleW3CBaseTurtleStyle2()
        {
            //Missing dot
            String graph = "@base <http://example.org/>";
            Graph g = new Graph();
            this._parser.Load(g, new StringReader(graph));

            Assert.AreEqual(new Uri("http://example.org"), g.BaseUri);
        }

        [TestMethod,ExpectedException(typeof(RdfParseException))]
        public void ParsingTurtleW3CBaseSparqlStyle1()
        {
            //Forbidden dot
            String graph = "BASE <http://example.org/> .";
            Graph g = new Graph();
            this._parser.Load(g, new StringReader(graph));

            Assert.AreEqual(new Uri("http://example.org"), g.BaseUri);
        }

        [TestMethod]
        public void ParsingTurtleW3CBaseSparqlStyle2()
        {
            //No dot required
            String graph = "BASE <http://example.org/>";
            Graph g = new Graph();
            this._parser.Load(g, new StringReader(graph));

            Assert.AreEqual(new Uri("http://example.org"), g.BaseUri);
        }

        [TestMethod]
        public void ParsingTurtleW3CPrefixTurtleStyle1()
        {
            //Dot required
            String graph = "@prefix ex: <http://example.org/> .";
            Graph g = new Graph();
            this._parser.Load(g, new StringReader(graph));

            Assert.AreEqual(new Uri("http://example.org"), g.NamespaceMap.GetNamespaceUri("ex"));
        }

        [TestMethod, ExpectedException(typeof(RdfParseException))]
        public void ParsingTurtleW3CPrefixTurtleStyle2()
        {
            //Missing dot
            String graph = "@prefix ex: <http://example.org/>";
            Graph g = new Graph();
            this._parser.Load(g, new StringReader(graph));

            Assert.AreEqual(new Uri("http://example.org"), g.NamespaceMap.GetNamespaceUri("ex"));
        }

        [TestMethod, ExpectedException(typeof(RdfParseException))]
        public void ParsingTurtleW3CPrefixSparqlStyle1()
        {
            //Forbidden dot
            String graph = "PREFIX ex: <http://example.org/> .";
            Graph g = new Graph();
            this._parser.Load(g, new StringReader(graph));

            Assert.AreEqual(new Uri("http://example.org"), g.NamespaceMap.GetNamespaceUri("ex"));
        }

        [TestMethod]
        public void ParsingTurtleW3CPrefixSparqlStyle2()
        {
            //No dot required
            String graph = "PREFIX ex: <http://example.org/>";
            Graph g = new Graph();
            this._parser.Load(g, new StringReader(graph));

            Assert.AreEqual(new Uri("http://example.org"), g.NamespaceMap.GetNamespaceUri("ex"));
        }
    }
}
