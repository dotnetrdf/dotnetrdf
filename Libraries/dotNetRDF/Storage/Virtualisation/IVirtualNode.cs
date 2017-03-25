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
    /// Interface for Virtual Nodes
    /// </summary>
    /// <typeparam name="TNodeID">Node ID Type</typeparam>
    /// <typeparam name="TGraphID">Graph ID Type</typeparam>
    public interface IVirtualNode<TNodeID, TGraphID> 
        : INode, IEquatable<IVirtualNode<TNodeID, TGraphID>>, IComparable<IVirtualNode<TNodeID, TGraphID>>,
        IVirtualIdComparable
    {
        /// <summary>
        /// Gets the Node ID
        /// </summary>
        TNodeID VirtualID
        {
            get;
        }

        /// <summary>
        /// Gets the Virtual Node provider
        /// </summary>
        IVirtualRdfProvider<TNodeID, TGraphID> Provider
        {
            get;
        }

        /// <summary>
        /// Gets whether the Nodes value has been materialised
        /// </summary>
        bool IsMaterialised
        {
            get;
        }

        /// <summary>
        /// Gets the materialised value forcing it to be materialised if necessary
        /// </summary>
        INode MaterialisedValue
        {
            get;
        }
    }

    /// <summary>
    /// Interface for comparing nodes on their VirtualID property
    /// </summary>
    public interface IVirtualIdComparable
    {
        /// <summary>
        /// Attempt to compare the VirtualID of this node with the VirtualID of the other node
        /// </summary>
        /// <param name="other">The other node to try to compare against</param>
        /// <param name="comparisonResult">The result of the comparison if it could be performed</param>
        /// <returns>True if a comparison could be performed, false otherwise.</returns>
        bool TryCompareVirtualId(INode other, out int comparisonResult);
    }

    /// <summary>
    /// Interface for nodes that know for themseves how to create a copy of themselves to a different graph
    /// </summary>
    /// <remarks>
    /// Especially virtual nodes need to copy themselves during query algebra processing,
    /// because the standard copy tools might destroy their virtual state by duplicating it's virtualized
    /// values. In consequence all indices in the various triple stores fail to match such value-copied nodes
    /// </remarks> 
    public interface ICanCopy 
    {
        // Note: could someone please check, if every node should know how to copy itself.

        /// <summary>
        /// Copies the Node into another Graph, currently only used by virtual nodes
        /// </summary>
        /// <param name="target">Target Graph</param>
        /// <returns></returns>
        INode CopyNode(IGraph target);
    }
}
