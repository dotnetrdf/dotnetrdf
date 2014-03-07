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
using System.Linq;
using NUnit.Framework;

namespace VDS.RDF.Parsing
{
    [TestFixture]
    public class TriGTests
    {
        private static ITripleStore TestParsing(String data, TriGSyntax syntax, bool shouldParse)
        {
            TriGParser parser = new TriGParser(syntax);
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

        [Test]
        public void ParsingTriGBaseDeclaration1()
        {
            String fragment = "@base <http://example.org/base/> .";
            TestParsing(fragment, TriGSyntax.Original, false);
            TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [Test]
        public void ParsingTriGBaseDeclaration2()
        {
            String fragment = "{ @base <http://example.org/base/> . }";
            TestParsing(fragment, TriGSyntax.Original, false);
            TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [Test]
        public void ParsingTriGBaseDeclaration3()
        {
            String fragment = "<http://graph> { @base <http://example.org/base/> . }";
            TestParsing(fragment, TriGSyntax.Original, false);
            TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [Test]
        public void ParsingTriGPrefixDeclaration1()
        {
            String fragment = "@prefix ex: <http://example.org/> .";
            TestParsing(fragment, TriGSyntax.Original, true);
            TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [Test]
        public void ParsingTriGPrefixDeclaration2()
        {
            String fragment = "{ @prefix ex: <http://example.org/> . }";
            TestParsing(fragment, TriGSyntax.Original, false);
            TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [Test]
        public void ParsingTriGPrefixDeclaration3()
        {
            String fragment = "<http://graph> { @prefix ex: <http://example.org/> . }";
            TestParsing(fragment, TriGSyntax.Original, false);
            TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [Test]
        public void ParsingTrigBaseScope1()
        {
            String fragment = "@base <http://example.org/base/> . { <subj> <pred> <obj> . }";
            TestParsing(fragment, TriGSyntax.Original, false);
            TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [Test]
        public void ParsingTrigBaseScope2()
        {
            String fragment = "{ @base <http://example.org/base/> . <subj> <pred> <obj> . }";
            TestParsing(fragment, TriGSyntax.Original, false);
            TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [Test]
        public void ParsingTrigBaseScope3()
        {
            String fragment = "{ @base <http://example.org/base/> . } <http://graph> { <subj> <pred> <obj> . }";
            TestParsing(fragment, TriGSyntax.Original, false);
            TestParsing(fragment, TriGSyntax.MemberSubmission, false);
        }

        [Test]
        public void ParsingTrigBaseScope4()
        {
            String fragment = "{ @base <http://example.org/base/> . <subj> <pred> <obj> . } <http://graph> { <subj> <pred> <obj> . }";
            TestParsing(fragment, TriGSyntax.Original, false);
            TestParsing(fragment, TriGSyntax.MemberSubmission, false);
        }

        [Test]
        public void ParsingTrigPrefixScope1()
        {
            String fragment = "@prefix ex: <http://example.org/> . { ex:subj ex:pred ex:obj . }";
            TestParsing(fragment, TriGSyntax.Original, true);
            TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [Test]
        public void ParsingTrigPrefixScope2()
        {
            String fragment = "{ @prefix ex: <http://example.org/> . ex:subj ex:pred ex:obj . }";
            TestParsing(fragment, TriGSyntax.Original, false);
            TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [Test]
        public void ParsingTrigPrefixScope3()
        {
            String fragment = "{ @prefix ex: <http://example.org/> . } <http://graph> { ex:subj ex:pred ex:obj . }";
            TestParsing(fragment, TriGSyntax.Original, false);
            TestParsing(fragment, TriGSyntax.MemberSubmission, false);
        }

        [Test]
        public void ParsingTrigPrefixScope4()
        {
            String fragment = "{ @prefix ex: <http://example.org/> . ex:subj ex:pred ex:obj . } <http://graph> { ex:subj ex:pred ex:obj . }";
            TestParsing(fragment, TriGSyntax.Original, false);
            TestParsing(fragment, TriGSyntax.MemberSubmission, false);
        }

        [Test]
        public void ParsingTriGCollectionSyntax1()
        {
            const String data = @"@prefix : <http://example/> .
:graph
{
    :resource :predicate1 ( ""a"" ""b"" ""c"" ).
    :resource :predicate2 ( :a :b :c ).
}";
            TriGParser parser = new TriGParser();
            TripleStore store = new TripleStore();
            parser.Load(store, new StringReader(data));

            Assert.AreEqual(1, store.Graphs.Count);
            Assert.AreEqual(14, store.Triples.Count());
        }

        [Test]
        public void ParsingTriGCollectionSyntax2()
        {
            const String data = @"@prefix : <http://example/> .
:graph
{
    :resource :predicate1 ( ""a"" ""b""@en ""c""^^:datatype ).
    :resource :predicate2 ( :a :b :c ).
}";
            TriGParser parser = new TriGParser();
            TripleStore store = new TripleStore();
            parser.Load(store, new StringReader(data));

            Assert.AreEqual(1, store.Graphs.Count);
            Assert.AreEqual(14, store.Triples.Count());
        }

        [Test]
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
            TriGParser parser = new TriGParser();
            TripleStore store = new TripleStore();
            parser.Load(store, new StringReader(data));

            Assert.AreEqual(1, store.Graphs.Count);
            Assert.AreEqual(17, store.Triples.Count());
        }

        [Test]
        public void ParsingTriGCollectionSyntax4()
        {
            const String data = @"@prefix : <http://example/> .
:graph
{
    :resource :predicate1 ( true 1 12.3 123e4 ).
}";
            TriGParser parser = new TriGParser();
            TripleStore store = new TripleStore();
            parser.Load(store, new StringReader(data));

            Assert.AreEqual(1, store.Graphs.Count);
            Assert.AreEqual(9, store.Triples.Count());
        }
    }
}
