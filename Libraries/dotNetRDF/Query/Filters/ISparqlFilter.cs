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
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Filters
{
    /// <summary>
    /// Interface for Classes which implement SPARQL Filter Functions
    /// </summary>
    public interface ISparqlFilter
    {
        /// <summary>
        /// Evaluates a Filter in the given Evaluation Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        void Evaluate(SparqlEvaluationContext context);

        /// <summary>
        /// Gets the enumeration of Variables that are used in the Filter
        /// </summary>
        IEnumerable<String> Variables
        {
            get;
        }

        /// <summary>
        /// Gets the Expression that this Filter uses
        /// </summary>
        ISparqlExpression Expression
        {
            get;
        }
    }
}
