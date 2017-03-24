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
    /// A Virtual RDF Provider is a provider that transforms materialised values into virtual ID values.  These virtual values can be used to do much faster term equality checking and to minimise memory usage when accessing out of memory data.
    /// </summary>
    /// <typeparam name="TNodeID">Node ID Type</typeparam>
    /// <typeparam name="TGraphID">Graph ID Type</typeparam>
    /// <remarks>
    /// <para>
    /// An implementation of this is typically in addition to a more general RDF store implementation (such as an <see cref="IStorageProvider">IStorageProvider</see>) and was originally designed and intended for use in creating <see cref="VDS.RDF.Query.Datasets.ISparqlDataset">ISparqlDataset</see> instances which allow out of memory data to be queried more efficiently.
    /// </para>
    /// <para>
    /// It is expected that most implementations will use a cache to ensure that repeated transformations are as fast as possible
    /// </para>
    /// <h3>Important Note re: Blank Nodes</h3>
    /// <para>
    /// In order for code that uses this class to function correctly it must be ensured that IDs issued for Blank Nodes are graph scoped, as such a specific method for converting Blank Nodes into Virtual Node IDs is given
    /// </para>
    /// </remarks>
    public interface IVirtualRdfProvider<TNodeID, TGraphID>
    {
        /// <summary>
        /// Given a Node ID returns the materialised value in the given Graph
        /// </summary>
        /// <param name="g">Graph to create the Node in</param>
        /// <param name="id">Node ID</param>
        /// <returns></returns>
        INode GetValue(IGraph g, TNodeID id);

        /// <summary>
        /// Given a Graph ID returns the value of the Graph URI
        /// </summary>
        /// <param name="id">Graph ID</param>
        /// <returns></returns>
        Uri GetGraphUri(TGraphID id);

        /// <summary>
        /// Given a non-blank Node returns the Node ID
        /// </summary>
        /// <param name="value">Node</param>
        /// <remarks>
        /// Should function as equivalent to the two argument version with the <strong>createIfNotExists</strong> parameter set to false
        /// </remarks>
        TNodeID GetID(INode value);

        /// <summary>
        /// Gets the Graph ID for a Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        /// <remarks>
        /// Should function as equivalent to the two argument version with the <strong>createIfNotExists</strong> parameter set to false
        /// </remarks>
        TGraphID GetGraphID(IGraph g);

        /// <summary>
        /// Gets the Graph ID for a Graph creating it if necessary
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="createIfNotExists">Determines whether to create a new Graph ID if there is not already one for the given Graph</param>
        /// <returns></returns>
        TGraphID GetGraphID(IGraph g, bool createIfNotExists);

        /// <summary>
        /// Gets the Graph ID for a Graph URI
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        /// <remarks>
        /// Should function as equivalent to the two argument version with the <strong>createIfNotExists</strong> parameter set to false
        /// </remarks>
        TGraphID GetGraphID(Uri graphUri);
        
        /// <summary>
        /// Gets the Graph ID for a Graph URI
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="createIfNotExists">Determines whether to create a new Graph ID if there is not already one for the given Graph URI</param>
        /// <returns></returns>
        TGraphID GetGraphID(Uri graphUri, bool createIfNotExists);

        /// <summary>
        /// Given a non-blank Node returns the Node ID
        /// </summary>
        /// <param name="value">Node</param>
        /// <param name="createIfNotExists">Determines whether to create a new Node ID if there is not already one for the given value</param>
        /// <returns></returns>
        TNodeID GetID(INode value, bool createIfNotExists);

        /// <summary>
        /// Given a Blank Node returns a Graph scoped Node ID
        /// </summary>
        /// <param name="value">Blank Node</param>
        /// <param name="createIfNotExists">Determines whether to create a new Node ID if there is not already one for the given value</param>
        /// <returns></returns>
        TNodeID GetBlankNodeID(IBlankNode value, bool createIfNotExists);

        /// <summary>
        /// Given a Blank Node returns a Graph scoped Node ID
        /// </summary>
        /// <param name="value">Blank Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Should function as equivalent to the two argument version with the <strong>createIfNotExists</strong> parameter set to false
        /// </remarks>
        TNodeID GetBlankNodeID(IBlankNode value);

        /// <summary>
        /// Gets the Node ID that is used to indicate that a Node does not exist in the underlying storage
        /// </summary>
        TNodeID NullID
        {
            get;
        }

        /// <summary>
        /// Loads a Graph creating all the Triples with virtual node values
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        void LoadGraphVirtual(IGraph g, Uri graphUri);
    }
}
