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

using FluentAssertions;
using Resta.UriTemplates;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.LDF.Hydra;
using VDS.RDF.Writing.Formatting;
using Xunit;

namespace VDS.RDF.LDF.Client;

public class TpfParametersTests
{
    private const string templateString = "urn:example:qpfBase{?subject,predicate,object}";
    private const INode nil = null;
    private static readonly NodeFactory factory = new();
    private static readonly IriTemplate template = CreateTemplate();
    private static readonly IUriNode iri = factory.CreateUriNode(UriFactory.Create("urn:example:iri"));
    private static readonly IBlankNode blank = factory.CreateBlankNode("blank");
    private static readonly ILiteralNode lit = factory.CreateLiteralNode("literal");
    private static readonly IVariableNode variable = factory.CreateVariableNode("var");
    private static readonly IGraphLiteralNode graph = factory.CreateGraphLiteralNode();
    private static readonly ITripleNode triple = factory.CreateTripleNode(new Triple(iri, iri, iri));
    private static readonly INodeFormatter formatter = new ExplicitRepresentationFormatter();

    [Fact(DisplayName = "Requires template")]
    public void RequiresUri()
    {
        var constructor = () => new TpfParameters(null);

        constructor.Should().ThrowExactly<ArgumentNullException>("because the template was null");
    }

    [Theory(DisplayName = "Rejects illegal parameters")]
    [MemberData(nameof(IllegalPatterns))]
    public void RejectsIllegal(INode s, INode p, INode o, string param, string type)
    {
        var constructor = () => new TpfParameters(template, s, p, o);

        constructor.Should().ThrowExactly<ArgumentOutOfRangeException>("because the {0} was a {1}", param, type);
    }

    [Theory(DisplayName = "Resolves legal parameters")]
    [MemberData(nameof(LegalPatterns))]
    public void ResolvesLegal(INode s, INode p, INode o)
    {
        var uri = (Uri)new TpfParameters(template, s, p, o);

        uri.Should().Be(ResolveTemplate(s, p, o));
    }

    public static IEnumerable<TheoryDataRow<INode, INode, INode>> LegalPatterns =>
        from s in new List<INode> { nil, iri }
        from p in new List<INode> { nil, iri }
        from o in new List<INode> { nil, iri, lit }
        select new TheoryDataRow<INode, INode, INode>(s, p, o);

    public static IEnumerable<TheoryDataRow<INode, INode, INode, string, string>> IllegalPatterns => [
        new(lit, nil, nil, "subject", nameof(lit)),
        new(blank, nil, nil, "subject", nameof(blank)),
        new(variable, nil, nil, "subject", nameof(variable)),
        new(graph, nil, nil, "subject", nameof(graph)),
        new(triple, nil, nil, "subject", nameof(triple)),
        new(nil, lit, nil, "predicate", nameof(lit)),
        new(nil, blank, nil, "predicate", nameof(blank)),
        new(nil, variable, nil, "predicate", nameof(variable)),
        new(nil, graph, nil, "predicate", nameof(graph)),
        new(nil, triple, nil, "predicate", nameof(triple)),
        new(nil, nil, blank, "object", nameof(blank)),
        new(nil, nil, variable, "object", nameof(variable)),
        new(nil, nil, graph, "object", nameof(graph)),
        new(nil, nil, triple, "object", nameof(triple)),
    ];

    private static Uri ResolveTemplate(INode s, INode p, INode o) =>
        new UriTemplate(templateString).ResolveUri(new Dictionary<string, object>() {
            { "subject",   Format(s) },
            { "predicate", Format(p) },
            { "object",    Format(o) },
        });

    private static string Format(INode s) => s is null ? null : formatter.Format(s);

    private static IriTemplate CreateTemplate()
    {
        var g = new Graph();
        var search = g.CreateBlankNode();
        var subjectMapping = g.CreateBlankNode();
        var predicateMapping = g.CreateBlankNode();
        var objectMapping = g.CreateBlankNode();

        g.Assert(search, Vocabulary.Hydra.Template, g.CreateLiteralNode(templateString));
        g.Assert(search, Vocabulary.Hydra.Mapping, subjectMapping);
        g.Assert(search, Vocabulary.Hydra.Mapping, predicateMapping);
        g.Assert(search, Vocabulary.Hydra.Mapping, objectMapping);
        g.Assert(subjectMapping, Vocabulary.Hydra.Variable, g.CreateLiteralNode("subject"));
        g.Assert(subjectMapping, Vocabulary.Hydra.Property, Vocabulary.Rdf.Subject);
        g.Assert(predicateMapping, Vocabulary.Hydra.Variable, g.CreateLiteralNode("predicate"));
        g.Assert(predicateMapping, Vocabulary.Hydra.Property, Vocabulary.Rdf.Predicate);
        g.Assert(objectMapping, Vocabulary.Hydra.Variable, g.CreateLiteralNode("object"));
        g.Assert(objectMapping, Vocabulary.Hydra.Property, Vocabulary.Rdf.Object);

        return new IriTemplate(search, g);
    }
}