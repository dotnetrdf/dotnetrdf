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
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Query
{
    [TestClass]
    public class ScalarArgumentsTests
    {
        private SparqlQueryParser _parser11 = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1);
        private SparqlQueryParser _parserExt = new SparqlQueryParser(SparqlQuerySyntax.Extended);

        [TestMethod]
        public void SparqlScalarArgsGroupBySeparator()
        {
            String query = "SELECT (GROUP_CONCAT(?s, ?p, ?o ; SEPARATOR = \" - \") AS ?concat) WHERE {?s ?p ?o}";
            Console.WriteLine(query);
            Console.WriteLine();
            this.CheckQueryFailsToParseIn11(query);
            this.CheckQueryFailsToParseInExtended(query);
        }

        [TestMethod]
        public void SparqlScalarArgsCountSeparator()
        {
            String query = "SELECT (COUNT(?s ; SEPARATOR = \" - \") AS ?count) WHERE {?s ?p ?o}";
            Console.WriteLine(query);
            Console.WriteLine();
            this.CheckQueryFailsToParseIn11(query);
            this.CheckQueryFailsToParseInExtended(query);
        }

        [TestMethod]
        public void SparqlScalarArgsGroupByCustom()
        {
            String query = "SELECT (GROUP_CONCAT(?s, ?p, ?o ; <ex:custom> = \" - \") AS ?concat) WHERE {?s ?p ?o}";
            Console.WriteLine(query);
            Console.WriteLine();
            this.CheckQueryFailsToParseIn11(query);
            this.CheckQueryFailsToParseInExtended(query);
        }

        [TestMethod]
        public void SparqlScalarArgsCountCustom()
        {
            String query = "SELECT (COUNT(?s ; <ex:custom> = \" - \") AS ?count) WHERE {?s ?p ?o}";
            Console.WriteLine(query);
            Console.WriteLine();
            this.CheckQueryFailsToParseIn11(query);
            this.CheckQueryParsesInExtended(query);
        }

        private void CheckQueryParsesIn11(String query)
        {
                SparqlQuery q = this._parser11.ParseFromString(query);
                Console.WriteLine("Query Parses under SPARQL 1.1 as expected");
        }

        private void CheckQueryFailsToParseIn11(String query)
        {
            try
            {
                SparqlQuery q = this._parser11.ParseFromString(query);
                Assert.Fail("Query Parsed under SPARQL 1.1 when it should have failed to parse");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Query Fails to parse under SPARQL 1.1 as expected");
            }
        }

        private void CheckQueryParsesInExtended(String query)
        {
                SparqlQuery q = this._parserExt.ParseFromString(query);
                Console.WriteLine("Query Parses under SPARQL 1.1 with Extensions as expected");
        }

        private void CheckQueryFailsToParseInExtended(String query)
        {
            try
            {
                SparqlQuery q = this._parserExt.ParseFromString(query);
                Assert.Fail("Query Parsed under SPARQL 1.1 with Extensions when it should have failed to parse");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Query Fails to parse under SPARQL 1.1 with Extensions as expected");
            }
        }
    }
}
