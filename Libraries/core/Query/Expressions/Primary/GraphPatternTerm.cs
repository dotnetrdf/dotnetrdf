using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Expressions.Primary
{
    /// <summary>
    /// Class for representing Graph Pattern Terms (as used in EXISTS/NOT EXISTS)
    /// </summary>
    public class GraphPatternTerm
        : ISparqlExpression
    {
        private GraphPattern _pattern;

        /// <summary>
        /// Creates a new Graph Pattern Term
        /// </summary>
        /// <param name="pattern">Graph Pattern</param>
        public GraphPatternTerm(GraphPattern pattern)
        {
            this._pattern = pattern;
        }

        /// <summary>
        /// Gets the value of this Term as evaluated for the given Bindings in the given Context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bindingID"></param>
        /// <returns></returns>
        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            throw new RdfQueryException("Graph Pattern Terms do not have a Node Value");
        }

        /// <summary>
        /// Gets the Graph Pattern this term represents
        /// </summary>
        public GraphPattern Pattern
        {
            get
            {
                return this._pattern;
            }
        }

        /// <summary>
        /// Gets the Variables used in the Expression
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                return this._pattern.Variables;
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Primary;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public string Functor
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return Enumerable.Empty<ISparqlExpression>();
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return this;
        }
    }
}
