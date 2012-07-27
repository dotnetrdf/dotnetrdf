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
using System.IO;
using VDS.RDF.Storage.Params;

namespace VDS.RDF
{    
    /// <summary>
    /// Interface to be implemented by Triple Store Writers
    /// </summary>
    public interface IStoreWriter
    {
        /// <summary>
        /// Method for saving data to a Triple Store
        /// </summary>
        /// <param name="store">Triple Store</param>
        /// <param name="parameters">Parameters indicating where the Writer should write to</param> 
        /// <exception cref="RdfStorageException">May be thrown if the Parameters are not valid for this Writer</exception>
        [Obsolete("This overload is considered obsolete, please use alternative overloads", true)]
        void Save(ITripleStore store, IStoreParams parameters);

        /// <summary>
        /// Method for saving data to a Triple Store
        /// </summary>
        /// <param name="store">Triple Store</param>
        /// <param name="filename">File to save to</param>
        void Save(ITripleStore store, String filename);

        /// <summary>
        /// Method for saving data to a Triple Store
        /// </summary>
        /// <param name="store">Triple Store</param>
        /// <param name="output">Write to save to</param>
        void Save(ITripleStore store, TextWriter output);

        /// <summary>
        /// Event which writers can raise to indicate possible ambiguities or issues in the syntax they are producing
        /// </summary>
        event StoreWriterWarning Warning;
    }
}
