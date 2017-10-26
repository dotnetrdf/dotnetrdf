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
    public partial class CountHandlerTests
    {
        private void ParsingUsingCountHandler(String tempFile, IRdfReader parser)
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
            g.SaveToFile(tempFile);

            CountHandler handler = new CountHandler();
            parser.Load(handler, tempFile);

            Console.WriteLine("Counted " + handler.Count + " Triples");
            Assert.Equal(g.Triples.Count, handler.Count);
        }

        [Fact]
        public void ParsingCountHandlerNTriples()
        {
            this.ParsingUsingCountHandler("test.nt", new NTriplesParser());
        }

        [Fact]
        public void ParsingCountHandlerTurtle()
        {
            this.ParsingUsingCountHandler("test.ttl", new TurtleParser());
        }

        [Fact]
        public void ParsingCountHandlerNotation3()
        {
            this.ParsingUsingCountHandler("temp.n3", new Notation3Parser());
        }

        [Fact]
        public void ParsingCountHandlerRdfA()
        {
            this.ParsingUsingCountHandler("test.html", new RdfAParser());
        }

        [Fact]
        public void ParsingCountHandlerRdfJson()
        {
            this.ParsingUsingCountHandler("test.json", new RdfJsonParser());
        }
    }
}
