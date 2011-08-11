using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Spin;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Spin
{
    [TestClass]
    public class SpinSparqlSyntaxTests
    {
        private const String SpinToRdfServiceUri = "http://spinservices.org:8080/spin/sparqlmotion?id=sparql2spin&format=turtle&text=";
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private int _dumpFile = 1;

        private IGraph GetSpinRdf(SparqlQuery q)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SpinToRdfServiceUri + Uri.EscapeDataString(q.ToString()));
            request.Accept = MimeTypesHelper.Any;

            Graph g = new Graph();
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                TurtleParser parser = new TurtleParser();
                parser.Load(g, new StreamReader(response.GetResponseStream()));
                response.Close();
            }

            return g;
        }

        private void CompareSpinRdf(String query)
        {
            SparqlQuery q = this._parser.ParseFromString(query);
            this.CompareSpinRdf(q);
        }

        private void CompareSpinRdfGraphPattern(String query)
        {
            SparqlQuery q = this._parser.ParseFromString(query);

            GraphPattern rgp = q.RootGraphPattern;
            Graph g = new Graph();
            rgp.ToSpinRdf(g, new SpinVariableTable(g));

            NTriplesFormatter formatter = new NTriplesFormatter();
            CompressingTurtleWriter writer = new CompressingTurtleWriter(WriterCompressionLevel.High);

            //Show our representation
            Console.WriteLine("Our Representation (" + g.Triples.Count + " Triple(s)):");
            foreach (Triple t in g.Triples.OrderBy(x => x))
            {
                Console.WriteLine(t.ToString(formatter));
            }
            Console.WriteLine();
            Console.WriteLine("As Compressed Turtle:");
            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            writer.Save(g, strWriter);
            Console.WriteLine(strWriter.ToString());
            Console.WriteLine();

            //Check Round Tripping
            Graph h = new Graph();
            StringParser.Parse(h, strWriter.ToString());

            //Show Round Tripped Representation
            Console.WriteLine("Round Tripped Representation:");
            foreach (Triple t in h.Triples.OrderBy(x => x))
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.AreEqual(g, h);
        }

        private void CompareSpinRdf(SparqlQuery q)
        {
            //Convert it to SPIN RDF
            IGraph g = q.ToSpinRdf();

            NTriplesFormatter formatter = new NTriplesFormatter();
            CompressingTurtleWriter writer = new CompressingTurtleWriter(WriterCompressionLevel.High);

            //Show our representation
            Console.WriteLine("Our Representation (" + g.Triples.Count + " Triple(s)):");
            foreach (Triple t in g.Triples.OrderBy(x => x))
            {
                Console.WriteLine(t.ToString(formatter));
            }
            Console.WriteLine();

            //Console.WriteLine("As Compressed Turtle:");
            //System.IO.StringWriter strWriter = new System.IO.StringWriter();
            //writer.Save(g, strWriter);
            //Console.WriteLine(strWriter.ToString());
            //Console.WriteLine();

            Console.WriteLine(new String('-', 30));

            //Get TopBraid Representation
            Console.WriteLine();
            IGraph h = this.GetSpinRdf(q);

            //Delete the Text Triple from TopBraid's response
            h.Retract(h.GetTriplesWithPredicate(h.CreateUriNode(new Uri(SpinSyntax.SpinPropertyText))));

            //Show TopBraid's representation
            Console.WriteLine("TopBraid's Representation (" + h.Triples.Count + " Triple(s)):");
            foreach (Triple t in h.Triples.OrderBy(x => x))
            {
                Console.WriteLine(t.ToString(formatter));
            }
            Console.WriteLine();

            //Console.WriteLine("As Compressed Turtle:");
            //strWriter = new System.IO.StringWriter();
            //writer.Save(h, strWriter);
            //Console.WriteLine(strWriter.ToString());
            //Console.WriteLine();

            //Show Differences (if any)
            GraphDiffReport report = h.Difference(g);
            if (!report.AreEqual)
            {
                TestTools.ShowDifferences(report);

                //Dump two graphs as SVG so we can visually see the difference (hopefully)
                GraphVizGenerator generator = new GraphVizGenerator("svg");
                generator.Generate(g, "dump" + (_dumpFile++) + ".svg", false);
                generator.Generate(h, "dump" + (_dumpFile++) + ".svg", false);
                Console.WriteLine("Dumped SVGs of graphs as 'dump" + (_dumpFile - 2) + ".svg' and 'dump" + (_dumpFile - 1) + ".svg'");

                Assert.Fail("RDF Representation should be equal");
            }
        }

        #region Some General Tests

        [TestMethod]
        public void SpinSparqlSyntaxSelect()
        {
            String query = "SELECT ?x ?y WHERE { ?x ?p ?y }";
            this.CompareSpinRdf(query);
        }

        [TestMethod]
        public void SpinSparqlSyntaxSelect2()
        {
            String query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z }";
            this.CompareSpinRdf(query);
        }

        [TestMethod]
        public void SpinSparqlSyntaxSelect3()
        {
            String query = "SELECT * WHERE {  ?s ?p ?o  {?x ?y ?z } }";
            this.CompareSpinRdf(query);
        }

        [TestMethod]
        public void SpinSparqlSyntaxSelect4()
        {
            String query = "SELECT * WHERE {  ?s ?p ?o  OPTIONAL {?x ?y ?z } }";
            this.CompareSpinRdf(query);
        }

        [TestMethod]
        public void SpinSparqlSyntaxSelect5()
        {
            String query = "SELECT * WHERE {  ?s ?p ?o  GRAPH <http://example.org/graph> {?x ?y ?z } }";
            this.CompareSpinRdf(query);
        }

        [TestMethod]
        public void SpinSparqlSyntaxSelect6()
        {
            String query = "SELECT * WHERE {  ?s ?p ?o  GRAPH ?g {?x ?y ?z } }";
            this.CompareSpinRdf(query);
        }

        #endregion

        #region Tests based on examples from the SPIN RDF Specification

        [TestMethod]
        public void SpinSparqlSyntaxSpecExampleOptional()
        {
            String query = "SELECT * WHERE { OPTIONAL { ?this a ?type } }";
            this.CompareSpinRdfGraphPattern(query);
        }

        [TestMethod]
        public void SpinSparqlSyntaxSpecExampleUnion()
        {
            String query = "PREFIX ex: <http://example.org/> SELECT * WHERE { { ?this ex:age 42 } UNION { ?this ex:age 43 } }";
            this.CompareSpinRdfGraphPattern(query);
        }

        #endregion
    }
}
