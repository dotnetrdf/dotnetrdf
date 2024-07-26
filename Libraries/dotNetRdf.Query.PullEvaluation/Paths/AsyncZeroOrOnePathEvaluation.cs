using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace dotNetRdf.Query.PullEvaluation.Paths;

internal class AsyncZeroOrOnePathEvaluation(IAsyncEvaluation pathEvaluation, PatternItem pathStart, PatternItem pathEnd)
    : IAsyncEvaluation
{
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var haveStartTerm = TryEvaluatePattern(pathStart, input, out INode? startTerm);
        var haveEndTerm = TryEvaluatePattern(pathEnd, input, out INode? endTerm);
        if (haveStartTerm && haveEndTerm && startTerm != null && endTerm != null)
        {
            if (startTerm.Equals(endTerm))
            {
                yield return input ?? new Set();
            }
            else if (await pathEvaluation.Evaluate(context, input, activeGraph, cancellationToken).AnyAsync(cancellationToken))
            {
                yield return input ?? new Set();
            }
        }
        else if (haveStartTerm && pathEnd is VariablePattern vp)
        {
            Set result = input == null ? new Set() : new Set(input);
            result.Add(vp.VariableName, startTerm);
            yield return result;
            await foreach (ISet solution in pathEvaluation.Evaluate(context, input, activeGraph, cancellationToken).Distinct().WithCancellation(cancellationToken))
            {
                if (startTerm == null || !startTerm.Equals(solution[vp.VariableName]))
                {
                    yield return solution;
                }
            }
        }
        else if (haveEndTerm && pathStart is VariablePattern svp)
        {
            var result = new Set(input);
            result.Add(svp.VariableName, endTerm);
            yield return result;
            await foreach (ISet solution in pathEvaluation.Evaluate(context, input, activeGraph, cancellationToken).Distinct().WithCancellation(cancellationToken))
            {
                if (endTerm == null || !endTerm.Equals(solution[svp.VariableName]))
                {
                    yield return solution;
                }
            }
        }
        else
        {
            await foreach (ISet solution in pathEvaluation.Evaluate(context, input, activeGraph, cancellationToken).Distinct().WithCancellation(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return solution;
            }
        }
    }

    private static bool TryEvaluatePattern(PatternItem patternItem, ISet? input, out INode? node)
    {
        switch (patternItem)
        {
            case VariablePattern vp:
                {
                    if (input != null && input.ContainsVariable(vp.VariableName))
                    {
                        INode? tmp = input[vp.VariableName];
                        if (tmp != null)
                        {
                            node = tmp;
                            return true;
                        }
                    }

                    break;
                }
            case NodeMatchPattern nmp:
                node = nmp.Node;
                return true;
            default:
                throw new RdfQueryException(
                    $"Support for pattern item {patternItem} ({patternItem.GetType()}) is not yet implemented.");
        }

        node = null;
        return false;
    }
}