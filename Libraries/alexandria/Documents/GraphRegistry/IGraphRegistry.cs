using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.Alexandria.Documents.GraphRegistry
{
    /// <summary>
    /// Interface for Graph Registrys
    /// </summary>
    public interface IGraphRegistry
    {
        /// <summary>
        /// Converts from a Graph URI to a Document Name
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        String GetDocumentName(String graphUri);

        /// <summary>
        /// Converts from a Document Name to a Graph URI
        /// </summary>
        /// <param name="name">Document Name</param>
        /// <returns></returns>
        String GetGraphUri(String name);

        /// <summary>
        /// Registers a Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="name">Document Name</param>
        /// <returns></returns>
        bool RegisterGraph(String graphUri, String name);

        /// <summary>
        /// Unregisters a Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="name">Document Name</param>
        /// <returns></returns>
        bool UnregisterGraph(String graphUri, String name);

        /// <summary>
        /// Gets the Document Names of all the Documents that contain Graphs
        /// </summary>
        IEnumerable<String> DocumentNames
        {
            get;
        }

        /// <summary>
        /// Gets the Graph URIs of all Graphs
        /// </summary>
        IEnumerable<String> GraphUris
        {
            get;
        }

        /// <summary>
        /// Gets all the Mappings from Document Names to Graph URIs
        /// </summary>
        IEnumerable<KeyValuePair<String, String>> DocumentToGraphMappings
        {
            get;
        }

        /// <summary>
        /// Gets all the Mappings from Graph URIs to Document Names
        /// </summary>
        IEnumerable<KeyValuePair<String, String>> GraphToDocumentMappings
        {
            get;
        }
    }
}
