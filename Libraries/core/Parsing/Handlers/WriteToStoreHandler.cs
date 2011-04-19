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
    public class WriteToStoreHandler : BaseRdfHandler
    {
        public const int DefaultBatchSize = 1000;

        private IGenericIOManager _manager;
        private List<Triple> _actions;
        private Uri _defaultGraphUri, _currGraphUri;
        private int _batchSize;

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
        }

        public WriteToStoreHandler(IGenericIOManager manager, Uri graphUri)
            : this(manager, graphUri, DefaultBatchSize) { }

        public WriteToStoreHandler(IGenericIOManager manager, int batchSize)
            : this(manager, null, batchSize) { }

        public WriteToStoreHandler(IGenericIOManager manager)
            : this(manager, null, DefaultBatchSize) { }

        protected override void StartRdfInternal()
        {
            this._actions.Clear();
            this._currGraphUri = this._defaultGraphUri;
        }

        protected override void EndRdfInternal(bool ok)
        {
            if (this._actions.Count > 0)
            {
                this.ProcessBatch();
            }
        }

        protected override bool HandleTripleInternal(Triple t)
        {
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
    }
}

#endif