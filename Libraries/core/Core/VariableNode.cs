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
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract Base Class for Variable Nodes
    /// </summary>
#if !SILVERLIGHT
    [Serializable,XmlRoot(ElementName="variable")]
#endif
    public abstract class BaseVariableNode
        : BaseNode, IVariableNode, IEquatable<BaseVariableNode>, IComparable<BaseVariableNode>
    {
        private String _var;

        /// <summary>
        /// Creates a new Variable Node
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="varname">Variable Name</param>
        protected internal BaseVariableNode(IGraph g, String varname)
            : base(g, NodeType.Variable)
        {
            if (varname.StartsWith("?") || varname.StartsWith("$"))
            {
                this._var = varname.Substring(1);
            }
            else
            {
                this._var = varname;
            }
            this._hashcode = (this._nodetype + this.ToString()).GetHashCode();
        }

        /// <summary>
        /// Deserialization Only Constructor
        /// </summary>
        protected BaseVariableNode()
            : base(null, NodeType.Variable) { }

#if !SILVERLIGHT

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected BaseVariableNode(SerializationInfo info, StreamingContext context)
            : base(null, NodeType.Variable)
        {
            this._var = info.GetString("name");
            this._hashcode = (this._nodetype + this.ToString()).GetHashCode();
        }

#endif

        /// <summary>
        /// Gets the Variable Name
        /// </summary>
        public String VariableName
        {
            get
            {
                return this._var;
            }
        }

        /// <summary>
        /// Gets whether this Node is equal to some other Node
        /// </summary>
        /// <param name="other">Node to test</param>
        /// <returns></returns>
        public override bool Equals(INode other)
        {
            if ((Object)other == null) return false;

            if (ReferenceEquals(this, other)) return true;

            if (other.NodeType == NodeType.Variable)
            {
                return EqualityHelper.AreVariablesEqual(this, (IVariableNode)other);
            }
            else
            {
                //Can only be equal to other Variables
                return false;
            }
        }

        /// <summary>
        /// Determines whether this Node is equal to a Blank Node (should always be false)
        /// </summary>
        /// <param name="other">Blank Node</param>
        /// <returns></returns>
        public override bool Equals(IBlankNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            return false;
        }

        /// <summary>
        /// Determines whether this Node is equal to a Graph Literal Node (should always be false)
        /// </summary>
        /// <param name="other">Graph Literal Node</param>
        /// <returns></returns>
        public override bool Equals(IGraphLiteralNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            return false;
        }

        /// <summary>
        /// Determines whether this Node is equal to a Literal Node (should always be false)
        /// </summary>
        /// <param name="other">Literal Node</param>
        /// <returns></returns>
        public override bool Equals(ILiteralNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            return false;
        }

        /// <summary>
        /// Determines whether this Node is equal to a URI Node (should always be false)
        /// </summary>
        /// <param name="other">URI Node</param>
        /// <returns></returns>
        public override bool Equals(IUriNode other)
        {
            if (ReferenceEquals(this, other)) return true;
            return false;
        }

        /// <summary>
        /// Determines whether this Node is equal to a Variable Node
        /// </summary>
        /// <param name="other">Variable Node</param>
        /// <returns></returns>
        public override bool Equals(IVariableNode other)
        {
            if ((Object)other == null) return false;

            if (ReferenceEquals(this, other)) return true;

            return EqualityHelper.AreVariablesEqual(this, other);
        }

        /// <summary>
        /// Determines whether this Node is equal to a Variable Node
        /// </summary>
        /// <param name="other">Variable Node</param>
        /// <returns></returns>
        public bool Equals(BaseVariableNode other)
        {
            return this.Equals((IVariableNode)other);
        }

        /// <summary>
        /// Gets whether this Node is equal to some Object
        /// </summary>
        /// <param name="obj">Object to test</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (ReferenceEquals(this, obj)) return true;

            if (obj is INode)
            {
                return this.Equals((INode)obj);
            }
            else
            {
                //Can only be equal to other Nodes
                return false;
            }
        }

        /// <summary>
        /// Gets the String representation of this Node
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "?" + this._var;
        }

        /// <summary>
        /// Compares this Node to another Node
        /// </summary>
        /// <param name="other">Node to compare with</param>
        /// <returns></returns>
        public override int CompareTo(INode other)
        {
            if (ReferenceEquals(this, other)) return 0;

            if (other == null)
            {
                //Variables are considered greater than null
                return 1;
            }
            else if (other.NodeType == NodeType.Variable)
            {
                return this.CompareTo((IVariableNode)other);
            }
            else
            {
                //Variable Nodes are less than everything else
                return -1;
            }
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public override int CompareTo(IBlankNode other)
        {
            if (ReferenceEquals(this, other)) return 0;

            if (other == null)
            {
                //Variables are considered greater than null
                return 1;
            }
            else
            {
                //Variable Nodes are less than everything else
                return -1;
            }
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public override int CompareTo(IGraphLiteralNode other)
        {
            if (ReferenceEquals(this, other)) return 0;

            if (other == null)
            {
                //Variables are considered greater than null
                return 1;
            }
            else
            {
                //Variable Nodes are less than everything else
                return -1;
            }
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public override int CompareTo(ILiteralNode other)
        {
            if (ReferenceEquals(this, other)) return 0;

            if (other == null)
            {
                //Variables are considered greater than null
                return 1;
            }
            else
            {
                //Variable Nodes are less than everything else
                return -1;
            }
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public override int CompareTo(IUriNode other)
        {
            if (ReferenceEquals(this, other)) return 0;

            if (other == null)
            {
                //Variables are considered greater than null
                return 1;
            }
            else
            {
                //Variable Nodes are less than everything else
                return -1;
            }
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public override int CompareTo(IVariableNode other)
        {
            if (ReferenceEquals(this, other)) return 0;

            if (other == null)
            {
                //Variables are considered greater than null
                return 1;
            }
            else
            {
                return ComparisonHelper.CompareVariables(this, other);
            }
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public int CompareTo(BaseVariableNode other)
        {
            return this.CompareTo((IVariableNode)other);
        }

#if !SILVERLIGHT

        /// <summary>
        /// Gets the data for serialization
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        public sealed override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", this._var);
        }

        /// <summary>
        /// Reads the data for XML deserialization
        /// </summary>
        /// <param name="reader">XML Reader</param>
        public sealed override void ReadXml(XmlReader reader)
        {
            this._var = reader.ReadElementContentAsString();
            this._hashcode = (this._nodetype + this.ToString()).GetHashCode();
        }

        /// <summary>
        /// Writes the data for XML serialization
        /// </summary>
        /// <param name="writer">XML Writer</param>
        public sealed override void WriteXml(XmlWriter writer)
        {
            writer.WriteValue(this._var);
        }

#endif
    }

    /// <summary>
    /// Class representing Variable Nodes (only used for N3)
    /// </summary>
#if !SILVERLIGHT
    [Serializable,XmlRoot(ElementName="variable")]
#endif
    public class VariableNode
        : BaseVariableNode, IEquatable<VariableNode>, IComparable<VariableNode>
    {
        /// <summary>
        /// Creates a new Variable Node
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="varname">Variable Name</param>
        protected internal VariableNode(IGraph g, String varname)
            : base(g, varname) { }

        /// <summary>
        /// Deserialization Only Constructor
        /// </summary>
        protected VariableNode()
            : base() { }

#if !SILVERLIGHT
        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected VariableNode(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
#endif

        /// <summary>
        /// Compares this Node to another Variable Node
        /// </summary>
        /// <param name="other">Variable Node</param>
        /// <returns></returns>
        public int CompareTo(VariableNode other)
        {
            return base.CompareTo((IVariableNode)other);
        }

        /// <summary>
        /// Determines whether this Node is equal to a Variable Node
        /// </summary>
        /// <param name="other">Variable Node</param>
        /// <returns></returns>
        public bool Equals(VariableNode other)
        {
            return base.Equals((IVariableNode)other);
        }
    }
}
