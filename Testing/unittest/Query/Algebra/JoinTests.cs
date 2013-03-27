using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VDS.RDF.Query.Algebra
{
    [TestClass]
    public class JoinTests
    {
        private NodeFactory _factory = new NodeFactory();

        [TestMethod]
        public void SparqlAlgebraJoinSingleVariable1()
        {
            ISet x = new Set();
            x.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));

            BaseMultiset lhs = new Multiset();
            lhs.Add(x);
            BaseMultiset rhs = new Multiset();
            rhs.Add(x);

            BaseMultiset joined = lhs.Join(rhs);

            Assert.AreEqual(1, joined.Count);
        }

        [TestMethod]
        public void SparqlAlgebraJoinSingleVariable2()
        {
            ISet x = new Set();
            x.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));
            ISet y1 = new Set();
            y1.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));
            ISet y2 = new Set();
            y2.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));

            BaseMultiset lhs = new Multiset();
            lhs.Add(x);
            BaseMultiset rhs = new Multiset();
            rhs.Add(y1);
            rhs.Add(y2);

            BaseMultiset joined = lhs.Join(rhs);

            Assert.AreEqual(2, joined.Count);
        }

        [TestMethod]
        public void SparqlAlgebraJoinMultiVariable1()
        {
            ISet x = new Set();
            x.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));
            x.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y")));

            ISet y1 = new Set();
            y1.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));
            y1.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y")));
            ISet y2 = new Set();
            y2.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));
            y2.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y")));

            BaseMultiset lhs = new Multiset();
            lhs.Add(x);
            BaseMultiset rhs = new Multiset();
            rhs.Add(y1);
            rhs.Add(y2);

            BaseMultiset joined = lhs.Join(rhs);

            Assert.AreEqual(2, joined.Count);
        }

        [TestMethod]
        public void SparqlAlgebraJoinMultiVariable2()
        {
            ISet x1 = new Set();
            x1.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));
            x1.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y1")));
            ISet x2 = new Set();
            x2.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));
            x2.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y2")));

            ISet y1 = new Set();
            y1.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));
            y1.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y1")));
            ISet y2 = new Set();
            y2.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));
            y2.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y2")));

            BaseMultiset lhs = new Multiset();
            lhs.Add(x1);
            lhs.Add(x2);
            BaseMultiset rhs = new Multiset();
            rhs.Add(y1);
            rhs.Add(y2);

            BaseMultiset joined = lhs.Join(rhs);

            Assert.AreEqual(2, joined.Count);
        }

        [TestMethod]
        public void SparqlAlgebraJoinMultiVariable3()
        {
            ISet x1 = new Set();
            x1.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x1")));
            x1.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y1")));
            ISet x2 = new Set();
            x2.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x2")));
            x2.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y2")));

            ISet y1 = new Set();
            y1.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x1")));
            y1.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y1")));
            ISet y2 = new Set();
            y2.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x2")));
            y2.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y2")));

            BaseMultiset lhs = new Multiset();
            lhs.Add(x1);
            lhs.Add(x2);
            BaseMultiset rhs = new Multiset();
            rhs.Add(y1);
            rhs.Add(y2);

            BaseMultiset joined = lhs.Join(rhs);

            Assert.AreEqual(2, joined.Count);
        }

        [TestMethod]
        public void SparqlAlgebraJoinMultiVariable4()
        {
            ISet x1 = new Set();
            x1.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x1")));
            x1.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y1")));
            ISet x2 = new Set();
            x2.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x2")));
            x2.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y2")));

            ISet y1 = new Set();
            y1.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x1")));
            y1.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y2")));
            ISet y2 = new Set();
            y2.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x2")));
            y2.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y1")));

            BaseMultiset lhs = new Multiset();
            lhs.Add(x1);
            lhs.Add(x2);
            BaseMultiset rhs = new Multiset();
            rhs.Add(y1);
            rhs.Add(y2);

            BaseMultiset joined = lhs.Join(rhs);

            Assert.AreEqual(0, joined.Count);
        }
    }
}
