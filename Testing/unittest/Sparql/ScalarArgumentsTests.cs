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
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Test.Sparql
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
            this.CheckQueryParsesIn11(query);
            this.CheckQueryParsesInExtended(query);
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
