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
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;
using Xunit;

namespace VDS.RDF.LDF.Hydra;

public class ExplicitRepresentationFormatterTests
{
    private static readonly INodeFormatter formatter = new ExplicitRepresentationFormatter();
    private static readonly NodeFactory factory = new();

    [Fact(DisplayName = "Requires node")]
    public void RequiresNode()
    {
        var format = () => formatter.Format(default);

        format.Should().ThrowExactly<ArgumentNullException>("because the node was null");
    }

    [Fact(DisplayName = "URI nodes are formatted as bare absolute URIs")]
    public void IRI()
    {
        var lexical = $"urn:uuid:{Guid.NewGuid()}";
        var node = factory.CreateUriNode(UriFactory.Create(lexical));

        formatter.Format(node).Should().Be(lexical, "because Hydra represents IRIs as-is");
    }

    [Fact(DisplayName = "Literal nodes are formatted with surrounding double quotes")]
    public void Literal()
    {
        var lexical = Guid.NewGuid().ToString();
        var node = factory.CreateLiteralNode(lexical);

        formatter.Format(node).Should().Be($@"""{lexical}""", "because Hydra represents literals by their lexical form, surrounded by a single pair of doubles quotes");
    }

    [Fact(DisplayName = "Typed literals are formatted with surrounding double quotes followed by ^^ and datatype IRI")]
    public void TypedLiteral()
    {
        var lexical = Guid.NewGuid().ToString();
        var datatype = new Uri("urn:example:datatype");
        var node = factory.CreateLiteralNode(lexical, datatype);

        formatter.Format(node).Should().Be($@"""{lexical}""^^{datatype}", "because Hydra appends two caret symbols and the datatype IRI to typed literals");
    }

    [Fact(DisplayName = "Language-tagged literals are formatted with surrounding double quotes followed by @ and tag")]
    public void LangString()
    {
        var lexical = Guid.NewGuid().ToString();
        var lang = "ab-cd";
        var node = factory.CreateLiteralNode(lexical, lang);

        formatter.Format(node).Should().Be($@"""{lexical}""@{lang}", "because Hydra appends an at symbol and by the language tag");
    }

    [Theory(DisplayName = "Other node types are not supported")]
    [MemberData(nameof(IllegalNodes))]
    public void Illegal(INode node)
    {
        var format = () => formatter.Format(node);

        format.Should().ThrowExactly<LdfException>("because only URI and literal nodes are supported");
    }

    [Theory(DisplayName = "Nodes are formattted regardless of position")]
    [MemberData(nameof(NodesAndSegments))]
    public void x(INode node, TripleSegment segment1, TripleSegment segment2) =>
        formatter.Format(node, segment1).Should().Be(formatter.Format(node, segment2), "because formatting is agnostic of position");

    public static IEnumerable<TheoryDataRow<INode>> IllegalNodes
    {
        get
        {
            yield return new(factory.CreateBlankNode());
            yield return new(factory.CreateVariableNode("var"));
            yield return new(factory.CreateTripleNode(default));
            yield return new(factory.CreateGraphLiteralNode());
        }
    }

    public static IEnumerable<TheoryDataRow<INode, TripleSegment, TripleSegment>> NodesAndSegments
    {
        get
        {
            var uri = factory.CreateUriNode(new Uri($"urn:uuid:{Guid.NewGuid()}"));
            var literal = factory.CreateLiteralNode(Guid.NewGuid().ToString());
            var lang = factory.CreateLiteralNode(Guid.NewGuid().ToString(), "ab-cd");
            var typed = factory.CreateLiteralNode(Guid.NewGuid().ToString(), new Uri("urn:example:datatype"));

            var nodes = new List<INode> { uri, literal, lang, typed };
            var segments = new List<TripleSegment> { TripleSegment.Subject, TripleSegment.Predicate, TripleSegment.Object };

            return
                from node in nodes
                from segment1 in segments
                from segment2 in segments
                select new TheoryDataRow<INode, TripleSegment, TripleSegment>(node, segment1, segment2);
        }
    }
}
