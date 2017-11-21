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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace VDS.RDF.JsonLd
{
    /// <summary>
    /// Represents a term definition in a context
    /// </summary>
    public class JsonLdTermDefinition
    {
        /// <summary>
        /// Get or set the IRI mapping for the term
        /// </summary>
        public string IriMapping { get; set; }

        /// <summary>
        /// Indicates if this term represents a reverse property
        /// </summary>
        public bool Reverse { get; set; }

        /// <summary>
        /// Get or set the type mapping for this term definition
        /// </summary>
        /// <remarks>May be null. MUST be null if LanguageMapping is not null</remarks>
        public string TypeMapping { get; set; }

        private string _languageMapping;
        /// <summary>
        /// Get or set the language mapping for this term definition
        /// </summary>
        /// <remarks>May be null. MUST be null if TypeMapping is not null</remarks>
        public string LanguageMapping {
            get => _languageMapping;
            set { _languageMapping = value; HasLanguageMapping = true; }
        }

        /// <summary>
        /// Boolean flag indicating if this term definition specifies a language mapping
        /// </summary>
        public bool HasLanguageMapping { get; private set; }

        /// <summary>
        /// Get or set the context specified for this term definition
        /// </summary>
        public JToken LocalContext { get; set; }

        /// <summary>
        /// Get or set the container mapping for this term definition
        /// </summary>
        public JsonLdContainer ContainerMapping { get; set; }

        /// <summary>
        /// Get or set the nest property for this term definition
        /// </summary>
        public string Nest { get; set; }

        /// <summary>
        /// Create a clone of this term defintion
        /// </summary>
        /// <returns></returns>
        public JsonLdTermDefinition Clone()
        {
            var clone = new JsonLdTermDefinition()
            {
                IriMapping = IriMapping,
                Reverse = Reverse,
                TypeMapping = TypeMapping,
                LanguageMapping = LanguageMapping,
                HasLanguageMapping = HasLanguageMapping,
                ContainerMapping = ContainerMapping,
                Nest = Nest,
                LocalContext = LocalContext?.DeepClone(), // TODO: Check if it correct to just directly clone the local context
            };
            return clone;
        }
    }
}
