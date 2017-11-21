/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.IO;
using System.IO.Compression;

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
    public abstract class BaseGZipDatasetWriter : IStoreWriter
    {
        private readonly IStoreWriter _writer;

        /// <summary>
        /// Creates a new GZiped Writer
        /// </summary>
        /// <param name="writer">Underlying writer</param>
        public BaseGZipDatasetWriter(IStoreWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _writer.Warning += RaiseWarning;
        }

        /// <summary>
        /// Saves a RDF Dataset as GZipped output
        /// </summary>
        /// <param name="store">Store to save</param>
        /// <param name="filename">File to save to</param>
        public void Save(ITripleStore store, string filename)
        {
            if (filename == null) throw new RdfOutputException("Cannot output to a null file");
            Save(store, new StreamWriter(new GZipStream(new FileStream(filename, FileMode.Create, FileAccess.Write), CompressionMode.Compress)), false);
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
                    _writer.Save(store, writer);
                }
                else
                {
                    _writer.Save(store, new StreamWriter(new GZipStream(writer.BaseStream, CompressionMode.Compress)));
                }
            }
            else
            {
                throw new RdfOutputException("GZip Dataset Writers can only write to StreamWriter instances");
            }
        }

        /// <summary>
        /// Saves a RDF Dataset as GZipped output
        /// </summary>
        /// <param name="store">Store to save</param>
        /// <param name="output">Writer to save to. Must be an instance of <see cref="StreamWriter"/></param>
        /// <param name="leaveOpen">Boolean flag indicating if the output stream should remain open after the output is written</param>
        public void Save(ITripleStore store, TextWriter output, bool leaveOpen)
        {
            if (store == null) throw new RdfOutputException("Cannot output a null Triple Store");
            if (output == null) throw new RdfOutputException("Cannot output to a null writer");

            if (output is StreamWriter writer)
            {
                if (writer.BaseStream is GZipStream)
                {
                    _writer.Save(store, writer, leaveOpen);
                }
                else
                {
                    _writer.Save(store, new StreamWriter(new GZipStream(writer.BaseStream, CompressionMode.Compress)), leaveOpen);
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
        private void RaiseWarning(string message)
        {
            Warning?.Invoke(message);
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
            return "GZipped " + _writer;
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
