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
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Represents the SPARQL UUID Function
    /// </summary>
    public class UUIDFunction
        : BaseUUIDFunction
    {
        /// <summary>
        /// Evaluates the function by generating the URN UUID form based on the given UUID
        /// </summary>
        /// <param name="uuid">UUID</param>
        /// <returns></returns>
        protected override IValuedNode EvaluateInternal(Guid uuid)
        {
            return new UriNode(null, new Uri("urn:uuid:" + uuid.ToString()));
        }

        /// <summary>
        /// Gets the functor for the expression
        /// </summary>
        public override string Functor => SparqlSpecsHelper.SparqlKeywordUUID;

        /// <inheritdoc />
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordUUID + "()";
        }
    }

    /// <summary>
    /// Represents the SPARQL STRUUID Function
    /// </summary>
    public class StrUUIDFunction
        : BaseUUIDFunction
    {
        /// <summary>
        /// Evaluates the function by returning the string form of the given UUID
        /// </summary>
        /// <param name="uuid">UUID</param>
        /// <returns></returns>
        protected override IValuedNode EvaluateInternal(Guid uuid)
        {
            return new StringNode(null, uuid.ToString());
        }

        /// <summary>
        /// Gets the functor for the expression
        /// </summary>
        public override string Functor => SparqlSpecsHelper.SparqlKeywordStrUUID;

        /// <inheritdoc />
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordStrUUID + "()";
        }
    }
}
