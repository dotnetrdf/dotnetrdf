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
        private const String SpinToRdfServiceUri = "http://sparqlpedia.org:8080/tbl/tbl/actions?action=sparqlmotion&id=sparql2spin&format=n3&text=";
        private SparqlQueryParser _parser = new SparqlQueryParser();

        private IGraph GetSpinRdf(SparqlQuery q)
        {
            //TODO: Get this code working properly
            return new Graph();

            SparqlFormatter formatter = new SparqlFormatter();
            String query = formatter.Format(q);

            //Invoke a process to generate TopBraid SPIN using their java SPIN API implementation
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "java";
            info.Arguments = "-classpath lib/*;spinrdf-1.1.2.jar;. SpinGenerator \"" + query.Replace("\"", "\\\"") + "\"";
            info.RedirectStandardOutput = true;
            info.CreateNoWindow = true;
            info.UseShellExecute = false;

            Process p = new Process();
            p.StartInfo = info;
            p.Start();
            using (StreamWriter writer = new StreamWriter("topbraid-spin.ttl", false, Encoding.UTF8))
            {
                writer.Write(p.StandardOutput.ReadToEnd());
                writer.Close();
            }
            p.WaitForExit();

            Graph g = new Graph();
            g.LoadFromFile("topbraid-spin.ttl");
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
            Console.WriteLine("Our Representation:");
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
            Console.WriteLine("Our Representation:");
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

            Console.WriteLine(new String('-', 30));

            //Get and Show TopBraid Representation
            Console.WriteLine();
            Console.WriteLine("TopBraid's Representation:");
            IGraph h = this.GetSpinRdf(q);

            //Delete the Text Triple from TopBraid's response
            h.Retract(h.GetTriplesWithPredicate(h.CreateUriNode(new Uri(SpinSyntax.SpinPropertyText))));

            foreach (Triple t in h.Triples.OrderBy(x => x))
            {
                Console.WriteLine(t.ToString(formatter));
            }
            Console.WriteLine();
            Console.WriteLine("As Compressed Turtle:");
            strWriter = new System.IO.StringWriter();
            writer.Save(h, strWriter);
            Console.WriteLine(strWriter.ToString());
            Console.WriteLine();

            //Show Differences (if any)
            GraphDiffReport report = h.Difference(g);
            if (!report.AreEqual)
            {
                TestTools.ShowDifferences(report);
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
