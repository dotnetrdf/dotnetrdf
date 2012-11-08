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

using System;

namespace VDS.Web.Configuration
{
    /// <summary>
    /// Represents a MIME Type mapping
    /// </summary>
    public class MimeTypeMapping
    {
        private String _extension;
        private String _mimeType;
        private bool _binary = false;

        /// <summary>
        /// Creates a new mapping
        /// </summary>
        /// <param name="extension">File Extension</param>
        /// <param name="mimeType">MIME Type</param>
        /// <remarks>
        /// Assumes non-binary content
        /// </remarks>
        public MimeTypeMapping(String extension, String mimeType)
            : this(extension, mimeType, false) { }

        /// <summary>
        /// Creates a new mapping
        /// </summary>
        /// <param name="extension">File Extension</param>
        /// <param name="mimeType">MIME Type</param>
        /// <param name="binary">Whether the content is binary</param>
        public MimeTypeMapping(String extension, String mimeType, bool binary)
        {
            this._extension = extension;
            if (!this._extension.StartsWith(".")) this._extension = "." + this._extension;
            if (!mimeType.Contains("/")) throw new ArgumentException("'" + mimeType + "' is not a valid Mime Type", "mimeType");
            this._mimeType = mimeType;
            this._binary = binary;
        }

        /// <summary>
        /// Gets the File Extension
        /// </summary>
        public String Extension
        {
            get
            {
                return this._extension;
            }
        }

        /// <summary>
        /// Gets the MIME Type
        /// </summary>
        public String MimeType
        {
            get
            {
                return this._mimeType;
            }
        }

        /// <summary>
        /// Gets whether the content is binary
        /// </summary>
        public bool IsBinaryData
        {
            get
            {
                return this._binary;
            }
        }
    }
}
