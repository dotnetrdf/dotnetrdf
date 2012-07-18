using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VDS.RDF.Test
{
    [TestClass]
    public class ListTests
    {
        private INode TestListsBasic(IGraph g)
        {
            List<INode> items = Enumerable.Range(1, 10).Select(i => i.ToLiteral(g)).OfType<INode>().ToList();
            INode listRoot = g.AssertList(items);

            TestTools.ShowGraph(g);

            Assert.AreEqual(items.Count * 2, g.Triples.Count, "Expected " + (items.Count * 2) + " Triples");
            List<INode> listItems = g.GetListItems(listRoot).ToList();
            Assert.AreEqual(items.Count, listItems.Count, "Expected " + items.Count + " Items in the List");

            for (int i = 0; i < items.Count; i++)
            {
                Assert.AreEqual(items[i], listItems[i], "Items were not in list in correct order");
            }

            Assert.IsTrue(listRoot.IsListRoot(g), "Should be considered a list root");

            List<INode> listNodes = g.GetListNodes(listRoot).Skip(1).ToList();
            foreach (INode n in listNodes)
            {
                Assert.IsFalse(n.IsListRoot(g), "Should not be considered a list root");
            }

            return listRoot;
        }

        [TestMethod]
        public void GraphLists1()
        {
            Graph g = new Graph();
            INode listRoot = this.TestListsBasic(g);

            g.RetractList(listRoot);
            Assert.AreEqual(0, g.Triples.Count, "Should be no triples after the list is retracted");
        }

        [TestMethod]
        public void GraphLists2()
        {
            Graph g = new Graph();
            INode listRoot = this.TestListsBasic(g);

            //Try extending the list
            List<INode> items = Enumerable.Range(11, 10).Select(i => i.ToLiteral(g)).OfType<INode>().ToList();
            g.AddToList(listRoot, items);
            TestTools.ShowGraph(g);

            Assert.AreEqual(items.Count * 4, g.Triples.Count, "Expected " + (items.Count * 4) + " Triples");
            List<INode> listItems = g.GetListItems(listRoot).ToList();
            Assert.AreEqual(items.Count * 2, listItems.Count, "Expected " + (items.Count * 2) + " Items in the List");

            for (int i = 0; i < items.Count; i++)
            {
                Assert.AreEqual(items[i], listItems[i + 10], "Items were not in list in correct order");
            }

            g.RetractList(listRoot);
            Assert.AreEqual(0, g.Triples.Count, "Should be no triples after the list is retracted");
        }

        [TestMethod]
        public void GraphLists3()
        {
            Graph g = new Graph();
            INode listRoot = this.TestListsBasic(g);

            //Try removing items from the list
            List<INode> items = Enumerable.Range(1, 10).Where(i => i % 2 == 0).Select(i => i.ToLiteral(g)).OfType<INode>().ToList();
            g.RemoveFromList(listRoot, items);
            TestTools.ShowGraph(g);

            Assert.AreEqual(items.Count * 2, g.Triples.Count, "Expected " + (items.Count * 2) + " Triples");
            List<INode> listItems = g.GetListItems(listRoot).ToList();
            Assert.AreEqual(items.Count * 2, listItems.Count * 2, "Expected " + (items.Count * 2) + " Items in the List");

            for (int i = 0; i < items.Count; i++)
            {
                Assert.IsFalse(listItems.Contains(items[i]), "Item " + items[i].ToString() + " which should have been removed from the list is still present");
            }

            g.RetractList(listRoot);
            Assert.AreEqual(0, g.Triples.Count, "Should be no triples after the list is retracted");
        }

        [TestMethod]
        public void GraphLists4()
        {
            Graph g = new Graph();
            g.AddToList(g.CreateBlankNode(), Enumerable.Empty<INode>());
        }

        [TestMethod]
        public void GraphLists5()
        {
            Graph g = new Graph();
            g.AddToList(g.CreateBlankNode(), Enumerable.Empty<INode>());
        }

        [TestMethod, ExpectedException(typeof(RdfException))]
        public void GraphListsError1()
        {
            Graph g = new Graph();
            g.GetListItems(g.CreateBlankNode());
        }

        [TestMethod, ExpectedException(typeof(RdfException))]
        public void GraphListsError2()
        {
            Graph g = new Graph();
            g.GetListAsTriples(g.CreateBlankNode());
        }

        [TestMethod, ExpectedException(typeof(RdfException))]
        public void GraphListsError3()
        {
            Graph g = new Graph();
            g.RetractList(g.CreateBlankNode());
        }

        [TestMethod, ExpectedException(typeof(RdfException))]
        public void GraphListsError4()
        {
            Graph g = new Graph();
            g.AddToList<int>(g.CreateBlankNode(), Enumerable.Range(1, 10), i => i.ToLiteral(g));
        }

        [TestMethod, ExpectedException(typeof(RdfException))]
        public void GraphListsError5()
        {
            Graph g = new Graph();
            g.RemoveFromList<int>(g.CreateBlankNode(), Enumerable.Range(1, 10), i => i.ToLiteral(g));
        }
    }
}
