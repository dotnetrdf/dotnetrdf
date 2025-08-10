/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using Newtonsoft.Json.Linq;
using VDS.RDF.JsonLd.Syntax;

namespace VDS.RDF.JsonLd;

/// <summary>
/// Represents a term definition in a context.
/// </summary>
public class JsonLdTermDefinition
{
    /// <summary>
    /// Create a new term definition.
    /// </summary>
    public JsonLdTermDefinition()
    {
        ContainerMapping = new HashSet<JsonLdContainer>();
    }

    /// <summary>
    /// Get or set the IRI mapping for the term.
    /// </summary>
    public string IriMapping { get; set; }

    /// <summary>
    /// Get or set the prefix flag for the term.
    /// </summary>
    public bool Prefix { get; set; }

    /// <summary>
    /// Get or set the protected flag for the term.
    /// </summary>
    public bool Protected { get; set; }

    /// <summary>
    /// Get or set the base URL for the term.
    /// </summary>
    public Uri BaseUrl { get; set; }

    /// <summary>
    /// Indicates if this term represents a reverse property.
    /// </summary>
    public bool Reverse { get; set; }

    /// <summary>
    /// Get or set the container mapping for this term definition.
    /// </summary>
    public ISet<JsonLdContainer> ContainerMapping { get; }

    /// <summary>
    /// Get or set the text direction mapping for this term definition.
    /// </summary>
    public LanguageDirection? DirectionMapping { get; set; }

    /// <summary>
    /// Get or set the type mapping for this term definition.
    /// </summary>
    /// <remarks>May be null. MUST be null if LanguageMapping is not null.</remarks>
    public string TypeMapping { get; set; }

    /// <summary>
    /// Get or set the index mapping for this term definition.
    /// </summary>
    public string IndexMapping { get; set; }

    private string _languageMapping;
    /// <summary>
    /// Get or set the language mapping for this term definition.
    /// </summary>
    /// <remarks>May be null. MUST be null if TypeMapping is not null.</remarks>
    public string LanguageMapping {
        get => _languageMapping;
        set { _languageMapping = value; HasLanguageMapping = true; }
    }

    /// <summary>
    /// Boolean flag indicating if this term definition specifies a language mapping.
    /// </summary>
    public bool HasLanguageMapping { get; private set; }

    /// <summary>
    /// Get or set the context specified for this term definition.
    /// </summary>
    public JToken LocalContext { get; set; }

    /// <summary>
    /// Get or set the nest property for this term definition.
    /// </summary>
    public string Nest { get; set; }

    /// <summary>
    /// Create a clone of this term definition.
    /// </summary>
    /// <returns></returns>
    public JsonLdTermDefinition Clone()
    {
        var clone = new JsonLdTermDefinition()
        {
            IriMapping = IriMapping,
            Prefix = Prefix,
            Protected = Protected,
            BaseUrl =  BaseUrl,
            Reverse = Reverse,
            DirectionMapping = DirectionMapping,
            TypeMapping = TypeMapping,
            IndexMapping = IndexMapping,
            LanguageMapping = LanguageMapping,
            HasLanguageMapping = HasLanguageMapping,
            Nest = Nest,
            LocalContext = LocalContext?.DeepClone(), // TODO: Check if it correct to just directly clone the local context
        };
        clone.ContainerMapping.UnionWith(ContainerMapping);
        return clone;
    }

    /// <summary>
    /// Determines whether this term definition is the same as another term definition in all respects other than its <see cref="Protected"/> attribute value.
    /// </summary>
    /// <param name="other"></param>
    /// <returns>True if the term definitions are the same in all respects other than the value of <see cref="Protected"/>, false otherwise.</returns>
    public bool EquivalentTo(JsonLdTermDefinition other)
    {
        return IriMapping == other.IriMapping &&
               Prefix == other.Prefix &&
               BaseUrl == other.BaseUrl &&
               Reverse == other.Reverse &&
               DirectionMapping == other.DirectionMapping &&
               TypeMapping == other.TypeMapping &&
               IndexMapping == other.IndexMapping &&
               HasLanguageMapping == other.HasLanguageMapping &&
               Nest == other.Nest &&
               (ContainerMapping == null && other.ContainerMapping == null ||
                ContainerMapping != null && other.ContainerMapping != null &&
                ContainerMapping.SetEquals(other.ContainerMapping));
    }
}
