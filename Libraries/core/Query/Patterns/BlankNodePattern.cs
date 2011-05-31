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
using VDS.RDF.Query.Construct;

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Pattern which matches Blank Nodes
    /// </summary>
    public class BlankNodePattern : PatternItem
    {
        private String _name;

        /// <summary>
        /// Creates a new Pattern representing a Blank Node
        /// </summary>
        /// <param name="name">Blank Node ID</param>
        public BlankNodePattern(String name)
        {
            this._name = "_:" + name;
        }

        /// <summary>
        /// Gets the Blank Node ID
        /// </summary>
        public String ID
        {
            get
            {
                return this._name;
            }
        }

        /// <summary>
        /// Checks whether the given Node is a valid value for the Temporary Variable
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="obj">Node to test</param>
        /// <returns></returns>
        protected internal override bool Accepts(SparqlEvaluationContext context, INode obj)
        {
            //if (context.RigorousEvaluation)
            //{
                if (context.InputMultiset.ContainsVariable(this._name))
                {
                    return context.InputMultiset.ContainsValue(this._name, obj);
                }
                else if (this.Repeated)
                {
                    return true;
                }
                else
                {
                    return true;
                }
            //}
            //else
            //{
            //    return true;
            //}
        }

        /// <summary>
        /// Constructs a Node based on the given Set
        /// </summary>
        /// <param name="context">Construct Context</param>
        /// <returns></returns>
        protected internal override INode Construct(ConstructContext context)
        {
            return context.GetBlankNode(this._name);
        }

        /// <summary>
        /// Gets the String representation of this Pattern
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._name;
        }

        /// <summary>
        /// Gets the Temporary Variable Name of this Pattern
        /// </summary>
        public override string VariableName
        {
            get
            {
                return this._name;
            }
        }
    }
}
