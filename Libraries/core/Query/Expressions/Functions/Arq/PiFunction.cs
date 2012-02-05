using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Arq
{
    /// <summary>
    /// Represents the ARQ pi() function
    /// </summary>
    public class PiFunction 
        : ISparqlExpression
    {
        private IValuedNode _node;

        /// <summary>
        /// Creates a new ARQ Pi function
        /// </summary>
        public PiFunction()
        {
            this._node = new DoubleNode(null, Math.PI);
        }

        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            return this._node;
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public string ToString()
        {
            return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Pi + ">()";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public string Functor
        {
            get
            {
                return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Pi;
            }
        }

        public IEnumerable<string> Variables
        {
            get
            {
                return Enumerable.Empty<string>();
            }
        }

        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

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
            return transformer.Transform(this);
        }
    }
}
