/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;

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
                return this._nsmapper;
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
                return this._qnameMapper;
            }
            set
            {
                this._qnameMapper = value;
            }
        }

        /// <summary>
        /// Adds a Uri to the list of URIs for Graphs that are waiting to be written
        /// </summary>
        /// <param name="u"></param>
        public void Add(Uri u)
        {
            this._writeList.Enqueue(u);
        }

        /// <summary>
        /// Gets the next Uri for a Graph that is waiting to be written
        /// </summary>
        /// <returns>Uri of next Graph to be written</returns>
        public Uri GetNextUri()
        {
            Uri temp = null;
            try
            {
                Monitor.Enter(this._writeList);
                if (this._writeList.Count > 0)
                {
                    temp = this._writeList.Dequeue();
                }
            }
            finally
            {
                Monitor.Exit(this._writeList);
            }
            return temp;
        }
    }

#if !NO_STORAGE

    /// <summary>
    /// Writer Context for Store Writers which write using a <see cref="IGenericIOManager">IGenericIOManager</see>
    /// </summary>
    /// <remarks>
    /// Provides a queue for queuing the URIs of Graphs from the Store that need writing and a thread safe way of retrieving the next Uri to be written from the Queue
    /// </remarks>
    public class GenericStoreWriterContext : IStoreWriterContext
    {
        private Queue<Uri> _writeList = new Queue<Uri>();
        private GenericIOParams _params;
        private ITripleStore _store;

        /// <summary>
        /// Creates a new Generic Store Writer Context
        /// </summary>
        /// <param name="store">Triple Store that will be written to the underlying Store</param>
        /// <param name="parameters">Generic IO Parameters</param>
        public GenericStoreWriterContext(ITripleStore store, GenericIOParams parameters)
        {
            this._store = store;
            this._params = parameters;
        }

        /// <summary>
        /// Gets the Triple Store that is being written to the underlying Store
        /// </summary>
        public ITripleStore Store
        {
            get 
            {
                return this._store; 
            }
        }

        /// <summary>
        /// Gets the IO Manager for the underlying store
        /// </summary>
        public IGenericIOManager Manager
        {
            get
            {
                return this._params.Manager;
            }
        }

        /// <summary>
        /// Gets the number of Threads to use for operations that can be multi-threaded
        /// </summary>
        public int Threads
        {
            get
            {
                return this._params.Threads;
            }
        }

        /// <summary>
        /// Gets the Generic IO Parameters
        /// </summary>
        public GenericIOParams Parameters
        {
            get
            {
                return this._params;
            }
        }

        /// <summary>
        /// Adds a Uri to the list of URIs for Graphs that are waiting to be written
        /// </summary>
        /// <param name="u"></param>
        public void Add(Uri u)
        {
            this._writeList.Enqueue(u);
        }

        /// <summary>
        /// Gets the next Uri for a Graph that is waiting to be written
        /// </summary>
        /// <returns>Uri of next Graph to be written</returns>
        public Uri GetNextURI()
        {
            Uri temp = null;
            try
            {
                Monitor.Enter(this._writeList);
                if (this._writeList.Count > 0)
                {
                    temp = this._writeList.Dequeue();
                }
            }
            finally
            {
                Monitor.Exit(this._writeList);
            }
            return temp;
        }
    }

#endif
}
