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
using System.Linq;
using System.Text;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.XunitExtensions;

namespace VDS.RDF
{

    public class NativeStoreTests
        : BaseTest
    {
        [SkippableFact]
        public void StorageNativeGraph()
        {
            //Load in our Test Graph
            TurtleParser ttlparser = new TurtleParser();
            Graph g = new Graph();
            ttlparser.Load(g, "resources\\Turtle.ttl");

            Console.WriteLine("Loaded Test Graph OK");
            Console.WriteLine("Test Graph contains:");

            Assert.False(g.IsEmpty, "Test Graph should be non-empty");

            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }
            Console.WriteLine();

            //Create our Native Managers
            List<IStorageProvider> managers = new List<IStorageProvider>() {
                new InMemoryManager(),
#if NET40
                VirtuosoTest.GetConnection()
#endif
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

                Assert.False(native.IsEmpty, "Retrieved Graph should contain Triples");
                Assert.Equal(g.Triples.Count, native.Triples.Count);

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
