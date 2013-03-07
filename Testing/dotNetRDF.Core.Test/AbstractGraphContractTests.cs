using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract tests for <see cref="IGraph"/> implementations
    /// </summary>
    [TestClass]
    public abstract class AbstractGraphContractTests
    {
        /// <summary>
        /// Gets a new fresh instance of a graph for testing
        /// </summary>
        /// <returns></returns>
        protected abstract IGraph GetInstance();

        protected IEnumerable<Triple> GenerateTriples(int n)
        {
            for (int i = 0; i < n; i++)
            {
                yield return new Triple(new UriNode(new Uri("http://test/" + i)), new UriNode(new Uri("http://predicate")), new LiteralNode(i.ToString()));
            }
        }

        [TestMethod]
        public void GraphContractCount1()
        {
            IGraph g = this.GetInstance();
            Assert.AreEqual(0, g.Count);
        }

        [TestMethod]
        public void GraphContractCount2()
        {
            IGraph g = this.GetInstance();
            g.Assert(this.GenerateTriples(1));
            Assert.AreEqual(1, g.Count);
        }

        [TestMethod]
        public void GraphContractCount3()
        {
            IGraph g = this.GetInstance();
            g.Assert(this.GenerateTriples(100));
            Assert.AreEqual(100, g.Count);
        }

        [TestMethod]
        public void GraphContractIsEmpty1()
        {
            IGraph g = this.GetInstance();
            Assert.IsTrue(g.IsEmpty);
        }

        [TestMethod]
        public void GraphContractIsEmpty2()
        {
            IGraph g = this.GetInstance();
            g.Assert(this.GenerateTriples(1));
            Assert.IsFalse(g.IsEmpty);
        }

    }

    [TestClass]
    public class GraphContractTests
        : AbstractGraphContractTests
    {
        protected override IGraph GetInstance()
        {
            return new Graph();
        }
    }
}
