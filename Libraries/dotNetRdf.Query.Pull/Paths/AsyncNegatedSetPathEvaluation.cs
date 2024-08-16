using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace dotNetRdf.Query.Pull.Paths;

internal class AsyncNegatedSetPathEvaluation(NegatedSet algebra, PatternItem pathStart, PatternItem pathEnd) : IAsyncPathEvaluation
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