/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

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
using VDS.RDF.Test.Configuration;

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

        private void TestApplication(SparqlOperatorType opType, IEnumerable<IValuedNode> ns, IValuedNode expected, bool shouldFail)
        {
            ISparqlOperator op = null;
            if (SparqlOperators.TryGetOperator(opType, out op, ns.ToArray()))
            {
                IValuedNode actual;
                try
                {
                    actual = op.Apply(ns.ToArray());
                }
                catch (Exception ex)
                {
                    if (shouldFail) return;
                    throw;
                }

                Assert.AreEqual(expected, actual);
            }
            else
            {
                if (!shouldFail) Assert.Fail("Expected to be able to select an operator to apply to the inputs");
            }
        }

        private void TestStrictApplication(SparqlOperatorType opType, IEnumerable<IValuedNode> ns, IValuedNode expected, bool shouldFail)
        {
            try
            {
                Options.StrictOperators = true;
                this.TestApplication(opType, ns, expected, shouldFail);
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

        [TestMethod]
        public void SparqlOperatorApplicationAddNumeric1()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new LongNode(null, 1),
                new LongNode(null, 2)
            };
            IValuedNode expected = new LongNode(null, 3);
            this.TestApplication(SparqlOperatorType.Add, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationAddNumeric2()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new LongNode(null, 1),
                new DecimalNode(null, 2)
            };
            IValuedNode expected = new DecimalNode(null, 3);
            this.TestApplication(SparqlOperatorType.Add, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationAddNumeric3()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new LongNode(null, 1),
                new FloatNode(null, 2)
            };
            IValuedNode expected = new FloatNode(null, 3);
            this.TestApplication(SparqlOperatorType.Add, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationAddNumeric4()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new LongNode(null, 1),
                new DoubleNode(null, 2)
            };
            IValuedNode expected = new DoubleNode(null, 3);
            this.TestApplication(SparqlOperatorType.Add, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationAddNumeric5()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new DecimalNode(null, 1),
                new DecimalNode(null, 2)
            };
            IValuedNode expected = new DecimalNode(null, 3);
            this.TestApplication(SparqlOperatorType.Add, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationAddNumeric6()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new FloatNode(null, 1),
                new DecimalNode(null, 2)
            };
            IValuedNode expected = new FloatNode(null, 3);
            this.TestApplication(SparqlOperatorType.Add, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationAddNumeric7()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new DoubleNode(null, 1),
                new DecimalNode(null, 2)
            };
            IValuedNode expected = new DoubleNode(null, 3);
            this.TestApplication(SparqlOperatorType.Add, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationAddNumeric8()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new FloatNode(null, 1),
                new FloatNode(null, 2)
            };
            IValuedNode expected = new FloatNode(null, 3);
            this.TestApplication(SparqlOperatorType.Add, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationAddNumeric9()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new FloatNode(null, 1),
                new DoubleNode(null, 2)
            };
            IValuedNode expected = new DoubleNode(null, 3);
            this.TestApplication(SparqlOperatorType.Add, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationAddNumeric10()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new DoubleNode(null, 1),
                new DoubleNode(null, 2)
            };
            IValuedNode expected = new DoubleNode(null, 3);
            this.TestApplication(SparqlOperatorType.Add, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationSubtractNumeric1()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new LongNode(null, 1),
                new LongNode(null, 2)
            };
            IValuedNode expected = new LongNode(null, -1);
            this.TestApplication(SparqlOperatorType.Subtract, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationSubtractNumeric2()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new DecimalNode(null, 1),
                new LongNode(null, 2)
            };
            IValuedNode expected = new DecimalNode(null, -1);
            this.TestApplication(SparqlOperatorType.Subtract, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationSubtractNumeric3()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new LongNode(null, 1),
                new FloatNode(null, 2)
            };
            IValuedNode expected = new FloatNode(null, -1);
            this.TestApplication(SparqlOperatorType.Subtract, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationSubtractNumeric4()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new LongNode(null, 1),
                new DoubleNode(null, 2)
            };
            IValuedNode expected = new DoubleNode(null, -1);
            this.TestApplication(SparqlOperatorType.Subtract, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationSubtractNumeric5()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new DecimalNode(null, 1),
                new DecimalNode(null, 2)
            };
            IValuedNode expected = new DecimalNode(null, -1);
            this.TestApplication(SparqlOperatorType.Subtract, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationSubtractNumeric6()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new DecimalNode(null, 1),
                new FloatNode(null, 2)
            };
            IValuedNode expected = new FloatNode(null, -1);
            this.TestApplication(SparqlOperatorType.Subtract, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationSubtractNumeric7()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new DecimalNode(null, 1),
                new DoubleNode(null, 2)
            };
            IValuedNode expected = new DoubleNode(null, -1);
            this.TestApplication(SparqlOperatorType.Subtract, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationSubtractNumeric8()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new FloatNode(null, 1),
                new FloatNode(null, 2)
            };
            IValuedNode expected = new FloatNode(null, -1);
            this.TestApplication(SparqlOperatorType.Subtract, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationSubtractNumeric9()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new FloatNode(null, 1),
                new DoubleNode(null, 2)
            };
            IValuedNode expected = new DoubleNode(null, -1);
            this.TestApplication(SparqlOperatorType.Subtract, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationSubtractNumeric10()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new DoubleNode(null, 1),
                new DoubleNode(null, 2)
            };
            IValuedNode expected = new DoubleNode(null, -1);
            this.TestApplication(SparqlOperatorType.Subtract, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationDivideNumeric1()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new LongNode(null, 1),
                new LongNode(null, 2)
            };
            IValuedNode expected = new DecimalNode(null, 0.5m);
            this.TestApplication(SparqlOperatorType.Divide, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationDivideNumeric2()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new LongNode(null, 1),
                new DecimalNode(null, 2)
            };
            IValuedNode expected = new DecimalNode(null, 0.5m);
            this.TestApplication(SparqlOperatorType.Divide, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationDivideNumeric3()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new LongNode(null, 1),
                new FloatNode(null, 2)
            };
            IValuedNode expected = new FloatNode(null, 0.5f);
            this.TestApplication(SparqlOperatorType.Divide, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationDivideNumeric4()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new LongNode(null, 1),
                new DoubleNode(null, 2)
            };
            IValuedNode expected = new DoubleNode(null, 0.5d);
            this.TestApplication(SparqlOperatorType.Divide, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationDivideNumeric5()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new DecimalNode(null, 1),
                new DecimalNode(null, 2)
            };
            IValuedNode expected = new DecimalNode(null, 0.5m);
            this.TestApplication(SparqlOperatorType.Divide, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationDivideNumeric6()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new DecimalNode(null, 1),
                new FloatNode(null, 2)
            };
            IValuedNode expected = new FloatNode(null, 0.5f);
            this.TestApplication(SparqlOperatorType.Divide, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationDivideNumeric7()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new DecimalNode(null, 1),
                new DoubleNode(null, 2)
            };
            IValuedNode expected = new DoubleNode(null, 0.5d);
            this.TestApplication(SparqlOperatorType.Divide, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationDivideNumeric8()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new FloatNode(null, 1),
                new FloatNode(null, 2)
            };
            IValuedNode expected = new FloatNode(null, 0.5f);
            this.TestApplication(SparqlOperatorType.Divide, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationDivideNumeric9()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new FloatNode(null, 1),
                new DoubleNode(null, 2)
            };
            IValuedNode expected = new DoubleNode(null, 0.5d);
            this.TestApplication(SparqlOperatorType.Divide, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationDivideNumeric10()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new DoubleNode(null, 1),
                new DoubleNode(null, 2)
            };
            IValuedNode expected = new DoubleNode(null, 0.5d);
            this.TestApplication(SparqlOperatorType.Divide, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationMultiplyNumeric1()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new LongNode(null, 3),
                new LongNode(null, 6)
            };
            IValuedNode expected = new LongNode(null, 18);
            this.TestApplication(SparqlOperatorType.Multiply, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationMultiplyNumeric2()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new LongNode(null, 3),
                new DecimalNode(null, 6)
            };
            IValuedNode expected = new DecimalNode(null, 18);
            this.TestApplication(SparqlOperatorType.Multiply, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationMultiplyNumeric3()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new LongNode(null, 3),
                new FloatNode(null, 6)
            };
            IValuedNode expected = new FloatNode(null, 18);
            this.TestApplication(SparqlOperatorType.Multiply, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationMultiplyNumeric4()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new LongNode(null, 3),
                new DoubleNode(null, 6)
            };
            IValuedNode expected = new DoubleNode(null, 18);
            this.TestApplication(SparqlOperatorType.Multiply, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationMultiplyNumeric5()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new DecimalNode(null, 3),
                new DecimalNode(null, 6)
            };
            IValuedNode expected = new DecimalNode(null, 18);
            this.TestApplication(SparqlOperatorType.Multiply, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationMultiplyNumeric6()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new DecimalNode(null, 3),
                new FloatNode(null, 6)
            };
            IValuedNode expected = new FloatNode(null, 18);
            this.TestApplication(SparqlOperatorType.Multiply, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationMultiplyNumeric7()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new DecimalNode(null, 3),
                new DoubleNode(null, 6)
            };
            IValuedNode expected = new DoubleNode(null, 18);
            this.TestApplication(SparqlOperatorType.Multiply, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationMultiplyNumeric8()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new FloatNode(null, 3),
                new FloatNode(null, 6)
            };
            IValuedNode expected = new FloatNode(null, 18);
            this.TestApplication(SparqlOperatorType.Multiply, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationMultiplyNumeric9()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new FloatNode(null, 3),
                new DoubleNode(null, 6)
            };
            IValuedNode expected = new DoubleNode(null, 18);
            this.TestApplication(SparqlOperatorType.Multiply, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationMultiplyNumeric10()
        {
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new DoubleNode(null, 3),
                new DoubleNode(null, 6)
            };
            IValuedNode expected = new DoubleNode(null, 18);
            this.TestApplication(SparqlOperatorType.Multiply, ns, expected, false);
        }

        [TestMethod]
        public void SparqlOperatorApplicationAddDateTime1()
        {
            DateTimeOffset now = DateTimeOffset.Now;
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new DateTimeNode(null, now),
                new TimeSpanNode(null, new TimeSpan(1, 0, 0))
            };
            IValuedNode expected = new DateTimeNode(null, now.AddHours(1));
            this.TestApplication(SparqlOperatorType.Add, ns, expected, false);
            this.TestStrictApplication(SparqlOperatorType.Add, ns, expected, true);
        }

        [TestMethod]
        public void SparqlOperatorApplicationSubtractDateTime1()
        {
            DateTimeOffset now = DateTimeOffset.Now;
            List<IValuedNode> ns = new List<IValuedNode>()
            {
                new DateTimeNode(null, now),
                new TimeSpanNode(null, new TimeSpan(1, 0, 0))
            };
            IValuedNode expected = new DateTimeNode(null, now.AddHours(-1));
            this.TestApplication(SparqlOperatorType.Subtract, ns, expected, false);
            this.TestStrictApplication(SparqlOperatorType.Subtract, ns, expected, true);
        }

        [TestMethod]
        public void SparqlOperatorRegistration1()
        {
            try
            {
                MockSparqlOperator op = new MockSparqlOperator();
                SparqlOperators.AddOperator(op);
                Assert.IsTrue(SparqlOperators.IsRegistered(op));
                SparqlOperators.RemoveOperator(op);
                Assert.IsFalse(SparqlOperators.IsRegistered(op));
            }
            finally
            {
                SparqlOperators.Reset();
            }
        }
        [TestMethod]
        public void SparqlOperatorRegistration2()
        {
            try
            {
                MockSparqlOperator op = new MockSparqlOperator();
                SparqlOperators.AddOperator(op);
                Assert.IsTrue(SparqlOperators.IsRegistered(op));
                SparqlOperators.RemoveOperatorByType(new MockSparqlOperator());
                Assert.IsFalse(SparqlOperators.IsRegistered(op));
            }
            finally
            {
                SparqlOperators.Reset();
            }
        }

        [TestMethod]
        public void SparqlOperatorRegistration3()
        {
            try
            {
                MockSparqlOperator op = new MockSparqlOperator();
                SparqlOperators.AddOperator(op);
                Assert.IsTrue(SparqlOperators.IsRegistered(op));
                SparqlOperators.Reset();
                Assert.IsFalse(SparqlOperators.IsRegistered(op));
            }
            finally
            {
                SparqlOperators.Reset();
            }
        }

    }
}
