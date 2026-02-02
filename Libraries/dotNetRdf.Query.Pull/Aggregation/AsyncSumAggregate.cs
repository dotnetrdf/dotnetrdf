/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System.Diagnostics;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Pull.Aggregation;

internal class AsyncSumAggregate : IAsyncAggregation
{
    private readonly ISparqlExpression _expression;

    private bool _error;
    private long _longSum = 0;
    private decimal _decimalSum = 0.0m;
    private float _floatSum = 0.0f;
    private double _doubleSum = 0.0d;
    private readonly PullEvaluationContext _context;
    private SparqlNumericType _maxtype = SparqlNumericType.NaN;
    private SparqlNumericType _numtype = SparqlNumericType.NaN;
    private readonly bool _distinct;
    private readonly HashSet<INode>? _values;
    
    internal AsyncSumAggregate(ISparqlExpression valueExpression, bool distinct, string varName, PullEvaluationContext context)
    {
        VariableName = varName;
        _expression = valueExpression;
        _context = context;
        if (distinct) _values = [];
        _distinct = distinct;
    }
    public string VariableName { get; }

    public INode? Value
    {
        get
        {
            return _numtype switch
            {
                SparqlNumericType.NaN => null,
                SparqlNumericType.Integer => new LongNode(_longSum),
                SparqlNumericType.Decimal => new DecimalNode(_decimalSum),
                SparqlNumericType.Float => new FloatNode(_floatSum),
                SparqlNumericType.Double => new DoubleNode(_doubleSum),
                _ => null
            };
        }
    }

    public virtual void Start()
    {
        _longSum = 0;
        _decimalSum = 0.0m;
        _floatSum = 0.0f;
        _doubleSum = 0.0d;
    }

    public bool Accept(ExpressionContext expressionContext)
    {
        if (_error) return false;
        IValuedNode? tmp = _expression.Accept(_context.ExpressionProcessor, _context, expressionContext);
        if (tmp == null) { 
            _error = true;
            return false;
        }

        if (_distinct)
        {
            Debug.Assert(_values != null, nameof(_values) + " != null");
            if (!_values.Add(tmp))
            {
                return true;
            }
        }
        _numtype = tmp.NumericType;
        if (_numtype == SparqlNumericType.NaN)
        {
            _error = true;
            return false;
        }
        
        // Track the Numeric Type
        if ((int)_numtype > (int)_maxtype)
        {
            _maxtype = _numtype;
        }

        // Increment the Totals based on the current Numeric Type
        switch (_maxtype)
        {
            case SparqlNumericType.Integer:
                _longSum += tmp.AsInteger();
                _decimalSum += tmp.AsDecimal();
                _floatSum += tmp.AsFloat();
                _doubleSum += tmp.AsDouble();
                break;
            case SparqlNumericType.Decimal:
                _decimalSum += tmp.AsDecimal();
                _floatSum += tmp.AsFloat();
                _doubleSum += tmp.AsDouble();
                break;
            case SparqlNumericType.Float:
                _floatSum += tmp.AsFloat();
                _doubleSum += tmp.AsDouble();
                break;
            case SparqlNumericType.Double:
                _doubleSum += tmp.AsDouble();
                break;
        }

        return true;
    }

    public void End()
    {
    }
}