/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract Class for Nodes, implements the two basic properties of the INode Interface.
    /// </summary>
    public abstract class BaseNode : INode
    {
        /// <summary>
        /// Node Type for the Node.
        /// </summary>
        protected NodeType _nodeType;

        /// <summary>
        /// Base Constructor which instantiates the Graph reference, Graph Uri and Node Type of the Node.
        /// </summary>
        /// <param name="type">Node Type.</param>
        protected BaseNode(NodeType type)
        {
            _nodeType = type;
        }

        /// <summary>
        /// Nodes have a Type.
        /// </summary>
        public NodeType NodeType
        {
            get
            {
                return _nodeType;
            }
        }

        /// <summary>Serves as the default hash function.</summary>
        /// <returns>A hash code for the current object.</returns>
        public abstract override int GetHashCode();

        /// <summary>
        /// Nodes must implement an Equals method.
        /// </summary>
        /// <param name="obj">Object to compare against.</param>
        /// <returns></returns>
        public abstract override bool Equals(object obj);

        /// <summary>
        /// Nodes must implement a ToString method.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Essential for the implementation of GetHashCode to work correctly, Nodes should generate a String representation that is 'unique' as far as that is possible.
        /// </para>
        /// <para>
        /// Any two Nodes which match via the Equals method (based on strict RDF Specification Equality) should produce the same String representation since Hash Codes are generated by calling GetHashCode on this String.
        /// </para>
        /// </remarks>
        public abstract override string ToString();

        /// <summary>
        /// Gets the String representation of the Node formatted with the given Node formatter.
        /// </summary>
        /// <param name="formatter">Formatter.</param>
        /// <returns></returns>
        public virtual string ToString(INodeFormatter formatter)
        {
            return formatter.Format(this);
        }

        /// <summary>
        /// Gets the String representation of the Node formatted with the given Node formatter.
        /// </summary>
        /// <param name="formatter">Formatter.</param>
        /// <param name="segment">Triple Segment.</param>
        /// <returns></returns>
        public virtual string ToString(INodeFormatter formatter, TripleSegment segment)
        {
            return formatter.Format(this, segment);
        }

        /// <summary>
        /// The Equality operator is defined for Nodes.
        /// </summary>
        /// <param name="a">First Node.</param>
        /// <param name="b">Second Node.</param>
        /// <returns>Whether the two Nodes are equal.</returns>
        /// <remarks>Uses the Equals method to evaluate the result.</remarks>
        public static bool operator ==(BaseNode a, BaseNode b)
        {
            if (a is null)
            {
                return b is null;
            }

            return a.Equals(b);
        }

        /// <summary>
        /// The Non-Equality operator is defined for Nodes.
        /// </summary>
        /// <param name="a">First Node.</param>
        /// <param name="b">Second Node.</param>
        /// <returns>Whether the two Nodes are non-equal.</returns>
        /// <remarks>Uses the Equals method to evaluate the result.</remarks>
        public static bool operator !=(BaseNode a, BaseNode b)
        {
            if (a is null)
            {
                return !(b is null);
            }

            return !a.Equals(b);
        }

        /// <summary>
        /// Nodes must implement a CompareTo method to allow them to be Sorted.
        /// </summary>
        /// <param name="other">Node to compare self to.</param>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should use the SPARQL Term Sort Order for ordering nodes (as opposed to value sort order).  Standard implementations of Node type specific comparisons can be found in <see cref="ComparisonHelper">ComparisonHelper</see>.
        /// </remarks>
        public abstract int CompareTo(INode other);

        /// <summary>
        /// Nodes must implement a CompareTo method to allow them to be Sorted.
        /// </summary>
        /// <param name="other">Node to compare self to.</param>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should use the SPARQL Term Sort Order for ordering nodes (as opposed to value sort order).  Standard implementations of Node type specific comparisons can be found in <see cref="ComparisonHelper">ComparisonHelper</see>.
        /// </remarks>
        public abstract int CompareTo(IBlankNode other);

        /// <summary>
        /// Nodes must implement a CompareTo method to allow them to be Sorted.
        /// </summary>
        /// <param name="other">Node to compare self to.</param>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should use the SPARQL Term Sort Order for ordering nodes (as opposed to value sort order).  Standard implementations of Node type specific comparisons can be found in <see cref="ComparisonHelper">ComparisonHelper</see>.
        /// </remarks>
        public abstract int CompareTo(IGraphLiteralNode other);

        /// <summary>
        /// Nodes must implement a CompareTo method to allow them to be Sorted.
        /// </summary>
        /// <param name="other">Node to compare self to.</param>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should use the SPARQL Term Sort Order for ordering nodes (as opposed to value sort order).  Standard implementations of Node type specific comparisons can be found in <see cref="ComparisonHelper">ComparisonHelper</see>.
        /// </remarks>
        public abstract int CompareTo(ILiteralNode other);

        /// <summary>
        /// Nodes must implement a CompareTo method to allow them to be Sorted.
        /// </summary>
        /// <param name="other">Node to compare self to.</param>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should use the SPARQL Term Sort Order for ordering nodes (as opposed to value sort order).  Standard implementations of Node type specific comparisons can be found in <see cref="ComparisonHelper">ComparisonHelper</see>.
        /// </remarks>
        public abstract int CompareTo(IUriNode other);

        /// <summary>
        /// Nodes must implement a CompareTo method to allow them to be Sorted.
        /// </summary>
        /// <param name="other">Node to compare self to.</param>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should use the SPARQL Term Sort Order for ordering nodes (as opposed to value sort order).  Standard implementations of Node type specific comparisons can be found in <see cref="ComparisonHelper">ComparisonHelper</see>.
        /// </remarks>
        public abstract int CompareTo(IVariableNode other);

        /// <summary>
        /// Nodes must implement a CompareTo method to allow them to be Sorted.
        /// </summary>
        /// <param name="other">Node to compare self to.</param>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should use the SPARQL Term Sort Order for ordering nodes (as opposed to value sort order).  Standard implementations of Node type specific comparisons can be found in <see cref="ComparisonHelper">ComparisonHelper</see>.
        /// </remarks>
        public abstract int CompareTo(IRefNode other);

        /// <summary>
        /// Nodes must implement a CompareTo method to allow them to be Sorted.
        /// </summary>
        /// <param name="other">Node to compare self to.</param>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should use the SPARQL Term Sort Order for ordering nodes (as opposed to value sort order).  Standard implementations of Node type specific comparisons can be found in <see cref="ComparisonHelper">ComparisonHelper</see>.
        /// </remarks>
        public abstract int CompareTo(ITripleNode other);

        /// <summary>
        /// Nodes must implement an Equals method so we can do type specific equality.
        /// </summary>
        /// <param name="other">Node to check for equality.</param>
        /// <returns></returns>
        /// <remarks>
        /// Nodes implementations are also required to implement an override of the non-generic Equals method.  Standard implementations of some equality comparisons can be found in <see cref="EqualityHelper">EqualityHelper</see>.
        /// </remarks>
        public abstract bool Equals(INode other);

        /// <summary>
        /// Nodes must implement an Equals method so we can do type specific equality.
        /// </summary>
        /// <param name="other">Node to check for equality.</param>
        /// <returns></returns>
        /// <remarks>
        /// Nodes implementations are also required to implement an override of the non-generic Equals method.  Standard implementations of some equality comparisons can be found in <see cref="EqualityHelper">EqualityHelper</see>.
        /// </remarks>
        public abstract bool Equals(IBlankNode other);

        /// <summary>
        /// Nodes must implement an Equals method so we can do type specific equality.
        /// </summary>
        /// <param name="other">Node to check for equality.</param>
        /// <returns></returns>
        /// <remarks>
        /// Nodes implementations are also required to implement an override of the non-generic Equals method.  Standard implementations of some equality comparisons can be found in <see cref="EqualityHelper">EqualityHelper</see>.
        /// </remarks>
        public abstract bool Equals(IGraphLiteralNode other);

        /// <summary>
        /// Nodes must implement an Equals method so we can do type specific equality.
        /// </summary>
        /// <param name="other">Node to check for equality.</param>
        /// <returns></returns>
        /// <remarks>
        /// Nodes implementations are also required to implement an override of the non-generic Equals method.  Standard implementations of some equality comparisons can be found in <see cref="EqualityHelper">EqualityHelper</see>.
        /// </remarks>
        public abstract bool Equals(ILiteralNode other);

        /// <summary>
        /// Nodes must implement an Equals method so we can do type specific equality.
        /// </summary>
        /// <param name="other">Node to check for equality.</param>
        /// <returns></returns>
        /// <remarks>
        /// Nodes implementations are also required to implement an override of the non-generic Equals method.  Standard implementations of some equality comparisons can be found in <see cref="EqualityHelper">EqualityHelper</see>.
        /// </remarks>
        public abstract bool Equals(IUriNode other);

        /// <summary>
        /// Nodes must implement an Equals method so we can do type specific equality.
        /// </summary>
        /// <param name="other">Node to check for equality.</param>
        /// <returns></returns>
        /// <remarks>
        /// Nodes implementations are also required to implement an override of the non-generic Equals method.  Standard implementations of some equality comparisons can be found in <see cref="EqualityHelper">EqualityHelper</see>.
        /// </remarks>
        public abstract bool Equals(IVariableNode other);

        /// <summary>
        /// Nodes must implement an Equals method so we can do type specific equality.
        /// </summary>
        /// <param name="other">Node to check for equality.</param>
        /// <returns></returns>
        /// <remarks>
        /// Nodes implementations are also required to implement an override of the non-generic Equals method.  Standard implementations of some equality comparisons can be found in <see cref="EqualityHelper">EqualityHelper</see>.
        /// </remarks>
        public abstract bool Equals(IRefNode other);

        /// <summary>
        /// Nodes must implement an Equals method so we can do type specific equality.
        /// </summary>
        /// <param name="other">Node to check for equality.</param>
        /// <returns></returns>
        /// <remarks>
        /// Nodes implementations are also required to implement an override of the non-generic Equals method.  Standard implementations of some equality comparisons can be found in <see cref="EqualityHelper">EqualityHelper</see>.
        /// </remarks>
        public abstract bool Equals(ITripleNode other);

    }
}