/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
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
        event NamespaceChanged NamespaceAdded;

        /// <summary>
        /// Event which is raised when a Namespace is Modified
        /// </summary>
        event NamespaceChanged NamespaceModified;

        /// <summary>
        /// Event which is raised when a Namespace is Removed
        /// </summary>
        event NamespaceChanged NamespaceRemoved;

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
