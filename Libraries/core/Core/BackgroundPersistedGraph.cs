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
using System.Threading;

namespace VDS.RDF
{
    /// <summary>
    /// Base Class for Graphs which have their persistence to some underlying Store handled by a background thread
    /// </summary>
    /// <remarks>
    /// This class provides a useful general base class for Graphs which are based on some underlying Storage where the persistence to the Store can be done asychronously.
    /// </remarks>
    [Obsolete("The BackgroundPersistedGraph has been superceded by the GraphPersistenceWrapper", true)]
    public abstract class BackgroundPersistedGraph : Graph, IDisposable
    {
        /// <summary>
        /// Change Buffer for Added Triples
        /// </summary>
        protected List<Triple> _addedTriplesBuffer = new List<Triple>();
        /// <summary>
        /// Change Buffer for Removed Triples
        /// </summary>
        protected List<Triple> _removedTriplesBuffer = new List<Triple>();
        /// <summary>
        /// Thread which does the Persistence
        /// </summary>
        protected Thread _persister = null;
        /// <summary>
        /// Boolean indicating whether persistence is suspended
        /// </summary>
        /// <remarks>
        /// Set to true during initial loading so the system does not attempt to persist the existing contents of the Graph back to Storage.  Set to false at the end of the <see cref="BackgroundPersistedGraph.Initialise">Initialise()</see> method
        /// </remarks>
        protected bool _suspendPersistence = true;

        private bool _endPersistence = false, _finished = false;

        #region Data Persistence to Native Storage

        /// <summary>
        /// Initialises the Thread which will manage persistence of data
        /// </summary>
        protected virtual void Intialise()
        {
            this._suspendPersistence = false;
            this._persister = new Thread(new ThreadStart(PersistData));
            this._persister.IsBackground = true;
            this._persister.Start();
        }

        /// <summary>
        /// Method for the Background Thread which handles persistence of data
        /// </summary>
        protected virtual void PersistData()
        {
            while (true)
            {
                try
                {
                    if (this._addedTriplesBuffer.Count > 0 || this._removedTriplesBuffer.Count > 0)
                    {
                        this.Flush();
                    }

                    //If we've been asked to End Persistence we'll do so unless we've not finished flushing the buffers
                    if (this._endPersistence)
                    {
                        if (this._addedTriplesBuffer.Count > 0 || this._removedTriplesBuffer.Count > 0) continue;
                        this._finished = true;
                        return;
                    }

                    //Only sleep if there aren't anything in the queue
                    if (this._addedTriplesBuffer.Count > 0 || this._removedTriplesBuffer.Count > 0) continue;

                    Thread.Sleep(5000);
                }
                catch (ThreadAbortException)
                {
                    //Asked to stop
                    this.Flush();
#if !SILVERLIGHT
                    Thread.ResetAbort();
#endif
                    this._finished = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Forces any changes to the Graph to be persisted to the underlying Store immediately
        /// </summary>
        public virtual void Flush()
        {
            try
            {
                Monitor.Enter(this._addedTriplesBuffer);
                Monitor.Enter(this._removedTriplesBuffer);

                //Make the Update
                this.UpdateStore();
                this._addedTriplesBuffer.Clear();
                this._removedTriplesBuffer.Clear();
            }
            finally
            {
                Monitor.Exit(this._addedTriplesBuffer);
                Monitor.Exit(this._removedTriplesBuffer);
            }
        }

        /// <summary>
        /// Abstract Method to be implemented by derived classes which Updates the Store
        /// </summary>
        /// <remarks>Implements should take appropriate steps to push the Updates to the underlying Store.  This will in practise be a one/two line implementation which invokes an appropriate method on some <see cref="IGenericIOManager">IGenericIOManager</see> or similar class.</remarks>
        protected abstract void UpdateStore();

        #endregion

        #region Triple Assertion & Retraction

        /// <summary>
        /// Asserts a Triple in the Graph
        /// </summary>
        /// <param name="t">The Triple to add to the Graph</param>
        public override void Assert(Triple t)
        {
            if (!this._suspendPersistence)
            {
                if (!this._triples.Contains(t))
                {
                    lock (this._addedTriplesBuffer)
                    {
                        this._addedTriplesBuffer.Add(t);
                    }
                }
            }
            base.Assert(t);
        }

        /// <summary>
        /// Asserts multiple Triples in the Graph
        /// </summary>
        /// <param name="ts">Array of Triples to add</param>
        public override void Assert(Triple[] ts)
        {
            foreach (Triple t in ts)
            {
                this.Assert(t);
            }
        }

        /// <summary>
        /// Asserts a List of Triples in the graph
        /// </summary>
        /// <param name="ts">List of Triples to add to the Graph</param>
        public override void Assert(List<Triple> ts)
        {
            foreach (Triple t in ts)
            {
                this.Assert(t);
            }
        }

        /// <summary>
        /// Asserts a List of Triples in the graph
        /// </summary>
        /// <param name="ts">List of Triples in the form of an IEnumerable</param>
        public override void Assert(IEnumerable<Triple> ts)
        {
            this.Assert(ts.ToList());
        }

        /// <summary>
        /// Retracts a Triple from the Graph
        /// </summary>
        /// <param name="t">Triple to Retract</param>
        /// <remarks>
        /// Current implementation may have some defunct Nodes left in the Graph as only the Triple is retracted
        /// </remarks>
        public override void Retract(Triple t)
        {
            if (!this._suspendPersistence)
            {
                lock (this._removedTriplesBuffer)
                {
                    this._removedTriplesBuffer.Add(t);
                }
            }
            base.Retract(t);
        }

        /// <summary>
        /// Retracts multiple Triples from the Graph
        /// </summary>
        /// <param name="ts">Array of Triples to retract</param>
        public override void Retract(Triple[] ts)
        {
            foreach (Triple t in ts)
            {
                this.Retract(t);
            }
        }

        /// <summary>
        /// Retracts a List of Triples from the graph
        /// </summary>
        /// <param name="ts">List of Triples to retract from the Graph</param>
        public override void Retract(List<Triple> ts)
        {
            foreach (Triple t in ts)
            {
                this.Retract(t);
            }
        }

        /// <summary>
        /// Retracts a enumeration of Triples from the graph
        /// </summary>
        /// <param name="ts">Enumeration of Triples to retract</param>
        public override void Retract(IEnumerable<Triple> ts)
        {
            this.Retract(ts.ToList());
        }

        #endregion

        /// <summary>
        /// Disposes of a Background Persisted Graph
        /// </summary>
        public override void Dispose()
        {
            if (this._persister != null && !this._finished)
            {
                //Use a signal to terminate the Thread rather than Thread.Abort() which may block indefinitely in some circumstances
                this._endPersistence = true;
                this._suspendPersistence = true;
                Thread.Sleep(100);
            }
            base.Dispose();
        }
    }
}
