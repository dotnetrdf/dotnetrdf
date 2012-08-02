using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Operators;
using VDS.RDF.Query.Operators.Numeric;
using VDS.RDF.Query.Operators.DateTime;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class OperatorTests
    {
        private List<IValuedNode> _numArgs, _someNullArgs, _dtArgs, _tsArgs;

        [TestInitialize]
        public void Setup()
        {
            this._numArgs = new List<IValuedNode>() 
            { 
                new LongNode(null, 12345), 
                new DecimalNode(null, 123.45m), 
                new FloatNode(null, 123.45f), 
                new DoubleNode(null, 123.45d)
            };
            this._someNullArgs = new List<IValuedNode>()
            {
                new LongNode(null, 12345),
                null,
                new BooleanNode(null, false),
                null
            };
            this._dtArgs = new List<IValuedNode>()
            {
                new DateTimeNode(null, DateTimeOffset.Now),
                new TimeSpanNode(null, new TimeSpan(0, 1, 30))
            };
            this._tsArgs = new List<IValuedNode>()
            {
                new TimeSpanNode(null, new TimeSpan(1, 0, 0)),
                new TimeSpanNode(null, new TimeSpan(0, 30, 0)),
                new TimeSpanNode(null, new TimeSpan(0, 0, 15))
            };
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

        private void TestStrictLookup(SparqlOperatorType opType, Type returnedOpInstanceType, IEnumerable<IValuedNode> ns, bool opExists)
        {
            try
            {
                Options.StrictOperators = true;
                this.TestLookup(opType, returnedOpInstanceType, ns, opExists);
            }
            finally
            {
                Options.StrictOperators = false;
            }
        }

        [TestMethod]
        public void SparqlOperatorLookup1()
        {
            this.TestLookup(SparqlOperatorType.Add, null, Enumerable.Empty<IValuedNode>(), false);
            this.TestLookup(SparqlOperatorType.Add, null, this._someNullArgs, false);
            this.TestLookup(SparqlOperatorType.Add, typeof(AdditionOperator), this._numArgs, true);
        }

        [TestMethod]
        public void SparqlOperatorLookup2()
        {
            this.TestLookup(SparqlOperatorType.Subtract, null, Enumerable.Empty<IValuedNode>(), false);
            this.TestLookup(SparqlOperatorType.Subtract, null, this._someNullArgs, false);
            this.TestLookup(SparqlOperatorType.Subtract, typeof(SubtractionOperator), this._numArgs, true);
        }

        [TestMethod]
        public void SparqlOperatorLookup3()
        {
            this.TestLookup(SparqlOperatorType.Divide, null, Enumerable.Empty<IValuedNode>(), false);
            this.TestLookup(SparqlOperatorType.Divide, null, this._someNullArgs, false);
            this.TestLookup(SparqlOperatorType.Divide, typeof(DivisionOperator), this._numArgs, true);
        }

        [TestMethod]
        public void SparqlOperatorLookup4()
        {
            this.TestLookup(SparqlOperatorType.Multiply, null, Enumerable.Empty<IValuedNode>(), false);
            this.TestLookup(SparqlOperatorType.Multiply, null, this._someNullArgs, false);
            this.TestLookup(SparqlOperatorType.Multiply, typeof(MultiplicationOperator), this._numArgs, true);
        }

        [TestMethod]
        public void SparqlOperatorLookup5()
        {
            this.TestLookup(SparqlOperatorType.Add, typeof(DateTimeAddition), this._dtArgs, true);
            this.TestStrictLookup(SparqlOperatorType.Add, null, this._dtArgs, false);
        }

        [TestMethod]
        public void SparqlOperatorLookup6()
        {
            this.TestLookup(SparqlOperatorType.Subtract, typeof(DateTimeSubtraction), this._dtArgs, true);
            this.TestStrictLookup(SparqlOperatorType.Subtract, null, this._dtArgs, false);
        }

        [TestMethod]
        public void SparqlOperatorLookup7()
        {
            this.TestLookup(SparqlOperatorType.Add, typeof(TimeSpanAddition), this._tsArgs, true);
            this.TestStrictLookup(SparqlOperatorType.Add, null, this._tsArgs, false);
        }

        [TestMethod]
        public void SparqlOperatorLookup8()
        {
            this.TestLookup(SparqlOperatorType.Subtract, typeof(TimeSpanSubtraction), this._tsArgs, true);
            this.TestStrictLookup(SparqlOperatorType.Subtract, null, this._tsArgs, false);
        }
    }
}
