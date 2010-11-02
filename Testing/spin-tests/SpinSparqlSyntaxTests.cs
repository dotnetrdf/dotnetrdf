using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Spin;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Spin
{
    [TestClass]
    public class SpinSparqlSyntaxTests
    {
        [TestMethod]
        public void SpinSparqlSyntaxSelect()
        {
            String query = "SELECT ?x ?y WHERE { ?x ?p ?y }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            IGraph g = q.ToSpinRdf();
            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
        }

        [TestMethod]
        public void SpinSparqlSyntaxSelect2()
        {
            String query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            IGraph g = q.ToSpinRdf();
            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
        }

        [TestMethod]
        public void SpinSparqlSyntaxSelect3()
        {
            String query = "SELECT * WHERE {  ?s ?p ?o  {?x ?y ?z } }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            IGraph g = q.ToSpinRdf();
            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
        }
    }
}
