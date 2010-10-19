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

#if !NO_WEB && !NO_STORAGE

using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Web;
using VDS.RDF.Storage;

namespace VDS.RDF.Update.Protocol
{
    /// <summary>
    /// A processor for the SPARQL Uniform HTTP Protocol which operates by performing the desired operations on some arbitrary underlying Store for which an <see cref="IGenericIOManager">IGenericIOManager</see> is available
    /// </summary>
    public class GenericProtocolProcessor : BaseProtocolProcessor
    {
        private IGenericIOManager _manager;

        /// <summary>
        /// Creates a new Generic Protocol Processor
        /// </summary>
        /// <param name="manager">Generic IO Manager</param>
        public GenericProtocolProcessor(IGenericIOManager manager)
        {
            this._manager = manager;
        }

        /// <summary>
        /// Processes a GET operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <remarks>
        /// Implemented by making a call to <see cref="IGenericIOManager.LoadGraph">LoadGraph()</see> on the underlying <see cref="IGenericIOManager">IGenericIOManager</see>
        /// </remarks>
        public override void ProcessGet(HttpContext context)
        {
            Graph g = new Graph();
            Uri graphUri = this.ResolveGraphUri(context);
            this._manager.LoadGraph(g, graphUri);

            String ctype;
            IRdfWriter writer = MimeTypesHelper.GetWriter(context.Request.AcceptTypes, out ctype);
            context.Response.ContentType = ctype;
            writer.Save(g, new StreamWriter(context.Response.OutputStream));
        }

        /// <summary>
        /// Processes a POST operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <remarks>
        /// <para>
        /// <strong>Warning: </strong> If the underlying <see cref="IGenericIOManager">IGenericIOManager</see> is read-only then this operation returns a 403 Forbidden.
        /// </para>
        /// <para>
        /// Otherwise this is implemented using <see cref="IGenericIOManager.UpdateGraph">UpdateGraph()</see> if updates are supported, if not then the Graph has to be loaded, the POSTed data merged into it and then the Graph is saved again.
        /// </para>
        /// </remarks>
        public override void ProcessPost(HttpContext context)
        {
            //If the Manager is read-only then a 403 Forbidden will be returned
            if (this._manager.IsReadOnly)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            IGraph g = this.ParsePayload(context);
            if (g == null) return;

            Uri graphUri = this.ResolveGraphUri(context, g);

            if (this._manager.UpdateSupported)
            {
                this._manager.UpdateGraph(graphUri, g.Triples, Enumerable.Empty<Triple>());
            }
            else
            {
                //If the Manager does not support update we attempt to get around this by loading the Graph
                //appending the additions to it (via merging) and then saving it back to the Store
                Graph current = new Graph();
                this._manager.LoadGraph(current, graphUri);
                current.Merge(g);
                this._manager.SaveGraph(current);
            }
        }

        /// <summary>
        /// Processes a PUT operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <remarks>
        /// <para>
        /// <strong>Warning: </strong> If the underlying <see cref="IGenericIOManager">IGenericIOManager</see> is read-only then this operation returns a 403 Forbidden.
        /// </para>
        /// <para>
        /// Implemented by calling <see cref="IGenericIOManager.SaveGraph">SaveGraph()</see> on the underlying manager
        /// </para>
        /// </remarks>
        public override void ProcessPut(HttpContext context)
        {
            //If the Manager is read-only then a 403 Forbidden will be returned
            if (this._manager.IsReadOnly)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            IGraph g = this.ParsePayload(context);
            Uri graphUri = this.ResolveGraphUri(context, g);
            if (g == null) g = new Graph();
            g.BaseUri = graphUri;

            this._manager.SaveGraph(g);
        }

        /// <summary>
        /// Processes a DELETE operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <remarks>
        /// <para>
        /// <strong>Warning: </strong> If the underlying <see cref="IGenericIOManager">IGenericIOManager</see> is read-only then this operation returns a 403 Forbidden.
        /// </para>
        /// <para>
        /// The delete operation does not explicitly remove the Graph but simply replaces it with an empty Graph
        /// </para>
        /// </remarks>
        public override void ProcessDelete(HttpContext context)
        {
            //If the Manager is read-only then a 403 Forbidden will be returned
            if (this._manager.IsReadOnly)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            IGraph g = new Graph();
            Uri graphUri = this.ResolveGraphUri(context);
            g.BaseUri = graphUri;

            this._manager.SaveGraph(g);
        }

        public override void ProcessHead(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public override void ProcessOptions(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public override void ProcessPatch(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}

#endif