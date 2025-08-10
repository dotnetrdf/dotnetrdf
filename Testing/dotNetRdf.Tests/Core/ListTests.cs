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

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing;
using Xunit;

namespace VDS.RDF;


public class ListTests
{
    private INode TestListsBasic(IGraph g)
    {
        var items = Enumerable.Range(1, 10).Select(i => i.ToLiteral(g)).OfType<INode>().ToList();
        INode listRoot = g.AssertList(items);

        TestTools.ShowGraph(g);

        Assert.Equal(items.Count * 2, g.Triples.Count);
        var listItems = g.GetListItems(listRoot).ToList();
        Assert.Equal(items.Count, listItems.Count);

        for (var i = 0; i < items.Count; i++)
        {
            Assert.Equal(items[i], listItems[i]);
        }

        Assert.True(listRoot.IsListRoot(g), "Should be considered a list root");

        var listNodes = g.GetListNodes(listRoot).Skip(1).ToList();
        foreach (INode n in listNodes)
        {
            Assert.False(n.IsListRoot(g), "Should not be considered a list root");
        }

        return listRoot;
    }

    [Fact]
    public void GraphLists1()
    {
        var g = new Graph();
        INode listRoot = TestListsBasic(g);

        g.RetractList(listRoot);
        Assert.Equal(0, g.Triples.Count);
    }

    [Fact]
    public void GraphLists2()
    {
        var g = new Graph();
        INode listRoot = TestListsBasic(g);

        //Try extending the list
        var items = Enumerable.Range(11, 10).Select(i => i.ToLiteral(g)).OfType<INode>().ToList();
        g.AddToList(listRoot, items);
        TestTools.ShowGraph(g);

        Assert.Equal(items.Count * 4, g.Triples.Count);
        var listItems = g.GetListItems(listRoot).ToList();
        Assert.Equal(items.Count * 2, listItems.Count);

        for (var i = 0; i < items.Count; i++)
        {
            Assert.Equal(items[i], listItems[i + 10]);
        }

        g.RetractList(listRoot);
        Assert.Equal(0, g.Triples.Count);
    }

    [Fact]
    public void GraphLists3()
    {
        var g = new Graph();
        INode listRoot = TestListsBasic(g);

        //Try removing items from the list
        var items = Enumerable.Range(1, 10).Where(i => i % 2 == 0).Select(i => i.ToLiteral(g)).OfType<INode>().ToList();
        g.RemoveFromList(listRoot, items);
        TestTools.ShowGraph(g);

        Assert.Equal(items.Count * 2, g.Triples.Count);
        var listItems = g.GetListItems(listRoot).ToList();
        Assert.Equal(items.Count * 2, listItems.Count * 2);

        for (var i = 0; i < items.Count; i++)
        {
            Assert.False(listItems.Contains(items[i]), "Item " + items[i].ToString() + " which should have been removed from the list is still present");
        }

        g.RetractList(listRoot);
        Assert.Equal(0, g.Triples.Count);
    }

    [Fact]
    public void GraphLists4()
    {
        var g = new Graph();
        g.AddToList(g.CreateBlankNode(), Enumerable.Empty<INode>());
    }

    [Fact]
    public void RemoveFromListRemovesFirstOccurrenceOnly()
    {
        var g = new Graph();
        INode list = g.AssertList([0, 0, 1], x => x.ToLiteral(g));
        g.RemoveFromList(list, [0], x=>x.ToLiteral(g));
        g.GetListItems(list).Should().HaveCount(2);
    }

    [Fact]
    public void RemoveAllFromListRemovesAllOccurrences()
    {
        var g = new Graph();
        INode list = g.AssertList([0, 0, 1], x => x.ToLiteral(g));
        g.RemoveAllFromList(list, [0], x=>x.ToLiteral(g));
        g.GetListItems(list).Should().HaveCount(1);
    }

    [Fact]
    public void AnEmptyListCanBeAssertedAndRetrieved()
    {
        var g = new Graph();
        INode list = g.AssertList(new List<INode>());
        g.GetListItems(list).Should().BeEmpty();
    }

    [Fact]
    public void GraphListsError1()
    {
        var g = new Graph();

        Assert.Throws<RdfException>(() => g.GetListItems(g.CreateBlankNode()));
    }

    [Fact]
    public void GraphListsError2()
    {
        var g = new Graph();

        Assert.Throws<RdfException>(() => g.GetListAsTriples(g.CreateBlankNode()));
    }

    [Fact]
    public void GraphListsError3()
    {
        var g = new Graph();

        Assert.Throws<RdfException>(() => g.RetractList(g.CreateBlankNode()));
    }

    [Fact]
    public void GraphListsError4()
    {
        var g = new Graph();

        Assert.Throws<RdfException>(() => g.AddToList<int>(g.CreateBlankNode(), Enumerable.Range(1, 10), i => i.ToLiteral(g)));
    }

    [Fact]
    public void GraphListsError5()
    {
        var g = new Graph();

        Assert.Throws<RdfException>(() => g.RemoveFromList<int>(g.CreateBlankNode(), Enumerable.Range(1, 10), i => i.ToLiteral(g)));
    }
}
