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
using VDS.RDF.Query.Pull;

namespace VDS.RDF.Query.PullAsync;

internal interface IAsyncEvaluation
{
    /// <summary>
    /// Run the evaluation with an optional set of input variable bindings.
    /// </summary>
    /// <param name="context">The evaluation context to use.</param>
    /// <param name="input">The variable bindings to apply to the evaluation. Null if there are no input variable bindings.</param>
    /// <param name="activeGraph">Overrides the active graph(s) in the context dataset.</param>
    /// <param name="cancellationToken">The cancellation token that can be used to cancel the evaluation.</param>
    /// <returns>An async enumerable over the results of the evaluation.</returns>
    [Obsolete("Replaced by EvaluateBatch()")]
    IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph, CancellationToken cancellationToken = default);

    /// <summary>
    /// Run the evaluation in batch mode
    /// </summary>
    /// <param name="context"></param>
    /// <param name="input"></param>
    /// <param name="activeGraph"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IAsyncEnumerable<IEnumerable<ISet>> EvaluateBatch(PullEvaluationContext context, IEnumerable<ISet?> input, IRefNode? activeGraph, CancellationToken cancellationToken = default);
}