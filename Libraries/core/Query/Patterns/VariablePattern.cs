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
    /// Pattern which matches Variables
    /// </summary>
    public class VariablePattern : PatternItem
    {
        private String _varname;

        /// <summary>
        /// Creates a new Variable Pattern
        /// </summary>
        /// <param name="name">Variable name including the leading ?/$</param>
        public VariablePattern(String name)
        {
            this._varname = name.Substring(1);
        }

        /// <summary>
        /// Checks whether the given Node is a valid value for the Variable in the current Binding Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="obj">Node to test</param>
        /// <returns></returns>
        protected internal override bool Accepts(SparqlEvaluationContext context, INode obj)
        {
            //if (context.RigorousEvaluation)
            //{
                if (context.InputMultiset.ContainsVariable(this._varname))
                {
                    return context.InputMultiset.ContainsValue(this._varname, obj);
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
        /// <returns>The Node which is bound to this Variable in this Solution</returns>
        protected internal override INode Construct(ConstructContext context)
        {
            INode value = context.Set[this._varname];

            if (value == null) throw new RdfQueryException("Unable to construct a Value for this Variable for this solution as it is bound to a null");
            switch (value.NodeType)
            {
                case NodeType.Blank:
                    if (!context.PreserveBlankNodes && value.GraphUri != null)
                    {
                        //Rename Blank Node based on the Graph Uri Hash Code
                        int hash = value.GraphUri.GetEnhancedHashCode();
                        if (hash >= 0)
                        {
                            return new BlankNode(context.Graph, ((IBlankNode)value).InternalID + "-" + value.GraphUri.GetEnhancedHashCode());
                        }
                        else
                        {
                            return new BlankNode(context.Graph, ((IBlankNode)value).InternalID + hash);
                        }
                    }
                    else
                    {
                        return new BlankNode(context.Graph, ((IBlankNode)value).InternalID);
                    }

                default:
                    return context.GetNode(value);
            }
        }

        /// <summary>
        /// Gets the String representation of this pattern
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "?" + this._varname;
        }

        /// <summary>
        /// Gets the Name of the Variable this Pattern matches
        /// </summary>
        public override string VariableName
        {
            get
            {
                return this._varname;
            }
        }
    }
}
