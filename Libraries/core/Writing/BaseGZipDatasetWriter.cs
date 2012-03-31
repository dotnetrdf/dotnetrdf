/*

Copyright Robert Vesse 2009-12
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

#if !NO_COMPRESSION

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using VDS.RDF.Storage.Params;

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
        /// <param name="parameters">Storage Parameters</param>
        public void Save(ITripleStore store, IStoreParams parameters)
        {
            if (store == null) throw new RdfOutputException("Cannot output a new Triple Store");
            if (parameters == null) throw new RdfOutputException("Cannot output using null parameters");

            if (parameters is StreamParams)
            {
                StreamParams sp = (StreamParams)parameters;
                StreamWriter output = sp.StreamWriter;

                if (output.BaseStream is GZipStream)
                {
                    this._writer.Save(store, sp);
                }
                else
                {
                    this._writer.Save(store, new StreamParams(new GZipStream(output.BaseStream, CompressionMode.Compress)));
                }
            }
            else
            {
                throw new RdfOutputException("GZip Dataset Writers can only write to StreamParams instances");
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