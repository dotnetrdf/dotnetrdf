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
using Xunit;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Operators;
using VDS.RDF.Query.Operators.Numeric;
using VDS.RDF.Query.Operators.DateTime;

namespace VDS.RDF.Query;


public class OperatorTests
{
    private List<IValuedNode> _numArgs, _someNullArgs, _dtArgs, _tsArgs;

    public OperatorTests()
    {
        _numArgs = new List<IValuedNode>()
        {
            new LongNode(12345),
            new DecimalNode(123.45m),
            new FloatNode(123.45f),
            new DoubleNode(123.45d)
        };
        _someNullArgs = new List<IValuedNode>()
        {
            new LongNode(12345),
            null,
            new BooleanNode(false),
            null
        };
        _dtArgs = new List<IValuedNode>()
        {
            new DateTimeNode(DateTimeOffset.Now),
            new TimeSpanNode(new TimeSpan(0, 1, 30))
        };
        _tsArgs = new List<IValuedNode>()
        {
            new TimeSpanNode(new TimeSpan(1, 0, 0)),
            new TimeSpanNode(new TimeSpan(0, 30, 0)),
            new TimeSpanNode(new TimeSpan(0, 0, 15))
        };
    }

    private void TestLookup(SparqlOperatorType opType, bool strict, Type returnedOpInstanceType,
        IEnumerable<IValuedNode> ns, bool opExists)
    {
        ISparqlOperator op = null;
        if (SparqlOperators.TryGetOperator(opType, strict, out op, ns.ToArray()))
        {
            Assert.True(opExists, "Operator returned when no operator was expected for the given inputs");
            Assert.Equal(returnedOpInstanceType, op.GetType());
        }
        else
        {
            Assert.False(opExists, "No Operator returned when an operator was expected for the given inputs");
        }
    }

    private void TestStrictLookup(SparqlOperatorType opType, Type returnedOpInstanceType,
        IEnumerable<IValuedNode> ns, bool opExists)
    {
        TestLookup(opType, true, returnedOpInstanceType, ns, opExists);
    }

    private void TestApplication(SparqlOperatorType opType, bool strict, IEnumerable<IValuedNode> ns,
        IValuedNode expected, bool shouldFail)
    {
        ISparqlOperator op = null;
        if (SparqlOperators.TryGetOperator(opType, strict, out op, ns.ToArray()))
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
            Assert.True(shouldFail, "Expected to be able to select an operator to apply to the inputs");
        }
    }

    private void TestStrictApplication(SparqlOperatorType opType, IEnumerable<IValuedNode> ns, IValuedNode expected,
        bool shouldFail)
    {
        TestApplication(opType, true, ns, expected, shouldFail);
    }

    [Fact]
    public void SparqlOperatorLookup1()
    {
        TestLookup(SparqlOperatorType.Add, false, null, Enumerable.Empty<IValuedNode>(), false);
        TestLookup(SparqlOperatorType.Add, false, null, _someNullArgs, false);
        TestLookup(SparqlOperatorType.Add, false, typeof(AdditionOperator), _numArgs, true);
    }

    [Fact]
    public void SparqlOperatorLookup2()
    {
        TestLookup(SparqlOperatorType.Subtract, false, null, Enumerable.Empty<IValuedNode>(), false);
        TestLookup(SparqlOperatorType.Subtract, false, null, _someNullArgs, false);
        TestLookup(SparqlOperatorType.Subtract, false, typeof(SubtractionOperator), _numArgs, true);
    }

    [Fact]
    public void SparqlOperatorLookup3()
    {
        TestLookup(SparqlOperatorType.Divide, false, null, Enumerable.Empty<IValuedNode>(), false);
        TestLookup(SparqlOperatorType.Divide, false, null, _someNullArgs, false);
        TestLookup(SparqlOperatorType.Divide, false, typeof(DivisionOperator), _numArgs, true);
    }

    [Fact]
    public void SparqlOperatorLookup4()
    {
        TestLookup(SparqlOperatorType.Multiply, false, null, Enumerable.Empty<IValuedNode>(), false);
        TestLookup(SparqlOperatorType.Multiply, false, null, _someNullArgs, false);
        TestLookup(SparqlOperatorType.Multiply, false, typeof(MultiplicationOperator), _numArgs, true);
    }

    [Fact]
    public void SparqlOperatorLookup5()
    {
        TestLookup(SparqlOperatorType.Add, false, typeof(DateTimeAddition), _dtArgs, true);
        TestStrictLookup(SparqlOperatorType.Add, null, _dtArgs, false);
    }

    [Fact]
    public void SparqlOperatorLookup6()
    {
        TestLookup(SparqlOperatorType.Subtract, false, typeof(DateTimeSubtraction), _dtArgs, true);
        TestStrictLookup(SparqlOperatorType.Subtract, null, _dtArgs, false);
    }

    [Fact]
    public void SparqlOperatorLookup7()
    {
        TestLookup(SparqlOperatorType.Add, false, typeof(TimeSpanAddition), _tsArgs, true);
        TestStrictLookup(SparqlOperatorType.Add, null, _tsArgs, false);
    }

    [Fact]
    public void SparqlOperatorLookup8()
    {
        TestLookup(SparqlOperatorType.Subtract, false, typeof(TimeSpanSubtraction), _tsArgs, true);
        TestStrictLookup(SparqlOperatorType.Subtract, null, _tsArgs, false);
    }

    [Fact]
    public void SparqlOperatorApplicationAddNumeric1()
    {
        var ns = new List<IValuedNode>()
        {
            new LongNode(1),
            new LongNode(2)
        };
        IValuedNode expected = new LongNode(3);
        TestApplication(SparqlOperatorType.Add, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationAddNumeric2()
    {
        var ns = new List<IValuedNode>()
        {
            new LongNode(1),
            new DecimalNode(2)
        };
        IValuedNode expected = new DecimalNode(3);
        TestApplication(SparqlOperatorType.Add, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationAddNumeric3()
    {
        var ns = new List<IValuedNode>()
        {
            new LongNode(1),
            new FloatNode(2)
        };
        IValuedNode expected = new FloatNode(3);
        TestApplication(SparqlOperatorType.Add, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationAddNumeric4()
    {
        var ns = new List<IValuedNode>()
        {
            new LongNode(1),
            new DoubleNode(2)
        };
        IValuedNode expected = new DoubleNode(3);
        TestApplication(SparqlOperatorType.Add, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationAddNumeric5()
    {
        var ns = new List<IValuedNode>()
        {
            new DecimalNode(1),
            new DecimalNode(2)
        };
        IValuedNode expected = new DecimalNode(3);
        TestApplication(SparqlOperatorType.Add, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationAddNumeric6()
    {
        var ns = new List<IValuedNode>()
        {
            new FloatNode(1),
            new DecimalNode(2)
        };
        IValuedNode expected = new FloatNode(3);
        TestApplication(SparqlOperatorType.Add, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationAddNumeric7()
    {
        var ns = new List<IValuedNode>()
        {
            new DoubleNode(1),
            new DecimalNode(2)
        };
        IValuedNode expected = new DoubleNode(3);
        TestApplication(SparqlOperatorType.Add, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationAddNumeric8()
    {
        var ns = new List<IValuedNode>()
        {
            new FloatNode(1),
            new FloatNode(2)
        };
        IValuedNode expected = new FloatNode(3);
        TestApplication(SparqlOperatorType.Add, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationAddNumeric9()
    {
        var ns = new List<IValuedNode>()
        {
            new FloatNode(1),
            new DoubleNode(2)
        };
        IValuedNode expected = new DoubleNode(3);
        TestApplication(SparqlOperatorType.Add, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationAddNumeric10()
    {
        var ns = new List<IValuedNode>()
        {
            new DoubleNode(1),
            new DoubleNode(2)
        };
        IValuedNode expected = new DoubleNode(3);
        TestApplication(SparqlOperatorType.Add, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationSubtractNumeric1()
    {
        var ns = new List<IValuedNode>()
        {
            new LongNode(1),
            new LongNode(2)
        };
        IValuedNode expected = new LongNode(-1);
        TestApplication(SparqlOperatorType.Subtract, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationSubtractNumeric2()
    {
        var ns = new List<IValuedNode>()
        {
            new DecimalNode(1),
            new LongNode(2)
        };
        IValuedNode expected = new DecimalNode(-1);
        TestApplication(SparqlOperatorType.Subtract, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationSubtractNumeric3()
    {
        var ns = new List<IValuedNode>()
        {
            new LongNode(1),
            new FloatNode(2)
        };
        IValuedNode expected = new FloatNode(-1);
        TestApplication(SparqlOperatorType.Subtract, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationSubtractNumeric4()
    {
        var ns = new List<IValuedNode>()
        {
            new LongNode(1),
            new DoubleNode(2)
        };
        IValuedNode expected = new DoubleNode(-1);
        TestApplication(SparqlOperatorType.Subtract, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationSubtractNumeric5()
    {
        var ns = new List<IValuedNode>()
        {
            new DecimalNode(1),
            new DecimalNode(2)
        };
        IValuedNode expected = new DecimalNode(-1);
        TestApplication(SparqlOperatorType.Subtract, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationSubtractNumeric6()
    {
        var ns = new List<IValuedNode>()
        {
            new DecimalNode(1),
            new FloatNode(2)
        };
        IValuedNode expected = new FloatNode(-1);
        TestApplication(SparqlOperatorType.Subtract, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationSubtractNumeric7()
    {
        var ns = new List<IValuedNode>()
        {
            new DecimalNode(1),
            new DoubleNode(2)
        };
        IValuedNode expected = new DoubleNode(-1);
        TestApplication(SparqlOperatorType.Subtract, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationSubtractNumeric8()
    {
        var ns = new List<IValuedNode>()
        {
            new FloatNode(1),
            new FloatNode(2)
        };
        IValuedNode expected = new FloatNode(-1);
        TestApplication(SparqlOperatorType.Subtract, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationSubtractNumeric9()
    {
        var ns = new List<IValuedNode>()
        {
            new FloatNode(1),
            new DoubleNode(2)
        };
        IValuedNode expected = new DoubleNode(-1);
        TestApplication(SparqlOperatorType.Subtract, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationSubtractNumeric10()
    {
        var ns = new List<IValuedNode>()
        {
            new DoubleNode(1),
            new DoubleNode(2)
        };
        IValuedNode expected = new DoubleNode(-1);
        TestApplication(SparqlOperatorType.Subtract, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationDivideNumeric1()
    {
        var ns = new List<IValuedNode>()
        {
            new LongNode(1),
            new LongNode(2)
        };
        IValuedNode expected = new DecimalNode(0.5m);
        TestApplication(SparqlOperatorType.Divide, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationDivideNumeric2()
    {
        var ns = new List<IValuedNode>()
        {
            new LongNode(1),
            new DecimalNode(2)
        };
        IValuedNode expected = new DecimalNode(0.5m);
        TestApplication(SparqlOperatorType.Divide, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationDivideNumeric3()
    {
        var ns = new List<IValuedNode>()
        {
            new LongNode(1),
            new FloatNode(2)
        };
        IValuedNode expected = new FloatNode(0.5f);
        TestApplication(SparqlOperatorType.Divide, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationDivideNumeric4()
    {
        var ns = new List<IValuedNode>()
        {
            new LongNode(1),
            new DoubleNode(2)
        };
        IValuedNode expected = new DoubleNode(0.5d);
        TestApplication(SparqlOperatorType.Divide, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationDivideNumeric5()
    {
        var ns = new List<IValuedNode>()
        {
            new DecimalNode(1),
            new DecimalNode(2)
        };
        IValuedNode expected = new DecimalNode(0.5m);
        TestApplication(SparqlOperatorType.Divide, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationDivideNumeric6()
    {
        var ns = new List<IValuedNode>()
        {
            new DecimalNode(1),
            new FloatNode(2)
        };
        IValuedNode expected = new FloatNode(0.5f);
        TestApplication(SparqlOperatorType.Divide, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationDivideNumeric7()
    {
        var ns = new List<IValuedNode>()
        {
            new DecimalNode(1),
            new DoubleNode(2)
        };
        IValuedNode expected = new DoubleNode(0.5d);
        TestApplication(SparqlOperatorType.Divide, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationDivideNumeric8()
    {
        var ns = new List<IValuedNode>()
        {
            new FloatNode(1),
            new FloatNode(2)
        };
        IValuedNode expected = new FloatNode(0.5f);
        TestApplication(SparqlOperatorType.Divide, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationDivideNumeric9()
    {
        var ns = new List<IValuedNode>()
        {
            new FloatNode(1),
            new DoubleNode(2)
        };
        IValuedNode expected = new DoubleNode(0.5d);
        TestApplication(SparqlOperatorType.Divide, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationDivideNumeric10()
    {
        var ns = new List<IValuedNode>()
        {
            new DoubleNode(1),
            new DoubleNode(2)
        };
        IValuedNode expected = new DoubleNode(0.5d);
        TestApplication(SparqlOperatorType.Divide, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationMultiplyNumeric1()
    {
        var ns = new List<IValuedNode>()
        {
            new LongNode(3),
            new LongNode(6)
        };
        IValuedNode expected = new LongNode(18);
        TestApplication(SparqlOperatorType.Multiply, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationMultiplyNumeric2()
    {
        var ns = new List<IValuedNode>()
        {
            new LongNode(3),
            new DecimalNode(6)
        };
        IValuedNode expected = new DecimalNode(18);
        TestApplication(SparqlOperatorType.Multiply, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationMultiplyNumeric3()
    {
        var ns = new List<IValuedNode>()
        {
            new LongNode(3),
            new FloatNode(6)
        };
        IValuedNode expected = new FloatNode(18);
        TestApplication(SparqlOperatorType.Multiply, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationMultiplyNumeric4()
    {
        var ns = new List<IValuedNode>()
        {
            new LongNode(3),
            new DoubleNode(6)
        };
        IValuedNode expected = new DoubleNode(18);
        TestApplication(SparqlOperatorType.Multiply, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationMultiplyNumeric5()
    {
        var ns = new List<IValuedNode>()
        {
            new DecimalNode(3),
            new DecimalNode(6)
        };
        IValuedNode expected = new DecimalNode(18);
        TestApplication(SparqlOperatorType.Multiply, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationMultiplyNumeric6()
    {
        var ns = new List<IValuedNode>()
        {
            new DecimalNode(3),
            new FloatNode(6)
        };
        IValuedNode expected = new FloatNode(18);
        TestApplication(SparqlOperatorType.Multiply, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationMultiplyNumeric7()
    {
        var ns = new List<IValuedNode>()
        {
            new DecimalNode(3),
            new DoubleNode(6)
        };
        IValuedNode expected = new DoubleNode(18);
        TestApplication(SparqlOperatorType.Multiply, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationMultiplyNumeric8()
    {
        var ns = new List<IValuedNode>()
        {
            new FloatNode(3),
            new FloatNode(6)
        };
        IValuedNode expected = new FloatNode(18);
        TestApplication(SparqlOperatorType.Multiply, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationMultiplyNumeric9()
    {
        var ns = new List<IValuedNode>()
        {
            new FloatNode(3),
            new DoubleNode(6)
        };
        IValuedNode expected = new DoubleNode(18);
        TestApplication(SparqlOperatorType.Multiply, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationMultiplyNumeric10()
    {
        var ns = new List<IValuedNode>()
        {
            new DoubleNode(3),
            new DoubleNode(6)
        };
        IValuedNode expected = new DoubleNode(18);
        TestApplication(SparqlOperatorType.Multiply, false, ns, expected, false);
    }

    [Fact]
    public void SparqlOperatorApplicationAddDateTime1()
    {
        DateTimeOffset now = DateTimeOffset.Now;
        var ns = new List<IValuedNode>()
        {
            new DateTimeNode(now),
            new TimeSpanNode(new TimeSpan(1, 0, 0))
        };
        IValuedNode expected = new DateTimeNode(now.AddHours(1));
        TestApplication(SparqlOperatorType.Add, false, ns, expected, false);
        TestStrictApplication(SparqlOperatorType.Add, ns, expected, true);
    }

    [Fact]
    public void SparqlOperatorApplicationSubtractDateTime1()
    {
        DateTimeOffset now = DateTimeOffset.Now;
        var ns = new List<IValuedNode>()
        {
            new DateTimeNode(now),
            new TimeSpanNode(new TimeSpan(1, 0, 0))
        };
        IValuedNode expected = new DateTimeNode(now.AddHours(-1));
        TestApplication(SparqlOperatorType.Subtract, false, ns, expected, false);
        TestStrictApplication(SparqlOperatorType.Subtract, ns, expected, true);
    }

    [Fact]
    public void SparqlOperatorRegistration1()
    {
        try
        {
            var op = new MockSparqlOperator();
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
            var op = new MockSparqlOperator();
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
            var op = new MockSparqlOperator();
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
