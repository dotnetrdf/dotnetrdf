using System;
using VDS.RDF.Query.Builder.Expressions;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Interface for creating SELECT queries
    /// </summary>
    public interface ISelectQueryBuilder : IQueryBuilder
    {
        /// <summary>
        /// Adds additional SELECT <paramref name="variables"/>
        /// </summary>
        ISelectQueryBuilder And(params SparqlVariable[] variables);
        /// <summary>
        /// Adds additional SELECT <paramref name="variables"/>
        /// </summary>
        ISelectQueryBuilder And(params string[] variables);

        AssignmentVariableNamePart<ISelectQueryBuilder> And(Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression);
    }
}