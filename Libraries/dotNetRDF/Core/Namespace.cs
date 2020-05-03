/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2020 dotNetRDF Project (http://dotnetrdf.org/)
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
    /// Provides a convenient way to generate URIs from a fixed URI prefix and string suffix.
    /// </summary>
    public class Namespace 
    {
        /// <summary>
        /// The internal cache mapping a suffix string to its full expansion.
        /// </summary>
        private readonly Dictionary<string, string> _mappings = new Dictionary<string, string>();

        /// <summary>
        /// Get the base URI for this namespace.
        /// </summary>
        public string BaseUri { get; }

        /// <summary>
        /// Create a new namespace with the specified URI prefix.
        /// </summary>
        /// <param name="baseUri">The prefix used to generate URIs in this namespace.</param>
        public Namespace(string baseUri)
        {
            if (baseUri == null) throw new ArgumentNullException(nameof(baseUri), "Base URI must not be null");
            if (string.Empty.Equals(baseUri)) throw new ArgumentException("Base Uri must be a non-empty string", nameof(baseUri));
            BaseUri = baseUri;
        }

        /// <summary>
        /// Return a string created by concatenating <paramref name="suffix"/> with the <see cref="BaseUri"/> of this namespace instance.
        /// </summary>
        /// <param name="suffix">The string to be concatenated with the <see cref="BaseUri"/> of this namespace.</param>
        /// <returns>The concatenation of <see cref="BaseUri"/> and <paramref name="suffix"/>.</returns>
        /// <remarks>This class uses an internal cache of the concatenated strings and will return the value from that cache on repeated calls with the same suffix.</remarks>
        public string this[string suffix]
        {
            get
            {
                if (_mappings.TryGetValue(suffix, out var result))
                {
                    return result;
                }
                result = BaseUri + suffix;
                _mappings[suffix] = result;
                return result;
            }
        }

        /// <summary>
        /// The <see cref="Namespace"/> instance for the standard XML Schema namespace.
        /// </summary>
        public static readonly Namespace Xsd = new Namespace("http://www.w3.org/2001/XMLSchema#");

        /// <summary>
        /// The Namespace instance for the standard RDF namespace.
        /// </summary>
        public static readonly Namespace Rdf = new Namespace("http://www.w3.org/1999/02/22-rdf-syntax-ns#");

        /// <summary>
        /// The Namespace instance for the standard RDF Schema namespace.
        /// </summary>
        public static readonly Namespace Rdfs = new Namespace("http://www.w3.org/2000/01/rdf-schema#");


    }

}
