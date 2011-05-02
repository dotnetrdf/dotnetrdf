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
    /// Pattern which matches the Blank Node with the given Internal ID regardless of the Graph the nodes come from
    /// </summary>
    public class FixedBlankNodePattern : PatternItem
    {
        private String _id;

        /// <summary>
        /// Creates a new Fixed Blank Node Pattern
        /// </summary>
        /// <param name="id">ID</param>
        public FixedBlankNodePattern(String id)
        {
            if (id.StartsWith("_:"))
            {
                this._id = id.Substring(2);
            }
            else
            {
                this._id = id;
            }
        }

        /// <summary>
        /// Gets the Blank Node ID
        /// </summary>
        public String InternalID
        {
            get
            {
                return this._id;
            }
        }

        /// <summary>
        /// Checks whether the pattern accepts the given Node
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="obj">Node to test</param>
        /// <returns></returns>
        protected internal override bool Accepts(SparqlEvaluationContext context, INode obj)
        {
            if (obj.NodeType == NodeType.Blank)
            {
                return ((IBlankNode)obj).InternalID.Equals(this._id);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a Blank Node with a fixed ID scoped to whichever graph is provided
        /// </summary>
        /// <param name="context">Construct Context</param>
        protected internal override INode Construct(ConstructContext context)
        {
            if (context.Graph != null)
            {
                IBlankNode b = context.Graph.GetBlankNode(this._id);
                if (b != null)
                {
                    return b;
                }
                else
                {
                    return context.Graph.CreateBlankNode(this._id);
                }
            }
            else
            {
                return new BlankNode(context.Graph, this._id);
            }
        }

        /// <summary>
        /// Gets the String representation of the Pattern Item
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<_:" + this._id + ">";
        }
    }
}
