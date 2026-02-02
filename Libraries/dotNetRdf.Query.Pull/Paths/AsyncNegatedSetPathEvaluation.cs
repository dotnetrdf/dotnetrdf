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
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Pull.Paths;

internal class AsyncNegatedSetPathEvaluation(NegatedSet algebra, PatternItem pathEnd) : IAsyncPathEvaluation
{
    public async IAsyncEnumerable<PathResult> Evaluate(PatternItem stepStart, PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (PathResult match in GetMatches(stepStart, context, input, activeGraph, cancellationToken).Distinct())
        {
            yield return match;
        }
    }

    IEnumerable<PathResult> GetMatches(PatternItem stepStart, PullEvaluationContext context, ISet? input,
        IRefNode? activeGraph, CancellationToken cancellationToken)
    {
        foreach (Triple fwdCandidate in GetForwardCandidates(stepStart, context, input, activeGraph))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Set tmp = input != null ? new Set(input) : new Set();
            if (stepStart.Accepts(context, fwdCandidate.Subject, tmp) &&
                pathEnd.Accepts(context, fwdCandidate.Object, tmp))
            {
                yield return new PathResult(fwdCandidate.Subject, fwdCandidate.Predicate);
            }
        }

        foreach (Triple reverseCandidate in GetReverseCandidates(stepStart, context, input, activeGraph))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Set tmp = input != null ? new Set(input) : new Set();
            if (stepStart.Accepts(context, reverseCandidate.Object, tmp) &&
                pathEnd.Accepts(context, reverseCandidate.Subject, tmp))
            {
                yield return new PathResult(reverseCandidate.Object, reverseCandidate.Subject);
            }
        }
    }

    IEnumerable<Triple> GetForwardCandidates(PatternItem stepStart, PullEvaluationContext context, ISet? input, IRefNode? activeGraph)
    {
        if (!algebra.Properties.Any())
        {
            return [];
        }

        PatternItem subjPattern = stepStart.TryEvaluatePattern(input, out INode? subjNode)
            ? new NodeMatchPattern(subjNode)
            : stepStart;
        var predPattern = new VariablePattern(context.AutoVarFactory.NextId());
        PatternItem objPattern = pathEnd.TryEvaluatePattern(input, out INode? objNode) ? new NodeMatchPattern(objNode) : pathEnd;
        var tp = new TriplePattern(subjPattern, predPattern, objPattern);
        return context.GetTriples(tp, input, activeGraph).Where(t => !algebra.Properties.Any(prop => t.HasPredicate(prop.Predicate)));
    }

    IEnumerable<Triple> GetReverseCandidates(PatternItem stepStart, PullEvaluationContext context, ISet? input,
        IRefNode? activeGraph)
    {
        if (!algebra.InverseProperties.Any()) { return []; }
        PatternItem subjPattern = pathEnd.TryEvaluatePattern(input, out INode? subjNode) ? new NodeMatchPattern(subjNode) : pathEnd;
        var predPattern = new VariablePattern(context.AutoVarFactory.NextId());
        PatternItem objPattern = stepStart.TryEvaluatePattern(input, out INode? objNode)
            ? new NodeMatchPattern(objNode)
            : stepStart;
        var tp = new TriplePattern(subjPattern, predPattern, objPattern);
        return context.GetTriples(tp, input, activeGraph)
            .Where(t => !algebra.InverseProperties.Any(prop => t.HasPredicate(prop.Predicate)));
    }
}