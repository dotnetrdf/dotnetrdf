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
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Class for representing Node Patterns in Sparql Queries
    /// </summary>
    public abstract class PatternItem
    {
        /// <summary>
        /// Binding Context for Pattern Item
        /// </summary>
        protected SparqlResultBinder _context = null;

        private bool _repeated = false;

        /// <summary>
        /// Checks whether the Pattern Item accepts the given Node in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="obj">Node to test</param>
        /// <returns></returns>
        protected internal abstract bool Accepts(SparqlEvaluationContext context, INode obj);

        /// <summary>
        /// Constructs a Node based on this Pattern for the given Set
        /// </summary>
        /// <param name="context">Construct Context</param>
        /// <returns></returns>
        protected internal abstract INode Construct(ConstructContext context);

        /// <summary>
        /// Sets the Binding Context for the Pattern Item
        /// </summary>
        public SparqlResultBinder BindingContext
        {
            set
            {
                this._context = value;
            }
        }

        /// <summary>
        /// Gets the String representation of the Pattern
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Gets the Variable Name if this is a Variable Pattern or null otherwise
        /// </summary>
        public virtual String VariableName
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Variable is repeated in the Pattern
        /// </summary>
        public virtual bool Repeated
        {
            get
            {
                return this._repeated;
            }
            set
            {
                this._repeated = value;
            }
        }

    }
}