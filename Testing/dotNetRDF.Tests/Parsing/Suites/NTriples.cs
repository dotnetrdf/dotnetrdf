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
using VDS.RDF.XunitExtensions;
using Xunit;

namespace VDS.RDF.Parsing.Suites
{

    public class NTriples
        : BaseRdfParserSuite
    {
        public NTriples()
            : base(new NTriplesParser(NTriplesSyntax.Original), new NTriplesParser(NTriplesSyntax.Original), @"ntriples\")
        {
            this.CheckResults = false;
        }

        [SkippableFact]
        public void ParsingSuiteNTriples()
        {
            //Run manifests
            this.RunDirectory(f => Path.GetExtension(f).Equals(".nt"), true);

            if (this.Count == 0) Assert.True(false, "No tests found");

            Console.WriteLine(this.Count + " Tests - " + this.Passed + " Passed - " + this.Failed + " Failed");
            Console.WriteLine((((double) this.Passed/(double) this.Count)*100) + "% Passed");

            if (this.Failed > 0) Assert.True(false, this.Failed + " Tests failed");
            if (this.Indeterminate > 0) throw new SkipTestException(this.Indeterminate + " Tests are indeterminate");
        }

        [Fact]
        public void ParsingNTriplesUnicodeEscapes1()
        {
            Graph g = new Graph();
            g.LoadFromFile(@"resources\turtle11\localName_with_assigned_nfc_bmp_PN_CHARS_BASE_character_boundaries.nt", this.Parser);
            Assert.False(g.IsEmpty);
            Assert.Equal(1, g.Triples.Count);
        }

        [Fact]
        public void ParsingNTriplesComplexBNodeIDs()
        {
            const String data = @"_:node-id.complex_id.blah <http://p> <http://o> .
<http://s> <http://p> _:node.id.";

            Graph g = new Graph();
            Assert.Throws<RdfParseException>(() => g.LoadFromString(data, this.Parser));
        }

        [Fact]
        public void ParsingNTriplesLiteralEscapes1()
        {
            const String data = @"<http://s> <http://p> ""literal\'quote"" .";

            Graph g = new Graph();
            Assert.Throws<RdfParseException>(() => g.LoadFromString(data, this.Parser));
        }

        [Fact]
        public void ParsingNTriplesLiteralEscapes2()
        {
            const String data = @"<http://s> <http://p> ""literal\""quote"" .";

            Graph g = new Graph();
            g.LoadFromString(data, this.Parser);

            Assert.False(g.IsEmpty);
            Assert.Equal(1, g.Triples.Count);
        }
    }


    public class NTriples11
        : BaseRdfParserSuite
    {
        public NTriples11()
            : base(new NTriplesParser(), new NTriplesParser(), @"ntriples11\")
        {
            this.CheckResults = false;
            this.Parser.Warning += TestTools.WarningPrinter;
        }

        [SkippableFact]
        public void ParsingSuiteNTriples11()
        {
            //Nodes for positive and negative tests
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("rdft", UriFactory.Create("http://www.w3.org/ns/rdftest#"));
            INode posSyntaxTest = g.CreateUriNode("rdft:TestNTriplesPositiveSyntax");
            INode negSyntaxTest = g.CreateUriNode("rdft:TestNTriplesNegativeSyntax");

            //Run manifests
            this.RunManifest(@"resources\ntriples11\manifest.ttl", posSyntaxTest, negSyntaxTest);

            if (this.Count == 0) Assert.True(false, "No tests found");

            Console.WriteLine(this.Count + " Tests - " + this.Passed + " Passed - " + this.Failed + " Failed");
            Console.WriteLine((((double) this.Passed/(double) this.Count)*100) + "% Passed");

            if (this.Failed > 0) Assert.True(false, this.Failed + " Tests failed");
            if (this.Indeterminate > 0) throw new SkipTestException(this.Indeterminate + " Tests are indeterminate");
        }

        [Fact]
        public void ParsingNTriples11ComplexBNodeIDs()
        {
            const String data = @"_:node-id.complex_id.blah <http://p> <http://o> .
<http://s> <http://p> _:node.id.";

            Graph g = new Graph();
            g.LoadFromString(data, this.Parser);
            Assert.False(g.IsEmpty);
            Assert.Equal(2, g.Triples.Count);
        }

        [Fact]
        public void ParsingNTriples11LiteralEscapes1()
        {
            const String data = @"<http://s> <http://p> ""literal\'quote"" .";

            Graph g = new Graph();
            g.LoadFromString(data, this.Parser);

            Assert.False(g.IsEmpty);
            Assert.Equal(1, g.Triples.Count);
        }

        [Fact]
        public void ParsingNTriples11LiteralEscapes2()
        {
            const String data = @"<http://s> <http://p> ""literal\""quote"" .";

            Graph g = new Graph();
            g.LoadFromString(data, this.Parser);

            Assert.False(g.IsEmpty);
            Assert.Equal(1, g.Triples.Count);
        }
    }
}