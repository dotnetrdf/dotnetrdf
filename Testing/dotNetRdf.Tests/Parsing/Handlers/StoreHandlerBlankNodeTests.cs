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
using System.Linq;
using Xunit;

namespace VDS.RDF.Parsing.Handlers;


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
            var parser = new TriGParser();
            var store = new TripleStore();
            parser.Load(store, new StringReader(TestFragment));

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

        Assert.Equal(2, store.Graphs.Count);
        Assert.Equal(8, store.Graphs.Sum(g => g.Triples.Count));

        IGraph def = store[(IRefNode)null];
        IGraph named = store[new UriNode(new Uri("http://example.org/bnodes#graph"))];

        var subjects = new HashSet<INode>();
        var defSubjects = new HashSet<INode>();
        var namedSubjects = new HashSet<INode>();
        foreach (Triple t in def.Triples)
        {
            subjects.Add(t.Subject);
            defSubjects.Add(t.Subject);
        }
        foreach (Triple t in named.Triples)
        {
            subjects.Add(t.Subject);
            namedSubjects.Add(t.Subject);
        }
        Assert.Equal(2, defSubjects.Count);
        Assert.Equal(2, namedSubjects.Count);
        Assert.Equal(3, subjects.Count);
    }

    [Fact(SkipExceptions = [typeof(PlatformNotSupportedException)])]
    public void ParsingStoreHandlerBlankNodesTriG()
    {
        TestTools.TestInMTAThread(ParsingStoreHandlerBlankNodesTriGActual);
    }

    protected void ParsingStoreHandlerBlankNodesTriGActual()
    {
        EnsureTestData("test-bnodes.trig");

        var parser = new TriGParser();
        var store = new TripleStore();
        parser.Load(store, "test-bnodes.trig");

        EnsureTestResults(store);
    }

    [Fact(SkipExceptions = [typeof(PlatformNotSupportedException)])]
    public void ParsingStoreHandlerBlankNodesTriX()
    {
        TestTools.TestInMTAThread(ParsingStoreHandlerBlankNodesTriXActual);
    }

    protected void ParsingStoreHandlerBlankNodesTriXActual()
    {
        EnsureTestData("test-bnodes.xml");

        var parser = new TriXParser();
        var store = new TripleStore();
        parser.Load(store, "test-bnodes.xml");

        EnsureTestResults(store);
    }

    [Fact(SkipExceptions = [typeof(PlatformNotSupportedException)])]
    public void ParsingStoreHandlerBlankNodesNQuads()
    {
        TestTools.TestInMTAThread(ParsingStoreHandlerBlankNodesNQuadsActual);
    }

    protected void ParsingStoreHandlerBlankNodesNQuadsActual()
    {
        EnsureTestData("test-bnodes.nq");

        var parser = new NQuadsParser();
        var store = new TripleStore();
        parser.Load(store, "test-bnodes.nq");

        EnsureTestResults(store);
    }
}
