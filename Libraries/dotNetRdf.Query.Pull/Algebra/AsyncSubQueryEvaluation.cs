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

using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Pull.Algebra;

internal class AsyncSubQueryEvaluation: IAsyncEvaluation
{
    private readonly List<string> _subQueryVariables;
    private readonly IAsyncEvaluation _inner;
    private readonly PullEvaluationContext _innerContext;

    public AsyncSubQueryEvaluation(SubQuery subQuery, IAsyncEvaluation inner, PullEvaluationContext subContext)
    {
        _subQueryVariables = subQuery.Variables.ToList();
        _inner = inner;
        _innerContext = subContext;
    }

    public AsyncSubQueryEvaluation(SubQueryPattern subQueryPattern, IAsyncEvaluation inner,
        PullEvaluationContext subContext)
    {
        _subQueryVariables = subQueryPattern.SubQuery.Variables.Where(v => v.IsResultVariable).Select(v => v.Name)
            .ToList();
        _inner = inner;
        _innerContext = subContext;
    }

    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        CancellationToken cancellationToken = default)
    {
        ISet innerInput = new Set();
        if (input != null)
        {
            foreach (var v in _subQueryVariables)
            {
                if (input.ContainsVariable(v))
                {
                    innerInput.Add(v, input[v]);
                }
            }
        }

        return _inner.Evaluate(_innerContext, innerInput, activeGraph, cancellationToken);
    }
}