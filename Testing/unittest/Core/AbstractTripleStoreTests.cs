using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VDS.RDF.Core
{
    /// <summary>
    /// Abstract set of Triple Stores tests which can be used to test any <see cref="ITripleStore"/> implementation
    /// </summary>
    [TestClass]
    public abstract class AbstractTripleStoreTests
    {
        /// <summary>
        /// Method which derived tests should implement to provide a fresh instance that can be used for testing
        /// </summary>
        /// <returns></returns>
        protected abstract ITripleStore GetInstance();

        [TestMethod]
        public void TripleStoreIsEmpty01()
        {
            ITripleStore store = this.GetInstance();

            Assert.IsTrue(store.IsEmpty);
        }

        [TestMethod]
        public void TripleStoreIsEmpty02()
        {
            ITripleStore store = this.GetInstance();
            store.Add(new Graph());

            Assert.IsFalse(store.IsEmpty);
        }

        [TestMethod]
        public void TripleStoreAdd01()
        {
            ITripleStore store = this.GetInstance();

            Graph g = new Graph();
            store.Add(g);

            Assert.IsFalse(store.IsEmpty);
            Assert.IsTrue(store.HasGraph(g.BaseUri));
        }

        [TestMethod]
        public void TripleStoreAdd02()
        {
            ITripleStore store = this.GetInstance();

            IGraph g = new Graph();
            g.BaseUri = new Uri("http://example.org/graph");
            store.Add(g);

            Assert.IsFalse(store.IsEmpty);
            Assert.IsTrue(store.HasGraph(g.BaseUri));
        }

        [TestMethod]
        public void TripleStoreHasGraph01()
        {
            ITripleStore store = this.GetInstance();

            Assert.IsFalse(store.HasGraph(new Uri("http://thereisnosuchdomain.com/graph")));
        }

        [TestMethod]
        public void TripleStoreHasGraph02()
        {
            ITripleStore store = this.GetInstance();

            IGraph g = new Graph();
            store.Add(g);

            Assert.IsTrue(store.HasGraph(null));
        }

        [TestMethod]
        public void TripleStoreHasGraph03()
        {
            ITripleStore store = this.GetInstance();

            IGraph g = new Graph();
            g.BaseUri = new Uri("http://nosuchdomain.com/graph");
            store.Add(g);

            Assert.IsTrue(store.HasGraph(g.BaseUri));
        }
    }

    [TestClass]
    public class TripleStoreTests
        : AbstractTripleStoreTests
    {
        protected override ITripleStore GetInstance()
        {
            return new TripleStore();
        }
    }

    [TestClass]
    public class ThreadSafeTripleStoreTests
        : AbstractTripleStoreTests
    {
        protected override ITripleStore GetInstance()
        {
            return new TripleStore(new ThreadSafeGraphCollection());
        }
    }

    [TestClass]
    public class WebDemandTripleStoreTests
        : AbstractTripleStoreTests
    {
        protected override ITripleStore GetInstance()
        {
            return new WebDemandTripleStore();
        }
    }

    [TestClass]
    public class DiskDemandTripleStoreTests
        : AbstractTripleStoreTests
    {
        protected override ITripleStore GetInstance()
        {
            return new DiskDemandTripleStore();
        }
    }
}
