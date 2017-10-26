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
using Xunit;
using VDS.RDF.Parsing;

namespace VDS.RDF.Parsing
{

    public class NamespaceTests
    {
        private const String TurtleExample = @"[] a <relative> .";

        [Fact]
        public void ParsingRelativeUriAppBaseRdfXml1()
        {
            //This invocation succeeds because when invoking via the FileLoader
            //the Base URI will be set to the file URI

            Graph g = new Graph();
            RdfXmlParser parser = new RdfXmlParser();
            g.LoadFromFile("resources\\rdfxml-relative-uri.rdf", parser);

            //Expect a non-empty grpah with a single triple
            Assert.False(g.IsEmpty);
            Assert.Equal(1, g.Triples.Count);
            Triple t = g.Triples.First();

            //Object should get it's relative URI resolved into
            //a File URI
            Uri obj = ((IUriNode)t.Object).Uri;
            Assert.True(obj.IsFile);
            Assert.Equal("relative", obj.Segments[obj.Segments.Length - 1]);
        }

        [Fact]
        public void ParsingRelativeUriAppBaseRdfXml2()
        {
            //This invocation succeeds because when invoking because
            //we manually set the Base URI prior to invoking the parser

            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org");
            RdfXmlParser parser = new RdfXmlParser();
            parser.Load(g, "resources\\rdfxml-relative-uri.rdf");

            //Expect a non-empty grpah with a single triple
            Assert.False(g.IsEmpty);
            Assert.Equal(1, g.Triples.Count);
            Triple t = g.Triples.First();

            //Object should get it's relative URI resolved into
            //the correct HTTP URI
            Uri obj = ((IUriNode)t.Object).Uri;
            Assert.Equal("http", obj.Scheme);
            Assert.Equal("example.org", obj.Host);
            Assert.Equal("relative", obj.Segments[1]);
        }

        [Fact]
        public void ParsingRelativeUriNoBaseRdfXml()
        {
            //This invocation fails because when invoking the parser directly
            //the Base URI is not set to the file URI

            Graph g = new Graph();
            RdfXmlParser parser = new RdfXmlParser();

            Assert.Throws<RdfParseException>(() => parser.Load(g, "resources\\rdfxml-relative-uri.rdf"));
        }

        [Fact]
        public void ParsingRelativeUriNoBaseTurtle()
        {
            //This invocation fails because there is no Base URI to
            //resolve against
            Graph g = new Graph();
            TurtleParser parser = new TurtleParser();

            Assert.Throws<RdfParseException>(() => parser.Load(g, new StringReader(TurtleExample)));
        }

        [Fact]
        public void ParsingRelativeUriAppBaseTurtle()
        {
            //This invocation succeeds because we define a Base URI
            //resolve against
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org");
            TurtleParser parser = new TurtleParser();
            parser.Load(g, new StringReader(TurtleExample));

            //Expect a non-empty grpah with a single triple
            Assert.False(g.IsEmpty);
            Assert.Equal(1, g.Triples.Count);
            Triple t = g.Triples.First();

            //Predicate should get it's relative URI resolved into
            //the correct HTTP URI
            Uri obj = ((IUriNode)t.Object).Uri;
            Assert.Equal("http", obj.Scheme);
            Assert.Equal("example.org", obj.Host);
            Assert.Equal("relative", obj.Segments[1]);
        }
    }
}
