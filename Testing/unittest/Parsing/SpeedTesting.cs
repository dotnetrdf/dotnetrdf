/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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

namespace VDS.RDF.Parsing
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

            //Force a GC prior to each of these tests
            GC.GetTotalMemory(true);
        }

        private void CalculateSpeed(int triples, Stopwatch watch)
        {
            double tps = (double)triples * (1000.0d / (double)watch.ElapsedMilliseconds);
            Console.WriteLine("Triples/Second = " + tps);
        }

        [TestMethod]
        public void ParsingSpeedTurtle10Thousand()
        {
            try
            {
                Options.InternUris = false;
                EnsureTestData(10000, "10thou.ttl", new TurtleFormatter());

                Graph g = new Graph();
                Stopwatch watch = new Stopwatch();
                TurtleParser parser = new TurtleParser();

                watch.Start();
                parser.Load(g, "10thou.ttl");
                watch.Stop();

                Console.WriteLine(watch.Elapsed);
                this.CalculateSpeed(10000, watch);
            } 
            finally 
            {
                Options.InternUris = true;
            }
        }

        [TestMethod]
        public void ParsingSpeedTurtle100Thousand()
        {
            try
            {
                Options.InternUris = false;
                EnsureTestData(100000, "100thou.ttl", new TurtleFormatter());

                Graph g = new Graph();
                Stopwatch watch = new Stopwatch();
                TurtleParser parser = new TurtleParser();

                watch.Start();
                parser.Load(g, "100thou.ttl");
                watch.Stop();

                Console.WriteLine(watch.Elapsed);
                this.CalculateSpeed(100000, watch);
            }
            finally
            {
                Options.InternUris = true;
            }
        }

        [TestMethod]
        public void ParsingSpeedTurtle500Thousand()
        {
            try
            {
                Options.InternUris = false;
                EnsureTestData(500000, "500thou.ttl", new TurtleFormatter());

                Graph g = new Graph();
                Stopwatch watch = new Stopwatch();
                TurtleParser parser = new TurtleParser();

                watch.Start();
                parser.Load(g, "500thou.ttl");
                watch.Stop();

                Console.WriteLine(watch.Elapsed);
                this.CalculateSpeed(500000, watch);
            }
            finally
            {
                Options.InternUris = true;
            }
        }

        [TestMethod]
        public void ParsingSpeedTurtle10ThousandCountOnly()
        {
            try
            {
                Options.InternUris = false;
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
            finally
            {
                Options.InternUris = true;
            }
        }

        [TestMethod]
        public void ParsingSpeedTurtle100ThousandCountOnly()
        {
            try
            {
                Options.InternUris = false;
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
            finally
            {
                Options.InternUris = true;
            }
        }

        [TestMethod]
        public void ParsingSpeedNTriples10Thousand()
        {
            try
            {
                Options.InternUris = false;
                EnsureTestData(10000, "10thou.nt", new NTriplesFormatter());

                Graph g = new Graph();
                Stopwatch watch = new Stopwatch();
                NTriplesParser parser = new NTriplesParser();

                watch.Start();
                parser.Load(g, "10thou.nt");
                watch.Stop();

                Console.WriteLine(watch.Elapsed);
                this.CalculateSpeed(10000, watch);
            }
            finally
            {
                Options.InternUris = true;
            }
        }

        [TestMethod]
        public void ParsingSpeedNTriples100Thousand()
        {
            try
            {
                Options.InternUris = false;
                EnsureTestData(100000, "100thou.nt", new NTriplesFormatter());

                Graph g = new Graph();
                Stopwatch watch = new Stopwatch();
                NTriplesParser parser = new NTriplesParser();

                watch.Start();
                parser.Load(g, "100thou.nt");
                watch.Stop();

                Console.WriteLine(watch.Elapsed);
                this.CalculateSpeed(100000, watch);
            }
            finally
            {
                Options.InternUris = true;
            }
        }

        [TestMethod]
        public void ParsingSpeedNTriples500Thousand()
        {
            try
            {
                Options.InternUris = false;
                EnsureTestData(500000, "500thou.nt", new NTriplesFormatter());

                Graph g = new Graph();
                Stopwatch watch = new Stopwatch();
                NTriplesParser parser = new NTriplesParser();

                watch.Start();
                parser.Load(g, "500thou.nt");
                watch.Stop();

                Console.WriteLine(watch.Elapsed);
                this.CalculateSpeed(500000, watch);
            }
            finally
            {
                Options.InternUris = true;
            }
        }

        [TestMethod]
        public void ParsingSpeedNTriples1Million()
        {
            try
            {
                Options.InternUris = false;
                EnsureTestData(1000000, "million.nt", new NTriplesFormatter());

                Graph g = new Graph();
                Stopwatch watch = new Stopwatch();
                NTriplesParser parser = new NTriplesParser();

                watch.Start();
                parser.Load(g, "million.nt");
                watch.Stop();

                Console.WriteLine(watch.Elapsed);
                this.CalculateSpeed(1000000, watch);
            }
            finally
            {
                Options.InternUris = true;
            }
        }
    }
}
