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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace VDS.RDF.Writing.Contexts
{
    /// <summary>
    /// Interface for Store Writer Contexts
    /// </summary>
    public interface IStoreWriterContext
    {
        /// <summary>
        /// Gets the Store being written
        /// </summary>
        ITripleStore Store
        {
            get;
        }
    }

    /// <summary>
    /// Base Class for Store Writer Context Objects
    /// </summary>
    public class BaseStoreWriterContext : IStoreWriterContext
    {
        private ITripleStore _store;
        private TextWriter _output;
        /// <summary>
        /// Pretty Print Mode setting
        /// </summary>
        protected bool _prettyPrint = true;
        /// <summary>
        /// High Speed Mode setting
        /// </summary>
        protected bool _hiSpeedAllowed = true;

        /// <summary>
        /// Creates a new Base Store Writer Context with default settings
        /// </summary>
        /// <param name="store">Store to write</param>
        /// <param name="output">TextWriter being written to</param>
        public BaseStoreWriterContext(ITripleStore store, TextWriter output)
        {
            this._store = store;
            this._output = output;
        }

        /// <summary>
        /// Creates a new Base Store Writer Context with custom settings
        /// </summary>
        /// <param name="store">Store to write</param>
        /// <param name="output">TextWriter being written to</param>
        /// <param name="prettyPrint">Pretty Print Mode</param>
        /// <param name="hiSpeedAllowed">High Speed Mode</param>
        public BaseStoreWriterContext(ITripleStore store, TextWriter output, bool prettyPrint, bool hiSpeedAllowed)
            : this(store, output)
        {
            this._prettyPrint = prettyPrint;
            this._hiSpeedAllowed = hiSpeedAllowed;
        }

        /// <summary>
        /// Gets/Sets the Pretty Printing Mode used
        /// </summary>
        public bool PrettyPrint
        {
            get
            {
                return this._prettyPrint;
            }
            set
            {
                this._prettyPrint = value;
            }
        }

        /// <summary>
        /// Gets/Sets the High Speed Mode used
        /// </summary>
        public bool HighSpeedModePermitted
        {
            get
            {
                return this._hiSpeedAllowed;
            }
            set
            {
                this._hiSpeedAllowed = value;
            }
        }

        /// <summary>
        /// Gets the Store being written
        /// </summary>
        public ITripleStore Store
        {
            get
            {
                return this._store;
            }
        }

        /// <summary>
        /// Gets the TextWriter being written to
        /// </summary>
        public TextWriter Output
        {
            get
            {
                return this._output;
            }
        }

        /// <summary>
        /// Formats a URI as a String for full Output
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        public virtual String FormatUri(String u)
        {
            String uri = Uri.EscapeUriString(u);
            uri = uri.Replace(">", "\\>");
            return uri;
        }

        /// <summary>
        /// Formats a URI as a String for full Output
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        public virtual String FormatUri(Uri u)
        {
            return this.FormatUri(u.ToString());
        }
    }
}
