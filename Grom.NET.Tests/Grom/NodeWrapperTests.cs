namespace Grom.Tests
{
    using Grom;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq.Expressions;
    using VDS.RDF;

    [TestClass]
    public class NodeWrapperTests
    {
        [TestMethod]
        [DynamicData(nameof(Data.ComparisomParameters), typeof(Data))]
        public void Compares_correctly(NodeWrapper left, ExpressionType binaryOperator, NodeWrapper right, string methodName)
        {
            var leftOperand = Expression.Constant(left, typeof(NodeWrapper));
            var rightOperand = Expression.Constant(right, typeof(NodeWrapper));

            var assertMethod = typeof(Assert).GetMethod(methodName, new[] { typeof(bool) });

            var condition = Expression.MakeBinary(binaryOperator, leftOperand, rightOperand);
            var assertion = Expression.Call(assertMethod, condition);
            var lambdaExpression = Expression.Lambda<Action>(assertion);

            var assertAction = lambdaExpression.Compile();

            assertAction.Invoke();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_fails_null_node()
        {
            new NodeWrapper(null);
        }

        [TestMethod]
        public void Equals_false_node_and_other()
        {
            var wrapper = new NodeWrapper(
                new NodeFactory().CreateBlankNode());

            Assert.AreNotEqual(
                wrapper,
                new object());
        }

        [TestMethod]
        public void GetHashCode_salts_graphNode()
        {
            var node = new NodeFactory().CreateBlankNode();

            var wrapper = new NodeWrapper(node);

            Assert.AreEqual(
                string.Concat(nameof(NodeWrapper), node.ToString()).GetHashCode(),
                wrapper.GetHashCode());
        }

        [TestMethod]
        public void ToString_delegates_to_graphNode()
        {
            var node = new NodeFactory().CreateBlankNode();

            var wrapper = new NodeWrapper(node);

            Assert.AreEqual(
                node.ToString(), 
                wrapper.ToString());
        }
    }
}
