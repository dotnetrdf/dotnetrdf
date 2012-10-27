namespace VDS.RDF.Query.Builder
{
    public interface ISelectQueryBuilder : ICommonQueryBuilder
    {
        ISelectQueryBuilder And(params SparqlVariable[] variables);
        ISelectQueryBuilder And(params string[] variables);
    }
}