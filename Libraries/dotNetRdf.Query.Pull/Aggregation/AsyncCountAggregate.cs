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

using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Pull.Aggregation;

internal class AsyncCountAggregate(
    ISparqlExpression valueExpression,
    string? countVar,
    string variableName,
    PullEvaluationContext context)
    : IAsyncAggregation
{
    private long _count = 0;

    public string VariableName { get; } = variableName;

    public INode? Value { get { return new LongNode(_count); } }

    public void Start()
    {
        _count = 0;
    }

    public bool Accept(ExpressionContext expressionContext)
    {
        if (countVar != null && expressionContext.Bindings.ContainsVariable(countVar) &&
            expressionContext.Bindings[countVar] != null)
        {
            _count++;
        }
        else
        {
            INode? tmp = valueExpression.Accept(context.ExpressionProcessor, context, expressionContext);
            if (tmp != null) _count++;
        }

        return true;
    }

    public void End()
    {

    }
}