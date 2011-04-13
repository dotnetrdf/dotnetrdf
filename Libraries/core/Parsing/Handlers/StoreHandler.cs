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

namespace VDS.RDF.Parsing.Handlers
{
    public class StoreHandler : IRdfHandler
    {
        private bool _inUse = false;
        private NodeFactory _factory;
        private ITripleStore _store;

        public StoreHandler(ITripleStore store)
        {
            if (store == null) throw new ArgumentNullException("store");
            this._store = store;
            this._factory = new NodeFactory();
        }

        #region IRdfHandler Members

        public void StartRdf()
        {
            if (this._inUse) throw new RdfParseException("Cannot use this StoreHandler as an RDF Handler for parsing as it is already in-use");
            this._inUse = true;
        }

        public void EndRdf(bool ok)
        {
            if (!this._inUse) throw new RdfParseException("Cannot End RDF Handling as this RDF Handler is not currently in-use");
            this._inUse = false;
        }

        public bool HandleNamespace(string prefix, Uri namespaceUri)
        {
            if (!this._inUse) throw new RdfParseException("Cannot Handle Namespace as this RDF Handler is not currently in-use");
            return true;
        }

        public bool HandleBaseUri(Uri baseUri)
        {
            if (!this._inUse) throw new RdfParseException("Cannot Handle Base URI as this RDF Handler is not currently in-use");
            return true;
        }

        public bool HandleTriple(Triple t)
        {
            if (!this._inUse) throw new RdfParseException("Cannot Handle Triple as this RDF Handler is not currently in-use");

            if (!this._store.HasGraph(t.GraphUri))
            {
                Graph g = new Graph();
                g.BaseUri = t.GraphUri;
                this._store.Add(g);
            }
            IGraph target = this._store.Graph(t.GraphUri);
            target.Assert(t.CopyTriple(target));
            return true;
        }

        #endregion

        #region INodeFactory Members

        public IBlankNode CreateBlankNode()
        {
            return this._factory.CreateBlankNode();
        }

        public IBlankNode CreateBlankNode(string nodeId)
        {
            return this._factory.CreateBlankNode(nodeId);
        }

        public IGraphLiteralNode CreateGraphLiteralNode()
        {
            return this._factory.CreateGraphLiteralNode();
        }

        public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
        {
            return this._factory.CreateGraphLiteralNode(subgraph);
        }

        public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
        {
            return this._factory.CreateLiteralNode(literal, datatype);
        }

        public ILiteralNode CreateLiteralNode(string literal)
        {
            return this._factory.CreateLiteralNode(literal);
        }

        public ILiteralNode CreateLiteralNode(string literal, string langspec)
        {
            return this._factory.CreateLiteralNode(literal, langspec);
        }

        public IUriNode CreateUriNode(Uri uri)
        {
            return this._factory.CreateUriNode(uri);
        }

        public IVariableNode CreateVariableNode(string varname)
        {
            return this._factory.CreateVariableNode(varname);
        }

        public string GetNextBlankNodeID()
        {
            return this._factory.GetNextBlankNodeID();
        }

        #endregion
    }
}
