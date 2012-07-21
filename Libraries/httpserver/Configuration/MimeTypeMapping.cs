/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
