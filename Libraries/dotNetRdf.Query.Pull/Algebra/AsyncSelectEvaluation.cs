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

using System.Runtime.CompilerServices;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Algebra;

internal class AsyncSelectEvaluation(Select select, IAsyncEvaluation inner) : IAsyncEvaluation
{
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (ISet innerResult in inner.Evaluate(context, input, activeGraph, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return ProcessInnerResult(innerResult, context, activeGraph);
        }
    }

    private ISet ProcessInnerResult(ISet innerResult, PullEvaluationContext context, IRefNode? activeGraph)
    {
        if (select.IsSelectAll)
        {
            // Filter out auto-generated variables
            foreach (var v in innerResult.Variables)
            {
                if (v.StartsWith(context.AutoVarFactory.Prefix))
                {
                    innerResult.Remove(v);
                }
            }

            return innerResult;
        }

        var resultSet = new Set();
        foreach (SparqlVariable sv in select.SparqlVariables)
        {
            if (sv.IsResultVariable)
            {
                if (sv.Name.StartsWith(context.AutoVarFactory.Prefix)) continue;
                INode? variableBinding =
                    sv.IsProjection ? TryProcessProjection(sv, innerResult, context, activeGraph) :
                    innerResult.ContainsVariable(sv.Name) ? innerResult[sv.Name] : null;
                resultSet.Add(sv.Name, variableBinding);
            }
        }

        return resultSet;
    }

    private INode? TryProcessProjection(SparqlVariable sv, ISet innerResult, PullEvaluationContext context,
        IRefNode? activeGraph)
    {
        try
        {
            return sv.Projection.Accept(context.ExpressionProcessor, context,
                new ExpressionContext(innerResult, activeGraph));
        }
        catch (RdfQueryException)
        {
            return null;
        }
    }
}