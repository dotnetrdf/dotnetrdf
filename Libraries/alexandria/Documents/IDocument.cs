using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Alexandria.Documents
{
    /// <summary>
    /// Represents a Document used by Alexandria
    /// </summary>
    public interface IDocument : IDisposable
    {
        /// <summary>
        /// Gets the Manager that generated the Document
        /// </summary>
        IDocumentManager DocumentManager
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
        TextWriter BeginWrite(bool append);

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
}
