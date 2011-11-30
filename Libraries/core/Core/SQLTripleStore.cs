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
using System.Data;
using System.Threading;
using VDS.RDF.Storage;
using VDS.RDF.Parsing;

namespace VDS.RDF
{
    /// <summary>
    /// Class for representing Triple Stores which are automatically stored to a backing SQL Store as it is modified
    /// </summary>
    [Obsolete("The legacy SQL store format and related classes are officially deprecated - please see http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration for details on upgrading to the new ADO store format", false)] 
    public class SqlTripleStore : TripleStore, ITransactionalStore
    {
        /// <summary>
        /// Store Manager which manages the IO to the Triple Store
        /// </summary>
        protected ISqlIOManager _manager;

        /// <summary>
        /// Empty Constructor for use by derived classes
        /// </summary>
        protected SqlTripleStore() : base() 
        {
            this._storeInferencesExternally = true;
        }

        /// <summary>
        /// Opens a SQL Triple Store using the provided Store Manager, automatically loads all data contained in that Store
        /// </summary>
        /// <param name="manager">An <see cref="ISqlIOManager">ISqlIOManager</see> for your chosen backing SQL Store</param>
        public SqlTripleStore(ISqlIOManager manager) : base() {
            this._manager = manager;
            this._storeInferencesExternally = true;

            this.LoadInternal();
        }

        /// <summary>
        /// Opens a SQL Triple Store using the default Manager for a dotNetRDF Store accessible at the given database settings, automatically loads all data contained in that Store
        /// </summary>
        /// <param name="dbserver">Database Server</param>
        /// <param name="dbname">Database Name</param>
        /// <param name="dbuser">Database User</param>
        /// <param name="dbpassword">Database Password</param>
        public SqlTripleStore(String dbserver, String dbname, String dbuser, String dbpassword)
        : this(new MicrosoftSqlStoreManager(dbserver,dbname,dbuser,dbpassword)) { }

        /// <summary>
        /// Opens a SQL Triple Store using the default Manager for a dotNetRDF Store accessible at the given database settings, automatically loads all data contained in that Store
        /// </summary>
        /// <param name="dbname">Database Name</param>
        /// <param name="dbuser">Database User</param>
        /// <param name="dbpassword">Database Password</param>
        /// <remarks>Assumes that the Store is located on the localhost</remarks>
        public SqlTripleStore(String dbname, String dbuser, String dbpassword) :
            this("localhost", dbname, dbuser, dbpassword) { }

        /// <summary>
        /// Internal Method which loads the data from the SQL backed Store when the class is instantiated
        /// </summary>
        protected virtual void LoadInternal()
        {
            try
            {
                //Get all the Graphs
                List<Uri> graphUris = this._manager.GetGraphUris();

                foreach (Uri u in graphUris)
                {
                    //Create a SQL Graph
                    //This will load that Graph automatically
                    SqlGraph g = new SqlGraph(u, this._manager);

                    //Apply Inference
                    this.ApplyInference(g);

                    //Add to Graph Collection
                    this._graphs.Add(g, false);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Adds a Graph into the Triple Store
        /// </summary>
        /// <param name="g">Graph to add</param>
        public override void Add(IGraph g)
        {
            this.Add(g, false);
        }

        /// <summary>
        /// Adds a Graph into the Triple Store using the desired Merge Behaviour
        /// </summary>
        /// <param name="g">Graph to add</param>
        /// <param name="mergeIfExists">Whether the Graph should be merged with an existing Graph with the same Base Uri</param>
        public override void Add(IGraph g, bool mergeIfExists)
        {
            //Check if it already exists
            bool exists = this._graphs.Contains(g.BaseUri);
            if (exists && !mergeIfExists)
            {
                throw new RdfException("The Graph you tried to add already exists in the Graph Collection");
            }
            else if (exists)
            {
                //It already exists as a SqlGraph in the collection
                //Load with merging
                this._graphs[g.BaseUri].Merge(g);
            } 
            else
            {
                if (g.BaseUri == null) g.BaseUri = new Uri(GraphCollection.DefaultGraphUri);

                //Move it into a SqlGraph using our Shared Manager
                SqlGraph h = new SqlGraph(g.BaseUri, this._manager);
                
                //Do a direct copy of Triples
                h.Assert(g.Triples);

                //Load with no merging
                base.Add(h, mergeIfExists);
            }
        }

        /// <summary>
        /// Adds a Graph into the Triple Store by retrieving it from the given Uri
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to add</param>
        public override void AddFromUri(Uri graphUri)
        {
            this.AddFromUri(graphUri, false);
        }

        /// <summary>
        /// Adds a Graph into the Triple Store by retrieving it from the given Uri and using the selected Merge Behaviour
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to add</param>
        /// <param name="mergeIfExists">Whether the Graph should be merged with an existing Graph with the same Base Uri</param>
        public override void AddFromUri(Uri graphUri, bool mergeIfExists)
        {
            //Check if it already exists
            bool exists = this._graphs.Contains(graphUri);
            if (exists && !mergeIfExists)
            {
                throw new RdfException("The Graph you tried to add already exists in the Graph Collection");
            }
            else if (exists)
            {
                //Load into SqlGraph
                //The merge is implied here and will be handled by the Parsers
                base.AddFromUri(graphUri, mergeIfExists);
            }
            else
            {
                //Load into SqlGraph and add
                Graph g = new Graph();
                UriLoader.Load(g, graphUri);
                this.Add(g, mergeIfExists);
            }
        }

        /// <summary>
        /// Removes a Graph from the Triple Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to remove</param>
        public override void Remove(Uri graphUri)
        {
            String tempID = this._manager.GetGraphID(graphUri);
            this._manager.RemoveGraph(tempID);
        }

        /// <summary>
        /// Flushes any outstanding changes to the underlying SQL Store
        /// </summary>
        public void Flush()
        {
            this._manager.Flush();
        }

        /// <summary>
        /// Discards any outstanding changes
        /// </summary>
        public void Discard()
        {
            //Does nothing
        }

        /// <summary>
        /// Disposes of the Triple Store
        /// </summary>
        public override void Dispose()
        {
            this._manager.Dispose();
            base.Dispose();
        }
    }

    /// <summary>
    /// Class for representing Triple Stores which are automatically stored to a backing SQL Store as it is modified, use this class with large Stores to load them more efficiently
    /// </summary>
    /// <remarks>Uses multi-threaded loading to improve initial Load Times</remarks>
    [Obsolete("The legacy SQL store format and related classes are officially deprecated - please see http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration for details on upgrading to the new ADO store format", false)]
    public class ThreadedSqlTripleStore :  SqlTripleStore
    {
        private int _loadThreads = 8;
        private Queue<Uri> _loadList = new Queue<Uri>();
        private bool _async = false;

        private const int PollInterval = 250;

        /// <summary>
        /// Opens a SQL Triple Store using the provided Store Manager, automatically loads all data contained in that Store
        /// </summary>
        /// <param name="manager">An <see cref="IThreadedSqlIOManager">IThreadedSqlIOManager</see> for your chosen backing SQL Store</param>
        /// <param name="threads">The Number of Threads to use for Loading (Default is 8)</param>
        /// <param name="isAsync">Whether loading should be done asynchronously</param>
        /// <remarks>
        /// If loading is set to Asynchronous then the Graphs will be loaded in a multi-threaded fashion without waiting for them to load before proceeding.  This means that code can start working immediately with the Triple Store but that the data contained in it may be incomplete.
        /// <br />
        /// The default behaviour is for Synchronous loading which means calling code must wait for the Triple Store to load before it can proceeed.
        /// </remarks>
        public ThreadedSqlTripleStore(IThreadedSqlIOManager manager, int threads, bool isAsync) 
            : base() 
        {
            this._manager = manager;
            this._loadThreads = threads;
            this._async = isAsync;

            if (this._async)
            {
                this._graphs = new ThreadSafeGraphCollection();
            }

            this.LoadInternal();
        }

        /// <summary>
        /// Opens a SQL Triple Store using the provided Store Manager, automatically loads all data contained in that Store
        /// </summary>
        /// <param name="manager">An <see cref="IThreadedSqlIOManager">IThreadedSqlIOManager</see> for your chosen backing SQL Store</param>
        /// <param name="async">Whether loading should be done asynchronously</param>
        /// <remarks>Uses the Default 8 Threads for Loading</remarks>
        public ThreadedSqlTripleStore(IThreadedSqlIOManager manager, bool isAsync)
            : this(manager, 8, isAsync) { }

        /// <summary>
        /// Opens a SQL Triple Store using the provided Store Manager, automatically loads all data contained in that Store
        /// </summary>
        /// <param name="manager">An <see cref="IThreadedSqlIOManager">IThreadedSqlIOManager</see> for your chosen backing SQL Store</param>
        /// <param name="threads">The Number of Threads to use for Loading (Default is 8)</param>
        /// <remarks>Uses Synchronous Loading</remarks>
        public ThreadedSqlTripleStore(IThreadedSqlIOManager manager, int threads) 
            : this(manager, threads, false) { }

        /// <summary>
        /// Opens a SQL Triple Store using the provided Store Manager, automatically loads all data contained in that Store
        /// </summary>
        /// <param name="manager">An <see cref="IThreadedSqlIOManager">IThreadedSqlIOManager</see> for your chosen backing SQL Store</param>
        /// <remarks>Uses the Default 8 Threads for Loading</remarks>
        public ThreadedSqlTripleStore(IThreadedSqlIOManager manager) 
            : this(manager, 8) { }

        /// <summary>
        /// Opens a SQL Triple Store using the default Threaded Manager for a dotNetRDF Store accessible at the given database settings, automatically loads all data contained in that Store
        /// </summary>
        /// <param name="dbserver">Database Server</param>
        /// <param name="dbname">Database Name</param>
        /// <param name="dbuser">Database User</param>
        /// <param name="dbpassword">Database Password</param>
        /// <remarks>Uses the Default 8 Threads for Loading</remarks>
        public ThreadedSqlTripleStore(String dbserver, String dbname, String dbuser, String dbpassword)
            : this(new MicrosoftSqlStoreManager(dbserver,dbname,dbuser,dbpassword)) { }

        /// <summary>
        /// Opens a SQL Triple Store using the default Threaded Manager for a dotNetRDF Store accessible at the given database settings, automatically loads all data contained in that Store
        /// </summary>
        /// <param name="dbname">Database Name</param>
        /// <param name="dbuser">Database User</param>
        /// <param name="dbpassword">Database Password</param>
        /// <remarks>Assumes the Store is on the localhost.  Uses the Default 8 Threads for Loading</remarks>
        public ThreadedSqlTripleStore(String dbname, String dbuser, String dbpassword) 
            : this("localhost", dbname, dbuser, dbpassword) { }

        /// <summary>
        /// Internal Method which loads the data from the SQL backed Store when the class is instantiated
        /// </summary>
        protected override void LoadInternal()
        {
            try
            {
                //Get all the Graphs URIs
                List<Uri> graphUris = this._manager.GetGraphUris();

                //Prepare the Threads
                List<Thread> threads = new List<Thread>();
                Thread temp;
                for (int i = 0; i < this._loadThreads; i++)
                {
                    //Get a Thread and add it to Thread list
                    temp = new Thread(new ThreadStart(this.LoadGraphs));
                    threads.Add(temp);
                }

                //Queue URIs
                foreach (Uri u in graphUris)
                {
                    this._loadList.Enqueue(u);
                }

                //Start all the Threads
                foreach (Thread loader in threads)
                {
                    loader.Start();
                }

                //Poll the Threads until they all complete (unless asynchronous)
                if (!this._async)
                {
                    int activeThreads = threads.Count;
                    while (activeThreads > 0)
                    {
                        activeThreads = 0;
                        foreach (Thread loader in threads)
                        {
                            if (loader.ThreadState == ThreadState.Running)
                            {
                                activeThreads++;
                                break;
                            }
                        }
                        Thread.Sleep(PollInterval);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Internal Method which performs multi-threaded loading of data
        /// </summary>
        private void LoadGraphs()
        {
            try
            {
                Uri u = this.GetNextUri();
                while (u != null)
                {
                    //Create a SQL Graph
                    //This will load that Graph automatically
                    SqlGraph g = new SqlGraph(u, this._manager);

                    //Add to Graph Collection
                    try
                    {
                        Monitor.Enter(this._graphs);
                        this.ApplyInference(g);
                        lock (this._graphs)
                        {
                            this._graphs.Add(g, false);
                        }
                    }
                    finally
                    {
                        Monitor.Exit(this._graphs);
                    }

                    //Get the Next Uri
                    u = this.GetNextUri();
                }
            }
            catch (ThreadAbortException)
            {
                //We've been terminated, don't do anything
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                throw new RdfStorageException("Error in Threaded Loader in Thread ID " + Thread.CurrentThread.ManagedThreadId, ex);
            }
        }

        /// <summary>
        /// Internal Helper method which manages concurrent access to the queue of URIs of Graphs to be loaded
        /// </summary>
        /// <returns>Uri of next Graph to be loaded</returns>
        private Uri GetNextUri()
        {
            Uri temp = null;
            try
            {
                Monitor.Enter(this._loadList);
                if (this._loadList.Count > 0)
                {
                    temp = this._loadList.Dequeue();
                }
            }
            finally
            {
                Monitor.Exit(this._loadList);
            }
            return temp;
        }
    }
}

#endif