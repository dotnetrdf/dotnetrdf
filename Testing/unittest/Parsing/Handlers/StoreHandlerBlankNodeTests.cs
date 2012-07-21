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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Test.Parsing.Handlers
{
    [TestClass]
    public class StoreHandlerBlankNodeTests
    {
        private const String TestFragment = @"@prefix : <http://example.org/bnodes#> .
{
  [] a :BNode , :AnonymousBNode .
  _:autos1 a :Node , :NamedBNode .
}
:graph {
  [] a :BNode , :AnonymousBNode .
  _:autos1 a :Node , :NamedBNode .
}
";
        private void EnsureTestData(String testFile)
        {
            if (!File.Exists(testFile))
            {
                TriGParser parser = new TriGParser();
                TripleStore store = new TripleStore();
                parser.Load(store, new TextReaderParams(new StringReader(TestFragment)));

                store.SaveToFile(testFile);
            }
        }

        private void EnsureTestResults(TripleStore store)
        {
            foreach (IGraph g in store.Graphs)
            {
                TestTools.ShowGraph(g);
                Console.WriteLine();
            }

            Assert.AreEqual(2, store.Graphs.Count, "Expected 2 Graphs");
            Assert.AreEqual(8, store.Graphs.Sum(g => g.Triples.Count), "Expected 4 Triples");

            IGraph def = store.Graph(null);
            IGraph named = store.Graph(new Uri("http://example.org/bnodes#graph"));

            HashSet<INode> subjects = new HashSet<INode>();
            foreach (Triple t in def.Triples)
            {
                subjects.Add(t.Subject);
            }
            foreach (Triple t in named.Triples)
            {
                subjects.Add(t.Subject);
            }

            Console.WriteLine("Subjects:");
            foreach (INode subj in subjects)
            {
                Console.WriteLine(subj.ToString() + " from Graph " + (subj.GraphUri != null ? subj.GraphUri.AbsoluteUri : "Default"));
            }
            Assert.AreEqual(4, subjects.Count, "Expected 4 distinct subjects");
        }

        [TestMethod]
        public void ParsingStoreHandlerBlankNodesTriG()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.ParsingStoreHandlerBlankNodesTriGActual));
        }

        public void ParsingStoreHandlerBlankNodesTriGActual()
        {
            EnsureTestData("test-bnodes.trig");

            TriGParser parser = new TriGParser();
            TripleStore store = new TripleStore();
            parser.Load(store, "test-bnodes.trig");

            EnsureTestResults(store);
        }

        [TestMethod]
        public void ParsingStoreHandlerBlankNodesTriX()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.ParsingStoreHandlerBlankNodesTriXActual));
        }

        public void ParsingStoreHandlerBlankNodesTriXActual()
        {
            EnsureTestData("test-bnodes.xml");

            TriXParser parser = new TriXParser();
            TripleStore store = new TripleStore();
            parser.Load(store, "test-bnodes.xml");

            EnsureTestResults(store);
        }

        [TestMethod]
        public void ParsingStoreHandlerBlankNodesNQuads()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.ParsingStoreHandlerBlankNodesNQuadsActual));
        }

        public void ParsingStoreHandlerBlankNodesNQuadsActual()
        {
            EnsureTestData("test-bnodes.nq");

            NQuadsParser parser = new NQuadsParser();
            TripleStore store = new TripleStore();
            parser.Load(store, "test-bnodes.nq");

            EnsureTestResults(store);
        }
    }
}
