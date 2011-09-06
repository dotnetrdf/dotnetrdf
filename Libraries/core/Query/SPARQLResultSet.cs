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
#if !NO_DATA
using System.Data;
#endif
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Aggregates;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Writing;
#if !SILVERLIGHT
using VDS.RDF.Writing.Serialization;
#endif

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
#if !SILVERLIGHT
    [Serializable,XmlRoot(ElementName="resultSet")]
#endif
    public sealed class SparqlResultSet 
        : IEnumerable<SparqlResult>, IDisposable
#if !SILVERLIGHT
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

#if !SILVERLIGHT
        private ResultSetDeserializationInfo _dsInfo;
#endif

        /// <summary>
        /// Creates an Empty Sparql Result Set
        /// </summary>
        /// <remarks>Useful where you need a possible guarentee of returning an result set even if it proves to be empty and also necessary for the implementation of Result Set Parsers.</remarks>
        public SparqlResultSet()
        {
            //No actions needed
        }

        /// <summary>
        /// Creates a Sparql Result Set for the Results of an ASK Query with the given Result value
        /// </summary>
        /// <param name="result"></param>
        public SparqlResultSet(bool result)
        {
            this._result = result;
            this._type = SparqlResultsType.Boolean;
        }

        /// <summary>
        /// Creates a SPARQL Result Set for the Results of a Query with the Leviathan Engine
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        public SparqlResultSet(SparqlEvaluationContext context)
        {
            this._type = (context.Query.QueryType == SparqlQueryType.Ask) ? SparqlResultsType.Boolean : SparqlResultsType.VariableBindings;
            if (context.OutputMultiset is NullMultiset)
            {
                this._result = false;
            }
            else if (context.OutputMultiset is IdentityMultiset)
            {
                this._result = true;
                foreach (String var in context.OutputMultiset.Variables)
                {
                    this._variables.Add(var);
                }
            }
            else
            {
                this._result = true;
                foreach (String var in context.OutputMultiset.Variables)
                {
                    this._variables.Add(var);
                }
                foreach (ISet s in context.OutputMultiset.Sets)
                {
                    this.AddResult(new SparqlResult(s));
                }
            }
        }

#if !SILVERLIGHT
        private SparqlResultSet(SerializationInfo info, StreamingContext context)
        {
            this._dsInfo = new ResultSetDeserializationInfo(info, context);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (this._dsInfo != null) this._dsInfo.Apply(this);
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
                return this._type;
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
                //If No Variables then must have been an ASK Query with an empty <head>
                if (this._variables.Count == 0)
                {
                    //In this case the _result field contains the boolean result
                    return this._result;
                }
                else
                {
                    //In any other case then it will contain true even if the result set was empty
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
                return this._results.Count;
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
                switch (this._type)
                {
                    case SparqlResultsType.Boolean:
                        return false;
                    case SparqlResultsType.Unknown:
                        return true;
                    case SparqlResultsType.VariableBindings:
                        return this._results.Count == 0;
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
                return this._results;
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
                return this._results[index];
            }
        }

        /// <summary>
        /// Gets the Variables used in the Result Set
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return (from v in this._variables
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
            this._results.ForEach(r => r.Trim());
        }

        #region Internal Methods for filling the ResultSet

        /// <summary>
        /// Adds a Variable to the Result Set
        /// </summary>
        /// <param name="var">Variable Name</param>
        protected internal void AddVariable(String var)
        {
            if (!this._variables.Contains(var))
            {
                this._variables.Add(var);
            }
            this._type = SparqlResultsType.VariableBindings;
        }

        /// <summary>
        /// Adds a Result to the Result Set
        /// </summary>
        /// <param name="result">Result</param>
        protected internal void AddResult(SparqlResult result)
        {
            if (this._type == SparqlResultsType.Boolean) throw new RdfException("Cannot add a Variable Binding Result to a Boolean Result Set");
            this._results.Add(result);
            this._type = SparqlResultsType.VariableBindings;
        }

        /// <summary>
        /// Sets the Boolean Result for the Result Set
        /// </summary>
        /// <param name="result">Boolean Result</param>
        protected internal void SetResult(bool result)
        {
            if (this._type != SparqlResultsType.Unknown) throw new RdfException("Cannot set the Boolean Result value for this Result Set as its Result Type has already been set");
            this._result = result;
            if (this._type == SparqlResultsType.Unknown) this._type = SparqlResultsType.Boolean;
        }

        #endregion

        #region Enumerator

        /// <summary>
        /// Gets an Enumerator for the Results List
        /// </summary>
        /// <returns></returns>
        public IEnumerator<SparqlResult> GetEnumerator()
        {
            return this._results.GetEnumerator();
        }

        /// <summary>
        /// Gets an Enumerator for the Results List
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this._results.GetEnumerator();
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

            if (obj is SparqlResultSet)
            {
                SparqlResultSet results = (SparqlResultSet)obj;

                //Must contain same number of Results to be equal
                if (this.Count != results.Count) return false;

                //Must have same Boolean result to be equal
                if (this.Result != results.Result) return false;

                //Must contain the same set of variables
                if (this.Variables.Count() != results.Variables.Count()) return false;
                if (!this.Variables.All(v => results.Variables.Contains(v))) return false;
                if (results.Variables.Any(v => !this._variables.Contains(v))) return false;

                //If both have no results then they are equal
                if (this.Count == 0 && results.Count == 0) return true;

                //All Ground Results from the Result Set must appear in the Other Result Set
                List<SparqlResult> otherResults = results.OrderByDescending(r => r.Variables.Count()).ToList();
                List<SparqlResult> localResults = new List<SparqlResult>();
                int grCount = 0;
                foreach (SparqlResult result in this.Results.OrderByDescending(r => r.Variables.Count()))
                {
                    if (result.IsGroundResult)
                    {
                        //If a Ground Result in this Result Set is not in the other Result Set we're not equal
                        if (!otherResults.Remove(result)) return false;
                        grCount++;
                    }
                    else
                    {
                        localResults.Add(result);
                    }
                }

                //If all the Results were ground results and we've emptied all the Results from the other Result Set
                //then we were equal
                if (this.Count == grCount && otherResults.Count == 0) return true;

                //If the Other Results still contains Ground Results we're not equal
                if (otherResults.Any(r => r.IsGroundResult)) return false;

                //Create Graphs of the two sets of non-Ground Results
                SparqlResultSet local = new SparqlResultSet();
                SparqlResultSet other = new SparqlResultSet();
                foreach (String var in this._variables)
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

                //Compare the two Graphs for equality
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
            return this.ToTripleCollection(g, "s", "p", "o");
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
            IndexedTripleCollection tripleCollection = new IndexedTripleCollection();

            foreach (SparqlResult r in this.Results)
            {
                //Must have values available for all three variables
                if (r.HasValue(subjVar) && r.HasValue(predVar) && r.HasValue(objVar))
                {
                    //None of the values is allowed to be unbound (i.e. null)
                    if (r[subjVar] == null || r[predVar] == null || r[objVar] == null) continue;

                    //If this is all OK we can generate a Triple
                    tripleCollection.Add(new Triple(r[subjVar].CopyNode(g), r[predVar].CopyNode(g), r[objVar].CopyNode(g)));
                }
            }

            return tripleCollection;
        }

#if !NO_DATA
        /// <summary>
        /// Casts a SPARQL Result Set to a DataTable with all Columns typed as <see cref="INode">INode</see> (Results with unbound variables will have nulls in the appropriate columns of their <see cref="System.Data.DataRow">DataRow</see>)
        /// </summary>
        /// <param name="results">SPARQL Result Set</param>
        /// <returns></returns>
        /// <remarks>
        /// <strong>Warning:</strong> Not available under builds which remove the Data Storage layer from dotNetRDF e.g. Silverlight
        /// </remarks>
        public static explicit operator DataTable(SparqlResultSet results)
        {
            DataTable table = new DataTable();
            DataRow row;

            switch (results.ResultsType)
            {
                case SparqlResultsType.VariableBindings:
                    foreach (String var in results.Variables)
                    {
                        table.Columns.Add(new DataColumn(var, typeof(INode)));
                    }

                    foreach (SparqlResult r in results)
                    {
                        row = table.NewRow();

                        foreach (String var in results.Variables)
                        {
                            if (r.HasValue(var))
                            {
                                row[var] = r[var];
                            }
                            else
                            {
                                row[var] = null;
                            }
                        }
                        table.Rows.Add(row);
                    }
                    break;
                case SparqlResultsType.Boolean:
                    table.Columns.Add(new DataColumn("ASK", typeof(bool)));
                    row = table.NewRow();
                    row["ASK"] = results.Result;
                    table.Rows.Add(row);
                    break;

                case SparqlResultsType.Unknown:
                default:
                    throw new InvalidCastException("Unable to cast a SparqlResultSet to a DataTable as the ResultSet has yet to be filled with data and so has no SparqlResultsType which determines how it is cast to a DataTable");
            }
        
            return table;
        }
#endif

        /// <summary>
        /// Disposes of a Result Set
        /// </summary>
        public void Dispose()
        {
            this._results.Clear();
            this._variables.Clear();
            this._result = false;
        }

#if !SILVERLIGHT

        #region Serialization

        /// <summary>
        /// Gets the data for serialization
        /// </summary>
        /// <param name="info">Serialization Information</param>
        /// <param name="context">Streaming Context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("type", this._type);
            info.AddValue("variables", this._variables);
            info.AddValue("results", this._results);
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
            switch (this._type)
            {
                case SparqlResultsType.Boolean:
                    writer.WriteStartElement("boolean");
                    writer.WriteValue(this._result);
                    writer.WriteEndElement();
                    break;

                case SparqlResultsType.VariableBindings:
                    writer.WriteStartElement("variables");
                    foreach (String var in this._variables)
                    {
                        writer.WriteStartElement("variable");
                        writer.WriteValue(var);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteStartElement("results");
                    foreach (SparqlResult r in this._results)
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
                    this._type = SparqlResultsType.Boolean;
                    this._result = reader.ReadElementContentAsBoolean();
                    break;

                case "variables":
                    this._type = SparqlResultsType.VariableBindings;
                    this._result = true;
                    reader.Read();
                    while (reader.Name.Equals("variable"))
                    {
                        this._variables.Add(reader.ReadElementContentAsString());
                    }
                    reader.Read();
                    if (reader.Name.Equals("results"))
                    {
                        reader.Read();
                        while (reader.Name.Equals("result"))
                        {
                            this._results.Add(reader.DeserializeResult());
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
