using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class SetTests
    {
        private NodeFactory _factory = new NodeFactory();

        [TestMethod]
        public void SparqlSetHashCodes1()
        {
            INode a = this._factory.CreateLiteralNode("a");
            INode b = this._factory.CreateLiteralNode("b");

            Set x = new Set();
            x.Add("a", a);
            x.Add("b", b);
            Console.WriteLine(x.ToString());

            Set y = new Set();
            y.Add("b", b);
            y.Add("a", a);
            Console.WriteLine(y.ToString());

            Assert.AreEqual(x, y);
            Assert.AreEqual(x.GetHashCode(), y.GetHashCode());
        }

        [TestMethod]
        public void SparqlSetHashCodes2()
        {
            INode a = this._factory.CreateLiteralNode("a");
            INode b = this._factory.CreateLiteralNode("b");

            Set x = new Set();
            x.Add("a", a);
            Console.WriteLine(x.ToString());

            Set y = new Set();
            y.Add("b", b);
            Console.WriteLine(y.ToString());

            Assert.AreNotEqual(x, y);
            Assert.AreNotEqual(x.GetHashCode(), y.GetHashCode());

            ISet z1 = x.Join(y);
            ISet z2 = y.Join(x);

            Assert.AreEqual(z1, z2);
            Assert.AreEqual(z1.GetHashCode(), z2.GetHashCode());
        }

        [TestMethod]
        public void SparqlSetDistinct1()
        {
            INode a = this._factory.CreateBlankNode();
            INode b1 = (1).ToLiteral(this._factory);
            INode b2 = (2).ToLiteral(this._factory);

            Set x = new Set();
            x.Add("a", a);
            x.Add("_:b", b1);

            Set y = new Set();
            y.Add("a", a);
            y.Add("_:b", b2);

            Assert.AreNotEqual(x, y);

            Multiset data = new Multiset();
            data.Add(x);
            data.Add(y);
            Assert.AreEqual(2, data.Count);

            Table table = new Table(data);
            Distinct distinct = new Distinct(table);

            //Distinct should yield a single result since temporary variables
            //are stripped
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, null);
            BaseMultiset results = distinct.Evaluate(context);
            Assert.AreEqual(1, results.Count);
            Assert.IsFalse(results.ContainsVariable("_:b"));
        }

        [TestMethod]
        public void SparqlSetDistinct2()
        {
            INode a = this._factory.CreateBlankNode();
            INode b1 = (1).ToLiteral(this._factory);
            INode b2 = (2).ToLiteral(this._factory);

            Set x = new Set();
            x.Add("a", a);
            x.Add("_:b", b1);

            Set y = new Set();
            y.Add("a", a);
            y.Add("_:b", b2);

            Assert.AreNotEqual(x, y);

            Multiset data = new Multiset();
            data.Add(x);
            data.Add(y);
            Assert.AreEqual(2, data.Count);

            Table table = new Table(data);
            Distinct distinct = new Distinct(table, true);

            //Distinct should yield two result and temporary variables should still
            //be present
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, null);
            BaseMultiset results = distinct.Evaluate(context);
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.ContainsVariable("_:b"));
        }
    }
}
