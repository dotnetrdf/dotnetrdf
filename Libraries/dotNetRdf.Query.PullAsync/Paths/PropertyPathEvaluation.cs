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

namespace VDS.RDF.Query.PullAsync.Paths;

internal class PropertyPathEvaluation(INode predicate, PatternItem pathStart, PatternItem pathEnd) : IPathEvaluation
{
    public IEnumerable<PathResult> Evaluate(PatternItem stepStart, PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        CancellationToken cancellationToken)
    {
        return context
            .GetTriples(new TriplePattern(stepStart, new NodeMatchPattern(predicate), pathEnd), input, activeGraph)
            .Select(t => new PathResult(t.Subject, t.Object));
    }

    public async IAsyncEnumerable<PathResult> EvaluateAsync(PatternItem stepStart, PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (Triple t in context.GetTriples(new TriplePattern(stepStart, new NodeMatchPattern(predicate), pathEnd),
                     input, activeGraph))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new PathResult(t.Subject, t.Object);
        }
    }
}