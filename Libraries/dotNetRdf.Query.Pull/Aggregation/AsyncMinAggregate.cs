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

using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Pull.Aggregation;

internal class AsyncMinAggregate(
    ISparqlExpression valueExpression,
    string? maxVar,
    string variableName,
    PullEvaluationContext context)
    : IAsyncAggregation
{
    private INode? _min = null;
    private readonly ISparqlNodeComparer _comparer = context.OrderingComparer;

    public string VariableName { get; } = variableName;
    public INode? Value { get { return _min; } }

    private bool _failed = false;
    public void Start()
    {
    }

    public bool Accept(ExpressionContext expressionContext)
    {
        if (_failed) return true;
        INode? tmp = maxVar != null
            ? (expressionContext.Bindings.ContainsVariable(maxVar) ? expressionContext.Bindings[maxVar] : null)
            : valueExpression.Accept(context.ExpressionProcessor, context, expressionContext);
        if (tmp == null)
        {
            _failed = true;
            return true;
        }

        if (_min == null)
        {
            _min = tmp;
            return true;
        }

        if (_comparer.TryCompare(tmp, _min, out var result))
        {
            if (result < 0)
            {
                _min = tmp;
            }
            return true;
        }

        _failed = true;
        _min = null;
        return true;
    }

    public void End()
    {
    }
}