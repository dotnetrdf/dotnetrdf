using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Parsing
{
    [TestClass]
    public class SpeedTesting
    {
        private void EnsureTestData(int triples, String file, ITripleFormatter formatter)
        {
            if (!File.Exists(file))
            {
                Graph g = new Graph();
                g.NamespaceMap.AddNamespace(String.Empty, new Uri("http://example.org/node#"));

                int padding = triples.ToString().Length;

                using (StreamWriter writer = new StreamWriter(file))
                {
                    for (int i = 1; i <= triples; i++)
                    {
                        IUriNode temp = g.CreateUriNode(":" + i);
                        writer.WriteLine(formatter.Format(new Triple(temp, temp, temp)));
                    }
                    writer.Close();
                }
            }
        }

        private void CalculateSpeed(int triples, Stopwatch watch)
        {
            double tps = (double)triples * (1000.0d / (double)watch.ElapsedMilliseconds);
            Console.WriteLine("Triples/Second = " + tps);
        }

        [TestMethod]
        public void ParsingSpeedTurtle10Thousand()
        {
            EnsureTestData(10000, "10thou.ttl", new TurtleFormatter());

            Graph g = new Graph(new IndexedTripleCollection(1));
            Stopwatch watch = new Stopwatch();
            TurtleParser parser = new TurtleParser();

            watch.Start();
            parser.Load(g, "10thou.ttl");
            watch.Stop();

            Console.WriteLine(watch.Elapsed);
            this.CalculateSpeed(10000, watch);
        }

        [TestMethod]
        public void ParsingSpeedTurtle100Thousand()
        {
            EnsureTestData(100000, "100thou.ttl", new TurtleFormatter());

            Graph g = new Graph(new IndexedTripleCollection(1));
            Stopwatch watch = new Stopwatch();
            TurtleParser parser = new TurtleParser();

            watch.Start();
            parser.Load(g, "100thou.ttl");
            watch.Stop();

            Console.WriteLine(watch.Elapsed);
            this.CalculateSpeed(100000, watch);
        }

        [TestMethod]
        public void ParsingSpeedTurtle500Thousand()
        {
            EnsureTestData(500000, "500thou.ttl", new TurtleFormatter());

            Graph g = new Graph(new IndexedTripleCollection(1));
            Stopwatch watch = new Stopwatch();
            TurtleParser parser = new TurtleParser();

            watch.Start();
            parser.Load(g, "500thou.ttl");
            watch.Stop();

            Console.WriteLine(watch.Elapsed);
            this.CalculateSpeed(500000, watch);
        }

        [TestMethod]
        public void ParsingSpeedTurtle10ThousandCountOnly()
        {
            EnsureTestData(10000, "10thou.ttl", new TurtleFormatter());

            CountHandler handler = new CountHandler();
            Stopwatch watch = new Stopwatch();
            TurtleParser parser = new TurtleParser();

            watch.Start();
            parser.Load(handler, "10thou.ttl");
            watch.Stop();

            Console.WriteLine(watch.Elapsed);
            this.CalculateSpeed(10000, watch);

            Assert.AreEqual(10000, handler.Count);
        }

        [TestMethod]
        public void ParsingSpeedTurtle100ThousandCountOnly()
        {
            EnsureTestData(100000, "100thou.ttl", new TurtleFormatter());

            CountHandler handler = new CountHandler();
            Stopwatch watch = new Stopwatch();
            TurtleParser parser = new TurtleParser();

            watch.Start();
            parser.Load(handler, "100thou.ttl");
            watch.Stop();

            Console.WriteLine(watch.Elapsed);
            this.CalculateSpeed(100000, watch);

            Assert.AreEqual(100000, handler.Count);
        }

        [TestMethod]
        public void ParsingSpeedNTriples10Thousand()
        {
            EnsureTestData(10000, "10thou.nt", new NTriplesFormatter());

            Graph g = new Graph(new IndexedTripleCollection(1));
            Stopwatch watch = new Stopwatch();
            NTriplesParser parser = new NTriplesParser();

            watch.Start();
            parser.Load(g, "10thou.nt");
            watch.Stop();

            Console.WriteLine(watch.Elapsed);
            this.CalculateSpeed(10000, watch);
        }

        [TestMethod]
        public void ParsingSpeedNTriples100Thousand()
        {
            EnsureTestData(100000, "100thou.nt", new NTriplesFormatter());

            Graph g = new Graph(new IndexedTripleCollection(1));
            Stopwatch watch = new Stopwatch();
            NTriplesParser parser = new NTriplesParser();

            watch.Start();
            parser.Load(g, "100thou.nt");
            watch.Stop();

            Console.WriteLine(watch.Elapsed);
            this.CalculateSpeed(100000, watch);
        }

        [TestMethod]
        public void ParsingSpeedNTriples500Thousand()
        {
            EnsureTestData(500000, "500thou.nt", new NTriplesFormatter());

            Graph g = new Graph(new IndexedTripleCollection(1));
            Stopwatch watch = new Stopwatch();
            NTriplesParser parser = new NTriplesParser();

            watch.Start();
            parser.Load(g, "500thou.nt");
            watch.Stop();

            Console.WriteLine(watch.Elapsed);
            this.CalculateSpeed(500000, watch);
        }

        [TestMethod]
        public void ParsingSpeedNTriples1Million()
        {
            EnsureTestData(1000000, "million.nt", new NTriplesFormatter());

            Graph g = new Graph(new IndexedTripleCollection(1));
            Stopwatch watch = new Stopwatch();
            NTriplesParser parser = new NTriplesParser();

            watch.Start();
            parser.Load(g, "million.nt");
            watch.Stop();

            Console.WriteLine(watch.Elapsed);
            this.CalculateSpeed(1000000, watch);
        }
    }
}
