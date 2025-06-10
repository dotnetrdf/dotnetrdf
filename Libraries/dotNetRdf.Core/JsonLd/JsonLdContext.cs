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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.JsonLd.Processors;
using VDS.RDF.JsonLd.Syntax;

namespace VDS.RDF.JsonLd;

/// <summary>
/// Represents a JSON-LD context.
/// </summary>
public class JsonLdContext
{
    /// <summary>
    /// The collection of active term definitions indexed by the term key.
    /// </summary>
    private readonly Dictionary<string, JsonLdTermDefinition> _termDefinitions;

    private JObject _inverseContext;

    /// <summary>
    /// Create a new empty context.
    /// </summary>
    public JsonLdContext()
    {
        _termDefinitions = new Dictionary<string, JsonLdTermDefinition>();
    }

    /// <summary>
    /// Create a new empty context with the Base and OriginalBase properties
    /// both set to the specified base URI.
    /// </summary>
    /// <param name="baseIri">The base IRI.</param>
    public JsonLdContext(Uri baseIri)
    {
        Base = OriginalBase = baseIri;
        _termDefinitions = new Dictionary<string, JsonLdTermDefinition>();
    }

    private Uri _base;
    /// <summary>
    /// Get or set the base IRI specified by this context.
    /// </summary>
    /// <remarks>The value may be a relative or an absolute IRI or null.</remarks>
    public Uri Base { 
        get => _base;
        set { _base = value; }
    }

    /// <summary>
    /// Get the base IRI that this context was originally created with.
    /// </summary>
    public Uri OriginalBase
    {
        get;
    }

    /// <summary>
    /// Get the default language code specified by this context.
    /// </summary>
    /// <remarks>May be null.</remarks>
    public string Language { get; set; }

    /// <summary>
    /// Get or set the direction used when a string does not have a direction associated with it directly.
    /// </summary>
    public LanguageDirection? BaseDirection { get; set; }

    /// <summary>
    /// Get the default vocabulary IRI.
    /// </summary>
    public string Vocab { get; set; }

    /// <summary>
    /// Get or set the version of the JSON-LD syntax specified by this context.
    /// </summary>
    public JsonLdSyntax Version { get; private set; }

    /// <summary>
    /// Get or set the previous context to be used when a non-propagated context is defined.
    /// </summary>
    public JsonLdContext PreviousContext { get; set; }

    /// <summary>
    /// An enumeration of the terms defined by this context.
    /// </summary>
    public IEnumerable<string> Terms => _termDefinitions.Keys;

    /// <summary>
    /// Add a term definition to this context.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="termDefinition"></param>
    public void AddTerm(string key, JsonLdTermDefinition termDefinition)
    {
        _termDefinitions.Add(key, termDefinition);
    }

    /// <summary>
    /// Get the inverse context for this context.
    /// </summary>
    public JObject InverseContext => _inverseContext ??= CreateInverseContext();

    /// <summary>
    /// Remove the base IRI from this context.
    /// </summary>
    /// <remarks>Sets <see cref="Base"/> to null.</remarks>
    public void RemoveBase()
    {
        Base = null;
    }

    /// <summary>
    /// Create a deep clone of this context.
    /// </summary>
    /// <returns>A new JsonLdContext that is a clone of this context.</returns>
    public JsonLdContext Clone()
    {
        var clone = new JsonLdContext(OriginalBase)
        {
            Base = Base,
            Language = Language,
            BaseDirection = BaseDirection,
            Version = Version,
            PreviousContext = PreviousContext,
            Vocab = Vocab,
        };
        foreach(KeyValuePair<string, JsonLdTermDefinition> termDefEntry in _termDefinitions)
        {
            clone.AddTerm(termDefEntry.Key, termDefEntry.Value.Clone());
        }
        return clone;
    }

    /// <summary>
    /// Add or update an existing term definition.
    /// </summary>
    /// <param name="term">The term key.</param>
    /// <param name="definition">The new value for the term definition.</param>
    public void SetTerm(string term, JsonLdTermDefinition definition)
    {
        _termDefinitions[term] = definition;
    }

    /// <summary>
    /// Remote an existing term definition.
    /// </summary>
    /// <param name="term">The key for the term to be removed.</param>
    /// <returns>The removed term definition, or null if the term was not defined in this context.</returns>
    public JsonLdTermDefinition RemoveTerm(string term)
    {
        if (!_termDefinitions.TryGetValue(term, out JsonLdTermDefinition termDefinition)) return null;
        _termDefinitions.Remove(term);
        return termDefinition;

    }

    /// <summary>
    /// Get an existing term definition.
    /// </summary>
    /// <param name="term">The key for the term to be retrieved.</param>
    /// <param name="includeAliases">Include searching for <paramref name="term"/> against the <see cref="JsonLdTermDefinition.IriMapping"/> values of the term definitions.</param>
    /// <returns>The term definition found for the specified key or a default empty term definition if there is no term definition defined for that key.</returns>
    public JsonLdTermDefinition GetTerm(string term, bool includeAliases = false)
    {
        if (_termDefinitions.TryGetValue(term, out JsonLdTermDefinition ret)) return ret;
        return includeAliases ? _termDefinitions.Values.FirstOrDefault(td => td.IriMapping.Equals(term)) : null;
    }


    /// <summary>
    /// Attempt to get an existing term definition.
    /// </summary>
    /// <param name="term">The key for the term to be retrieved.</param>
    /// <param name="termDefinition">Receives the term definition found.</param>
    /// <param name="includeAliases">Include searching for <paramref name="term"/> against the <see cref="JsonLdTermDefinition.IriMapping"/> values of the term definitions.</param>
    /// <returns>True if an entry was found for <paramref name="term"/>, false otherwise.</returns>
    public bool TryGetTerm(string term, out JsonLdTermDefinition termDefinition, bool includeAliases = false)
    {
        var foundTerm =  _termDefinitions.TryGetValue(term, out termDefinition);
        if (foundTerm || !includeAliases) return foundTerm;
        foreach (var alias in GetAliases(term))
        {
            if (_termDefinitions.TryGetValue(alias, out termDefinition)) return true;
        }
        return false;
    }

    /// <summary>
    /// Retrieve all mapped aliases for the given keyword.
    /// </summary>
    /// <param name="keyword"></param>
    /// <returns>An enumeration of the key of each term definition whose IriMapping matches the specified keyword.</returns>
    public IEnumerable<string> GetAliases(string keyword)
    {
        if (keyword == null) throw new ArgumentNullException(nameof(keyword));
        return _termDefinitions.Where(entry => keyword.Equals(entry.Value?.IriMapping)).Select(entry => entry.Key);
    }

    /// <summary>
    /// Determine if this context contains any protected term definitions.
    /// </summary>
    /// <returns>True if any term definition in this context is protected, false otherwise.</returns>
    public bool HasProtectedTerms()
    {
        return _termDefinitions.Values.Any(td => td.Protected);
    }

    /// <summary>
    /// Check if this context contains a Term Definition for a specific term.
    /// </summary>
    /// <param name="term">The term to check for.</param>
    /// <returns>True if this context contains a definition for <paramref name="term"/>, false otherwise.</returns>
    public bool HasTerm(string term)
    {
        return _termDefinitions.ContainsKey(term);
    }

    /// <summary>
    /// Implementation of the Term Selection algorithm using this instance as the active context.
    /// </summary>
    /// <param name="iri"></param>
    /// <param name="containers"></param>
    /// <param name="typeLanguage"></param>
    /// <param name="preferredValues"></param>
    /// <returns></returns>
    public string SelectTerm(string iri, IEnumerable<string> containers, string typeLanguage, List<string> preferredValues)
    {
        // 1 - If the active context has a null inverse context, set inverse context in active context to the result of calling the Inverse Context Creation algorithm using active context.
        // 2 - Initialize inverse context to the value of inverse context in active context.
        // 3 - Initialize container map to the value associated with iri in the inverse context.
        var containerMap = InverseContext[iri] as JObject;

        // 4 - For each item container in containers:
        foreach (var container in containers)
        {
            // 4.1 - If container is not an entry of container map, then there is no term with a matching container mapping for it, so continue to the next container.
            if (!containerMap.ContainsKey(container)) continue;

            // 4.2 - Initialize type/language map to the value associated with the container entry in container map.
            JToken typeLanguageMap = containerMap[container];

            // 4.3 - Initialize value map to the value associated with type/language entry in type/language map.
            var valueMap = typeLanguageMap[typeLanguage] as JObject;

            // 4.4 - For each item in preferred values:
            foreach (var item in preferredValues)
            {
                // 4.4.1 - If item is not an entry of value map, then there is no term with a matching type mapping or language mapping, so continue to the next item.
                if (!valueMap.ContainsKey(item)) continue;
                // 4.4.2 - Otherwise, a matching term has been found, return the value associated with the item member in value map.
                return valueMap[item].Value<string>();
            }
        }
        // 5 - No matching term has been found. Return null.
        return null;
    }

    private JObject CreateInverseContext()
    {
        // 1. Initialize result to an empty map.
        var result = new JObject();

        // 2. Initialize default language to @none. If the active context has a default language, set default language to the default language from the active context normalized to lower case.
        var defaultLanguage = "@none";
        if (Language != null) defaultLanguage = Language.ToLowerInvariant();

        // 3. For each key term and value term definition in the active context, ordered by shortest term first (breaking ties by choosing the lexicographically least term):
        foreach (var term in Terms.OrderBy(t => t.Length).ThenBy(t => t))
        {
            JsonLdTermDefinition termDefinition = GetTerm(term);
            // 3.1 - If the term definition is null, term cannot be selected during compaction, so continue to the next term.
            // KA: Term definitions with a null IriMapping are not used for IRI expansion (or compaction) so they should be skipped too.
            if (termDefinition?.IriMapping == null)
            {
                continue;
            }
            // 3.2 - Initialize container to @none. If there is a container mapping in term definition, set container to its associated value.
            var container = termDefinition.ContainerMapping.Any()
                ? termDefinition.ContainerMapping.Select(ContainerAsString).OrderBy(s => s)
                    .Aggregate(string.Empty, (seed, add) => seed + add)
                : "@none";

            // 3.3 - Initialize var to the value of the IRI mapping for the term definition.
            var iri = termDefinition.IriMapping;

            // 3.4 - If var is not an entry of result, add an entry where the key is var and the value is an empty map to result.
            if (result.Property(iri) == null)
            {
                result.Add(iri, new JObject());
            }

            // 3.5 - Reference the value associated with the iri member in result using the variable container map.
            var containerMap = result[iri] as JObject;

            // 3.6 - If container map has no container entry, create one and set its value to a new map with three entries.
            // The first entry is @language and its value is a new empty map, the second entry is @type and its value is a new empty map,
            // and the third entry is @any and its value is a new map with the entry @none set to the term being processed.
            if (containerMap.Property(container) == null)
            {
                containerMap.Add(container, new JObject(
                    new JProperty("@language", new JObject()),
                    new JProperty("@type", new JObject()),
                    new JProperty("@any", new JObject(new JProperty("@none", term)))));
            }

            // 3.7 - Reference the value associated with the container member in container map using the variable type/language map.
            var typeLanguageMap = containerMap[container] as JObject;

            // 3.8 - Reference the value associated with the @type member in type/language map using the variable type map.
            var typeMap = typeLanguageMap["@type"] as JObject;

            // 3.9 - Reference the value associated with the @language entry in type/language map using the variable language map.
            var languageMap = typeLanguageMap["@language"] as JObject;

            // 3.10 - If the term definition indicates that the term represents a reverse property:
            if (termDefinition.Reverse)
            {
                // 3.10.1 - If type map does not have an @reverse entry, create one and set its value to the term being processed.
                if (typeMap.Property("@reverse") == null)
                {
                    typeMap.Add("@reverse", term);
                }
            }
            // 3. 11 - Otherwise, if term definition has a type mapping which is @none: 
            else if ("@none".Equals(termDefinition.TypeMapping))
            {
                // 3.11.1 - If language map does not have an @any entry, create one and set its value to the term being processed.
                if (!languageMap.ContainsKey("@any"))
                {
                    languageMap.Add(new JProperty("@any", term));
                }
                // 3.11.2 - If type map does not have an @any entry, create one and set its value to the term being processed.
                if (!typeMap.ContainsKey("@any"))
                {
                    typeMap.Add(new JProperty("@any", term));
                }
            }
            // 3.12 - Otherwise, if term definition has a type mapping:
            else if (termDefinition.TypeMapping != null)
            {
                // 3.12.1 - If type map does not have an entry corresponding to the type mapping in term definition, create one and set its value to the term being processed.
                if (typeMap.Property(termDefinition.TypeMapping) == null)
                {
                    typeMap.Add(termDefinition.TypeMapping, term);
                }
            }
            // 3.13 - Otherwise, if term definition has both a language mapping and a direction mapping: 
            else if (termDefinition.HasLanguageMapping && termDefinition.DirectionMapping.HasValue)
            {
                string langDir;
                if (termDefinition.LanguageMapping != null &&
                    termDefinition.DirectionMapping != LanguageDirection.Unspecified)
                {
                    langDir = termDefinition.LanguageMapping.ToLowerInvariant() + "_" +
                              JsonLdUtils.SerializeLanguageDirection(termDefinition.DirectionMapping.Value);
                }
                else if (termDefinition.LanguageMapping != null)
                {
                    langDir = termDefinition.LanguageMapping.ToLowerInvariant();
                }
                else if (termDefinition.DirectionMapping != LanguageDirection.Unspecified)
                {
                    langDir = "_" + JsonLdUtils.SerializeLanguageDirection(termDefinition.DirectionMapping.Value);
                }
                else
                {
                    langDir = "@null";
                }

                if (!languageMap.ContainsKey(langDir))
                {
                    languageMap.Add(new JProperty(langDir, term));
                }
            }
            // 3.14 - Otherwise, if term definition has a language mapping (might be null):
            else if (termDefinition.HasLanguageMapping)
            {
                // 3.14.1 - If the language mapping equals null, set language to @null; otherwise set it to the language code in language mapping,  normalized to lower case.
                var language = termDefinition.LanguageMapping?.ToLowerInvariant() ?? "@null";
                // 3.14.2 - If language map does not have a language member, create one and set its value to the term being processed.
                if (languageMap.Property(language) == null)
                {
                    languageMap.Add(language, term);
                }
            }
            // 3.15 - Otherwise, if term definition has a direction mapping (might be null): 
            else if (termDefinition.DirectionMapping.HasValue)
            {
                // 3.15.1 If the direction mapping equals null, set direction to @none; otherwise to direction mapping preceded by an underscore ("_").
                var direction = termDefinition.DirectionMapping.Value == LanguageDirection.Unspecified
                    ? "@none"
                    : "_" + JsonLdUtils.SerializeLanguageDirection(termDefinition.DirectionMapping.Value);
                if (!languageMap.ContainsKey(direction))
                {
                    languageMap.Add(new JProperty(direction, term));
                }
            }
            // 3.16 - Otherwise, if active context has a default base direction: 
            else if (BaseDirection.HasValue)
            {
                // 3.16.1 - Initialize a variable lang dir with the concatenation of default language and default base direction, separate by an underscore ("_"), normalized to lower case.
                var langDir = (Language?.ToLowerInvariant() ?? string.Empty) + "_" +
                              JsonLdUtils.SerializeLanguageDirection(BaseDirection.Value);
                // 3.16.2 - If language map does not have a lang dir entry, create one and set its value to the term being processed.
                if (!languageMap.ContainsKey(langDir))
                {
                    languageMap.Add(new JProperty(langDir, term));
                }
                // 3.16.3 - If language map does not have an @none entry, create one and set its value to the term being processed.
                if (!languageMap.ContainsKey("@none"))
                {
                    languageMap.Add(new JProperty("@none", term));
                }
                // 3.16.4 - If type map does not have an @none entry, create one and set its value to the term being processed.
                if (!typeMap.ContainsKey("@none"))
                {
                    typeMap.Add(new JProperty("@none", term));
                }
            }
            // 3.17 - Otherwise
            else
            {
                // 3.17.1 - If language map does not have a default language entry (after being normalized to lower case), create one and set its value to the term being processed.
                if (!languageMap.ContainsKey(defaultLanguage.ToLowerInvariant()))
                {
                    languageMap.Add(new JProperty(defaultLanguage.ToLowerInvariant(), term));
                }

                // 3.17.2 - If language map does not have an @none member, create one and set its value to the term being processed.
                if (!languageMap.ContainsKey("@none"))
                {
                    languageMap.Add(new JProperty("@none", term));
                }

                // 3.17.3 - If type map does not have an @none member, create one and set its value to the term being processed.
                if (!typeMap.ContainsKey("@none"))
                {
                    typeMap.Add(new JProperty("@none", term));
                }
            }
        }
        return result;
    }

    private static string ContainerAsString(JsonLdContainer container)
    {
        switch (container)
        {
            case JsonLdContainer.Graph:
                return "@graph";
            case JsonLdContainer.Type:
                return "@type";
            case JsonLdContainer.Id:
                return "@id";
            case JsonLdContainer.Index:
                return "@index";
            case JsonLdContainer.Language:
                return "@language";
            case JsonLdContainer.List:
                return "@list";
            case JsonLdContainer.Set:
                return "@set";
            default:
                return "@none";
        }
    }
}
