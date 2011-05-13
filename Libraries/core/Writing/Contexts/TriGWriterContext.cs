/*

Copyright Robert Vesse 2009-11
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
using System.IO;
using System.Linq;
using System.Text;

namespace VDS.RDF.Writing.Contexts
{
    /// <summary>
    /// Writer Context for TriG
    /// </summary>
    public class TriGWriterContext : ThreadedStoreWriterContext
    {
        private int _compressionLevel = WriterCompressionLevel.Default;
        private bool _n3compatability = false;

        /// <summary>
        /// Creates a new TriG Writer context
        /// </summary>
        /// <param name="store">Triple Store to save</param>
        /// <param name="output">TextWriter to output to</param>
        /// <param name="prettyPrint">Whether to use pretty printing</param>
        /// <param name="hiSpeedAllowed">Whether high speed mode is permitted</param>
        /// <param name="compressionLevel">Compression Level to use</param>
        /// <param name="n3compatability">Whether to enable N3 compatability mode</param>
        public TriGWriterContext(ITripleStore store, TextWriter output, bool prettyPrint, bool hiSpeedAllowed, int compressionLevel, bool n3compatability)
            : base(store, output, prettyPrint, hiSpeedAllowed)
        {
            this._compressionLevel = compressionLevel;
        }

        /// <summary>
        /// Gets/Sets the Compression Level
        /// </summary>
        public int CompressionLevel
        {
            get
            {
                return this._compressionLevel;
            }
            set
            {
                this._compressionLevel = value;
            }
        }

        /// <summary>
        /// Gets/Sets N3 Compatability Mode
        /// </summary>
        public bool N3CompatabilityMode
        {
            get
            {
                return this._n3compatability;
            }
            set
            {
                this._n3compatability = value;
            }
        }
    }
}
