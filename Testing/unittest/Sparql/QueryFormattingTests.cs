using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class QueryFormattingTests
    {
        private SparqlFormatter _formatter = new SparqlFormatter();

        [TestMethod]
        public void SparqlFormattingFilter1()
        {
            String query = "SELECT * WHERE { { ?s ?p ?o } FILTER(ISURI(?o)) }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Console.WriteLine("ToString() form:");
            String toString = q.ToString();
            Console.WriteLine(toString);
            Console.WriteLine();
            Console.WriteLine("Format() form:");
            String formatted = this._formatter.Format(q);
            Console.WriteLine(formatted);

            Assert.IsTrue(toString.Contains("FILTER"), "ToString() form should contain FILTER");
            Assert.IsTrue(formatted.Contains("FILTER"), "Format() form should contain FILTER");
        }

        [TestMethod]
        public void SparqlFormattingFilter2()
        {
            String query = "SELECT * WHERE { { ?s ?p ?o } FILTER(REGEX(?o, 'search', 'i')) }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Console.WriteLine("ToString() form:");
            String toString = q.ToString();
            Console.WriteLine(toString);
            Console.WriteLine();
            Console.WriteLine("Format() form:");
            String formatted = this._formatter.Format(q);
            Console.WriteLine(formatted);

            Assert.IsTrue(toString.Contains("FILTER"), "ToString() form should contain FILTER");
            Assert.IsTrue(toString.Contains("i"), "ToString() form should contain i option");
            Assert.IsTrue(formatted.Contains("FILTER"), "Format() form should contain FILTER");
            Assert.IsTrue(toString.Contains("i"), "Format() form should contain i option");
        }
    }
}
