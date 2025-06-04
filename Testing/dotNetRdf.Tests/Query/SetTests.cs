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
using Xunit;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query;


public class SetTests
{
    private NodeFactory _factory = new NodeFactory();

    [Fact]
    public void SparqlSetHashCodes1()
    {
        INode a = _factory.CreateLiteralNode("a");
        INode b = _factory.CreateLiteralNode("b");

        var x = new Set();
        x.Add("a", a);
        x.Add("b", b);
        Console.WriteLine(x.ToString());

        var y = new Set();
        y.Add("b", b);
        y.Add("a", a);
        Console.WriteLine(y.ToString());

        Assert.Equal(x, y);
        Assert.Equal(x.GetHashCode(), y.GetHashCode());
    }

    [Fact]
    public void SparqlSetHashCodes2()
    {
        INode a = _factory.CreateLiteralNode("a");
        INode b = _factory.CreateLiteralNode("b");

        var x = new Set();
        x.Add("a", a);
        Console.WriteLine(x.ToString());

        var y = new Set();
        y.Add("b", b);
        Console.WriteLine(y.ToString());

        Assert.NotEqual(x, y);
        Assert.NotEqual(x.GetHashCode(), y.GetHashCode());

        ISet z1 = x.Join(y);
        ISet z2 = y.Join(x);

        Assert.Equal(z1, z2);
        Assert.Equal(z1.GetHashCode(), z2.GetHashCode());
    }

    [Fact]
    public void SparqlSetDistinct1()
    {
        INode a = _factory.CreateBlankNode();
        INode b1 = (1).ToLiteral(_factory);
        INode b2 = (2).ToLiteral(_factory);

        var x = new Set();
        x.Add("a", a);
        x.Add("_:b", b1);

        var y = new Set();
        y.Add("a", a);
        y.Add("_:b", b2);

        Assert.NotEqual(x, y);

        var data = new Multiset();
        data.Add(x);
        data.Add(y);
        Assert.Equal(2, data.Count);

        var table = new Table(data);
        var distinct = new Distinct(table);

        //Distinct should yield a single result since temporary variables
        //are stripped
        var context = new SparqlEvaluationContext(null, new LeviathanQueryOptions());
        var processor = new LeviathanQueryProcessor(new InMemoryDataset());
        BaseMultiset results = distinct.Accept(processor, context);
        Assert.Equal(1, results.Count);
        Assert.False(results.ContainsVariable("_:b"));
    }

    [Fact]
    public void SparqlSetDistinct2()
    {
        INode a = _factory.CreateBlankNode();
        INode b1 = (1).ToLiteral(_factory);
        INode b2 = (2).ToLiteral(_factory);

        var x = new Set();
        x.Add("a", a);
        x.Add("_:b", b1);

        var y = new Set();
        y.Add("a", a);
        y.Add("_:b", b2);

        Assert.NotEqual(x, y);

        var data = new Multiset();
        data.Add(x);
        data.Add(y);
        Assert.Equal(2, data.Count);

        var table = new Table(data);
        var distinct = new Distinct(table, true);

        //Distinct should yield two result and temporary variables should still
        //be present
        var context = new SparqlEvaluationContext(null, new LeviathanQueryOptions());
        var processor = new LeviathanQueryProcessor(new InMemoryDataset());
        BaseMultiset results = distinct.Accept(processor, context);
        Assert.Equal(2, results.Count);
        Assert.True(results.ContainsVariable("_:b"));
    }

    [Fact]
    public void SparqlSetDistinctnessComparer1()
    {
        ISet x = new Set();
        x.Add("a", _factory.CreateUriNode(UriFactory.Root.Create("http://x")));
        ISet y = new Set();
        y.Add("a", _factory.CreateUriNode(UriFactory.Root.Create("http://x")));


        var comparer = new SetDistinctnessComparer(new String[] { "a" });
        var xHash = comparer.GetHashCode(x);
        var yHash = comparer.GetHashCode(y);

        Assert.Equal(xHash, yHash);
        Assert.True(comparer.Equals(x, y));
    }

    [Fact]
    public void SparqlSetDistinctnessComparer2()
    {
        ISet x = new Set();
        x.Add("a", _factory.CreateUriNode(UriFactory.Root.Create("http://x")));
        x.Add("b", _factory.CreateLiteralNode("x"));
        ISet y = new Set();
        y.Add("a", _factory.CreateUriNode(UriFactory.Root.Create("http://x")));
        y.Add("b", _factory.CreateLiteralNode("y"));
        
        var comparer = new SetDistinctnessComparer(new String[] { "a" });
        var xHash = comparer.GetHashCode(x);
        var yHash = comparer.GetHashCode(y);

        Assert.Equal(xHash, yHash);
        Assert.True(comparer.Equals(x, y));
    }

    [Fact]
    public void SparqlSetDistinctnessComparer3()
    {
        ISet x = new Set();
        x.Add("a", _factory.CreateUriNode(UriFactory.Root.Create("http://x")));
        x.Add("b", _factory.CreateLiteralNode("x"));
        ISet y = new Set();
        y.Add("a", _factory.CreateUriNode(UriFactory.Root.Create("http://x")));
        y.Add("b", _factory.CreateLiteralNode("y"));

        var comparer = new SetDistinctnessComparer(new String[] { "a", "b" });
        var xHash = comparer.GetHashCode(x);
        var yHash = comparer.GetHashCode(y);

        Assert.NotEqual(xHash, yHash);
        Assert.False(comparer.Equals(x, y));
    }

    [Fact]
    public void SparqlSetDistinctnessComparer4()
    {
        ISet x = new Set();
        ISet y = new Set();
        y.Add("a", _factory.CreateUriNode(UriFactory.Root.Create("http://x")));

        var comparer = new SetDistinctnessComparer(new String[] { "a" });
        var xHash = comparer.GetHashCode(x);
        var yHash = comparer.GetHashCode(y);

        Assert.NotEqual(xHash, yHash);
        Assert.False(comparer.Equals(x, y));
    }
}
