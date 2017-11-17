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
using System.Linq;

namespace VDS.RDF.JsonLd
{
    /// <summary>
    /// Represents a JSON-LD context
    /// </summary>
    public class JsonLdContext
    {
        /// <summary>
        /// The collection of active term definitions indexed by the term key
        /// </summary>
        private Dictionary<string, JsonLdTermDefinition> _termDefinitions;

        /// <summary>
        /// Create a new empty context
        /// </summary>
        public JsonLdContext()
        {
            _termDefinitions = new Dictionary<string, JsonLdTermDefinition>();
        }

        private Uri _base;
        /// <summary>
        /// Get or set the base IRI specified by this context
        /// </summary>
        /// <remarks>The value may be a relative or an absolute IRI or null</remarks>
        public Uri Base { get { return _base; } set { _base = value; HasBase = true; } }

        /// <summary>
        /// Returns true if the Base property of this context has been explicitly set.
        /// </summary>
        public bool HasBase { get; private set; }

        /// <summary>
        /// Get the default language code specified by this context
        /// </summary>
        /// <remarks>May be null</remarks>
        public string Language { get; set; }

        /// <summary>
        /// Get the default vocabulary IRI
        /// </summary>
        public string Vocab { get; set; }

        /// <summary>
        /// Get or set the version of the JSON-LD syntax specified by this context
        /// </summary>
        public JsonLdSyntax Version { get; private set; }

        /// <summary>
        /// An enumeration of the terms defined by this context
        /// </summary>
        public IEnumerable<string> Terms => _termDefinitions.Keys;

        /// <summary>
        /// Add a term definition to this context
        /// </summary>
        /// <param name="key"></param>
        /// <param name="termDefinition"></param>
        public void AddTerm(string key, JsonLdTermDefinition termDefinition)
        {
            _termDefinitions.Add(key, termDefinition);
        }

        /// <summary>
        /// Create a deep clone of this context
        /// </summary>
        /// <returns>A new JsonLdContext that is a clone of this context</returns>
        public JsonLdContext Clone()
        {
            var clone = new JsonLdContext
            {
                Base = Base,
                HasBase = HasBase,
                Language = Language,
                Version = Version,
                Vocab = Vocab,
            };
            foreach(var termDefEntry in _termDefinitions)
            {
                clone.AddTerm(termDefEntry.Key, termDefEntry.Value.Clone());
            }
            return clone;
        }

        /// <summary>
        /// Add or update an existing term definition
        /// </summary>
        /// <param name="term">The term key</param>
        /// <param name="definition">The new value for the term definition</param>
        public void SetTerm(string term, JsonLdTermDefinition definition)
        {
            _termDefinitions[term] = definition;
        }

        /// <summary>
        /// Remote an existing term definition
        /// </summary>
        /// <param name="term">The key for the term to be removed</param>
        public void RemoveTerm(string term)
        {
            _termDefinitions.Remove(term);
        }

        /// <summary>
        /// Get an existing term defintiion
        /// </summary>
        /// <param name="term">The key for the term to be retrieved</param>
        /// <returns>The term definition found for the specified key or null if there is no term definition defined for that key</returns>
        public JsonLdTermDefinition GetTerm(string term)
        {
            return _termDefinitions.TryGetValue(term, out var ret) ? ret : null;
        }

        /// <summary>
        /// Attempt to get an existing term defintion
        /// </summary>
        /// <param name="term">The key for the term to be retrieved</param>
        /// <param name="termDefinition">Receives the term definition found</param>
        /// <returns>True if an entry was found for <paramref name="term"/>, false otherwise</returns>
        public bool TryGetTerm(string term, out JsonLdTermDefinition termDefinition)
        {
            return _termDefinitions.TryGetValue(term, out termDefinition);
        }

        /// <summary>
        /// Retrieve all mapped aliases for the given keyword
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns>An enumeration of the key of each term definition whose IriMapping matches the specified keyword.</returns>
        public IEnumerable<string> GetAliases(string keyword)
        {
            if (keyword == null) throw new ArgumentNullException(nameof(keyword));
            return _termDefinitions.Where(entry => keyword.Equals(entry.Value?.IriMapping)).Select(entry => entry.Key);
        }
    }

}
