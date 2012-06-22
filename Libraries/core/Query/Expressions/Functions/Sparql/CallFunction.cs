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
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql
{
    /// <summary>
    /// Class representing the SPARQL CALL() function
    /// </summary>
    public class CallFunction 
        : ISparqlExpression
    {
        private List<ISparqlExpression> _args = new List<ISparqlExpression>();
        private Dictionary<string, ISparqlExpression> _functionCache = new Dictionary<string, ISparqlExpression>();

        /// <summary>
        /// Creates a new COALESCE function with the given expressions as its arguments
        /// </summary>
        /// <param name="expressions">Argument expressions</param>
        public CallFunction(IEnumerable<ISparqlExpression> expressions)
        {
            this._args.AddRange(expressions);
        }

        /// <summary>
        /// Gets the value of the expression as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            if (this._args.Count == 0) return null;

            IValuedNode funcIdent = this._args[0].Evaluate(context, bindingID);
            if (funcIdent == null) throw new RdfQueryException("Function identifier is unbound");
            if (funcIdent.NodeType == NodeType.Uri)
            {
                Uri funcUri = ((IUriNode)funcIdent).Uri;
                ISparqlExpression func;
                if (this._functionCache.TryGetValue(funcUri.ToString(), out func))
                {
                    if (func == null) throw new RdfQueryException("Function identifier does not identify a known function");
                }
                else
                {
                    try
                    {
                        //Try to create the function and cache it - remember to respect the queries Expression Factories if present
                        func = SparqlExpressionFactory.CreateExpression(funcUri, this._args.Skip(1).ToList(), (context.Query != null ? context.Query.ExpressionFactories : Enumerable.Empty<ISparqlCustomExpressionFactory>()));
                        this._functionCache.Add(funcUri.ToString(), func);
                    }
                    catch
                    {
                        //If something goes wrong creating the function cache a null so we ignore this function URI for later calls
                        this._functionCache.Add(funcUri.ToString(), null);
                    }
                }
                //Now invoke the function
                return func.Evaluate(context, bindingID);
            }
            else
            {
                throw new RdfQueryException("Function identifier is not a URI");
            }
        }

        /// <summary>
        /// Gets the Variables used in all the argument expressions of this function
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            {
                return (from e in this._args
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
            output.Append("CALL(");
            for (int i = 0; i < this._args.Count; i++)
            {
                output.Append(this._args[i].ToString());
                if (i < this._args.Count - 1)
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
        public string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordCall;
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
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new CallFunction(this._args.Select(e => transformer.Transform(e)));
        }
    }
}
