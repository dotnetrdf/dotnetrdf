/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

#if !NO_COMPRESSION

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Abstract Base class for Dataset writers that produce GZipped Output
    /// </summary>
    /// <remarks>
    /// <para>
    /// While the normal witers can be used with GZip streams directly this class just abstracts the wrapping of file/stream output into a GZip stream if it is not already passed as such
    /// </para>
    /// </remarks>
    public abstract class BaseGZipDatasetWriter
        : IStoreWriter
    {
        private IStoreWriter _writer;

        /// <summary>
        /// Creates a new GZiped Writer
        /// </summary>
        /// <param name="writer">Underlying writer</param>
        public BaseGZipDatasetWriter(IStoreWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            this._writer = writer;
            this._writer.Warning += this.RaiseWarning;
        }

        /// <summary>
        /// Saves a RDF Dataset as GZipped output
        /// </summary>
        /// <param name="store">Store to save</param>
        /// <param name="filename">File to save to</param>
        public void Save(ITripleStore store, String filename)
        {
            if (filename == null) throw new RdfOutputException("Cannot output to a null file");
            this.Save(store, new StreamWriter(new GZipStream(new FileStream(filename, FileMode.Create, FileAccess.Write), CompressionMode.Compress)));
        }

        /// <summary>
        /// Saves a RDF Dataset as GZipped output
        /// </summary>
        /// <param name="store">Store to save</param>
        /// <param name="output">Writer to save to</param>
        public void Save(ITripleStore store, TextWriter output)
        {
            if (store == null) throw new RdfOutputException("Cannot output a null Triple Store");
            if (output == null) throw new RdfOutputException("Cannot output to a null writer");

            if (output is StreamWriter)
            {
                StreamWriter writer = (StreamWriter)output;
                if (writer.BaseStream is GZipStream)
                {
                    this._writer.Save(store, writer);
                }
                else
                {
                    this._writer.Save(store, new StreamWriter(new GZipStream(writer.BaseStream, CompressionMode.Compress)));
                }
            }
            else
            {
                throw new RdfOutputException("GZip Dataset Writers can only write to StreamWriter instances");
            }
        }

        /// <summary>
        /// Helper method for raising warning events
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            StoreWriterWarning d = this.Warning;
            if (d != null) d(message);
        }

        /// <summary>
        /// Event raised when non-fatal output errors
        /// </summary>
        public event StoreWriterWarning Warning;

        /// <summary>
        /// Gets the description of the writer
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "GZipped " + this._writer.ToString();
        }
    }

    /// <summary>
    /// Writer for creating GZipped NQuads output
    /// </summary>
    public class GZippedNQuadsWriter
        : BaseGZipDatasetWriter
    {
        /// <summary>
        /// Creates a new GZipped NQuads output
        /// </summary>
        public GZippedNQuadsWriter()
            : base(new NQuadsWriter()) { }
    }

    /// <summary>
    /// Writer for creating GZipped TriG outptut
    /// </summary>
    public class GZippedTriGWriter
        : BaseGZipDatasetWriter
    {
        /// <summary>
        /// Creates a new GZipped TriG output
        /// </summary>
        public GZippedTriGWriter()
            : base(new TriGWriter()) { }
    }

    /// <summary>
    /// Writer for creating GZipped TriX output
    /// </summary>
    public class GZippedTriXWriter
        : BaseGZipDatasetWriter
    {
        /// <summary>
        /// Creates a new GZipped TriX output
        /// </summary>
        public GZippedTriXWriter()
            : base(new TriXWriter()) { }
    }
}

#endif