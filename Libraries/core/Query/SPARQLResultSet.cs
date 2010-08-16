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
using System.Xml;
using System.IO;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Aggregates;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Writing;

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
        /// The Result Set represents an unknown result
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Class for representing Sparql Result Sets
    /// </summary>
    public sealed class SparqlResultSet : IEnumerable<SparqlResult>, IDisposable
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
        /// <summary>
        /// Indicates whether the Result Set is Empty and can have Results safely loaded into it
        /// </summary>
        private bool _empty = true;
        private SparqlResultsType _type = SparqlResultsType.Unknown;

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
            this._empty = false;
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
                foreach (Set s in context.OutputMultiset.Sets)
                {
                    this._results.Add(new SparqlResult(s));
                }
            }
        }

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
        protected internal bool IsEmpty
        {
            get
            {
                return this._empty;
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

        #region Internal Methods for filling the ResultSet

        /// <summary>
        /// Adds a Variable to the Result Set
        /// </summary>
        /// <param name="var">Variable Name</param>
        protected internal void AddVariable(String var)
        {
            this._variables.Add(var);
            this._type = SparqlResultsType.VariableBindings;
        }

        /// <summary>
        /// Adds a Result to the Result Set
        /// </summary>
        /// <param name="result">Result</param>
        protected internal void AddResult(SparqlResult result)
        {
            this._results.Add(result);
            this._type = SparqlResultsType.VariableBindings;
        }

        /// <summary>
        /// Sets the Boolean Result for the Result Set
        /// </summary>
        /// <param name="result">Boolean Result</param>
        protected internal void SetResult(bool result)
        {
            this._result = result;
            if (this._type == SparqlResultsType.Unknown) this._type = SparqlResultsType.Boolean;
        }

        /// <summary>
        /// Sets whether the Result Set is empty
        /// </summary>
        /// <param name="empty">Whether the Result Set is empty</param>
        protected internal void SetEmpty(bool empty)
        {
            this._empty = empty;
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
                Graph g = writer.GenerateOutput(local);
                Graph h = writer.GenerateOutput(other);
                return g.Equals(h);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Disposes of a Result Set
        /// </summary>
        public void Dispose()
        {
            this._results.Clear();
            this._variables.Clear();
            this._result = false;
            this._empty = true;
        }
    }
}
