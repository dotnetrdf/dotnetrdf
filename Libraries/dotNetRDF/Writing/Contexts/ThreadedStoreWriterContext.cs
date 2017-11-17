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
using System.IO;
using System.Threading;

namespace VDS.RDF.Writing.Contexts
{
    /// <summary>
    /// Writer Context for Store Writers which do multi-threaded writing
    /// </summary>
    /// <remarks>
    /// Provides a queue for queuing the URIs of Graphs from the Store that need writing and a thread safe way of retrieving the next Uri to be written from the Queue
    /// </remarks>
    public class ThreadedStoreWriterContext : BaseStoreWriterContext
    {
        private Queue<Uri> _writeList = new Queue<Uri>();
        private NamespaceMapper _nsmapper = new NamespaceMapper();
        private ThreadSafeQNameOutputMapper _qnameMapper;

        /// <summary>
        /// Creates a new Threaded Store Writer Context with default settings
        /// </summary>
        /// <param name="store">Store to be written</param>
        /// <param name="output">TextWriter to write to</param>
        public ThreadedStoreWriterContext(ITripleStore store, TextWriter output)
            : base(store, output) { }

        /// <summary>
        /// Creates a new Threaded Store Writer Context with custom settings
        /// </summary>
        /// <param name="store">Store to be written</param>
        /// <param name="output">TextWriter to write to</param>
        /// <param name="prettyPrint">Pretty Print Mode</param>
        /// <param name="hiSpeedAllowed">High Speed Mode</param>
        public ThreadedStoreWriterContext(ITripleStore store, TextWriter output, bool prettyPrint, bool hiSpeedAllowed)
            : base(store, output, prettyPrint, hiSpeedAllowed) { }

        /// <summary>
        /// Gets the NamespaceMap used for reducing URIs to QNames since there may only be one shared map written to the output
        /// </summary>
        public NamespaceMapper NamespaceMap
        {
            get
            {
                return _nsmapper;
            }
        }

        /// <summary>
        /// Gets the QName Mapper
        /// </summary>
        /// <remarks>
        /// Must be manually initialised by the user
        /// </remarks>
        public ThreadSafeQNameOutputMapper QNameMapper
        {
            get
            {
                return _qnameMapper;
            }
            set
            {
                _qnameMapper = value;
            }
        }

        /// <summary>
        /// Adds a Uri to the list of URIs for Graphs that are waiting to be written
        /// </summary>
        /// <param name="u"></param>
        public void Add(Uri u)
        {
            _writeList.Enqueue(u);
        }

        /// <summary>
        /// Gets the next Uri for a Graph that is waiting to be written
        /// </summary>
        /// <returns>Uri of next Graph to be written</returns>
        public bool TryGetNextUri(out Uri uri)
        {
            uri = null;
            bool ok = false;
            try
            {
                Monitor.Enter(_writeList);
                if (_writeList.Count > 0)
                {
                    uri = _writeList.Dequeue();
                    ok = true;
                }
            }
            finally
            {
                Monitor.Exit(_writeList);
            }
            return ok;
        }
    }
}
