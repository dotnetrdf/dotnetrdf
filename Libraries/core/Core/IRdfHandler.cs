using System;

namespace VDS.RDF
{
    /// <summary>
    /// Interface for Handlers which handle the RDF retrieved from Parsing
    /// </summary>
    public interface IRdfHandler : INodeFactory
    {
        /// <summary>
        /// Start the Handling of RDF
        /// </summary>
        /// <exception cref="RdfParseException">May be thrown if the Handler is already in use and the implementation is not thread-safe</exception>
        void StartRdf();

        /// <summary>
        /// End the Handling of RDF
        /// </summary>
        /// <param name="ok">Whether parsing finished without error</param>
        void EndRdf(bool ok);

        /// <summary>
        /// Handles a Namespace Definition
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns>Should return <strong>true</strong> if parsing should continue or <strong>false</strong> if it should be aborted</returns>
        bool HandleNamespace(String prefix, Uri namespaceUri);

        /// <summary>
        /// Handles a Base URI Definition
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <returns>Should return <strong>true</strong> if parsing should continue or <strong>false</strong> if it should be aborted</returns>
        bool HandleBaseUri(Uri baseUri);

        /// <summary>
        /// Handles a Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns>Should return <strong>true</strong> if parsing should continue or <strong>false</strong> if it should be aborted</returns>
        bool HandleTriple(Triple t);
    }
}
