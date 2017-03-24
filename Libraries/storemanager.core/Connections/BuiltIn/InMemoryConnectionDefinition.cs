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

using System.ComponentModel;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    /// <summary>
    /// Definition for in-memory connections
    /// </summary>
    public class InMemoryConnectionDefinition
        : BaseConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        public InMemoryConnectionDefinition()
            : base("In-Memory", "Create a temporary non-persistent in-memory store for testing and experimentation purposes", typeof(InMemoryManager)) { }

        ///// <summary>
        ///// Gets/Sets whether Full Text Indexing is enabled
        ///// </summary>
        //[Connection(DisplayName = "Enable Full Text Indexing?", Type = ConnectionSettingType.Boolean), DefaultValue(false)]
        //public bool UseFullTextIndexing
        //{
        //    get;
        //    set;
        //}

        /// <summary>
        /// Opens the Connection
        /// </summary>
        /// <returns></returns>
        protected override IStorageProvider OpenConnectionInternal()
        {
            //if (this.UseFullTextIndexing)
            //{
            //    return new InMemoryManager(new Fu
            //}
            //else
            //{
                return new InMemoryManager();
            //}
        }

        /// <summary>
        /// Makes a copy of the current connection definition
        /// </summary>
        /// <returns>Copy of the connection definition</returns>
        public override IConnectionDefinition Copy()
        {
            InMemoryConnectionDefinition definition = new InMemoryConnectionDefinition();
            return definition;
        }

        public override string ToString()
        {
            return "[In-Memory]";
        }
    }
}
