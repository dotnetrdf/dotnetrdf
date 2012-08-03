using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Nodes;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Core
{
    [TestClass]
    public class ValuedNodeTests
    {
        [TestMethod]
        public void NodeAsValuedTimeSpan()
        {
            Graph g = new Graph();
            INode orig = new TimeSpan(1, 0, 0).ToLiteral(g);
            IValuedNode valued = orig.AsValuedNode();

            Assert.AreEqual(((ILiteralNode)orig).Value, ((ILiteralNode)valued).Value);
            Assert.IsTrue(EqualityHelper.AreUrisEqual(((ILiteralNode)orig).DataType, ((ILiteralNode)valued).DataType));
            Assert.AreEqual(typeof(TimeSpanNode), valued.GetType());
        }
    }
}
