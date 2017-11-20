/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Operators;
using VDS.RDF.Query.Operators.Numeric;
using VDS.RDF.Query.Operators.DateTime;
using VDS.RDF.Configuration;

namespace VDS.RDF.Query
{

    public class OperatorTests
    {
        private List<IValuedNode> _numArgs, _someNullArgs, _dtArgs, _tsArgs;

        public OperatorTests()
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
                Assert.True(opExists, "Operator returned when no operator was expected for the given inputs");
                Assert.Equal(returnedOpInstanceType, op.GetType());
            }
            else
            {
                Assert.False(opExists, "No Operator returned when an operator was expected for the given inputs");
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
                catch (Exception)
                {
                    if (shouldFail) return;
                    throw;
                }

                Assert.Equal(expected, actual);
            }
            else
            {
                Assert.True(shouldFail,"Expected to be able to select an operator to apply to the inputs");
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

        [Fact]
        public void SparqlOperatorLookup1()
        {
            this.TestLookup(SparqlOperatorType.Add, null, Enumerable.Empty<IValuedNode>(), false);
            this.TestLookup(SparqlOperatorType.Add, null, this._someNullArgs, false);
            this.TestLookup(SparqlOperatorType.Add, typeof(AdditionOperator), this._numArgs, true);
        }

        [Fact]
        public void SparqlOperatorLookup2()
        {
            this.TestLookup(SparqlOperatorType.Subtract, null, Enumerable.Empty<IValuedNode>(), false);
            this.TestLookup(SparqlOperatorType.Subtract, null, this._someNullArgs, false);
            this.TestLookup(SparqlOperatorType.Subtract, typeof(SubtractionOperator), this._numArgs, true);
        }

        [Fact]
        public void SparqlOperatorLookup3()
        {
            this.TestLookup(SparqlOperatorType.Divide, null, Enumerable.Empty<IValuedNode>(), false);
            this.TestLookup(SparqlOperatorType.Divide, null, this._someNullArgs, false);
            this.TestLookup(SparqlOperatorType.Divide, typeof(DivisionOperator), this._numArgs, true);
        }

        [Fact]
        public void SparqlOperatorLookup4()
        {
            this.TestLookup(SparqlOperatorType.Multiply, null, Enumerable.Empty<IValuedNode>(), false);
            this.TestLookup(SparqlOperatorType.Multiply, null, this._someNullArgs, false);
            this.TestLookup(SparqlOperatorType.Multiply, typeof(MultiplicationOperator), this._numArgs, true);
        }

        [Fact]
        public void SparqlOperatorLookup5()
        {
            this.TestLookup(SparqlOperatorType.Add, typeof(DateTimeAddition), this._dtArgs, true);
            this.TestStrictLookup(SparqlOperatorType.Add, null, this._dtArgs, false);
        }

        [Fact]
        public void SparqlOperatorLookup6()
        {
            this.TestLookup(SparqlOperatorType.Subtract, typeof(DateTimeSubtraction), this._dtArgs, true);
            this.TestStrictLookup(SparqlOperatorType.Subtract, null, this._dtArgs, false);
        }

        [Fact]
        public void SparqlOperatorLookup7()
        {
            this.TestLookup(SparqlOperatorType.Add, typeof(TimeSpanAddition), this._tsArgs, true);
            this.TestStrictLookup(SparqlOperatorType.Add, null, this._tsArgs, false);
        }

        [Fact]
        public void SparqlOperatorLookup8()
        {
            this.TestLookup(SparqlOperatorType.Subtract, typeof(TimeSpanSubtraction), this._tsArgs, true);
            this.TestStrictLookup(SparqlOperatorType.Subtract, null, this._tsArgs, false);
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public void SparqlOperatorRegistration1()
        {
            try
            {
                MockSparqlOperator op = new MockSparqlOperator();
                SparqlOperators.AddOperator(op);
                Assert.True(SparqlOperators.IsRegistered(op));
                SparqlOperators.RemoveOperator(op);
                Assert.False(SparqlOperators.IsRegistered(op));
            }
            finally
            {
                SparqlOperators.Reset();
            }
        }
        [Fact]
        public void SparqlOperatorRegistration2()
        {
            try
            {
                MockSparqlOperator op = new MockSparqlOperator();
                SparqlOperators.AddOperator(op);
                Assert.True(SparqlOperators.IsRegistered(op));
                SparqlOperators.RemoveOperatorByType(new MockSparqlOperator());
                Assert.False(SparqlOperators.IsRegistered(op));
            }
            finally
            {
                SparqlOperators.Reset();
            }
        }

        [Fact]
        public void SparqlOperatorRegistration3()
        {
            try
            {
                MockSparqlOperator op = new MockSparqlOperator();
                SparqlOperators.AddOperator(op);
                Assert.True(SparqlOperators.IsRegistered(op));
                SparqlOperators.Reset();
                Assert.False(SparqlOperators.IsRegistered(op));
            }
            finally
            {
                SparqlOperators.Reset();
            }
        }

    }
}
