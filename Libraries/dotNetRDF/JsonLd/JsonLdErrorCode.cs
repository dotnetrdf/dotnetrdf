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

namespace VDS.RDF.JsonLd
{
    /// <summary>
    /// Enumeration of the error code defined in the JSON-LD specification
    /// </summary>
    /// <remarks>
    /// The error codes are converted to C# canel-case as follows:
    /// (1) Replace IRI by Iri
    /// (2) Remove @ character
    /// (3) Replace "language-tagged" by "Language Tagged"
    /// (4) Split on space character and convert each token to Sentence case (first letter uppercase, remainder lower-case)
    /// (5) Join tokens with no token separator
    /// </remarks>
    public enum JsonLdErrorCode
    {
        /// <summary>
        /// Two properties which expand to the same keyword have been detected. This might occur if a keyword and an alias thereof are used at the same time.
        /// </summary>
        CollidingKeywords,
        /// <summary>
        /// The compacted document contains a list of lists as multiple lists have been compacted to the same term.
        /// </summary>
        CompactionToListOfLists,
        /// <summary>
        /// Multiple conflicting indexes have been found for the same node.
        /// </summary>
        ConflictingIndexes,
        /// <summary>
        /// A cycle in IRI mappings has been detected.
        /// </summary>
        CyclicIriMapping,
        /// <summary>
        /// An @id member was encountered whose value was not a string.
        /// </summary>
        InvalidIdValue,
        /// <summary>
        /// An @index member was encountered whose value was not a string.
        /// </summary>
        InvalidIndexValue,
        /// <summary>
        /// An invalid value for @nest has been found.
        /// </summary>
        InvalidNestValue,
        /// <summary>
        /// An invalid value for @prefix has been found.
        /// </summary>
        InvalidPrefixValue,
        /// <summary>
        /// An invalid value for an @reverse member has been detected, i.e., the value was not a dictionary.
        /// </summary>
        InvalidReverseValue,
        /// <summary>
        /// The @version key was used in a context with an out of range value.
        /// </summary>
        InvalidVersionValue,
        /// <summary>
        /// An invalid base IRI has been detected, i.e., it is neither an absolute IRI nor null.
        /// </summary>
        InvalidBaseIri,
        /// <summary>
        /// An @container member was encountered whose value was not one of the following strings: @list, @set, or @index.
        /// </summary>
        InvalidContainerMapping,
        /// <summary>
        /// The value of the default language is not a string or null and thus invalid.
        /// </summary>
        InvalidDefaultLanguage,
        /// <summary>
        /// A local context contains a term that has an invalid or missing IRI mapping.
        /// </summary>
        InvalidIriMapping,
        /// <summary>
        /// An invalid keyword alias definition has been encountered.
        /// </summary>
        InvalidKeywordAlias,
        /// <summary>
        /// An invalid value in a language map has been detected. It has to be a string or an array of strings.
        /// </summary>
        InvalidLanguageMapValue,
        /// <summary>
        /// An @language member in a term definition was encountered whose value was neither a string nor null and thus invalid.
        /// </summary>
        InvalidLanguageMapping,
        /// <summary>
        /// A language-tagged string with an invalid language value was detected.
        /// </summary>
        InvalidLanguageTaggedString,
        /// <summary>
        /// A number, true, or false with an associated language tag was detected.
        /// </summary>
        InvalidLanguageTaggedValue,
        /// <summary>
        /// An invalid local context was detected.
        /// </summary>
        InvalidLocalContext,
        /// <summary>
        /// No valid context document has been found for a referenced, remote context.
        /// </summary>
        InvalidRemoteContext,
        /// <summary>
        /// An invalid reverse property definition has been detected.
        /// </summary>
        InvalidReverseProperty,
        /// <summary>
        /// An invalid reverse property map has been detected. No keywords apart from @context are allowed in reverse property maps.
        /// </summary>
        InvalidReversePropertyMap,
        /// <summary>
        /// An invalid value for a reverse property has been detected. The value of an inverse property must be a node object.
        /// </summary>
        InvalidReversePropertyValue,
        /// <summary>
        /// The local context defined within a term definition is invalid.
        /// </summary>
        InvalidScopedContext,
        /// <summary>
        /// A set object or list object with disallowed members has been detected.
        /// </summary>
        InvalidSetOrListObject,
        /// <summary>
        /// An invalid term definition has been detected.
        /// </summary>
        InvalidTermDefinition,
        /// <summary>
        /// An @type member in a term definition was encountered whose value could not be expanded to an absolute IRI.
        /// </summary>
        InvalidTypeMapping,
        /// <summary>
        /// An invalid value for an @type member has been detected, i.e., the value was neither a string nor an array of strings.
        /// </summary>
        InvalidTypeValue,
        /// <summary>
        /// A typed value with an invalid type was detected.
        /// </summary>
        InvalidTypedValue,
        /// <summary>
        /// A value object with disallowed members has been detected.
        /// </summary>
        InvalidValueObject,
        /// <summary>
        /// An invalid value for the @value member of a value object has been detected, i.e., it is neither a scalar nor null.
        /// </summary>
        InvalidValueObjectValue,
        /// <summary>
        /// An invalid vocabulary mapping has been detected, i.e., it is neither an absolute IRI nor null.
        /// </summary>
        InvalidVocabMapping,
        /// <summary>
        /// A keyword redefinition has been detected.
        /// </summary>
        KeywordRedefinition,
        /// <summary>
        /// A list of lists was detected. List of lists are not supported in this version of JSON-LD due to the algorithmic complexity.
        /// </summary>
        ListOfLists,
        /// <summary>
        /// The document could not be loaded or parsed as JSON.
        /// </summary>
        LoadingDocumentFailed,
        /// <summary>
        /// There was a problem encountered loading a remote context.
        /// </summary>
        LoadingRemoteContextFailed,
        /// <summary>
        /// Multiple HTTP Link Headers [RFC5988] using the http://www.w3.org/ns/json-ld#context link relation have been detected.
        /// </summary>
        MultipleContextLinkHeaders,
        /// <summary>
        /// An attempt was made to change the processing mode which is incompatible with the previous specified version.
        /// </summary>
        ProcessingModeConflict,
        /// <summary>
        /// A cycle in remote context inclusions has been detected.
        /// </summary>
        RecursiveContextInclusion
    };
}
