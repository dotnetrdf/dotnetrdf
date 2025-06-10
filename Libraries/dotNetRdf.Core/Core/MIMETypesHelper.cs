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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Writing;

namespace VDS.RDF;

/// <summary>
/// Helper Class containing definitions of MIME Types for the various RDF Concrete Syntaxes and Content Negotation Methods.
/// </summary>
public static class MimeTypesHelper
{
    #region Constants

    /// <summary>
    /// Constant for W3C File Formats Namespace.
    /// </summary>
    private const string W3CFormatsNamespace = "http://www.w3.org/ns/formats/";

    /// <summary>
    /// MIME Type for accept any content Type.
    /// </summary>
    public const string Any = "*/*";

    /// <summary>
    /// MIME Type for URL Encoded WWW Form Content used when POSTing over HTTP.
    /// </summary>
    public const string WWWFormURLEncoded = "application/x-www-form-urlencoded";

    /// <summary>
    /// MIME Type for URL Enoded WWW Form Content used when POSTing over HTTP in UTF-8 encoding.
    /// </summary>
    public const string Utf8WWWFormURLEncoded = WWWFormURLEncoded + ";charset=utf-8";

    /// <summary>
    /// MIME Type for Multipart Form Data.
    /// </summary>
    public const string FormMultipart = "multipart/form-data";

    /// <summary>
    /// MIME Types for Turtle.
    /// </summary>
    public static readonly string[] Turtle = { "text/turtle", "application/x-turtle", "application/turtle" };

    /// <summary>
    /// MIME Types for RDF/XML.
    /// </summary>
    public static readonly string[] RdfXml = { "application/rdf+xml", "text/xml", "application/xml" };

    /// <summary>
    /// MIME Types for Notation 3.
    /// </summary>
    public static readonly string[] Notation3 = { "text/n3", "text/rdf+n3" };

    /// <summary>
    /// MIME Types for NTriples.
    /// </summary>
    public static readonly string[] NTriples = { "application/n-triples", "text/plain", "text/ntriples", "text/ntriples+turtle", "application/rdf-triples", "application/x-ntriples", "application/ntriples" };

    /// <summary>
    /// MIME Types for NQuads.
    /// </summary>
    public static readonly string[] NQuads = { "application/n-quads", "text/x-nquads" };

    /// <summary>
    /// MIME Types for TriG.
    /// </summary>
    public static readonly string[] TriG = { "application/trig", "application/x-trig" };

    /// <summary>
    /// MIME Types for TriX.
    /// </summary>
    public static readonly string[] TriX = { "application/trix" };

    /// <summary>
    /// MIME Types for RDF/JSON.
    /// </summary>
    public static readonly string[] Json = { "application/json", "text/json", "application/rdf+json" };

    /// <summary>
    /// MIME types for JSON-LD.
    /// </summary>
    public static readonly string[] JsonLd = {"application/ld+json"};

    /// <summary>
    /// MIME Types for SPARQL Result Sets.
    /// </summary>
    public static readonly string[] SparqlResults = { "application/sparql-results+xml", "application/sparql-results+json" };

    /// <summary>
    /// MIME Types for SPARQL Results XML.
    /// </summary>
    public static string[] SparqlResultsXml = { "application/sparql-results+xml" };

    /// <summary>
    /// MIME Types for SPARQL Results JSON.
    /// </summary>
    public static readonly string[] SparqlResultsJson = { "application/sparql-results+json" };

    /// <summary>
    /// MIME Types for SPARQL Boolean Result.
    /// </summary>
    public static readonly string[] SparqlResultsBoolean = { "text/boolean" };

    /// <summary>
    /// MIME Type for SPARQL Queries.
    /// </summary>
    public const string SparqlQuery = "application/sparql-query";

    /// <summary>
    /// MIME Type for SPARQL Updates.
    /// </summary>
    public const string SparqlUpdate = "application/sparql-update";

    /// <summary>
    /// MIME Types for CSV.
    /// </summary>
    public static readonly string[] Csv = { "text/csv", "text/comma-separated-values" };

    /// <summary>
    /// MIME Types for TSV.
    /// </summary>
    public static readonly string[] Tsv = { "text/tab-separated-values" };

    /// <summary>
    /// MIME Types for HTML.
    /// </summary>
    public static readonly string[] Html = { "text/html", "application/xhtml+xml" };

    /// <summary>
    /// Default File Extension for Turtle Files.
    /// </summary>
    public const string DefaultTurtleExtension = "ttl";
    /// <summary>
    /// Default File Extension for RDF/XML.
    /// </summary>
    public const string DefaultRdfXmlExtension = "rdf";
    /// <summary>
    /// Default File Extension for Notation 3.
    /// </summary>
    public const string DefaultNotation3Extension = "n3";
    /// <summary>
    /// Default File Extension for NTriples.
    /// </summary>
    public const string DefaultNTriplesExtension = "nt";
    /// <summary>
    /// Default File Extension for Json formats.
    /// </summary>
    public const string DefaultJsonExtension = "json";
    /// <summary>
    /// Default file extension for JSON-LD formats.
    /// </summary>
    public const string DefaultJsonLdExtension = "jsonld";
    /// <summary>
    /// Default File Extension for RDF/JSON.
    /// </summary>
    public const string DefaultRdfJsonExtension = "rj";
    /// <summary>
    /// Default File Extension for SPARQL XML Results Format.
    /// </summary>
    public const string DefaultSparqlXmlExtension = "srx";
    /// <summary>
    /// Default File Extension for SPARQL JSON Results Format.
    /// </summary>
    public const string DefaultSparqlJsonExtension = "srj";
    /// <summary>
    /// Default File Extension for TriG.
    /// </summary>
    public const string DefaultTriGExtension = "trig";
    /// <summary>
    /// Default File Extension for NQuads.
    /// </summary>
    public const string DefaultNQuadsExtension = "nq";
    /// <summary>
    /// Default File Extension for TriX.
    /// </summary>
    public const string DefaultTriXExtension = "xml";
    /// <summary>
    /// Default File Extension for CSV.
    /// </summary>
    public const string DefaultCsvExtension = "csv";
    /// <summary>
    /// Default File Extension for TSV.
    /// </summary>
    public const string DefaultTsvExtension = "tsv";
    /// <summary>
    /// Default File Extension for HTML.
    /// </summary>
    public const string DefaultHtmlExtension = "html";
    /// <summary>
    /// Default File Extension for XHTML.
    /// </summary>
    public const string DefaultXHtmlExtension = "xhtml";
    /// <summary>
    /// Default File Extension for GZip.
    /// </summary>
    public const string DefaultGZipExtension = "gz";
    /// <summary>
    /// Default File Extension for SPARQL Queries.
    /// </summary>
    public const string DefaultSparqlQueryExtension = "rq";
    /// <summary>
    /// Default File Extension for SPARQL Updates.
    /// </summary>
    public const string DefaultSparqlUpdateExtension = "ru";


    /// <summary>
    /// Extensions which are considered stackable.
    /// </summary>
    private static readonly string[] AllowedStackableExtensions = { DefaultGZipExtension };

    /// <summary>
    /// Charset constants.
    /// </summary>
    public const string CharsetUtf8 = "utf-8",
                        CharsetUtf16 = "utf-16";

    #endregion 

    /// <summary>
    /// List of MIME Type Definition.
    /// </summary>
    private static List<MimeTypeDefinition> _mimeTypes;
    /// <summary>
    /// Whether MIME Type Definitions have been initialised.
    /// </summary>
    private static bool _init = false;
    private static readonly object _initLock = new Graph();

    /// <summary>
    /// Checks whether something is a valid MIME Type.
    /// </summary>
    /// <param name="type">MIME Type.</param>
    /// <returns></returns>
    internal static bool IsValidMimeType(string type)
    {
        var parts = type.Split('/');
        if (parts.Length != 2) return false;
        return IsValidMimeTypePart(parts[0]) && IsValidMimeTypePart(parts[1]);
    }

    /// <summary>
    /// Determines whether the given string is valid as a type/subtype for a MIME type.
    /// </summary>
    /// <param name="part">String.</param>
    /// <returns></returns>
    internal static bool IsValidMimeTypePart(string part)
    {
        foreach (var c in part.ToCharArray())
        {
            if (c <= 31) return false;
            if (c == 127) return false;
            switch (c)
            {
                case '(':
                case ')':
                case '<':
                case '>':
                case '@':
                case ',':
                case ';':
                case ':':
                case '\\':
                case '"':
                case '/':
                case '[':
                case ']':
                case '?':
                case '=':
                case '{':
                case '}':
                case ' ':
                case '\t':
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Register a collection of <see cref="MimeTypeDefinition"/> with the helper.
    /// </summary>
    /// <param name="mimeTypes"></param>
    public static void Register(IEnumerable<MimeTypeDefinition> mimeTypes)
    {
        Init();
        lock (_initLock)
        {
            foreach (MimeTypeDefinition mimeType in mimeTypes)
            {
                var existing = _mimeTypes.FindIndex(x => x.SyntaxName.Equals(mimeType.SyntaxName));
                if (existing >= 0) _mimeTypes.RemoveAt(existing);
                _mimeTypes.Add(mimeType);
            }
        }
    }

    /// <summary>
    /// Initialises the MIME Type definitions.
    /// </summary>
    private static void Init()
    {
        lock (_initLock)
        {
            if (!_init)
            {
                _mimeTypes = new List<MimeTypeDefinition>();

                // Define NTriples
                var ntriples = new MimeTypeDefinition("NTriples", W3CFormatsNamespace + "N-Triples", NTriples, new string[] { DefaultNTriplesExtension }, typeof(NTriplesParser), null, null, typeof(NTriplesWriter), null, null);
                ntriples.Encoding = Encoding.ASCII;
                _mimeTypes.Add(ntriples);
                var ntriplesGZipped = new MimeTypeDefinition("GZipped NTriples", NTriples, new string[] { DefaultNTriplesExtension + "." + DefaultGZipExtension }, typeof(GZippedNTriplesParser), null, null, typeof(GZippedNTriplesWriter), null, null);
                _mimeTypes.Add(ntriplesGZipped);

                // Define Turtle
                _mimeTypes.Add(new MimeTypeDefinition("Turtle", W3CFormatsNamespace + "Turtle", Turtle, new string[] { DefaultTurtleExtension }, typeof(TurtleParser), null, null, typeof(CompressingTurtleWriter), null, null));
                _mimeTypes.Add(new MimeTypeDefinition("GZipped Turtle", Turtle, new string[] { DefaultTurtleExtension + "." + DefaultGZipExtension }, typeof(GZippedTurtleParser), null, null, typeof(GZippedTurtleWriter), null, null));

                // Define Notation 3
                _mimeTypes.Add(new MimeTypeDefinition("Notation 3", W3CFormatsNamespace + "N3", Notation3, new string[] { DefaultNotation3Extension }, typeof(Notation3Parser), null, null, typeof(Notation3Writer), null, null));
                _mimeTypes.Add(new MimeTypeDefinition("GZipped Notation 3", Notation3, new string[] { DefaultNotation3Extension + "." + DefaultGZipExtension }, typeof(GZippedNotation3Parser), null, null, typeof(GZippedNotation3Writer), null, null));

                // Define NQuads
                _mimeTypes.Add(new MimeTypeDefinition("NQuads", NQuads, new string[] { DefaultNQuadsExtension }, null, typeof(NQuadsParser), null, null, typeof(NQuadsWriter), null));
                _mimeTypes.Add(new MimeTypeDefinition("GZipped NQuads", NQuads, new string[] { DefaultNQuadsExtension + "." + DefaultGZipExtension }, null, typeof(GZippedNQuadsParser), null, null, typeof(GZippedNQuadsWriter), null));

                // Define TriG
                _mimeTypes.Add(new MimeTypeDefinition("TriG", TriG, new string[] { DefaultTriGExtension }, null, typeof(TriGParser), null, null, typeof(TriGWriter), null));
                _mimeTypes.Add(new MimeTypeDefinition("GZipped TriG", TriG, new string[] { DefaultTriGExtension + "." + DefaultGZipExtension }, null, typeof(GZippedTriGParser), null, null, typeof(GZippedTriGWriter), null));

                // Define TriX
                _mimeTypes.Add(new MimeTypeDefinition("TriX", TriX, new string[] { DefaultTriXExtension }, null, typeof(TriXParser), null, null, typeof(TriXWriter), null));
                _mimeTypes.Add(new MimeTypeDefinition("GZipped TriX", TriX, new string[] { DefaultTriXExtension + "." + DefaultGZipExtension }, null, typeof(GZippedTriXParser), null, null, typeof(GZippedTriXWriter), null));

                // Define SPARQL Results XML
                _mimeTypes.Add(new MimeTypeDefinition("SPARQL Results XML", W3CFormatsNamespace + "SPARQL_Results_XML", SparqlResultsXml, new string[] { DefaultSparqlXmlExtension }, null, null, typeof(SparqlXmlParser), null, null, typeof(SparqlXmlWriter)));
                _mimeTypes.Add(new MimeTypeDefinition("GZipped SPARQL Results XML", SparqlResultsXml, new string[] { DefaultSparqlXmlExtension + "." + DefaultGZipExtension }, null, null, typeof(GZippedSparqlXmlParser), null, null, typeof(GZippedSparqlXmlWriter)));

                // Define SPARQL Results JSON
                _mimeTypes.Add(new MimeTypeDefinition("SPARQL Results JSON", W3CFormatsNamespace + "SPARQL_Results_JSON", SparqlResultsJson, new string[] { DefaultSparqlJsonExtension, DefaultJsonExtension }, null, null, typeof(SparqlJsonParser), null, null, typeof(SparqlJsonWriter)));
                _mimeTypes.Add(new MimeTypeDefinition("GZipped SPARQL Results JSON", SparqlResultsJson, new string[] { DefaultSparqlJsonExtension + "." + DefaultGZipExtension, DefaultJsonExtension + "." + DefaultGZipExtension }, null, null, typeof(GZippedSparqlJsonParser), null, null, typeof(GZippedSparqlJsonWriter)));

                // Define SPARQL Boolean
                _mimeTypes.Add(new MimeTypeDefinition("SPARQL Boolean Result", SparqlResultsBoolean, Enumerable.Empty<string>(), null, null, typeof(SparqlBooleanParser), null, null, null));

                // Define RDF/XML - include SPARQL Parsers to support servers that send back incorrect MIME Type for SPARQL XML Results
                // We define this after SPARQL Results XML to ensure we favour the correct MIME type for it
                _mimeTypes.Add(new MimeTypeDefinition("RDF/XML", W3CFormatsNamespace + "RDF_XML", RdfXml, new string[] { DefaultRdfXmlExtension, "owl" }, typeof(RdfXmlParser), null, typeof(SparqlXmlParser), typeof(RdfXmlWriter), null, typeof(SparqlXmlWriter)));
                _mimeTypes.Add(new MimeTypeDefinition("GZipped RDF/XML", RdfXml, new string[] { DefaultRdfXmlExtension + "." + DefaultGZipExtension }, typeof(GZippedRdfXmlParser), null, null, typeof(GZippedRdfXmlWriter), null, null));

                // Define RDF/JSON - include SPARQL Parsers to support servers that send back incorrect MIME Type for SPARQL JSON Results
                // We define this after SPARQL Results JSON to ensure we favour the correct MIME type for it
                _mimeTypes.Add(new MimeTypeDefinition("RDF/JSON", Json, new string[] { DefaultRdfJsonExtension, DefaultJsonExtension }, typeof(RdfJsonParser), null, typeof(SparqlJsonParser), typeof(RdfJsonWriter), null, typeof(SparqlJsonWriter)));
                _mimeTypes.Add(new MimeTypeDefinition("GZipped RDF/JSON", Json, new string[] { DefaultRdfJsonExtension + "." + DefaultGZipExtension, DefaultJsonExtension + "." + DefaultGZipExtension }, typeof(GZippedRdfJsonParser), null, null, typeof(GZippedRdfJsonWriter), null, null));

                // Define JSON-LD
                _mimeTypes.Add(new MimeTypeDefinition("JSON-LD", JsonLd, new[] {DefaultJsonLdExtension, DefaultJsonExtension}, null, typeof(JsonLdParser), null, null, typeof(JsonLdWriter), null));
                _mimeTypes.Add(new MimeTypeDefinition("JSON-LD", JsonLd, new[] {DefaultJsonLdExtension + "." + DefaultGZipExtension, DefaultJsonExtension + "." + DefaultGZipExtension }, null, typeof(GZippedJsonLdParser), null, null, typeof(GZippedJsonLdWriter), null));

                // Define CSV
                _mimeTypes.Add(new MimeTypeDefinition("CSV", Csv, new string[] { DefaultCsvExtension }, null, null, typeof(SparqlCsvParser), typeof(CsvWriter), typeof(CsvStoreWriter), typeof(SparqlCsvWriter), 0.1m));
                _mimeTypes.Add(new MimeTypeDefinition("GZipped SPARQL CSV", Csv, new string[] { DefaultCsvExtension + "." + DefaultGZipExtension }, null, null, typeof(GZippedSparqlCsvParser), null, null, typeof(GZippedSparqlCsvWriter), 0.1m));

                // Define TSV
                _mimeTypes.Add(new MimeTypeDefinition("TSV", Tsv, new string[] { DefaultTsvExtension }, null, null, typeof(SparqlTsvParser), typeof(TsvWriter), typeof(TsvStoreWriter), typeof(SparqlTsvWriter), 0.1m));
                _mimeTypes.Add(new MimeTypeDefinition("GZipped TSV", Tsv, new string[] { DefaultTsvExtension + "." + DefaultGZipExtension }, null, null, typeof(GZippedSparqlTsvParser), null, null, typeof(GZippedSparqlTsvWriter), 0.1m));

                // Define HTML
                _mimeTypes.Add(new MimeTypeDefinition("HTML", W3CFormatsNamespace + "RDFa", Html, new string[] { DefaultHtmlExtension, DefaultXHtmlExtension, ".htm" }, typeof(RdfAParser), null, null, typeof(HtmlWriter), null, typeof(SparqlHtmlWriter)));
                _mimeTypes.Add(new MimeTypeDefinition("GZipped HTML", Html, new string[] { DefaultHtmlExtension + "." + DefaultGZipExtension, DefaultXHtmlExtension + "." + DefaultGZipExtension, ".htm." + DefaultGZipExtension }, typeof(GZippedRdfAParser), null, null, typeof(GZippedRdfAWriter), null, null));

                // Define GraphViz DOT
                _mimeTypes.Add(new MimeTypeDefinition("GraphViz DOT", new string[] { "text/vnd.graphviz" }, new string[] { ".gv", ".dot" }, null, null, null, typeof(GraphVizWriter), null, null));

                // Define SPARQL Query
                var qDef = new MimeTypeDefinition("SPARQL Query", new string[] { SparqlQuery }, new string[] { DefaultSparqlQueryExtension });
                qDef.SetObjectParserType<SparqlQuery>(typeof(SparqlQueryParser));
                _mimeTypes.Add(qDef);

                // Define SPARQL Update
                var uDef = new MimeTypeDefinition("SPARQL Update", new string[] { SparqlUpdate }, new string[] { DefaultSparqlUpdateExtension });
                uDef.SetObjectParserType<SparqlUpdateCommandSet>(typeof(SparqlUpdateParser));
                _mimeTypes.Add(uDef);

                _init = true;
            }
        }
     }

    /// <summary>
    /// Resets the MIME Type Definitions (the associations between file extensions, MIME types and their respective parsers and writers) to the library defaults.
    /// </summary>
    /// <remarks>
    /// <para>
    /// May be useful if you've altered the definitions and caused something to stop working as a result.
    /// </para>
    /// </remarks>
    public static void ResetDefinitions()
    {
        if (_init)
        {
            _init = false;
            _mimeTypes.Clear();
            Init();
        }
    }

    /// <summary>
    /// Gets the available MIME Type Definitions.
    /// </summary>
    public static IEnumerable<MimeTypeDefinition> Definitions
    {
        get
        {
            if (!_init) Init();

            return _mimeTypes;
        }
    }

    /// <summary>
    /// Adds a new MIME Type Definition.
    /// </summary>
    /// <param name="definition">MIME Type Definition.</param>
    public static void AddDefinition(MimeTypeDefinition definition)
    {
        if (!_init) Init();
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        _mimeTypes.Add(definition);
    }

    /// <summary>
    /// Registers a parser as the default RDF Parser for all the given MIME types and updates relevant definitions to include the MIME types and file extensions.
    /// </summary>
    /// <param name="parser">RDF Parser.</param>
    /// <param name="mimeTypes">MIME Types.</param>
    /// <param name="fileExtensions">File Extensions.</param>
    public static void RegisterParser(IRdfReader parser, IEnumerable<string> mimeTypes, IEnumerable<string> fileExtensions)
    {
        if (!_init) Init();

        if (!mimeTypes.Any()) throw new RdfException("Cannot register a parser without specifying at least 1 MIME Type");

        // Get any existing defintions that are to be altered
        IEnumerable<MimeTypeDefinition> existing = GetDefinitions(mimeTypes);
        foreach (MimeTypeDefinition def in existing)
        {
            foreach (var type in mimeTypes)
            {
                def.AddMimeType(type);
            }
            foreach (var ext in fileExtensions)
            {
                def.AddFileExtension(ext);
            }
            def.RdfParserType = parser.GetType();
        }

        // Create any new defintions
        IEnumerable<string> newTypes = mimeTypes.Where(t => !GetDefinitions(t).Any());
        if (newTypes.Any())
        {
            var newDef = new MimeTypeDefinition(string.Empty, newTypes, fileExtensions);
            newDef.RdfParserType = parser.GetType();
            AddDefinition(newDef);
        }
    }

    /// <summary>
    /// Registers a parser as the default RDF Dataset Parser for all the given MIME types and updates relevant definitions to include the MIME types and file extensions.
    /// </summary>
    /// <param name="parser">RDF Dataset Parser.</param>
    /// <param name="mimeTypes">MIME Types.</param>
    /// <param name="fileExtensions">File Extensions.</param>
    public static void RegisterParser(IStoreReader parser, IEnumerable<string> mimeTypes, IEnumerable<string> fileExtensions)
    {
        if (!_init) Init();

        if (!mimeTypes.Any()) throw new RdfException("Cannot register a parser without specifying at least 1 MIME Type");

        // Get any existing defintions that are to be altered
        IEnumerable<MimeTypeDefinition> existing = GetDefinitions(mimeTypes);
        foreach (MimeTypeDefinition def in existing)
        {
            foreach (var type in mimeTypes)
            {
                def.AddMimeType(type);
            }
            foreach (var ext in fileExtensions)
            {
                def.AddFileExtension(ext);
            }
            def.RdfDatasetParserType = parser.GetType();
        }

        // Create any new defintions
        IEnumerable<string> newTypes = mimeTypes.Where(t => !GetDefinitions(t).Any());
        if (newTypes.Any())
        {
            var newDef = new MimeTypeDefinition(string.Empty, newTypes, fileExtensions);
            newDef.RdfDatasetParserType = parser.GetType();
            AddDefinition(newDef);
        }
    }

    /// <summary>
    /// Registers a parser as the default SPARQL Rsults Parser for all the given MIME types and updates relevant definitions to include the MIME types and file extensions.
    /// </summary>
    /// <param name="parser">SPARQL Results Parser.</param>
    /// <param name="mimeTypes">MIME Types.</param>
    /// <param name="fileExtensions">File Extensions.</param>
    public static void RegisterParser(ISparqlResultsReader parser, IEnumerable<string> mimeTypes, IEnumerable<string> fileExtensions)
    {
        if (!_init) Init();

        if (!mimeTypes.Any()) throw new RdfException("Cannot register a parser without specifying at least 1 MIME Type");

        // Get any existing defintions that are to be altered
        IEnumerable<MimeTypeDefinition> existing = GetDefinitions(mimeTypes);
        foreach (MimeTypeDefinition def in existing)
        {
            foreach (var type in mimeTypes)
            {
                def.AddMimeType(type);
            }
            foreach (var ext in fileExtensions)
            {
                def.AddFileExtension(ext);
            }
            def.SparqlResultsParserType = parser.GetType();
        }

        // Create any new defintions
        IEnumerable<string> newTypes = mimeTypes.Where(t => !GetDefinitions(t).Any());
        if (newTypes.Any())
        {
            var newDef = new MimeTypeDefinition(string.Empty, newTypes, fileExtensions);
            newDef.SparqlResultsParserType = parser.GetType();
            AddDefinition(newDef);
        }
    }

    /// <summary>
    /// Registers a writer as the default RDF Writer for all the given MIME types and updates relevant definitions to include the MIME types and file extensions.
    /// </summary>
    /// <param name="writer">RDF Writer.</param>
    /// <param name="mimeTypes">MIME Types.</param>
    /// <param name="fileExtensions">File Extensions.</param>
    public static void RegisterWriter(IRdfWriter writer, IEnumerable<string> mimeTypes, IEnumerable<string> fileExtensions)
    {
        if (!_init) Init();

        if (!mimeTypes.Any()) throw new RdfException("Cannot register a writer without specifying at least 1 MIME Type");

        // Get any existing defintions that are to be altered
        IEnumerable<MimeTypeDefinition> existing = GetDefinitions(mimeTypes);
        foreach (MimeTypeDefinition def in existing)
        {
            foreach (var type in mimeTypes)
            {
                def.AddMimeType(type);
            }
            foreach (var ext in fileExtensions)
            {
                def.AddFileExtension(ext);
            }
            def.RdfWriterType = writer.GetType();
        }

        // Create any new defintions
        IEnumerable<string> newTypes = mimeTypes.Where(t => !GetDefinitions(t).Any());
        if (newTypes.Any())
        {
            var newDef = new MimeTypeDefinition(string.Empty, newTypes, fileExtensions);
            newDef.RdfWriterType = writer.GetType();
            AddDefinition(newDef);
        }
    }

    /// <summary>
    /// Registers a writer as the default RDF Dataset Writer for all the given MIME types and updates relevant definitions to include the MIME types and file extensions.
    /// </summary>
    /// <param name="writer">RDF Dataset Writer.</param>
    /// <param name="mimeTypes">MIME Types.</param>
    /// <param name="fileExtensions">File Extensions.</param>
    public static void RegisterWriter(IStoreWriter writer, IEnumerable<string> mimeTypes, IEnumerable<string> fileExtensions)
    {
        if (!_init) Init();

        if (!mimeTypes.Any()) throw new RdfException("Cannot register a writer without specifying at least 1 MIME Type");

        // Get any existing defintions that are to be altered
        IEnumerable<MimeTypeDefinition> existing = GetDefinitions(mimeTypes);
        foreach (MimeTypeDefinition def in existing)
        {
            foreach (var type in mimeTypes)
            {
                def.AddMimeType(type);
            }
            foreach (var ext in fileExtensions)
            {
                def.AddFileExtension(ext);
            }
            def.RdfDatasetWriterType = writer.GetType();
        }

        // Create any new defintions
        IEnumerable<string> newTypes = mimeTypes.Where(t => !GetDefinitions(t).Any());
        if (newTypes.Any())
        {
            var newDef = new MimeTypeDefinition(string.Empty, newTypes, fileExtensions);
            newDef.RdfDatasetWriterType = writer.GetType();
            AddDefinition(newDef);
        }
    }

    /// <summary>
    /// Registers a writer as the default SPARQL Results Writer for all the given MIME types and updates relevant definitions to include the MIME types and file extensions.
    /// </summary>
    /// <param name="writer">SPARQL Results Writer.</param>
    /// <param name="mimeTypes">MIME Types.</param>
    /// <param name="fileExtensions">File Extensions.</param>
    public static void RegisterWriter(ISparqlResultsWriter writer, IEnumerable<string> mimeTypes, IEnumerable<string> fileExtensions)
    {
        if (!_init) Init();

        if (!mimeTypes.Any()) throw new RdfException("Cannot register a writer without specifying at least 1 MIME Type");

        // Get any existing defintions that are to be altered
        IEnumerable<MimeTypeDefinition> existing = GetDefinitions(mimeTypes);
        foreach (MimeTypeDefinition def in existing)
        {
            foreach (var type in mimeTypes)
            {
                def.AddMimeType(type);
            }
            foreach (var ext in fileExtensions)
            {
                def.AddFileExtension(ext);
            }
            def.SparqlResultsWriterType = writer.GetType();
        }

        // Create any new defintions
        IEnumerable<string> newTypes = mimeTypes.Where(t => !GetDefinitions(t).Any());
        if (newTypes.Any())
        {
            var newDef = new MimeTypeDefinition(string.Empty, newTypes, fileExtensions);
            newDef.SparqlResultsWriterType = writer.GetType();
            AddDefinition(newDef);
        }
    }

    /// <summary>
    /// Gets all MIME Type definitions which support the given MIME Type.
    /// </summary>
    /// <param name="mimeType">MIME Type.</param>
    /// <returns></returns>
    public static IEnumerable<MimeTypeDefinition> GetDefinitions(string mimeType)
    {
        if (mimeType == null) return Enumerable.Empty<MimeTypeDefinition>();

        if (!_init) Init();

        var selector = MimeTypeSelector.Create(mimeType, 1);
        return (from definition in Definitions
                where definition.SupportsMimeType(selector)
                select definition);
    }

    /// <summary>
    /// Gets all MIME Type definitions which support the given MIME Types.
    /// </summary>
    /// <param name="mimeTypes">MIME Types.</param>
    /// <returns></returns>
    public static IEnumerable<MimeTypeDefinition> GetDefinitions(IEnumerable<string> mimeTypes)
    {
        if (mimeTypes == null) return Enumerable.Empty<MimeTypeDefinition>();

        if (!_init) Init();

        IEnumerable<MimeTypeSelector> selectors = MimeTypeSelector.CreateSelectors(mimeTypes);
        return (from selector in selectors
                from definition in Definitions
                where definition.SupportsMimeType(selector)
                select definition).Distinct();
    }

    /// <summary>
    /// Gets all MIME Types definitions which are associated with a given file extension.
    /// </summary>
    /// <param name="fileExt">File Extension.</param>
    /// <returns></returns>
    public static IEnumerable<MimeTypeDefinition> GetDefinitionsByFileExtension(string fileExt)
    {
        if (fileExt == null) return Enumerable.Empty<MimeTypeDefinition>();

        if (!_init) Init();

        return (from def in Definitions
                where def.SupportsFileExtension(fileExt)
                select def).Distinct();
    }

    /// <summary>
    /// Builds the String for the HTTP Accept Header that should be used when you want to ask for content in RDF formats (except Sparql Results).
    /// </summary>
    /// <returns></returns>
    public static string HttpAcceptHeader
    {
        get
        {
            if (!_init) Init();

            HashSet<MediaTypeWithQualityHeaderValue> accept = [];
            foreach (MimeTypeDefinition definition in Definitions)
            {
                if (definition.CanParseRdf)
                {
                    if (definition.Preference < 1.0m)
                    {
                        foreach (var mimeType in definition.MimeTypes)
                        {
                            accept.Add(new MediaTypeWithQualityHeaderValue(mimeType, Convert.ToDouble(definition.Preference)));
                        }
                    }
                    else
                    {
                        foreach (var mimeType in definition.MimeTypes)
                        {
                            accept.Add(new MediaTypeWithQualityHeaderValue(mimeType));
                        }
                    }
                }
            }

            accept.Add(new MediaTypeWithQualityHeaderValue("*/*", .5));

            HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> header = new HttpRequestMessage().Headers.Accept;
            foreach (MediaTypeWithQualityHeaderValue headerValue in accept)
            {
                header.Add(headerValue);
            }

            return header.ToString();
        }
    }

    /// <summary>
    /// Builds the String for the HTTP Accept Header that should be used for querying Sparql Endpoints where the response will be a SPARQL Result Set format.
    /// </summary>
    /// <returns></returns>
    public static string HttpSparqlAcceptHeader
    {
        get
        {
            if (!_init) Init();

            HashSet<MediaTypeWithQualityHeaderValue> accept = [];
            foreach (MimeTypeDefinition definition in Definitions)
            {
                if (definition.CanParseSparqlResults)
                {
                    if (definition.Preference < 1.0m)
                    {
                        foreach (var mimeType in definition.MimeTypes)
                        {
                            accept.Add(new MediaTypeWithQualityHeaderValue(mimeType, Convert.ToDouble(definition.Preference)));
                        }
                    }
                    else
                    {
                        foreach (var mimeType in definition.MimeTypes)
                        {
                            accept.Add(new MediaTypeWithQualityHeaderValue(mimeType));
                        }
                    }
                }
            }
            HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> header = new HttpRequestMessage().Headers.Accept;
            foreach (MediaTypeWithQualityHeaderValue headerValue in accept)
            {
                header.Add(headerValue);
            }
            return header.ToString();
        }
    }

    /// <summary>
    /// Builds the String for the HTTP Accept Header that should be used for making HTTP Requests where the returned data may be RDF or a SPARQL Result Set.
    /// </summary>
    /// <returns></returns>
    public static string HttpRdfOrSparqlAcceptHeader
    {
        get
        {
            if (!_init) Init();

            HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> header = new HttpRequestMessage().Headers.Accept;

            foreach (MimeTypeDefinition definition in Definitions)
            {
                if (definition.CanParseRdf || definition.CanParseSparqlResults)
                {
                    if (definition.Preference < 1.0m)
                    {
                        foreach (var mimeType in definition.MimeTypes)
                        {
                            header.Add(new MediaTypeWithQualityHeaderValue(mimeType, Convert.ToDouble(definition.Preference)));
                        }
                    }
                    else
                    {
                        foreach (var mimeType in definition.MimeTypes)
                        {
                            header.Add(new MediaTypeWithQualityHeaderValue(mimeType));
                        }
                    }
                }
            }

            header.Add(new MediaTypeWithQualityHeaderValue("*/*", .5));

            return header.ToString();
        }
    }

    /// <summary>
    /// Builds the String for the HTTP Accept Header that should be used for making HTTP Requests where the returned data will be an RDF dataset.
    /// </summary>
    public static string HttpRdfDatasetAcceptHeader
    {
        get
        {
            if (!_init) Init();

            HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> header = new HttpRequestMessage().Headers.Accept;
            var mimeTypes =
                new HashSet<string>(Definitions.Where(d => d.CanParseRdfDatasets).SelectMany(d => d.MimeTypes));
            foreach (var mimeType in mimeTypes)
            {
                header.Add(new MediaTypeWithQualityHeaderValue(mimeType));
            }
            return header.ToString();
        }
    }

    /// <summary>
    /// Builds the String for the HTTP Accept Header that should be used for making HTTP Requests where the returned data may be RDF or an RDF dataset.
    /// </summary>
    public static string HttpRdfOrDatasetAcceptHeader
    {
        get
        {
            if (!_init) Init();

            HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> header = new HttpRequestMessage().Headers.Accept;
            var mimeTypes =
                new HashSet<string>(Definitions.Where(d => d.CanParseRdf || d.CanParseRdfDatasets).SelectMany(d => d.MimeTypes));
            foreach (var mimeType in mimeTypes)
            {
                header.Add(new MediaTypeWithQualityHeaderValue(mimeType));
            }
            header.Add(new MediaTypeWithQualityHeaderValue("*/*", .5));

            return header.ToString();
        }
    }

    /// <summary>
    /// Creates a Custom HTTP Accept Header containing the given selection of MIME Types.
    /// </summary>
    /// <param name="mimeTypes">Enumeration of MIME Types to use.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> No validation is done on MIME Types so it is possible to generated a malformed header using this function.
    /// </para>
    /// </remarks>
    public static string CustomHttpAcceptHeader(IEnumerable<string> mimeTypes)
    {
        var header = new HttpRequestMessage().Headers.Accept;
        foreach (var mimeType in mimeTypes)
        {
            header.Add(new MediaTypeWithQualityHeaderValue(mimeType));
        }

        return header.ToString();
    }

    /// <summary>
    /// Creates a Custom HTTP Accept Header containing the given selection of MIME Types where those MIME Types also appear in the list of supported Types.
    /// </summary>
    /// <param name="mimeTypes">Enumeration of MIME Types to use.</param>
    /// <param name="supportedTypes">Enumeration of supported MIME Types.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> No validation is done on MIME Types so it is possible to generated a malformed header using this function.
    /// </para>
    /// <para>
    /// Use this function when you wish to generate a Custom Accept Header where the URI to which you are making requests supports a set range of URIs (given in the <paramref name="mimeTypes"/> parameter) where that range of types may exceed the range of types actually supported by the library or your response processing code.
    /// </para>
    /// </remarks>
    public static string CustomHttpAcceptHeader(IEnumerable<string> mimeTypes, IEnumerable<string> supportedTypes)
    {
        var output = new StringBuilder();
        var supported = new HashSet<string>(supportedTypes);

        foreach (var type in mimeTypes)
        {
            if (supported.Contains(type))
            {
                if (output.Length > 0) output.Append(",");
                output.Append(type);
            }
        }

        return output.ToString();
    }

    /// <summary>
    /// Creates a Custom HTTP Accept Header containing only the Accept Types supported by a specific parser.
    /// </summary>
    /// <param name="parser">RDF Parser.</param>
    /// <returns></returns>
    public static string CustomHttpAcceptHeader(IRdfReader parser)
    {
        if (!_init) Init();

        Type requiredType = parser.GetType();
        foreach (MimeTypeDefinition definition in Definitions)
        {
            if (requiredType.Equals(definition.RdfParserType))
            {
                return string.Join(",", definition.MimeTypes.ToArray());
            }
        }
        return HttpAcceptHeader;
    }

    /// <summary>
    /// Creates a Custom HTTP Accept Header containing only the Accept Types supported by a specific parser.
    /// </summary>
    /// <param name="parser">RDF Parser.</param>
    /// <returns></returns>
    public static string CustomHttpAcceptHeader(IStoreReader parser)
    {
        if (!_init) Init();

        Type requiredType = parser.GetType();
        foreach (MimeTypeDefinition definition in Definitions)
        {
            if (requiredType.Equals(definition.RdfDatasetParserType))
            {
                return string.Join(",", definition.MimeTypes.ToArray());
            }
        }
        return HttpRdfDatasetAcceptHeader;
    }

    /// <summary>
    /// Get the preferred MIME type that is registered for a specific writer.
    /// </summary>
    /// <param name="writer">RDF Writer.</param>
    /// <returns>The preferred MIME type associated with the parser.</returns>
    /// <exception cref="UnregisteredRdfWriterTypeException">Raised if the specific writer is of a type that is not associated with any registered MIME type.</exception>
    public static string GetMimeType(IRdfWriter writer)
    {
        if (writer == null) throw new ArgumentNullException(nameof(writer));
        if (!_init) Init();
        Type requiredType = writer.GetType();
        foreach (MimeTypeDefinition definition in Definitions)
        {
            if (requiredType == definition.RdfWriterType)
            {
                return definition.MimeTypes.First();
            }
        }

        throw new UnregisteredRdfWriterTypeException($"The type {requiredType} is not associated with a registered MIME Type.");
    }


    /// <summary>
    /// Gets the Enumeration of supported MIME Types for RDF Graphs.
    /// </summary>
    public static IEnumerable<string> SupportedRdfMimeTypes
    {
        get
        {
            if (!_init) Init();

            return (from definition in Definitions
                    where definition.CanParseRdf
                    from mimeType in definition.MimeTypes
                    select mimeType);
        }
    }

    /// <summary>
    /// Gets the Enumeration of supported MIME Types for RDF Datasets.
    /// </summary>
    public static IEnumerable<string> SupportedRdfDatasetMimeTypes
    {
        get
        {
            if (!_init) Init();

            return (from definition in Definitions
                    where definition.CanParseRdfDatasets
                    from mimeType in definition.MimeTypes
                    select mimeType);
        }
    }

    /// <summary>
    /// Gets the Enumeration of supported MIME Types for SPARQL Results.
    /// </summary>
    public static IEnumerable<string> SupportedSparqlMimeTypes
    {
        get
        {
            if (!_init) Init();

            return (from definition in Definitions
                    where definition.CanParseSparqlResults
                    from mimeType in definition.MimeTypes
                    select mimeType);
        }
    }

    /// <summary>
    /// Gets the Enumeration of supported MIME Types for RDF Graphs or SPARQL Results.
    /// </summary>
    public static IEnumerable<string> SupportedRdfOrSparqlMimeTypes
    {
        get
        {
            if (!_init) Init();

            return (from definition in Definitions
                    where definition.CanParseRdf || definition.CanParseSparqlResults
                    from mimeType in definition.MimeTypes
                    select mimeType);
        }
    }

    /// <summary>
    /// Generates a Filename Filter that can be used with any .Net application and includes all formats that dotNetRDF is aware of.
    /// </summary>
    /// <returns></returns>
    public static string GetFilenameFilter()
    {
        return GetFilenameFilter(_=>true, true);
    }

    /// <summary>
    /// Generates a Filename Filter that can be used with any .Net application and includes a user dictated subset of the formats that dotNetRDF is aware of.
    /// </summary>
    /// <param name="rdf">Allow RDF Graph formats (e.g. Turtle).</param>
    /// <param name="rdfDatasets">Allow RDF Dataset formats (e.g. NQuads).</param>
    /// <param name="sparqlResults">Allow SPARQL Results formats (e.g. SPARQL Results XML).</param>
    /// <param name="sparqlQuery">Allow SPARQL Query (i.e. .rq files).</param>
    /// <param name="sparqlUpdate">Allow SPARQL Update (i.e. .ru files).</param>
    /// <param name="allFiles">Allow All Files (i.e. */*).</param>
    /// <returns></returns>
    public static string GetFilenameFilter(bool rdf, bool rdfDatasets, bool sparqlResults, bool sparqlQuery, bool sparqlUpdate, bool allFiles)
    {
        return GetFilenameFilter(def => (rdf && (def.CanParseRdf || def.CanWriteRdf))
                                        || (rdfDatasets && (def.CanParseRdfDatasets || def.CanWriteRdfDatasets))
                                        || (sparqlResults && (def.CanParseSparqlResults || def.CanWriteSparqlResults))
                                        || (sparqlQuery && def.CanParseObject<SparqlQuery>())
                                        || (sparqlUpdate && def.CanParseObject<SparqlUpdateCommandSet>()), allFiles);
    }

    /// <summary>
    /// Generates a Filename Filter that can be used with any .Net application and includes a user dictated subset of the formats that dotNetRDF is aware of.
    /// </summary>
    /// <param name="typeSelector">A function that returns true for each known mime-type to be included in the filename filter.</param>
    /// <param name="allFiles">Add an All Files (i.e. *.*) option to the filename filter.</param>
    /// <returns>A string formatted as a filename filter that can be used in .NET file dialogs.</returns>
    public static string GetFilenameFilter(Func<MimeTypeDefinition, bool> typeSelector, bool allFiles)
    {
        if (!_init) Init();
        var filter = string.Empty;
        var exts = new List<string>();

        foreach (MimeTypeDefinition def in Definitions.Where(typeSelector))
        {
            exts.AddRange(def.FileExtensions);
            filter += def.SyntaxName + " Files|*." + string.Join(";*.", def.FileExtensions.ToArray()) + "|";
        }
        // Add an All Supported Formats option as first option
        filter = "All Supported Files|*." + string.Join(";*.", exts.ToArray()) + "|" + filter;

        if (allFiles)
        {
            filter += "All Files|*.*";
        }
        else
        {
            filter = filter.Substring(0, filter.Length - 1);
        }

        return filter;
    }

    /// <summary>
    /// Applies global options to a writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    /// <param name="compressionLevel">The compression level to apply to the writer if it implements the <see cref="ICompressingWriter"/> interface.</param>
    /// <param name="useDtd">Whether or not the writer should use the DTD to compress output if it implements the <see cref="IDtdWriter"/> interface.</param>
    /// <param name="useMultipleThreads">Whether or not the writer should use multiple threads to create the output. Applies only to writers that implement the <see cref="IMultiThreadedWriter"/> interface.</param>
    private static void ApplyWriterOptions(object writer, int compressionLevel, bool useDtd, bool useMultipleThreads)
    {
        if (writer is ICompressingWriter compressingWriter)
        {
            compressingWriter.CompressionLevel = compressionLevel;
        }
        if (writer is IDtdWriter dtdWriter)
        {
            dtdWriter.UseDtd = useDtd;
        }

        if (writer is IMultiThreadedWriter multiThreadedWriter)
        {
            multiThreadedWriter.UseMultiThreadedWriting = useMultipleThreads;
        }
    }

    /// <summary>
    /// Applies global options to a parser.
    /// </summary>
    /// <param name="parser">Parser.</param>
    /// <param name="tokenQueueMode">The default token queue mode used for tokeniser based parsers.</param>
    public static void ApplyParserOptions(object parser, TokenQueueMode tokenQueueMode)
    {
        if (parser is ITokenisingParser tokenisingParser)
        {
            tokenisingParser.TokenQueueMode = tokenQueueMode;
        }
    }

    /// <summary>
    /// Selects an appropriate <see cref="IRdfWriter">IRdfWriter</see> based on the given MIME Types.
    /// </summary>
    /// <param name="ctypes">MIME Types.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// This method does not take account of any quality/charset preference parameters included in the Accept Header.
    /// </para>
    /// <para>
    /// Global options pertaining to writers will be applied to the selected writer.
    /// </para>
    /// </remarks>
    public static IRdfWriter GetWriter(IEnumerable<string> ctypes)
    {
        string temp;
        return GetWriter(ctypes, out temp);
    }

    /// <summary>
    /// Selects an appropriate <see cref="IRdfWriter">IRdfWriter</see> based on the given MIME Types.
    /// </summary>
    /// <param name="ctypes">MIME Types.</param>
    /// <param name="contentType">The Content Type header that should be sent in the Response to the Request.</param>
    /// <param name="compressionLevel">The compression level to apply to the writer if it implements the <see cref="ICompressingWriter"/> interface.</param>
    /// <param name="useDtd">Whether or not the writer should use the DTD to compress output if it implements the <see cref="IDtdWriter"/> interface.</param>
    /// <param name="useMultipleThreads">Whether or not the writer should use multiple threads to create the output. Applies only to writers that implement the <see cref="IMultiThreadedWriter"/> interface.</param>
    /// <remarks>
    /// <para>
    /// This method does not take account of any quality/charset preference parameters included in the Accept Header.
    /// </para>
    /// </remarks>
    /// <returns></returns>
    public static IRdfWriter GetWriter(IEnumerable<string> ctypes, out string contentType, int compressionLevel = WriterCompressionLevel.More, bool useDtd = true, bool useMultipleThreads = false)
    {
        if (ctypes != null)
        {
            // See if there are any MIME Type Definitions for the given MIME Types
            foreach (MimeTypeDefinition definition in GetDefinitions(ctypes))
            {
                // If so return the Writer from the first match found
                if (definition.CanWriteRdf)
                {
                    IRdfWriter writer = definition.GetRdfWriter();
                    ApplyWriterOptions(writer, compressionLevel, useDtd, useMultipleThreads);
                    contentType = definition.CanonicalMimeType;
                    return writer;
                }
            }
        }

        // Default to Turtle
        contentType = Turtle[0];
        IRdfWriter defaultWriter = new CompressingTurtleWriter();
        ApplyWriterOptions(defaultWriter, compressionLevel, useDtd, useMultipleThreads);
        return defaultWriter;
    }

    /// <summary>
    /// Selects an appropriate <see cref="IRdfWriter">IRdfWriter</see> based on the HTTP Accept header form a HTTP Request.
    /// </summary>
    /// <param name="acceptHeader">Value of the HTTP Accept Header.</param>
    /// <param name="contentType">The Content Type header that should be sent in the Response to the Request.</param>
    /// <param name="compressionLevel">The compression level to apply to the writer if it implements the <see cref="ICompressingWriter"/> interface.</param>
    /// <param name="useDtd">Whether or not the writer should use the DTD to compress output if it implements the <see cref="IDtdWriter"/> interface.</param>
    /// <param name="useMultipleThreads">Whether or not the writer should use multiple threads to create the output. Applies only to writers that implement the <see cref="IMultiThreadedWriter"/> interface.</param>
    /// <returns>A Writer for a Content Type the client accepts and the Content Type that should be sent to the client.</returns>
    /// <remarks>
    /// <para>
    /// This method does not take account of any quality/charset preference parameters included in the Accept Header.
    /// </para>
    /// </remarks>
    public static IRdfWriter GetWriter(string acceptHeader, out string contentType, int compressionLevel = WriterCompressionLevel.More, bool useDtd = true, bool useMultipleThreads = false)
    {
        string[] ctypes;

        // Parse Accept Header into a String Array
        if (acceptHeader != null)
        {
            acceptHeader = acceptHeader.Trim();
            if (acceptHeader.Contains(","))
            {
                ctypes = acceptHeader.Split(',');
            }
            else
            {
                ctypes = new string[] { acceptHeader };
            }
        }
        else
        {
            ctypes = new string[] { };
        }

        return GetWriter(ctypes, out contentType, compressionLevel, useDtd, useMultipleThreads);
    }

    /// <summary>
    /// Selects an appropriate <see cref="IRdfWriter">IRdfWriter</see> based on the HTTP Accept header form a HTTP Request.
    /// </summary>
    /// <param name="acceptHeader">Value of the HTTP Accept Header.</param>
    /// <param name="compressionLevel">The compression level to apply to the writer if it implements the <see cref="ICompressingWriter"/> interface.</param>
    /// <param name="useDtd">Whether or not the writer should use the DTD to compress output if it implements the <see cref="IDtdWriter"/> interface.</param>
    /// <param name="useMultipleThreads">Whether or not the writer should use multiple threads to create the output. Applies only to writers that implement the <see cref="IMultiThreadedWriter"/> interface.</param>
    /// <returns>A Writer for a Content Type the client accepts.</returns>
    /// <remarks>
    /// <para>
    /// This method does not take account of any quality/charset preference parameters included in the Accept Header.
    /// </para>
    /// </remarks>
    public static IRdfWriter GetWriter(string acceptHeader, int compressionLevel = WriterCompressionLevel.More, bool useDtd = true, bool useMultipleThreads = false)
    {
        string temp;
        return GetWriter(acceptHeader, out temp, compressionLevel, useDtd, useMultipleThreads);
    }

    /// <summary>
    /// Selects a <see cref="IRdfWriter"/> based on the file extension.
    /// </summary>
    /// <param name="fileExt">File Extension.</param>
    /// <param name="compressionLevel">The compression level to apply to the writer if it implements the <see cref="ICompressingWriter"/> interface.</param>
    /// <param name="useDtd">Whether or not the writer should use the DTD to compress output if it implements the <see cref="IDtdWriter"/> interface.</param>
    /// <param name="useMultipleThreads">Whether or not the writer should use multiple threads to create the output. Applies only to writers that implement the <see cref="IMultiThreadedWriter"/> interface.</param>
    /// <exception cref="RdfWriterSelectionException">Thrown if no writers are associated with the given file extension.</exception>
    /// <returns></returns>
    public static IRdfWriter GetWriterByFileExtension(string fileExt, int compressionLevel = WriterCompressionLevel.More, bool useDtd = true, bool useMultipleThreads = false)
    {
        string temp;
        return GetWriterByFileExtension(fileExt, out temp, compressionLevel, useDtd, useMultipleThreads);
    }

    /// <summary>
    /// Selects a <see cref="IRdfWriter"/> based on the file extension.
    /// </summary>
    /// <param name="fileExt">File Extension.</param>
    /// <param name="contentType">Content Type of the chosen writer.</param>
    /// <param name="compressionLevel">The compression level to apply to the writer if it implements the <see cref="ICompressingWriter"/> interface.</param>
    /// <param name="useDtd">Whether or not the writer should use the DTD to compress output if it implements the <see cref="IDtdWriter"/> interface.</param>
    /// <param name="useMultipleThreads">Whether or not the writer should use multiple threads to create the output. Applies only to writers that implement the <see cref="IMultiThreadedWriter"/> interface.</param>
    /// <exception cref="RdfWriterSelectionException">Thrown if no writers are associated with the given file extension.</exception>
    /// <remarks>
    /// <para>
    /// Global options pertaining to writers will be applied to the selected writer.
    /// </para>
    /// </remarks>
    /// <returns></returns>
    public static IRdfWriter GetWriterByFileExtension(string fileExt, out string contentType, int compressionLevel = WriterCompressionLevel.More, bool useDtd = true, bool useMultipleThreads = false)
    {
        if (fileExt == null) throw new ArgumentNullException(nameof(fileExt), "File extension cannot be null");

        // See if there are any MIME Type Definition for the file extension
        foreach (MimeTypeDefinition definition in GetDefinitionsByFileExtension(fileExt))
        {
            // If so return the Writer from the first match found
            if (definition.CanWriteRdf)
            {
                IRdfWriter writer = definition.GetRdfWriter();
                ApplyWriterOptions(writer, compressionLevel, useDtd, useMultipleThreads);
                contentType = definition.CanonicalMimeType;
                return writer;
            }
        }

        // Error if unable to select
        contentType = null;
        throw new RdfWriterSelectionException("Unable to select a RDF writer, no writers are associated with the file extension '" + fileExt + "'");
    }

    /// <summary>
    /// Selects an appropriate <see cref="IRdfReader">IRdfReader</see> based on the given MIME Types.
    /// </summary>
    /// <param name="ctypes">MIME TYpes.</param>
    /// <param name="tokenQueueMode">The default token queue mode used for tokeniser based parsers.</param>
    /// <exception cref="RdfParserSelectionException">Raised if there is no </exception>
    /// <returns></returns>
    public static IRdfReader GetParser(IEnumerable<string> ctypes, TokenQueueMode tokenQueueMode = TokenQueueMode.SynchronousBufferDuringParsing)
    {
        if (ctypes != null)
        {
            foreach (MimeTypeDefinition definition in GetDefinitions(ctypes))
            {
                if (definition.CanParseRdf)
                {
                    IRdfReader parser = definition.GetRdfParser();
                    ApplyParserOptions(parser, tokenQueueMode);
                    return parser;
                }
            }
        }

        var types = (ctypes == null) ? string.Empty : string.Join(",", ctypes.ToArray());
        throw new RdfParserSelectionException("The Library does not contain any Parsers for RDF Graphs in any of the following MIME Types: " + types);
    }

    /// <summary>
    /// Selects an appropriate <see cref="IRdfReader">IRdfReader</see> based on the HTTP Content-Type header from a HTTP Response.
    /// </summary>
    /// <param name="contentType">Value of the HTTP Content-Type Header.</param>
    /// <param name="tokenQueueMode">The default token queue mode used for tokeniser based parsers.</param>
    /// <returns></returns>
    public static IRdfReader GetParser(string contentType, TokenQueueMode tokenQueueMode = TokenQueueMode.SynchronousBufferDuringParsing)
    {
        return GetParser(contentType.AsEnumerable(), tokenQueueMode);
    }

    /// <summary>
    /// Selects a <see cref="IRdfReader"/> based on the file extension.
    /// </summary>
    /// <param name="fileExt">File Extension.</param>
    /// <param name="tokenQueueMode">The default token queue mode used for tokeniser based parsers.</param>
    /// <returns></returns>
    public static IRdfReader GetParserByFileExtension(string fileExt, TokenQueueMode tokenQueueMode = TokenQueueMode.SynchronousBufferDuringParsing)
    {
        if (fileExt == null) throw new ArgumentNullException(nameof(fileExt), "File extension cannot be null");

        foreach (MimeTypeDefinition def in GetDefinitionsByFileExtension(fileExt))
        {
            if (def.CanParseRdf)
            {
                IRdfReader parser = def.GetRdfParser();
                ApplyParserOptions(parser, tokenQueueMode);
                return parser;
            }
        }

        throw new RdfParserSelectionException("The Library does not contain any Parsers for RDF Graphs associated with the File Extension '" + fileExt + "'");
    }

    /// <summary>
    /// Selects a SPARQL Parser based on the MIME types.
    /// </summary>
    /// <param name="ctypes">MIME Types.</param>
    /// <param name="allowPlainTextResults">Whether to allow for plain text results.</param>
    /// <returns></returns>
    public static ISparqlResultsReader GetSparqlParser(IEnumerable<string> ctypes, bool allowPlainTextResults)
    {
        foreach (MimeTypeDefinition definition in GetDefinitions(ctypes))
        {
            if (definition.CanParseSparqlResults)
            {
                ISparqlResultsReader parser = definition.GetSparqlResultsParser();
                return parser;
            }
        }

        if (allowPlainTextResults && (ctypes.Contains("text/plain") || ctypes.Contains("text/boolean")))
        {
            ISparqlResultsReader bParser = new SparqlBooleanParser();
            return bParser;
        }
        else
        {
            var types = (ctypes == null) ? string.Empty : string.Join(",", ctypes.ToArray());
            throw new RdfParserSelectionException("The Library does not contain any Parsers for SPARQL Results in any of the following MIME Types: " + types);
        }
    }

    /// <summary>
    /// Selects an appropriate <see cref="ISparqlResultsReader">ISparqlResultsReader</see> based on the HTTP Content-Type header from a HTTP Response.
    /// </summary>
    /// <param name="contentType">Value of the HTTP Content-Type Header.</param>
    /// <returns></returns>
    public static ISparqlResultsReader GetSparqlParser(string contentType)
    {
        return GetSparqlParser(contentType.AsEnumerable(), false);
    }

    /// <summary>
    /// Selects an appropriate <see cref="ISparqlResultsReader">ISparqlResultsReader</see> based on the HTTP Content-Type header from a HTTP Response.
    /// </summary>
    /// <param name="contentType">Value of the HTTP Content-Type Header.</param>
    /// <param name="allowPlainTextResults">Whether you allow Sparql Boolean results in text/plain format (Boolean results in text/boolean are handled properly but text/plain results can be conflated with CONSTRUCT/DESCRIBE results in NTriples format).</param>
    /// <returns></returns>
    public static ISparqlResultsReader GetSparqlParser(string contentType, bool allowPlainTextResults)
    {
        return GetSparqlParser(contentType.AsEnumerable(), allowPlainTextResults);
    }

    /// <summary>
    /// Selects a <see cref="ISparqlResultsReader"/> based on the file extension.
    /// </summary>
    /// <param name="fileExt">File Extension.</param>
    /// <returns></returns>
    public static ISparqlResultsReader GetSparqlParserByFileExtension(string fileExt)
    {
        if (fileExt == null) throw new ArgumentNullException(nameof(fileExt), "File Extension cannot be null");

        foreach (MimeTypeDefinition def in GetDefinitionsByFileExtension(fileExt))
        {
            if (def.CanParseSparqlResults)
            {
                ISparqlResultsReader parser = def.GetSparqlResultsParser();
                return parser;
            }
        }

        throw new RdfParserSelectionException("The Library does not contain a Parser for SPARQL Results associated with the file extension '" + fileExt + "'");
    }

    /// <summary>
    /// Selects an appropriate <see cref="ISparqlResultsWriter">ISparqlResultsWriter</see> based on the given MIME Types.
    /// </summary>
    /// <param name="ctypes">MIME Types.</param>
    /// <returns>A Writer for a Content Type the client accepts.</returns>
    /// <remarks>
    /// <para>
    /// This method does not take account of any quality/charset preference parameters included in the Accept Header.
    /// </para>
    /// <para>
    /// Global options pertaining to writers will be applied to the selected writer.
    /// </para>
    /// </remarks>
    public static ISparqlResultsWriter GetSparqlWriter(IEnumerable<string> ctypes)
    {
        string temp;
        return GetSparqlWriter(ctypes, out temp);
    }

    /// <summary>
    /// Selects an appropriate <see cref="ISparqlResultsWriter">ISparqlResultsWriter</see> based on the HTTP Accept header form a HTTP Request.
    /// </summary>
    /// <param name="ctypes">String array of accepted Content Types.</param>
    /// <param name="contentType">The Content Type header that should be sent in the Response to the Request.</param>
    /// <returns>A Writer for a Content Type the client accepts and the Content Type that should be sent to the client.</returns>
    /// <remarks>
    /// <para>
    /// This method does not take account of any quality/charset preference parameters included in the Accept Header.
    /// </para>
    /// </remarks>
    public static ISparqlResultsWriter GetSparqlWriter(IEnumerable<string> ctypes, out string contentType)
    {
        foreach (MimeTypeDefinition definition in GetDefinitions(ctypes))
        {
            if (definition.CanWriteSparqlResults)
            {
                contentType = definition.CanonicalMimeType;
                ISparqlResultsWriter writer = definition.GetSparqlResultsWriter();
                return writer;
            }
        }

        // Default to SPARQL XML Output
        contentType = SparqlResultsXml[0];
        ISparqlResultsWriter defaultWriter = new SparqlXmlWriter();
        return defaultWriter;
    }

    /// <summary>
    /// Selects an appropriate <see cref="ISparqlResultsWriter">ISparqlResultsWriter</see> based on the HTTP Accept header form a HTTP Request.
    /// </summary>
    /// <param name="acceptHeader">Value of the HTTP Accept Header.</param>
    /// <param name="contentType">The Content Type header that should be sent in the Response to the Request.</param>
    /// <returns>A Writer for a Content Type the client accepts and the Content Type that should be sent to the client.</returns>
    /// <remarks>
    /// <para>
    /// This method does not take account of any quality/charset preference parameters included in the Accept Header.
    /// </para>
    /// <para>
    /// Global options pertaining to writers will be applied to the selected writer.
    /// </para>
    /// </remarks>
    public static ISparqlResultsWriter GetSparqlWriter(string acceptHeader, out string contentType)
    {
        string[] ctypes;

        // Parse Accept Header into a String Array
        acceptHeader = acceptHeader.Trim();
        if (acceptHeader.Contains(","))
        {
            ctypes = acceptHeader.Split(',');
        }
        else
        {
            ctypes = new string[] { acceptHeader };
        }

        return GetSparqlWriter(ctypes, out contentType);
    }

    /// <summary>
    /// Selects an appropriate <see cref="ISparqlResultsWriter">ISparqlResultsWriter</see> based on the HTTP Accept header form a HTTP Request.
    /// </summary>
    /// <param name="acceptHeader">Value of the HTTP Accept Header.</param>
    /// <returns>A Writer for a Content Type the client accepts.</returns>
    /// <remarks>
    /// <para>
    /// This method does not take account of any quality/charset preference parameters included in the Accept Header.
    /// </para>
    /// <para>
    /// Global options pertaining to writers will be applied to the selected writer.
    /// </para>
    /// </remarks>
    public static ISparqlResultsWriter GetSparqlWriter(string acceptHeader)
    {
        string temp;
        return GetSparqlWriter(acceptHeader, out temp);
    }

    /// <summary>
    /// Selects a <see cref="ISparqlResultsWriter"/> based on a file extension.
    /// </summary>
    /// <param name="fileExt">File Extension.</param>
    /// <returns></returns>
    public static ISparqlResultsWriter GetSparqlWriterByFileExtension(string fileExt)
    {
        string temp;
        return GetSparqlWriterByFileExtension(fileExt, out temp);
    }

    /// <summary>
    /// Selects a <see cref="ISparqlResultsWriter"/> based on a file extension.
    /// </summary>
    /// <param name="fileExt">File Extension.</param>
    /// <param name="contentType">Content Type of the selected writer.</param>
    /// <returns></returns>
    public static ISparqlResultsWriter GetSparqlWriterByFileExtension(string fileExt, out string contentType)
    {
        if (fileExt == null) throw new ArgumentNullException(nameof(fileExt), "File Extension cannot be null");

        foreach (MimeTypeDefinition def in GetDefinitionsByFileExtension(fileExt))
        {
            if (def.CanWriteSparqlResults)
            {
                ISparqlResultsWriter writer = def.GetSparqlResultsWriter();
                contentType = def.CanonicalMimeType;
                return writer;
            }
        }

        throw new RdfWriterSelectionException("Unable to select a SPARQL Results Writer, no writers are associated with the file extension '" + fileExt + "'");
    }

    /// <summary>
    /// Selects a Store parser based on the MIME types.
    /// </summary>
    /// <param name="ctypes">MIME Types.</param>
    /// <param name="tokenQueueMode">The default token queue mode used for tokeniser based parsers.</param>
    /// <returns></returns>
    public static IStoreReader GetStoreParser(IEnumerable<string> ctypes, TokenQueueMode tokenQueueMode = TokenQueueMode.SynchronousBufferDuringParsing)
    {
        foreach (MimeTypeDefinition def in GetDefinitions(ctypes))
        {
            if (def.CanParseRdfDatasets)
            {
                IStoreReader parser = def.GetRdfDatasetParser();
                ApplyParserOptions(parser, tokenQueueMode);
                return parser;
            }
        }

        var types = (ctypes == null) ? string.Empty : string.Join(",", ctypes.ToArray());
        throw new RdfParserSelectionException("The Library does not contain any Parsers for RDF Datasets in any of the following MIME Types: " + types);
    }

    /// <summary>
    /// Selects an appropriate <see cref="IStoreReader">IStoreReader</see> based on the HTTP Content-Type header from a HTTP Response.
    /// </summary>
    /// <param name="contentType">Value of the HTTP Content-Type Header.</param>
    /// <param name="tokenQueueMode">The default token queue mode used for tokeniser based parsers.</param>
    /// <returns></returns>
    public static IStoreReader GetStoreParser(string contentType, TokenQueueMode tokenQueueMode = TokenQueueMode.SynchronousBufferDuringParsing)
    {
        return GetStoreParser(contentType.AsEnumerable(), tokenQueueMode);
    }

    /// <summary>
    /// Selects a Store parser based on the file extension.
    /// </summary>
    /// <param name="fileExt">File Extension.</param>
    /// <param name="tokenQueueMode">The default token queue mode used for tokeniser based parsers.</param>
    /// <returns></returns>
    public static IStoreReader GetStoreParserByFileExtension(string fileExt, TokenQueueMode tokenQueueMode = TokenQueueMode.SynchronousBufferDuringParsing)
    {
        if (fileExt == null) throw new ArgumentNullException(nameof(fileExt), "File Extension cannot be null");

        foreach (MimeTypeDefinition def in GetDefinitionsByFileExtension(fileExt))
        {
            if (def.CanParseRdfDatasets)
            {
                IStoreReader parser = def.GetRdfDatasetParser();
                ApplyParserOptions(parser, tokenQueueMode);
                return parser;
            }
        }

        throw new RdfParserSelectionException("The Library does not contain any Parsers for RDF Datasets associated with the File Extension '" + fileExt + "'");
    }

    /// <summary>
    /// Selects an appropriate <see cref="IStoreWriter">IStoreWriter</see> based on the given MIME Types.
    /// </summary>
    /// <param name="ctypes">MIME Types.</param>
    /// <param name="compressionLevel">The compression level to apply to the writer if it implements the <see cref="ICompressingWriter"/> interface.</param>
    /// <param name="useDtd">Whether or not the writer should use the DTD to compress output if it implements the <see cref="IDtdWriter"/> interface.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// This method does not take account of any quality/charset preference parameters included in the Accept Header.
    /// </para>
    /// </remarks>
    public static IStoreWriter GetStoreWriter(IEnumerable<string> ctypes, int compressionLevel = WriterCompressionLevel.More, bool useDtd = true)
    {
        string temp;
        return GetStoreWriter(ctypes, out temp, compressionLevel, useDtd);
    }

    /// <summary>
    /// Selects an appropriate <see cref="IStoreWriter">IStoreWriter</see> based on the given MIME Types.
    /// </summary>
    /// <param name="ctypes">MIME Types.</param>
    /// <param name="contentType">The Content Type header that should be sent in the Response to the Request.</param>
    /// <param name="compressionLevel">The compression level to apply to the writer if it implements the <see cref="ICompressingWriter"/> interface.</param>
    /// <param name="useDtd">Whether or not the writer should use the DTD to compress output if it implements the <see cref="IDtdWriter"/> interface.</param>
    /// <param name="useMultipleThreads">Whether or not the writer should use multiple threads to create the output. Applies only to writers that implement the <see cref="IMultiThreadedWriter"/> interface.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// This method does not take account of any quality/charset preference parameters included in the Accept Header.
    /// </para>
    /// </remarks>
    public static IStoreWriter GetStoreWriter(IEnumerable<string> ctypes, out string contentType, int compressionLevel = WriterCompressionLevel.More, bool useDtd = true, bool useMultipleThreads = false)
    {
        foreach (MimeTypeDefinition definition in GetDefinitions(ctypes))
        {
            if (definition.CanWriteRdfDatasets)
            {
                contentType = definition.CanonicalMimeType;
                IStoreWriter writer = definition.GetRdfDatasetWriter();
                ApplyWriterOptions(writer, compressionLevel, useDtd, useMultipleThreads);
                return writer;
            }
        }

        contentType = NQuads[0];
        IStoreWriter defaultWriter = new NQuadsWriter();
        ApplyWriterOptions(defaultWriter, compressionLevel, useDtd, useMultipleThreads);
        return defaultWriter;
    }

    /// <summary>
    /// Selects an appropriate <see cref="IStoreWriter">IStoreWriter</see> based on the HTTP Accept header form a HTTP Request.
    /// </summary>
    /// <param name="acceptHeader">Value of the HTTP Accept Header.</param>
    /// <param name="contentType">The Content Type header that should be sent in the Response to the Request.</param>
    /// <param name="compressionLevel">The compression level to apply to the writer if it implements the <see cref="ICompressingWriter"/> interface.</param>
    /// <param name="useDtd">Whether or not the writer should use the DTD to compress output if it implements the <see cref="IDtdWriter"/> interface.</param>
    /// <returns>A Writer for a Content Type the client accepts and the Content Type that should be sent to the client.</returns>
    /// <remarks>This method does not take account of any quality/charset preference parameters included in the Accept Header.</remarks>
    public static IStoreWriter GetStoreWriter(string acceptHeader, out string contentType, int compressionLevel = WriterCompressionLevel.More, bool useDtd = true)
    {
        string[] ctypes;

        // Parse Accept Header into a String Array
        acceptHeader = acceptHeader.Trim();
        if (acceptHeader.Contains(","))
        {
            ctypes = acceptHeader.Split(',');
        }
        else
        {
            ctypes = new string[] { acceptHeader };
        }

        return GetStoreWriter(ctypes, out contentType, compressionLevel, useDtd);
    }

    /// <summary>
    /// Selects an appropriate <see cref="IStoreWriter">IStoreWriter</see> based on the HTTP Accept header form a HTTP Request.
    /// </summary>
    /// <param name="acceptHeader">Value of the HTTP Accept Header.</param>
    /// <param name="compressionLevel">The compression level to apply to the writer if it implements the <see cref="ICompressingWriter"/> interface.</param>
    /// <param name="useDtd">Whether or not the writer should use the DTD to compress output if it implements the <see cref="IDtdWriter"/> interface.</param>
    /// <returns>A Writer for a Content Type the client accepts.</returns>
    /// <remarks>This method does not take account of any quality/charset preference parameters included in the Accept Header.</remarks>
    public static IStoreWriter GetStoreWriter(string acceptHeader, int compressionLevel = WriterCompressionLevel.More, bool useDtd = true)
    {
        return GetStoreWriter(acceptHeader, out _, compressionLevel, useDtd);
    }

    /// <summary>
    /// Selects a <see cref="IStoreWriter"/> by file extension.
    /// </summary>
    /// <param name="fileExt">File Extension.</param>
    /// <param name="compressionLevel">The compression level to apply to the writer if it implements the <see cref="ICompressingWriter"/> interface.</param>
    /// <param name="useDtd">Whether or not the writer should use the DTD to compress output if it implements the <see cref="IDtdWriter"/> interface.</param>
    /// <param name="useMultipleThreads">Whether or not the writer should use multiple threads when writing output if it implements the <see cref="IMultiThreadedWriter"/> interface.</param>
    /// <returns></returns>
    public static IStoreWriter GetStoreWriterByFileExtension(string fileExt, int compressionLevel = WriterCompressionLevel.More, bool useDtd = true, bool useMultipleThreads = false)
    {
        return GetStoreWriterByFileExtension(fileExt, out _, compressionLevel, useDtd, useMultipleThreads);
    }

    /// <summary>
    /// Selects a <see cref="IStoreWriter"/> by file extension.
    /// </summary>
    /// <param name="fileExt">File Extension.</param>
    /// <param name="contentType">Content Type of the selected writer.</param>
    /// <param name="compressionLevel">The compression level to apply to the writer if it implements the <see cref="ICompressingWriter"/> interface.</param>
    /// <param name="useDtd">Whether or not the writer should use the DTD to compress output if it implements the <see cref="IDtdWriter"/> interface.</param>
    /// <param name="useMultipleThreads">Whether or not the writer should use multiple threads to create the output. Applies only to writers that implement the <see cref="IMultiThreadedWriter"/> interface.</param>
    /// <returns></returns>
    public static IStoreWriter GetStoreWriterByFileExtension(string fileExt, out string contentType, int compressionLevel = WriterCompressionLevel.More, bool useDtd = true, bool useMultipleThreads = false)
    {
        if (fileExt == null) throw new ArgumentNullException(nameof(fileExt), "File Extension cannot be null");

        foreach (MimeTypeDefinition def in GetDefinitionsByFileExtension(fileExt))
        {
            if (def.CanWriteRdfDatasets)
            {
                IStoreWriter writer = def.GetRdfDatasetWriter();
                ApplyWriterOptions(writer, compressionLevel, useDtd, useMultipleThreads);
                contentType = def.CanonicalMimeType;
                return writer;
            }
        }

        throw new RdfWriterSelectionException("Unable to select a RDF Dataset writer, no writers are associated with the file extension '" + fileExt + "'");
    }

    /// <summary>
    /// Selects the appropriate MIME Type for the given File Extension if the File Extension is a standard extension for an RDF format.
    /// </summary>
    /// <param name="fileExt">File Extension.</param>
    /// <returns></returns>
    [Obsolete("This method is deprecated, please use GetDefinitionsForExtension() to find relevant definitions and extract the MIME types from there", true)]
    public static string GetMimeType(string fileExt)
    {
        if (!_init) Init();
        foreach (MimeTypeDefinition definition in Definitions)
        {
            if (definition.SupportsFileExtension(fileExt))
            {
                return definition.CanonicalMimeType;
            }
        }

        // Unknown File Extension
        throw new RdfParserSelectionException("Unable to determine the appropriate MIME Type for the File Extension '" + fileExt + "' as this is not a standard extension for an RDF format");
    }

    /// <summary>
    /// Gets all the MIME Types associated with a given File Extension.
    /// </summary>
    /// <param name="fileExt">File Extension.</param>
    /// <returns></returns>
    [Obsolete("This method is deprecated, please use GetDefinitionsForExtension() to find relevant definitions and extract the MIME types from there", true)]
    public static IEnumerable<string> GetMimeTypes(string fileExt)
    {
        if (!_init) Init();
        var types = new List<string>();
        foreach (MimeTypeDefinition definition in Definitions)
        {
            if (definition.SupportsFileExtension(fileExt))
            {
                types.AddRange(definition.MimeTypes);
            }
        }

        if (types.Count > 0) return types;

        // Unknown File Extension
        throw new RdfParserSelectionException("Unable to determine the appropriate MIME Type for the File Extension '" + fileExt + "' as this is not a standard extension for an RDF format");
    }


    /// <summary>
    /// Gets the true file extension for a filename.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// This is an alternative to using <see cref="System.IO.Path.GetExtension(string)"/> which is designed to take into account known extensions which are used in conjunction with other extensions and mask the true extension, for example. <strong>.gz</strong>
    /// </para>
    /// <para>
    /// Consider the filename <strong>example.ttl.gz</strong>, obtaining the extension the standard way gives only <strong>.gz</strong> which is unhelpful since it doesn't actually tell us the underlying format of the data only that it is GZipped and if it is GZipped we almost certainly want to stream the data rather than read all into memory and heuristically detect the actual format.  Instead we'd like to get <strong>.ttl.gz</strong> as the file extension which is much more useful and this is what this function does.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> This method does not blindly return double extensions whenever they are present (since they may simply by period characters in the filename and not double extensions at all) rather it returns double extensions only when the standard extension is an extension is known to be used with double extensions e.g. <strong>.gz</strong> that is relevan to the library.
    /// </para>
    /// </remarks>
    public static string GetTrueFileExtension(string filename)
    {
        var actualFilename = Path.GetFileName(filename);
        var extIndex = actualFilename.IndexOf('.');

        // If no extension(s) return standard method
        if (extIndex == -1) return Path.GetExtension(filename);

        // Otherwise get the detected extension and then check for double extensions
        var stdExt = Path.GetExtension(actualFilename);

        // Only proceed to do double extension checking if the extension is known to be stackable
        if (!AllowedStackableExtensions.Contains(stdExt.Substring(1))) return stdExt;

        var stdIndex = actualFilename.Length - stdExt.Length;

        // If the indexes match then the standard method returned the only extension present
        if (extIndex == stdIndex) return stdExt;

        // Otherwise we have a double extension
        actualFilename = actualFilename.Substring(0, stdIndex);
        var realExt = Path.GetExtension(actualFilename);

        return realExt + stdExt;
    }

    /// <summary>
    /// Gets the true extension for a resource.
    /// </summary>
    /// <param name="resource">Resource.</param>
    /// <returns></returns>
    public static string GetTrueResourceExtension(string resource)
    {
        var extIndex = resource.IndexOf('.');
        
        // if no extensions(s) return empty
        if (extIndex == -1) return string.Empty;

        // Get the standard extension
        var stdExt = resource.Substring(resource.LastIndexOf('.'));

        // Only proceed to do double extension checking if the extension is known to be stackable
        if (!AllowedStackableExtensions.Contains(stdExt.Substring(1))) return stdExt;

        var stdIndex = resource.Length - stdExt.Length;

        // If the indexes match then the standard method returned the only extension present
        if (extIndex == stdIndex) return stdExt;

        // Otherwise we have a double extension
        var partialResource = resource.Substring(0, stdIndex);
        var realExt = partialResource.Substring(partialResource.LastIndexOf('.'));

        return realExt + stdExt;
    }

    /// <summary>
    /// Selects the appropriate File Extension for the given MIME Type.
    /// </summary>
    /// <param name="mimeType">MIME Type.</param>
    /// <returns></returns>
    public static string GetFileExtension(string mimeType)
    {
        if (!_init) Init();
        var selector = MimeTypeSelector.Create(mimeType, 1);
        foreach (MimeTypeDefinition definition in Definitions)
        {
            if (definition.SupportsMimeType(selector))
            {
                return definition.CanonicalFileExtension;
            }
        }

        throw new RdfException("Unable to determine the appropriate File Extension for the MIME Type '" + mimeType + "'");
    }

    /// <summary>
    /// Selects the appropriate File Extension for the given RDF Writer.
    /// </summary>
    /// <param name="writer">RDF Writer.</param>
    /// <returns></returns>
    public static string GetFileExtension(IRdfWriter writer)
    {
        if (!_init) Init();
        Type requiredType = writer.GetType();
        foreach (MimeTypeDefinition definition in Definitions)
        {
            if (requiredType.Equals(definition.RdfWriterType))
            {
                return definition.CanonicalFileExtension;
            }
        }

        throw new RdfException("Unable to determine the appropriate File Extension for the RDF Writer '" + writer.GetType() + "'");
    }

    /// <summary>
    /// Selects the appropriate File Extension for the given Store Writer.
    /// </summary>
    /// <param name="writer">Store Writer.</param>
    /// <returns></returns>
    public static string GetFileExtension(IStoreWriter writer)
    {
        if (!_init) Init();
        Type requiredType = writer.GetType();
        foreach (MimeTypeDefinition definition in Definitions)
        {
            if (requiredType.Equals(definition.RdfDatasetWriterType))
            {
                return definition.CanonicalFileExtension;
            }
        }
            
        throw new RdfException("Unable to determine the appropriate File Extension for the Store Writer '" + writer.GetType() + "'");
    }
}
