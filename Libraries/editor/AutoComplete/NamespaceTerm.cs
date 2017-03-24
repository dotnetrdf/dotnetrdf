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
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete
{
    /// <summary>
    /// Represents information about a term defined in some namespace
    /// </summary>
    public class NamespaceTerm
    {
        /// <summary>
        /// Creates a new namespace term
        /// </summary>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <param name="term">Term</param>
        public NamespaceTerm(String namespaceUri, String term)
            : this(namespaceUri, term, String.Empty) { }

        /// <summary>
        /// Creates a new namespace term
        /// </summary>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <param name="term">Term</param>
        /// <param name="label">Label</param>
        public NamespaceTerm(String namespaceUri, String term, String label)
        {
            this.NamespaceUri = namespaceUri;
            this.Term = term;
            this.Label = label;
        }

        /// <summary>
        /// Gets the Namespace URI
        /// </summary>
        public String NamespaceUri
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the term
        /// </summary>
        public String Term
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets/Sets the label
        /// </summary>
        public String Label
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the URI string for the term
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.NamespaceUri + this.Term;
        }

        /// <summary>
        /// Gets the hash code for the term
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <summary>
        /// Determines whether a term is equal to some other object
        /// </summary>
        /// <param name="obj">Other Object</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is NamespaceTerm)
            {
                NamespaceTerm other = (NamespaceTerm)obj;
                return this.ToString().Equals(other.ToString(), StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return false;
            }
        }
    }
}
