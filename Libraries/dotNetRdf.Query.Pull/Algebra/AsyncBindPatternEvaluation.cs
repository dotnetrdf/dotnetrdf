using System.Runtime.CompilerServices;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Pull.Algebra;

internal class AsyncBindPatternEvaluation(BindPattern bindPattern, IAsyncEvaluation? inner) : IAsyncEvaluation
{
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (inner == null)
        {
            ISet result = new Set();
            result.Add(bindPattern.Variable, bindPattern.InnerExpression.Accept(context.ExpressionProcessor, context, new ExpressionContext(result, activeGraph)));
            yield return result;
        }
        else
        {
            await foreach (ISet solution in inner.Evaluate(context, input, activeGraph, cancellationToken))
            {
                if (solution.ContainsVariable(bindPattern.Variable))
                {
                    throw new RdfQueryException(
                        $"Cannot use BIND to assign a value to ?{bindPattern.Variable} a bound value for the variable already exists.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                INode? bindValue = null;
                try
                {
                    bindValue = bindPattern.InnerExpression.Accept(context.ExpressionProcessor, context, new ExpressionContext(solution, activeGraph));
                }
                catch
                {
                    // pass
                }

                solution.Add(bindPattern.Variable, bindValue);
                yield return solution;
            }
        }
    }
}