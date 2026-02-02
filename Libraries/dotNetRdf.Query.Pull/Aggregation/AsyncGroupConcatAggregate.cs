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

using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Pull.Aggregation;

internal class AsyncGroupConcatAggregate : IAsyncAggregation
{
    private readonly StringBuilder _builder;
    private readonly ISparqlExpression _valueExpression;
    private readonly ISparqlExpression? _separatorExpression;
    private readonly bool _hasSeparator;
    private readonly bool _separatorIsConstant;
    private readonly string? _separator;
    private readonly PullEvaluationContext _context;
    private bool _hasValue;

    public AsyncGroupConcatAggregate(
        ISparqlExpression valueExpression,
        ISparqlExpression? separatorExpression,
        string variableName,
        PullEvaluationContext context
    )
    {
        VariableName = variableName;
        _valueExpression = valueExpression;
        if (separatorExpression != null)
        {
            _hasSeparator = true;
            _separatorExpression = separatorExpression;
            if (_separatorExpression is ConstantTerm separatorConstant)
            {
                _separator = separatorConstant.Node.AsString();
                _separatorIsConstant = true;
            }
        }

        _builder = new StringBuilder();
        _context = context;
    }
    
    public string VariableName { get; }
    public INode? Value { get; private set; }
    public void Start()
    {
        _hasValue = false;
    }

    public bool Accept(ExpressionContext expressionContext)
    {
        try
        {
            IValuedNode value = _valueExpression.Accept(_context.ExpressionProcessor, _context, expressionContext);
            if (value == null)
            {
                return true;
            }
            if (_hasValue && _hasSeparator)
            {
                if (_separatorIsConstant)
                {
                    _builder.Append(_separator);
                }
                else if (_separatorExpression != null)
                {
                    IValuedNode separatorTerm = _separatorExpression.Accept(_context.ExpressionProcessor, _context,
                        expressionContext);
                    _builder.Append(separatorTerm.ToString());
                }
            }

            _builder.Append(value.AsString());
            _hasValue = true;
        }
        catch (RdfQueryException)
        {
            // Ignore expressions that fail to evaluate
        }

        return true;
    }

    public void End()
    {
        if (_hasValue)
        {
            Value = new StringNode(_builder.ToString());
        }
    }
}