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

#if !NO_DATA && !NO_STORAGE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Writing.Contexts
{
    /// <summary>
    /// Writer Context for Threaded SQL Store Writers
    /// </summary>
    public class ThreadedSqlStoreWriterContext : IStoreWriterContext
    {
        private ITripleStore _store;
        private int _writeThreads = 8;
        private Queue<IGraph> _writeQueue = new Queue<IGraph>();
        private IThreadedSqlIOManager _manager;
        private bool _clearIfExists = false;

        /// <summary>
        /// Creates a new Threaded SQL Store Writer Context
        /// </summary>
        /// <param name="store">Triple Store to save</param>
        /// <param name="params">Threaded SQL IO Parameters</param>
        public ThreadedSqlStoreWriterContext(ITripleStore store, ThreadedSqlIOParams @params)
            : this(store, @params.ThreadedManager, @params.Threads, @params.ClearIfExists) { }

        /// <summary>
        /// Creates a new Threaded SQL Store Writer Context
        /// </summary>
        /// <param name="store">Triple Store to save</param>
        /// <param name="manager">SQL IO Manager</param>
        /// <param name="threads">Number of threads to use</param>
        /// <param name="clearIfExists">Whether to clear graphs if they already exist</param>
        public ThreadedSqlStoreWriterContext(ITripleStore store, IThreadedSqlIOManager manager, int threads, bool clearIfExists)
        {
            this._store = store;
            this._manager = manager;
            this._writeThreads = threads;
            this._clearIfExists = clearIfExists;
        }

        /// <summary>
        /// Creates a new Threaded SQL Store Writer Context
        /// </summary>
        /// <param name="store">Triple Store to save</param>
        /// <param name="manager">SQL IO Manager</param>
        public ThreadedSqlStoreWriterContext(ITripleStore store, IThreadedSqlIOManager manager)
            : this(store, manager, 8, false) { }

        /// <summary>
        /// Gets the Triple Store that is being written
        /// </summary>
        public ITripleStore Store
        {
            get
            {
                return _store;
            }
        }

        /// <summary>
        /// Gets the number of threads to use
        /// </summary>
        public int Threads
        {
            get
            {
                return this._writeThreads;
            }
        }

        /// <summary>
        /// Gets the SQL IO Manager in use
        /// </summary>
        public IThreadedSqlIOManager Manager
        {
            get
            {
                return this._manager;
            }
        }

        /// <summary>
        /// Gets whether to clear graphs if they exist
        /// </summary>
        public bool ClearIfExists
        {
            get
            {
                return this._clearIfExists;
            }
        }

        /// <summary>
        /// Adds a Graph to the queue of Graphs to be written
        /// </summary>
        /// <param name="g">Graph</param>
        public void Add(IGraph g)
        {
            this._writeQueue.Enqueue(g);
        }

        /// <summary>
        /// Gets the Next Graph to be written
        /// </summary>
        /// <returns>Next Graph to write or null if all Graphs have been written</returns>
        public IGraph GetNextGraph()
        {
            IGraph temp = null;
            try
            {
                Monitor.Enter(this._writeQueue);
                if (this._writeQueue.Count > 0)
                {
                    temp = this._writeQueue.Dequeue();
                }
            }
            finally
            {
                Monitor.Exit(this._writeQueue);
            }
            return temp;
        }
    }
}

#endif