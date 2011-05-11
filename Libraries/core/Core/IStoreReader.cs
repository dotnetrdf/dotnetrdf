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

using VDS.RDF.Storage.Params;

namespace VDS.RDF
{
    /// <summary>
    /// Interface to be implemented by Triple Store Readers
    /// </summary>
    public interface IStoreReader
    {
        /// <summary>
        /// Loads a RDF dataset into a Triple Store
        /// </summary>
        /// <param name="store">Triple Store</param>
        /// <param name="parameters">Parameters indicating where the Reader should read from</param>
        /// <exception cref="RdfStorageException">May be thrown if the Parameters are not valid for this Reader</exception>
        void Load(ITripleStore store, IStoreParams parameters);

        /// <summary>
        /// Loads a RDF dataset using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="parameters">Parameters indicating where the Reader should read from</param>
        /// <exception cref="RdfStorageException">May be thrown if the Parameters are not valid for this Reader</exception>
        void Load(IRdfHandler handler, IStoreParams parameters);

        /// <summary>
        /// Event which Readers can raise when they notice syntax that is ambigious/deprecated etc which can still be parsed
        /// </summary>
        event StoreReaderWarning Warning;
    }
}
