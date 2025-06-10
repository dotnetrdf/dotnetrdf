using System.Collections.Generic;
using VDS.RDF.Parsing;
using Xunit;

namespace VDS.RDF.TestSuite.RdfStar;

public class SubGraphMatcherTests
{
    private const string Prefixes = "@prefix : <http://example.org/> .\n";
    [Theory]
    [InlineData("blank node in quoted subject", "<< _:x :p :o >> :p :o .", "<< _:a :p :o >> :p :o .", true, "a", "x")]
    [InlineData("blank node in quoted object", "<< :s :p _:x >> :p :o .", "<< :s :p  _:a >> :p :o .", true, "a", "x")]
    [InlineData("Blank node subject quoted and asserted", "<< _:x :p :o >> :p :o . _:x :p :o .", "<< _:a :p :o >> :p :o . _:a :p :o .", true, "a", "x")]
    [InlineData("Blank node object quoted and asserted", "<< :s :p _:x >> :p :o . _:x :p :o .", "<< :s :p _:a >> :p :o . _:a :p :o .", true, "a", "x")]
    [InlineData("Blank nodes must match across quoted and asserted triples",
        "<< _:x :p :o >> :p :o . _:x :p :o .",
        "<< _:a :p :o >> :p :o . _:b :p :o .",
        false)]
    public void ItMatchesQuotedBlankNodes(string _, string parentData, string subgraphData, bool expectSubgraph, params string[] expectMappings)
    {
        IGraph parent = new Graph();
        IGraph subGraph = new Graph();
        var parser = new TurtleParser(TurtleSyntax.Rdf11Star, false);
        parent.LoadFromString(Prefixes +parentData, parser);
        subGraph.LoadFromString(Prefixes + subgraphData, parser);
        Assert.Equal(expectSubgraph, subGraph.IsSubGraphOf(parent, out Dictionary<INode, INode> mapping));
        if (expectSubgraph)
        {
            Assert.Equal(expectMappings.Length / 2, mapping.Count);
            for (var i = 0; i < expectMappings.Length; i += 2)
            {
                INode expectKey = new BlankNode(expectMappings[i]);
                INode expectValue = new BlankNode(expectMappings[i + 1]);
                Assert.True(mapping.ContainsKey(expectKey));
                Assert.Equal(expectValue, mapping[expectKey]);
            }
        }
    }
}
