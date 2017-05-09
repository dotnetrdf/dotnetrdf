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
        CollidingKeywords,
        CompactionToListOfLists,
        ConflictingIndexes,
        CyclicIriMapping,
        InvalidIdValue,
        InvalidIndexValue,
        InvalidNestValue,
        InvalidReverseValue,
        InvalidVersionValue,
        InvalidBaseIri,
        InvalidContainerMapping,
        InvalidDefaultLanguage,
        InvalidIriMapping,
        InvalidKeywordAlias,
        InvalidLanguageMapValue,
        InvalidLanguageMapping,
        InvalidLanguageTaggedString,
        InvalidLanguageTaggedValue,
        InvalidLocalContext,
        InvalidRemoteContext,
        InvalidReverseProperty,
        InvalidReversePropertyMap,
        InvalidReversePropertyValue,
        InvalidScopedContext,
        InvalidSetOrListObject,
        InvalidTermDefinition,
        InvalidTypeMapping,
        InvalidTypeValue,
        InvalidTypedValue,
        InvalidValueObject,
        InvalidValueObjectValue,
        InvalidVocabMapping,
        KeywordRedefinition,
        ListOfLists,
        LoadingDocumentFailed,
        LoadingRemoteContextFailed,
        MultipleContextLinkHeaders,
        ProcessingModeConflict,
        RecursiveContextInclusion
    };
}
