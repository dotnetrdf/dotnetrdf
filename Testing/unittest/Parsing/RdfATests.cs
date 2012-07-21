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
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Test.Parsing
{
    [TestClass]
    public class RdfATests
    {
        [TestMethod]
        public void ParsingRdfABadSyntax()
        {
            RdfAParser parser = new RdfAParser();
            Graph g = new Graph();
            Console.WriteLine("Tests parsing a file which has much invalid RDFa syntax in it, some triples will be produced (6-8) but most of the triples are wrongly encoded and will be ignored");
            g.BaseUri = new Uri("http://www.wurvoc.org/vocabularies/om-1.6/Kelvin_scale");
            FileLoader.Load(g, "bad_rdfa.html");

            Console.WriteLine(g.Triples.Count + " Triples");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }

            Console.WriteLine();
            CompressingTurtleWriter ttlwriter = new CompressingTurtleWriter(WriterCompressionLevel.High);
            ttlwriter.HighSpeedModePermitted = false;
            ttlwriter.Save(g, "test.ttl");
        }

        [TestMethod]
        public void ParsingRdfAGoodRelations()
        {
            try
            {
                Options.UriLoaderCaching = false;
                List<String> tests = new List<string>()
                {
                    "gr1",
                    "gr2",
                    "gr3"
                };

                FileLoader.Warning += TestTools.WarningPrinter;

                foreach (String test in tests)
                {
                    Console.WriteLine("Test '" + test + "'");
                    Console.WriteLine();

                    Graph g = new Graph();
                    g.BaseUri = new Uri("http://example.org/goodrelations");
                    Graph h = new Graph();
                    h.BaseUri = g.BaseUri;

                    Console.WriteLine("Graph A Warnings:");
                    FileLoader.Load(g, test + ".xhtml");
                    Console.WriteLine();
                    Console.WriteLine("Graph B Warnings:");
                    FileLoader.Load(h, test + "b.xhtml");
                    Console.WriteLine();

                    Console.WriteLine("Graph A (RDFa 1.0)");
                    TestTools.ShowGraph(g);
                    Console.WriteLine();
                    Console.WriteLine("Graph B (RDFa 1.1)");
                    TestTools.ShowGraph(h);
                    Console.WriteLine();

                    Assert.AreEqual(g, h, "Graphs should have been equal");
                }
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void ParsingRdfABadProfile()
        {
            RdfAParser parser = new RdfAParser(RdfASyntax.RDFa_1_1);
            parser.Warning += TestTools.WarningPrinter;

            Graph g = new Graph();
            parser.Load(g, "bad_profile.xhtml");

            TestTools.ShowGraph(g);

            Assert.AreEqual(1, g.Triples.Count, "Should only produce 1 Triple");
        }
    }
}
