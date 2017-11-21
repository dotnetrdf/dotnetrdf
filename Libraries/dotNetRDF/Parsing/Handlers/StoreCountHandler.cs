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

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler which simply counts the Triples and Graphs
    /// </summary>
    public class StoreCountHandler : BaseRdfHandler
    {
        private int _counter = 0;
        private HashSet<String> _graphs;

        /// <summary>
        /// Creates a new Store Count Handler
        /// </summary>
        public StoreCountHandler()
            : base(new NodeFactory()) { }

        /// <summary>
        /// Starts RDF Handling by reseting the counters
        /// </summary>
        protected override void StartRdfInternal()
        {
            _counter = 0;
            _graphs = new HashSet<string>();
        }

        /// <summary>
        /// Handles Triples/Quads by counting the Triples and distinct Graph URIs
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            _counter++;
            _graphs.Add(t.GraphUri.ToSafeString());
            return true;
        }

        /// <summary>
        /// Gets the count of Triples
        /// </summary>
        public int TripleCount
        {
            get
            {
                return _counter;
            }
        }

        /// <summary>
        /// Gets the count of distinct Graph URIs
        /// </summary>
        public int GraphCount
        {
            get 
            {
                return _graphs.Count;
            }
        }

        /// <summary>
        /// Gets that this Handler accepts all Triples
        /// </summary>
        public override bool AcceptsAll
        {
            get 
            {
                return true; 
            }
        }
    }
}
