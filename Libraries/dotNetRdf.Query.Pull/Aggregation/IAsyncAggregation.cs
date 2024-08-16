using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Aggregation;

/// <summary>
/// An interface to be implemented by classes providing aggregation over an ISet enumeration
/// </summary>
public interface IAsyncAggregation
{
    /// <summary>
    /// Get the name of the variable that the <see cref="Value"/> node will be bound to
    /// </summary>
    public string VariableName { get; }

    /// <summary>
    /// Get the current value of the aggregation. 
    /// </summary>
    /// <remarks></remarks>
    public INode? Value { get; }

    /// <summary>
    /// Invoked before the first <see cref="ISet"/> is passed to <see cref="Accept"/>.
    /// </summary>
    public void Start();
    
    /// <summary>
    /// Invoked once for each solution binding to aggregate over.
    /// </summary>
    /// <param name="expressionContext">The context for evaluation of the aggregate expression, consisting of a set of variable bindings and an optional active graph name.</param>
    /// <returns>True if the solution binding was successfully processed by the aggregate, false if an error has occured.</returns>
    /// <remarks>If a call to <see cref="Accept"/> returns false, the caller may choose to end iteration of solutions as an error during aggregation will typically result in a null value being produced for the aggregation.</remarks>
    public bool Accept(ExpressionContext expressionContext);
    
    /// <summary>
    /// Invoked once after the last <see cref="ISet"/> is passed to <see cref="Accept"/>
    /// </summary>
    public void End();
}