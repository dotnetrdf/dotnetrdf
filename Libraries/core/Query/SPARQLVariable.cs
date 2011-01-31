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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Aggregates;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Class of Sparql Variables
    /// </summary>
    public class SparqlVariable
    {
        private String _name;
        private bool _isResultVar;
        private ISparqlAggregate _aggregate = null;
        private ISparqlExpression _expr = null;

        /// <summary>
        /// Creates a new Sparql Variable
        /// </summary>
        /// <param name="name">Variable Name (with leading ?/$ removed)</param>
        /// <param name="isResultVar">Does this Variable appear in the Result Set?</param>
        public SparqlVariable(String name, bool isResultVar) {
            this._name = name;
            this._isResultVar = isResultVar;
        }

        /// <summary>
        /// Creates a new Sparql Variable
        /// </summary>
        /// <param name="name">Variable Name (with leading ?/$ removed)</param>
        public SparqlVariable(String name) : this(name, false) { }

        /// <summary>
        /// Creates a new Sparql Variable which is an Aggregate
        /// </summary>
        /// <param name="name">Variable Name (with leading ?/$ removed)</param>
        /// <param name="aggregate">Aggregate Function</param>
        /// <remarks>All Aggregate Variables are automatically considered as Result Variables</remarks>
        public SparqlVariable(String name, ISparqlAggregate aggregate)
            : this(name, true)
        {
            this._aggregate = aggregate;
        }

        /// <summary>
        /// Creates a new Sparql Variable which is a Projection Expression
        /// </summary>
        /// <param name="name">Variable Name (with leading ?/$ removed)</param>
        /// <param name="expr">Projection Expression</param>
        public SparqlVariable(String name, ISparqlExpression expr)
            : this(name, true)
        {
            this._expr = expr;
        }

        /// <summary>
        /// Variable Name
        /// </summary>
        public String Name
        {
            get
            {
                return this._name;
            }
        }

        /// <summary>
        /// Gets whether the Variable appears in the Result Set
        /// </summary>
        public bool IsResultVariable
        {
            get
            {
                return this._isResultVar;
            }
        }

        /// <summary>
        /// Gets whether the Variable is an Aggregate 
        /// </summary>
        public bool IsAggregate
        {
            get
            {
                return (this._aggregate != null);
            }
        }

        /// <summary>
        /// Gets whether the Variable is a Projection Expression
        /// </summary>
        public bool IsProjection
        {
            get
            {
                return (this._expr != null);
            }
        }

        /// <summary>
        /// Gets the Aggregate Function for this Variable
        /// </summary>
        public ISparqlAggregate Aggregate
        {
            get
            {
                return this._aggregate;
            }
        }

        /// <summary>
        /// Gets the Projection Expression for this Variable
        /// </summary>
        public ISparqlExpression Projection
        {
            get
            {
                return this._expr;
            }
        }
        
        /// <summary>
        /// Get the String representation of the Variable
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (this._aggregate != null)
            {
                output.Append('(');
                output.Append(this._aggregate.ToString());
                output.Append(" AS ?" + this._name);
                output.Append(')');
            }
            else if (this._expr != null)
            {
                output.Append('(');
                output.Append(this._expr.ToString());
                output.Append(" AS ?" + this._name);
                output.Append(')');
            }
            else
            {
                output.Append("?" + this._name);
            }
            return output.ToString();
        }
    }
}
