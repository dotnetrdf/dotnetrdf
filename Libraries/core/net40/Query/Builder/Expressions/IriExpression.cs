using System;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    /// <summary>
    /// Represents a IRI expression
    /// </summary>
    public class IriExpression : RdfTermExpression
    {
        internal IriExpression(string iriLiteral) 
            : base(new ConstantTerm(new UriNode(null, new Uri(iriLiteral))))
        {
        }

        /// <summary>
        /// Wraps the <paramref name="expression"/> as an IRI expression
        /// </summary>
        public IriExpression(ISparqlExpression expression) : base(expression)
        {
        }
    }
}