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
using VDS.RDF.Query.PullAsync;
using VDS.RDF.Query.PullAsync.Paths;

namespace VDS.RDF.Query.PullAsync.Algebra;

internal class AsyncPropertyPathPatternEvaluation(IPathEvaluation pathEvaluation, PatternItem pathStart, PatternItem pathEnd) : IAsyncEvaluation
{
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (PathResult pathResult in pathEvaluation.EvaluateAsync(pathStart, context, input, activeGraph,
                           cancellationToken).Distinct().WithCancellation(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Set output = input != null ? new Set(input) : new Set();
            if (pathStart.Accepts(context, pathResult.StartNode, output) &&
                pathEnd.Accepts(context, pathResult.EndNode, output))
            {
                yield return output;
            }
        }
    }

    public IAsyncEnumerable<IEnumerable<ISet>> EvaluateBatch(PullEvaluationContext context,
        IEnumerable<ISet?> batch,
        IRefNode? activeGraph,
        CancellationToken cancellationToken = default)
    {
        IList<ISet> results = new List<ISet>();
        foreach (ISet? input in batch)
        {
            foreach (PathResult? pathResult in pathEvaluation
                         .Evaluate(pathStart, context, input, activeGraph, cancellationToken).Distinct())
            {
                cancellationToken.ThrowIfCancellationRequested();
                Set output = input != null ? new Set(input) : new Set();
                if (pathStart.Accepts(context, pathResult.StartNode, output) &&
                    pathEnd.Accepts(context, pathResult.EndNode, output))
                {
                    results.Add(output);
                }
            }
        }
        return results.AsEnumerable().ToAsyncEnumerable();
    }
}