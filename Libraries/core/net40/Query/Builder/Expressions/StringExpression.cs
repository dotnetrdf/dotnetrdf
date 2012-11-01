namespace VDS.RDF.Query.Builder.Expressions
{
    class StringExpression : TypedLiteralExpression<string>
    {
        public StringExpression(string str) : base(str.ToLiteral(NodeFactory))
        {
        }
    }
}