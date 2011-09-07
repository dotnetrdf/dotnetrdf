/*

Copyright Robert Vesse 2009-11
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
    /// Represents an Unknown Function that is not supported by dotNetRDF
    /// </summary>
    /// <remarks>
    /// <para>
    /// This exists as a placeholder class so users may choose to parse Unknown Functions and have them appear in queries even if they cannot be evaluated.  This is useful when you wish to parse a query locally to check syntactic validity before passing it to an external query processor which may understand how to evaluate the function.  Using this placeholder also allows queries containing Unknown Functions to still be formatted properly.
    /// </para>
    /// </remarks>
    public class UnknownFunction
        : ISparqlExpression
    {
        private Uri _funcUri;
        private List<ISparqlExpression> _args = new List<ISparqlExpression>();

        /// <summary>
        /// Creates a new Unknown Function that has no Arguments
        /// </summary>
        /// <param name="funcUri">Function URI</param>
        public UnknownFunction(Uri funcUri)
        {
            this._funcUri = funcUri;
        }

        /// <summary>
        /// Creates a new Unknown Function that has a Single Argument
        /// </summary>
        /// <param name="funcUri">Function URI</param>
        /// <param name="expr">Argument Expression</param>
        public UnknownFunction(Uri funcUri, ISparqlExpression expr)
            : this(funcUri)
        {
            this._args.Add(expr);
        }

        /// <summary>
        /// Creates a new Unknown Function that has multiple Arguments
        /// </summary>
        /// <param name="funcUri">Function URI</param>
        /// <param name="exprs">Argument Expressions</param>
        public UnknownFunction(Uri funcUri, IEnumerable<ISparqlExpression> exprs)
            : this(funcUri)
        {
            this._args.AddRange(exprs);
        }

        /// <summary>
        /// Gives null as the Value since dotNetRDF does not know how to evaluate Unknown Functions
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            return null;
        }

        /// <summary>
        /// Gets the Effective Boolean Value of the Function (will always be an error as dotNetRDF does not know how to evaluate Unknown Functions)
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
        }

        /// <summary>
        /// Gets the Variables used in the Function
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            {
                return (from arg in this._args
                        from v in arg.Variables
                        select v).Distinct();
            }
        }

        /// <summary>
        /// Gets the Expression Type
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function; 
            }
        }

        /// <summary>
        /// Gets the Function URI of the Expression
        /// </summary>
        public string Functor
        {
            get 
            {
                return this._funcUri.ToString(); 
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return this._args; 
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(this._funcUri.ToString().Replace(">", "\\>"));
            output.Append('>');
            output.Append('(');
            for (int i = 0; i < this._args.Count; i++)
            {
                output.Append(this._args[i].ToString());

                if (i < this._args.Count - 1)
                {
                    output.Append(", ");
                }
            }
            output.Append(')');
            return output.ToString();
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new UnknownFunction(this._funcUri, this._args.Select(e => transformer.Transform(e)));
        }
    }
}
