/*

Copyright Robert Vesse 2009-12
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
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    class PreviewGraphTask : CancellableTask<IGraph>
    {
        private IGenericIOManager _manager;
        private String _graphUri;
        private int _previewSize = 100;
        private CancellableHandler _canceller;

        public PreviewGraphTask(IGenericIOManager manager, String graphUri, int previewSize)
            : base("Preview Graph")
        {
            this._manager = manager;
            this._graphUri = graphUri;
            this._previewSize = previewSize;
        }

        protected override IGraph RunTaskInternal()
        {
            if (this._graphUri != null && !this._graphUri.Equals(String.Empty))
            {
                this.Information = "Previewing Graph " + this._graphUri.ToString() + "...";
            }
            else
            {
                this.Information = "Previewing Default Graph...";
            }

            Graph g = new Graph();
            this._canceller = new CancellableHandler(new PagingHandler(new GraphHandler(g), this._previewSize));
            this._manager.LoadGraph(this._canceller, this._graphUri);
            this.Information = "Previewed Graph previews first " + g.Triples.Count + " Triple(s)";
            return g;
        }

        protected override void CancelInternal()
        {
            if (this._canceller != null)
            {
                this._canceller.Cancel();
            }
        }
    }
}
