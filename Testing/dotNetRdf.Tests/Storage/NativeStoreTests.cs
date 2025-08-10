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
using System.IO;
using VDS.RDF.Parsing;
using Xunit;

namespace VDS.RDF.Storage;


public class NativeStoreTests
    : BaseTest
{
    public NativeStoreTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void StorageNativeGraph()
    {
        //Load in our Test Graph
        var ttlparser = new TurtleParser();
        var testGraphName  =new UriNode(new Uri("http://example.org/testGraph"));
        var g = new Graph(testGraphName);
        ttlparser.Load(g, Path.Combine("resources", "Turtle.ttl"));

        Assert.False(g.IsEmpty, "Test Graph should be non-empty");

        //Create our Native Managers
        var managers = new List<IStorageProvider>
        {
            new InMemoryManager()
        };

        //Save the Graph to each Manager
        foreach (IStorageProvider manager in managers)
        {
            manager.SaveGraph(g);
        }

        //Load Back from each Manager
        foreach (IStorageProvider manager in managers)
        {
            var native = new StoreGraphPersistenceWrapper(manager, g.Name);
            Assert.False(native.IsEmpty, "Retrieved Graph should contain Triples");
            Assert.Equal(g.Triples.Count, native.Triples.Count);
            native.Dispose();
        }
    }
}
