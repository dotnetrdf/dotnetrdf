using System;

namespace VDS.RDF.Query.Builder.Expressions
{
    public class DateTimeExpression : TypedLiteralExpression<DateTime>
    {
        internal DateTimeExpression(DateTime literalValue)
            : base(literalValue.ToLiteral(NodeFactory))
        {
        }
    }
}