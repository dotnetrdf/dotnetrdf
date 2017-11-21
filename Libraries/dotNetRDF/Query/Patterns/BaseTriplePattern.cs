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

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Base class for representing all types of Triple Patterns in SPARQL queries
    /// </summary>
    public abstract class BaseTriplePattern 
        : ITriplePattern
    {
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
        /// Returns whether the Triple Pattern is an accept all
        /// </summary>
        public abstract bool IsAcceptAll
        {
            get;
        }

        /// <summary>
        /// Gets the Triple Pattern Type
        /// </summary>
        public abstract TriplePatternType PatternType
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
                return _vars;
            }
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the pattern i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public abstract IEnumerable<String> FloatingVariables { get; }

        /// <summary>
        /// Gets the enumeration of fixed variables in the pattern i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public abstract IEnumerable<String> FixedVariables { get; }

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
            if (_vars.Count < other.Variables.Count)
            {
                // We have fewer variables so we go before the other pattern
                return -1;
            }
            if (_vars.Count > other.Variables.Count)
            {
                // We have more variables so we go after the other pattern
                return 1;
            }
            // Neither pattern has any variables so we consider these to be equivalent
            // Order of these patterns has no effect
            if (_vars.Count <= 0) return 0;

            for (int i = 0; i < _vars.Count; i++)
            {
                int c = String.Compare(_vars[i], other.Variables[i], StringComparison.Ordinal);
                if (c < 0)
                {
                    // Our variables occur alphabetically sooner than the other patterns so we go before the other pattern
                    return -1;
                }
                if (c > 0)
                {
                    // Other variables occur alphabetically sooner than ours so we go after
                    return 1;
                }
                // Otherwise we continue checking
            }

            // If we reach this point then we contain the same variables
            // Now we order based on our Index Types
            TriplePatternTypeComparer sorter = new TriplePatternTypeComparer();
            return sorter.Compare(PatternType, other.PatternType);
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
        /// Gets whether the Pattern has no blank variables
        /// </summary>
        public abstract bool HasNoBlankVariables
        {
            get;
        }

        /// <summary>
        /// Gets the String representation of the Pattern
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();
    }
}
