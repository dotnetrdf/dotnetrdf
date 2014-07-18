using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Engine
{
    /// <summary>
    /// Abstract Base Class for implementations of the <see cref="ISet">ISet</see> interface
    /// </summary>
    public abstract class BaseSet
        : ISet
    {
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
        public abstract bool IsCompatibleWith(ISet s, IEnumerable<string> vars);

        /// <summary>
        /// Gets whether the Set is minus compatible with a given set based on the given variables
        /// </summary>
        /// <param name="s">Set</param>
        /// <param name="vars">Variables</param>
        /// <returns></returns>
        public abstract bool IsMinusCompatibleWith(ISet s, IEnumerable<string> vars);

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
        public abstract INode this[string variable] { get; }

        /// <summary>
        /// Gets the Values in the Set
        /// </summary>
        public abstract IEnumerable<INode> Values { get; }

        /// <summary>
        /// Gets the Variables in the Set
        /// </summary>
        public abstract IEnumerable<string> Variables { get; }

        /// <summary>
        /// Gets whether the set is empty
        /// </summary>
        public abstract bool IsEmpty { get; }

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
            return this.Variables.All(v => other.ContainsVariable(v) && ((this[v] == null && other[v] == null) || EqualityHelper.AreNodesEqual(this[v], other[v])));
        }

        /// <summary>
        /// Gets whether the Set is equal to another object
        /// </summary>
        /// <param name="obj">Object to compare with</param>
        /// <returns></returns>
        public override sealed bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj == null) return false;
            if (obj is ISet)
            {
                return this.Equals((ISet) obj);
            }
            return false;
        }

        /// <summary>
        /// Gets the Hash Code of the Set
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <summary>
        /// Gets the String representation of the Set
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            int count = 0;
            foreach (String var in this.Variables.OrderBy(v => v))
            {
                output.Append("?" + var + " = " + this[var].ToSafeString());
                output.Append(" , ");
                count++;
            }
            if (count > 0) output.Remove(output.Length - 3, 3);
            return output.ToString();
        }
    }
}