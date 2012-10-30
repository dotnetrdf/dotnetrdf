using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Builder.Expressions
{
    public abstract class TypedSparqlExpression<T> : SparqlExpression where T : ISparqlExpression
    {
        protected TypedSparqlExpression(T expression)
            : base(expression)
        {
        }

        public new T Expression { get { return (T) base.Expression; } }
    }
}