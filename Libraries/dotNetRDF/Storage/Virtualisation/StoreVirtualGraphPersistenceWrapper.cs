/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;

namespace VDS.RDF.Storage.Virtualisation
{
    /// <summary>
    /// Base class for update operations on virtualized graphs. Implementors have to provide a method to 
    /// convert standard Nodes to their virtual form according to the IVirtualRdfProvider which is in use.
    /// </summary>
    /// <typeparam name="TNodeID">Node ID Type</typeparam>
    /// <typeparam name="TGraphID">Graph ID Type</typeparam>
    public abstract class StoreVirtualGraphPersistenceWrapper<TNodeID, TGraphID> 
        : StoreGraphPersistenceWrapper
    {

        /// <summary>
        /// Converts a standard INode to a virtualized node with a pre-materialized value.
        /// </summary>
        /// <param name="provider">Virtual RDF Provider, the object, e.g. a storage manger, that provides virtualization of nodes</param>
        /// <param name="preMaterializedValue">Node that has to be converted to it's virtualized form with itself as materialized value. Usually a parsed Literal or Uri.</param>
        protected abstract INode CreateVirtual(IVirtualRdfProvider<TNodeID, TGraphID> provider, INode preMaterializedValue);

        /// <summary>
        /// Virtual RDF Provider
        /// </summary>
        protected readonly IVirtualRdfProvider<TNodeID, TGraphID> _provider;

        /// <summary>
        /// Creates a new Store Graph Persistence Wrapper for Virtualized Nodes
        /// </summary>
        /// <param name="manager">Generic IO Manager</param>
        /// <param name="provider">Virtual RDF Provider</param>
        /// <param name="g">Graph with virtualized Nodes to wrap</param>
        /// <param name="graphUri">Graph URI (the URI the Graph will be persisted as)</param>
        /// <param name="writeOnly">Whether to operate in write-only mode</param>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> In order to operate in write-only mode the <see cref="IStorageProvider">IStorageProvider</see> must support triple level updates indicated by it returning true to its <see cref="IStorageCapabilities.UpdateSupported">UpdateSupported</see> property and the Graph to be wrapped must be an empty Graph
        /// </para>
        /// </remarks>
        public StoreVirtualGraphPersistenceWrapper(IStorageProvider manager, IVirtualRdfProvider<TNodeID, TGraphID> provider, IGraph g, Uri graphUri, bool writeOnly)
            : base(manager, g, graphUri, writeOnly)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider), "Cannot persist virtual nodes without a virtual RDF provider");
        }

        /// <summary>
        /// Asserts a Triple after virtualization in the Graph
        /// </summary>
        /// <param name="t">Triple</param>
        public override bool Assert(Triple t)
        {
            return base.Assert(VirtualizeTriple(t));
        }

        /// <summary>
        /// Retracts a Triple after virtualization from the Graph
        /// </summary>
        /// <param name="t">Triple</param>
        public override bool Retract(Triple t)
        {
            return base.Retract(VirtualizeTriple(t));
        }


        /// <summary>
        /// Gets whether the virtualized form of a given Triple exists in this Graph
        /// </summary>
        /// <param name="t">Triple to test</param>
        /// <returns>Triple is known to the Graph</returns>
        public override bool ContainsTriple(Triple t)
        {
            return base.ContainsTriple(VirtualizeTriple(t));
        }

        /// <summary>
        /// Converts subject, predicate and object of a given Triple to their respective virtualized forms
        /// </summary>
        /// <param name="t">Triple to virtualize</param>
        /// <returns>The virtualized Triple. Itself, if it was already virtual.</returns>
        protected Triple VirtualizeTriple(Triple t)
        {
            var s = VirtualizeNode(t.Subject);
            var p = VirtualizeNode(t.Predicate);
            var o = VirtualizeNode(t.Object);
            if (ReferenceEquals(s, t.Subject) && ReferenceEquals(p, t.Predicate) && ReferenceEquals(o, t.Object)) 
            {
                return t;
            }
            return new Triple(s, p, o, _g);
        }

        /// <summary>
        /// Virtualizes a Node
        /// </summary>
        /// <param name="n">Node to be virtualized</param>
        /// <returns>The Node in its virtual form. Itself, if it was already virtual.</returns>
        protected INode VirtualizeNode(INode n)
        {
            if (n is IVirtualNode<TNodeID, TGraphID>)
            {
                return n;
            }
            return CreateVirtual(_provider, n);
        }
    }
}
