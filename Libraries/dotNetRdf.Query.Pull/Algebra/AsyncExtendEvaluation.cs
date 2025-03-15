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
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Algebra;

internal class AsyncExtendEvaluation(Extend extend, PullEvaluationContext context, IAsyncEvaluation inner)
    : IAsyncEvaluation
{
    private readonly PullEvaluationContext _context = context;

    [Obsolete("Replaced by EvaluateBatch()")]
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (ISet solution in inner.Evaluate(context, input, activeGraph, cancellationToken))
        {
            yield return Extend(context, solution, activeGraph);
        }
    }

    public async IAsyncEnumerable<IEnumerable<ISet>> EvaluateBatch(
        PullEvaluationContext context,
        IEnumerable<ISet?> input,
        IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (IEnumerable<ISet> batch in inner.EvaluateBatch(context, input, activeGraph, cancellationToken))
        {
            yield return batch.Select(s => Extend(context, s, activeGraph));
        }
    }

    private ISet Extend(PullEvaluationContext context, ISet solution, IRefNode? activeGraph)
    {
        if (solution.ContainsVariable(extend.VariableName))
        {
            throw new RdfQueryException(
                $"Cannot assign to the variable ?{extend.VariableName} since it has previously been used in the query.");
        }

        try
        {
            INode value = extend.AssignExpression.Accept(context.ExpressionProcessor, context, new ExpressionContext(solution, activeGraph));
            solution.Add(extend.VariableName, value);
        }
        catch
        {
            // No assignment if there is an error, but the solution is preserved.
        }

        return solution;
    }
}