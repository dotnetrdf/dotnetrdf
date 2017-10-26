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
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;
using VDS.RDF.XunitExtensions;

namespace VDS.RDF.Parsing.Suites
{

    public class TurtleMemberSubmission
        : BaseRdfParserSuite
    {
        public TurtleMemberSubmission()
            : base(new TurtleParser(TurtleSyntax.Original), new NTriplesParser(), "turtle\\") { }

        [SkippableFact]
        public void ParsingSuiteTurtleOriginal()
        {
            //Run manifests
            this.RunManifest("resources/turtle/manifest.ttl", true);
            this.RunManifest("resources/turtle/manifest-bad.ttl", false);

            if (this.Count == 0) Assert.True(false, "No tests found");

            Console.WriteLine(this.Count + " Tests - " + this.Passed + " Passed - " + this.Failed + " Failed");
            Console.WriteLine((((double)this.Passed / (double)this.Count) * 100) + "% Passed");

            if (this.Failed > 0) Assert.True(false, this.Failed + " Tests failed");
            if (this.Indeterminate > 0) throw new SkipTestException(this.Indeterminate + " Tests are indeterminate");
        }

        [Fact]
        public void ParsingTurtleOriginalBaseTurtleStyle1()
        {
            //Dot required
            String graph = "@base <http://example.org/> .";
            Graph g = new Graph();
            this.Parser.Load(g, new StringReader(graph));

            Assert.Equal(new Uri("http://example.org"), g.BaseUri);
        }

        [Fact]
        public void ParsingTurtleOriginalBaseTurtleStyle2()
        {
            //Missing dot
            String graph = "@base <http://example.org/>";
            Graph g = new Graph();
            Assert.Throws<RdfParseException>(() => this.Parser.Load(g, new StringReader(graph)));

            Assert.Equal(new Uri("http://example.org"), g.BaseUri);
        }

        [Fact]
        public void ParsingTurtleOriginalBaseSparqlStyle1()
        {
            //Forbidden in Original Turtle
            String graph = "BASE <http://example.org/> .";
            Graph g = new Graph();
            Assert.Throws<RdfParseException>(() => this.Parser.Load(g, new StringReader(graph)));
        }

        [Fact]
        public void ParsingTurtleOriginalBaseSparqlStyle2()
        {
            //Forbidden in Original Turtle
            String graph = "BASE <http://example.org/>";
            Graph g = new Graph();
            Assert.Throws<RdfParseException>(() => this.Parser.Load(g, new StringReader(graph)));
        }

        [Fact]
        public void ParsingTurtleOriginalPrefixTurtleStyle1()
        {
            //Dot required
            String graph = "@prefix ex: <http://example.org/> .";
            Graph g = new Graph();
            this.Parser.Load(g, new StringReader(graph));
            Assert.Equal(new Uri("http://example.org"), g.NamespaceMap.GetNamespaceUri("ex"));
        }

        [Fact]
        public void ParsingTurtleOriginalPrefixTurtleStyle2()
        {
            //Missing dot
            String graph = "@prefix ex: <http://example.org/>";
            Graph g = new Graph();
            Assert.Throws<RdfParseException>(() => this.Parser.Load(g, new StringReader(graph)));

            Assert.Equal(new Uri("http://example.org"), g.NamespaceMap.GetNamespaceUri("ex"));
        }

        [Fact]
        public void ParsingTurtleOriginalPrefixSparqlStyle1()
        {
            //Forbidden in Original Turtle
            String graph = "PREFIX ex: <http://example.org/> .";
            Graph g = new Graph();
            Assert.Throws<RdfParseException>(() => this.Parser.Load(g, new StringReader(graph)));
        }

        [Fact]
        public void ParsingTurtleOriginalPrefixSparqlStyle2()
        {
            //Forbidden in Original Turtle
            String graph = "PREFIX ex: <http://example.org/>";
            Graph g = new Graph();
            Assert.Throws<RdfParseException>(() => this.Parser.Load(g, new StringReader(graph)));
        }

        [Fact]
        public void ParsingTurtleOriginalPrefixedNames1()
        {
            Assert.True(TurtleSpecsHelper.IsValidQName(":a1", TurtleSyntax.Original));
        }

        [Fact]
        public void ParsingTurtleOriginalPrefixedNames2()
        {
            this.Parser.Load(new Graph(), @"resources\turtle\test-14.ttl");
        }
    }
}
