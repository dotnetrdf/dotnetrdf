/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler that rewrites the Graph URIs of Triples and/or Quads before passing them to an inner handler
    /// </summary>
    public class GraphUriRewriteHandler
        : BaseRdfHandler
    {
        private readonly IRdfHandler _handler;
        private readonly INode _graphName;

        /// <summary>
        /// Creates a new Graph URI rewriting handler
        /// </summary>
        /// <param name="handler">Handler to wrap</param>
        /// <param name="graphUri">Graph URI to rewrite to</param>
        public GraphUriRewriteHandler(IRdfHandler handler, Uri graphUri)
        {
            this._handler = handler;
            this._graphName = this.CreateUriNode(graphUri);
        }

        /// <summary>
        /// Starts handling of RDF
        /// </summary>
        protected override void StartRdfInternal()
        {
            base.StartRdfInternal();
            this._handler.StartRdf();
        }

        /// <summary>
        /// Ends handling of RDF
        /// </summary>
        /// <param name="ok">Whether parsing completed OK</param>
        protected override void EndRdfInternal(bool ok)
        {
            this._handler.EndRdf(ok);
            base.EndRdfInternal(ok);
        }

        /// <summary>
        /// Handles a Base URI declaration
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <returns></returns>
        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            return this._handler.HandleBaseUri(baseUri);
        }

        /// <summary>
        /// Handles a Namespace declaration
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            return this._handler.HandleNamespace(prefix, namespaceUri);
        }

        /// <summary>
        /// Handles a Triple by rewriting the Graph URI and passing it to the inner handler
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            return this._handler.HandleQuad(t.AsQuad(this._graphName));
        }

        /// <summary>
        /// Handles a Quad by rewriting
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        protected override bool HandleQuadInternal(Quad q)
        {
            return this._handler.HandleQuad(q.CopyTo(this._graphName));
        }

        /// <summary>
        /// Returns true since this handler accepts all triples
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
