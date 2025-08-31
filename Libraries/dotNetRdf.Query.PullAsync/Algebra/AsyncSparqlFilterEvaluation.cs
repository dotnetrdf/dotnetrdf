/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Pull;
using VDS.RDF.Query.PullAsync;

namespace VDS.RDF.Query.PullAsync.Algebra;

internal class AsyncSparqlFilterEvaluation(ISparqlFilter filter, IAsyncEvaluation inner, bool failSilently)
    : IAsyncEvaluation
{
    [Obsolete("Replaced by EvaluateBatch()")]
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (ISet innerResult in inner.Evaluate(context, input, activeGraph, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool filterResult;
            try
            {
                filterResult = filter.Expression.Accept(context.ExpressionProcessor, context, new ExpressionContext(innerResult, activeGraph))
                    .AsSafeBoolean();
            }
            catch (RdfQueryException)
            {
                // If the result processed was an identity result, swallow the error
                filterResult = false;
                if (!failSilently && innerResult.Variables.Any()) { throw; }
            }

            if (filterResult) yield return innerResult;
        }
    }

    public async IAsyncEnumerable<IEnumerable<ISet>> EvaluateBatch(PullEvaluationContext context,
        IEnumerable<ISet?> input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (IEnumerable<ISet> innerBatch in inner.EvaluateBatch(context, input, activeGraph,
                           cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return innerBatch.Where(s=>ApplyFilter(context, s, activeGraph));
        }
    }

    private bool ApplyFilter(PullEvaluationContext context, ISet solution, IRefNode? activeGraph)
    {
        try
        {
            return filter.Expression.Accept(context.ExpressionProcessor, context, new ExpressionContext(solution, activeGraph))
                .AsSafeBoolean();
        }
        catch (RdfQueryException)
        {
            if (!failSilently && solution.Variables.Any()) { throw; }
            return false;
        }
    }
}