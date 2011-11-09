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
        public void SparqlFormattingFilter()
        {
            String query = "SELECT * WHERE { { ?s ?p ?o } FILTER(ISURI(?o)) }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Console.WriteLine("ToString() form:");
            Console.WriteLine(q.ToString());
            Console.WriteLine();
            Console.WriteLine("Format() form:");
            Console.WriteLine(this._formatter.Format(q));
        }
    }
}
