using System;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for creating DESCRIBE queries
    /// </summary>
    public interface IDescribeBuilder : IQueryWithVariablesBuilder
    {
        /// <summary>
        /// Adds additional <paramref name="variables"/> to DESCRIBE
        /// </summary>
        IDescribeBuilder And(params string[] variables);
        /// <summary>
        /// Adds additional <paramref name="uris"/> to DESCRIBE
        /// </summary>
        IDescribeBuilder And(params Uri[] uris);
    }
}