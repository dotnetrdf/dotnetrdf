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
using VDS.RDF.Graphs;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler which asserts Triples into a Graph
    /// </summary>
    public class GraphHandler 
        : BaseRdfHandler
    {
        private IGraph _g;

        /// <summary>
        /// Creates a new Graph Handler
        /// </summary>
        /// <param name="g">Graph</param>
        public GraphHandler(IGraph g)
            : base(g)
        {
            if (g == null) throw new ArgumentNullException("graph");
            this._g = g;
        }

        /// <summary>
        /// Gets the Graph that this handler wraps
        /// </summary>
        protected IGraph Graph
        {
            get
            {
                return this._g;
            }
        }

        /// <summary>
        /// Handles Namespace Declarations by adding them to the Graphs Namespace Map
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            this._g.Namespaces.AddNamespace(prefix, namespaceUri);
            return true;
        }

        /// <summary>
        /// Handles Base URI Declarations by setting the Graphs Base URI
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <returns></returns>
        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            return true;
        }

        /// <summary>
        /// Handles Triples by asserting them in the Graph
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            this._g.Assert(t);
            return true;
        }

        /// <summary>
        /// Handles quads that are in the default graph by asserting them in the graph, all other quads are discarded
        /// </summary>
        /// <param name="q">Quad</param>
        /// <returns></returns>
        protected override bool HandleQuadInternal(Quad q)
        {
            if (q.InDefaultGraph) this._g.Assert(q.AsTriple());
            return true;
        }

        /// <summary>
        /// Gets that this Handler accepts all Triples
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
