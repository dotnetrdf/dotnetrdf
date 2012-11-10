using System;
using VDS.RDF.Query.Builder.Expressions;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Interface for building SPARQL queries 
    /// </summary>
    public interface IQueryBuilder : ICommonQueryBuilder<IQueryBuilder>
    {
        /// <summary>
        /// Applies the DISTINCT modifier if the Query is a SELECT, otherwise leaves query unchanged (since results from any other query are DISTINCT by default)
        /// </summary>
        IQueryBuilder Distinct();
        /// <summary>
        /// Applies a LIMIT
        /// </summary>
        IQueryBuilder Limit(int limit);
        /// <summary>
        /// Applies an OFFSET
        /// </summary>
        IQueryBuilder Offset(int offset);
        /// <summary>
        /// Applies both a LIMIT and OFFSET
        /// </summary>
        IQueryBuilder Slice(int limit, int offset);
        /// <summary>
        /// Builds and returns a <see cref="SparqlQuery"/>
        /// </summary>
        SparqlQuery BuildQuery();

        AssignmentVariableNamePart Bind (Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression);
    }

    public sealed class AssignmentVariableNamePart
    {
        public IQueryBuilder As(string variableName)
        {
            throw new NotImplementedException();
        }
    }
}