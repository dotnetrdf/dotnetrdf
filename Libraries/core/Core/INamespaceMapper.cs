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

namespace VDS.RDF
{
    /// <summary>
    /// Interface for Namespace Maps which provide mappings between Namespace Prefixes and Namespace URIs
    /// </summary>
    public interface INamespaceMapper : IDisposable
    {
        /// <summary>
        /// Adds a Namespace to the Namespace Map
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="uri">Namespace Uri</param>
        void AddNamespace(string prefix, Uri uri);

        /// <summary>
        /// Clears the Namespace Map
        /// </summary>
        void Clear();

        /// <summary>
        /// Returns the Namespace URI associated with the given Prefix
        /// </summary>
        /// <param name="prefix">The Prefix to lookup the Namespace URI for</param>
        /// <returns>URI for the Namespace</returns>
        Uri GetNamespaceUri(string prefix);

        /// <summary>
        /// Returns the Prefix associated with the given Namespace URI
        /// </summary>
        /// <param name="uri">The Namespace URI to lookup the Prefix for</param>
        /// <returns>String prefix for the Namespace</returns>
        String GetPrefix(Uri uri);

        /// <summary>
        /// Method which checks whether a given Namespace Prefix is defined
        /// </summary>
        /// <param name="prefix">Prefix to test</param>
        /// <returns></returns>
        bool HasNamespace(string prefix);

        /// <summary>
        /// Imports the contents of another Namespace Map into this Namespace Map
        /// </summary>
        /// <param name="nsmap">Namespace Map to import</param>
        /// <remarks>
        /// Prefixes in the imported Map which are already defined in this Map are ignored, this may change in future releases.
        /// </remarks>
        void Import(INamespaceMapper nsmap);

        /// <summary>
        /// Event which is raised when a Namespace is Added
        /// </summary>
        event VDS.RDF.NamespaceChanged NamespaceAdded;

        /// <summary>
        /// Event which is raised when a Namespace is Modified
        /// </summary>
        event VDS.RDF.NamespaceChanged NamespaceModified;

        /// <summary>
        /// Event which is raised when a Namespace is Removed
        /// </summary>
        event VDS.RDF.NamespaceChanged NamespaceRemoved;

        /// <summary>
        /// Gets a Enumeratorion of all the Prefixes
        /// </summary>
        IEnumerable<string> Prefixes 
        { 
            get; 
        }

        /// <summary>
        /// A Function which attempts to reduce a Uri to a QName
        /// </summary>
        /// <param name="uri">The Uri to attempt to reduce</param>
        /// <param name="qname">The value to output the QName to if possible</param>
        /// <returns></returns>
        /// <remarks>
        /// This function will return a Boolean indicated whether it succeeded in reducing the Uri to a QName.  If it did then the out parameter qname will contain the reduction, otherwise it will be the empty string.
        /// </remarks>
        bool ReduceToQName(string uri, out string qname);

        /// <summary>
        /// Removes a Namespace from the Namespace Map
        /// </summary>
        /// <param name="prefix">Namespace Prefix of the Namespace to remove</param>
        void RemoveNamespace(string prefix);
    }
}
