using System;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Builder.Expressions
{
    public class DateTimeExpression : NumericExpression<DateTime>
    {
        public DateTimeExpression(ISparqlExpression expression)
            : base(expression)
        {
        }
    }
}