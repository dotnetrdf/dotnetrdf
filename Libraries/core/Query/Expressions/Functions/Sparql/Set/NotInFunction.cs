using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Set
{
    /// <summary>
    /// Class representing the SPARQL NOT IN set function
    /// </summary>
    public class NotInFunction
        : BaseSetFunction
    {
        /// <summary>
        /// Creates a new SPARQL NOT IN function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="set">Set</param>
        public NotInFunction(ISparqlExpression expr, IEnumerable<ISparqlExpression> set)
            : base(expr, set) { }

        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            INode result = this._expr.Evaluate(context, bindingID);
            if (result != null)
            {
                if (this._expressions.Count == 0) return new BooleanNode(null, true);

                //Have to use SPARQL Value Equality here
                //If any expressions error and nothing in the set matches then an error is thrown
                bool errors = false;
                foreach (ISparqlExpression expr in this._expressions)
                {
                    try
                    {
                        INode temp = expr.Evaluate(context, bindingID);
                        if (SparqlSpecsHelper.Equality(result, temp)) return new BooleanNode(null, false);
                    }
                    catch
                    {
                        errors = true;
                    }
                }

                if (errors)
                {
                    throw new RdfQueryException("One/more expressions in a Set function failed to evaluate");
                }
                else
                {
                    return new BooleanNode(null, true);
                }
            }
            else
            {
                return new BooleanNode(null, true);
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordNotIn;
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append(this._expr.ToString());
            output.Append(" NOT IN (");
            for (int i = 0; i < this._expressions.Count; i++)
            {
                output.Append(this._expressions[i].ToString());
                if (i < this._expressions.Count - 1)
                {
                    output.Append(" , ");
                }
            }
            output.Append(")");
            return output.ToString();
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new NotInFunction(transformer.Transform(this._expr), this._expressions.Select(e => transformer.Transform(e)));
        }
    }
}
