/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF
{
    /// <summary>
    /// Helper Class containing arrays of MIME Types for the various RDF Concrete Syntaxes and Content Negotation Methods
    /// </summary>
    /// <remarks>
    /// The first type in each array is the canonical type that should be used
    /// </remarks>
    public class MimeTypesHelper
    {
        /// <summary>
        /// Constant for Valid MIME Types
        /// </summary>
        internal const String ValidMimeTypePattern = @"[\w\-]+/\w+(-\w+)*";

        #region Default MIME Type Definitions

        /// <summary>
        /// List of MIME Type Definition
        /// </summary>
        private static List<MimeTypeDefinition> _mimeTypes;
        /// <summary>
        /// Whether MIME Type Definitions have been initialised
        /// </summary>
        private static bool _init = false;

        /// <summary>
        /// Initialises the MIME Type definitions
        /// </summary>
        private static void Init()
        {
            if (!_init)
            {
                _mimeTypes = new List<MimeTypeDefinition>();
                
                //Define RDF/XML
                _mimeTypes.Add(new MimeTypeDefinition("RDF/XML", RdfXml, new String[] { DefaultRdfXmlExtension, ".owl" }, typeof(RdfXmlParser), null, null, typeof(RdfXmlWriter), null, null));                

                //Define NTriples
                MimeTypeDefinition ntriples = new MimeTypeDefinition("NTriples", NTriples, new String[] { DefaultNTriplesExtension }, typeof(NTriplesParser), null, null, typeof(NTriplesWriter), null, null);
#if !SILVERLIGHT
                ntriples.Encoding = Encoding.ASCII;
#endif
                _mimeTypes.Add(ntriples);

                //Define Turtle
                _mimeTypes.Add(new MimeTypeDefinition("Turtle", Turtle, new String[] { DefaultTurtleExtension }, typeof(TurtleParser), null, null, typeof(CompressingTurtleWriter), null, null));

                //Define Notation 3
                _mimeTypes.Add(new MimeTypeDefinition("Notation 3", Notation3, new String[] { DefaultNotation3Extension }, typeof(Notation3Parser), null, null, typeof(Notation3Writer), null, null));

                //Define RDF/JSON - include SPARQL Parsers to support servers that send back incorrect MIME Type for SPARQL JSON Results
                _mimeTypes.Add(new MimeTypeDefinition("RDF/JSON", Json, new String[] { DefaultJsonExtension }, typeof(RdfJsonParser), null, typeof(SparqlJsonParser), typeof(RdfJsonWriter), null, typeof(SparqlJsonWriter)));

                //Define NQuads
                _mimeTypes.Add(new MimeTypeDefinition("NQuads", NQuads, new String[] { DefaultNQuadsExtension }, null, typeof(NQuadsParser), null, null, typeof(NQuadsWriter), null));

                //Define TriG
                _mimeTypes.Add(new MimeTypeDefinition("TriG", TriG, new String[] { DefaultTriGExtension }, null, typeof(TriGParser), null, null, typeof(TriGWriter), null));

                //Define TriX
                _mimeTypes.Add(new MimeTypeDefinition("TriX", TriX, new String[] { DefaultTriXExtension }, null, typeof(TriXParser), null, null, typeof(TriXWriter), null));

                //Define SPARQL Results XML
                _mimeTypes.Add(new MimeTypeDefinition("SPARQL Results XML", SparqlXml, new String[] { DefaultSparqlXmlExtension }, null, null, typeof(SparqlXmlParser), null, null, typeof(SparqlXmlWriter)));

                //Define SPARQL Results JSON
                _mimeTypes.Add(new MimeTypeDefinition("SPARQL Results JSON", SparqlJson, new String[] { DefaultJsonExtension }, null, null, typeof(SparqlJsonParser), null, null, typeof(SparqlJsonWriter)));

                //Define CSV
                _mimeTypes.Add(new MimeTypeDefinition("CSV", Csv, new String[] { DefaultCsvExtension }, null, null, null, typeof(CsvWriter), typeof(CsvStoreWriter), typeof(SparqlCsvWriter)));

                //Define TSV
                _mimeTypes.Add(new MimeTypeDefinition("TSV", Tsv, new String[] { DefaultTsvExtension }, null, null, null, typeof(TsvWriter), typeof(TsvStoreWriter), typeof(SparqlTsvWriter)));

                //Define HTML
                _mimeTypes.Add(new MimeTypeDefinition("HTML", Html, new String[] { DefaultHtmlExtension, DefaultXHtmlExtension, ".htm" }, typeof(RdfAParser), null, null, typeof(HtmlWriter), null, typeof(SparqlHtmlWriter)));

                //Q: Define SPARQL Query?

                _init = true;
            }
        }

        #endregion

        #region MIME Types

        /// <summary>
        /// Gets the available MIME Type Definitions
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
        /// Adds a new MIME Type Definition
        /// </summary>
        /// <param name="definition">MIME Type Definition</param>
        public static void AddDefinition(MimeTypeDefinition definition)
        {
            if (!_init) Init();
            _mimeTypes.Add(definition);
        }

        /// <summary>
        /// Gets all MIME Type definitions which support the given MIME Type
        /// </summary>
        /// <param name="mimeType">MIME Type</param>
        /// <returns></returns>
        public static IEnumerable<MimeTypeDefinition> GetDefinitions(String mimeType)
        {
            if (!_init) Init();

            return (from definition in MimeTypesHelper.Definitions
                    where definition.MimeTypes.Contains(mimeType)
                    select definition);
        }

        /// <summary>
        /// Gets all MIME Type definition which support the given MIME Types
        /// </summary>
        /// <param name="mimeTypes">MIME Types</param>
        /// <returns></returns>
        public static IEnumerable<MimeTypeDefinition> GetDefinitions(String[] mimeTypes)
        {
            if (!_init) Init();

            if (mimeTypes.Length == 0) return Enumerable.Empty<MimeTypeDefinition>();

            //Clean up the MIME Types to remove any Charset/Quality parameters
            for (int i = 0; i < mimeTypes.Length; i++)
            {
                if (mimeTypes[i].Contains(";"))
                {
                    mimeTypes[i] = mimeTypes[i].Substring(0, mimeTypes[i].IndexOf(';'));
                }
            }

            return (from mimeType in mimeTypes
                    from definition in MimeTypesHelper.GetDefinitions(mimeType)
                    select definition).Distinct();
                    
        }

        /// <summary>
        /// MIME Type for accept any content Type
        /// </summary>
        public const String Any = "*/*";

        /// <summary>
        /// MIME Type for URL Encoded WWW Form Content used when POSTing over HTTP
        /// </summary>
        public const String WWWFormURLEncoded = "application/x-www-form-urlencoded";

        /// <summary>
        /// MIME Types for Turtle
        /// </summary>
        internal static string[] Turtle = { "text/turtle", "application/x-turtle", "application/turtle" };

        /// <summary>
        /// MIME Types for RDF/XML
        /// </summary>
        internal static string[] RdfXml = { "application/rdf+xml", "text/xml" };

        /// <summary>
        /// MIME Types for Notation 3
        /// </summary>
        internal static string[] Notation3 = { "text/n3", "text/rdf+n3" };

        /// <summary>
        /// MIME Types for NTriples
        /// </summary>
        internal static string[] NTriples = { "text/plain", "application/x-ntriples" };

        /// <summary>
        /// MIME Types for NQuads
        /// </summary>
        internal static string[] NQuads = { "text/x-nquads" };

        /// <summary>
        /// MIME Types for TriG
        /// </summary>
        internal static string[] TriG = { "application/x-trig" };

        /// <summary>
        /// MIME Types for TriX
        /// </summary>
        internal static string[] TriX = { "application/trix" };

        /// <summary>
        /// MIME Types for RDF/Json
        /// </summary>
        internal static string[] Json = { "application/json", "text/json" };

        /// <summary>
        /// MIME Types for SPARQL Result Sets
        /// </summary>
        internal static string[] Sparql = { "application/sparql-results+xml", "application/sparql-results+json" };

        /// <summary>
        /// MIME Types for SPARQL Results XML
        /// </summary>
        internal static string[] SparqlXml = { "application/sparql-results+xml" };

        /// <summary>
        /// Mime Types for SPARQL Results JSON
        /// </summary>
        internal static string[] SparqlJson = { "application/sparql-results+json" };

        /// <summary>
        /// MIME Types for CSV
        /// </summary>
        internal static string[] Csv = { "text/csv", "text/comma-separated-values" };

        /// <summary>
        /// MIME Types for TSV
        /// </summary>
        internal static string[] Tsv = { "text/tab-separated-values" };

        /// <summary>
        /// MIME Types for HTML
        /// </summary>
        internal static string[] Html = { "text/html", "application/xhtml+xml" };

        #endregion

        #region File Extension Constants

        /// <summary>
        /// Default File Extension for Turtle Files
        /// </summary>
        public const String DefaultTurtleExtension = "ttl";
        /// <summary>
        /// Default File Extension for RDF/XML
        /// </summary>
        public const String DefaultRdfXmlExtension = "rdf";
        /// <summary>
        /// Default File Extension for Notation 3
        /// </summary>
        public const String DefaultNotation3Extension = "n3";
        /// <summary>
        /// Default File Extension for NTriples
        /// </summary>
        public const String DefaultNTriplesExtension = "nt";
        /// <summary>
        /// Default File Extension for Json formats
        /// </summary>
        public const String DefaultJsonExtension = "json";
        /// <summary>
        /// Default File Extension for Sparql XML Results Format
        /// </summary>
        public const String DefaultSparqlXmlExtension = "srx";
        /// <summary>
        /// Default File Extension for TriG
        /// </summary>
        public const String DefaultTriGExtension = "trig";
        /// <summary>
        /// Default File Extension for NQuads
        /// </summary>
        public const String DefaultNQuadsExtension = "nq";
        /// <summary>
        /// Default File Extension for TriX
        /// </summary>
        public const String DefaultTriXExtension = "xml";
        /// <summary>
        /// Default File Extension for CSV
        /// </summary>
        public const String DefaultCsvExtension = "csv";
        /// <summary>
        /// Default File Extension for TSV
        /// </summary>
        public const String DefaultTsvExtension = "tsv";
        /// <summary>
        /// Default File Extension for HTML
        /// </summary>
        public const String DefaultHtmlExtension = "html";
        /// <summary>
        /// Default File Extension for XHTML
        /// </summary>
        public const String DefaultXHtmlExtension = "xhtml";

        #endregion

        #region HTTP Header Properties

        /// <summary>
        /// Builds the String for the HTTP Accept Header that should be used when you want to ask for content in RDF formats (except Sparql Results)
        /// </summary>
        /// <returns></returns>
        public static String HttpAcceptHeader
        {
            get
            {
                if (!_init) Init();

                StringBuilder output = new StringBuilder();

                foreach (MimeTypeDefinition definition in MimeTypesHelper.Definitions)
                {
                    if (definition.CanParseRdf)
                    {
                        output.Append(String.Join(",", definition.MimeTypes.ToArray()));
                        output.Append(',');
                    }
                }
                if (output[output.Length - 1] == ',') output.Remove(output.Length - 1, 1);
                output.Append(";q=0.9,*/*;q=0.8");

                return output.ToString();
            }
        }

        /// <summary>
        /// Builds the String for the HTTP Accept Header that should be used for querying Sparql Endpoints where the response will be a SPARQL Result Set format
        /// </summary>
        /// <returns></returns>
        public static String HttpSparqlAcceptHeader
        {
            get
            {
                if (!_init) Init();

                StringBuilder output = new StringBuilder();

                foreach (MimeTypeDefinition definition in MimeTypesHelper.Definitions)
                {
                    if (definition.CanParseSparqlResults)
                    {
                        output.Append(String.Join(",", definition.MimeTypes.ToArray()));
                        output.Append(',');
                    }
                }
                if (output[output.Length - 1] == ',') output.Remove(output.Length - 1, 1);
                output.Append(";q=1.0");

                return output.ToString();
            }
        }

        /// <summary>
        /// Builds the String for the HTTP Accept Header that should be used for making HTTP Requests where the returned data may be RDF or a SPARQL Result Set
        /// </summary>
        /// <returns></returns>
        public static String HttpRdfOrSparqlAcceptHeader
        {
            get
            {
                if (!_init) Init();

                StringBuilder output = new StringBuilder();

                foreach (MimeTypeDefinition definition in MimeTypesHelper.Definitions)
                {
                    if (definition.CanParseRdf || definition.CanParseSparqlResults)
                    {
                        output.Append(String.Join(",", definition.MimeTypes.ToArray()));
                        output.Append(',');
                    }
                }
                if (output[output.Length - 1] == ',') output.Remove(output.Length - 1, 1);
                output.Append(";q=0.9,*/*;q=0.8");

                return output.ToString();
            }
        }

        /// <summary>
        /// Builds the String for the HTTP Accept Header that should be used for making HTTP Requests where the returned data will be an RDF dataset
        /// </summary>
        public static String HttpRdfDatasetAcceptHeader
        {
            get
            {
                if (!_init) Init();

                StringBuilder output = new StringBuilder();

                foreach (MimeTypeDefinition definition in MimeTypesHelper.Definitions)
                {
                    if (definition.CanParseRdfDatasets)
                    {
                        output.Append(String.Join(",", definition.MimeTypes.ToArray()));
                        output.Append(',');
                    }
                }
                if (output[output.Length - 1] == ',') output.Remove(output.Length - 1, 1);
                output.Append(";q=1.0");

                return output.ToString();
            }
        }

        /// <summary>
        /// Builds the String for the HTTP Accept Header that should be used for making HTTP Requests where the returned data may be RDF or an RDF dataset
        /// </summary>
        public static String HttpRdfOrDatasetAcceptHeader
        {
            get
            {
                if (!_init) Init();

                StringBuilder output = new StringBuilder();

                foreach (MimeTypeDefinition definition in MimeTypesHelper.Definitions)
                {
                    if (definition.CanParseRdf || definition.CanParseRdfDatasets)
                    {
                        output.Append(String.Join(",", definition.MimeTypes.ToArray()));
                        output.Append(',');
                    }
                }
                if (output[output.Length - 1] == ',') output.Remove(output.Length - 1, 1);
                output.Append(";q=0.9,*/*;q=0.8");

                return output.ToString();
            }
        }

        /// <summary>
        /// Creates a Custom HTTP Accept Header containing the given selection of MIME Types
        /// </summary>
        /// <param name="mimeTypes">Enumeration of MIME Types to use</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> No validation is done on MIME Types so it is possible to generated a malformed header using this function
        /// </para>
        /// </remarks>
        public static String CustomHttpAcceptHeader(IEnumerable<String> mimeTypes)
        {
            return String.Join(",", mimeTypes.ToArray());
        }

        /// <summary>
        /// Creates a Custom HTTP Accept Header containing the given selection of MIME Types where those MIME Types also appear in the list of supported Types
        /// </summary>
        /// <param name="mimeTypes">Enumeration of MIME Types to use</param>
        /// <param name="supportedTypes">Enumeration of supported MIME Types</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> No validation is done on MIME Types so it is possible to generated a malformed header using this function
        /// </para>
        /// <para>
        /// Use this function when you wish to generate a Custom Accept Header where the URI to which you are making requests supports a set range of URIs (given in the <paramref name="mimeTypes"/> parameter) where that range of types may exceed the range of types actually supported by the library or your response processing code.
        /// </para>
        /// </remarks>
        public static String CustomHttpAcceptHeader(IEnumerable<String> mimeTypes, IEnumerable<String> supportedTypes)
        {
            StringBuilder output = new StringBuilder();
            HashSet<String> supported = new HashSet<string>(supportedTypes);

            foreach (String type in mimeTypes)
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
        /// Creates a Custom HTTP Accept Header containing only the Accept Types supported by a specific parser
        /// </summary>
        /// <param name="parser">RDF Parser</param>
        /// <returns></returns>
        public static String CustomHttpAcceptHeader(IRdfReader parser)
        {
            if (!_init) Init();

            Type requiredType = parser.GetType();
            foreach (MimeTypeDefinition definition in MimeTypesHelper.Definitions)
            {
                if (requiredType.Equals(definition.RdfParserType))
                {
                    return String.Join(",", definition.MimeTypes.ToArray());
                }
            }
            return MimeTypesHelper.HttpAcceptHeader;
        }

        /// <summary>
        /// Creates a Custom HTTP Accept Header containing only the Accept Types supported by a specific parser
        /// </summary>
        /// <param name="parser">RDF Parser</param>
        /// <returns></returns>
        public static String CustomHttpAcceptHeader(IStoreReader parser)
        {
            if (!_init) Init();

            Type requiredType = parser.GetType();
            foreach (MimeTypeDefinition definition in MimeTypesHelper.Definitions)
            {
                if (requiredType.Equals(definition.RdfDatasetParserType))
                {
                    return String.Join(",", definition.MimeTypes.ToArray());
                }
            }
            return MimeTypesHelper.HttpRdfDatasetAcceptHeader;
        }

        /// <summary>
        /// Gets the Enumeration of supported MIME Types for RDF Graphs
        /// </summary>
        public static IEnumerable<String> SupportedRdfMimeTypes
        {
            get
            {
                if (!_init) Init();

                return (from definition in MimeTypesHelper.Definitions
                        where definition.CanParseRdf
                        from mimeType in definition.MimeTypes
                        select mimeType);
            }
        }

        /// <summary>
        /// Gets the Enumeration of supported MIME Types for SPARQL Results
        /// </summary>
        public static IEnumerable<String> SupportedSparqlMimeTypes
        {
            get
            {
                if (!_init) Init();

                return (from definition in MimeTypesHelper.Definitions
                        where definition.CanParseSparqlResults
                        from mimeType in definition.MimeTypes
                        select mimeType);
            }
        }

        /// <summary>
        /// Gets the Enumeration of supported MIME Types for RDF Graphs or SPARQL Results
        /// </summary>
        public static IEnumerable<String> SupportedRdfOrSparqlMimeTypes
        {
            get
            {
                if (!_init) Init();

                return (from definition in MimeTypesHelper.Definitions
                        where definition.CanParseRdf || definition.CanParseSparqlResults
                        from mimeType in definition.MimeTypes
                        select mimeType);
            }
        }

        #endregion

        #region Reader and Writer Selection

        /// <summary>
        /// Selects an appropriate <see cref="IRdfWriter">IRdfWriter</see> based on the HTTP Accept header form a HTTP Request
        /// </summary>
        /// <param name="ctypes">String array of accepted Content Types</param>
        /// <param name="contentType">The Content Type header that should be sent in the Response to the Request</param>
        /// <returns>A Writer for a Content Type the client accepts and the Content Type that should be sent to the client</returns>
        /// <remarks>
        /// <para>
        /// This method does not take account of any quality/charset preference parameters included in the Accept Header
        /// </para>
        /// <para>
        /// For writers which support <see cref="ICompressingWriter">ICompressingWriter</see> they will be instantiated with the Compression Level specified by <see cref="Options.DefaultCompressionLevel">Options.DefaultCompressionLevel</see>
        /// </para>
        /// </remarks>
        public static IRdfWriter GetWriter(String[] ctypes, out String contentType)
        {
            String type;

            foreach (String ctype in ctypes)
            {
                //Strip off the Charset/Quality if specified
                if (ctype.Contains(";"))
                {
                    type = ctype.Substring(0, ctype.IndexOf(";"));
                }
                else
                {
                    type = ctype;
                }
                type = type.ToLowerInvariant();

                //See if there are any MIME Type Definitions for this MIME Type
                foreach (MimeTypeDefinition definition in MimeTypesHelper.GetDefinitions(type))
                {
                    //If so return the Writer from the first match found
                    if (definition.CanWriteRdf)
                    {
                        IRdfWriter writer = definition.GetRdfWriter();
                        if (writer is ICompressingWriter)
                        {
                            ((ICompressingWriter)writer).CompressionLevel = Options.DefaultCompressionLevel;
                        }
                        contentType = definition.CanonicalMimeType;
                        return writer;
                    }
                }
            }

            //Default to NTriples
            contentType = MimeTypesHelper.NTriples[0];
            return new NTriplesWriter();
        }

        /// <summary>
        /// Selects an appropriate <see cref="IRdfWriter">IRdfWriter</see> based on the HTTP Accept header form a HTTP Request
        /// </summary>
        /// <param name="acceptHeader">Value of the HTTP Accept Header</param>
        /// <param name="contentType">The Content Type header that should be sent in the Response to the Request</param>
        /// <returns>A Writer for a Content Type the client accepts and the Content Type that should be sent to the client</returns>
        /// <remarks>
        /// <para>
        /// This method does not take account of any quality/charset preference parameters included in the Accept Header
        /// </para>
        /// <para>
        /// For writers which support <see cref="ICompressingWriter">ICompressingWriter</see> they will be instantiated with the Compression Level specified by <see cref="Options.DefaultCompressionLevel">Options.DefaultCompressionLevel</see>
        /// </para>
        /// </remarks>
        public static IRdfWriter GetWriter(String acceptHeader, out String contentType)
        {
            String[] ctypes;

            //Parse Accept Header into a String Array
            acceptHeader = acceptHeader.Trim();
            if (acceptHeader.Contains(","))
            {
                ctypes = acceptHeader.Split(',');
            }
            else
            {
                ctypes = new String[] { acceptHeader };
            }

            return GetWriter(ctypes, out contentType);
        }

        /// <summary>
        /// Selects an appropriate <see cref="IRdfWriter">IRdfWriter</see> based on the HTTP Accept header form a HTTP Request
        /// </summary>
        /// <param name="acceptHeader">Value of the HTTP Accept Header</param>
        /// <returns>A Writer for a Content Type the client accepts</returns>
        /// <remarks>This method does not take account of any quality/charset preference parameters included in the Accept Header</remarks>
        public static IRdfWriter GetWriter(String acceptHeader)
        {
            String temp;
            return GetWriter(acceptHeader, out temp);
        }

        /// <summary>
        /// Selects an appropriate <see cref="IRdfReader">IRdfReader</see> based on the HTTP Content-Type header from a HTTP Response
        /// </summary>
        /// <param name="contentType">Value of the HTTP Content-Type Header</param>
        /// <returns></returns>
        public static IRdfReader GetParser(String contentType)
        {
            //Strip off Charset specifier of the Content Type if any
            if (contentType.Contains(";"))
            {
                contentType = contentType.Substring(0, contentType.IndexOf(";"));
            }
            contentType = contentType.ToLowerInvariant();

            foreach (MimeTypeDefinition definition in MimeTypesHelper.GetDefinitions(contentType))
            {
                if (definition.CanParseRdf)
                {
                    return definition.GetRdfParser();
                }
            }

            throw new RdfParserSelectionException("The Library does not contain a Parser which understands RDF Graphs in the format '" + contentType + "'");
        }

        /// <summary>
        /// Selects an appropriate <see cref="ISparqlResultsReader">ISparqlResultsReader</see> based on the HTTP Content-Type header from a HTTP Response
        /// </summary>
        /// <param name="contentType">Value of the HTTP Content-Type Header</param>
        /// <returns></returns>
        public static ISparqlResultsReader GetSparqlParser(String contentType)
        {
            //Strip off Charset specifier of the Content Type if any
            if (contentType.Contains(";"))
            {
                contentType = contentType.Substring(0, contentType.IndexOf(";"));
            }
            contentType = contentType.ToLowerInvariant();

            foreach (MimeTypeDefinition definition in MimeTypesHelper.Definitions)
            {
                if (definition.CanParseSparqlResults)
                {
                    return definition.GetSparqlResultsParser();
                }
            }

            throw new RdfParserSelectionException("The Library does not contain a Parser which understands SPARQL Results in the format '" + contentType + "'");
        }

        /// <summary>
        /// Selects an appropriate <see cref="ISparqlResultsReader">ISparqlResultsReader</see> based on the HTTP Content-Type header from a HTTP Response
        /// </summary>
        /// <param name="contentType">Value of the HTTP Content-Type Header</param>
        /// <param name="allowPlainTextResults">Whether you allow Sparql Boolean results in text/plain format</param>
        /// <returns></returns>
        public static ISparqlResultsReader GetSparqlParser(String contentType, bool allowPlainTextResults)
        {
            //Strip off Charset specifier of the Content Type if any
            if (contentType.Contains(";"))
            {
                contentType = contentType.Substring(0, contentType.IndexOf(";"));
            }
            contentType = contentType.ToLowerInvariant();

            try
            {
                return GetSparqlParser(contentType);
            }
            catch (RdfParserSelectionException)
            {
                if (allowPlainTextResults && contentType.Equals("text/plain"))
                {
                    return new SparqlBooleanParser();
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Selects an appropriate <see cref="ISparqlResultsWriter">ISparqlResultsWriter</see> based on the HTTP Accept header form a HTTP Request
        /// </summary>
        /// <param name="ctypes">String array of accepted Content Types</param>
        /// <param name="contentType">The Content Type header that should be sent in the Response to the Request</param>
        /// <returns>A Writer for a Content Type the client accepts and the Content Type that should be sent to the client</returns>
        /// <remarks>This method does not take account of any quality/charset preference parameters included in the Accept Header</remarks>
        public static ISparqlResultsWriter GetSparqlWriter(String[] ctypes, out String contentType)
        {
            String type;

            foreach (String ctype in ctypes)
            {
                //Strip off the Charset/Quality if specified
                if (ctype.Contains(";"))
                {
                    type = ctype.Substring(0, ctype.IndexOf(";"));
                }
                else
                {
                    type = ctype;
                }
                type = type.ToLowerInvariant();

                foreach (MimeTypeDefinition definition in MimeTypesHelper.Definitions)
                {
                    if (definition.CanWriteSparqlResults)
                    {
                        contentType = definition.CanonicalMimeType;
                        return definition.GetSparqlResultsWriter();
                    }
                }
            }

            //Default to HTML Output
            contentType = MimeTypesHelper.Html[0];
            return new SparqlHtmlWriter();
        }

        /// <summary>
        /// Selects an appropriate <see cref="ISparqlResultsWriter">ISparqlResultsWriter</see> based on the HTTP Accept header form a HTTP Request
        /// </summary>
        /// <param name="acceptHeader">Value of the HTTP Accept Header</param>
        /// <param name="contentType">The Content Type header that should be sent in the Response to the Request</param>
        /// <returns>A Writer for a Content Type the client accepts and the Content Type that should be sent to the client</returns>
        /// <remarks>This method does not take account of any quality/charset preference parameters included in the Accept Header</remarks>
        public static ISparqlResultsWriter GetSparqlWriter(String acceptHeader, out String contentType)
        {
            String[] ctypes;

            //Parse Accept Header into a String Array
            acceptHeader = acceptHeader.Trim();
            if (acceptHeader.Contains(","))
            {
                ctypes = acceptHeader.Split(',');
            }
            else
            {
                ctypes = new String[] { acceptHeader };
            }

            return GetSparqlWriter(ctypes, out contentType);
        }

        /// <summary>
        /// Selects an appropriate <see cref="ISparqlResultsWriter">ISparqlResultsWriter</see> based on the HTTP Accept header form a HTTP Request
        /// </summary>
        /// <param name="acceptHeader">Value of the HTTP Accept Header</param>
        /// <returns>A Writer for a Content Type the client accepts</returns>
        /// <remarks>This method does not take account of any quality/charset preference parameters included in the Accept Header</remarks>
        public static ISparqlResultsWriter GetSparqlWriter(String acceptHeader)
        {
            String temp;
            return GetSparqlWriter(acceptHeader, out temp);
        }

        /// <summary>
        /// Selects an appropriate <see cref="IStoreReader">IStoreReader</see> based on the HTTP Content-Type header from a HTTP Response
        /// </summary>
        /// <param name="contentType">Value of the HTTP Content-Type Header</param>
        /// <returns></returns>
        public static IStoreReader GetStoreParser(String contentType)
        {
            //Strip off Charset specifier of the Content Type if any
            if (contentType.Contains(";"))
            {
                contentType = contentType.Substring(0, contentType.IndexOf(";"));
            }
            contentType = contentType.ToLowerInvariant();

            foreach (MimeTypeDefinition definition in MimeTypesHelper.Definitions)
            {
                if (definition.CanParseRdfDatasets)
                {
                    return definition.GetRdfDatasetParser();
                }
            }

            throw new RdfParserSelectionException("The Library does not contain a Parser which understands RDF datasets in the format '" + contentType + "'");
        }

        /// <summary>
        /// Selects an appropriate <see cref="IStoreWriter">IStoreWriter</see> based on the HTTP Accept header form a HTTP Request
        /// </summary>
        /// <param name="ctypes">String array of accepted Content Types</param>
        /// <param name="contentType">The Content Type header that should be sent in the Response to the Request</param>
        /// <returns>A Writer for a Content Type the client accepts and the Content Type that should be sent to the client</returns>
        /// <remarks>This method does not take account of any quality/charset preference parameters included in the Accept Header</remarks>
        public static IStoreWriter GetStoreWriter(String[] ctypes, out String contentType)
        {
            String type;
            foreach (String ctype in ctypes)
            {
                //Strip off the Charset/Quality if specified
                if (ctype.Contains(";"))
                {
                    type = ctype.Substring(0, ctype.IndexOf(";"));
                }
                else
                {
                    type = ctype;
                }
                type = type.ToLowerInvariant();

                foreach (MimeTypeDefinition definition in MimeTypesHelper.GetDefinitions(type))
                {
                    if (definition.CanWriteRdfDatasets)
                    {
                        contentType = definition.CanonicalMimeType;
                        return definition.GetRdfDatasetWriter();
                    }
                }
            }

            throw new RdfException("The Library does not contain a writer which can output RDF datasets in a format supported by the Client");
        }

        /// <summary>
        /// Selects an appropriate <see cref="IStoreWriter">IStoreWriter</see> based on the HTTP Accept header form a HTTP Request
        /// </summary>
        /// <param name="acceptHeader">Value of the HTTP Accept Header</param>
        /// <param name="contentType">The Content Type header that should be sent in the Response to the Request</param>
        /// <returns>A Writer for a Content Type the client accepts and the Content Type that should be sent to the client</returns>
        /// <remarks>This method does not take account of any quality/charset preference parameters included in the Accept Header</remarks>
        public static IStoreWriter GetStoreWriter(String acceptHeader, out String contentType)
        {
            String[] ctypes;

            //Parse Accept Header into a String Array
            acceptHeader = acceptHeader.Trim();
            if (acceptHeader.Contains(","))
            {
                ctypes = acceptHeader.Split(',');
            }
            else
            {
                ctypes = new String[] { acceptHeader };
            }

            return GetStoreWriter(ctypes, out contentType);
        }

        /// <summary>
        /// Selects an appropriate <see cref="IStoreWriter">IStoreWriter</see> based on the HTTP Accept header form a HTTP Request
        /// </summary>
        /// <param name="acceptHeader">Value of the HTTP Accept Header</param>
        /// <returns>A Writer for a Content Type the client accepts</returns>
        /// <remarks>This method does not take account of any quality/charset preference parameters included in the Accept Header</remarks>
        public static IStoreWriter GetStoreWriter(String acceptHeader)
        {
            String temp;
            return GetStoreWriter(acceptHeader, out temp);
        }

        #endregion

        #region MIME Type Selection

        /// <summary>
        /// Selects the appropriate MIME Type for the given File Extension if the File Extension is a standard extension for an RDF format
        /// </summary>
        /// <param name="fileExt">File Extension</param>
        /// <returns></returns>
        public static String GetMimeType(String fileExt)
        {
            //Only use the last bit of the extension
            if (fileExt.Contains("."))
            {
                fileExt = fileExt.Substring(fileExt.LastIndexOf(".") + 1);
            }

            //Determine MIME Type
            if (fileExt.Equals(DefaultTurtleExtension, StringComparison.OrdinalIgnoreCase))
            {
                return MimeTypesHelper.Turtle[0];
            }
            else if (fileExt.Equals(DefaultNotation3Extension, StringComparison.OrdinalIgnoreCase))
            {
                return MimeTypesHelper.Notation3[0];
            }
            else if (fileExt.Equals(DefaultNTriplesExtension, StringComparison.OrdinalIgnoreCase))
            {
                return MimeTypesHelper.NTriples[0];
            }
            else if (fileExt.Equals(DefaultRdfXmlExtension, StringComparison.OrdinalIgnoreCase) || fileExt.Equals("owl"))
            {
                return MimeTypesHelper.RdfXml[0];
            }
            else if (fileExt.Equals(DefaultJsonExtension, StringComparison.OrdinalIgnoreCase))
            {
                return MimeTypesHelper.Json[0];
            }
            else if (fileExt.Equals(DefaultSparqlXmlExtension, StringComparison.OrdinalIgnoreCase))
            {
                return MimeTypesHelper.Sparql[0];
            }
            else if (fileExt.Equals(DefaultTriGExtension, StringComparison.OrdinalIgnoreCase))
            {
                return MimeTypesHelper.TriG[0];
            }
            else if (fileExt.Equals(DefaultNQuadsExtension, StringComparison.OrdinalIgnoreCase))
            {
                return MimeTypesHelper.NQuads[0];
            }
            else if (fileExt.Equals(DefaultTriXExtension, StringComparison.OrdinalIgnoreCase) || fileExt.Equals("trix", StringComparison.OrdinalIgnoreCase))
            {
                return MimeTypesHelper.TriX[0];
            }
            else if (fileExt.Equals(DefaultCsvExtension, StringComparison.OrdinalIgnoreCase))
            {
                return MimeTypesHelper.Csv[0];
            }
            else if (fileExt.Equals(DefaultTsvExtension, StringComparison.OrdinalIgnoreCase))
            {
                return MimeTypesHelper.Tsv[0];
            }
            else if (fileExt.Equals(DefaultHtmlExtension, StringComparison.OrdinalIgnoreCase))
            {
                return MimeTypesHelper.Html[0];
            }
            else if (fileExt.Equals(DefaultXHtmlExtension, StringComparison.OrdinalIgnoreCase))
            {
                return MimeTypesHelper.Html[0];
            }
            else
            {
                //Unknown File Extension
                throw new RdfParserSelectionException("Unable to determine the appropriate MIME Type for the File Extension '" + fileExt + "' as this is not a standard extension for an RDF format");
            }
        }

        #endregion

        #region File Extension Selection

        /// <summary>
        /// Selects the appropriate File Extension for the given MIME Type
        /// </summary>
        /// <param name="mimeType">MIME Type</param>
        /// <returns></returns>
        public static String GetFileExtension(String mimeType)
        {
            //Strip off Charset specifier of the Content Type if any
            if (mimeType.Contains(";"))
            {
                mimeType = mimeType.Substring(0, mimeType.IndexOf(";"));
            }

            if (!_init) Init();
            foreach (MimeTypeDefinition definition in MimeTypesHelper.Definitions)
            {
                if (definition.MimeTypes.Contains(mimeType))
                {
                    return definition.CanonicalFileExtension;
                }
            }

            throw new RdfException("Unable to determine the appropriate File Extension for the MIME Type '" + mimeType + "'");
        }

        /// <summary>
        /// Selects the appropriate File Extension for the given RDF Writer
        /// </summary>
        /// <param name="writer">RDF Writer</param>
        /// <returns></returns>
        public static String GetFileExtension(IRdfWriter writer)
        {
            if (!_init) Init();
            Type requiredType = writer.GetType();
            foreach (MimeTypeDefinition definition in MimeTypesHelper.Definitions)
            {
                if (requiredType.Equals(definition.RdfWriterType))
                {
                    return definition.CanonicalFileExtension;
                }
            }

            throw new RdfException("Unable to determine the appropriate File Extension for the RDF Writer '" + writer.GetType().ToString() + "'");
        }

        /// <summary>
        /// Selects the appropriate File Extension for the given Store Writer
        /// </summary>
        /// <param name="writer">Store Writer</param>
        /// <returns></returns>
        public static String GetFileExtension(IStoreWriter writer)
        {
            if (!_init) Init();
            Type requiredType = writer.GetType();
            foreach (MimeTypeDefinition definition in MimeTypesHelper.Definitions)
            {
                if (requiredType.Equals(definition.RdfDatasetWriterType))
                {
                    return definition.CanonicalFileExtension;
                }
            }
                
            throw new RdfException("Unable to determine the appropriate File Extension for the Store Writer '" + writer.GetType().ToString() + "'");
        }

        #endregion

   }
}
