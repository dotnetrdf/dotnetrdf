/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Abstract Base Class for functions that generate UUIDs
    /// </summary>
    public abstract class BaseUUIDFunction
        : ISparqlExpression
    {
        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            Guid uuid = Guid.NewGuid();
            return this.EvaluateInternal(uuid);
        }

        /// <summary>
        /// Method to be implemented by derived classes to implement the actual logic of turning the generated UUID into a RDF term
        /// </summary>
        /// <param name="uuid">UUID</param>
        /// <returns></returns>
        protected abstract IValuedNode EvaluateInternal(Guid uuid);

        /// <summary>
        /// Gets the variables used in the expression
        /// </summary>
        public virtual IEnumerable<string> Variables
        {
            get
            {
                return Enumerable.Empty<System.String>(); 
            }
        }

        /// <summary>
        /// Gets the Type of the expression
        /// </summary>
        public virtual SparqlExpressionType Type
        {
            get
            { 
                return SparqlExpressionType.Function; 
            }
        }

        /// <summary>
        /// Gets the Functor of the expression
        /// </summary>
        public abstract string Functor
        {
            get;
        }

        /// <summary>
        /// Gets the arguments of the expression
        /// </summary>
        public virtual IEnumerable<ISparqlExpression> Arguments
        {
            get 
            { 
                return Enumerable.Empty<ISparqlExpression>();
            }
        }

        /// <summary>
        /// Applies the transformer to the arguments of this expression
        /// </summary>
        /// <param name="transformer">Transformer</param>
        /// <returns></returns>
        public virtual ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return this;
        }

        /// <summary>
        /// Returns whether the function can be parallelised
        /// </summary>
        public virtual bool CanParallelise
        {
            get 
            {
                return true;
            }
        }
    }
}
