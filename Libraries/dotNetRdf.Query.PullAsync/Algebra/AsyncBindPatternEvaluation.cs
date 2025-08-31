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
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Pull;

namespace VDS.RDF.Query.PullAsync.Algebra;

internal class AsyncBindPatternEvaluation(BindPattern bindPattern, IAsyncEvaluation? inner) : IAsyncEvaluation
{
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (inner == null)
        {
            ISet result = new Set();
            result.Add(bindPattern.Variable,
                bindPattern.InnerExpression.Accept(context.ExpressionProcessor, context,
                    new ExpressionContext(result, activeGraph)));
            yield return result;
        }
        else
        {
            await foreach (ISet solution in inner.Evaluate(context, input, activeGraph, cancellationToken))
            {
                yield return ApplyBinding(solution, context, activeGraph, cancellationToken);
            }
        }
    }

    public async IAsyncEnumerable<IEnumerable<ISet>> EvaluateBatch(PullEvaluationContext context,
        IEnumerable<ISet?> input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (inner == null)
        {
            yield return input.Select(s =>
            {
                ISet result = new Set();
                result.Add(bindPattern.Variable,
                    bindPattern.InnerExpression.Accept(context.ExpressionProcessor, context,
                        new ExpressionContext(result, activeGraph)));
                return result;
            });
        }
        else
        {
            await foreach (IEnumerable<ISet> resultBatch in inner.EvaluateBatch(context, input, activeGraph, cancellationToken))
            {
                yield return resultBatch.Select(s=>ApplyBinding(s, context, activeGraph, cancellationToken));
            }
        }
    }

    private ISet ApplyBinding(ISet input, PullEvaluationContext context, IRefNode? activeGraph, CancellationToken cancellationToken = default)
    {
        if (input.ContainsVariable(bindPattern.Variable))
        {
            throw new RdfQueryException(
                $"Cannot use BIND to assign a value to ?{bindPattern.Variable} a bound value for the variable already exists.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        INode? bindValue = null;
        try
        {
            bindValue = bindPattern.InnerExpression.Accept(context.ExpressionProcessor, context, new ExpressionContext(input, activeGraph));
        }
        catch
        {
            // pass
        }

        input.Add(bindPattern.Variable, bindValue);
        return input;
    }
}