using System.Runtime.CompilerServices;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Algebra;

internal class AsyncExtendEvaluation(Extend extend, PullEvaluationContext context, IAsyncEvaluation inner)
    : IAsyncEvaluation
{
    private readonly PullEvaluationContext _context = context;

    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (ISet solution in inner.Evaluate(context, input, activeGraph, cancellationToken))
        {
            if (solution.ContainsVariable(extend.VariableName))
            {
                throw new RdfQueryException(
                    $"Cannot assign to the variable ?{extend.VariableName} since it has previously been used in the query.");
            }

            try
            {
                INode value = extend.AssignExpression.Accept(context.ExpressionProcessor, context, new ExpressionContext(solution, activeGraph));
                solution.Add(extend.VariableName, value);
            }
            catch
            {
                // No assignment if there is an error, but the solution is preserved.
            }

            yield return solution;
        }
    }
}