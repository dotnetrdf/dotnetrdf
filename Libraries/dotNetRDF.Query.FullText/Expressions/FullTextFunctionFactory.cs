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
using System.Text;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// A SPARQL Expression Factory reserved as a future extension point but not currently compiled or used
    /// </summary>
    public class FullTextFunctionFactory
        : ISparqlCustomExpressionFactory
    {

        /// <summary>
        /// Tries to create an expression
        /// </summary>
        /// <param name="u">Function URI</param>
        /// <param name="args">Arguments</param>
        /// <param name="scalarArguments">Scalar Arguments</param>
        /// <param name="expr">Resulting SPARQL Expression</param>
        /// <returns>True if a SPARQL Expression could be created, False otherwise</returns>
        public bool TryCreateExpression(Uri u, List<ISparqlExpression> args, Dictionary<string, ISparqlExpression> scalarArguments, out ISparqlExpression expr)
        {
            //TODO: Add support for FullTextMatchFunction and FullTextSearchFunction

            //switch (u.ToString())
            //{

            //}

            expr = null;
            return false;
        }

        /// <summary>
        /// Gets the URIs of available extension functions
        /// </summary>
        public IEnumerable<Uri> AvailableExtensionFunctions
        {
            get 
            {
                return Enumerable.Empty<Uri>(); 
            }
        }

        /// <summary>
        /// Gets the URIs of available extension aggregates
        /// </summary>
        public IEnumerable<Uri> AvailableExtensionAggregates
        {
            get 
            {
                return Enumerable.Empty<Uri>();
            }
        }
    }
}
