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
    public class SparqlParsingComplex
    {
        [TestMethod]
        public void SparqlNestedGraphPatternFirstItem()
        {
            try
            {
                SparqlQueryParser parser = new SparqlQueryParser();
                SparqlQuery q = parser.ParseFromFile("childgraphpattern.rq");

                Console.WriteLine(q.ToString());
                Console.WriteLine();
                Console.WriteLine(q.ToAlgebra().ToString());
            }
            catch (RdfParseException parseEx)
            {
                TestTools.ReportError("Parsing Error", parseEx, true);
            }
        }

        [TestMethod]
        public void SparqlNestedGraphPatternFirstItem2()
        {
            try
            {
                SparqlQueryParser parser = new SparqlQueryParser();
                SparqlQuery q = parser.ParseFromFile("childgraphpattern2.rq");

                Console.WriteLine(q.ToString());
                Console.WriteLine();
                Console.WriteLine(q.ToAlgebra().ToString());
            }
            catch (RdfParseException parseEx)
            {
                TestTools.ReportError("Parsing Error", parseEx, true);
            }
        }
    }
}
