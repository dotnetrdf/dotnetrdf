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
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Ordering
{
    /// <summary>
    /// Interface for classes that represent SPARQL ORDER BY clauses
    /// </summary>
    /// <remarks>A SPARQL Order By clause provides a list of orderings, when parsed into the dotNetRDF model this is represented as a single <see cref="ISparqlOrderBy">ISparqlOrderBy</see> for the first term in the clause chained to <see cref="ISparqlOrderBy">ISparqlOrderBy</see>'s for each subsequent term via the <see cref="ISparqlOrderBy.Child">Child</see> property.</remarks>
    public interface ISparqlOrderBy : IComparer<ISet>
    {
        /// <summary>
        /// Gets/Sets the Child Ordering that applies if the two Objects are considered equal
        /// </summary>
        ISparqlOrderBy Child
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the Evaluation Context for the Order By
        /// </summary>
        SparqlEvaluationContext Context
        {
            set;
        }

        /// <summary>
        /// Sets whether the Ordering is Descending
        /// </summary>
        bool Descending
        {
            get;
            set;
        }

        /// <summary>
        /// Gets whether the Ordering is simple (i.e. applies on variables only)
        /// </summary>
        bool IsSimple
        {
            get;
        }

        /// <summary>
        /// Gets all the Variables used in the Ordering
        /// </summary>
        IEnumerable<String> Variables
        {
            get;
        }

        /// <summary>
        /// Gets the Expression used to do the Ordering
        /// </summary>
        ISparqlExpression Expression
        {
            get;
        }

        /// <summary>
        /// Generates a Comparer than can be used to do Ordering based on the given Triple Pattern
        /// </summary>
        /// <param name="pattern">Triple Pattern</param>
        /// <returns></returns>
        IComparer<Triple> GetComparer(IMatchTriplePattern pattern);
    }
}
