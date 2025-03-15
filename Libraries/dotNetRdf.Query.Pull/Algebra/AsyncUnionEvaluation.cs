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

using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Algebra;

internal class AsyncUnionEvaluation(IAsyncEvaluation lhs, IAsyncEvaluation rhs) : IAsyncEvaluation
{
    [Obsolete("Replaced by EvaluateBatch()")]
    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph, CancellationToken cancellationToken = default)
    {
        return AsyncEnumerableEx.Merge(lhs.Evaluate(context, input, activeGraph, cancellationToken),
            rhs.Evaluate(context, input, activeGraph, cancellationToken));
    }

    public IAsyncEnumerable<IEnumerable<ISet>> EvaluateBatch(PullEvaluationContext context, IEnumerable<ISet?> input, IRefNode? activeGraph,
        CancellationToken cancellationToken = default)
    {
        IList<ISet?> inputList = input.ToList();
        return AsyncEnumerableEx.Merge(lhs.EvaluateBatch(context, inputList, activeGraph, cancellationToken),
            rhs.EvaluateBatch(context, inputList, activeGraph, cancellationToken));
    }
}