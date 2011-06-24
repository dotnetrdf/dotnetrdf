using System;
using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF.Query.Algebra
{
    public interface ISet : IEquatable<ISet>
    {
        void Add(string variable, INode value);

        bool ContainsVariable(string variable);

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
    }

    public abstract class BaseSet : ISet
    {
        private int _id = 0;

        public abstract void Add(string variable, INode value);

        public abstract bool ContainsVariable(string variable);

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
