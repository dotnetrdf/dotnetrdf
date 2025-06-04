using FluentAssertions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace VDS.RDF.Parsing.Tokens;

public class NTriplesTokeniserTests
{
    [Theory]
    [InlineData(
        "<< <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/p> <http://example.org/o> .",
        new[]
        {
            Token.BOF, Token.STARTQUOTE, Token.URI, Token.URI, Token.URI, Token.ENDQUOTE, Token.URI, Token.URI,
            Token.DOT, Token.EOF
        })]
    [InlineData(
        "<<<http://example.org/s><http://example.org/p><http://example.org/o>>><http://example.org/p><http://example.org/o>.",
        new[]
        {
            Token.BOF, Token.STARTQUOTE, Token.URI, Token.URI, Token.URI, Token.ENDQUOTE, Token.URI, Token.URI,
            Token.DOT, Token.EOF
        })]
    [InlineData(
        "<< << <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/p> <http://example.org/o> >> <http://example.org/p> <http://example.org/o> .",
        new[]
        {
            Token.BOF, Token.STARTQUOTE, Token.STARTQUOTE, Token.URI, Token.URI, Token.URI, Token.ENDQUOTE,
            Token.URI, Token.URI, Token.ENDQUOTE, 
            Token.URI, Token.URI, Token.DOT, Token.EOF
        })]
    public void RdfStarTokenisationTests(string inputString, int[] expectTokens)
    {
        var input = new StringReader(inputString);
        var tokeniser = new NTriplesTokeniser(input) { Syntax = NTriplesSyntax.Rdf11Star };
        var allTokens = ReadAll(tokeniser).ToList();
        allTokens.Count.Should().Be(expectTokens.Length);
        allTokens.Select(t => t.TokenType).Should().ContainInOrder(expectTokens);
    }

    private IEnumerable<IToken> ReadAll(ITokeniser tokeniser)
    {
        IToken next;
        do
        {
            next = tokeniser.GetNextToken();
            yield return next;
        } while (next.TokenType != Token.EOF);
    }
}
