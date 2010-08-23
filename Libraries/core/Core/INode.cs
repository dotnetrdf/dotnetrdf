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
    /// Node Type Values
    /// </summary>
    public enum NodeType
    {
        /// <summary>
        /// A Blank Node
        /// </summary>
        Blank = 0, 
        /// <summary>
        /// A Uri Node
        /// </summary>
        Uri = 1, 
        /// <summary>
        /// A Literal Node
        /// </summary>
        Literal = 2,
        /// <summary>
        /// A Graph Literal Node
        /// </summary>
        GraphLiteral = 3
    }

    /// <summary>
    /// Interface for Nodes
    /// </summary>
    public interface INode : IComparable<INode>, IEquatable<INode>
    {
        /// <summary>
        /// Nodes have a Type
        /// </summary>
        /// <remarks>Primarily provided so can do quick integer comparison to see what type of Node you have without having to do actual full blown Type comparison</remarks>
        NodeType NodeType
        {
            get;
        }

        /// <summary>
        /// Nodes belong to a Graph
        /// </summary>
        IGraph Graph
        {
            get;
        }

        /// <summary>
        /// The Graph a Node belongs to may have a Uri
        /// </summary>
        Uri GraphUri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether the Nodes Hash Code collides with other Nodes in the Graph
        /// </summary>
        /// <remarks>
        /// Designed for internal use only, exposed via the Interface in order to simplify implementation.  For Triples the equivalent method is protected internal since we pass a concrete class as the parameter and can do this but without switching the entire API to use <see cref="BaseNode">BaseNode</see> as the type for Nodes the same is not possible and this is not a change we wish to make to the API as it limits extensibility
        /// </remarks>
        bool Collides
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Abstract Class for Nodes, implements the two basic properties of the INode Interface
    /// </summary>
    public abstract class BaseNode : INode
    {
        /// <summary>
        /// Reference to the Graph that the Node belongs to
        /// </summary>
        protected IGraph _graph;
        /// <summary>
        /// Uri of the Graph that the Node belongs to
        /// </summary>
        protected Uri _graphUri;
        /// <summary>
        /// Node Type for the Node
        /// </summary>
        protected NodeType _nodetype = NodeType.Literal;
        /// <summary>
        /// Stores the computed Hash Code for this Node
        /// </summary>
        protected int _hashcode;

        private bool _collides = false;

        /// <summary>
        /// Base Constructor which instantiates the Graph reference, Graph Uri and Node Type of the Node
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="type">Node Type</param>
        public BaseNode(IGraph g, NodeType type)
        {
            this._graph = g;
            if (this._graph != null) this._graphUri = this._graph.BaseUri;
            this._nodetype = type;
        }

        /// <summary>
        /// Nodes have a Type
        /// </summary>
        public NodeType NodeType
        {
            get
            {
                return _nodetype;
            }
        }

        /// <summary>
        /// Nodes belong to a Graph
        /// </summary>
        public IGraph Graph
        {
            get
            {
                return _graph;
            }
        }

        /// <summary>
        /// Gets/Sets the Graph Uri of the Node
        /// </summary>
        public Uri GraphUri
        {
            get
            {
                return this._graphUri;
            }
            set
            {
                this._graphUri = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Nodes Hash Code collides with other Nodes in the Node Collection it is contained within
        /// </summary>
        public bool Collides
        {
            get
            {
                return this._collides;
            }
            set
            {
                this._collides = value;
            }
        }

        /// <summary>
        /// Nodes must implement an Equals method
        /// </summary>
        /// <param name="obj">Object to compare against</param>
        /// <returns></returns>
        public abstract override bool Equals(object obj);

        /// <summary>
        /// Nodes must implement a ToString method
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Essential for the implementation of GetHashCode to work correctly, Nodes should generate a String representation that is 'unique' as far as that is possible.
        /// </para>
        /// <para>
        /// Any two Nodes which match via the Equals method (based on strict RDF Specification Equality) should produce the same String representation since Hash Codes are generated by calling GetHashCode on this String
        /// </para>
        /// </remarks>
        public abstract override string ToString();

        /// <summary>
        /// Gets a Hash Code for a Node
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Implemented by getting the Hash Code of the result of ToString for a Node prefixed with its Node Type, this is pre-computed for efficiency when a Node is created since Nodes are immutable.  See remarks on ToString for more detail.
        /// </para>
        /// <para>
        /// Since Hash Codes are based on a String representation there is no guarantee of uniqueness though the same Triple will always give the same Hash Code (on a given Platform - see the MSDN Documentation for <see cref="System.String.GetHashCode">System.String.GetHashCode()</see> for further details)
        /// </para>
        /// </remarks>
        public override int GetHashCode()
        {
            return this._hashcode;
        }

        /// <summary>
        /// The Equality operator is defined for Nodes
        /// </summary>
        /// <param name="a">First Node</param>
        /// <param name="b">Second Node</param>
        /// <returns>Whether the two Nodes are equal</returns>
        /// <remarks>Uses the Equals method to evaluate the result</remarks>
        public static bool operator ==(BaseNode a, BaseNode b)
        {
            if (((Object)a) == null)
            {
                if (((Object)b) == null) return true;
                return false;
            }
            else
            {
                return a.Equals(b);
            }
        }

        /// <summary>
        /// The Non-Equality operator is defined for Nodes
        /// </summary>
        /// <param name="a">First Node</param>
        /// <param name="b">Second Node</param>
        /// <returns>Whether the two Nodes are non-equal</returns>
        /// <remarks>Uses the Equals method to evaluate the result</remarks>
        public static bool operator !=(BaseNode a, BaseNode b)
        {
            if (((Object)a) == null)
            {
                if (((Object)b) == null) return false;
                return true;
            }
            else
            {
                return !a.Equals(b);
            }
        }

        /// <summary>
        /// Nodes must implement a CompareTo method to allow them to be Sorted
        /// </summary>
        /// <param name="other">Node to compare self to</param>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should use the SPARQL Term Sort Order for ordering nodes (as opposed to value sort order)
        /// </remarks>
        public abstract int CompareTo(INode other);

        /// <summary>
        /// Nodes must implement an Equals method so we can do type specific equality
        /// </summary>
        /// <param name="other">Node to check for equality</param>
        /// <returns></returns>
        /// <remarks>
        /// Nodes implementations are also required to implement an override of the non-generic Equals method
        /// </remarks>
        public abstract bool Equals(INode other);
    }
}
