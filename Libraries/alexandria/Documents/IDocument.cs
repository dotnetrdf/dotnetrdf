using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VDS.Alexandria.Documents
{
    /// <summary>
    /// Represents a Document used by Alexandria
    /// </summary>
    /// <typeparam name="TReader">Reader Type that can be used to read the contents of a Document</typeparam>
    /// <typeparam name="TWriter">Writer Type that can be used to write the contents of a Document</typeparam>
    public interface IDocument<TReader, TWriter> : IDisposable
    {
        /// <summary>
        /// Gets the Manager that generated the Document
        /// </summary>
        IDocumentManager<TReader,TWriter> DocumentManager
        {
            get;
        }

        /// <summary>
        /// Gets the Documents Name
        /// </summary>
        String Name
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
        /// <param name="append">Whether the Document is opened for apending or whether a new document should be written in place of the existing document</param>
        /// <returns></returns>
        TWriter BeginWrite(bool append);

        /// <summary>
        /// Signals that the write has ended
        /// </summary>
        void EndWrite();

        /// <summary>
        /// Opens a Document to start reading
        /// </summary>
        /// <returns></returns>
        TReader BeginRead();

        /// <summary>
        /// Signals that the read has ended
        /// </summary>
        void EndRead();
    }
}
