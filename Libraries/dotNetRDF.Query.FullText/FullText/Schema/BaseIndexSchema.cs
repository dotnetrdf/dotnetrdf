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
    /// Abstract Base Implementation of an Index Schema
    /// </summary>
    public abstract class BaseIndexSchema
        : IFullTextIndexSchema
    {
        /// <summary>
        /// Gets the field in which the full text is indexed
        /// </summary>
        public string IndexField
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the field in which the Graph URI is indexed
        /// </summary>
        public String GraphField
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the field in which the hash is stored
        /// </summary>
        public String HashField
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the field in which the Node type is stored
        /// </summary>
        public string NodeTypeField
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the field in which the Node value is stored
        /// </summary>
        public string NodeValueField
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the field in which the Node meta is stored
        /// </summary>
        public string NodeMetaField
        {
            get;
            protected set;
        }
    }
}
