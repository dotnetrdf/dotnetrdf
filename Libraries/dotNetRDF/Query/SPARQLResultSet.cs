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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Serialization;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Represents the type of the SPARQL Results Set
    /// </summary>
    public enum SparqlResultsType
    {
        /// <summary>
        /// The Result Set represents a Boolean Result
        /// </summary>
        Boolean,
        /// <summary>
        /// The Result Set represents a set of Variable Bindings
        /// </summary>
        VariableBindings,
        /// <summary>
        /// The Result Set represents an unknown result i.e. it has yet to be filled with Results
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Class for representing Sparql Result Sets
    /// </summary>
#if !NETCORE
    [Serializable,XmlRoot(ElementName="resultSet")]
#endif
    public sealed class SparqlResultSet 
        : IEnumerable<SparqlResult>, IDisposable
#if !NETCORE
        , ISerializable, IXmlSerializable
#endif
    {
        /// <summary>
        /// Lists of Sparql Results
        /// </summary>
        private List<SparqlResult> _results = new List<SparqlResult>();
        /// <summary>
        /// Lists of Variables in the Result Set
        /// </summary>
        private List<String> _variables = new List<string>();
        /// <summary>
        /// Boolean Result
        /// </summary>
        private bool _result = false;
        private SparqlResultsType _type = SparqlResultsType.Unknown;

#if !NETCORE
        private ResultSetDeserializationInfo _dsInfo;
#endif

        /// <summary>
        /// Creates an Empty Sparql Result Set
        /// </summary>
        /// <remarks>Useful where you need a possible guarentee of returning an result set even if it proves to be empty and also necessary for the implementation of Result Set Parsers.</remarks>
        public SparqlResultSet()
        {
            // No actions needed
        }

        /// <summary>
        /// Creates a Sparql Result Set for the Results of an ASK Query with the given Result value
        /// </summary>
        /// <param name="result"></param>
        public SparqlResultSet(bool result)
        {
            _result = result;
            _type = SparqlResultsType.Boolean;
        }

        /// <summary>
        /// Creates a Sparql Result Set for the collection of results
        /// </summary>
        /// <param name="results">Results</param>
        public SparqlResultSet(IEnumerable<SparqlResult> results)
        {
            _type = SparqlResultsType.VariableBindings;
            _results = results.ToList();
 
            if (_results.Any())
            {
                _variables = _results.First().Variables.ToList();
            }
        }
 
        /// <summary>
        /// Creates a SPARQL Result Set for the Results of a Query with the Leviathan Engine
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        public SparqlResultSet(SparqlEvaluationContext context)
        {
            _type = (context.Query.QueryType == SparqlQueryType.Ask) ? SparqlResultsType.Boolean : SparqlResultsType.VariableBindings;
            if (context.OutputMultiset is NullMultiset)
            {
                _result = false;
            }
            else if (context.OutputMultiset is IdentityMultiset)
            {
                _result = true;
                foreach (String var in context.OutputMultiset.Variables)
                {
                    _variables.Add(var);
                }
            }
            else
            {
                _result = true;
                foreach (String var in context.OutputMultiset.Variables)
                {
                    _variables.Add(var);
                }
                foreach (ISet s in context.OutputMultiset.Sets)
                {
                    AddResult(new SparqlResult(s, context.OutputMultiset.Variables));
                }
            }
        }

#if !NETCORE
        private SparqlResultSet(SerializationInfo info, StreamingContext context)
        {
            _dsInfo = new ResultSetDeserializationInfo(info, context);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (_dsInfo != null) _dsInfo.Apply(this);
        }

#endif

        #region Properties

        /// <summary>
        /// Gets the Type of the Results Set
        /// </summary>
        public SparqlResultsType ResultsType
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Gets the Result of an ASK Query
        /// </summary>
        /// <remarks>Result Set is deemed to refer to an ASK query if the Variables list is empty since an ASK Query result has an empty &lt;head&gt;.  It is always true for any other Query type where one/more variables were requested even if the Result Set is empty.</remarks>
        public bool Result
        {
            get
            {
                // If No Variables then must have been an ASK Query with an empty <head>
                if (_variables.Count == 0)
                {
                    // In this case the _result field contains the boolean result
                    return _result;
                }
                else
                {
                    // In any other case then it will contain true even if the result set was empty
                    return true;
                }
            }
        }

        /// <summary>
        /// Gets the number of Results in the Result Set
        /// </summary>
        public int Count
        {
            get
            {
                return _results.Count;
            }
        }

        /// <summary>
        /// Gets whether the Result Set is empty and can have Results loaded into it
        /// </summary>
        /// <remarks>
        /// </remarks>
        public bool IsEmpty
        {
            get
            {
                switch (_type)
                {
                    case SparqlResultsType.Boolean:
                        return false;
                    case SparqlResultsType.Unknown:
                        return true;
                    case SparqlResultsType.VariableBindings:
                        return _results.Count == 0;
                    default:
                        return true;
                }
            }
        }

        /// <summary>
        /// Gets the List of Results
        /// </summary>
        public List<SparqlResult> Results
        {
            get
            {
                return _results;
            }
        }

        /// <summary>
        /// Index directly into the Results
        /// </summary>
        /// <param name="index">Index of the Result you wish to retrieve</param>
        /// <returns></returns>
        public SparqlResult this[int index]
        {
            get
            {
                return _results[index];
            }
        }

        /// <summary>
        /// Gets the Variables used in the Result Set
        /// </summary>
        /// <remarks>
        /// As of 1.0 where possible dotNetRDF tries to preserve the ordering of variables however this may not be possible depending on where the result set originates from or how it is populated
        /// </remarks>
        public IEnumerable<String> Variables
        {
            get
            {
                return (from v in _variables
                        select v);
            }
        }

        #endregion

        /// <summary>
        /// Trims the Result Set to remove unbound variables from results
        /// </summary>
        /// <remarks>
        /// <strong>Note: </strong> This does not remove empty results this only removes unbound variables from individual results
        /// </remarks>
        public void Trim()
        {
            _results.ForEach(r => r.Trim());
        }

        #region Internal Methods for filling the ResultSet

        /// <summary>
        /// Adds a Variable to the Result Set
        /// </summary>
        /// <param name="var">Variable Name</param>
        internal void AddVariable(String var)
        {
            if (!_variables.Contains(var))
            {
                _variables.Add(var);
            }
            _type = SparqlResultsType.VariableBindings;
        }

        /// <summary>
        /// Adds a Result to the Result Set
        /// </summary>
        /// <param name="result">Result</param>
        internal void AddResult(SparqlResult result)
        {
            if (_type == SparqlResultsType.Boolean) throw new RdfException("Cannot add a Variable Binding Result to a Boolean Result Set");
            _results.Add(result);
            _type = SparqlResultsType.VariableBindings;
        }

        /// <summary>
        /// Sets the Boolean Result for the Result Set
        /// </summary>
        /// <param name="result">Boolean Result</param>
        internal void SetResult(bool result)
        {
            if (_type != SparqlResultsType.Unknown) throw new RdfException("Cannot set the Boolean Result value for this Result Set as its Result Type has already been set");
            _result = result;
            if (_type == SparqlResultsType.Unknown) _type = SparqlResultsType.Boolean;
        }

        #endregion

        #region Enumerator

        /// <summary>
        /// Gets an Enumerator for the Results List
        /// </summary>
        /// <returns></returns>
        public IEnumerator<SparqlResult> GetEnumerator()
        {
            return _results.GetEnumerator();
        }

        /// <summary>
        /// Gets an Enumerator for the Results List
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _results.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Determines whether two Result Sets are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <remarks>
        /// Experimental and not yet complete
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is SparqlResultSet results)
            {
                // Must contain same number of Results to be equal
                if (Count != results.Count) return false;

                // Must have same Boolean result to be equal
                if (Result != results.Result) return false;

                // Must contain the same set of variables
                if (Variables.Count() != results.Variables.Count()) return false;
                if (!Variables.All(v => results.Variables.Contains(v))) return false;
                if (results.Variables.Any(v => !_variables.Contains(v))) return false;

                // If both have no results then they are equal
                if (Count == 0 && results.Count == 0) return true;

                // All Ground Results from the Result Set must appear in the Other Result Set
                List<SparqlResult> otherResults = results.OrderByDescending(r => r.Variables.Count()).ToList();
                List<SparqlResult> localResults = new List<SparqlResult>();
                int grCount = 0;
                foreach (SparqlResult result in Results.OrderByDescending(r => r.Variables.Count()))
                {
                    if (result.IsGroundResult)
                    {
                        // If a Ground Result in this Result Set is not in the other Result Set we're not equal
                        if (!otherResults.Remove(result)) return false;
                        grCount++;
                    }
                    else
                    {
                        localResults.Add(result);
                    }
                }

                // If all the Results were ground results and we've emptied all the Results from the other Result Set
                // then we were equal
                if (Count == grCount && otherResults.Count == 0) return true;

                // If the Other Results still contains Ground Results we're not equal
                if (otherResults.Any(r => r.IsGroundResult)) return false;

                // Create Graphs of the two sets of non-Ground Results
                SparqlResultSet local = new SparqlResultSet();
                SparqlResultSet other = new SparqlResultSet();
                foreach (String var in _variables)
                {
                    local.AddVariable(var);
                    other.AddVariable(var);
                }
                foreach (SparqlResult r in localResults) 
                {
                    local.AddResult(r);
                }
                foreach (SparqlResult r in otherResults)
                {
                    other.AddResult(r);
                }

                // Compare the two Graphs for equality
                SparqlRdfWriter writer = new SparqlRdfWriter();
                IGraph g = writer.GenerateOutput(local);
                IGraph h = writer.GenerateOutput(other);
                return g.Equals(h);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Converts a Result Set into a Triple Collection
        /// </summary>
        /// <param name="g">Graph to generate the Nodes in</param>
        /// <returns></returns>
        /// <remarks>
        /// Assumes the Result Set contains three variables ?s, ?p and ?o to use as the Subject, Predicate and Object respectively.  Only Results for which all three variables have bound values will generate Triples
        /// </remarks>
        public BaseTripleCollection ToTripleCollection(IGraph g)
        {
            return ToTripleCollection(g, "s", "p", "o");
        }

        /// <summary>
        /// Converts a Result Set into a Triple Collection
        /// </summary>
        /// <param name="g">Graph to generate the Nodes in</param>
        /// <param name="subjVar">Variable whose value should be used for Subjects of Triples</param>
        /// <param name="predVar">Variable whose value should be used for Predicates of Triples</param>
        /// <param name="objVar">Variable whose value should be used for Object of Triples</param>
        /// <returns></returns>
        /// <remarks>
        /// Only Results for which all three variables have bound values will generate Triples
        /// </remarks>
        public BaseTripleCollection ToTripleCollection(IGraph g, String subjVar, String predVar, String objVar)
        {
            BaseTripleCollection tripleCollection = new TreeIndexedTripleCollection();

            foreach (SparqlResult r in Results)
            {
                // Must have values available for all three variables
                if (r.HasValue(subjVar) && r.HasValue(predVar) && r.HasValue(objVar))
                {
                    // None of the values is allowed to be unbound (i.e. null)
                    if (r[subjVar] == null || r[predVar] == null || r[objVar] == null) continue;

                    // If this is all OK we can generate a Triple
                    tripleCollection.Add(new Triple(r[subjVar].CopyNode(g), r[predVar].CopyNode(g), r[objVar].CopyNode(g)));
                }
            }

            return tripleCollection;
        }

        /// <summary>
        /// Disposes of a Result Set
        /// </summary>
        public void Dispose()
        {
            _results.Clear();
            _variables.Clear();
            _result = false;
        }

#if !NETCORE

        #region Serialization

        /// <summary>
        /// Gets the data for serialization
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("type", _type);
            info.AddValue("variables", _variables);
            info.AddValue("results", _results);
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
            switch (_type)
            {
                case SparqlResultsType.Boolean:
                    writer.WriteStartElement("boolean");
                    writer.WriteValue(_result);
                    writer.WriteEndElement();
                    break;

                case SparqlResultsType.VariableBindings:
                    writer.WriteStartElement("variables");
                    foreach (String var in _variables)
                    {
                        writer.WriteStartElement("variable");
                        writer.WriteValue(var);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteStartElement("results");
                    foreach (SparqlResult r in _results)
                    {
                        r.SerializeResult(writer);
                    }
                    writer.WriteEndElement();
                    break;

                default:
                    throw new NotSupportedException("Cannot serialize a SparqlResultSet which has not been filled with results");
            }
        }

        /// <summary>
        /// Reads the data for XML deserialization (.Net serialization not the official SPARQL results serialization)
        /// </summary>
        /// <param name="reader">XML Reader</param>
        public void ReadXml(XmlReader reader)
        {
            reader.Read();
            switch (reader.Name)
            {
                case "boolean":
                    _type = SparqlResultsType.Boolean;
                    _result = reader.ReadElementContentAsBoolean();
                    break;

                case "variables":
                    _type = SparqlResultsType.VariableBindings;
                    _result = true;
                    reader.Read();
                    while (reader.Name.Equals("variable"))
                    {
                        _variables.Add(reader.ReadElementContentAsString());
                    }
                    reader.Read();
                    if (reader.Name.Equals("results"))
                    {
                        reader.Read();
                        while (reader.Name.Equals("result"))
                        {
                            _results.Add(reader.DeserializeResult());
                        }
                    }
                    else
                    {
                        throw new RdfParseException("Unable to deserialize a SparqlResultSet as did not get the expected <results> element after the <variables> element");
                    }
                    break;

                default:
                    throw new RdfParseException("Unable to deserialize a SparqlResultSet, expected a <boolean> or <results> element after the <resultSet> element");
            }
        }

        #endregion

#endif
    }
}
