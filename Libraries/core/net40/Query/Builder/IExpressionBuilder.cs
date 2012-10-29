namespace VDS.RDF.Query.Builder
{
    public interface IExpressionBuilder
    {
        void Regex(string regularExpression, string regexPattern);
    }
}