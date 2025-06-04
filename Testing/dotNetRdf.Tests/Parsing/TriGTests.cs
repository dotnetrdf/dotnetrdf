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
using System.IO;
using System.Linq;
using Xunit;

namespace VDS.RDF.Parsing;


public class TriGTests
{
    private static ITripleStore TestParsing(String data, TriGSyntax syntax, bool shouldParse)
    {
        var parser = new TriGParser(syntax);
        ITripleStore store = new TripleStore();

        try
        {
            parser.Load(store, new StringReader(data));

            if (!shouldParse) Assert.Fail("Parsed using syntax " + syntax.ToString() + " when an error was expected");
        }
        catch (Exception ex)
        {
            if (shouldParse) throw new RdfParseException("Error parsing using syntax " + syntax.ToString() + " when success was expected", ex);
        }

        return store;
    }

    [Fact]
    public void ParsingTriGBaseDeclaration1()
    {
        var fragment = "@base <http://example.org/base/> .";
        TestParsing(fragment, TriGSyntax.Original, false);
        TestParsing(fragment, TriGSyntax.MemberSubmission, true);
    }

    [Fact]
    public void ParsingTriGBaseDeclaration2()
    {
        var fragment = "{ @base <http://example.org/base/> . }";
        TestParsing(fragment, TriGSyntax.Original, false);
        TestParsing(fragment, TriGSyntax.MemberSubmission, true);
    }

    [Fact]
    public void ParsingTriGBaseDeclaration3()
    {
        var fragment = "<http://graph> { @base <http://example.org/base/> . }";
        TestParsing(fragment, TriGSyntax.Original, false);
        TestParsing(fragment, TriGSyntax.MemberSubmission, true);
    }

    [Fact]
    public void ParsingTriGPrefixDeclaration1()
    {
        var fragment = "@prefix ex: <http://example.org/> .";
        TestParsing(fragment, TriGSyntax.Original, true);
        TestParsing(fragment, TriGSyntax.MemberSubmission, true);
    }

    [Fact]
    public void ParsingTriGPrefixDeclaration2()
    {
        var fragment = "{ @prefix ex: <http://example.org/> . }";
        TestParsing(fragment, TriGSyntax.Original, false);
        TestParsing(fragment, TriGSyntax.MemberSubmission, true);
    }

    [Fact]
    public void ParsingTriGPrefixDeclaration3()
    {
        var fragment = "<http://graph> { @prefix ex: <http://example.org/> . }";
        TestParsing(fragment, TriGSyntax.Original, false);
        TestParsing(fragment, TriGSyntax.MemberSubmission, true);
    }

    [Fact]
    public void ParsingTrigBaseScope1()
    {
        var fragment = "@base <http://example.org/base/> . { <subj> <pred> <obj> . }";
        TestParsing(fragment, TriGSyntax.Original, false);
        TestParsing(fragment, TriGSyntax.MemberSubmission, true);
    }

    [Fact]
    public void ParsingTrigBaseScope2()
    {
        var fragment = "{ @base <http://example.org/base/> . <subj> <pred> <obj> . }";
        TestParsing(fragment, TriGSyntax.Original, false);
        TestParsing(fragment, TriGSyntax.MemberSubmission, true);
    }

    [Fact]
    public void ParsingTrigBaseScope3()
    {
        var fragment = "{ @base <http://example.org/base/> . } <http://graph> { <subj> <pred> <obj> . }";
        TestParsing(fragment, TriGSyntax.Original, false);
        TestParsing(fragment, TriGSyntax.MemberSubmission, false);
    }

    [Fact]
    public void ParsingTrigBaseScope4()
    {
        var fragment = "{ @base <http://example.org/base/> . <subj> <pred> <obj> . } <http://graph> { <subj> <pred> <obj> . }";
        TestParsing(fragment, TriGSyntax.Original, false);
        TestParsing(fragment, TriGSyntax.MemberSubmission, false);
    }

    [Fact]
    public void ParsingTrigPrefixScope1()
    {
        var fragment = "@prefix ex: <http://example.org/> . { ex:subj ex:pred ex:obj . }";
        TestParsing(fragment, TriGSyntax.Original, true);
        TestParsing(fragment, TriGSyntax.MemberSubmission, true);
    }

    [Fact]
    public void ParsingTrigPrefixScope2()
    {
        var fragment = "{ @prefix ex: <http://example.org/> . ex:subj ex:pred ex:obj . }";
        TestParsing(fragment, TriGSyntax.Original, false);
        TestParsing(fragment, TriGSyntax.MemberSubmission, true);
    }

    [Fact]
    public void ParsingTrigPrefixScope3()
    {
        var fragment = "{ @prefix ex: <http://example.org/> . } <http://graph> { ex:subj ex:pred ex:obj . }";
        TestParsing(fragment, TriGSyntax.Original, false);
        TestParsing(fragment, TriGSyntax.MemberSubmission, false);
    }

    [Fact]
    public void ParsingTrigPrefixScope4()
    {
        var fragment = "{ @prefix ex: <http://example.org/> . ex:subj ex:pred ex:obj . } <http://graph> { ex:subj ex:pred ex:obj . }";
        TestParsing(fragment, TriGSyntax.Original, false);
        TestParsing(fragment, TriGSyntax.MemberSubmission, false);
    }

    [Fact]
    public void ParsingTriGCollectionSyntax1()
    {
        const String data = @"@prefix : <http://example/> .
:graph
{
    :resource :predicate1 ( ""a"" ""b"" ""c"" ).
    :resource :predicate2 ( :a :b :c ).
}";
        var parser = new TriGParser();
        var store = new TripleStore();
        parser.Load(store, new StringReader(data));

        Assert.Equal(1, store.Graphs.Count);
        Assert.Equal(14, store.Triples.Count());
    }

    [Fact]
    public void ParsingTriGCollectionSyntax2()
    {
        const String data = @"@prefix : <http://example/> .
:graph
{
    :resource :predicate1 ( ""a"" ""b""@en ""c""^^:datatype ).
    :resource :predicate2 ( :a :b :c ).
}";
        var parser = new TriGParser();
        var store = new TripleStore();
        parser.Load(store, new StringReader(data));

        Assert.Equal(1, store.Graphs.Count);
        Assert.Equal(14, store.Triples.Count());
    }

    [Fact]
    public void ParsingTriGCollectionSyntax3()
    {
        const String data = @"@prefix : <http://example/> .
:graph
{
    :resource :predicate1 ( 
                            ( ""a"" ) # 2 triples
                            [] 
                            _:blank 
                            (
                                ""b""@en-us
                                ( ""c""^^:datatype ) # 2 triples
                            ) # 4 triples
                          ) . # 8 triples
}";
        var parser = new TriGParser();
        var store = new TripleStore();
        parser.Load(store, new StringReader(data));

        Assert.Equal(1, store.Graphs.Count);
        Assert.Equal(17, store.Triples.Count());
    }

    [Fact]
    public void ParsingTriGCollectionSyntax4()
    {
        const String data = @"@prefix : <http://example/> .
:graph
{
    :resource :predicate1 ( true 1 12.3 123e4 ).
}";
        var parser = new TriGParser();
        var store = new TripleStore();
        parser.Load(store, new StringReader(data));

        Assert.Equal(1, store.Graphs.Count);
        Assert.Equal(9, store.Triples.Count());
    }

    [Fact]
    public void ParsingTrigRecommendationSyntax1()
    {
        const string data = "@prefix : <http://example.com/> . { :1 :p :o . }";

        var store = new TripleStore();
        store.LoadFromString(data, new TriGParser(TriGSyntax.Rdf11));

        Assert.Single(store.Graphs);
        Assert.Single(store.Triples);
    }

    [Fact]
    public void ParsingPrefixWithDot()
    {
        const string data = "@prefix exdata: <https://example.com/data/>. exdata:test1 { exdata:test.1 a <http://example.com/Thing> .}";
        var store = new TripleStore();
        store.LoadFromString(data, new TriGParser(TriGSyntax.Rdf11));
        Assert.Single(store.Graphs);
        Assert.Single(store.Triples);
    }

    [Fact]
    public void ParsingPrefixedNameWithColon()
    {
        // Reproduce error reported in #634
        const string input = "@prefix ex: <http://example.org/>. ex:foo { ex:foo:bar:baz ex:value \"a\" . }";
        var store = new TripleStore();
        store.LoadFromString(input, new TriGParser(TriGSyntax.Rdf11));
        Assert.Single(store.Graphs);
        Assert.Single(store.Triples);
        store.Triples.First().Subject.Should().BeAssignableTo<IUriNode>().Which.Uri.AbsoluteUri.Should()
            .Be("http://example.org/foo:bar:baz");
    }

    [Fact]
    public void ParsingPrefixCalledPrefix()
    {
        const string data = @"@prefix prefix1: <http://example.com/>.
<http://example.org/cyrillic> {
    prefix1:test1 prefix1:pred1 ""литерал"".
    prefix1:test2 prefix1:pred1 ""EnglishTest"".
}";
        TestParsing(data, TriGSyntax.Rdf11Star, true);
    }
}
