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

namespace VDS.RDF.Query.FullText.Schema
{
    /// <summary>
    /// Interface for Index Schemas
    /// </summary>
    /// <remarks>
    /// Index Schemas are used to provide the set of field names that is used to encode indexed data onto a document in the index
    /// </remarks>
    public interface IFullTextIndexSchema
    {
        /// <summary>
        /// Gets the field in which the full text is indexed
        /// </summary>
        String IndexField
        {
            get;
        }

        /// <summary>
        /// Gets the field in which the Graph URI is indexed
        /// </summary>
        String GraphField
        {
            get;
        }

        /// <summary>
        /// Gets the field in which the hash is stored
        /// </summary>
        /// <remarks>
        /// Used for unindexing
        /// </remarks>
        String HashField
        {
            get;
        }

        /// <summary>
        /// Gets the field in which the Node type is stored
        /// </summary>
        String NodeTypeField
        {
            get;
        }

        /// <summary>
        /// Gets the field in which the Node value is stored
        /// </summary>
        String NodeValueField
        {
            get;
        }

        /// <summary>
        /// Gets the field in which the Node meta is stored
        /// </summary>
        String NodeMetaField
        {
            get;
        }
    }
}
