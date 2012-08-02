using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Operators;
using VDS.RDF.Query.Operators.Numeric;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class OperatorTests
    {
        private List<IValuedNode> _numArgs;

        [TestInitialize]
        public void Setup()
        {
            this._numArgs = new List<IValuedNode>() 
            { 
                new LongNode(null, 12345), 
                new DecimalNode(null, 123.45m), 
                new FloatNode(null, 123.45f), 
                new DoubleNode(null, 123.45d) };
        }

        private void TestLookup(SparqlOperatorType opType, Type returnedOpInstanceType, IEnumerable<IValuedNode> ns, bool opExists)
        {
            ISparqlOperator op = null;
            if (SparqlOperators.TryGetOperator(opType, out op, ns.ToArray()))
            {
                if (!opExists) Assert.Fail("Operator returned when no operator was expected for the given inputs");
                Assert.AreEqual(returnedOpInstanceType, op.GetType());
            }
            else
            {
                if (opExists) Assert.Fail("No Operator returned when an operator was expected for the given inputs");
            }
        }

        [TestMethod]
        public void SparqlOperatorLookup1()
        {
            this.TestLookup(SparqlOperatorType.Add, null, Enumerable.Empty<IValuedNode>(), false);
            this.TestLookup(SparqlOperatorType.Add, typeof(AdditionOperator), this._numArgs, true);
        }
    }
}
