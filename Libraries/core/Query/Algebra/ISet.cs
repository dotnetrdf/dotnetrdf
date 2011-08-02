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
    public interface ISet : IEquatable<ISet>
    {
        void Add(string variable, INode value);

        bool ContainsVariable(string variable);

        
        bool IsCompatibleWith(ISet s, IEnumerable<String> vars);

        int ID 
        { 
            get; 
            set; 
        }

        void Remove(string variable);

        INode this[string variable] 
        { 
            get; 
        }

        IEnumerable<INode> Values 
        { 
            get;
        }

        IEnumerable<string> Variables 
        { 
            get;
        }

        ISet Join(ISet other);

        ISet Copy();
    }

    /// <summary>
    /// Abstract Base Class for implementations of the <see cref="ISet">ISet</see> interface
    /// </summary>
    public abstract class BaseSet : ISet
    {
        private int _id = 0;

        public abstract void Add(string variable, INode value);

        public abstract bool ContainsVariable(string variable);

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

        public abstract void Remove(string variable);

        public abstract INode this[string variable]
        {
            get;
        }

        public abstract IEnumerable<INode> Values
        {
            get;
        }

        public abstract IEnumerable<string> Variables
        {
            get;
        }

        public abstract ISet Join(ISet other);

        public abstract ISet Copy();

        public bool Equals(ISet other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            return this.Variables.All(v => other.ContainsVariable(v) && ((this[v] == null && other[v] == null) || this[v].Equals(other[v])));
        }

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

        public abstract override string ToString();

        public abstract override int GetHashCode();
    }
}
