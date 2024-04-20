using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace dotNetRdf.Query.PullEvaluation;

public class AsyncBindPatternEvaluation : IAsyncEvaluation
{
    private readonly BindPattern _bindPattern;
    private readonly IAsyncEvaluation? _inner;
    
    public AsyncBindPatternEvaluation(BindPattern bindPattern, IAsyncEvaluation? inner)
    {
        _bindPattern = bindPattern;
        _inner = inner;
    }


    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_inner == null)
        {
            ISet result = new Set();
            result.Add(_bindPattern.Variable, _bindPattern.InnerExpression.Accept(context.ExpressionProcessor, context, result));
            yield return result;
        }
        else
        {
            await foreach (ISet solution in _inner.Evaluate(context, input, activeGraph, cancellationToken))
            {
                if (solution.ContainsVariable(_bindPattern.Variable))
                {
                    throw new RdfQueryException(
                        $"Cannot use BIND to assign a value to ?{_bindPattern.Variable} a bound value for the variable already exists.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                INode? bindValue = null;
                try
                {
                    bindValue = _bindPattern.InnerExpression.Accept(context.ExpressionProcessor, context, solution);
                }
                catch
                {
                    // pass
                }

                solution.Add(_bindPattern.Variable, bindValue);
                yield return solution;
            }
        }
    }
}