using System.Collections.Generic;
using Xunit;

namespace VDS.RDF;

public class QuadTests
{
    [Fact]
    public void ConstructQuadFromFourNodes()
    {
        var nf = new NodeFactory();
        INode s = nf.CreateUriNode(UriFactory.Create("http://example.org/s"));
        INode p = nf.CreateUriNode(UriFactory.Create("http://example.org/p"));
        INode o = nf.CreateUriNode(UriFactory.Create("http://example.org/o"));
        IRefNode g = nf.CreateUriNode(UriFactory.Create("http://example.org/g"));
        var q = new Quad(s, p, o, g);
        Assert.Equal("http://example.org/s, http://example.org/p, http://example.org/o, http://example.org/g", q.ToString());
    }
    
    [Fact]
    public void ConstructQuadFromTripleAndGraph()
    {
        var nf = new NodeFactory();
        INode s = nf.CreateUriNode(UriFactory.Create("http://example.org/s"));
        INode p = nf.CreateUriNode(UriFactory.Create("http://example.org/p"));
        INode o = nf.CreateUriNode(UriFactory.Create("http://example.org/o"));
        var t = new Triple(s, p, o);
        IRefNode g = nf.CreateUriNode(UriFactory.Create("http://example.org/g"));
        var q = new Quad(t, g);
        Assert.Equal("http://example.org/s, http://example.org/p, http://example.org/o, http://example.org/g", q.ToString());
    }

    [Fact]
    public void QuadsEquality()
    {
        var nf = new NodeFactory();
        INode s = nf.CreateUriNode(UriFactory.Create("http://example.org/s"));
        INode p = nf.CreateUriNode(UriFactory.Create("http://example.org/p"));
        INode o = nf.CreateUriNode(UriFactory.Create("http://example.org/o"));
        var t = new Triple(s, p, o);
        IRefNode g = nf.CreateUriNode(UriFactory.Create("http://example.org/g"));
        var q1 = new Quad(t, g);
        var q2 = new Quad(t.Subject, t.Predicate, t.Object, g);
        var nf2 = new NodeFactory();
        INode s2 = nf2.CreateUriNode(UriFactory.Create("http://example.org/s"));
        INode p2 = nf2.CreateUriNode(UriFactory.Create("http://example.org/p"));
        INode o2 = nf2.CreateUriNode(UriFactory.Create("http://example.org/o"));
        IRefNode g2 = nf2.CreateUriNode(UriFactory.Create("http://example.org/g"));
        var q3 = new Quad(s2, p2, o2, g2);
        Assert.True(q1.Equals(q2));
        Assert.True(q1.Equals(q3));
        Assert.True(q2.Equals(q3));
    }

    [Fact]
    public void QuadsComparability()
    {
        var nf = new NodeFactory();
        INode s = nf.CreateUriNode(UriFactory.Create("http://example.org/s"));
        INode p = nf.CreateUriNode(UriFactory.Create("http://example.org/p"));
        INode o = nf.CreateLiteralNode("1");
        IRefNode g = nf.CreateUriNode(UriFactory.Create("http://example.org/g"));   
        INode s2 = nf.CreateUriNode(UriFactory.Create("http://example.org/s2"));
        INode p2 = nf.CreateUriNode(UriFactory.Create("http://example.org/p2"));
        INode o2 = nf.CreateLiteralNode("2");
        IRefNode g2 = nf.CreateUriNode(UriFactory.Create("http://example.org/g2"));
        var q1 = new Quad(s, p, o, g);
        var q2 = new Quad(s2, p, o, g);
        var q3 = new Quad(s, p2, o, g);
        var q4 = new Quad(s, p, o2, g);
        var q5 = new Quad(s, p, o, g2);
        var l = new List<Quad>
        {
            q1,
            q2,
            q3,
            q4,
            q5
        };
        l.Sort();
        var expectSort = new List<Quad>
        {
            q1,
            q5,
            q4,
            q3,
            q2
        };
        Assert.Equal(expectSort, l);
    }
}