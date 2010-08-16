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
using System.Threading;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Writing.Contexts
{
    /// <summary>
    /// Writer Context for Store Writers which write to multiple files in a folder
    /// </summary>
    public class FolderStoreWriterContext : IStoreWriterContext
    {
        private Queue<Uri> _writeList = new Queue<Uri>();
        private ITripleStore _store;
        private String _folder;
        private int _threads = 8;
        private FolderStoreFormat _format = FolderStoreFormat.Turtle;

        /// <summary>
        /// Creates a new Writer Context
        /// </summary>
        /// <param name="store">Triple Store</param>
        /// <param name="params">Folder Store Parameters</param>
        public FolderStoreWriterContext(ITripleStore store, FolderStoreParams @params)
            : this(store, @params.Folder, @params.Format, @params.Threads) { }

        /// <summary>
        /// Creates a new Writer Context
        /// </summary>
        /// <param name="store">Triple Store</param>
        /// <param name="folder">Folder to write to</param>
        /// <param name="format">Folder Store Format</param>
        /// <param name="threads">Threads to use</param>
        public FolderStoreWriterContext(ITripleStore store, String folder, FolderStoreFormat format, int threads)
            : this(store, folder, format)
        {
            this._threads = threads;
        }

        /// <summary>
        /// Creates a new Writer Context
        /// </summary>
        /// <param name="store">Triple Store</param>
        /// <param name="folder">Folder to write to</param>
        /// <param name="format">Folder Store Format</param>
        public FolderStoreWriterContext(ITripleStore store, String folder, FolderStoreFormat format)
            : this(store, folder)
        {
            this._format = format;
        }

        /// <summary>
        /// Creates a new Writer Context
        /// </summary>
        /// <param name="store">Triple Store</param>
        /// <param name="folder">Folder to write to</param>
        public FolderStoreWriterContext(ITripleStore store, String folder)
        {
            this._store = store;
            this._folder = folder;
        }

        /// <summary>
        /// Gets the Triple Store that is being written
        /// </summary>
        public ITripleStore Store
        {
            get 
            {
                return _store;
            }
        }

        /// <summary>
        /// Gets the Folder to which the Store should be written
        /// </summary>
        public String Folder
        {
            get
            {
                return this._folder;
            }
        }

        /// <summary>
        /// Gets the number of threads to use
        /// </summary>
        public int Threads
        {
            get
            {
                return this._threads;
            }
        }

        /// <summary>
        /// Gets the Format to use for files in the Folder
        /// </summary>
        public FolderStoreFormat Format
        {
            get
            {
                return this._format;
            }
        }

        /// <summary>
        /// Adds a Uri to the list of URIs for Graphs that are waiting to be written
        /// </summary>
        /// <param name="u"></param>
        public void Add(Uri u)
        {
            this._writeList.Enqueue(u);
        }

        /// <summary>
        /// Gets the next Uri for a Graph that is waiting to be written
        /// </summary>
        /// <returns>Uri of next Graph to be written</returns>
        public Uri GetNextURI()
        {
            Uri temp = null;
            try
            {
                Monitor.Enter(this._writeList);
                if (this._writeList.Count > 0)
                {
                    temp = this._writeList.Dequeue();
                }
            }
            finally
            {
                Monitor.Exit(this._writeList);
            }
            return temp;
        }
    }
}

#endif