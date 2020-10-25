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
using FluentAssertions;
using Xunit;

namespace VDS.RDF.Parsing
{
    public class RdfATests
    {
        [Fact]
        public void ParsingRdfABadSyntax()
        {
            //Tests parsing a file which has much invalid RDFa syntax in it, some triples will be produced (6-8) but most of the triples are wrongly encoded and will be ignored
            var g = new Graph {BaseUri = new Uri("http://www.wurvoc.org/vocabularies/om-1.6/Kelvin_scale")};
            FileLoader.Load(g, "resources\\bad_rdfa.html");
            g.IsEmpty.Should().BeFalse("some triples should have been harvested from the bad RDFa example");
        }

        [Fact]
        public void ParsingRdfAGoodRelations()
        {
            var tests = new List<string>
            {
                "gr1",
                "gr2",
                "gr3"
            };

            FileLoader.Warning += TestTools.WarningPrinter;

            foreach (var test in tests)
            {
                var g = new Graph
                {
                    BaseUri = new Uri("http://example.org/goodrelations")
                };
                var h = new Graph
                {
                    BaseUri = g.BaseUri
                };

                FileLoader.Load(g, $"resources\\{test}.xhtml");
                FileLoader.Load(h, $"resources\\{test}b.xhtml");
                Assert.Equal(g, h);
            }
        }

        [Fact]
        public void ParsingRdfABadProfile()
        {
            var parser = new RdfAParser(RdfASyntax.RDFa_1_1);
            parser.Warning += TestTools.WarningPrinter;

            var g = new Graph();
            parser.Load(g, "resources\\bad_profile.xhtml");

            TestTools.ShowGraph(g);

            Assert.Equal(1, g.Triples.Count);
        }
    }
}
