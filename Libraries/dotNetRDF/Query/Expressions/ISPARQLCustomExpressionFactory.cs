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

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Interface for implementing SPARQL custom expression factories which turn URI specified functions into SPARQL Expressions
    /// </summary>
    public interface ISparqlCustomExpressionFactory
    {
        /// <summary>
        /// Tries to Create a SPARQL Expression for a function with the given URI and set of arguments
        /// </summary>
        /// <param name="u">URI of the function</param>
        /// <param name="args">List of Arguments</param>
        /// <param name="scalarArguments">Dictionary of Scalar Arguments which are supportable by aggregates when Syntax is set to SPARQL 1.1 Extended</param>
        /// <param name="expr">Resulting Expression if able to generate</param>
        /// <returns>True if an expression is generated, false if not</returns>
        bool TryCreateExpression(Uri u, List<ISparqlExpression> args, Dictionary<String,ISparqlExpression> scalarArguments, out ISparqlExpression expr);

        /// <summary>
        /// Gets the Extension Function URIs that this Factory provides
        /// </summary>
        IEnumerable<Uri> AvailableExtensionFunctions
        {
            get;
        }

        /// <summary>
        /// Gets the Extension Aggregate URIs that this Factory provides
        /// </summary>
        IEnumerable<Uri> AvailableExtensionAggregates
        {
            get;
        }
    }
}
