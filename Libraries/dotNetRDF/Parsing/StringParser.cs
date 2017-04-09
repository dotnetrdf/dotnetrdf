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

using System;
using System.IO;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Static Helper Class which allows raw strings of RDF/SPARQL Results to be parsed directly
    /// </summary>
    /// <remarks>
    /// The API structure for dotNetRDF means that our <see cref="IRdfReader">IRdfReader</see> classes which are our Parsers only have to support parsing from a file or a stream.  For most applications this is fine but there may be occassions when you wish to parse a small fragment of RDF and you don't want to have to put it into a file before you can parse it.
    /// </remarks>
    public static class StringParser
    {
        /// <summary>
        /// Parses a raw RDF String using the given <see cref="IRdfReader">IRdfReader</see>
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="data">Raw RDF String</param>
        /// <param name="reader">Parser to use to read the data</param>
        /// <remarks>Use this when you have a raw RDF string and you know the syntax the RDF is in</remarks>
        public static void Parse(IGraph g, String data, IRdfReader reader)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            if (data == null) return;

            if (reader == null)
            {
                // If no parser supplied then should auto-detect syntax
                Parse(g, data);
            }
            else
            {
                try
                {
                    reader.Load(g, new StringReader(data));
                }
                catch
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Parses a raw RDF String (attempts to auto-detect the format)
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="data">Raw RDF String</param>
        /// <remarks>
        /// <p>
        /// Auto-detection is based on testing the string to see if it contains certain keyword constructs which might indicate a particular syntax has been used.  This detection may not always be accurate and it may choose a parser which is less expressive than the actual syntax e.g. <see cref="TurtleParser">TurtleParser</see> instead of <see cref="Notation3Parser">Notation3Parser</see> as it tends to guess downwards.  
        /// </p>
        /// <p>
        /// For example if you parsed a Notation 3 string that contained Graph Literals but didn't use any of the Notation 3 specific directives like @keywords it would be assumed to be Turtle but then would fail to parse
        /// </p>
        /// <p>
        /// The auto-detection rules used are as follows:
        /// </p>
        /// <ol>
        /// <li>If it contains &lt;?xml and &lt;rdf:RDF then it's most likely RDF/XML</li>
        /// <li>If it contains &lt;html then it's most likely HTML with possibly RDFa embedded</li>
        /// <li>
        /// If it contains @prefix or @base then its Turtle/Notation 3
        ///     <ol>
        ///     <li>If it contains @keywords, @forall or @forsome then it's Notation 3</li>
        ///     <li>Otherwise it's Turtle</li>
        ///     </ol>
        /// </li>
        /// <li>If it contains all of a set of terms and symbols that occur in RDF/JSON then it's most likely RDF/JSON.  These terms are "value","type",{,},[ and ]</li>
        /// <li>Otherwise try it as NTriples, NTriples has no real distinctive syntax so hard to test if it's NTriples other than by parsing it</li>
        /// </ol>
        /// </remarks>
        public static void Parse(IGraph g, String data)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            if (data == null) return;

            // Try to guess the format
            String format = "Unknown";
            try
            {
                if (data.Contains("<?xml") && data.Contains("<rdf:RDF"))
                {
                    // Probably RDF/XML
                    format = "RDF/XML";
                    Parse(g, data, new RdfXmlParser());
                }
                else if (data.Contains("<html"))
                {
                    // HTML (possibly containing RDFa)
                    format = "HTML+RDFa";
                    Parse(g, data, new RdfAParser());
                }
                else if (data.Contains("@prefix") || data.Contains("@base"))
                {
                    // Turtle/Notation 3
                    if (data.Contains("@keywords") || data.Contains("@forall") || data.Contains("@forsome"))
                    {
                        // Notation 3
                        format = "Notation 3";
                        Parse(g, data, new Notation3Parser());
                    }
                    else
                    {
                        // Probably Turtle
                        format = "Turtle";
                        Parse(g, data, new TurtleParser());
                    }
                }
                else if (data.Contains("\"value\"") &&
                           data.Contains("\"type\"") &&
                           data.Contains("{") &&
                           data.Contains("}") &&
                           data.Contains("[") &&
                           data.Contains("]"))
                {
                    // If we have all those things then it's very likely RDF/Json
                    format = "RDF/JSON";
                    Parse(g, data, new RdfJsonParser());
                }
                else
                {
                    // Take a stab at it being NTriples
                    // No real way to test as there's nothing particularly distinctive in NTriples
                    format = "NTriples";
                    Parse(g, data, new NTriplesParser());
                }
            }
            catch (RdfParseException parseEx)
            {
                // Wrap the exception in an informational exception about what we guessed
                throw new RdfParseException("StringParser failed to parse the RDF string correctly, StringParser auto-detection guessed '" + format + "' but this failed to parse.  RDF string may be malformed or StringParser may have guessed incorrectly", parseEx);
            }
        }

        /// <summary>
        /// Parses a raw RDF Dataset String using the given Parser
        /// </summary>
        /// <param name="store">Store to load into</param>
        /// <param name="data">Raw RDF Dataset String</param>
        /// <param name="reader">Parser to use</param>
        public static void ParseDataset(ITripleStore store, String data, IStoreReader reader)
        {
            if (store == null) throw new RdfParseException("Cannot read a RDF dataset into a null Graph");
            if (data == null) return;

            if (reader == null)
            {
                // If no parser supplied then should auto-detect syntax
                ParseDataset(store, data);
            }
            else
            {
                try
                {
                    reader.Load(store, new StringReader(data));
                }
                catch
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Parses a raw RDF Dataset String (attempts to auto-detect the format)
        /// </summary>
        /// <param name="store">Store to load into</param>
        /// <param name="data">Raw RDF Dataset String</param>
        /// <remarks>
        /// <p>
        /// Auto-detection is based on testing the string to see if it contains certain keyword constructs which might indicate a particular syntax has been used.  This detection may not always be accurate.
        /// </p>
        /// </remarks>
        public static void ParseDataset(ITripleStore store, String data)
        {
            if (store == null) throw new RdfParseException("Cannot read a RDF dataset into a null Graph");
            if (data == null) return;

            // Try to guess the format
            String format = "Unknown";
            try
            {
                if (data.Contains("<?xml") && data.Contains("<TriX"))
                {
                    // Probably TriX
                    format = "TriX";
                    ParseDataset(store, data, new TriXParser());
                }
                else if (data.Contains("@prefix") || data.Contains("@base"))
                {
                    // Probably TriG
                    format = "TriG";
                    ParseDataset(store, data, new TriGParser());
                }
                else
                {
                    // Take a stab at it being NQuads
                    // No real way to test as there's nothing particularly distinctive in NQuads
                    format = "NQuads";
                    ParseDataset(store, data, new NQuadsParser());
                }
            }
            catch (RdfParseException parseEx)
            {
                // Wrap the exception in an informational exception about what we guessed
                throw new RdfParseException("StringParser failed to parse the RDF Dataset string correctly, StringParser auto-detection guessed '" + format + "' but this failed to parse.  RDF Dataset string may be malformed or StringParser may have guessed incorrectly", parseEx);
            }
        }

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

            // Try to guess the format
            String format = "Unknown";
            try
            {
                if (data.Contains("<?xml") || data.Contains("<sparql"))
                {
                    // Probably XML
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
                    // If we have all those things then it's very likely RDF/Json
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
                // Wrap the exception in an informational exception about what we guessed
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
                // If no parser specified then auto-detect syntax
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
        /// Uses the rules described in the remarks for the <see cref="StringParser.Parse(IGraph, String)">Parse()</see> to return the most likely Parser
        /// </summary>
        /// <param name="data">Raw RDF String</param>
        public static IRdfReader GetParser(String data)
        {
            if (data == null) throw new RdfParserSelectionException("Cannot select a parser for a null String");

            if (data.Contains("<?xml") && data.Contains("<rdf:RDF"))
            {
                // Probably RDF/XML
                return new RdfXmlParser();
            }
            else if (data.Contains("<html"))
            {
                // HTML (possibly containing RDFa)
                return new RdfAParser();
            }
            else if (data.Contains("@prefix") || data.Contains("@base"))
            {
                // Turtle/Notation 3
                if (data.Contains("@keywords") || data.Contains("@forall") || data.Contains("@forsome"))
                {
                    // Notation 3
                    return new Notation3Parser();
                }
                else
                {
                    // Probably Turtle
                    return new TurtleParser();
                }
            }
            else if (data.Contains("\"value\"") &&
                       data.Contains("\"type\"") &&
                       data.Contains("{") &&
                       data.Contains("}") &&
                       data.Contains("[") &&
                       data.Contains("]"))
            {
                // If we have all those things then it's very likely RDF/Json
                return new RdfJsonParser();
            }
            else
            {
                // Take a stab at it being NTriples
                // No real way to test as there's nothing particularly distinctive in NTriples
                return new NTriplesParser();
            }
        }

        /// <summary>
        /// Uses the format detection rules to determine the most likely RDF Dataset Parser
        /// </summary>
        /// <param name="data">Raw RDF Dataset String</param>
        /// <returns></returns>
        public static IStoreReader GetDatasetParser(String data)
        {
            if (data == null) throw new RdfParserSelectionException("Cannot select a Dataset parser for a null String");

            if (data.Contains("<?xml") && data.Contains("<TriX"))
            {
                // Probably TriX
                return new TriXParser();
            }
            else if (data.Contains("@prefix") || data.Contains("{") || data.Contains("}"))
            {
                // Probably TriG
                return new TriGParser();
            }
            else
            {
                // Take a stab at it being NQuads
                // No real way to test as there's nothing particularly distinctive in NQuads
                return new NQuadsParser();
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
                // Probably XML
                return new SparqlXmlParser();
            }
            else if (data.Contains("\"head\"") &&
               (data.Contains("\"results\"") || data.Contains("\"boolean\"")) &&
               data.Contains("{") &&
               data.Contains("}") &&
               data.Contains("[") &&
               data.Contains("]"))
            {
                // If we have all those things then it's very likely JSON
                return new SparqlJsonParser();
            }
            else
            {
                throw new RdfParserSelectionException("StringParser is unable to detect the SPARQL Results Format as the given String does not appear to be SPARQL Results in either XML or JSON format");
            }
        }
    }
}
