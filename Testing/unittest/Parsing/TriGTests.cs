using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Test.Parsing
{
    [TestClass]
    public class TriGTests
    {
        private ITripleStore TestParsing(String data, TriGSyntax syntax, bool shouldParse)
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

        [TestMethod]
        public void ParsingTriGBaseDeclaration1()
        {
            String fragment = "@base <http://example.org/base/> .";
            this.TestParsing(fragment, TriGSyntax.Original, false);
            this.TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [TestMethod]
        public void ParsingTriGBaseDeclaration2()
        {
            String fragment = "{ @base <http://example.org/base/> . }";
            this.TestParsing(fragment, TriGSyntax.Original, false);
            this.TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [TestMethod]
        public void ParsingTriGBaseDeclaration3()
        {
            String fragment = "<http://graph> { @base <http://example.org/base/> . }";
            this.TestParsing(fragment, TriGSyntax.Original, false);
            this.TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [TestMethod]
        public void ParsingTriGPrefixDeclaration1()
        {
            String fragment = "@prefix ex: <http://example.org/> .";
            this.TestParsing(fragment, TriGSyntax.Original, true);
            this.TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [TestMethod]
        public void ParsingTriGPrefixDeclaration2()
        {
            String fragment = "{ @prefix ex: <http://example.org/> . }";
            this.TestParsing(fragment, TriGSyntax.Original, false);
            this.TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [TestMethod]
        public void ParsingTriGPrefixDeclaration3()
        {
            String fragment = "<http://graph> { @prefix ex: <http://example.org/> . }";
            this.TestParsing(fragment, TriGSyntax.Original, false);
            this.TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [TestMethod]
        public void ParsingTrigBaseScope1()
        {
            String fragment = "@base <http://example.org/base/> . { <subj> <pred> <obj> . }";
            this.TestParsing(fragment, TriGSyntax.Original, false);
            this.TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [TestMethod]
        public void ParsingTrigBaseScope2()
        {
            String fragment = "{ @base <http://example.org/base/> . <subj> <pred> <obj> . }";
            this.TestParsing(fragment, TriGSyntax.Original, false);
            this.TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [TestMethod]
        public void ParsingTrigBaseScope3()
        {
            String fragment = "{ @base <http://example.org/base/> . } <http://graph> { <subj> <pred> <obj> . }";
            this.TestParsing(fragment, TriGSyntax.Original, false);
            this.TestParsing(fragment, TriGSyntax.MemberSubmission, false);
        }

        [TestMethod]
        public void ParsingTrigBaseScope4()
        {
            String fragment = "{ @base <http://example.org/base/> . <subj> <pred> <obj> . } <http://graph> { <subj> <pred> <obj> . }";
            this.TestParsing(fragment, TriGSyntax.Original, false);
            this.TestParsing(fragment, TriGSyntax.MemberSubmission, false);
        }

        [TestMethod]
        public void ParsingTrigPrefixScope1()
        {
            String fragment = "@prefix ex: <http://example.org/> . { ex:subj ex:pred ex:obj . }";
            this.TestParsing(fragment, TriGSyntax.Original, true);
            this.TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [TestMethod]
        public void ParsingTrigPrefixScope2()
        {
            String fragment = "{ @prefix ex: <http://example.org/> . ex:subj ex:pred ex:obj . }";
            this.TestParsing(fragment, TriGSyntax.Original, false);
            this.TestParsing(fragment, TriGSyntax.MemberSubmission, true);
        }

        [TestMethod]
        public void ParsingTrigPrefixScope3()
        {
            String fragment = "{ @prefix ex: <http://example.org/> . } <http://graph> { ex:subj ex:pred ex:obj . }";
            this.TestParsing(fragment, TriGSyntax.Original, false);
            this.TestParsing(fragment, TriGSyntax.MemberSubmission, false);
        }

        [TestMethod]
        public void ParsingTrigPrefixScope4()
        {
            String fragment = "{ @prefix ex: <http://example.org/> . ex:subj ex:pred ex:obj . } <http://graph> { ex:subj ex:pred ex:obj . }";
            this.TestParsing(fragment, TriGSyntax.Original, false);
            this.TestParsing(fragment, TriGSyntax.MemberSubmission, false);
        }
    }
}
