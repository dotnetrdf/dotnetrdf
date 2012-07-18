using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Web;

namespace VDS.RDF.Test.Web
{
    [TestClass]
    public class ETagTests
    {
        [TestMethod]
        public void WebETagComputation()
        {
            try
            {
                Graph g = new Graph();
                FileLoader.Load(g, "InferenceTest.ttl");

                Stopwatch timer = new Stopwatch();
                timer.Start();
                String etag = g.GetETag();
                timer.Stop();
                Console.WriteLine("ETag is " + etag);
                Console.WriteLine(etag.Length);
                Console.WriteLine("Took " + timer.Elapsed + " to calculate");
                Console.WriteLine();

                Assert.IsFalse(String.IsNullOrEmpty(etag));
                Assert.AreEqual(40, etag.Length, "Expected ETag Length was 40 characters");

                timer.Reset();
                timer.Start();
                String etag2 = g.GetETag();
                timer.Stop();
                Console.WriteLine("ETag is " + etag2);
                Console.WriteLine("Took " + timer.Elapsed + " to calculate");
                Console.WriteLine();

                Assert.AreEqual(etag, etag2, "ETags should be consistent unless the Graph changes");
                Assert.AreEqual(40, etag2.Length, "Expected ETag Length was 40 characters");

                g.Retract(g.Triples.First());
                timer.Reset();
                timer.Start();
                String etag3 = g.GetETag();
                timer.Stop();
                Console.WriteLine("Graph has now been altered so ETag should change");
                Console.WriteLine("ETag is " + etag3);
                Console.WriteLine("Took " + timer.Elapsed + " to calculate");
                Console.WriteLine();

                Assert.AreNotEqual(etag, etag3, "ETags should change if the Graph changes");
                Assert.AreNotEqual(etag2, etag3, "ETags should change if the Graph changes");
                Assert.AreEqual(40, etag3.Length, "Expected ETag Length was 40 characters");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
