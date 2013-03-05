/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Threading;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler that loads Quads into a <see cref="ITripleStore">ITripleStore</see> instance
    /// </summary>
    public class StoreHandler 
        : BaseRdfHandler
    {
        private ITripleStore _store;
        private INamespaceMapper _nsmap = new NamespaceMapper();

        /// <summary>
        /// Creates a new Store Handler
        /// </summary>
        /// <param name="store">Triple Store</param>
        public StoreHandler(ITripleStore store)
            : base()
        {
            if (store == null) throw new ArgumentNullException("store");
            this._store = store;
        }

        /// <summary>
        /// Gets the Triple Store that this Handler is populating
        /// </summary>
        protected ITripleStore Store
        {
            get
            {
                return this._store;
            }
        }

        #region IRdfHandler Members

        /// <summary>
        /// Handles namespaces by adding them to each graph
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            this._nsmap.AddNamespace(prefix, namespaceUri);
            return true;
        }

        /// <summary>
        /// Handles Triples by asserting them into the appropriate Graph creating the Graph if necessary
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            if (!this._store.HasGraph(null))
            {
                Graph g = new Graph();
                g.BaseUri = null;
                this._store.Add(g);
            }
            IGraph target = this._store[null];
            target.Assert(t);
            return true;
        }

        protected override bool HandleQuadInternal(Quad q)
        {
            if (!this._store.HasGraph(q.Graph))
            {
                Graph g = new Graph();
                g.BaseUri = q.Graph;
                this._store.Add(g);
            }
            IGraph target = this._store[q.Graph];
            target.Assert(q.AsTriple());
            return true;
        }

        /// <summary>
        /// Starts handling RDF
        /// </summary>
        protected override void StartRdfInternal()
        {
            this._nsmap.Clear();
        }

        /// <summary>
        /// Ends RDF handling and propogates all discovered namespaces to all discovered graphs
        /// </summary>
        /// <param name="ok">Whether parsing completed successfully</param>
        protected override void EndRdfInternal(bool ok)
        {
            //Propogate discovered namespaces to all graphs
            foreach (IGraph g in this._store.Graphs)
            {
                g.NamespaceMap.Import(this._nsmap);
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

        #endregion
    }
}
