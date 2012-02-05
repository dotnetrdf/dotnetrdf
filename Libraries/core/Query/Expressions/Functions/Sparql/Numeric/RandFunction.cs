using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Numeric
{
    /// <summary>
    /// Represents the SPARQL RAND() Function
    /// </summary>
    public class RandFunction
        : ISparqlExpression
    {
        private Random _rnd = new Random();

        /// <summary>
        /// Creates a new SPARQL RAND() Function
        /// </summary>
        public RandFunction()
            : base() { }

        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            return new DoubleNode(null, this._rnd.NextDouble());
        }

        /// <summary>
        /// Gets the Variables used in this Expression
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Gets the Type of this Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Arguments of this Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return Enumerable.Empty<ISparqlExpression>();
            }
        }

        /// <summary>
        /// Gets the Functor of this Expression
        /// </summary>
        public string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordRand;
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordRand + "()";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return transformer.Transform(this);
        }
    }
}
