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
    using System.Dynamic;
    using System.Linq.Expressions;

    /// <summary>
    /// A <see cref="WrapperGraph">wrapper</see> that provides read/write dictionary and dynamic functionality.
    /// </summary>
    public partial class DynamicGraph : WrapperGraph, IDynamicMetaObjectProvider
    {
        private readonly Uri subjectBaseUri;
        private readonly Uri predicateBaseUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicGraph"/> class.
        /// </summary>
        /// <param name="graph">The <see cref="IGraph"/> to wrap.</param>
        /// <param name="subjectBaseUri">The <see cref="Uri"/> used for resolving relative subject references.</param>
        /// <param name="predicateBaseUri">The <see cref="Uri"/> used for resolving relative predicate references.</param>
        public DynamicGraph(IGraph graph = null, Uri subjectBaseUri = null, Uri predicateBaseUri = null)
            : base(graph ?? new Graph())
        {
            this.subjectBaseUri = subjectBaseUri;
            this.predicateBaseUri = predicateBaseUri;
        }

        /// <summary>
        /// Gets the <see cref="Uri"/> used for resolving relative subject references.
        /// </summary>
        public Uri SubjectBaseUri
        {
            get
            {
                return this.subjectBaseUri ?? this.BaseUri;
            }
        }

        /// <summary>
        /// Gets the URI used for resolving relative predicate references.
        /// </summary>
        public Uri PredicateBaseUri
        {
            get
            {
                return this.predicateBaseUri ?? this.SubjectBaseUri;
            }
        }

        /// <inheritdoc/>
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new DictionaryMetaObject(parameter, this);
        }
    }
}
