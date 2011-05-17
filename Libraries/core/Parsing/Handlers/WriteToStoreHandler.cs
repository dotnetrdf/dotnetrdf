/*

Copyright Robert Vesse 2009-11
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

#if !NO_STORAGE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler which writes the Triples being parsed directly to a <see cref="IGenericIOManager">IGenericIOManager</see> in batches provided the manager supports the <see cref="IGenericIOManager.UpdateGraph">UpdateGraph()</see> method
    /// </summary>
    public class WriteToStoreHandler : BaseRdfHandler
    {
        /// <summary>
        /// Default Batch Size for writes
        /// </summary>
        public const int DefaultBatchSize = 1000;

        private IGenericIOManager _manager;
        private List<Triple> _actions, _bnodeActions;
        private HashSet<String> _bnodeUris;
        private Uri _defaultGraphUri, _currGraphUri;
        private int _batchSize;

        /// <summary>
        /// Creates a new Write to Store Handler
        /// </summary>
        /// <param name="manager">Manager to write to</param>
        /// <param name="defaultGraphUri">Graph URI to write Triples from the default graph to</param>
        /// <param name="batchSize">Batch Size</param>
        public WriteToStoreHandler(IGenericIOManager manager, Uri defaultGraphUri, int batchSize)
        {
            if (manager == null) throw new ArgumentNullException("manager", "Cannot write to a null Generic IO Manager");
            if (manager.IsReadOnly) throw new ArgumentException("manager", "Cannot write to a Read-Only Generic IO Manager");
            if (!manager.UpdateSupported) throw new ArgumentException("manager", "Generic IO Manager must support Triple Level updates to be used with this Handler");
            if (batchSize <= 0) throw new ArgumentException("batchSize", "Batch Size must be >= 1");

            this._manager = manager;
            this._defaultGraphUri = defaultGraphUri;
            this._batchSize = batchSize;

            //Make the Actions Queue one larger than the Batch Size
            this._actions = new List<Triple>(this._batchSize + 1);
            this._bnodeActions = new List<Triple>(this._batchSize + 1);
            this._bnodeUris = new HashSet<string>();
        }

        /// <summary>
        /// Creates a new Write to Store Handler
        /// </summary>
        /// <param name="manager">Manager to write to</param>
        /// <param name="defaultGraphUri">Graph URI to write Triples from the default graph to</param>
        public WriteToStoreHandler(IGenericIOManager manager, Uri defaultGraphUri)
            : this(manager, defaultGraphUri, DefaultBatchSize) { }

        /// <summary>
        /// Creates a new Write to Store Handler
        /// </summary>
        /// <param name="manager">Manager to write to</param>
        /// <param name="batchSize">Batch Size</param>
        public WriteToStoreHandler(IGenericIOManager manager, int batchSize)
            : this(manager, null, batchSize) { }

        /// <summary>
        /// Creates a new Write to Store Handler
        /// </summary>
        /// <param name="manager">Manager to write to</param>
        public WriteToStoreHandler(IGenericIOManager manager)
            : this(manager, null, DefaultBatchSize) { }

        /// <summary>
        /// Starts RDF Handling by ensuring the queue of Triples to write is empty
        /// </summary>
        protected override void StartRdfInternal()
        {
            this._actions.Clear();
            this._bnodeActions.Clear();
            this._bnodeUris.Clear();
            this._currGraphUri = this._defaultGraphUri;
        }

        /// <summary>
        /// Ends RDF Handling by ensuring the queue of Triples to write has been processed
        /// </summary>
        /// <param name="ok">Indicates whether parsing completed without error</param>
        protected override void EndRdfInternal(bool ok)
        {
            //First process the last batch of ground triples (if any)
            if (this._actions.Count > 0)
            {
                this.ProcessBatch();
            }
            //Then process each batch of non-ground triples
            List<Uri> uris = (from u in this._bnodeUris
                              select (u.Equals(String.Empty) ? null : new Uri(u))).ToList();
            foreach (Uri u in uris)
            {
                List<Triple> batch = new List<Triple>();
                for (int i = 0; i < this._bnodeActions.Count; i++)
                {
                    if (EqualityHelper.AreUrisEqual(u, this._bnodeActions[i].GraphUri))
                    {
                        batch.Add(this._bnodeActions[i]);
                        this._bnodeActions.RemoveAt(i);
                        i--;
                    }
                }
                if (u == null)
                {
                    this._manager.UpdateGraph(this._defaultGraphUri, batch, null);
                }
                else
                {
                    this._manager.UpdateGraph(u, batch, null);
                }
            }
        }

        /// <summary>
        /// Handles Triples by queuing them for writing and enacting the writing if the Batch Size has been reached/exceeded
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            if (t.IsGroundTriple)
            {
                //Ground Triples are processed in Batches as we handle the Triples
                if (t.GraphUri != null && !EqualityHelper.AreUrisEqual(t.GraphUri, this._currGraphUri))
                {
                    //The Triple has a Graph URI and it is not the same as the Current Graph URI
                    //so we process the existing Batch and then set the Current Graph URI to the new Graph URI
                    this.ProcessBatch();
                    this._currGraphUri = t.GraphUri;
                }
                else if (t.GraphUri == null && !EqualityHelper.AreUrisEqual(this._currGraphUri, this._defaultGraphUri))
                {
                    //The Triple has no Graph URI and the Current Graph URI is not the Default Graph URI so
                    //we process the existing Batch and reset the Current Graph URI to the Default Graph URI
                    this.ProcessBatch();
                    this._currGraphUri = this._defaultGraphUri;
                }

                this._actions.Add(t);

                //Whenever we hit the Batch Size process it
                if (this._actions.Count >= this._batchSize)
                {
                    this.ProcessBatch();
                }
            }
            else
            {
                //Non-Ground Triples (i.e. those with Blank Nodes) are saved up until the end to ensure that Blank
                //Node are persisted properly
                this._bnodeActions.Add(t);
                this._bnodeUris.Add(t.GraphUri.ToSafeString());
            }
            return true;
        }

        private void ProcessBatch()
        {
            if (this._actions.Count > 0)
            {
                this._manager.UpdateGraph(this._currGraphUri, this._actions, null);
                this._actions.Clear();
            }
        }

        /// <summary>
        /// Gets that the Handler accepts all Triples
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

#endif