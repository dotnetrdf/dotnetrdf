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

using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Writing;
using System.Net.Http;

namespace VDS.RDF;


public class MimeTypesTests : IDisposable
{
    public MimeTypesTests()
    {
        MimeTypesHelper.ResetDefinitions();
    }

    public void Dispose()
    {
        MimeTypesHelper.ResetDefinitions();
    }

    [Fact]
    public void MimeTypesGetDefinitionsAll()
    {
        var count = MimeTypesHelper.Definitions.Count();
        Assert.Equal(32, count);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeAny()
    {
        var count = MimeTypesHelper.GetDefinitions(MimeTypesHelper.Any).Count();
        Assert.Equal(32, count);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeNotation3_1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/n3");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(Notation3Parser), d.RdfParserType);
        Assert.Equal(typeof(Notation3Writer), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedNotation3Parser), d.RdfParserType);
        Assert.Equal(typeof(GZippedNotation3Writer), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeNotation3_2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/rdf+n3");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(Notation3Parser), d.RdfParserType);
        Assert.Equal(typeof(Notation3Writer), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedNotation3Parser), d.RdfParserType);
        Assert.Equal(typeof(GZippedNotation3Writer), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtNotation3_1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".n3");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(Notation3Parser), d.RdfParserType);
        Assert.Equal(typeof(Notation3Writer), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtNotation3_2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("n3");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(Notation3Parser), d.RdfParserType);
        Assert.Equal(typeof(Notation3Writer), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtNotation3_3()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".n3.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedNotation3Parser), d.RdfParserType);
        Assert.Equal(typeof(GZippedNotation3Writer), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtNotation3_4()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("n3.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedNotation3Parser), d.RdfParserType);
        Assert.Equal(typeof(GZippedNotation3Writer), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeTurtle1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/turtle");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(TurtleParser), d.RdfParserType);
        Assert.Equal(typeof(CompressingTurtleWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedTurtleParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedTurtleWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeTurtle2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/x-turtle");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(TurtleParser), d.RdfParserType);
        Assert.Equal(typeof(CompressingTurtleWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedTurtleParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedTurtleWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeTurtle3()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/turtle");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(TurtleParser), d.RdfParserType);
        Assert.Equal(typeof(CompressingTurtleWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedTurtleParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedTurtleWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtTurtle1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".ttl");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(TurtleParser), d.RdfParserType);
        Assert.Equal(typeof(CompressingTurtleWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtTurtle2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("ttl");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(TurtleParser), d.RdfParserType);
        Assert.Equal(typeof(CompressingTurtleWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtTurtle3()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".ttl.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedTurtleParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedTurtleWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtTurtle4()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("ttl.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedTurtleParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedTurtleWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeNTriples1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/rdf-triples");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(NTriplesParser), d.RdfParserType);
        Assert.Equal(typeof(NTriplesWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedNTriplesParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedNTriplesWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeNTriples2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/plain");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(NTriplesParser), d.RdfParserType);
        Assert.Equal(typeof(NTriplesWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedNTriplesParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedNTriplesWriter), d.RdfWriterType);
    }
   
    [Fact]
    public void MimeTypesGetDefinitionsByTypeNTriples3()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/ntriples");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(NTriplesParser), d.RdfParserType);
        Assert.Equal(typeof(NTriplesWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedNTriplesParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedNTriplesWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeNTriples4()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/ntriples+turtle");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(NTriplesParser), d.RdfParserType);
        Assert.Equal(typeof(NTriplesWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedNTriplesParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedNTriplesWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeNTriples5()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/x-ntriples");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(NTriplesParser), d.RdfParserType);
        Assert.Equal(typeof(NTriplesWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedNTriplesParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedNTriplesWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtNTriples1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".nt");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(NTriplesParser), d.RdfParserType);
        Assert.Equal(typeof(NTriplesWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtNTriples2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("nt");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(NTriplesParser), d.RdfParserType);
        Assert.Equal(typeof(NTriplesWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtNTriples3()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".nt.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedNTriplesParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedNTriplesWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtNTriples4()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("nt.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedNTriplesParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedNTriplesWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeRdfXml1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/rdf+xml");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(RdfXmlParser), d.RdfParserType);
        Assert.Equal(typeof(RdfXmlWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedRdfXmlParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedRdfXmlWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeRdfXml2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/xml");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(RdfXmlParser), d.RdfParserType);
        Assert.Equal(typeof(RdfXmlWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedRdfXmlParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedRdfXmlWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeRdfXml3()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/xml");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(RdfXmlParser), d.RdfParserType);
        Assert.Equal(typeof(RdfXmlWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedRdfXmlParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedRdfXmlWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfXml1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".rdf");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(RdfXmlParser), d.RdfParserType);
        Assert.Equal(typeof(RdfXmlWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfXml2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("rdf");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(RdfXmlParser), d.RdfParserType);
        Assert.Equal(typeof(RdfXmlWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfXml3()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".rdf.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedRdfXmlParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedRdfXmlWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfXml4()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("rdf.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedRdfXmlParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedRdfXmlWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeRdfJson1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/json");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(RdfJsonParser), d.RdfParserType);
        Assert.Equal(typeof(RdfJsonWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedRdfJsonParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedRdfJsonWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeRdfJson2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/json");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(RdfJsonParser), d.RdfParserType);
        Assert.Equal(typeof(RdfJsonWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedRdfJsonParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedRdfJsonWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfJson1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".rj");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(RdfJsonParser), d.RdfParserType);
        Assert.Equal(typeof(RdfJsonWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfJson2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("rj");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(RdfJsonParser), d.RdfParserType);
        Assert.Equal(typeof(RdfJsonWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfJson3()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".rj.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedRdfJsonParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedRdfJsonWriter), d.RdfWriterType);
    }
    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfJson4()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("rj.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedRdfJsonParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedRdfJsonWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeRdfA1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/xhtml+xml");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(RdfAParser), d.RdfParserType);
        Assert.Equal(typeof(HtmlWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedRdfAParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedRdfAWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeRdfA2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/html");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(RdfAParser), d.RdfParserType);
        Assert.Equal(typeof(HtmlWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedRdfAParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedRdfAWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfA1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".html");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(RdfAParser), d.RdfParserType);
        Assert.Equal(typeof(HtmlWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfA2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("html");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(RdfAParser), d.RdfParserType);
        Assert.Equal(typeof(HtmlWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfA3()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".html.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedRdfAParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedRdfAWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfA4()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("html.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedRdfAParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedRdfAWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfA5()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".htm");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(RdfAParser), d.RdfParserType);
        Assert.Equal(typeof(HtmlWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfA6()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("htm");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(RdfAParser), d.RdfParserType);
        Assert.Equal(typeof(HtmlWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfA7()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".htm.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedRdfAParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedRdfAWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfA8()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("htm.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedRdfAParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedRdfAWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfA9()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".xhtml");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(RdfAParser), d.RdfParserType);
        Assert.Equal(typeof(HtmlWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfA10()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("xhtml");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(RdfAParser), d.RdfParserType);
        Assert.Equal(typeof(HtmlWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfA11()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".xhtml.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedRdfAParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedRdfAWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtRdfA12()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("xhtml.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedRdfAParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedRdfAWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeSparqlXml1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/sparql-results+xml");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(SparqlXmlParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(SparqlXmlWriter), d.SparqlResultsWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedSparqlXmlParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(GZippedSparqlXmlWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtSparqlXml1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".srx");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(SparqlXmlParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(SparqlXmlWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtSparqlXml2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("srx");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(SparqlXmlParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(SparqlXmlWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtSparqlXml3()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".srx.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedSparqlXmlParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(GZippedSparqlXmlWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtSparqlXml4()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("srx.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedSparqlXmlParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(GZippedSparqlXmlWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeSparqlJson1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/sparql-results+json");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(SparqlJsonParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(SparqlJsonWriter), d.SparqlResultsWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedSparqlJsonParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(GZippedSparqlJsonWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtSparqlJson1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".srj");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(SparqlJsonParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(SparqlJsonWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtSparqlJson2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("srj");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(SparqlJsonParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(SparqlJsonWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtSparqlJson3()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".srj.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedSparqlJsonParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(GZippedSparqlJsonWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtSparqlJson4()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("srj.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedSparqlJsonParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(GZippedSparqlJsonWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeSparqlCsv1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/csv");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(SparqlCsvParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(SparqlCsvWriter), d.SparqlResultsWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedSparqlCsvParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(GZippedSparqlCsvWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeSparqlCsv2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/comma-separated-values");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(SparqlCsvParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(SparqlCsvWriter), d.SparqlResultsWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedSparqlCsvParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(GZippedSparqlCsvWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtSparqlCsv1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".csv");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(SparqlCsvParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(SparqlCsvWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtSparqlCsv2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("csv");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(SparqlCsvParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(SparqlCsvWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtSparqlCsv3()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".csv.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedSparqlCsvParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(GZippedSparqlCsvWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtSparqlCsv4()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("csv.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedSparqlCsvParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(GZippedSparqlCsvWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeSparqlTsv1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/tab-separated-values");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(SparqlTsvParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(SparqlTsvWriter), d.SparqlResultsWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedSparqlTsvParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(GZippedSparqlTsvWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtSparqlTsv1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".tsv");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(SparqlTsvParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(SparqlTsvWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtSparqlTsv2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("tsv");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(SparqlTsvParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(SparqlTsvWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtSparqlTsv3()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".tsv.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedSparqlTsvParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(GZippedSparqlTsvWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtSparqlTsv4()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("tsv.gz");
        Assert.Single(defs);

        //Check GZipped definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(GZippedSparqlTsvParser), d.SparqlResultsParserType);
        Assert.Equal(typeof(GZippedSparqlTsvWriter), d.SparqlResultsWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeJsonLd1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/ld+json");
        Assert.Equal(2, defs.Count());
        AssertIsJsonLd(defs.First());
        AssertIsGzippedJsonLd(defs.Last());
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtJsonLd1()
    {
        var defs = MimeTypesHelper.GetDefinitionsByFileExtension("jsonld");
        Assert.Single(defs);
        AssertIsJsonLd(defs.First());
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtJsonLd2()
    {
        var defs = MimeTypesHelper.GetDefinitionsByFileExtension(".jsonld");
        Assert.Single(defs);
        AssertIsJsonLd(defs.First());
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtJsonLd3()
    {
        var defs = MimeTypesHelper.GetDefinitionsByFileExtension("jsonld.gz");
        Assert.Single(defs);
        AssertIsGzippedJsonLd(defs.First());
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtJsonLd4()
    {
        var defs = MimeTypesHelper.GetDefinitionsByFileExtension(".jsonld.gz");
        Assert.Single(defs);
        AssertIsGzippedJsonLd(defs.First());
    }

    private void AssertIsJsonLd(MimeTypeDefinition d)
    {
        Assert.Null(d.RdfParserType);
        Assert.Null(d.RdfWriterType);
        Assert.Equal(typeof(JsonLdParser), d.RdfDatasetParserType);
        Assert.Equal(typeof(JsonLdWriter), d.RdfDatasetWriterType);
    }

    private void AssertIsGzippedJsonLd(MimeTypeDefinition d)
    {
        Assert.Equal(typeof(GZippedJsonLdParser), d.RdfDatasetParserType);
        Assert.Equal(typeof(GZippedJsonLdWriter), d.RdfDatasetWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeCaseSensitivity1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("TEXT/TURTLE");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(TurtleParser), d.RdfParserType);
        Assert.Equal(typeof(CompressingTurtleWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedTurtleParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedTurtleWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeCaseSensitivity2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("TEXT/turtle");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(TurtleParser), d.RdfParserType);
        Assert.Equal(typeof(CompressingTurtleWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedTurtleParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedTurtleWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeCaseSensitivity3()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("TeXt/TuRtLe");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(TurtleParser), d.RdfParserType);
        Assert.Equal(typeof(CompressingTurtleWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedTurtleParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedTurtleWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtCaseSensitivity1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".TTL");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(TurtleParser), d.RdfParserType);
        Assert.Equal(typeof(CompressingTurtleWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByExtCaseSensitivity2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".tTl");
        Assert.Single(defs);

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(TurtleParser), d.RdfParserType);
        Assert.Equal(typeof(CompressingTurtleWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeExtraParams1()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/turtle; charset=utf-8");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(TurtleParser), d.RdfParserType);
        Assert.Equal(typeof(CompressingTurtleWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedTurtleParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedTurtleWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetDefinitionsByTypeExtraParams2()
    {
        IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/turtle; q=1.0");
        Assert.Equal(2, defs.Count());

        //Check normal definition
        MimeTypeDefinition d = defs.First();
        Assert.Equal(typeof(TurtleParser), d.RdfParserType);
        Assert.Equal(typeof(CompressingTurtleWriter), d.RdfWriterType);

        //Check GZipped definition
        d = defs.Last();
        Assert.Equal(typeof(GZippedTurtleParser), d.RdfParserType);
        Assert.Equal(typeof(GZippedTurtleWriter), d.RdfWriterType);
    }

    [Fact]
    public void MimeTypesGetParserByTypeNTriples1()
    {
        IRdfReader parser = MimeTypesHelper.GetParser("text/plain");
        Assert.IsType<NTriplesParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByTypeNTriples2()
    {
        IRdfReader parser = MimeTypesHelper.GetParser("text/ntriples");
        Assert.IsType<NTriplesParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByTypeNTriples3()
    {
        IRdfReader parser = MimeTypesHelper.GetParser("text/ntriples+turtle");
        Assert.IsType<NTriplesParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByTypeNTriples4()
    {
        IRdfReader parser = MimeTypesHelper.GetParser("application/rdf-triples");
        Assert.IsType<NTriplesParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByTypeNTriples5()
    {
        IRdfReader parser = MimeTypesHelper.GetParser("application/x-ntriples");
        Assert.IsType<NTriplesParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByTypeTurtle1()
    {
        IRdfReader parser = MimeTypesHelper.GetParser("text/turtle");
        Assert.IsType<TurtleParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByTypeTurtle2()
    {
        IRdfReader parser = MimeTypesHelper.GetParser("application/x-turtle");
        Assert.IsType<TurtleParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByTypeTurtle3()
    {
        IRdfReader parser = MimeTypesHelper.GetParser("application/turtle");
        Assert.IsType<TurtleParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByTypeNotation3_1()
    {
        IRdfReader parser = MimeTypesHelper.GetParser("text/n3");
        Assert.IsType<Notation3Parser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByTypeNotation3_2()
    {
        IRdfReader parser = MimeTypesHelper.GetParser("text/rdf+n3");
        Assert.IsType<Notation3Parser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByTypeRdfXml1()
    {
        IRdfReader parser = MimeTypesHelper.GetParser("text/xml");
        Assert.IsType<RdfXmlParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByTypeRdfXml2()
    {
        IRdfReader parser = MimeTypesHelper.GetParser("application/rdf+xml");
        Assert.IsType<RdfXmlParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByTypeRdfXml3()
    {
        IRdfReader parser = MimeTypesHelper.GetParser("application/xml");
        Assert.IsType<RdfXmlParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByTypeRdfJson1()
    {
        IRdfReader parser = MimeTypesHelper.GetParser("text/json");
        Assert.IsType<RdfJsonParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByTypeRdfJson2()
    {
        IRdfReader parser = MimeTypesHelper.GetParser("application/json");
        Assert.IsType<RdfJsonParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByTypeRdfA1()
    {
        IRdfReader parser = MimeTypesHelper.GetParser("text/html");
        Assert.IsType<RdfAParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByTypeRdfA2()
    {
        IRdfReader parser = MimeTypesHelper.GetParser("application/xhtml+xml");
        Assert.IsType<RdfAParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByTypeUnknown()
    {
        Assert.Throws<RdfParserSelectionException>(() => MimeTypesHelper.GetParser("application/unknown"));
    }

    [Fact]
    public void MimeTypesGetParserByExtNTriples1()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".nt");
        Assert.IsType<NTriplesParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtNTriples2()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("nt");
        Assert.IsType<NTriplesParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtNTriples3()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".nt.gz");
        Assert.IsType<GZippedNTriplesParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtNTriples4()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("nt.gz");
        Assert.IsType<GZippedNTriplesParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtTurtle1()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".ttl");
        Assert.IsType<TurtleParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtTurtle2()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("ttl");
        Assert.IsType<TurtleParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtTurtle3()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".ttl.gz");
        Assert.IsType<GZippedTurtleParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtTurtle4()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("ttl.gz");
        Assert.IsType<GZippedTurtleParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtNotation3_1()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".n3");
        Assert.IsType<Notation3Parser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtNotation3_2()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("n3");
        Assert.IsType<Notation3Parser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtNotation3_3()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".n3.gz");
        Assert.IsType<GZippedNotation3Parser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtNotation3_4()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("n3.gz");
        Assert.IsType<GZippedNotation3Parser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfXml1()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".rdf");
        Assert.IsType<RdfXmlParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfXml2()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("rdf");
        Assert.IsType<RdfXmlParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfXml3()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".rdf.gz");
        Assert.IsType<GZippedRdfXmlParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfXml4()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("rdf.gz");
        Assert.IsType<GZippedRdfXmlParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfJson1()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".rj");
        Assert.IsType<RdfJsonParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfJson2()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("rj");
        Assert.IsType<RdfJsonParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfJson3()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".rj.gz");
        Assert.IsType<GZippedRdfJsonParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfJson4()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("rj.gz");
        Assert.IsType<GZippedRdfJsonParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfA1()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".html");
        Assert.IsType<RdfAParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfA2()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("html");
        Assert.IsType<RdfAParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfA3()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".html.gz");
        Assert.IsType<GZippedRdfAParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfA4()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("html.gz");
        Assert.IsType<GZippedRdfAParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfA5()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".htm");
        Assert.IsType<RdfAParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfA6()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("htm");
        Assert.IsType<RdfAParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfA7()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".htm.gz");
        Assert.IsType<GZippedRdfAParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfA8()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("htm.gz");
        Assert.IsType<GZippedRdfAParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfA9()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".xhtml");
        Assert.IsType<RdfAParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfA10()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("xhtml");
        Assert.IsType<RdfAParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfA11()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".xhtml.gz");
        Assert.IsType<GZippedRdfAParser>(parser);
    }

    [Fact]
    public void MimeTypesGetParserByExtRdfA12()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("xhtml.gz");
        Assert.IsType<GZippedRdfAParser>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeUnknown()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("application/unknown");
        Assert.IsType<CompressingTurtleWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeAny()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter(MimeTypesHelper.Any);
        Assert.IsType<NTriplesWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeNTriples1()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("text/plain");
        Assert.IsType<NTriplesWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeNTriples2()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("text/ntriples");
        Assert.IsType<NTriplesWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeNTriples3()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("text/ntriples+turtle");
        Assert.IsType<NTriplesWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeNTriples4()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("application/rdf-triples");
        Assert.IsType<NTriplesWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeNTriples5()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("application/x-ntriples");
        Assert.IsType<NTriplesWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeTurtle1()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("text/turtle");
        Assert.IsType<CompressingTurtleWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeTurtle2()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("application/x-turtle");
        Assert.IsType<CompressingTurtleWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeTurtle3()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("application/turtle");
        Assert.IsType<CompressingTurtleWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeNotation3_1()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("text/n3");
        Assert.IsType<Notation3Writer>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeNotation3_2()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("text/rdf+n3");
        Assert.IsType<Notation3Writer>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeRdfXml1()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("text/xml");
        Assert.IsType<RdfXmlWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeRdfXml2()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("application/xml");
        Assert.IsType<RdfXmlWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeRdfXml3()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("application/rdf+xml");
        Assert.IsType<RdfXmlWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeRdfJson1()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("text/json");
        Assert.IsType<RdfJsonWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeRdfJson2()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("application/json");
        Assert.IsType<RdfJsonWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeRdfJson3()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("application/rdf+json");
        Assert.IsType<RdfJsonWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeRdfA1()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("text/html");
        Assert.IsType<HtmlWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByTypeRdfA2()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("application/xhtml+xml");
        Assert.IsType<HtmlWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetWriterByExtNTriples1()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".nt");
        Assert.IsType<NTriplesWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtNTriples2()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("nt");
        Assert.IsType<NTriplesWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtNTriples3()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".nt.gz");
        Assert.IsType<GZippedNTriplesWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtNTriples4()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("nt.gz");
        Assert.IsType<GZippedNTriplesWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtTurtle1()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".ttl");
        Assert.IsType<CompressingTurtleWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtTurtle2()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("ttl");
        Assert.IsType<CompressingTurtleWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtTurtle3()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".ttl.gz");
        Assert.IsType<GZippedTurtleWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtTurtle4()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("ttl.gz");
        Assert.IsType<GZippedTurtleWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtNotation3_1()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".n3");
        Assert.IsType<Notation3Writer>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtNotation3_2()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("n3");
        Assert.IsType<Notation3Writer>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtNotation3_3()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".n3.gz");
        Assert.IsType<GZippedNotation3Writer>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtNotation3_4()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("n3.gz");
        Assert.IsType<GZippedNotation3Writer>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfXml1()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".rdf");
        Assert.IsType<RdfXmlWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfXml2()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("rdf");
        Assert.IsType<RdfXmlWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfXml3()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".rdf.gz");
        Assert.IsType<GZippedRdfXmlWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfXml4()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("rdf.gz");
        Assert.IsType<GZippedRdfXmlWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfJson1()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".rj");
        Assert.IsType<RdfJsonWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfJson2()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("rj");
        Assert.IsType<RdfJsonWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfJson3()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".rj.gz");
        Assert.IsType<GZippedRdfJsonWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfJson4()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("rj.gz");
        Assert.IsType<GZippedRdfJsonWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfA1()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".html");
        Assert.IsType<HtmlWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfA2()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("html");
        Assert.IsType<HtmlWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfA3()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".html.gz");
        Assert.IsType<GZippedRdfAWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfA4()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("html.gz");
        Assert.IsType<GZippedRdfAWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfA5()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".htm");
        Assert.IsType<HtmlWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfA6()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("htm");
        Assert.IsType<HtmlWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfA7()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".htm.gz");
        Assert.IsType<GZippedRdfAWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfA8()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("htm.gz");
        Assert.IsType<GZippedRdfAWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfA9()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".xhtml");
        Assert.IsType<HtmlWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfA10()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("xhtml");
        Assert.IsType<HtmlWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfA11()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".xhtml.gz");
        Assert.IsType<GZippedRdfAWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetWriterByExtRdfA12()
    {
        IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("xhtml.gz");
        Assert.IsType<GZippedRdfAWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByTypeUnknown()
    {
        Assert.Throws<RdfParserSelectionException>(() => MimeTypesHelper.GetSparqlParser("application/unknown"));
    }

    [Fact]
    public void MimeTypesGetSparqlParserByTypeSparqlXml1()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("application/sparql-results+xml");
        Assert.IsType<SparqlXmlParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByTypeSparqlXml2()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("application/xml");
        Assert.IsType<SparqlXmlParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByTypeSparqlJson1()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("application/sparql-results+json");
        Assert.IsType<SparqlJsonParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByTypeSparqlJson2()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("application/json");
        Assert.IsType<SparqlJsonParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByTypeSparqlCsv1()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("text/csv");
        Assert.IsType<SparqlCsvParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByTypeSparqlCsv2()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("text/comma-separated-values");
        Assert.IsType<SparqlCsvParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByTypeSparqlTsv1()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("text/tab-separated-values");
        Assert.IsType<SparqlTsvParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByExtSparqlXml1()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".srx");
        Assert.IsType<SparqlXmlParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByExtSparqlXml2()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("srx");
        Assert.IsType<SparqlXmlParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByExtSparqlXml3()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".srx.gz");
        Assert.IsType<GZippedSparqlXmlParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByExtSparqlXml4()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("srx.gz");
        Assert.IsType<GZippedSparqlXmlParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByExtSparqlJson1()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".srj");
        Assert.IsType<SparqlJsonParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByExtSparqlJson2()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("srj");
        Assert.IsType<SparqlJsonParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByExtSparqlJson3()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".srj.gz");
        Assert.IsType<GZippedSparqlJsonParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByExtSparqlJson4()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("srj.gz");
        Assert.IsType<GZippedSparqlJsonParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByExtSparqlTsv1()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".tsv");
        Assert.IsType<SparqlTsvParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByExtSparqlTsv2()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("tsv");
        Assert.IsType<SparqlTsvParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByExtSparqlTsv3()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".tsv.gz");
        Assert.IsType<GZippedSparqlTsvParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByExtSparqlTsv4()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("tsv.gz");
        Assert.IsType<GZippedSparqlTsvParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByExtSparqlCsv1()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".csv");
        Assert.IsType<SparqlCsvParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByExtSparqlCsv2()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("csv");
        Assert.IsType<SparqlCsvParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByExtSparqlCsv3()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".csv.gz");
        Assert.IsType<GZippedSparqlCsvParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlParserByExtSparqlCsv4()
    {
        ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("csv.gz");
        Assert.IsType<GZippedSparqlCsvParser>(parser);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByTypeUnknown()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("application/unknown");
        Assert.IsType<SparqlXmlWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByTypeAny()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter(MimeTypesHelper.Any);
        Assert.IsType<SparqlXmlWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByTypeSparqlXml1()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("application/sparql-results+xml");
        Assert.IsType<SparqlXmlWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByTypeSparqlXml2()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("application/xml");
        Assert.IsType<SparqlXmlWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByTypeSparqlJson1()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("application/sparql-results+json");
        Assert.IsType<SparqlJsonWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByTypeSparqlJson2()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("application/json");
        Assert.IsType<SparqlJsonWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByTypeSparqlCsv1()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("text/csv");
        Assert.IsType<SparqlCsvWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByTypeSparqlCsv2()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("text/comma-separated-values");
        Assert.IsType<SparqlCsvWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByTypeSparqlTsv1()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("text/tab-separated-values");
        Assert.IsType<SparqlTsvWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByExtSparqlXml1()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".srx");
        Assert.IsType<SparqlXmlWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByExtSparqlXml2()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("srx");
        Assert.IsType<SparqlXmlWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByExtSparqlXml3()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".srx.gz");
        Assert.IsType<GZippedSparqlXmlWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByExtSparqlXml4()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("srx.gz");
        Assert.IsType<GZippedSparqlXmlWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByExtSparqlJson1()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".srj");
        Assert.IsType<SparqlJsonWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByExtSparqlJson2()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("srj");
        Assert.IsType<SparqlJsonWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByExtSparqlJson3()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".srj.gz");
        Assert.IsType<GZippedSparqlJsonWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByExtSparqlJson4()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("srj.gz");
        Assert.IsType<GZippedSparqlJsonWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByExtSparqlTsv1()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".tsv");
        Assert.IsType<SparqlTsvWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByExtSparqlTsv2()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("tsv");
        Assert.IsType<SparqlTsvWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByExtSparqlTsv3()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".tsv.gz");
        Assert.IsType<GZippedSparqlTsvWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByExtSparqlTsv4()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("tsv.gz");
        Assert.IsType<GZippedSparqlTsvWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByExtSparqlCsv1()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".csv");
        Assert.IsType<SparqlCsvWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByExtSparqlCsv2()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("csv");
        Assert.IsType<SparqlCsvWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByExtSparqlCsv3()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".csv.gz");
        Assert.IsType<GZippedSparqlCsvWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetSparqlWriterByExtSparqlCsv4()
    {
        ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("csv.gz");
        Assert.IsType<GZippedSparqlCsvWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetStoreParserByTypeUnknown()
    {
        Assert.Throws<RdfParserSelectionException>(() => MimeTypesHelper.GetStoreParser("application/unknown"));
    }

    [Fact]
    public void MimeTypesGetStoreParserByTypeNQuads1()
    {
        IStoreReader parser = MimeTypesHelper.GetStoreParser("text/x-nquads");
        Assert.IsType<NQuadsParser>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreParserByTypeTriG1()
    {
        IStoreReader parser = MimeTypesHelper.GetStoreParser("application/x-trig");
        Assert.IsType<TriGParser>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreParserByTypeTriX1()
    {
        IStoreReader parser = MimeTypesHelper.GetStoreParser("application/trix");
        Assert.IsType<TriXParser>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreParserByExtNQuads1()
    {
        IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".nq");
        Assert.IsType<NQuadsParser>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreParserByExtNQuads2()
    {
        IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension("nq");
        Assert.IsType<NQuadsParser>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreParserByExtNQuads3()
    {
        IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".nq.gz");
        Assert.IsType<GZippedNQuadsParser>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreParserByExtNQuads4()
    {
        IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension("nq.gz");
        Assert.IsType<GZippedNQuadsParser>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreParserByExtTriG1()
    {
        IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".trig");
        Assert.IsType<TriGParser>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreParserByExtTriG2()
    {
        IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension("trig");
        Assert.IsType<TriGParser>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreParserByExtTriG3()
    {
        IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".trig.gz");
        Assert.IsType<GZippedTriGParser>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreParserByExtTriG4()
    {
        IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension("trig.gz");
        Assert.IsType<GZippedTriGParser>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreParserByExtTriX1()
    {
        IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".xml");
        Assert.IsType<TriXParser>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreParserByExtTriX2()
    {
        IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension("xml");
        Assert.IsType<TriXParser>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreParserByExtTriX3()
    {
        IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".xml.gz");
        Assert.IsType<GZippedTriXParser>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreParserByExtTriX4()
    {
        IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension("xml.gz");
        Assert.IsType<GZippedTriXParser>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreWriterByTypeUnknown()
    {
        IStoreWriter writer = MimeTypesHelper.GetStoreWriter("application/unknown");
        Assert.IsType<NQuadsWriter>(writer);
    }

    [Fact]
    public void MimeTypesGetStoreWriterByTypeNQuads1()
    {
        IStoreWriter parser = MimeTypesHelper.GetStoreWriter("text/x-nquads");
        Assert.IsType<NQuadsWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreWriterByTypeTriG1()
    {
        IStoreWriter parser = MimeTypesHelper.GetStoreWriter("application/x-trig");
        Assert.IsType<TriGWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreWriterByTypeTriX1()
    {
        IStoreWriter parser = MimeTypesHelper.GetStoreWriter("application/trix");
        Assert.IsType<TriXWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreWriterByExtNQuads1()
    {
        IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension(".nq");
        Assert.IsType<NQuadsWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreWriterByExtNQuads2()
    {
        IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension("nq");
        Assert.IsType<NQuadsWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreWriterByExtNQuads3()
    {
        IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension(".nq.gz");
        Assert.IsType<GZippedNQuadsWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreWriterByExtNQuads4()
    {
        IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension("nq.gz");
        Assert.IsType<GZippedNQuadsWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreWriterByExtTriG1()
    {
        IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension(".trig");
        Assert.IsType<TriGWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreWriterByExtTriG2()
    {
        IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension("trig");
        Assert.IsType<TriGWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreWriterByExtTriG3()
    {
        IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension(".trig.gz");
        Assert.IsType<GZippedTriGWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreWriterByExtTriG4()
    {
        IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension("trig.gz");
        Assert.IsType<GZippedTriGWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreWriterByExtTriX1()
    {
        IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension(".xml");
        Assert.IsType<TriXWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreWriterByExtTriX2()
    {
        IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension("xml");
        Assert.IsType<TriXWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreWriterByExtTriX3()
    {
        IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension(".xml.gz");
        Assert.IsType<GZippedTriXWriter>(parser);
    }

    [Fact]
    public void MimeTypesGetStoreWriterByExtTriX4()
    {
        IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension("xml.gz");
        Assert.IsType<GZippedTriXWriter>(parser);
    }

    [Fact]
    public void MimeTypesApplyWriterOptions1()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriter("application/turtle", WriterCompressionLevel.High);
        Assert.IsType<CompressingTurtleWriter>(writer);
        Assert.Equal(WriterCompressionLevel.High, ((ICompressingWriter)writer).CompressionLevel);
    }

    [Fact]
    public void MimeTypesApplyWriterOptions2()
    {
        IRdfWriter writer = MimeTypesHelper.GetWriterByFileExtension(".ttl", WriterCompressionLevel.High);
        Assert.IsType<CompressingTurtleWriter>(writer);
        Assert.Equal(WriterCompressionLevel.High, ((ICompressingWriter)writer).CompressionLevel);
    }

    [Fact]
    public void MimeTypesApplyWriterOptions3()
    {
        IStoreWriter writer = MimeTypesHelper.GetStoreWriter("application/x-trig", WriterCompressionLevel.High);
        Assert.IsType<TriGWriter>(writer);
        Assert.Equal(WriterCompressionLevel.High, ((ICompressingWriter)writer).CompressionLevel);
    }

    [Fact]
    public void MimeTypesApplyWriterOptions4()
    {
        IStoreWriter writer = MimeTypesHelper.GetStoreWriterByFileExtension(".trig", WriterCompressionLevel.High);
        Assert.IsType<TriGWriter>(writer);
        Assert.Equal(WriterCompressionLevel.High, ((ICompressingWriter)writer).CompressionLevel);
    }

    [Fact]
    public void MimeTypesApplyParserOptions1()
    {
        IRdfReader parser = MimeTypesHelper.GetParser("application/turtle", TokenQueueMode.AsynchronousBufferDuringParsing);
        Assert.IsType<TurtleParser>(parser);
        Assert.Equal(TokenQueueMode.AsynchronousBufferDuringParsing, ((ITokenisingParser)parser).TokenQueueMode);
    }

    [Fact]
    public void MimeTypesApplyParserOptions2()
    {
        IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".ttl", TokenQueueMode.AsynchronousBufferDuringParsing);
        Assert.IsType<TurtleParser>(parser);
        Assert.Equal(TokenQueueMode.AsynchronousBufferDuringParsing, ((ITokenisingParser)parser).TokenQueueMode);
    }

    [Fact]
    public void MimeTypesApplyParserOptions3()
    {
        IStoreReader parser = MimeTypesHelper.GetStoreParser("text/x-nquads", TokenQueueMode.AsynchronousBufferDuringParsing);
        Assert.IsType<NQuadsParser>(parser);
        Assert.Equal(TokenQueueMode.AsynchronousBufferDuringParsing, ((ITokenisingParser)parser).TokenQueueMode);
    }

    [Fact]
    public void MimeTypesApplyParserOptions4()
    {
        IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".nq", TokenQueueMode.AsynchronousBufferDuringParsing);
        Assert.IsType<NQuadsParser>(parser);
        Assert.Equal(TokenQueueMode.AsynchronousBufferDuringParsing, ((ITokenisingParser)parser).TokenQueueMode);
    }

    [Fact]
    public void MimeTypesContentNegotiation1()
    {
        var types = new String[] { "application/turtle" , "application/rdf+xml", "text/plain" };
        MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(types).FirstOrDefault();
        Assert.NotNull(def);
        Assert.Equal(typeof(TurtleParser), def.RdfParserType);
    }

    [Fact]
    public void MimeTypesContentNegotiation2()
    {
        var types = new String[] { "application/rdf+xml", "application/turtle", "text/plain" };
        MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(types).FirstOrDefault();
        Assert.NotNull(def);
        Assert.Equal(typeof(RdfXmlParser), def.RdfParserType);
    }

    [Fact]
    public void MimeTypesContentNegotiation3()
    {
        var types = new String[] { "text/plain", "application/rdf+xml", "application/turtle" };
        MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(types).FirstOrDefault();
        Assert.NotNull(def);
        Assert.Equal(typeof(NTriplesParser), def.RdfParserType);
    }

    [Fact]
    public void MimeTypesContentNegotiation4()
    {
        MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(MimeTypesHelper.Any).FirstOrDefault();
        Assert.NotNull(def);
    }

    [Fact]
    public void MimeTypesContentNegotiation5()
    {
        var types = new String[] { "application/turtle; q=0.8", "application/rdf+xml", "text/plain; q=0.9" };
        MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(types).FirstOrDefault();
        Assert.NotNull(def);
        Assert.Equal(typeof(RdfXmlParser), def.RdfParserType);
    }

    private void PrintSelectors(IEnumerable<MimeTypeSelector> selectors)
    {
        foreach (MimeTypeSelector selector in selectors)
        {
            Console.WriteLine(selector.ToString());
        }
    }

    [Fact]
    public void MimeTypesSelectors1()
    {
        var types = new String[] { "audio/*; q=0.2", "audio/basic" };
        var selectors = MimeTypeSelector.CreateSelectors(types).ToList();
        PrintSelectors(selectors);

        Assert.False(selectors[0].IsRange);
        Assert.Equal("audio/basic", selectors[0].Type);
        Assert.True(selectors[1].IsRange);
        Assert.Equal("audio/*", selectors[1].Type);
        Assert.Equal(0.2d, selectors[1].Quality);
    }

    [Fact]
    public void MimeTypesSelectors2()
    {
        var types = new String[] { "text/plain; q=0.5", "text/turtle" };
        var selectors = MimeTypeSelector.CreateSelectors(types).ToList();
        PrintSelectors(selectors);

        Assert.Equal("text/turtle", selectors[0].Type);
        Assert.Equal("text/plain", selectors[1].Type);
        Assert.Equal(0.5d, selectors[1].Quality);
    }

    [Fact]
    public void MimeTypesSelectors3()
    {
        var types = new String[] { "text/plain", "text/turtle" };
        var selectors = MimeTypeSelector.CreateSelectors(types).ToList();
        PrintSelectors(selectors);

        Assert.Equal("text/plain", selectors[0].Type);
        Assert.Equal("text/turtle", selectors[1].Type);
    }

    [Fact]
    public void MimeTypesSelectors4()
    {
        var types = new String[] { "text/*", "text/html", "*/*" };
        var selectors = MimeTypeSelector.CreateSelectors(types).ToList();
        PrintSelectors(selectors);

        Assert.Equal("text/html", selectors[0].Type);
        Assert.Equal("text/*", selectors[1].Type);
        Assert.Equal(MimeTypesHelper.Any, selectors[2].Type);
    }

    [Fact]
    public void MimeTypesSelectors5()
    {
        var types = new String[] { "text/plain; q=0.5", "text/turtle; q=0.5" };
        var selectors = MimeTypeSelector.CreateSelectors(types).ToList();
        PrintSelectors(selectors);

        Assert.Equal("text/plain", selectors[0].Type);
        Assert.Equal(0.5d, selectors[0].Quality);
        Assert.Equal("text/turtle", selectors[1].Type);
        Assert.Equal(0.5d, selectors[1].Quality);
    }

    [Fact]
    public void MimeTypesSelectors6()
    {
        var types = new String[] { "text/turtle; q=0.5", "text/plain; q=0.5" };
        var selectors = MimeTypeSelector.CreateSelectors(types).ToList();
        PrintSelectors(selectors);

        Assert.Equal("text/turtle", selectors[0].Type);
        Assert.Equal(0.5d, selectors[0].Quality);
        Assert.Equal("text/plain", selectors[1].Type);
        Assert.Equal(0.5d, selectors[1].Quality);
    }

    [Fact]
    public void MimeTypesGetDefinitionByUpperCaseExt()
    {
        foreach (MimeTypeDefinition def in MimeTypesHelper.Definitions)
        {
            if (!def.HasFileExtensions) continue;
            var ext = def.CanonicalFileExtension.ToUpper();
            MimeTypeDefinition def2 = MimeTypesHelper.GetDefinitionsByFileExtension(ext).FirstOrDefault();
            Assert.NotNull(def2);
            Assert.Equal(def.SyntaxName, def2.SyntaxName);
            Assert.Equal(def.CanonicalMimeType, def2.CanonicalMimeType);
            Assert.Equal(def.HasFileExtensions, def2.HasFileExtensions);
            Assert.Equal(def.CanonicalFileExtension, def2.CanonicalFileExtension);
            Assert.Equal(def.CanParseRdf, def2.CanParseRdf);
            Assert.Equal(def.CanParseRdfDatasets, def2.CanParseRdfDatasets);
            Assert.Equal(def.CanParseSparqlResults, def2.CanParseSparqlResults);
            Assert.Equal(def.CanWriteRdf, def2.CanWriteRdf);
            Assert.Equal(def.CanWriteRdfDatasets, def2.CanWriteRdfDatasets);
            Assert.Equal(def.CanWriteSparqlResults, def2.CanWriteSparqlResults);
            Assert.Equal(def.RdfParserType, def2.RdfParserType);
            Assert.Equal(def.RdfDatasetParserType, def2.RdfDatasetParserType);
            Assert.Equal(def.SparqlResultsParserType, def2.SparqlResultsParserType);
            Assert.Equal(def.RdfWriterType, def2.RdfWriterType);
            Assert.Equal(def.RdfDatasetWriterType, def2.RdfDatasetWriterType);
            Assert.Equal(def.SparqlResultsWriterType, def2.SparqlResultsWriterType);
        }
    }

    [Fact]
    public void MimeTypesDefaultHttpRdfOrSparqlAcceptHeader()
    {
        var request = new HttpRequestMessage();
        request.Headers.Add(HeaderNames.Accept, MimeTypesHelper.HttpRdfOrSparqlAcceptHeader);
        Assert.Equal(52, request.Headers.Accept.Count);
    }

    [Fact]
    public void MimeTypesDefaultHttpSparqlAcceptHeader()
    {
        var request = new HttpRequestMessage();
        request.Headers.Add(HeaderNames.Accept, MimeTypesHelper.HttpSparqlAcceptHeader);
        Assert.Equal(12, request.Headers.Accept.Count);
    }

    [Fact]
    public void MimeTypesDefaultHttpAcceptHeader()
    {
        var request = new HttpRequestMessage();
        request.Headers.Add(HeaderNames.Accept, MimeTypesHelper.HttpAcceptHeader);
        Assert.Equal(21, request.Headers.Accept.Count);
    }

    [Fact]
    public void MimeTypesDefaultHttpRdfDatasetAcceptHeader()
    {
        var request = new HttpRequestMessage();
        request.Headers.Add(HeaderNames.Accept, MimeTypesHelper.HttpRdfDatasetAcceptHeader);
        Assert.Equal(6, request.Headers.Accept.Count);
    }
}
