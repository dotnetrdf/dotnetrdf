/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Abstract Base Class for Variable Nodes
    /// </summary>
#if !SILVERLIGHT
    [Serializable,XmlRoot(ElementName="variable")]
#endif
    public abstract class BaseVariableNode
        : BaseNode, IEquatable<BaseVariableNode>, IComparable<BaseVariableNode>, IValuedNode
    {
        /// <summary>
        /// Creates a new Variable Node
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="varname">Variable Name</param>
        protected internal BaseVariableNode(String varname)
            : base(NodeType.Variable)
        {
            if (varname.StartsWith("?") || varname.StartsWith("$"))
            {
                this.VariableName = varname.Substring(1);
            }
            else
            {
                this.VariableName = varname;
            }
            this._hashcode = Tools.CreateHashCode(this);
        }

#if !SILVERLIGHT

        /// <summary>
        /// Deserialization Only Constructor
        /// </summary>
        protected BaseVariableNode()
            : base(NodeType.Variable) { }

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        protected BaseVariableNode(SerializationInfo info, StreamingContext context)
            : base(NodeType.Variable)
        {
            this.VariableName = info.GetString("name");
            this._hashcode = Tools.CreateHashCode(this);
        }

#endif

        /// <summary>
        /// Gets the Variable Name
        /// </summary>
        public override String VariableName { get; protected set; }

        /// <summary>
        /// Gets whether this Node is equal to some other Node
        /// </summary>
        /// <param name="other">Node to test</param>
        /// <returns></returns>
        public override bool Equals(INode other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;

            //Can only be equal to other Variables
            return other.NodeType == NodeType.Variable && EqualityHelper.AreVariablesEqual(this, other);
        }

        /// <summary>
        /// Determines whether this Node is equal to a Variable Node
        /// </summary>
        /// <param name="other">Variable Node</param>
        /// <returns></returns>
        public bool Equals(BaseVariableNode other)
        {
            return this.Equals((INode)other);
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

            //Can only be equal to other Nodes
            if (obj is INode)
            {
                return this.Equals((INode)obj);
            }
            return false;
        }

        /// <summary>
        /// Gets the String representation of this Node
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "?" + this.VariableName;
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
            if (other.NodeType == NodeType.Variable)
            {
                return ComparisonHelper.CompareVariables(this, other);
            }
            //Variable Nodes are less than everything else
            return -1;
        }

        /// <summary>
        /// Returns an Integer indicating the Ordering of this Node compared to another Node
        /// </summary>
        /// <param name="other">Node to test against</param>
        /// <returns></returns>
        public int CompareTo(BaseVariableNode other)
        {
            return this.CompareTo((INode)other);
        }

#if !SILVERLIGHT

        /// <summary>
        /// Gets the data for serialization
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        public sealed override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", this.VariableName);
        }

        /// <summary>
        /// Reads the data for XML deserialization
        /// </summary>
        /// <param name="reader">XML Reader</param>
        public sealed override void ReadXml(XmlReader reader)
        {
            this.VariableName = reader.ReadElementContentAsString();
            this._hashcode = Tools.CreateHashCode(this);
        }

        /// <summary>
        /// Writes the data for XML serialization
        /// </summary>
        /// <param name="writer">XML Writer</param>
        public sealed override void WriteXml(XmlWriter writer)
        {
            writer.WriteValue(this.VariableName);
        }

#endif

        #region IValuedNode Members

        /// <summary>
        /// Throws an error as variables cannot be converted to types
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            throw new NodeValueException("Cannot cast Variable Nodes to types");
        }

        /// <summary>
        /// Throws an error as variables cannot be converted to types
        /// </summary>
        /// <returns></returns>
        public long AsInteger()
        {
            throw new NodeValueException("Cannot cast Variable Nodes to types");
        }

        /// <summary>
        /// Throws an error as variables cannot be converted to types
        /// </summary>
        /// <returns></returns>
        public decimal AsDecimal()
        {
            throw new NodeValueException("Cannot cast Variable Nodes to types");
        }

        /// <summary>
        /// Throws an error as variables cannot be converted to types
        /// </summary>
        /// <returns></returns>
        public float AsFloat()
        {
            throw new NodeValueException("Cannot cast Variable Nodes to types");
        }

        /// <summary>
        /// Throws an error as variables cannot be converted to types
        /// </summary>
        /// <returns></returns>
        public double AsDouble()
        {
            throw new NodeValueException("Cannot cast Variable Nodes to types");
        }

        /// <summary>
        /// Throws an error as variables cannot be converted to types
        /// </summary>
        /// <returns></returns>
        public bool AsBoolean()
        {
            throw new NodeValueException("Cannot cast Variable Nodes to types");
        }

        /// <summary>
        /// Throws an error as variables cannot be converted to types
        /// </summary>
        /// <returns></returns>
        public DateTime AsDateTime()
        {
            throw new NodeValueException("Cannot cast Variable Nodes to types");
        }

        /// <summary>
        /// Throws an error as variables cannot be converted to types
        /// </summary>
        /// <returns></returns>
        public DateTimeOffset AsDateTimeOffset()
        {
            throw new NodeValueException("Cannot cast Variable Nodes to types");
        }

        /// <summary>
        /// Throws an error as variables cannot be cast to a time span
        /// </summary>
        /// <returns></returns>
        public TimeSpan AsTimeSpan()
        {
            throw new NodeValueException("Cannot cast Variable Nodes to a types");
        }

        /// <summary>
        /// Gets the URI of the datatype this valued node represents as a String
        /// </summary>
        public String EffectiveType
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the numeric type of the expression
        /// </summary>
        public EffectiveNumericType NumericType
        {
            get
            {
                return EffectiveNumericType.NaN;
            }
        }

        #endregion
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
        public VariableNode(String varname)
            : base(varname) { }

#if !SILVERLIGHT
        /// <summary>
        /// Deserialization Only Constructor
        /// </summary>
        protected VariableNode()
            : base() { }

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
            return base.CompareTo((INode)other);
        }

        /// <summary>
        /// Determines whether this Node is equal to a Variable Node
        /// </summary>
        /// <param name="other">Variable Node</param>
        /// <returns></returns>
        public bool Equals(VariableNode other)
        {
            return base.Equals((INode)other);
        }
    }
}
