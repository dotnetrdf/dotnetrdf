using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using Alexandria.Documents.Adaptors;

namespace Alexandria.Documents
{
    /// <summary>
    /// A Document Manager manages access to the documents used by Alexandria ensuring concu
    /// </summary>
    public interface IDocumentManager : IDisposable
    {
        /// <summary>
        /// Returns whether the Manager has a specific document
        /// </summary>
        /// <param name="name">Name</param>
        bool HasDocument(String name);

        /// <summary>
        /// Creates a Document
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        bool CreateDocument(String name);

        /// <summary>
        /// Deletes a Document
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        /// <remarks>
        /// Documents should not be deleted if there are any active references to them, if you've retrieved a Document with <see cref="IDocumentManager.GetDocument">GetDocument()</see> then you must call <see cref="IDocumentManager.ReleaseDocument">ReleaseDocument()</see> before attempting to delete it
        /// </remarks>
        bool DeleteDocument(String name);

        /// <summary>
        /// Gets a Document
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        /// <remarks>
        /// A Document Manager must track the number of references to a Document that is retrieved by this method
        /// </remarks>
        IDocument GetDocument(String name);

        /// <summary>
        /// Gets the Graph Registry that stores the mappings between Graph URIs and Document Names
        /// </summary>
        IGraphRegistry GraphRegistry
        {
            get;
        }

        /// <summary>
        /// Signals that a Document is no longer needed by the caller
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        bool ReleaseDocument(String name);

        /// <summary>
        /// Gets the Adaptor used to convert from Documents to Graphs
        /// </summary>
        IDataAdaptor DataAdaptor
        {
            get;
        }
    }
}
