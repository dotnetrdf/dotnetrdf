using System;
using VDS.RDF.Query.Builder.Expressions;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Interface for creating SELECT queries
    /// </summary>
    public interface ISelectBuilder : ICommonQueryBuilder<IQueryBuilder>
    {
        /// <summary>
        /// Adds additional SELECT return <paramref name="variables"/>
        /// </summary>
        ISelectBuilder And(params SparqlVariable[] variables);
        /// <summary>
        /// Adds additional SELECT return <paramref name="variables"/>
        /// </summary>
        ISelectBuilder And(params string[] variables);
        /// <summary>
        /// Adds additional SELECT expression
        /// </summary>
        IAssignmentVariableNamePart<ISelectBuilder> And(Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression);
        /// <summary>
        /// Applies the DISTINCT modifier if the Query is a SELECT, otherwise leaves query unchanged (since results from any other query are DISTINCT by default)
        /// </summary>
        ISelectBuilder Distinct();
    }
}