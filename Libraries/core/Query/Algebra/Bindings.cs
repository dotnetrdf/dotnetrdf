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
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a BINDINGS modifier on a SPARQL Query
    /// </summary>
    public class Bindings
        : ITerminalOperator
    {
        private BindingsPattern _bindings;
        private BaseMultiset _mset;

        /// <summary>
        /// Creates a new BINDINGS modifier
        /// </summary>
        /// <param name="bindings">Bindings</param>
        public Bindings(BindingsPattern bindings)
        {
            if (bindings == null) throw new ArgumentNullException("Null Bindings");
            this._bindings = bindings;
        }

        /// <summary>
        /// Evaluates the BINDINGS modifier
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            if (this._mset == null)
            {
                this._mset = this._bindings.ToMultiset();
            }
            return this._mset;
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return this._bindings.Variables;
            }
        }

        /// <summary>
        /// Gets the Bindings 
        /// </summary>
        public BindingsPattern BindingsPattern
        {
            get
            {
                return this._bindings;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Bindings()";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            GraphPattern gp = this.ToGraphPattern();
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = gp;
            return q;
        }

        /// <summary>
        /// Convers the Algebra back to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern gp = new GraphPattern();
            gp.AddInlineData(this._bindings);
            return gp;
        }

        ///// <summary>
        ///// Transforms the Inner Algebra using the given Optimiser
        ///// </summary>
        ///// <param name="optimiser">Optimiser</param>
        ///// <returns></returns>
        //public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        //{
        //    return this;
        //}
    }
}
