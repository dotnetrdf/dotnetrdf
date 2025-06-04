/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

#nullable enable

using System;
using System.Linq;
using Xunit;

namespace VDS.RDF;

/// <summary>
/// Abstract set of Triple Stores tests which can be used to test any <see cref="ITripleStore"/> implementation
/// </summary>

public abstract class AbstractTripleStoreTests
{
    /// <summary>
    /// Method which derived tests should implement to provide a fresh instance that can be used for testing
    /// </summary>
    /// <returns></returns>
    protected abstract ITripleStore GetInstance();

    [Fact]
    public void TripleStoreIsEmpty01()
    {
        ITripleStore store = GetInstance();

        Assert.True(store.IsEmpty);
    }

    [Fact]
    public void TripleStoreIsEmpty02()
    {
        ITripleStore store = GetInstance();
        store.Add(new Graph());

        Assert.False(store.IsEmpty);
    }

    [Fact]
    public void TripleStoreAdd01()
    {
        ITripleStore store = GetInstance();

        var g = new Graph();
        store.Add(g);

        Assert.False(store.IsEmpty);
        Assert.True(store.HasGraph(g.Name));
    }

    [Fact]
    public void TripleStoreAdd02()
    {
        ITripleStore store = GetInstance();

        IGraph g = new Graph(new UriNode(new Uri("https://example.org/graph")));
        store.Add(g);

        Assert.False(store.IsEmpty);
        Assert.True(store.HasGraph(g.Name));
    }

    [Fact]
    public void TripleStoreHasGraph01()
    {
        ITripleStore store = GetInstance();

        Assert.False(store.HasGraph(new UriNode(new Uri("https://thereisnosuchdomain.com:1234/graph"))));
    }

    [Fact]
    public void TripleStoreHasGraph02()
    {
        ITripleStore store = GetInstance();

        IGraph g = new Graph();
        store.Add(g);

        Assert.True(store.HasGraph((IRefNode?)null));
    }

    [Fact]
    public void TripleStoreHasGraph03()
    {
        ITripleStore store = GetInstance();

        IGraph g = new Graph(new UriNode(new Uri("https://nosuchdomain.com/graph")));
        store.Add(g);

        Assert.True(store.HasGraph(g.Name));
    }

    [Fact]
    public void EnumerateQuads01()
    {
        ITripleStore store = GetInstance();
        IGraph g = new Graph(new UriNode(new Uri("https://example.org/g1")));
        INode s1 = g.CreateUriNode(new Uri("https://example.org/s1"));
        IUriNode p = g.CreateUriNode(new Uri("https://example.org/p"));
        IUriNode o = g.CreateUriNode(new Uri("https://example.org/o"));
        IUriNode s2 = g.CreateUriNode(new Uri("https://example.org/s2"));
        g.Assert(new Triple(s1, p, o));
        g.Assert(new Triple(s2, p, o));
        
        // No quads until graph is added
        var quads = store.Quads.ToList();
        Assert.Empty(quads);
        
        store.Add(g);
        
        quads = store.Quads.ToList();
        Assert.Equal(2, quads.Count);
        Assert.Contains(new Quad(s1, p, o, g.Name), quads);
        Assert.Contains(new Quad(s2, p, o, g.Name), quads);
    }
    
    [Fact]
    public void EnumerateQuads02()
    {
        ITripleStore store = GetInstance();
        var g1 = new Graph(new UriNode(new Uri("https://example.org/g1")));
        INode s1 = g1.CreateUriNode(new Uri("https://example.org/s1"));
        IUriNode p = g1.CreateUriNode(new Uri("https://example.org/p"));
        IUriNode o = g1.CreateUriNode(new Uri("https://example.org/o"));
        IUriNode s2 = g1.CreateUriNode(new Uri("https://example.org/s2"));
        g1.Assert(new Triple(s1, p, o));
        g1.Assert(new Triple(s2, p, o));

        var g2 = new Graph(new UriNode(new Uri("https://example.org/g2")));
        INode s12 = g2.CreateUriNode(new Uri("https://example.org/s1"));
        IUriNode p2 = g2.CreateUriNode(new Uri("https://example.org/p"));
        IUriNode o2 = g2.CreateUriNode(new Uri("https://example.org/o"));
        IUriNode s22 = g2.CreateUriNode(new Uri("https://example.org/s2"));
        g2.Assert(new Triple(s12, p2, o2));
        g2.Assert(new Triple(s22, p2, o2));

        // No quads until graph is added
        var quads = store.Quads.ToList();
        Assert.Empty(quads);
        
        store.Add(g1);
        
        quads = store.Quads.ToList();
        Assert.Equal(2, quads.Count);
        Assert.Contains(new Quad(s1, p, o, g1.Name), quads);
        Assert.Contains(new Quad(s2, p, o, g1.Name), quads);

        store.Add(g2);

        quads = store.Quads.ToList();
        Assert.Equal(4, quads.Count);
        Assert.Contains(new Quad(s1, p, o, g1.Name), quads);
        Assert.Contains(new Quad(s2, p, o, g1.Name), quads);
        Assert.Contains(new Quad(s12, p2, o2, g2.Name), quads);
        Assert.Contains(new Quad(s22, p2, o2, g2.Name), quads);
    }

    [Fact]
    public void AssertQuadsIntoNewGraphs()
    {
        ITripleStore store = GetInstance();
        INode s = new UriNode(store.UriFactory.Create("https://example.org/s"));
        INode p = new UriNode(store.UriFactory.Create("https://example.org/p"));
        INode o = new UriNode(store.UriFactory.Create("https://example.org/o"));
        IRefNode g1 = new UriNode(store.UriFactory.Create("https://example.org/g1"));
        IRefNode g2 = new UriNode(store.UriFactory.Create("https://example.org/g2"));
        store.Assert(new Quad(s,p,o,g1));
        store.Assert(new Quad(s, p,o, g2));
        Assert.Equal(2, store.Graphs.Count);
        Assert.Equal(2, store.Quads.ToList().Count);
    }

    [Fact]
    public void AssertQuadsIntoExistingGraphs()
    {
        ITripleStore store = GetInstance();
        INode s = new UriNode(store.UriFactory.Create("https://example.org/s"));
        INode p = new UriNode(store.UriFactory.Create("https://example.org/p"));
        INode o = new UriNode(store.UriFactory.Create("https://example.org/o"));
        INode o2= new UriNode(store.UriFactory.Create("https://example.org/o2"));
        IRefNode g1 = new UriNode(store.UriFactory.Create("https://example.org/g1"));
        var g = new Graph(g1);
        g.Assert(new Triple(s, p, o));
        store.Add(g);
        
        // Asserting an existing quad does not change the graph
        store.Assert(new Quad(s, p, o, g1));
        Assert.Single(store.Quads);
        Assert.Single(store.Graphs[g1].Triples);
        
        // Asserting a new quad in an existing graph updates the graph
        store.Assert(new Quad(s, p, o2, g1));
        Assert.Equal(2, store.Quads.Count());
        Assert.Equal(2, store.Graphs[g1].Triples.Count);
    }

    [Fact]
    public void AssertQuadsIntoNewUnnamedGraph()
    {
        ITripleStore store = GetInstance();
        INode s = new UriNode(store.UriFactory.Create("https://example.org/s"));
        INode p = new UriNode(store.UriFactory.Create("https://example.org/p"));
        INode o = new UriNode(store.UriFactory.Create("https://example.org/o"));
        store.Assert(new Quad(s, p, o, null));
        Assert.True(store.HasGraph((IRefNode?)null));
        Assert.Single(store.Graphs[(IRefNode?)null].Triples);
    }

    [Fact]
    public void AssertQuadsIntoExistingUnnamedGraph()
    {
        ITripleStore store = GetInstance();
        INode s = new UriNode(store.UriFactory.Create("https://example.org/s"));
        INode p = new UriNode(store.UriFactory.Create("https://example.org/p"));
        INode o = new UriNode(store.UriFactory.Create("https://example.org/o"));
        INode o2= new UriNode(store.UriFactory.Create("https://example.org/o2"));
        var g = new Graph();
        g.Assert(new Triple(s, p, o));
        store.Add(g);
        store.Assert(new Quad(s, p, o, null));
        Assert.Single(store.Graphs);
        Assert.Single(store.Graphs[(IRefNode?)null].Triples);
        store.Assert(new Quad(s, p, o2, null));
        Assert.Single(store.Graphs);
        Assert.Equal(2, store.Graphs[(IRefNode?)null].Triples.Count);
    }

    [Fact]
    public void RetractQuadsFromExistingNamedGraph()
    {
        ITripleStore store = GetInstance();
        INode s = new UriNode(store.UriFactory.Create("https://example.org/s"));
        INode p = new UriNode(store.UriFactory.Create("https://example.org/p"));
        INode o = new UriNode(store.UriFactory.Create("https://example.org/o"));
        INode o2= new UriNode(store.UriFactory.Create("https://example.org/o2"));
        IRefNode g1 = new UriNode(store.UriFactory.Create("https://example.org/g1"));
        var g = new Graph(g1);
        g.Assert(new Triple(s, p, o));
        store.Add(g);

        // Retracting a non-existent quad is a no-op
        store.Retract(new Quad(s, p, o2, g1));
        Assert.Single(store.Graphs[g1].Triples);
        Assert.Single(store.Graphs);
        
        // Retracting an existing quad changes the graph
        store.Retract(new Quad(s, p, o, g1));
        Assert.Single(store.Graphs);
        Assert.Empty(store.Graphs[g1].Triples);
        Assert.True(store.Graphs[g1].IsEmpty);
    }
    
    [Fact]
    public void RetractQuadsFromExistingUnnamedGraph()
    {
        ITripleStore store = GetInstance();
        INode s = new UriNode(store.UriFactory.Create("https://example.org/s"));
        INode p = new UriNode(store.UriFactory.Create("https://example.org/p"));
        INode o = new UriNode(store.UriFactory.Create("https://example.org/o"));
        INode o2= new UriNode(store.UriFactory.Create("https://example.org/o2"));
        IRefNode? unnamed = null;
        var g = new Graph();
        g.Assert(new Triple(s, p, o));
        store.Add(g);

        // Retracting a non-existent quad is a no-op
        store.Retract(new Quad(s, p, o2, unnamed));
        Assert.Single(store.Graphs[unnamed].Triples);
        Assert.Single(store.Graphs);
        
        // Retracting an existing quad changes the graph
        store.Retract(new Quad(s, p, o, unnamed));
        Assert.Single(store.Graphs);
        Assert.Empty(store.Graphs[unnamed].Triples);
        Assert.True(store.Graphs[unnamed].IsEmpty);
    }

    [Fact]
    public void RetractQuadsFromNonExistentNamedGraph()
    {
        ITripleStore store = GetInstance();
        INode s = new UriNode(store.UriFactory.Create("https://example.org/s"));
        INode p = new UriNode(store.UriFactory.Create("https://example.org/p"));
        INode o = new UriNode(store.UriFactory.Create("https://example.org/o"));
        IRefNode g1 = new UriNode(store.UriFactory.Create("https://example.org/g1"));
        IRefNode g2 = new UriNode(store.UriFactory.Create("https://example.org/g2"));
        var g = new Graph(g1);
        g.Assert(new Triple(s,p,o));
        store.Add(g);
        store.Retract(new Quad(s, p, o, g2));
        // Existing graph should be unmodified
        Assert.Single(store.Graphs);
        Assert.Single(store.Graphs[g1].Triples);
        // Non-existent graph should not have been created
        Assert.False(store.HasGraph(g2));
    }
    
    [Fact]
    public void RetractQuadsFromNonExistentUnnamedGraph()
    {
        ITripleStore store = GetInstance();
        INode s = new UriNode(store.UriFactory.Create("https://example.org/s"));
        INode p = new UriNode(store.UriFactory.Create("https://example.org/p"));
        INode o = new UriNode(store.UriFactory.Create("https://example.org/o"));
        IRefNode g1 = new UriNode(store.UriFactory.Create("https://example.org/g1"));
        IRefNode? unnamed = null;
        var g = new Graph(g1);
        g.Assert(new Triple(s,p,o));
        store.Add(g);
        store.Retract(new Quad(s, p, o, unnamed));
        // Existing graph should be unmodified
        Assert.Single(store.Graphs);
        Assert.Single(store.Graphs[g1].Triples);
        // Non-existent graph should not have been created
        Assert.False(store.HasGraph(unnamed));
    }

    [Fact]
    public void MatchQuadsInNamedGraph()
    {
        ITripleStore store = GetInstance();
        INode s = new UriNode(store.UriFactory.Create("https://example.org/s"));
        INode p = new UriNode(store.UriFactory.Create("https://example.org/p"));
        INode o = new UriNode(store.UriFactory.Create("https://example.org/o"));
        IRefNode g = new UriNode(store.UriFactory.Create("https://example.org/g1"));
        IRefNode g2 = new UriNode(store.UriFactory.Create("https://example.org/g2"));
        store.Assert(new Quad(s, p, o, g));
        store.Assert(new Quad(s, p, o, g2));
        Assert.Single(store.GetQuads(s: s, g: g));
        Assert.Single(store.GetQuads(p: p, g: g));
        Assert.Single(store.GetQuads(o: o, g: g));
        Assert.Single(store.GetQuads(g: g));
        Assert.Single(store.GetQuads(s, p, g:g));
        Assert.Single(store.GetQuads(s, o: o, g:g));
        Assert.Single(store.GetQuads(p: p, o: o, g: g));
        Assert.Single(store.GetQuads(s, p, o, g));
    }

    [Fact]
    public void MatchQuadsInUnnamedGraph()
    {
        ITripleStore store = GetInstance();
        INode s = new UriNode(store.UriFactory.Create("https://example.org/s"));
        INode p = new UriNode(store.UriFactory.Create("https://example.org/p"));
        INode o = new UriNode(store.UriFactory.Create("https://example.org/o"));
        IRefNode? unnamed = null;
        IRefNode g = new UriNode(store.UriFactory.Create("https://example.org/g1"));
        store.Assert(new Quad(s, p, o, unnamed));
        store.Assert(new Quad(s, p, o, g));
        Assert.Single(store.GetQuads(s: s, allGraphs:false));
        Assert.Single(store.GetQuads(p: p, allGraphs:false));
        Assert.Single(store.GetQuads(o: o, allGraphs:false));
        Assert.Single(store.GetQuads(allGraphs:false));
        Assert.Single(store.GetQuads(s, p, allGraphs:false));
        Assert.Single(store.GetQuads(s, o: o, allGraphs:false));
        Assert.Single(store.GetQuads(p: p, o: o, allGraphs:false));
        Assert.Single(store.GetQuads(s, p, o, unnamed, allGraphs:false));
    }

    [Fact]
    public void MatchQuadsInAllGraphs()
    {
        ITripleStore store = GetInstance();
        INode s = new UriNode(store.UriFactory.Create("https://example.org/s"));
        INode p = new UriNode(store.UriFactory.Create("https://example.org/p"));
        INode o = new UriNode(store.UriFactory.Create("https://example.org/o"));
        IRefNode g = new UriNode(store.UriFactory.Create("https://example.org/g1"));
        IRefNode g2 = new UriNode(store.UriFactory.Create("https://example.org/g2"));
        IRefNode? unnamed = null; 
        store.Assert(new Quad(s, p, o, g));
        store.Assert(new Quad(s, p, o, g2));
        store.Assert(new Quad(s, p, o, unnamed));
        Assert.Equal(3, store.GetQuads(s: s).Count());
        Assert.Equal(3,store.GetQuads(p: p).Count());
        Assert.Equal(3,store.GetQuads(o: o).Count());
        Assert.Equal(3,store.GetQuads().Count());
        Assert.Equal(3,store.GetQuads(s, p).Count());
        Assert.Equal(3,store.GetQuads(s, o: o).Count());
        Assert.Equal(3,store.GetQuads(p: p, o: o).Count());
        Assert.Equal(3,store.GetQuads(s, p, o).Count());
    }

}


public class TripleStoreTests
    : AbstractTripleStoreTests
{
    protected override ITripleStore GetInstance()
    {
        return new TripleStore();
    }
}


public class ThreadSafeTripleStoreTests
    : AbstractTripleStoreTests
{
    protected override ITripleStore GetInstance()
    {
        return new TripleStore(new ThreadSafeGraphCollection());
    }
}

public class WebDemandTripleStoreTests
    : AbstractTripleStoreTests
{
    protected override ITripleStore GetInstance()
    {
        return new WebDemandTripleStore();
    }
}

public class DiskDemandTripleStoreTests
    : AbstractTripleStoreTests
{
    protected override ITripleStore GetInstance()
    {
        return new DiskDemandTripleStore();
    }
}
