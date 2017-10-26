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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace VDS.RDF
{

    public class ListTests
    {
        private INode TestListsBasic(IGraph g)
        {
            List<INode> items = Enumerable.Range(1, 10).Select(i => i.ToLiteral(g)).OfType<INode>().ToList();
            INode listRoot = g.AssertList(items);

            TestTools.ShowGraph(g);

            Assert.Equal(items.Count * 2, g.Triples.Count);
            List<INode> listItems = g.GetListItems(listRoot).ToList();
            Assert.Equal(items.Count, listItems.Count);

            for (int i = 0; i < items.Count; i++)
            {
                Assert.Equal(items[i], listItems[i]);
            }

            Assert.True(listRoot.IsListRoot(g), "Should be considered a list root");

            List<INode> listNodes = g.GetListNodes(listRoot).Skip(1).ToList();
            foreach (INode n in listNodes)
            {
                Assert.False(n.IsListRoot(g), "Should not be considered a list root");
            }

            return listRoot;
        }

        [Fact]
        public void GraphLists1()
        {
            Graph g = new Graph();
            INode listRoot = this.TestListsBasic(g);

            g.RetractList(listRoot);
            Assert.Equal(0, g.Triples.Count);
        }

        [Fact]
        public void GraphLists2()
        {
            Graph g = new Graph();
            INode listRoot = this.TestListsBasic(g);

            //Try extending the list
            List<INode> items = Enumerable.Range(11, 10).Select(i => i.ToLiteral(g)).OfType<INode>().ToList();
            g.AddToList(listRoot, items);
            TestTools.ShowGraph(g);

            Assert.Equal(items.Count * 4, g.Triples.Count);
            List<INode> listItems = g.GetListItems(listRoot).ToList();
            Assert.Equal(items.Count * 2, listItems.Count);

            for (int i = 0; i < items.Count; i++)
            {
                Assert.Equal(items[i], listItems[i + 10]);
            }

            g.RetractList(listRoot);
            Assert.Equal(0, g.Triples.Count);
        }

        [Fact]
        public void GraphLists3()
        {
            Graph g = new Graph();
            INode listRoot = this.TestListsBasic(g);

            //Try removing items from the list
            List<INode> items = Enumerable.Range(1, 10).Where(i => i % 2 == 0).Select(i => i.ToLiteral(g)).OfType<INode>().ToList();
            g.RemoveFromList(listRoot, items);
            TestTools.ShowGraph(g);

            Assert.Equal(items.Count * 2, g.Triples.Count);
            List<INode> listItems = g.GetListItems(listRoot).ToList();
            Assert.Equal(items.Count * 2, listItems.Count * 2);

            for (int i = 0; i < items.Count; i++)
            {
                Assert.False(listItems.Contains(items[i]), "Item " + items[i].ToString() + " which should have been removed from the list is still present");
            }

            g.RetractList(listRoot);
            Assert.Equal(0, g.Triples.Count);
        }

        [Fact]
        public void GraphLists4()
        {
            Graph g = new Graph();
            g.AddToList(g.CreateBlankNode(), Enumerable.Empty<INode>());
        }

        [Fact]
        public void GraphLists5()
        {
            Graph g = new Graph();
            g.AddToList(g.CreateBlankNode(), Enumerable.Empty<INode>());
        }

        [Fact]
        public void GraphListsError1()
        {
            Graph g = new Graph();

            Assert.Throws<RdfException>(() => g.GetListItems(g.CreateBlankNode()));
        }

        [Fact]
        public void GraphListsError2()
        {
            Graph g = new Graph();

            Assert.Throws<RdfException>(() => g.GetListAsTriples(g.CreateBlankNode()));
        }

        [Fact]
        public void GraphListsError3()
        {
            Graph g = new Graph();

            Assert.Throws<RdfException>(() => g.RetractList(g.CreateBlankNode()));
        }

        [Fact]
        public void GraphListsError4()
        {
            Graph g = new Graph();

            Assert.Throws<RdfException>(() => g.AddToList<int>(g.CreateBlankNode(), Enumerable.Range(1, 10), i => i.ToLiteral(g)));
        }

        [Fact]
        public void GraphListsError5()
        {
            Graph g = new Graph();

            Assert.Throws<RdfException>(() => g.RemoveFromList<int>(g.CreateBlankNode(), Enumerable.Range(1, 10), i => i.ToLiteral(g)));
        }
    }
}
