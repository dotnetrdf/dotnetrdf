namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// SPARQL Expression Types
    /// </summary>
    public enum ExpressionType
    {
        /// <summary>
        /// The Expression is a Primary Expression which is a leaf in the expression tree
        /// </summary>
        Primary,
        /// <summary>
        /// The Expression is a Unary Operator which has a single argument
        /// </summary>
        UnaryOperator,
        /// <summary>
        /// The Expression is a Binary Operator which has two arguments
        /// </summary>
        BinaryOperator,
        /// <summary>
        /// The Expression is a Function which has zero/more arguments
        /// </summary>
        Function,
        /// <summary>
        /// The Expression is an Aggregate Function which has one/more arguments
        /// </summary>
        Aggregate,
        /// <summary>
        /// The Expression is a Set Operator where the first argument forms the LHS and all remaining arguments form a set on the RHS
        /// </summary>
        SetOperator,
        /// <summary>
        /// The Expression is a Unary Operator that applies to a Graph Pattern
        /// </summary>
        GraphOperator
    }
}