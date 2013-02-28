/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete.Vocabularies
{
    /// <summary>
    /// Represents a vocabulary definition
    /// </summary>
    public class VocabularyDefinition
    {
        private String _prefix, _uri, _descrip;

        /// <summary>
        /// Creates a new vocabulary definition
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="uri">Namespace URI</param>
        /// <param name="description">Description</param>
        public VocabularyDefinition(String prefix, String uri, String description)
        {
            this._prefix = prefix;
            this._uri = uri;
            this._descrip = description;
        }

        /// <summary>
        /// Gets the prefix
        /// </summary>
        public String Prefix
        {
            get
            {
                return this._prefix;
            }
        }

        /// <summary>
        /// Gets the namespace URI
        /// </summary>
        public String NamespaceUri
        {
            get
            {
                return this._uri;
            }
        }

        /// <summary>
        /// Gets the description
        /// </summary>
        public String Description
        {
            get
            {
                return this._descrip;
            }
        }
    }
}
