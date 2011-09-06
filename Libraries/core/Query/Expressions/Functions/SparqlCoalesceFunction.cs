/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// Class representing the SPARQL COALESCE() function
    /// </summary>
    public class CoalesceFunction 
        : ISparqlExpression
    {
        private List<ISparqlExpression> _expressions = new List<ISparqlExpression>();

        /// <summary>
        /// Creates a new COALESCE function with the given expressions as its arguments
        /// </summary>
        /// <param name="expressions">Argument expressions</param>
        public CoalesceFunction(IEnumerable<ISparqlExpression> expressions)
        {
            this._expressions.AddRange(expressions);
        }

        /// <summary>
        /// Gets the value of the expression as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            foreach (ISparqlExpression expr in this._expressions)
            {
                try
                {
                    //Test the expression
                    INode temp = expr.Value(context, bindingID);

                    //Don't return nulls
                    if (temp == null) continue;

                    //Otherwise return
                    return temp;
                }
                catch (RdfQueryException)
                {
                    //Ignore the error and try the next expression (if any)
                }
            }

            //Return error if all expressions are null/error
            throw new RdfQueryException("None of the arguments to the COALESCE function could be evaluated to give non-null/error responses for the given Binding");
        }

        /// <summary>
        /// Gets the Effective Boolean value of the expression as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
        }

        /// <summary>
        /// Gets the Variables used in all the argument expressions of this function
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            {
                return (from e in this._expressions
                        from v in e.Variables
                        select v);
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("COALESCE(");
            for (int i = 0; i < this._expressions.Count; i++)
            {
                output.Append(this._expressions[i].ToString());
                if (i < this._expressions.Count - 1)
                {
                    output.Append(", ");
                }
            }
            output.Append(")");
            return output.ToString();
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public String Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordCoalesce;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return this._expressions;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new CoalesceFunction(this._expressions.Select(e => transformer.Transform(e)));
        }
    }
}
