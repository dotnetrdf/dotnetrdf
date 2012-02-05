using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Arq
{
    /// <summary>
    /// Represents the ARQ e() function
    /// </summary>
    public class EFunction 
        : ISparqlExpression
    {
        private IValuedNode _node = new DoubleNode(null, Math.E);

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
            return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.E + ">()";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public string Functor
        {
            get
            {
                return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.E;
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

        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return transformer.Transform(this);
        }
    }
}
