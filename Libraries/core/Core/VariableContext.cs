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

namespace VDS.RDF
{
    /// <summary>
    /// Possible Variable Context Types
    /// </summary>
    public enum VariableContextType
    {
        /// <summary>
        /// There is currently no variable context
        /// </summary>
        None,
        /// <summary>
        /// Existential Variable Context
        /// </summary>
        Existential,
        /// <summary>
        /// Universal Variable Context
        /// </summary>
        Universal
    }

    /// <summary>
    /// Represents the Variable Context for Triples
    /// </summary>
    public class VariableContext : BasicTripleContext
    {
        private VariableContextType _type;
        private HashSet<INode> _vars = new HashSet<INode>();
        private VariableContext _innerContext;

        /// <summary>
        /// Creates a new Variable Context
        /// </summary>
        /// <param name="type">Context Type</param>
        public VariableContext(VariableContextType type)
        {
            this._type = type;
        }

        /// <summary>
        /// Gets the Context Type
        /// </summary>
        public VariableContextType Type
        {
            get
            {
                return this._type;
            }
        }

        /// <summary>
        /// Gets the Variables in this Context
        /// </summary>
        public IEnumerable<INode> Variables
        {
            get
            {
                return this._vars;
            }
        }

        /// <summary>
        /// Adds a Variable to this Context
        /// </summary>
        /// <param name="var">Variable</param>
        public void AddVariable(INode var)
        {
            if (var == null) return;

            if (this._innerContext == null)
            {
                this._vars.Add(var);
            }
            else
            {
                this._innerContext.AddVariable(var);
            }
        }

        /// <summary>
        /// Gets whether a given Variable exists in this Context
        /// </summary>
        /// <param name="var">Variable Node</param>
        /// <returns></returns>
        public bool IsVariable(INode var)
        {
            if (this.InnerContext == null)
            {
                return this._vars.Contains(var);
            }
            else
            {
                return this._vars.Contains(var) || this.InnerContext.IsVariable(var);
            }
        }

        /// <summary>
        /// Gets/Sets the Inner Context
        /// </summary>
        /// <remarks>
        /// When you set the Inner Context this sets the Inner Context of the most nested inner context, you can remove all nested contexts by setting this to null
        /// </remarks>
        public VariableContext InnerContext
        {
            get
            {
                return this._innerContext;
            }
            set
            {
                if (value == null || this._innerContext == null)
                {
                    this._innerContext = value;
                }
                else 
                {
                    this._innerContext.InnerContext = value;
                }
            }
        }
    }
}
