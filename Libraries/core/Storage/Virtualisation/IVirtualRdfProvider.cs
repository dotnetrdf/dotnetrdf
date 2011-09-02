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
    /// An implementation of this is typically in addition to a more general RDF store implementation (such as an <see cref="IGenericIOManager">IGenericIOManager</see>) and was originally designed and intended for use in creating <see cref="VDS.RDF.Query.Datasets.ISparqlDataset">ISparqlDataset</see> instances which allow out of memory data to be queried more efficiently.
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
