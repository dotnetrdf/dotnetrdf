/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using VDS.Common.Collections;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Writing.Formatting;
using VDS.RDF.Writing.Serialization;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Class for representing a Row of a Sparql Result Set
    /// </summary>
#if !NETCORE
    [Serializable,XmlRoot(ElementName="result")]
#endif
    public sealed class SparqlResult 
        : IEnumerable<KeyValuePair<String, INode>>
#if !NETCORE
        , ISerializable, IXmlSerializable
#endif
    {
        private List<String> _variables = new List<string>();
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
            foreach (var var in s.Variables)
            {
                _variables.Add(var);
                _resultValues.Add(var, s[var]);
            }
        }

        /// <summary>
        /// Creates a new SPARQL Result from the given Set which contains only the given variables in the given order
        /// </summary>
        /// <param name="s">Set</param>
        /// <param name="variables">Variables</param>
        public SparqlResult(ISet s, IEnumerable<String> variables)
        {
            _variables.AddRange(variables);
            foreach (var var in _variables)
            {
                _resultValues.Add(var, s[var]);
            }
        }

#if !NETCORE
        /// <summary>
        /// Deserialization only constructor
        /// </summary>
        /// <param name="info">Serialization Info</param>
        /// <param name="context">Streaming Context</param>
        private SparqlResult(SerializationInfo info, StreamingContext context)
        {
            _resultValues = (Dictionary<String,INode>)info.GetValue("bindings", typeof(Dictionary<String, INode>));
            _variables = new List<string>(_resultValues.Keys);
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
            if (_resultValues.ContainsKey(variable))
            {
                return _resultValues[variable];
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
                return Value(variable);
            }
        }

        /// <summary>
        /// Gets the Value that is bound at the given Index
        /// </summary>
        /// <param name="index">Index whose Value you wish to retrieve</param>
        /// <returns></returns>
        /// <remarks>
        /// As of 1.0.0 the order of variables in a result may/may not vary depending on the original query.  If a specific variable list was declared dotNetRDF tries to preserve that order but this may not always happen depending on how results are received.
        /// </remarks>
        /// <exception cref="IndexOutOfRangeException">Thrown if there is nothing bound at the given Index</exception>
        public INode this[int index]
        {
            get
            {
                if (index < 0 || index >= _variables.Count)
                {
                    throw new IndexOutOfRangeException("There is no variable at Index " + index);
                }
                else
                {
                    return _resultValues[_variables[index]];
                }
            }
        }

        /// <summary>
        /// Tries to get a value (which may be null) for the variable
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <param name="value">Value</param>
        /// <returns>True if the variable was present (even it was unbound) and false otherwise</returns>
        public bool TryGetValue(String variable, out INode value)
        {
            if (HasValue(variable))
            {
                value = this[variable];
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to get a non-null value for the variable
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <param name="value">Value</param>
        /// <returns>True if the variable was present and bound, false otherwise</returns>
        public bool TryGetBoundValue(String variable, out INode value)
        {
            if (HasValue(variable))
            {
                value = this[variable];
                return value != null;
            }
            else
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Gets the number of Variables for which this Result contains Bindings
        /// </summary>
        public int Count
        {
            get
            {
                return _resultValues.Count;
            }
        }

        /// <summary>
        /// Internal Only Method for setting the Value of a Result
        /// </summary>
        /// <param name="variable">Variable Name</param>
        /// <param name="value">Value bound to the Variable</param>
        internal void SetValue(string variable, INode value)
        {
            if (_resultValues.ContainsKey(variable))
            {
                _resultValues[variable] = value;
            }
            else
            {
                _variables.Add(variable);
                _resultValues.Add(variable, value);
            }
        }

        /// <summary>
        /// Sets the variable ordering for the result
        /// </summary>
        /// <param name="variables"></param>
        internal void SetVariableOrdering(IEnumerable<String> variables)
        {
            // Validate that the ordering is applicable
            if (variables.Count() < _variables.Count) throw new RdfQueryException("Cannot set a variable ordering that contains less variables then are currently specified");
            foreach (var var in _variables)
            {
                if (!variables.Contains(var)) throw new RdfQueryException("Cannot set a variable ordering that omits the variable ?" + var + " currently present in this result");
            }
            // Apply ordering
            _variables = new List<string>(variables);
        }

        /// <summary>
        /// Checks whether a given Variable has a value (which may be null) for this result
        /// </summary>
        /// <param name="variable">Variable Name</param>
        /// <returns>True if the variable is present, false otherwise</returns>
        /// <remarks>Returns true even if the value is null, use <see cref="SparqlResult.HasBoundValue"/> instead to see whether a non-null value is present for a variable.</remarks>
        public bool HasValue(string variable)
        {
            return _resultValues.ContainsKey(variable);
        }

        /// <summary>
        /// Checks whether a given Variable has a non-null value for this result
        /// </summary>
        /// <param name="variable">Variable Name</param>
        /// <returns>True if the variable is present and has a non-null value, false otherwise</returns>
        public bool HasBoundValue(String variable)
        {
            return _resultValues.ContainsKey(variable) && _resultValues[variable] != null;
        }

        /// <summary>
        /// Gets the set of Variables that are bound in this Result
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return new ImmutableView<String>(_variables);
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
                return _resultValues.Values.All(n => n == null || n.NodeType != NodeType.Blank);
            }
        }

        /// <summary>
        /// Removes all Variables Bindings where the Variable is Unbound
        /// </summary>
        public void Trim()
        {
            foreach (var var in _resultValues.Keys.ToList())
            {
                if (_resultValues[var] == null)
                {
                    _resultValues.Remove(var);
                }
            }
        }

        /// <summary>
        /// Displays the Result as a comma separated string of pairs of the form ?var = value
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var output = new StringBuilder();

            if (_resultValues.Count == 0) return "<Empty Result>";

            foreach (var var in _variables)
            {
                output.Append("?");
                output.Append(var);
                output.Append(" = ");
                if (_resultValues.TryGetValue(var, out var value) && value != null)
                {
                    output.Append(value.ToString());
                }

                output.Append(" , ");
            }

            var outString = output.ToString();
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
            var output = new StringBuilder();

            if (_resultValues.Count == 0) return "<Empty Result>";

            foreach (var var in _variables)
            {
                output.Append("?");
                output.Append(var);
                output.Append(" = ");
                if (_resultValues.TryGetValue(var, out var value) && value != null)
                {
                    output.Append(value.ToString(formatter));
                }
                output.Append(" , ");
            }

            var outString = output.ToString();
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
            if (obj is SparqlResult other)
            {
                // Empty Results are only equal to Empty Results
                if (_resultValues.Count == 0 && other._resultValues.Count == 0) return true;
                if (_resultValues.Count == 0 || other._resultValues.Count == 0) return false;

                // For differing numbers of values we must contain all the same values for variables
                // bound in both or the variable missing from us must be bound to null in the other
                foreach (var v in other.Variables)
                {
                    if (_resultValues.ContainsKey(v))
                    {
                        if (_resultValues[v] == null && other[v] != null)
                        {
                            return false;
                        }
                        else if (_resultValues[v] == null && other[v] == null)
                        {
                            continue;
                        }
                        else if (!_resultValues[v].Equals(other[v]))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (other.HasBoundValue(v)) return false;
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
            var output = new StringBuilder();

            foreach (var var in _resultValues.Keys.OrderBy(v => v))
            {
                output.Append("?");
                output.Append(var);
                output.Append(" = ");
                if (!(_resultValues[var] == null))
                {
                    output.Append(_resultValues[var].NodeType);
                    output.Append(_resultValues[var].ToString());
                }

                output.Append(" , ");
            }

            var outString = output.ToString();
            return outString.GetHashCode();

        }

        #region IEnumerable<KeyValuePair<string,INode>> Members

        /// <summary>
        /// Enumerates the Bindings of Variable Names to Values in this Result
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Does not respect the ordering of the variables (if any)
        /// </remarks>
        public IEnumerator<KeyValuePair<string, INode>> GetEnumerator()
        {
            return _resultValues.GetEnumerator();
        }

        /// <summary>
        /// Enumerates the Bindings of Variable Names to Values in this Result
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _resultValues.GetEnumerator();
        }

        #endregion

#if !NETCORE
        #region Serialization

        /// <summary>
        /// Gets the data for serialization
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("bindings", _resultValues);
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
            // writer.WriteStartElement("bindings");
            foreach (KeyValuePair<String, INode> binding in _resultValues)
            {
                writer.WriteStartElement("binding");
                writer.WriteAttributeString("name", binding.Key);
                binding.Value.SerializeNode(writer);
                writer.WriteEndElement();
            }
            // writer.WriteEndElement();
        }

        /// <summary>
        /// Reads the data for XML deserialization (.Net serialization not the official SPARQL results serialization)
        /// </summary>
        /// <param name="reader">XML Reader</param>
        public void ReadXml(XmlReader reader)
        {
            // <result> may be empty
            if (reader.IsEmptyElement) return;

            // Otherwise expect some values
            reader.Read();
            while (reader.Name.Equals("binding"))
            {
                // Get the attribute name
                reader.MoveToAttribute("name");
                String var = reader.Value;
                reader.MoveToElement();

                if (reader.IsEmptyElement)
                {
                    // May be empty indicating a null
                    _resultValues.Add(var, null);
                }
                else
                {
                    // Otherwise expect a deserializable node
                    reader.Read();
                    INode value = reader.DeserializeNode();
                    _resultValues.Add(var, value);
                }
                // Read to the next binding
                reader.Read();
            }
            reader.Read();
        }

        #endregion
#endif
    }
}
