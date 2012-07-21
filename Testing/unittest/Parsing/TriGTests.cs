/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

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
