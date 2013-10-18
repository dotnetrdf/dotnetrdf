using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF
{
    public static class SparqlIOManager
    {

        /// <summary>
        /// Selects a SPARQL Parser based on the MIME types
        /// </summary>
        /// <param name="ctypes">MIME Types</param>
        /// <param name="allowPlainTextResults">Whether to allow for plain text results</param>
        /// <returns></returns>
        public static ISparqlResultsReader GetSparqlParser(IEnumerable<String> ctypes, bool allowPlainTextResults)
        {
            foreach (MimeTypeDefinition definition in IOManager.GetDefinitions(ctypes))
            {
                if (definition.CanParseSparqlResults)
                {
                    ISparqlResultsReader parser = definition.GetSparqlResultsParser();
                    IOManager.ApplyParserOptions(parser);
                    return parser;
                }
            }

            if (allowPlainTextResults && (ctypes.Contains("text/plain") || ctypes.Contains("text/boolean")))
            {
                ISparqlResultsReader bParser = new SparqlBooleanParser();
                IOManager.ApplyParserOptions(bParser);
                return bParser;
            }
            else
            {
                String types = (ctypes == null) ? String.Empty : String.Join(",", ctypes.ToArray());
                throw new RdfParserSelectionException("The Library does not contain any Parsers for SPARQL Results in any of the following MIME Types: " + types);
            }
        }

        /// <summary>
        /// Selects an appropriate <see cref="ISparqlResultsReader">ISparqlResultsReader</see> based on the HTTP Content-Type header from a HTTP Response
        /// </summary>
        /// <param name="contentType">Value of the HTTP Content-Type Header</param>
        /// <returns></returns>
        public static ISparqlResultsReader GetSparqlParser(String contentType)
        {
            return IOManager.GetSparqlParser(contentType.AsEnumerable(), false);
        }

        /// <summary>
        /// Selects an appropriate <see cref="ISparqlResultsReader">ISparqlResultsReader</see> based on the HTTP Content-Type header from a HTTP Response
        /// </summary>
        /// <param name="contentType">Value of the HTTP Content-Type Header</param>
        /// <param name="allowPlainTextResults">Whether you allow Sparql Boolean results in text/plain format (Boolean results in text/boolean are handled properly but text/plain results can be conflated with CONSTRUCT/DESCRIBE results in NTriples format)</param>
        /// <returns></returns>
        public static ISparqlResultsReader GetSparqlParser(String contentType, bool allowPlainTextResults)
        {
            return IOManager.GetSparqlParser(contentType.AsEnumerable(), allowPlainTextResults);
        }

        /// <summary>
        /// Selects a <see cref="ISparqlResultsReader"/> based on the file extension
        /// </summary>
        /// <param name="fileExt">File Extension</param>
        /// <returns></returns>
        public static ISparqlResultsReader GetSparqlParserByFileExtension(String fileExt)
        {
            if (fileExt == null) throw new ArgumentNullException("fileExt", "File Extension cannot be null");

            foreach (MimeTypeDefinition def in IOManager.GetDefinitionsByFileExtension(fileExt))
            {
                if (def.CanParseSparqlResults)
                {
                    ISparqlResultsReader parser = def.GetSparqlResultsParser();
                    IOManager.ApplyParserOptions(parser);
                    return parser;
                }
            }

            throw new RdfParserSelectionException("The Library does not contain a Parser for SPARQL Results associated with the file extension '" + fileExt + "'");
        }

        /// <summary>
        /// Selects an appropriate <see cref="ISparqlResultsWriter">ISparqlResultsWriter</see> based on the given MIME Types
        /// </summary>
        /// <param name="ctypes">MIME Types</param>
        /// <returns>A Writer for a Content Type the client accepts</returns>
        /// <remarks>
        /// <para>
        /// This method does not take account of any quality/charset preference parameters included in the Accept Header
        /// </para>
        /// <para>
        /// Global options pertaining to writers will be applied to the selected writer
        /// </para>
        /// </remarks>
        public static ISparqlResultsWriter GetSparqlWriter(IEnumerable<String> ctypes)
        {
            String temp;
            return GetSparqlWriter(ctypes, out temp);
        }

        /// <summary>
        /// Selects an appropriate <see cref="ISparqlResultsWriter">ISparqlResultsWriter</see> based on the HTTP Accept header form a HTTP Request
        /// </summary>
        /// <param name="ctypes">String array of accepted Content Types</param>
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
        public static ISparqlResultsWriter GetSparqlWriter(IEnumerable<String> ctypes, out String contentType)
        {
            foreach (MimeTypeDefinition definition in IOManager.GetDefinitions(ctypes))
            {
                if (definition.CanWriteSparqlResults)
                {
                    contentType = definition.CanonicalMimeType;
                    ISparqlResultsWriter writer = definition.GetSparqlResultsWriter();
                    IOManager.ApplyWriterOptions(writer);
                    return writer;
                }
            }

            //Default to SPARQL XML Output
            contentType = IOManager.SparqlResultsXml[0];
            ISparqlResultsWriter defaultWriter = new SparqlXmlWriter();
            IOManager.ApplyWriterOptions(defaultWriter);
            return defaultWriter;
        }

        /// <summary>
        /// Selects an appropriate <see cref="ISparqlResultsWriter">ISparqlResultsWriter</see> based on the HTTP Accept header form a HTTP Request
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
        /// <remarks>
        /// <para>
        /// This method does not take account of any quality/charset preference parameters included in the Accept Header
        /// </para>
        /// <para>
        /// Global options pertaining to writers will be applied to the selected writer
        /// </para>
        /// </remarks>
        public static ISparqlResultsWriter GetSparqlWriter(String acceptHeader)
        {
            String temp;
            return GetSparqlWriter(acceptHeader, out temp);
        }

        /// <summary>
        /// Selects a <see cref="ISparqlResultsWriter"/> based on a file extension
        /// </summary>
        /// <param name="fileExt">File Extension</param>
        /// <returns></returns>
        public static ISparqlResultsWriter GetSparqlWriterByFileExtension(String fileExt)
        {
            String temp;
            return IOManager.GetSparqlWriterByFileExtension(fileExt, out temp);
        }

        /// <summary>
        /// Selects a <see cref="ISparqlResultsWriter"/> based on a file extension
        /// </summary>
        /// <param name="fileExt">File Extension</param>
        /// <param name="contentType">Content Type of the selected writer</param>
        /// <returns></returns>
        public static ISparqlResultsWriter GetSparqlWriterByFileExtension(String fileExt, out String contentType)
        {
            if (fileExt == null) throw new ArgumentNullException("fileExt", "File Extension cannot be null");

            foreach (MimeTypeDefinition def in IOManager.GetDefinitionsByFileExtension(fileExt))
            {
                if (def.CanWriteSparqlResults)
                {
                    ISparqlResultsWriter writer = def.GetSparqlResultsWriter();
                    IOManager.ApplyWriterOptions(writer);
                    contentType = def.CanonicalMimeType;
                    return writer;
                }
            }

            throw new RdfWriterSelectionException("Unable to select a SPARQL Results Writer, no writers are associated with the file extension '" + fileExt + "'");
        }
    }
}
