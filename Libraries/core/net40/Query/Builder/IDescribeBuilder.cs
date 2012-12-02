using System;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for creating DESCRIBE queries
    /// </summary>
    public interface IDescribeBuilder : ICommonQueryBuilder<IQueryBuilder>
    {
        /// <summary>
        /// Adds additional <paramref name="variables"/> to DESCRIBE
        /// </summary>
        IDescribeBuilder And(params string[] variables);
        /// <summary>
        /// Adds additional <paramref name="uris"/> to DESCRIBE
        /// </summary>
        IDescribeBuilder And(params Uri[] uris);
        /// <summary>
        /// Builds a simple DESCRIBE query without the WHERE part
        /// </summary>
        SparqlQuery BuildQuery();
    }
}