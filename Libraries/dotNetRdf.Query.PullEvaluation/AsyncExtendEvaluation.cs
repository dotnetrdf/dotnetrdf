using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

public class AsyncExtendEvaluation : IAsyncEvaluation
{
    private readonly Extend _extend;
    private readonly PullEvaluationContext _context;
    private readonly IAsyncEvaluation _inner;

    public AsyncExtendEvaluation(Extend extend, PullEvaluationContext context, IAsyncEvaluation inner)
    {
        _extend = extend;
        _context = context;
        _inner = inner;
    }
    
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (ISet solution in _inner.Evaluate(context, input, activeGraph, cancellationToken))
        {
            if (solution.ContainsVariable(_extend.VariableName))
            {
                throw new RdfQueryException(
                    $"Cannot assign to the variable ?{_extend.VariableName} since it has previously been used in the query.");
            }

            try
            {
                INode value = _extend.AssignExpression.Accept(context.ExpressionProcessor, context, solution);
                solution.Add(_extend.VariableName, value);
            }
            catch
            {
                // No assignment if there is an error, but the solution is preserved.
            }

            yield return solution;
        }
    }
}