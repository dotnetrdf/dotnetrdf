using System;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for creating DESCRIBE queries
    /// </summary>
    public interface IDescribeQueryBuilder : IQueryBuilder
    {
        /// <summary>
        /// Adds additional <paramref name="variables"/> to DESCRIBE
        /// </summary>
        IDescribeQueryBuilder And(params string[] variables);
        /// <summary>
        /// Adds additional <paramref name="uris"/> to DESCRIBE
        /// </summary>
        IDescribeQueryBuilder And(params Uri[] uris);
    }
}