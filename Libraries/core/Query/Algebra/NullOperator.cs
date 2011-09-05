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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a part of the algebra that has been determined to not return any results in advance and so can be replaced with this operator which always returns null
    /// </summary>
    /// <remarks>
    /// Primarily intended for use with Algebra Optimisers which are rewriting the algebra to run against an out of memory dataset (e.g. SQL based) where it may be easily possible to determine if a triple pattern will match in advance of actually returning the matches.
    /// </remarks>
    public class NullOperator 
        : ISparqlAlgebra, ITerminalOperator
    {
        private List<String> _vars = new List<string>();

        /// <summary>
        /// Creates a new Null Operator
        /// </summary>
        /// <param name="variables">Variables in the algebra that this null is replacing</param>
        public NullOperator(IEnumerable<String> variables)
        {
            this._vars.AddRange(variables);
        }

        /// <summary>
        /// Evaluates the Null operator which of course always returns a <see cref="NullMultiset">NullMultiset</see>
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            return new NullMultiset();
        }

        /// <summary>
        /// Gets the variables used in this algebra
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            {
                return this._vars;
            }
        }

        /// <summary>
        /// Throws an error since a null operator cannot be transformed back into a query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            throw new RdfQueryException("A NullOperator cannot be transformed back to a Query");
        }

        /// <summary>
        /// Throws an error since a null operator cannot be transformed back into a query
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            throw new RdfQueryException("A NullOperator cannot be transformed into a Graph Pattern");
        }

        /// <summary>
        /// Gets the string representation of the algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "NullOperator()";
        }
    }
}
