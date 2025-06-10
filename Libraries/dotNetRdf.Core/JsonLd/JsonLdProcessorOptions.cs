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
using VDS.RDF.JsonLd.Syntax;

namespace VDS.RDF.JsonLd;

/// <summary>
/// A collection of options for setting up the JSON-LD processor.
/// </summary>
public class JsonLdProcessorOptions
{
    /// <summary>
    /// The base IRI to use when expanding or compacting the document.
    /// </summary>
    /// <remarks>If set, this overrides the input document's IRI.</remarks>
    public Uri Base { get; set; }

    /// <summary>
    /// Flag indicating if arrays of one element should be replaced by the single value during compaction.
    /// </summary>
    /// <remarks>
    /// <para>If set to true, the JSON-LD processor replaces arrays with just one element with that element during compaction.
    /// If set to false, all arrays will remain arrays even if they have just one element. </para>
    /// <para>Defaults to true.</para>
    /// </remarks>
    public bool CompactArrays { get; set; } = true;

    /// <summary>
    /// Determines if IRIs are compacted relative to the base option or document location when compacting.
    /// </summary>
    /// <remarks>Defaults to true.</remarks>
    public bool CompactToRelative { get; set; } = true;

    /// <summary>
    /// The callback of the loader to be used to retrieve remote documents and contexts.
    /// </summary>
    /// <remarks>
    /// <para> If specified, the <see cref="DocumentLoader"/> is used to retrieve remote documents and contexts; otherwise, if not specified, the processor's built-in loader is used.</para>
    /// <para>If the function returns null or throws an exception, it will be assumed that dereferencing the IRI has failed.</para>
    /// </remarks>
    public Func<Uri, JsonLdLoaderOptions, RemoteDocument> DocumentLoader { get; set; }

    /// <summary>
    /// The callback of the loader to be used to retrieve remote documents and contexts.
    /// </summary>
    /// <remarks>This property has been replaced by the <see cref="DocumentLoader"/> property whose name matches that defined by the JSON-LD 1.1 API specification.</remarks>
    [Obsolete("The Loader property has been deprecated in favor of the DocumentLoader property.", true)]
    public Func<Uri, JsonLdLoaderOptions, RemoteDocument> Loader { get => DocumentLoader; set => DocumentLoader = value; }

    /// <summary>
    /// A context that is used to initialize the active context when expanding a document.
    /// </summary>
    public JToken ExpandContext { get; set; }

    /// <summary>
    /// Specifies whether HTML document processing should target all of the JSON-LD script elements in the document or not.
    /// </summary>
    /// <remarks>If set to true, when extracting JSON-LD script elements from HTML, unless a specific fragment identifier is targeted,
    /// extracts all encountered JSON-LD script elements using an array form, if necessary. Defaults to false.</remarks>
    public bool ExtractAllScripts { get; set; }

    /// <summary>
    /// Specifies whether special frame expansion rules should be applied during expansion and/or RDF serialization.
    /// </summary>
    /// <remarks>
    /// <para>Enables special frame processing rules for the Expansion Algorithm.</para>
    /// <para>Enables special rules for the Serialize RDF as JSON-LD Algorithm to use JSON-LD native types as values, where possible.</para>
    /// <para>Defaults to false.</para>
    /// </remarks>
    public bool FrameExpansion { get; set; }

    /// <summary>
    /// Specifies whether the processor should operate on properties in lexicographical order or not.
    /// </summary>
    /// <remarks>
    /// <para>If set to true, certain algorithm processing steps where indicated are ordered lexicographically. If false, order is not considered in processing.</para>
    /// <para>Defaults to false.</para>
    /// </remarks>
    public bool Ordered { get; set; }

    /// <summary>
    /// Get or set the processing model that the processor will use.
    /// </summary>
    /// <remarks>
    /// <para>This implementation supports only the json-ld-1.0 (using <see cref="JsonLdProcessingMode.JsonLd10"/>) and json-ld-1.1 (using <see cref="JsonLdProcessingMode.JsonLd11"/>)
    /// processing modes as defined in the JSON-LD 1.1 specification.</para>
    /// <para>Defaults to <see cref="JsonLdProcessingMode.JsonLd11"/>.</para>
    /// </remarks>
    public JsonLdProcessingMode? ProcessingMode { get; set; } = JsonLdProcessingMode.JsonLd11;

    /// <summary>
    /// If set to true, the JSON-LD processor may emit blank nodes for triple predicates, otherwise they will be omitted.
    /// </summary>
    /// <remarks>
    /// <para>This feature of JSON-LD is deprecated in JSON-LD 1.1 and may be removed in future versions of the specification.</para>
    /// <para>Default to false.</para>
    /// </remarks>
    public bool ProduceGeneralizedRdf { get; set; }

    /// <summary>
    /// Get or set the method by which literal values containing a base direction are transformed to and from RDF.
    /// </summary>
    /// <summary>
    /// <para>If set to <see cref="JsonLdRdfDirectionMode.I18NDatatype"/>, an RDF literal is generated using a datatype IRI based on https://www.w3.org/ns/i18n# with both the language tag (if present) and base direction encoded. When transforming from RDF, this datatype is decoded to create a value object containing @language (if present) and @direction.</para>
    /// <para>If set to <see cref="JsonLdRdfDirectionMode.CompoundLiteral"/>, a blank node is emitted instead of a literal, where the blank node is the subject of rdf:value, rdf:direction, and rdf:language (if present) properties. When transforming from RDF, this object is decoded to create a value object containing @language (if present) and @direction.</para>
    /// <para>If set to null, base direction is ignored in the transformations to/from RDF.</para>
    /// </summary>
    public JsonLdRdfDirectionMode? RdfDirection { get; set; }

    /// <summary>
    /// Get or set the flag that determines whether or not JSON native values should be used in literals.
    /// </summary>
    /// <remarks>
    /// <para>If enabled, causes the Serialize RDF as JSON-LD Algorithm to use native JSON values in value objects avoiding the need for an explicitly @type.</para>
    /// <para>Defaults to false.</para>
    /// </remarks>
    public bool UseNativeTypes { get; set; }

    /// <summary>
    /// Get or set the flag that controls the serialization of rdf:type properties when serializing RDF as JSON-LD.
    /// </summary>
    /// <remarks>
    /// Enables special rules for the Serialize RDF as JSON-LD Algorithm causing rdf:type properties to be kept as IRIs in the output, rather than use @type. Defaults to false.
    /// </remarks>
    public bool UseRdfType { get; set; }

    /// <summary>
    /// Get/set the value of the object embed flag used in the Framing Algorithm.
    /// </summary>
    public JsonLdEmbed Embed { get; set; } = JsonLdEmbed.Once;

    /// <summary>
    /// Get/Set the value of the explicit inclusion flag used in the Framing Algorithm.
    /// </summary>
    public bool Explicit { get; set; }

    /// <summary>
    /// Get/Set the value of the omit default flag used in the Framing Algorithm.
    /// </summary>
    public bool OmitDefault { get; set; }

    private bool? _omitGraph;
    /// <summary>
    /// Get or set the value of the omit graph flag used in the Framing Algorithm.
    /// </summary>
    /// <remarks>Defaults to false if <see cref="ProcessingMode"/> is <see cref="JsonLdProcessingMode.JsonLd10"/>, true otherwise.</remarks>
    public bool OmitGraph
    {
        get => _omitGraph ?? ProcessingMode != JsonLdProcessingMode.JsonLd10;
        set => _omitGraph = value;
    }

    /// <summary>
    /// Get or set the value of the require all flag used in the Framing Algorithm.
    /// </summary>
    public bool RequireAll { get; set; }

    /// <summary>
    /// Instead of framing a merged graph, frame only the default graph.
    /// </summary>
    public bool FrameDefault { get; set; }

    /// <summary>
    /// Removes @id from node objects where the value is a blank node identifier used only once within the document.
    /// </summary>
    public bool PruneBlankNodeIdentifiers { get; set; }

    /// <summary>
    /// The maximum number of remote context references to load during processing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If set to 0, remote context processing is disabled.
    /// If set to a value less than 0, there is no limit on the number of remote contexts that can be processed.
    /// Defaults to 10.
    /// </para>
    /// <para>
    /// This property is no longer part of the JSON-LD specification but is provided as as convenience for
    /// developers to ensure that there is a limit to remote context processing (or that remote context processing
    /// is completely disabled by setting this property to 0).
    /// </para>
    /// </remarks>
    public int RemoteContextLimit { get; set; } = 10;

    /// <summary>
    /// If json-ld processing results in a data drop, a warning is raised.
    /// </summary>
    public bool SafeMode { get; set; }

    /// <summary>
    /// Create a copy of this instance, cloning all of its values.
    /// </summary>
    /// <returns></returns>
    public JsonLdProcessorOptions Clone()
    {
        return new JsonLdProcessorOptions
        {
            Base = Base, 
            CompactArrays = CompactArrays, 
            CompactToRelative = CompactToRelative,
            DocumentLoader = DocumentLoader, 
            Embed = Embed, 
            Explicit=Explicit, 
            ExpandContext = ExpandContext, 
            ExtractAllScripts = ExtractAllScripts,
            FrameDefault=FrameDefault,
            FrameExpansion = FrameExpansion, 
            OmitDefault=OmitDefault, 
            Ordered = Ordered, 
            ProcessingMode = ProcessingMode, 
            ProduceGeneralizedRdf = ProduceGeneralizedRdf,
            RdfDirection = RdfDirection, 
            RemoteContextLimit = RemoteContextLimit, 
            RequireAll = RequireAll, 
            UseNativeTypes = UseNativeTypes, 
            UseRdfType = UseRdfType,
        };
    }
}
