/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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

                Graph g = new Graph(new IndexedTripleCollection(1));
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

                Graph g = new Graph(new IndexedTripleCollection(1));
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

                Graph g = new Graph(new IndexedTripleCollection(1));
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

                Graph g = new Graph(new IndexedTripleCollection(1));
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

                Graph g = new Graph(new IndexedTripleCollection(1));
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

                Graph g = new Graph(new IndexedTripleCollection(1));
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

                Graph g = new Graph(new IndexedTripleCollection(1));
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
