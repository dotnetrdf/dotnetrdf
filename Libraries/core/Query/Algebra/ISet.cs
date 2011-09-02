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
using System.Linq;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Interface for Sets which represents a possible solution during SPARQL evaluation
    /// </summary>
    public interface ISet 
        : IEquatable<ISet>
    {
        /// <summary>
        /// Adds a Value for a Variable to the Set
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <param name="value">Value</param>
        void Add(string variable, INode value);

        /// <summary>
        /// Checks whether the Set contains a given Variable
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <returns></returns>
        bool ContainsVariable(string variable);

        /// <summary>
        /// Gets whether the Set is compatible with a given set based on the given variables
        /// </summary>
        /// <param name="s">Set</param>
        /// <param name="vars">Variables</param>
        /// <returns></returns>
        bool IsCompatibleWith(ISet s, IEnumerable<String> vars);

        /// <summary>
        /// Gets/Sets the ID of the Set
        /// </summary>
        int ID 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Removes a Value for a Variable from the Set
        /// </summary>
        /// <param name="variable">Variable</param>
        void Remove(string variable);

        /// <summary>
        /// Retrieves the Value in this set for the given Variable
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <returns>Either a Node or a null</returns>
        INode this[string variable] 
        { 
            get; 
        }

        /// <summary>
        /// Gets the Values in the Set
        /// </summary>
        IEnumerable<INode> Values 
        { 
            get;
        }

        /// <summary>
        /// Gets the Variables in the Set
        /// </summary>
        IEnumerable<string> Variables 
        { 
            get;
        }

        /// <summary>
        /// Joins the set to another set
        /// </summary>
        /// <param name="other">Other Set</param>
        /// <returns></returns>
        ISet Join(ISet other);

        /// <summary>
        /// Copies the Set
        /// </summary>
        /// <returns></returns>
        ISet Copy();
    }

    /// <summary>
    /// Abstract Base Class for implementations of the <see cref="ISet">ISet</see> interface
    /// </summary>
    public abstract class BaseSet : ISet
    {
        private int _id = 0;

        /// <summary>
        /// Adds a Value for a Variable to the Set
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <param name="value">Value</param>
        public abstract void Add(string variable, INode value);

        /// <summary>
        /// Checks whether the Set contains a given Variable
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <returns></returns>
        public abstract bool ContainsVariable(string variable);

        /// <summary>
        /// Gets whether the Set is compatible with a given set based on the given variables
        /// </summary>
        /// <param name="s">Set</param>
        /// <param name="vars">Variables</param>
        /// <returns></returns>
        public abstract bool IsCompatibleWith(ISet s, IEnumerable<String> vars);

        /// <summary>
        /// Gets/Sets the ID of the Set
        /// </summary>
        public int ID
        {
            get
            {
                return this._id;
            }
            set
            {
                this._id = value;
            }
        }

        /// <summary>
        /// Removes a Value for a Variable from the Set
        /// </summary>
        /// <param name="variable">Variable</param>
        public abstract void Remove(string variable);

        /// <summary>
        /// Retrieves the Value in this set for the given Variable
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <returns>Either a Node or a null</returns>
        public abstract INode this[string variable]
        {
            get;
        }

        /// <summary>
        /// Gets the Values in the Set
        /// </summary>
        public abstract IEnumerable<INode> Values
        {
            get;
        }

        /// <summary>
        /// Gets the Variables in the Set
        /// </summary>
        public abstract IEnumerable<string> Variables
        {
            get;
        }

        /// <summary>
        /// Joins the set to another set
        /// </summary>
        /// <param name="other">Other Set</param>
        /// <returns></returns>
        public abstract ISet Join(ISet other);

        /// <summary>
        /// Copies the Set
        /// </summary>
        /// <returns></returns>
        public abstract ISet Copy();

        /// <summary>
        /// Gets whether the Set is equal to another set
        /// </summary>
        /// <param name="other">Set to compare with</param>
        /// <returns></returns>
        public bool Equals(ISet other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            return this.Variables.All(v => other.ContainsVariable(v) && ((this[v] == null && other[v] == null) || this[v].Equals(other[v])));
        }

        /// <summary>
        /// Gets whether the Set is equal to another object
        /// </summary>
        /// <param name="obj">Object to compare with</param>
        /// <returns></returns>
        public sealed override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
 	        if (obj == null) return false;
            if (obj is ISet)
            {
                return this.Equals((ISet)obj);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the String representation of the Set
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Gets the hash code of the Set
        /// </summary>
        /// <returns></returns>
        public abstract override int GetHashCode();
    }
}
