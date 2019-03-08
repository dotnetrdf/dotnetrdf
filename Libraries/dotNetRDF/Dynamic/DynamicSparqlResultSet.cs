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

namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq.Expressions;
    using VDS.RDF.Query;

    /// <summary>
    /// Provides dynamic functionality for <see cref="SparqlResultSet">SPARQL result sets</see>.
    /// </summary>
    public class DynamicSparqlResultSet : IEnumerable<DynamicSparqlResult>, IDynamicMetaObjectProvider
    {
        private readonly SparqlResultSet original;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicSparqlResultSet"/> class.
        /// </summary>
        /// <param name="original">The SPARQL result set to wrap.</param>
        public DynamicSparqlResultSet(SparqlResultSet original)
        {
            if (original is null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            this.original = original;
        }

        /// <summary>
        /// Returns an enumerator that iterates through dynamic results in the set.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through dynamic results in the set.</returns>
        public IEnumerator<DynamicSparqlResult> GetEnumerator()
        {
            foreach (var result in this.original)
            {
                yield return new DynamicSparqlResult(result);
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc/>
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new EnumerableMetaObject(parameter, this);
        }
    }
}
