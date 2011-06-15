using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage.Virtualisation
{
    /// <summary>
    /// A Virtual RDF Provider is a provider that transforms materialised values into virtual ID values.  These virtual values can be used to do much faster term equality checking and to minimise memory usage when accessing out of memory data.
    /// </summary>
    /// <typeparam name="T">ID Type</typeparam>
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
        /// <param name="id">Node ID</param>
        /// <returns></returns>
        INode GetValue(IGraph g, TNodeID id);

        /// <summary>
        /// Given a non-blank Node returns the Node ID
        /// </summary>
        /// <param name="value">Node</param>
        /// <remarks>
        /// Should function as equivalent to the two argument version with the <strong>createIfNotExists</strong> parameter set to false
        /// </remarks>
        TNodeID GetID(INode value);

        TGraphID GetGraphID(IGraph g);

        TGraphID GetGraphID(IGraph g, bool createIfNotExists);

        TGraphID GetGraphID(Uri graphUri);

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
    }
}
