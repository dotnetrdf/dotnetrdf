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
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Writing.Formatting;
#if !SILVERLIGHT
using VDS.RDF.Writing.Serialization;
#endif

namespace VDS.RDF.Query
{
    /// <summary>
    /// Class for representing a Row of a Sparql Result Set
    /// </summary>
#if !SILVERLIGHT
    [Serializable,XmlRoot(ElementName="result")]
#endif
    public sealed class SparqlResult 
        : IEnumerable<KeyValuePair<String, INode>>
#if !SILVERLIGHT
        , ISerializable, IXmlSerializable
#endif
    {
        private Dictionary<String, INode> _resultValues = new Dictionary<string, INode>();

        /// <summary>
        /// Creates a new empty SPARQL Result which can only be filled by methods internal to the dotNetRDF Library
        /// </summary>
        public SparqlResult()
        { }

        /// <summary>
        /// Creates a new SPARQL Result from the given Set
        /// </summary>
        /// <param name="s">Set</param>
        public SparqlResult(ISet s)
        {
            foreach (String var in s.Variables)
            {
                this._resultValues.Add(var, s[var]);
            }
        }

#if !SILVERLIGHT
        private SparqlResult(SerializationInfo info, StreamingContext context)
        {
            this._resultValues = (Dictionary<String,INode>)info.GetValue("bindings", typeof(Dictionary<String, INode>));
        }
#endif

        /// <summary>
        /// Gets the Value that is bound to the given Variable
        /// </summary>
        /// <param name="variable">Variable whose Value you wish to retrieve</param>
        /// <returns></returns>
        /// <exception cref="RdfException">Thrown if there is nothing bound to the given Variable Name for this Result</exception>
        public INode Value(string variable)
        {
            if (this._resultValues.ContainsKey(variable))
            {
                return this._resultValues[variable];
            }
            else
            {
                throw new RdfException("This result does not have any value bound to the variable '" + variable + "'");
            }
        }

        /// <summary>
        /// Gets the Value that is bound to the given Variable
        /// </summary>
        /// <param name="variable">Variable whose Value you wish to retrieve</param>
        /// <returns></returns>
        /// <exception cref="RdfException">Thrown if there is nothing bound to the given Variable Name for this Result</exception>
        public INode this[string variable]
        {
            get
            {
                return this.Value(variable);
            }
        }

        /// <summary>
        /// Gets the Value that is bound at the given Index
        /// </summary>
        /// <param name="index">Index whose Value you wish to retrieve</param>
        /// <returns></returns>
        /// <remarks>
        /// The order of variables in a Result is not guaranteed in any way
        /// </remarks>
        /// <exception cref="IndexOutOfRangeException">Thrown if there is nothing bound at the given Index</exception>
        public INode this[int index]
        {
            get
            {
                if (index < 0 || index >= this._resultValues.Count)
                {
                    throw new IndexOutOfRangeException("There is no Result Value at Index " + index);
                }
                else
                {
                    return this._resultValues.Values.ElementAt(index);
                }
            }
        }

        /// <summary>
        /// Gets the number of Variables for which this Result contains Bindings
        /// </summary>
        public int Count
        {
            get
            {
                return this._resultValues.Count;
            }
        }

        /// <summary>
        /// Internal Only Method for setting the Value of a Result
        /// </summary>
        /// <param name="variable">Variable Name</param>
        /// <param name="value">Value bound to the Variable</param>
        protected internal void SetValue(string variable, INode value)
        {
            if (this._resultValues.ContainsKey(variable))
            {
                this._resultValues[variable] = value;
            }
            else
            {
                this._resultValues.Add(variable, value);
            }
        }

        /// <summary>
        /// Checks whether a value is bound to the given Variable for this result
        /// </summary>
        /// <param name="variable">Variable Name</param>
        /// <returns></returns>
        public bool HasValue(string variable)
        {
            return this._resultValues.ContainsKey(variable);
        }

        /// <summary>
        /// Gets the set of Variables that are bound in this Result
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return (from v in this._resultValues.Keys
                        select v);
            }
        }

        /// <summary>
        /// Gets whether a Result is a Ground Result
        /// </summary>
        /// <remarks>
        /// A <strong>Ground Result</strong> is a result which is considered to be a fixed fact.  In practise this means it contains no Blank Nodes
        /// </remarks>
        public bool IsGroundResult
        {
            get
            {
                return this._resultValues.Values.All(n => n == null || n.NodeType != NodeType.Blank);
            }
        }

        /// <summary>
        /// Removes all Variables Bindings where the Variable is Unbound
        /// </summary>
        public void Trim()
        {
            foreach (String var in this._resultValues.Keys.ToList())
            {
                if (this._resultValues[var] == null)
                {
                    this._resultValues.Remove(var);
                }
            }
        }

        /// <summary>
        /// Displays the Result as a comma separated string of pairs of the form ?var = value
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();

            if (this._resultValues.Count == 0) return "<Empty Result>";

            foreach (String var in this._resultValues.Keys)
            {
                output.Append("?");
                output.Append(var);
                output.Append(" = ");
                if (this._resultValues[var] != null)
                {
                    output.Append(this._resultValues[var].ToString());
                }

                output.Append(" , ");
            }

            String outString = output.ToString();
            if (outString.Length > 3)
            {
                return outString.Substring(0, outString.Length - 3);
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Displays the Result as a comma separated string of paris of the form ?var = value where values are formatted using the given Node Formatter
        /// </summary>
        /// <param name="formatter">Node Formatter</param>
        /// <returns></returns>
        public String ToString(INodeFormatter formatter)
        {
            StringBuilder output = new StringBuilder();

            if (this._resultValues.Count == 0) return "<Empty Result>";

            foreach (String var in this._resultValues.Keys)
            {
                output.Append("?");
                output.Append(var);
                output.Append(" = ");
                if (this._resultValues[var] != null)
                {
                    output.Append(this._resultValues[var].ToString(formatter));
                }

                output.Append(" , ");
            }

            String outString = output.ToString();
            if (outString.Length > 3)
            {
                return outString.Substring(0, outString.Length - 3);
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Override of the Equals method for Results
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <remarks>Used implicitly in applying Distinct and Reduced modifiers to the Result Set</remarks>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else if (obj is SparqlResult)
            {
                SparqlResult other = (SparqlResult)obj;

                //Empty Results are only equal to Empty Results
                if (this._resultValues.Count == 0 && other._resultValues.Count == 0) return true;
                if (this._resultValues.Count == 0 || other._resultValues.Count == 0) return false;

                //For differing numbers of values we must contain all the same values for variables
                //bound in both or the variable missing from us must be bound to null in the other
                foreach (String v in other.Variables)
                {
                    if (this._resultValues.ContainsKey(v))
                    {
                        if (this._resultValues[v] == null && other[v] != null)
                        {
                            return false;
                        }
                        else if (this._resultValues[v] == null && other[v] == null)
                        {
                            continue;
                        }
                        else if (!this._resultValues[v].Equals(other[v]))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (other[v] != null) return false;
                    }
                }
                return true;

            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Override of the GetHashCode method for Results
        /// </summary>
        /// <returns></returns>
        /// <remarks>Used implicitly in applying Distinct and Reduced modifiers to the Result Set</remarks>
        public override int GetHashCode()
        {
            StringBuilder output = new StringBuilder();

            foreach (String var in this._resultValues.Keys)
            {
                output.Append("?");
                output.Append(var);
                output.Append(" = ");
                if (!(this._resultValues[var] == null))
                {
                    output.Append(this._resultValues[var].NodeType);
                    output.Append(this._resultValues[var].ToString());
                }

                output.Append(" , ");
            }

            String outString = output.ToString();
            return outString.GetHashCode();

        }

        #region IEnumerable<KeyValuePair<string,INode>> Members

        /// <summary>
        /// Enumerates the Bindings of Variable Names to Values in this Result
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, INode>> GetEnumerator()
        {
            return this._resultValues.GetEnumerator();
        }

        /// <summary>
        /// Enumerates the Bindings of Variable Names to Values in this Result
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this._resultValues.GetEnumerator();
        }

        #endregion

#if !SILVERLIGHT
        #region Serialization

        /// <summary>
        /// Gets the data for serialization
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("bindings", this._resultValues);
        }

        /// <summary>
        /// Gets the schema for XML serialization
        /// </summary>
        /// <returns></returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Writes the data for XML serialization (.Net serialization not the official SPARQL results serialization)
        /// </summary>
        /// <param name="writer">XML Writer</param>
        public void WriteXml(XmlWriter writer)
        {
            //writer.WriteStartElement("bindings");
            foreach (KeyValuePair<String, INode> binding in this._resultValues)
            {
                writer.WriteStartElement("binding");
                writer.WriteAttributeString("name", binding.Key);
                binding.Value.SerializeNode(writer);
                writer.WriteEndElement();
            }
            //writer.WriteEndElement();
        }

        /// <summary>
        /// Reads the data for XML deserialization (.Net serialization not the official SPARQL results serialization)
        /// </summary>
        /// <param name="reader">XML Reader</param>
        public void ReadXml(XmlReader reader)
        {
            //<result> may be empty
            if (reader.IsEmptyElement) return;

            //Otherwise expect some values
            reader.Read();
            while (reader.Name.Equals("binding"))
            {
                //Get the attribute name
                reader.MoveToAttribute("name");
                String var = reader.Value;
                reader.MoveToElement();

                if (reader.IsEmptyElement)
                {
                    //May be empty indicating a null
                    this._resultValues.Add(var, null);
                }
                else
                {
                    //Otherwise expect a deserializable node
                    reader.Read();
                    INode value = reader.DeserializeNode();
                    this._resultValues.Add(var, value);
                }
                //Read to the next binding
                reader.Read();
            }
            reader.Read();
        }

        #endregion
#endif
    }
}
