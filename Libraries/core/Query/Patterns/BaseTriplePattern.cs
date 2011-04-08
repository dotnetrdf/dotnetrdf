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

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Base class for representing all types of Triple Patterns in SPARQL queries
    /// </summary>
    public abstract class BaseTriplePattern : ITriplePattern
    {
        /// <summary>
        /// Stores the Index Type for the Triple Pattern
        /// </summary>
        protected TripleIndexType _indexType = TripleIndexType.None;
        /// <summary>
        /// Stores the list of variables that are used in the Pattern
        /// </summary>
        protected List<String> _vars = new List<string>();

        /// <summary>
        /// Evaluates the Triple Pattern in the given Evaluation Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public abstract void Evaluate(SparqlEvaluationContext context);

        /// <summary>
        /// Gets the Index Type we will use for this Pattern
        /// </summary>
        public TripleIndexType IndexType
        {
            get 
            {
                return this._indexType;
            }
        }

        /// <summary>
        /// Returns whether the Triple Pattern is an accept all
        /// </summary>
        public abstract bool IsAcceptAll
        {
            get;
        }

        /// <summary>
        /// Gets the List of Variables used in the Pattern
        /// </summary>
        /// <remarks>
        /// These are sorted in alphabetical order
        /// </remarks>
        public List<string> Variables
        {
            get 
            { 
                return this._vars;
            }
        }

        /// <summary>
        /// Compares a Triple Pattern to another Triple Pattern
        /// </summary>
        /// <param name="other">Other Triple Pattern</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// The aim of this function is to sort Triple Patterns into what is hopefully an optimal order such that during execution the query space is restricted as early as possible.
        /// </para>
        /// <para>
        /// The basic rules of this are as follows:
        /// <ol>
        ///     <li>Patterns with fewer variables should be executed first</li>
        ///     <li>Patterns using the same variables should be executed in sequence</li>
        ///     <li>Patterns using indexes which are considered more useful should be executed first</li>
        /// </ol>
        /// </para>
        /// </remarks>
        public virtual int CompareTo(ITriplePattern other)
        {
            if (this._vars.Count < other.Variables.Count)
            {
                //We have fewer variables so we go before the other pattern
                return -1;
            }
            else if (this._vars.Count > other.Variables.Count)
            {
                //We have more variables so we go after the other pattern
                return 1;
            }
            else
            {
                if (this._vars.Count > 0)
                {
                    for (int i = 0; i < this._vars.Count; i++)
                    {
                        int c = this._vars[i].CompareTo(other.Variables[i]);
                        if (c < 0)
                        {
                            //Our variables occur alphabetically sooner than the other patterns so we go before the other pattern
                            return -1;
                        }
                        else if (c > 0)
                        {
                            //Other variables occur alphabetically sooner than ours so we go after
                            return 1;
                        }
                        //Otherwise we continue checking
                    }

                    //If we reach this point then we contain the same variables
                    //Now we order based on our Index Types
                    TripleIndexSorter sorter = new TripleIndexSorter();
                    return sorter.Compare(this.IndexType, other.IndexType);
                }
                else
                {
                    //Neither pattern has any variables so we consider these to be equivalent
                    //Order of these patterns has no effect
                    return 0;
                }
            }
        }

        /// <summary>
        /// Gets whether a Triple Pattern is Thread Safe when evaluated
        /// </summary>
        /// <remarks>
        /// Almost all Triple Patterns are Thread Safe unless they are subquery patterns which themselves are not thread safe
        /// </remarks>
        public virtual bool UsesDefaultDataset
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the String representation of the Pattern
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();
    }
}
