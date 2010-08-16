/*

Copyright Robert Vesse 2009-10
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

#if !NO_STORAGE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage.Params
{
    /// <summary>
    /// Parameters for Folder Stores
    /// </summary>
    public class FolderStoreParams : IStoreParams
    {
        private String _folder;
        private FolderStoreFormat _format;
        private int _threads = 8;

        /// <summary>
        /// Creates a new set of Folder Store Parameters
        /// </summary>
        /// <param name="folder">Folder</param>
        /// <param name="format">Format of the Folder Store</param>
        /// <param name="threads">Threads to use</param>
        public FolderStoreParams(String folder, FolderStoreFormat format, int threads)
        {
            this._folder = folder;
            this._format = format;
            if (threads > 0) this._threads = threads;
        }

        /// <summary>
        /// Creates a new set of Folder Store Parameters
        /// </summary>
        /// <param name="folder">Folder</param>
        /// <param name="format">Format of the Folder Store</param>
        /// <remarks>Defaults to 8 Threads</remarks>
        public FolderStoreParams(String folder, FolderStoreFormat format) : this(folder, format, 8) { }

        /// <summary>
        /// Creates a new set of Folder Store Parameters
        /// </summary>
        /// <param name="folder">Folder</param>
        /// <param name="threads">Threads to use</param>
        /// <remarks>Defaults to <see cref="FolderStoreFormat.AutoDetect">AutoDetect</see> format</remarks>
        public FolderStoreParams(String folder, int threads) : this(folder, FolderStoreFormat.AutoDetect, 8) { }

        /// <summary>
        /// Creates a new set of Folder Store Parameters
        /// </summary>
        /// <param name="folder">Folder</param>
        /// <remarks>Defaults to <see cref="FolderStoreFormat.AutoDetect">AutoDetect</see> format and 8 Threads</remarks>
        public FolderStoreParams(String folder) : this(folder, FolderStoreFormat.AutoDetect, 8) { }

        /// <summary>
        /// Gets the Folder
        /// </summary>
        public String Folder
        {
            get
            {
                return this._folder;
            }
        }

        /// <summary>
        /// Gets the Format
        /// </summary>
        public FolderStoreFormat Format
        {
            get
            {
                return this._format;
            }
        }

        /// <summary>
        /// Gets the number of Threads to use
        /// </summary>
        public int Threads
        {
            get
            {
                return this._threads;
            }
        }
    }
}

#endif