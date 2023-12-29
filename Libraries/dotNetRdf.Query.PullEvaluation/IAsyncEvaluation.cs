using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

public interface IAsyncEvaluation
{
    /// <summary>
    /// Run the evaluation with an optional set of input variable bindings.
    /// </summary>
    /// <param name="bindings">The variable bindings to apply to the evaluation. Null if there are no input variable bindings.</param>
    /// <returns>An async enumerable over the results of the evaluation.</returns>
    IAsyncEnumerable<ISet> Evaluate(ISet? bindings);
}