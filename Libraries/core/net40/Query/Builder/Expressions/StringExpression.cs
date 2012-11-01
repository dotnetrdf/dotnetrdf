namespace VDS.RDF.Query.Builder.Expressions
{
    public class StringExpression : TypedLiteralExpression<string>
    {
        internal StringExpression(string str) : base(str.ToLiteral(NodeFactory))
        {
        }
    }
}