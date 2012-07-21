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
using VDS.RDF.Storage;
using VDS.RDF.Test.Storage;

namespace VDS.RDF.Test
{
    [TestClass]
    public class NativeStoreTests
        : BaseTest
    {
        [TestMethod]
        public void StorageNativeGraph()
        {
            //Load in our Test Graph
            TurtleParser ttlparser = new TurtleParser();
            Graph g = new Graph();
            ttlparser.Load(g, "Turtle.ttl");

            Console.WriteLine("Loaded Test Graph OK");
            Console.WriteLine("Test Graph contains:");

            Assert.IsFalse(g.IsEmpty, "Test Graph should be non-empty");

            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }
            Console.WriteLine();

            //Create our Native Managers
            List<IStorageProvider> managers = new List<IStorageProvider>() {
                new InMemoryManager(),
                VirtuosoTest.GetConnection()
            };

            //Save the Graph to each Manager
            foreach (IStorageProvider manager in managers)
            {
                Console.WriteLine("Saving using '" + manager.GetType().ToString() + "'");
                manager.SaveGraph(g);
                Console.WriteLine("Saved OK");
                Console.WriteLine();
            }

            //Load Back from each Manager
            foreach (IStorageProvider manager in managers)
            {
                Console.WriteLine("Loading using '" + manager.GetType().ToString() + "' with a NativeGraph");
                StoreGraphPersistenceWrapper native = new StoreGraphPersistenceWrapper(manager, g.BaseUri);
                Console.WriteLine("Loaded OK");

                Assert.IsFalse(native.IsEmpty, "Retrieved Graph should contain Triples");
                Assert.AreEqual(g.Triples.Count, native.Triples.Count, "Retrieved Graph should contain same number of Triples as original Graph");

                Console.WriteLine("Loaded Graph contains:");
                foreach (Triple t in native.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();

                native.Dispose();
            }
        }
    }
}
