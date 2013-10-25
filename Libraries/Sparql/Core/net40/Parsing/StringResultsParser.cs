using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing
{
    public static class StringResultsParser
    {
        /// <summary>
        /// Parses a raw SPARQL Results String (attempts to auto-detect the format)
        /// </summary>
        /// <param name="results">SPARQL Result Set to fill</param>
        /// <param name="data">Raw SPARQL Results String</param>
        /// <remarks>
        /// <p>
        /// Auto-detection is based on testing the string to see if it contains certain keyword constructs which might indicate a particular syntax has been used.  This detection may not always be accurate.
        /// </p>
        /// </remarks>
        public static void ParseResultSet(SparqlResultSet results, String data)
        {
            if (results == null) throw new RdfParseException("Cannot read SPARQL Results into a null Result Set");
            if (data == null) return;

            //Try to guess the format
            String format = "Unknown";
            try
            {
                if (data.Contains("<?xml") || data.Contains("<sparql"))
                {
                    //Probably XML
                    format = "SPARQL Results XML";
                    ParseResultSet(results, data, new SparqlXmlParser());
                }
                else if (data.Contains("\"head\"") &&
                          (data.Contains("\"results\"") || data.Contains("\"boolean\"")) &&
                          data.Contains("{") &&
                          data.Contains("}") &&
                          data.Contains("[") &&
                          data.Contains("]"))
                {
                    //If we have all those things then it's very likely RDF/Json
                    format = "SPARQL Results JSON";
                    ParseResultSet(results, data, new SparqlJsonParser());
                }
                else
                {
                    throw new RdfParserSelectionException("StringParser is unable to detect the SPARQL Results Format as the given String does not appear to be SPARQL Results in either XML or JSON format");
                }
            }
            catch (RdfParseException parseEx)
            {
                //Wrap the exception in an informational exception about what we guessed
                throw new RdfParserSelectionException("StringParser failed to parse the SPARQL Results string correctly, StringParser auto-detection guessed '" + format + "' but this failed to parse.  SPARQL Results string may be malformed or StringParser may have guessed incorrectly", parseEx);
            }
        }

        /// <summary>
        /// Parses a raw SPARQL Results String using the given Parser
        /// </summary>
        /// <param name="results">SPARQL Result Set to fill</param>
        /// <param name="data">Raw SPARQL Results String</param>
        /// <param name="reader">Parser to use</param>
        public static void ParseResultSet(SparqlResultSet results, String data, ISparqlResultsReader reader)
        {
            if (results == null) throw new RdfParseException("Cannot read SPARQL Results into a null Result Set");
            if (data == null) return;

            if (reader == null)
            {
                //If no parser specified then auto-detect syntax
                ParseResultSet(results, data);
            }
            else
            {
                try
                {
                    MemoryStream mem = new MemoryStream();
                    StreamWriter writer = new StreamWriter(mem);
                    writer.Write(data);
                    writer.Flush();
                    mem.Seek(0, SeekOrigin.Begin);

                    reader.Load(results, new StreamReader(mem));
                }
                catch
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Uses the format detection rules to return the most likely SPARQL Results parser
        /// </summary>
        /// <param name="data">Raw SPARQL Results String</param>
        /// <returns></returns>
        public static ISparqlResultsReader GetResultSetParser(String data)
        {
            if (data == null) throw new RdfParserSelectionException("Cannot select a Result Set parser from a null string");

            if ((data.Contains("<?xml") && !data.Contains("rdf:RDF")) || data.Contains("<sparql"))
            {
                //Probably XML
                return new SparqlXmlParser();
            }
            else if (data.Contains("\"head\"") &&
               (data.Contains("\"results\"") || data.Contains("\"boolean\"")) &&
               data.Contains("{") &&
               data.Contains("}") &&
               data.Contains("[") &&
               data.Contains("]"))
            {
                //If we have all those things then it's very likely JSON
                return new SparqlJsonParser();
            }
            else
            {
                throw new RdfParserSelectionException("StringParser is unable to detect the SPARQL Results Format as the given String does not appear to be SPARQL Results in either XML or JSON format");
            }
        }

    }
}
