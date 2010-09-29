using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;

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
        /// Signals that a Document is no longer needed by the caller
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        bool ReleaseDocument(String name);

        /// <summary>
        /// Gets the Adaptor used to convert from Documents to Graphs
        /// </summary>
        IDocumentToGraphAdaptor GraphAdaptor
        {
            get;
        }
    }

    /// <summary>
    /// Represents a Document used by Alexandria
    /// </summary>
    public interface IDocument : IDisposable
    {
        /// <summary>
        /// Gets the Documents Name
        /// </summary>
        String Name
        {
            get;
        }

        /// <summary>
        /// Gets whether a Document can be opened for Reading
        /// </summary>
        bool CanRead
        {
            get;
        }

        /// <summary>
        /// Gets whether a Document can be opened for Writing
        /// </summary>
        bool CanWrite
        {
            get;
        }

        /// <summary>
        /// Gets whether the Document exists
        /// </summary>
        bool Exists
        {
            get;
        }

        /// <summary>
        /// Opens a Document to start writing
        /// </summary>
        /// <returns></returns>
        TextWriter BeginWrite();

        /// <summary>
        /// Signals that the write has ended
        /// </summary>
        void EndWrite();

        /// <summary>
        /// Opens a Document to start reading
        /// </summary>
        /// <returns></returns>
        StreamReader BeginRead();

        /// <summary>
        /// Signals that the read has ended
        /// </summary>
        void EndRead();
    }

    /// <summary>
    /// An Adaptor which can convert a Document into an RDF Graph
    /// </summary>
    public interface IDocumentToGraphAdaptor
    {
        void ToGraph(IGraph g, IDocument document);

        void ToDocument(IGraph g, IDocument document);
    }
}
