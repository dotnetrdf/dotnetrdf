using System;
using VDS.RDF.Nodes;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Represents the SPARQL STRUUID Function
    /// </summary>
    public class StrUUIDFunction
        : BaseUuidFunction
    {
        /// <summary>
        /// Evaluates the function by returning the string form of the given UUID
        /// </summary>
        /// <param name="uuid">UUID</param>
        /// <returns></returns>
        protected override IValuedNode EvaluateInternal(Guid uuid)
        {
            return new StringNode(uuid.ToString());
        }

        /// <summary>
        /// Gets the functor for the expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordStrUUID;
            }
        }

        public override IExpression Copy()
        {
            return new StrUUIDFunction();
        }
    }
}