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
using System.IO;
using Xunit;

namespace VDS.RDF.Parsing;

[Collection("RdfServer")]
public class RdfATests(RdfServerFixture server)
{
    [Fact]
    public void ParsingRdfABadSyntax()
    {
        //Tests parsing a file which has much invalid RDFa syntax in it, some triples will be produced (6-8) but most of the triples are wrongly encoded and will be ignored
        var g = new Graph {BaseUri = new Uri("http://www.wurvoc.org/vocabularies/om-1.6/Kelvin_scale")};
        FileLoader.Load(g, Path.Combine("resources", "bad_rdfa.html"));
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
        var testGraphName = new UriNode(new Uri("http://example.org/goodrelations"));
        foreach (var test in tests)
        {
            var g = new Graph(testGraphName)
            {
                BaseUri = new Uri("http://example.org/goodrelations")
            };
            var h = new Graph(testGraphName)
            {
                BaseUri = g.BaseUri
            };

            var parser =
                new RdfAParser(new RdfAParserOptions() { Base = new Uri("http://example.org/goodrelations") });
            parser.Load(g, Path.Combine("resources", $"{test}.xhtml"));
            parser.Load(h, Path.Combine("resources", $"{test}b.xhtml"));
            Assert.Equal(g, h);
        }
    }

    [Fact]
    public void ParsingRdfABadProfile()
    {
        var parser = new RdfAParser(RdfASyntax.RDFa_1_1);
        parser.Warning += TestTools.WarningPrinter;

        var g = new Graph();
        parser.Load(g, Path.Combine("resources", "bad_profile.xhtml"));

        TestTools.ShowGraph(g);

        Assert.Equal(1, g.Triples.Count);
    }

    [Fact(DisplayName = "Populates base URI from handler if missing from options")]
    public void BaseFromHandler()
    {
        var load = () => new Graph().LoadFromUri(server.UriFor("/dbpedia_ldf.html"));

        load.Should().NotThrow("because the parser should set its base URI from the handler if its options don't have one");
    }
}
