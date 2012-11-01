using System;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    public class IriExpression : RdfTermExpression
    {
        public IriExpression(string iriLiteral) 
            : base(new ConstantTerm(new UriNode(null, new Uri(iriLiteral))))
        {
        }
    }
}