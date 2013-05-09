namespace VDS.RDF.Query.Builder
{
    public interface IQueryWithVariablesBuilder
    {
        /// <summary>
        /// Creates a <see cref="QueryBuilder"/>
        /// </summary>
        IQueryBuilder GetQueryBuilder();
    }
}