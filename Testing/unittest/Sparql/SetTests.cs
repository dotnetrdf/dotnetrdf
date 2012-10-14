using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Algebra;

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
    }
}
