using System;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Interface for blank node generators
    /// </summary>
    /// <remarks>
    /// Blank node generators are used to consistently map string identifiers for blank nodes in serialized RDF into blank nodes.  This functionality is generally only required during parsing operations.
    /// </remarks>
    public interface IBlankNodeGenerator
    {
        /// <summary>
        /// Create a new blank node ID
        /// </summary>
        /// <param name="id">String ID</param>
        /// <returns>Blank Node ID</returns>
        Guid GetGuid(string id);
    }
}