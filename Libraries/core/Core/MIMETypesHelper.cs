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
        #region MIME Types

        /// <summary>
        /// MIME Type for accept any content Type
        /// </summary>
        public static String Any = "*/*";

        /// <summary>
        /// MIME Type for URL Encoded WWW Form Content used when POSTing over HTTP
        /// </summary>
        public static String WWWFormURLEncoded = "application/x-www-form-urlencoded";

        /// <summary>
        /// MIME Types for Turtle
        /// </summary>
        public static string[] Turtle = { "text/turtle", "application/x-turtle", "application/turtle" };

        /// <summary>
        /// MIME Types for RDF/XML
        /// </summary>
        public static string[] RdfXml = { "application/rdf+xml", "text/xml" };

        /// <summary>
        /// MIME Types for Notation 3
        /// </summary>
        public static string[] Notation3 = { "text/n3", "text/rdf+n3" };

        /// <summary>
        /// MIME Types for NTriples
        /// </summary>
        public static string[] NTriples = { "text/plain", "application/x-ntriples" };

        /// <summary>
        /// MIME Types for NQuads
        /// </summary>
        public static string[] NQuads = { "text/x-nquads" };

        /// <summary>
        /// MIME Types for TriG
        /// </summary>
        public static string[] TriG = { "application/x-trig" };

        /// <summary>
        /// MIME Types for TriX
        /// </summary>
        public static string[] TriX = { "application/trix" };

        /// <summary>
        /// MIME Types for RDF/Json
        /// </summary>
        public static string[] Json = { "application/json", "text/json" };

        /// <summary>
        /// MIME Types for Sparql Result Sets
        /// </summary>
        public static string[] Sparql = { "application/sparql-results+xml", "application/sparql-results+json" };

        /// <summary>
        /// MIME Types for CSV
        /// </summary>
        public static string[] Csv = { "text/csv", "text/comma-separated-values" };

        /// <summary>
        /// MIME Types for TSV
        /// </summary>
        public static string[] Tsv = { "text/tab-separated-values" };

        /// <summary>
        /// MIME Types for HTML
        /// </summary>
        public static string[] Html = { "text/html", "application/xhtml+xml" };

        /// <summary>
        /// Gets the Canonical Type from a set of MIME Types where it is assumed the first type in the list is the Canonical Type
        /// </summary>
        /// <param name="mimeTypes">MIME Types</param>
        /// <returns></returns>
        public static String GetCanonicalType(IEnumerable<String> mimeTypes)
        {
            return mimeTypes.First();
        }

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
                StringBuilder output = new StringBuilder();
                output.Append(String.Join(",", RdfXml));
                output.Append(",");
                output.Append(String.Join(",", Notation3));
                output.Append(",");
                output.Append(String.Join(",", Turtle));
                output.Append(",");
                output.Append(String.Join(",", NTriples));
                output.Append(",");
                output.Append(String.Join(",", Json));
                output.Append(";q=0.9,*/*;q=0.8");
                //output.Append(",*/*");

                return output.ToString();
            }
        }

        /// <summary>
        /// Builds the String for the HTTP Accept Header that should be used for querying Sparql Endpoints where the response will be a Sparql Result Set format
        /// </summary>
        /// <returns></returns>
        public static String HttpSparqlAcceptHeader
        {
            get
            {
                StringBuilder output = new StringBuilder();
                output.Append(String.Join(",", Sparql));
                output.Append(";q=1.0");

                return output.ToString();
            }
        }

        /// <summary>
        /// Builds the String for the HTTP Accept Header that should be used for making HTTP Requests where the returned data may be RDF or a Sparql Result Set
        /// </summary>
        /// <returns></returns>
        public static String HttpRdfOrSparqlAcceptHeader
        {
            get
            {
                StringBuilder output = new StringBuilder();
                output.Append(String.Join(",", RdfXml));
                output.Append(",");
                output.Append(String.Join(",", Notation3));
                output.Append(",");
                output.Append(String.Join(",", Turtle));
                output.Append(",");
                output.Append(String.Join(",", NTriples));
                output.Append(",");
                output.Append(String.Join(",", Json));
                output.Append(",");
                output.Append(String.Join(",", Sparql));
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
                StringBuilder output = new StringBuilder();
                output.Append(String.Join(",", TriG));
                output.Append(",");
                output.Append(String.Join(",", NQuads));
                output.Append(",");
                output.Append(String.Join(",", TriX));
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
                StringBuilder output = new StringBuilder();
                output.Append(String.Join(",", RdfXml));
                output.Append(",");
                output.Append(String.Join(",", Notation3));
                output.Append(",");
                output.Append(String.Join(",", Turtle));
                output.Append(",");
                output.Append(String.Join(",", NTriples));
                output.Append(",");
                output.Append(String.Join(",", Json));
                output.Append(",");
                output.Append(String.Join(",", TriG));
                output.Append(",");
                output.Append(String.Join(",", NQuads));
                output.Append(",");
                output.Append(String.Join(",", TriX));
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
            if (parser is NTriplesParser)
            {
                return String.Join(",", NTriples);
            }
            else if (parser is TurtleParser)
            {
                return String.Join(",", Turtle);
            }
            else if (parser is Notation3Parser)
            {
                return String.Join(",", Notation3);
            }
            else if (parser is RdfXmlParser)
            {
                return String.Join(",", RdfXml);
            }
            else if (parser is RdfAParser)
            {
                return String.Join(",", MimeTypesHelper.Html);
            }
            else if (parser is RdfJsonParser)
            {
                return String.Join(",", MimeTypesHelper.Json);
            }
            else
            {
                return MimeTypesHelper.HttpAcceptHeader;
            }
        }

        /// <summary>
        /// Gets the Enumeration of supported MIME Types for RDF Graphs
        /// </summary>
        public static IEnumerable<String> SupportedRdfMimeTypes
        {
            get
            {
                return RdfXml.Concat(NTriples).Concat(Turtle).Concat(Notation3).Concat(Json);
            }
        }

        /// <summary>
        /// Gets the Enumeration of supported MIME Types for SPARQL Results
        /// </summary>
        public static IEnumerable<String> SupportedSparqlMimeTypes
        {
            get
            {
                return Sparql;
            }
        }

        /// <summary>
        /// Gets the Enumeration of supported MIME Types for RDF Graphs or SPARQL Results
        /// </summary>
        public static IEnumerable<String> SupportedRdfOrSparqlMimeTypes
        {
            get
            {
                return SupportedRdfMimeTypes.Concat(SupportedSparqlMimeTypes);
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
            bool htmlFallback = false;

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


                if (MimeTypesHelper.Notation3.Contains(type))
                {
                    //Client accepts Notation 3
                    contentType = MimeTypesHelper.Notation3[0];
                    return new Notation3Writer(Options.DefaultCompressionLevel);
                }
                else if (MimeTypesHelper.Turtle.Contains(type))
                {
                    //Client accepts Turtle
                    contentType = MimeTypesHelper.Turtle[0];
                    return new CompressingTurtleWriter(Options.DefaultCompressionLevel);
                }
#if !NO_XMLDOM
                else if (MimeTypesHelper.RdfXml.Contains(type))
                {
                    //Client accepts RDF/XML
                    contentType = MimeTypesHelper.RdfXml[0];
                    return new FastRdfXmlWriter();
                }
#endif
                else if (MimeTypesHelper.NTriples.Contains(type))
                {
                    //Client accepts NTriples
                    contentType = MimeTypesHelper.NTriples[0];
                    return new NTriplesWriter();
                }
                else if (MimeTypesHelper.Json.Contains(type))
                {
                    //Client accepts RDF/Json
                    contentType = MimeTypesHelper.Json[0];
                    return new RdfJsonWriter();
                }
                else if (MimeTypesHelper.Csv.Contains(type))
                {
                    //Client accepts CSV
                    contentType = MimeTypesHelper.Csv[0];
                    return new CsvWriter();
                }
                else if (MimeTypesHelper.Tsv.Contains(type))
                {
                    //Client accepts TSV
                    contentType = MimeTypesHelper.Tsv[0];
                    return new TsvWriter();
                }
                else if (MimeTypesHelper.Html.Contains(type))
                {
                    //Client accepts HTML
                    htmlFallback = true;
                }
            }

            //Default to NTriples unless the User accepted HTML explicitly
            if (htmlFallback)
            {
                contentType = MimeTypesHelper.Html[0];
                return new HtmlWriter();
            }
            else
            {
                contentType = MimeTypesHelper.NTriples[0];
                return new NTriplesWriter();
            }
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

            //Select a Parser based on Content Type
            if (MimeTypesHelper.RdfXml.Contains(contentType))
            {
                //XML/RDF
                return new RdfXmlParser();
            }
            else if (MimeTypesHelper.Turtle.Contains(contentType))
            {
                //Turtle
                return new TurtleParser();
            }
            else if (MimeTypesHelper.Notation3.Contains(contentType))
            {
                //Notation 3 (N3)
                return new Notation3Parser();
            }
            else if (MimeTypesHelper.NTriples.Contains(contentType))
            {
                //NTriples
                return new NTriplesParser();
            }
            else if (MimeTypesHelper.Json.Contains(contentType))
            {
                //RDF/Json
                return new RdfJsonParser();
            }
            else if (MimeTypesHelper.Html.Contains(contentType))
            {
                //May be RDFa embedded in the HTML
                return new RdfAParser();
            }
            else
            {
                throw new RdfParserSelectionException("The Library does not contain a Parser which understands RDF Graphs in the format '" + contentType + "'");
            }
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

            if (MimeTypesHelper.Sparql.Contains(contentType))
            {
                if (MimeTypesHelper.Sparql[1].Equals(contentType))
                {
                    return new SparqlJsonParser();
                }
                else if (MimeTypesHelper.Sparql[0].Equals(contentType))
                {
                    return new SparqlXmlParser();
                }
                else
                {
                    throw new RdfParserSelectionException("The Library does not contain a Parser which understands SPARQL Results in the format '" + contentType + "'");
                }
            }
            else
            {
                throw new RdfParserSelectionException("The Library does not contain a Parser which understands SPARQL Results in the format '" + contentType + "'");
            }
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

            try
            {
                return GetSparqlParser(contentType);
            }
            catch (RdfParserSelectionException)
            {
                if (contentType.Equals(MimeTypesHelper.NTriples[0]))
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

                if (MimeTypesHelper.Sparql.Contains(type))
                {
#if !NO_XMLDOM
                    //Client accepts some form of Sparql Result Set
                    if (type == MimeTypesHelper.Sparql[0])
                    {
                        //Sparql XML Results Format
                        contentType = MimeTypesHelper.Sparql[0];
                        return new SparqlXmlWriter();
                    }
                    else
                    {
#endif
                        //Sparql JSON Results Format
                        contentType = MimeTypesHelper.Sparql[1];
                        return new SparqlJsonWriter();
#if !NO_XMLDOM
                    }
#endif
                }
                else if (MimeTypesHelper.Csv.Contains(type))
                {
                    //CSV Format
                    contentType = MimeTypesHelper.Csv[0];
                    return new SparqlCsvWriter();
                }
                else if (MimeTypesHelper.Tsv.Contains(type))
                {
                    //TSV Format
                    contentType = MimeTypesHelper.Tsv[0];
                    return new SparqlTsvWriter();
                }
                else if (MimeTypesHelper.Json.Contains(type))
                {
                    //SPARQL JSON Results Format
                    contentType = MimeTypesHelper.Sparql[1];
                    return new SparqlJsonWriter();
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

            //Select a Parser based on Content Type
            if (MimeTypesHelper.TriG.Contains(contentType))
            {
                //TriG
                return new TriGParser();
            }
#if !NO_XMLDOM
            else if (MimeTypesHelper.TriX.Contains(contentType))
            {
                //TriX
                return new TriXParser();
            }
#endif
            else if (MimeTypesHelper.NQuads.Contains(contentType))
            {
                //NQuads
                return new NQuadsParser();
            }
            else
            {
                throw new RdfParserSelectionException("The Library does not contain a Parser which understands RDF datasets in the format '" + contentType + "'");
            }
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

                if (MimeTypesHelper.TriG.Contains(type))
                {
                    //Client accepts TriG
                    contentType = MimeTypesHelper.TriG[0];
                    return new TriGWriter();
                }
                else if (MimeTypesHelper.NQuads.Contains(type))
                {
                    //Client accepts NQuads
                    contentType = MimeTypesHelper.NQuads[0];
                    return new NQuadsWriter();
                }
#if !NO_XMLDOM
                else if (MimeTypesHelper.TriX.Contains(type))
                {
                    //Client accepts TriX
                    contentType = MimeTypesHelper.TriX[0];
                    return new TriXWriter();
                }
#endif
                else if (MimeTypesHelper.Csv.Contains(type))
                {
                    //Client accepts CSV
                    contentType = MimeTypesHelper.Csv[0];
                    return new CsvStoreWriter();
                }
                else if (MimeTypesHelper.Tsv.Contains(type))
                {
                    //Client accepts TSV
                    contentType = MimeTypesHelper.Tsv[0];
                    return new TsvStoreWriter();
                }
                else if (MimeTypesHelper.Any.Equals(type))
                {
                    //Default for Accept Any is TriG
                    contentType = MimeTypesHelper.TriG[0];
                    return new TriGWriter();
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

            //Select a File Extension based on Content Type
            if (MimeTypesHelper.RdfXml.Contains(mimeType))
            {
                //XML/RDF
                return DefaultRdfXmlExtension;
            }
            else if (MimeTypesHelper.Turtle.Contains(mimeType))
            {
                //Turtle
                return DefaultTurtleExtension;
            }
            else if (MimeTypesHelper.Notation3.Contains(mimeType))
            {
                //Notation 3 (N3)
                return DefaultNotation3Extension;
            }
            else if (MimeTypesHelper.NTriples.Contains(mimeType))
            {
                //NTriples
                return DefaultNTriplesExtension;
            }
            else if (MimeTypesHelper.Json.Contains(mimeType))
            {
                //RDF/Json
                return DefaultJsonExtension;
            }
            else if (MimeTypesHelper.TriG.Contains(mimeType))
            {
                //TriG
                return DefaultTriGExtension;
            }
            else if (MimeTypesHelper.NQuads.Contains(mimeType))
            {
                //NQuads
                return DefaultNQuadsExtension;
            }
            else if (MimeTypesHelper.TriX.Contains(mimeType))
            {
                //TriX
                return DefaultTriXExtension;
            }
            else if (MimeTypesHelper.Csv.Contains(mimeType))
            {
                //CSV
                return DefaultCsvExtension;
            }
            else if (MimeTypesHelper.Tsv.Contains(mimeType))
            {
                //TSV
                return DefaultTsvExtension;
            }
            else if (MimeTypesHelper.Html.Contains(mimeType))
            {
                //HTML
                return DefaultHtmlExtension;
            }
            else
            {
                throw new RdfException("Unable to determine the appropriate File Extension for the MIME Type '" + mimeType + "'");
            }
        }

        /// <summary>
        /// Selects the appropriate File Extension for the given RDF Writer
        /// </summary>
        /// <param name="writer">RDF Writer</param>
        /// <returns></returns>
        public static String GetFileExtension(IRdfWriter writer)
        {
            if (writer is NTriplesWriter)
            {
                return DefaultNTriplesExtension;
            }
            else if (writer is TurtleWriter || writer is CompressingTurtleWriter)
            {
                return DefaultTurtleExtension;
            }
            else if (writer is Notation3Writer)
            {
                return DefaultNotation3Extension;
            }
#if !NO_XMLDOM
            else if (writer is RdfXmlTreeWriter || writer is FastRdfXmlWriter)
            {
                return DefaultRdfXmlExtension;
            }
#endif
            else if (writer is RdfJsonWriter)
            {
                return DefaultJsonExtension;
            }
            else if (writer is CsvWriter)
            {
                return DefaultCsvExtension;
            }
            else if (writer is TsvWriter)
            {
                return DefaultTsvExtension;
            }
            else if (writer is HtmlWriter)
            {
                return DefaultHtmlExtension;
            }
            else
            {
                throw new RdfException("Unable to determine the appropriate File Extension for the RDF Writer '" + writer.GetType().ToString() + "'");
            }
        }

        /// <summary>
        /// Selects the appropriate File Extension for the given Store Writer
        /// </summary>
        /// <param name="writer">Store Writer</param>
        /// <returns></returns>
        public static String GetFileExtension(IStoreWriter writer)
        {
            if (writer is TriGWriter)
            {
                return DefaultTriGExtension;
            }
#if !NO_XMLDOM
            else if (writer is TriXWriter)
            {
                return DefaultTriXExtension;
            }
#endif
            else if (writer is NQuadsWriter)
            {
                return DefaultNQuadsExtension;
            }
            else if (writer is CsvStoreWriter)
            {
                return DefaultCsvExtension;
            }
            else if (writer is TsvStoreWriter)
            {
                return DefaultTsvExtension;
            }
            else
            {
                throw new RdfException("Unable to determine the appropriate File Extension for the Store Writer '" + writer.GetType().ToString() + "'");
            }
        }

        #endregion

   }
}
