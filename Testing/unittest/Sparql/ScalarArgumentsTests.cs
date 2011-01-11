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
            try
            {
                SparqlQuery q = this._parser11.ParseFromString(query);
                Console.WriteLine("Query Parses under SPARQL 1.1 as expected");
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Parsing Error using SPARQL 1.1", ex, true);
            }
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
            try
            {
                SparqlQuery q = this._parserExt.ParseFromString(query);
                Console.WriteLine("Query Parses under SPARQL 1.1 with Extensions as expected");
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Parsing Error using SPARQL 1.1 with Extensions", ex, true);
            }
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
