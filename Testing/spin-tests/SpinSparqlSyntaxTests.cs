using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Spin;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Spin
{
    [TestClass]
    public class SpinSparqlSyntaxTests
    {
        private const String SpinToRdfServiceUri = "http://sparqlpedia.org:8080/tbl/tbl/actions?action=sparqlmotion&id=sparql2spin&format=n3&text=";

        private IGraph GetSpinRdf(SparqlQuery q)
        {
            SparqlFormatter formatter = new SparqlFormatter();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SpinToRdfServiceUri + Uri.EscapeDataString(formatter.Format(q)));
            request.Method = "GET";
            request.Accept = MimeTypesHelper.HttpAcceptHeader;

            Graph g = new Graph();
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.ContentType == null || response.ContentType.Equals(String.Empty))
                {
                    Notation3Parser n3parser = new Notation3Parser();
                    n3parser.Load(g, new StreamReader(response.GetResponseStream()));
                }
                else
                {
                    IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                    parser.Load(g, new StreamReader(response.GetResponseStream()));
                }
                response.Close();
            }

            return g;
        }

        private void CompareSpinRdf(SparqlQuery q)
        {
            IGraph g = q.ToSpinRdf();
            NTriplesFormatter formatter = new NTriplesFormatter();
            CompressingTurtleWriter writer = new CompressingTurtleWriter(WriterCompressionLevel.High);
            Console.WriteLine("Our Representation");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
            //System.IO.StringWriter strWriter = new System.IO.StringWriter();
            //writer.Save(g, strWriter);
            //Console.WriteLine(strWriter.ToString());

            Console.WriteLine();
            Console.WriteLine("TopBraid's Representation");
            IGraph h = this.GetSpinRdf(q);

            //Delete the Text Triple from TopBraid's reponse
            h.Retract(h.GetTriplesWithPredicate(h.CreateUriNode(new Uri(SpinSyntax.SpinPropertyText))));

            foreach (Triple t in h.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
            //strWriter = new System.IO.StringWriter();
            //writer.Save(g, strWriter);
            //Console.WriteLine(strWriter.ToString());
            Console.WriteLine();

            GraphDiffReport report = h.Difference(g);
            if (!report.AreEqual)
            {
                TestTools.ShowDifferences(report);
                Assert.Fail("RDF Representation should be equal");
            }
        }

        [TestMethod]
        public void SpinSparqlSyntaxSelect()
        {
            String query = "SELECT ?x ?y WHERE { ?x ?p ?y }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            this.CompareSpinRdf(q);
        }

        [TestMethod]
        public void SpinSparqlSyntaxSelect2()
        {
            String query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            this.CompareSpinRdf(q);
        }

        [TestMethod]
        public void SpinSparqlSyntaxSelect3()
        {
            String query = "SELECT * WHERE {  ?s ?p ?o  {?x ?y ?z } }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            this.CompareSpinRdf(q);
        }
    }
}
