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

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Parser Context for Folder Store parsers
    /// </summary>
    public class FolderStoreParserContext : BaseStoreParserContext
    {
        private Queue<String> _loadList = new Queue<String>();
        private String _folder;
        private int _threads = 8;
        private FolderStoreFormat _format = FolderStoreFormat.Turtle;
        private bool _terminated = false;

        /// <summary>
        /// Creates a new Folder Store Parser Context
        /// </summary>
        /// <param name="store">Triple Store to parse into</param>
        /// <param name="params">Folder Store Parameters</param>
        public FolderStoreParserContext(ITripleStore store, FolderStoreParams @params)
            : this(store, @params.Folder, @params.Format, @params.Threads) { }

        /// <summary>
        /// Creates a new Folder Store Parser Context
        /// </summary>
        /// <param name="store">Triple Store to parse into</param>
        /// <param name="folder">Folder</param>
        /// <param name="format">Format</param>
        /// <param name="threads">Threads to use</param>
        public FolderStoreParserContext(ITripleStore store, String folder, FolderStoreFormat format, int threads)
            : base(store)
        {
            this._folder = folder;
            this._format = format;
            this._threads = threads;
        }

        /// <summary>
        /// Creates a new Folder Store Parser Context
        /// </summary>
        /// <param name="store">Triple Store to parse into</param>
        /// <param name="folder">Folder</param>
        /// <param name="format">Format</param>
        public FolderStoreParserContext(ITripleStore store, String folder, FolderStoreFormat format)
            : this(store, folder, format, 8) { }

        /// <summary>
        /// Creates a new Folder Store Parser Context
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="params">Folder Store Parameters</param>
        public FolderStoreParserContext(IRdfHandler handler, FolderStoreParams @params)
            : this(handler, @params.Folder, @params.Format, @params.Threads) { }

        /// <summary>
        /// Creates a new Folder Store Parser Context
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="folder">Folder</param>
        /// <param name="format">Format</param>
        /// <param name="threads">Threads to use</param>
        public FolderStoreParserContext(IRdfHandler handler, String folder, FolderStoreFormat format, int threads)
            : base(handler)
        {
            this._folder = folder;
            this._format = format;
            this._threads = threads;
        }

        /// <summary>
        /// Creates a new Folder Store Parser Context
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="folder">Folder</param>
        /// <param name="format">Format</param>
        public FolderStoreParserContext(IRdfHandler handler, String folder, FolderStoreFormat format)
            : this(handler, folder, format, 8) { }

        /// <summary>
        /// Gets the Folder containing the Store
        /// </summary>
        public String Folder
        {
            get
            {
                return this._folder;
            }
        }

        /// <summary>
        /// Gets/Sets the Format of the Store
        /// </summary>
        public FolderStoreFormat Format
        {
            get
            {
                return this._format;
            }
            set
            {
                this._format = value;
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
        /// Adds a filename to the queue of files to be read
        /// </summary>
        /// <param name="filename">Filename</param>
        public void Add(String filename)
        {
            this._loadList.Enqueue(filename);
        }

        /// <summary>
        /// Gets the filename of the next file to be read
        /// </summary>
        /// <returns>Filename of next Graph to be read</returns>
        public String GetNextFilename()
        {
            String temp = null;
            try
            {
                Monitor.Enter(this._loadList);
                if (this._loadList.Count > 0)
                {
                    temp = this._loadList.Dequeue();
                }
            }
            finally
            {
                Monitor.Exit(this._loadList);
            }
            return temp;
        }

        /// <summary>
        /// Clears the Filenames that were queued to be loaded
        /// </summary>
        public void ClearFilenames()
        {
            try
            {
                Monitor.Enter(this._loadList);
                this._loadList.Clear();
            }
            finally
            {
                Monitor.Exit(this._loadList);
            }          
        }

        /// <summary>
        /// Gets whether parsing has been terminated
        /// </summary>
        public bool Terminated
        {
            get
            {
                return this._terminated;
            }
            set
            {
                if (!this._terminated)
                {
                    this._terminated = value;
                }
            }
        }
    }
}

#endif