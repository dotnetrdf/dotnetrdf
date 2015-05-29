/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using VDS.RDF.Attributes;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF
{
    /// <summary>
    /// Manager for RDF IO operations
    /// </summary>
    /// <remarks>
    /// <para>
    /// Part of this classes duties is to maintain the registry of supported input and output formats along with their associated <see cref="IRdfReader"/> and <see cref="IRdfWriter"/> implementations.  Formats provided directly by this assembly are automatically registered, formats in other assemblies are either auto-detected from the <see cref="RdfIOAttribute"/> declarations in those assemblies or may be manually registered via various static methods of this class.
    /// </para>
    /// <para>
    /// The registry is lazily initialized so until you actually use a method that requires the registry it will not be populated, the initial population may be slow so it may be sensible to enumerate the available definitions via the <see cref="Definitions"/> property in order to force an initialization.
    /// </para>
    /// </remarks>
    public static class IOManager
    {
        #region Constants

        /// <summary>
        /// Constant for W3C File Formats Namespace
        /// </summary>
        public const String W3CFormatsNamespace = "http://www.w3.org/ns/formats/";

        /// <summary>
        /// MIME Type for accept any content Type
        /// </summary>
        public const String Any = "*/*";

        /// <summary>
        /// MIME Type for URL Encoded WWW Form Content used when POSTing over HTTP
        /// </summary>
        public const String WwwFormUrlEncoded = "application/x-www-form-urlencoded";

        /// <summary>
        /// MIME Type for URL Enoded WWW Form Content used when POSTing over HTTP in UTF-8 encoding
        /// </summary>
        public const String Utf8WwwFormUrlEncoded = WwwFormUrlEncoded + ";charset=utf-8";

        /// <summary>
        /// MIME Type for Multipart Form Data
        /// </summary>
        public const String FormMultipart = "multipart/form-data";

        /// <summary>
        /// MIME Types for Turtle
        /// </summary>
// ReSharper disable InconsistentNaming
        public static string[] Turtle = {"text/turtle", "application/x-turtle", "application/turtle"};

        /// <summary>
        /// MIME Types for RDF/XML
        /// </summary>
        public static string[] RdfXml = {"application/rdf+xml", "text/xml", "application/xml"};

        /// <summary>
        /// MIME Types for Notation 3
        /// </summary>
        public static string[] Notation3 = {"text/n3", "text/rdf+n3"};

        /// <summary>
        /// MIME Types for NTriples
        /// </summary>
        public static string[] NTriples = { "application/ntriples", "text/plain", "text/ntriples", "text/ntriples+turtle", "application/rdf-triples", "application/x-ntriples" };

        /// <summary>
        /// MIME Types for NQuads
        /// </summary>
        public static string[] NQuads = { "application/n-quads", "text/x-nquads" };

        /// <summary>
        /// MIME Types for TriG
        /// </summary>
        public static string[] TriG = {"application/x-trig"};

        /// <summary>
        /// MIME Types for TriX
        /// </summary>
        public static string[] TriX = {"application/trix"};

        /// <summary>
        /// MIME Types for RDF/JSON
        /// </summary>
        public static string[] Json = {"application/json", "text/json", "application/rdf+json"};

        /// <summary>
        /// MIME Types for CSV
        /// </summary>
        public static string[] Csv = {"text/csv", "text/comma-separated-values"};

        /// <summary>
        /// MIME Types for TSV
        /// </summary>
        public static string[] Tsv = {"text/tab-separated-values"};

        /// <summary>
        /// MIME Types for HTML
        /// </summary>
        public static string[] Html = {"text/html", "application/xhtml+xml"};

        // ReSharper restore InconsistentNaming

        /// <summary>
        /// MIME Type for SPARQL Queries
        /// </summary>
        public const String SparqlQuery = "application/sparql-query";

        /// <summary>
        /// MIME Type for SPARQL Updates
        /// </summary>
        public const String SparqlUpdate = "application/sparql-update";

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
        /// Default File Extension for RDF/JSON
        /// </summary>
        public const String DefaultRdfJsonExtension = "rj";

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

        /// <summary>
        /// Default File Extension for GZip
        /// </summary>
        public const String DefaultGZipExtension = "gz";

        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable InconsistentNaming

        // Not made readonly since we want this to be extensible in future

        /// <summary>
        /// Extensions which are considered stackable
        /// </summary>
        private static String[] AllowedStackableExtensions = new String[]
                                                                 {
                                                                     DefaultGZipExtension
                                                                 };

        // ReSharper restore InconsistentNaming
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        /// <summary>
        /// Charset constants
        /// </summary>
        public const String CharsetUtf8 = "utf-8",
                            CharsetUtf16 = "utf-16";

        #endregion

        /// <summary>
        /// List of MIME Type Definition
        /// </summary>
        private static List<MimeTypeDefinition> _mimeTypes;

        /// <summary>
        /// Whether MIME Type Definitions have been initialised
        /// </summary>
        private static bool _init;

        private static readonly Object _initLock = new Object();

        /// <summary>
        /// Checks whether something is a valid MIME Type
        /// </summary>
        /// <param name="type">MIME Type</param>
        /// <returns></returns>
        public static bool IsValidMimeType(String type)
        {
            String[] parts = type.Split('/');
            if (parts.Length != 2) return false;
            return IsValidMimeTypePart(parts[0]) && IsValidMimeTypePart(parts[1]);
        }

        /// <summary>
        /// Determines whether the given string is valid as a type/subtype for a MIME type
        /// </summary>
        /// <param name="part">String</param>
        /// <returns></returns>
        public static bool IsValidMimeTypePart(String part)
        {
            foreach (char c in part)
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
        /// Initialises the MIME Type definitions
        /// </summary>
        private static void Init()
        {
            lock (_initLock)
            {
                if (_init) return;
                _mimeTypes = new List<MimeTypeDefinition>();

                //Define NTriples
                MimeTypeDefinition ntriples = new MimeTypeDefinition {SyntaxName = "NTriples", FormatUri = W3CFormatsNamespace + "N-Triples", MimeTypes = NTriples, FileExtensions = new String[] {DefaultNTriplesExtension}, RdfParserType = typeof (NTriplesParser), RdfWriterType = typeof (NTriplesWriter), Quality = 0.8};
#if !SILVERLIGHT
                ntriples.Encoding = Encoding.ASCII;
#endif
                _mimeTypes.Add(ntriples);
#if !NO_COMPRESSION
                MimeTypeDefinition ntriplesGZipped = new MimeTypeDefinition {SyntaxName = "GZipped NTriples", MimeTypes = NTriples, FileExtensions = new String[] {DefaultNTriplesExtension + "." + DefaultGZipExtension}, RdfParserType = typeof (GZippedNTriplesParser), RdfWriterType = typeof (GZippedNTriplesWriter), Quality = 0.7};
#if !SILVERLIGHT
                ntriplesGZipped.Encoding = Encoding.ASCII;
#endif
                _mimeTypes.Add(ntriplesGZipped);
#endif

                //Define Turtle
                _mimeTypes.Add(new MimeTypeDefinition {SyntaxName = "Turtle", FormatUri = W3CFormatsNamespace + "Turtle", MimeTypes = Turtle, FileExtensions = new String[] {DefaultTurtleExtension}, RdfParserType = typeof (TurtleParser), RdfWriterType = typeof (CompressingTurtleWriter)});
#if !NO_COMPRESSION
                _mimeTypes.Add(new MimeTypeDefinition {SyntaxName = "GZipped Turtle", MimeTypes = Turtle, FileExtensions = new String[] {DefaultTurtleExtension + "." + DefaultGZipExtension}, RdfParserType = typeof (GZippedTurtleParser), RdfWriterType = typeof (GZippedTurtleWriter), Quality = 0.7});
#endif

                //Define Notation 3
                _mimeTypes.Add(new MimeTypeDefinition {SyntaxName = "Notation 3", FormatUri = W3CFormatsNamespace + "N3", MimeTypes = Notation3, FileExtensions = new String[] {DefaultNotation3Extension}, RdfParserType = typeof (Notation3Parser), RdfWriterType = typeof (Notation3Writer), Quality = 0.5});
#if !NO_COMPRESSION
                _mimeTypes.Add(new MimeTypeDefinition {SyntaxName = "GZipped Notation 3", MimeTypes = Notation3, FileExtensions = new String[] {DefaultNotation3Extension + "." + DefaultGZipExtension}, RdfParserType = typeof (GZippedNotation3Parser), RdfWriterType = typeof (GZippedNotation3Writer), Quality = 0.4});
#endif

                //Define NQuads
                _mimeTypes.Add(new MimeTypeDefinition {SyntaxName = "NQuads", MimeTypes = NQuads, FileExtensions = new String[] {DefaultNQuadsExtension}, RdfParserType = typeof (NQuadsParser), RdfWriterType = typeof (NQuadsWriter), Quality = 0.8});
#if !NO_COMPRESSION
                _mimeTypes.Add(new MimeTypeDefinition {SyntaxName = "GZipped NQuads", MimeTypes = NQuads, FileExtensions = new String[] {DefaultNQuadsExtension + "." + DefaultGZipExtension}, RdfParserType = typeof (GZippedNQuadsParser), RdfWriterType = typeof (GZippedNQuadsWriter), Quality = 0.7});
#endif

                //Define TriG
                _mimeTypes.Add(new MimeTypeDefinition {SyntaxName = "TriG", MimeTypes = TriG, FileExtensions = new String[] {DefaultTriGExtension}, RdfParserType = typeof (TriGParser), RdfWriterType = typeof (TriGWriter)});
#if !NO_COMPRESSION
                _mimeTypes.Add(new MimeTypeDefinition {SyntaxName = "GZipped TriG", MimeTypes = TriG, FileExtensions = new String[] {DefaultTriGExtension + "." + DefaultGZipExtension}, RdfParserType = typeof (GZippedTriGParser), RdfWriterType = typeof (GZippedTriGWriter)});
#endif

                //Define TriX
                _mimeTypes.Add(new MimeTypeDefinition {SyntaxName = "TriX", MimeTypes = TriX, FileExtensions = new String[] {DefaultTriXExtension}, RdfParserType = typeof (TriXParser), RdfWriterType = typeof (TriXWriter), Quality = 0.3});
#if !NO_COMPRESSION
                _mimeTypes.Add(new MimeTypeDefinition {SyntaxName = "GZipped TriX", MimeTypes = TriX, FileExtensions = new String[] {DefaultTriXExtension + "." + DefaultGZipExtension}, RdfParserType = typeof (GZippedTriXParser), RdfWriterType = typeof (GZippedTriXWriter), Quality = 0.2});
#endif
                //Define RDF/XML
                _mimeTypes.Add(new MimeTypeDefinition {SyntaxName = "RDF/XML", FormatUri = W3CFormatsNamespace + "RDF_XML", MimeTypes = RdfXml, FileExtensions = new String[] {DefaultRdfXmlExtension, "owl"}, RdfParserType = typeof (RdfXmlParser), RdfWriterType = typeof (RdfXmlWriter), Quality = 0.3});
#if !NO_COMPRESSION
                _mimeTypes.Add(new MimeTypeDefinition {SyntaxName = "GZipped RDF/XML", MimeTypes = RdfXml, FileExtensions = new String[] {DefaultRdfXmlExtension + "." + DefaultGZipExtension}, RdfParserType = typeof (GZippedRdfXmlParser), RdfWriterType = typeof (GZippedRdfXmlWriter), Quality = 0.2});
#endif


                //Define CSV
                _mimeTypes.Add(new MimeTypeDefinition {SyntaxName = "CSV", MimeTypes = Csv, FileExtensions = new String[] {DefaultCsvExtension}, RdfWriterType = typeof (CsvWriter), Quality = 0.1});
#if !NO_COMPRESSION
                _mimeTypes.Add(new MimeTypeDefinition {SyntaxName = "GZipped CSV", MimeTypes = Csv, FileExtensions = new String[] {DefaultCsvExtension + "." + DefaultGZipExtension}, RdfWriterType = typeof (GZippedCsvWriter), Quality = 0.1});
#endif

                //Define TSV
                _mimeTypes.Add(new MimeTypeDefinition {SyntaxName = "TSV", MimeTypes = Tsv, FileExtensions = new String[] {DefaultTsvExtension}, RdfWriterType = typeof (TsvWriter), Quality = 0.2});
#if !NO_COMPRESSION
                _mimeTypes.Add(new MimeTypeDefinition {SyntaxName = "GZipped TSV", MimeTypes = Tsv, FileExtensions = new String[] {DefaultTsvExtension + "." + DefaultGZipExtension}, RdfWriterType = typeof (GZippedTsvWriter), Quality = 0.2});
#endif

                //Define GraphViz DOT
                _mimeTypes.Add(new MimeTypeDefinition {SyntaxName = "GraphViz DOT", MimeTypes = new String[] {"text/vnd.graphviz"}, FileExtensions = new String[] {".gv", ".dot"}, RdfWriterType = typeof (GraphVizWriter), Quality = 0d});

                // Discover formats provided by other assemblies
                ScanDefinitions();

                _init = true;
            }
        }

        /// <summary>
        /// Scans for definitions for RDF input/output formats in any assembly in the current app domain
        /// </summary>
        /// <remarks>
        /// Formats are declared via the <see cref="RdfIOAttribute"/> at the assembly level
        /// </remarks>
        public static void ScanDefinitions()
        {
            lock (_mimeTypes)
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (RdfIOAttribute attribute in assembly.GetCustomAttributes(typeof (RdfIOAttribute), true))
                    {
                        _mimeTypes.Add(attribute.GetDefinition());
                    }
                }
            }
        }

        /// <summary>
        /// Resets the MIME Type Definitions (the associations between file extensions, MIME types and their respective parsers and writers) to the library defaults
        /// </summary>
        /// <remarks>
        /// <para>
        /// May be useful if you've altered the definitions and caused something to stop working as a result
        /// </para>
        /// </remarks>
        public static void ResetDefinitions()
        {
            lock (_initLock)
            {
                if (!_init) return;
                _init = false;
                _mimeTypes.Clear();
                Init();
            }
        }

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
            if (definition == null) throw new ArgumentNullException("definition");
            _mimeTypes.Add(definition);
        }

        /// <summary>
        /// Registers a parser as the default RDF Parser for all the given MIME types and updates relevant definitions to include the MIME types and file extensions
        /// </summary>
        /// <param name="parser">RDF Parser</param>
        /// <param name="mimeTypes">MIME Types</param>
        /// <param name="fileExtensions">File Extensions</param>
        public static void RegisterParser(IRdfReader parser, IEnumerable<String> mimeTypes, IEnumerable<String> fileExtensions)
        {
            if (!_init) Init();

            string[] types = mimeTypes as string[] ?? mimeTypes.ToArray();
            if (!types.Any()) throw new RdfException("Cannot register a parser without specifying at least 1 MIME Type");

            //Get any existing defintions that are to be altered
            IEnumerable<MimeTypeDefinition> existing = GetDefinitions(types);
            string[] extensions = fileExtensions as string[] ?? fileExtensions.ToArray();
            foreach (MimeTypeDefinition def in existing)
            {
                foreach (String type in types)
                {
                    def.AddMimeType(type);
                }
                foreach (String ext in extensions)
                {
                    def.AddFileExtension(ext);
                }
                def.RdfParserType = parser.GetType();
            }

            //Create any new defintions
            String[] newTypes = types.Where(t => !GetDefinitions(t).Any()).ToArray();
            if (!newTypes.Any()) return;
            MimeTypeDefinition newDef = new MimeTypeDefinition {MimeTypes = newTypes, FileExtensions = extensions, RdfParserType = parser.GetType()};
            AddDefinition(newDef);
        }

        /// <summary>
        /// Registers a writer as the default RDF Writer for all the given MIME types and updates relevant definitions to include the MIME types and file extensions
        /// </summary>
        /// <param name="writer">RDF Writer</param>
        /// <param name="mimeTypes">MIME Types</param>
        /// <param name="fileExtensions">File Extensions</param>
        public static void RegisterWriter(IRdfWriter writer, IEnumerable<String> mimeTypes, IEnumerable<String> fileExtensions)
        {
            if (!_init) Init();

            String[] types = mimeTypes as string[] ?? mimeTypes.ToArray();
            if (!types.Any()) throw new RdfException("Cannot register a writer without specifying at least 1 MIME Type");

            //Get any existing defintions that are to be altered
            IEnumerable<MimeTypeDefinition> existing = GetDefinitions(types);
            String[] extensions = fileExtensions as string[] ?? fileExtensions.ToArray();
            foreach (MimeTypeDefinition def in existing)
            {
                foreach (String type in types)
                {
                    def.AddMimeType(type);
                }
                foreach (String ext in extensions)
                {
                    def.AddFileExtension(ext);
                }
                def.RdfWriterType = writer.GetType();
            }

            //Create any new defintions
            String[] newTypes = types.Where(t => !GetDefinitions(t).Any()).ToArray();
            if (!newTypes.Any()) return;
            MimeTypeDefinition newDef = new MimeTypeDefinition
                                            {
                                                MimeTypes = newTypes, FileExtensions = extensions
                                                , RdfWriterType = writer.GetType()
                                            };
            AddDefinition(newDef);
        }

        /// <summary>
        /// Gets all MIME Type definitions which support the given MIME type
        /// </summary>
        /// <param name="mimeType">MIME Type</param>
        /// <returns></returns>
        public static IEnumerable<MimeTypeDefinition> GetDefinitions(String mimeType)
        {
            if (mimeType == null) return Enumerable.Empty<MimeTypeDefinition>();

            if (!_init) Init();

            MimeTypeSelector selector = MimeTypeSelector.Create(mimeType, 1);
            return (from definition in Definitions
                    where definition.SupportsMimeType(selector)
                    select definition);
        }

        /// <summary>
        /// Gets all MIME Type definitions which support the given MIME Types
        /// </summary>
        /// <param name="mimeTypes">MIME Types</param>
        /// <returns></returns>
        public static IEnumerable<MimeTypeDefinition> GetDefinitions(IEnumerable<String> mimeTypes)
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
        /// Gets all MIME Types definitions which are associated with a given file extension
        /// </summary>
        /// <param name="fileExt">File Extension</param>
        /// <returns></returns>
        public static IEnumerable<MimeTypeDefinition> GetDefinitionsByFileExtension(String fileExt)
        {
            if (fileExt == null) return Enumerable.Empty<MimeTypeDefinition>();

            if (!_init) Init();

            return (from def in Definitions
                    where def.SupportsFileExtension(fileExt)
                    select def).Distinct();
        }

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

                foreach (MimeTypeDefinition definition in Definitions)
                {
                    if (!definition.CanParseRdf) continue;
                    output.Append(definition.ToHttpHeader());
                    output.Append(',');
                }
                if (output[output.Length - 1] == ',') output.Remove(output.Length - 1, 1);
                output.Append(",*/*;q=0.1");

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
            foreach (MimeTypeDefinition definition in Definitions)
            {
                if (requiredType == definition.RdfParserType)
                {
                    return definition.ToHttpHeader();
                }
            }
            throw new ArgumentException("There are no MIME type definitions associated with the given parser type", "parser");
        }

        /// <summary>
        /// Gets the Enumeration of supported MIME Types for RDF Graphs
        /// </summary>
        public static IEnumerable<String> SupportedRdfMimeTypes
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
        /// Generates a Filename Filter that can be used with any .Net application and includes all RDF formats that dotNetRDF is aware of
        /// </summary>
        /// <returns></returns>
        public static String GetFilenameFilter()
        {
            return GetFilenameFilter(true);
        }

        /// <summary>
        /// Generates a Filename Filter that can be used with any .Net application and includes all the RDF formats that dotNetRDF is aware of
        /// </summary>
        /// <param name="allFiles">Whether to also allow all files (i.e. */*)</param>
        /// <returns></returns>
        public static string GetFilenameFilter(bool allFiles)
        {
            if (!_init) Init();

            String filter = String.Empty;
            List<String> exts = new List<string>();

            foreach (MimeTypeDefinition def in Definitions)
            {
                if ((!def.CanParseRdf && !def.CanWriteRdf)) continue;
                exts.AddRange(def.FileExtensions);
                filter += def.SyntaxName + " Files|*." + String.Join(";*.", def.FileExtensions.ToArray()) + "|";
            }
            //Add an All Supported Formats option as first option
            filter = "All Supported Files|*." + String.Join(";*.", exts.ToArray()) + "|" + filter;

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
        /// Applies global options to a writer
        /// </summary>
        /// <param name="writer">Writer</param>
        private static void ApplyWriterOptions(Object writer)
        {
            if (writer is ICompressingWriter)
            {
                ((ICompressingWriter) writer).CompressionLevel = IOOptions.DefaultCompressionLevel;
            }
            if (writer is IDtdWriter)
            {
                ((IDtdWriter) writer).UseDtd = IOOptions.UseDtd;
            }
        }

        /// <summary>
        /// Applies global options to a parser
        /// </summary>
        /// <param name="parser">Parser</param>
        public static void ApplyParserOptions(Object parser)
        {
            if (parser is ITokenisingParser)
            {
                ((ITokenisingParser) parser).TokenQueueMode = IOOptions.DefaultTokenQueueMode;
            }
        }

        /// <summary>
        /// Selects an appropriate <see cref="IRdfWriter">IRdfWriter</see> based on the given MIME Types
        /// </summary>
        /// <param name="ctypes">MIME Types</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This method does not take account of any quality/charset preference parameters included in the Accept Header
        /// </para>
        /// <para>
        /// Global options pertaining to writers will be applied to the selected writer
        /// </para>
        /// </remarks>
        public static IRdfWriter GetWriter(IEnumerable<String> ctypes)
        {
            String temp;
            return GetWriter(ctypes, out temp);
        }

        /// <summary>
        /// Selects an appropriate <see cref="IRdfWriter">IRdfWriter</see> based on the given MIME Types
        /// </summary>
        /// <param name="ctypes">MIME Types</param>
        /// <param name="contentType">The Content Type header that should be sent in the Response to the Request</param>
        /// <remarks>
        /// <para>
        /// This method does not take account of any quality/charset preference parameters included in the Accept Header
        /// </para>
        /// <para>
        /// Global options pertaining to writers will be applied to the selected writer
        /// </para>
        /// </remarks>
        /// <returns></returns>
        public static IRdfWriter GetWriter(IEnumerable<String> ctypes, out String contentType)
        {
            if (ctypes != null)
            {
                //See if there are any MIME Type Definitions for the given MIME Types
                foreach (MimeTypeDefinition definition in GetDefinitions(ctypes))
                {
                    //If so return the Writer from the first match found
                    if (!definition.CanWriteRdf) continue;
                    IRdfWriter writer = definition.GetRdfWriter();
                    ApplyWriterOptions(writer);
                    contentType = definition.CanonicalMimeType;
                    return writer;
                }
            }

            //Default to Turtle
            contentType = Turtle[0];
            IRdfWriter defaultWriter = new CompressingTurtleWriter();
            ApplyWriterOptions(defaultWriter);
            return defaultWriter;
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
        /// Global options pertaining to writers will be applied to the selected writer
        /// </para>
        /// </remarks>
        public static IRdfWriter GetWriter(String acceptHeader, out String contentType)
        {
            String[] ctypes;

            //Parse Accept Header into a String Array
            if (acceptHeader != null)
            {
                acceptHeader = acceptHeader.Trim();
                ctypes = acceptHeader.Contains(",") ? acceptHeader.Split(',') : new String[] {acceptHeader};
            }
            else
            {
                ctypes = new String[] {};
            }

            return GetWriter(ctypes, out contentType);
        }

        /// <summary>
        /// Selects an appropriate <see cref="IRdfWriter">IRdfWriter</see> based on the HTTP Accept header form a HTTP Request
        /// </summary>
        /// <param name="acceptHeader">Value of the HTTP Accept Header</param>
        /// <returns>A Writer for a Content Type the client accepts</returns>
        /// <remarks>
        /// <para>
        /// This method does not take account of any quality/charset preference parameters included in the Accept Header
        /// </para>
        /// <para>
        /// Global options pertaining to writers will be applied to the selected writer
        /// </para>
        /// </remarks>
        public static IRdfWriter GetWriter(String acceptHeader)
        {
            String temp;
            return GetWriter(acceptHeader, out temp);
        }

        /// <summary>
        /// Selects a <see cref="IRdfWriter"/> based on the file extension
        /// </summary>
        /// <param name="fileExt">File Extension</param>
        /// <exception cref="RdfWriterSelectionException">Thrown if no writers are associated with the given file extension</exception>
        /// <remarks>
        /// <para>
        /// Global options pertaining to writers will be applied to the selected writer
        /// </para>
        /// </remarks>
        /// <returns></returns>
        public static IRdfWriter GetWriterByFileExtension(String fileExt)
        {
            String temp;
            return GetWriterByFileExtension(fileExt, out temp);
        }

        /// <summary>
        /// Selects a <see cref="IRdfWriter"/> based on the file extension
        /// </summary>
        /// <param name="fileExt">File Extension</param>
        /// <param name="contentType">Content Type of the chosen writer</param>
        /// <exception cref="RdfWriterSelectionException">Thrown if no writers are associated with the given file extension</exception>
        /// <remarks>
        /// <para>
        /// Global options pertaining to writers will be applied to the selected writer
        /// </para>
        /// </remarks>
        /// <returns></returns>
        public static IRdfWriter GetWriterByFileExtension(String fileExt, out String contentType)
        {
            if (fileExt == null) throw new ArgumentNullException("fileExt", "File extension cannot be null");

            //See if there are any MIME Type Definition for the file extension
            foreach (MimeTypeDefinition definition in GetDefinitionsByFileExtension(fileExt))
            {
                //If so return the Writer from the first match found
                if (!definition.CanWriteRdf) continue;
                IRdfWriter writer = definition.GetRdfWriter();
                ApplyWriterOptions(writer);
                contentType = definition.CanonicalMimeType;
                return writer;
            }

            //Error if unable to select
            contentType = null;
            throw new RdfWriterSelectionException("Unable to select a RDF writer, no writers are associated with the file extension '" + fileExt + "'");
        }

        /// <summary>
        /// Selects an appropriate <see cref="IRdfReader">IRdfReader</see> based on the given MIME Types
        /// </summary>
        /// <param name="ctypes">MIME TYpes</param>
        /// <returns></returns>
        public static IRdfReader GetParser(IEnumerable<String> ctypes)
        {
            String[] mimeTypes = ctypes as string[] ?? ctypes.ToArray();
            if (ctypes != null)
            {
                foreach (MimeTypeDefinition definition in GetDefinitions(mimeTypes))
                {
                    if (!definition.CanParseRdf) continue;
                    IRdfReader parser = definition.GetRdfParser();
                    ApplyParserOptions(parser);
                    return parser;
                }
            }

            String types = (ctypes == null) ? String.Empty : String.Join(",", mimeTypes.ToArray());
            throw new RdfParserSelectionException("The Library does not contain any Parsers for RDF Graphs in any of the following MIME Types: " + types);
        }

        /// <summary>
        /// Selects an appropriate <see cref="IRdfReader">IRdfReader</see> based on the HTTP Content-Type header from a HTTP Response
        /// </summary>
        /// <param name="contentType">Value of the HTTP Content-Type Header</param>
        /// <returns></returns>
        public static IRdfReader GetParser(String contentType)
        {
            return GetParser(contentType.AsEnumerable());
        }

        /// <summary>
        /// Selects a <see cref="IRdfReader"/> based on the file extension
        /// </summary>
        /// <param name="fileExt">File Extension</param>
        /// <returns></returns>
        public static IRdfReader GetParserByFileExtension(String fileExt)
        {
            if (fileExt == null) throw new ArgumentNullException("fileExt", "File extension cannot be null");

            foreach (MimeTypeDefinition def in GetDefinitionsByFileExtension(fileExt))
            {
                if (!def.CanParseRdf) continue;
                IRdfReader parser = def.GetRdfParser();
                ApplyParserOptions(parser);
                return parser;
            }

            throw new RdfParserSelectionException("The Library does not contain any Parsers for RDF Graphs associated with the File Extension '" + fileExt + "'");
        }

        /// <summary>
        /// Gets the true file extension for a filename
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <returns>True extension</returns>
        /// <remarks>
        /// <para>
        /// This is an alternative to using <see cref="System.IO.Path.GetExtension(String)"/> which is designed to take into account known extensions which are used in conjunction with other extensions and mask the true extension, for example <strong>.gz</strong>
        /// </para>
        /// <para>
        /// Consider the filename <strong>example.ttl.gz</strong>, obtaining the extension the standard way gives only <strong>.gz</strong> which is unhelpful since it doesn't actually tell us the underlying format of the data only that it is GZipped and if it is GZipped we almost certainly want to stream the data rather than read all into memory and heuristically detect the actual format.  Instead we'd like to get <strong>.ttl.gz</strong> as the file extension which is much more useful and this is what this function does.
        /// </para>
        /// <para>
        /// <strong>Important:</strong> This method does not blindly return double extensions whenever they are present (since they may simply by period characters in the filename and not double extensions at all) rather it returns double extensions only when the standard extension is an extension is known to be used with double extensions e.g. <strong>.gz</strong> that is relevant to the library.  Acceptable double extensions are currently defined by the <see cref="AllowedStackableExtensions"/> array.
        /// </para>
        /// </remarks>
        public static String GetTrueFileExtension(String filename)
        {
            String actualFilename = Path.GetFileName(filename);
            if (actualFilename == null) throw new ArgumentException("Path.GetFileName() failed to get a file name for the given file name", "filename");
            int extIndex = actualFilename.IndexOf('.');

            //If no extension(s) return standard method
            if (extIndex == -1) return Path.GetExtension(filename);

            //Otherwise get the detected extension and then check for double extensions
            String stdExt = Path.GetExtension(actualFilename);
            if (stdExt == null) throw new ArgumentException("Path.GetExtension() failed to get an extension for the given file name", "filename");

            //Only proceed to do double extension checking if the extension is known to be stackable
            if (!AllowedStackableExtensions.Contains(stdExt.Substring(1))) return stdExt;

            int stdIndex = actualFilename.Length - stdExt.Length;

            //If the indexes match then the standard method returned the only extension present
            if (extIndex == stdIndex) return stdExt;

            //Otherwise we have a double extension
            actualFilename = actualFilename.Substring(0, stdIndex);
            String realExt = Path.GetExtension(actualFilename);

            return realExt + stdExt;
        }

        /// <summary>
        /// Gets the true extension for a resource
        /// </summary>
        /// <param name="resource">Resource</param>
        /// <returns></returns>
        public static String GetTrueResourceExtension(String resource)
        {
            int extIndex = resource.IndexOf('.');

            //if no extensions(s) return empty
            if (extIndex == -1) return String.Empty;

            //Get the standard extension
            String stdExt = resource.Substring(resource.LastIndexOf('.'));

            //Only proceed to do double extension checking if the extension is known to be stackable
            if (!AllowedStackableExtensions.Contains(stdExt.Substring(1))) return stdExt;

            int stdIndex = resource.Length - stdExt.Length;

            //If the indexes match then the standard method returned the only extension present
            if (extIndex == stdIndex) return stdExt;

            //Otherwise we have a double extension
            String partialResource = resource.Substring(0, stdIndex);
            String realExt = partialResource.Substring(partialResource.LastIndexOf('.'));

            return realExt + stdExt;
        }

        /// <summary>
        /// Selects the appropriate File Extension for the given MIME Type
        /// </summary>
        /// <param name="mimeType">MIME Type</param>
        /// <returns></returns>
        public static String GetFileExtension(String mimeType)
        {
            if (!_init) Init();
            MimeTypeSelector selector = MimeTypeSelector.Create(mimeType, 1);
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
        /// Selects the appropriate File Extension for the given RDF Writer
        /// </summary>
        /// <param name="writer">RDF Writer</param>
        /// <returns></returns>
        public static String GetFileExtension(IRdfWriter writer)
        {
            if (!_init) Init();
            Type requiredType = writer.GetType();
            foreach (MimeTypeDefinition definition in Definitions)
            {
                if (requiredType == definition.RdfWriterType)
                {
                    return definition.CanonicalFileExtension;
                }
            }

            throw new RdfException("Unable to determine the appropriate File Extension for the RDF Writer '" + writer.GetType() + "'");
        }
    }
}