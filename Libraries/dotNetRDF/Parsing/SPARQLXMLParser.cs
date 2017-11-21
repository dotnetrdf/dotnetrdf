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
using System.Web;
using System.Xml;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Parser for SPARQL Results XML Format
    /// </summary>
    public class SparqlXmlParser 
        : ISparqlResultsReader
    {
        /// <summary>
        /// Loads a Result Set from an Input
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="input">Input to read from</param>
        public void Load(SparqlResultSet results, TextReader input)
        {
            if (results == null) throw new RdfParseException("Cannot read SPARQL Results into a null Result Set");
            Load(new ResultSetHandler(results), input);
        }

        /// <summary>
        /// Loads a Result Set from an Input Stream
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="input">Input Stream to read from</param>
        public void Load(SparqlResultSet results, StreamReader input)
        {
            if (results == null) throw new RdfParseException("Cannot read SPARQL Results into a null Result Set");
            Load(new ResultSetHandler(results), input);
        }

        /// <summary>
        /// Loads a Result Set from a File
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="filename">File to load from</param>
        public void Load(SparqlResultSet results, string filename)
        {
            if (results == null) throw new RdfParseException("Cannot read SPARQL Results into a null Result Set");
            if (filename == null) throw new RdfParseException("Cannot read SPARQL Results from a null File");
            Load(results, new StreamReader(File.OpenRead(filename)));
        }

        /// <summary>
        /// Loads a Result Set from an Input using a Results Handler
        /// </summary>
        /// <param name="handler">Results Handler to use</param>
        /// <param name="input">Input to read from</param>
        public void Load(ISparqlResultsHandler handler, TextReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read SPARQL Results using a null Results Handler");
            if (input == null) throw new RdfParseException("Cannot read SPARQL Results from a null Input");

            try
            {
                // Parse the XML
                XmlReader reader = XmlReader.Create(input, GetSettings());
                Parse(new SparqlXmlParserContext(reader, handler));
            }
            catch
            {
                throw;
            }
            finally
            {
                try
                {
                    input.Close();
                }
                catch
                {
                    // No catch actions - this is a cleanup attempt only
                }
            }
        }

        /// <summary>
        /// Loads a Result Set from an Input using a Results Handler
        /// </summary>
        /// <param name="handler">Results Handler to use</param>
        /// <param name="input">Input Stream to read from</param>
        public void Load(ISparqlResultsHandler handler, StreamReader input)
        {
            if (input == null) throw new RdfParseException("Cannot read SPARQL Results from a null input");
            Load(handler, (TextReader)input);
        }

        /// <summary>
        /// Loads a Result Set from a file using a Results Handler
        /// </summary>
        /// <param name="handler">Results Handler to use</param>
        /// <param name="filename">File to read from</param>
        public void Load(ISparqlResultsHandler handler, String filename)
        {
            if (filename == null) throw new RdfParseException("Cannot read SPARQL Results from a null File");
            Load(handler, new StreamReader(File.OpenRead(filename)));
        }

        /// <summary>
        /// Initialises the XML Reader settings
        /// </summary>
        /// <returns></returns>
        private XmlReaderSettings GetSettings()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
#if NETCORE
            settings.DtdProcessing = DtdProcessing.Ignore;
#elif NET40
            settings.DtdProcessing = DtdProcessing.Parse;
#endif
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            return settings;
        }

        /// <summary>
        /// Parses the XML Result Set format into a set of SPARQLResult objects
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void Parse(SparqlXmlParserContext context)
        {
            try
            {
                context.Handler.StartResults();

                // Get the Document Element and check it's a Sparql element
                if (!context.Input.Read()) throw new RdfParseException("Unable to Parse a SPARQL Result Set as it was not possible to read a document element from the input");
                while (context.Input.NodeType != XmlNodeType.Element)
                {
                    if (!context.Input.Read()) throw new RdfParseException("Unable to Parse a SPARQL Result Set as it was not possible to read a document element from the input");
                }
                if (!context.Input.Name.Equals("sparql"))
                {
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set from the provided XML since the Document Element is not a <sparql> element!");
                }

                // Go through it's attributes and check the Namespace is specified
                bool nsfound = false;
                if (context.Input.HasAttributes)
                {
                    for (int i = 0; i < context.Input.AttributeCount; i++)
                    {
                        context.Input.MoveToNextAttribute();
                        if (context.Input.Name.Equals("xmlns"))
                        {
                            if (!context.Input.Value.Equals(SparqlSpecsHelper.SparqlNamespace))
                            {
                                throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <sparql> element has an incorrect Namespace!");
                            }
                            else
                            {
                                nsfound = true;
                            }
                        }
                    }
                }
                if (!nsfound)
                {
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <sparql> element fails to specify the SPARQL Namespace!");
                }

                // Get the Variables from the Header
                if (!context.Input.Read()) throw new RdfParseException("Unable to Parse a SPARQL Result Set as could not read a <head> element from the input");
                if (!context.Input.Name.Equals("head"))
                {
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set since the first Child Node of the <sparql> element is not the required <head> element!");
                }

                // Only parser <variable> and <link> elements if not an empty <head /> element
                if (!context.Input.IsEmptyElement)
                {
                    while (context.Input.Read())
                    {
                        // Stop reading when we hit the </head>
                        if (context.Input.NodeType == XmlNodeType.EndElement && context.Input.Name.Equals("head")) break;

                        // Looking for <variable> elements
                        if (context.Input.Name.Equals("variable"))
                        {
                            // Should only have 1 attribute
                            if (context.Input.AttributeCount != 1)
                            {
                                throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <variable> element has too few/many attributes, only a 'name' attribute should be present!");
                            }
                            else
                            {
                                // Add the Variable to the list
                                context.Input.MoveToNextAttribute();
                                if (!context.Handler.HandleVariable(context.Input.Value)) ParserHelper.Stop();
                                context.Variables.Add(context.Input.Value);
                            }
                        }
                        else if (context.Input.Name.Equals("link"))
                        {
                            // Not bothered about <link> elements
                        }
                        else
                        {
                            // Some unexpected element
                            throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <head> contains an unexpected element <" + context.Input.Name + ">!");
                        }
                    }
                }

                if (!context.Input.Name.Equals("head"))
                {
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set as reached the end of the input before the closing </head> element was found");
                }

                // Look at the <results> or <boolean> element
                if (!context.Input.Read()) throw new RdfParseException("Unable to Parse a SPARQL Result Set as could not read a <results> element from the input");
                if (context.Input.Name.Equals("results"))
                {
                    // Only parser <result> elements if it's not an empty <results /> element
                    if (!context.Input.IsEmptyElement)
                    {
                        while (context.Input.Read())
                        {
                            // Stop reading when we hit the </results>
                            if (context.Input.NodeType == XmlNodeType.EndElement && context.Input.Name.Equals("results")) break;

                            // Must be a <result> element
                            if (!context.Input.Name.Equals("result"))
                            {
                                throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <results> element contains an unexpected element <" + context.Input.Name + ">!");
                            }

                            // Empty Elements generate an Empty Result
                            if (context.Input.IsEmptyElement)
                            {
                                if (!context.Handler.HandleResult(new SparqlResult())) ParserHelper.Stop();
                                continue;
                            }

                            // Get the values of each Binding
                            String var;
                            INode value;
                            SparqlResult result = new SparqlResult();
                            while (context.Input.Read())
                            {
                                // Stop reading when we hit the </binding>
                                if (context.Input.NodeType == XmlNodeType.EndElement && context.Input.Name.Equals("result")) break;

                                // Must be a <binding> element
                                if (!context.Input.Name.Equals("binding"))
                                {
                                    throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <result> element contains an unexpected element <" + context.Input.Name + ">!");
                                }

                                // Must have only 1 attribute
                                if (context.Input.AttributeCount != 1)
                                {
                                    throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <binding> element has too few/many attributes, only a 'name' attribute should be present!");
                                }

                                // Get the Variable this is a binding for and its Value
                                context.Input.MoveToNextAttribute();
                                var = context.Input.Value;
                                if (!context.Input.Read()) throw new RdfParseException("Unable to Parse a SPARQL Result Set as reached the end of input when the contents of a <binding> element was expected");
                                value = ParseValue(context);

                                // Check that the Variable was defined in the Header
                                if (!context.Variables.Contains(var))
                                {
                                    throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <binding> element attempts to bind a value to the variable '" + var + "' which is not defined in the <head> by a <variable> element!");
                                }

                                // Set the Variable to the Value
                                result.SetValue(var, value);
                            }

                            // Check that all Variables are bound for a given result binding nulls where appropriate
                            foreach (String v in context.Variables)
                            {
                                if (!result.HasValue(v))
                                {
                                    result.SetValue(v, null);
                                }
                            }

                            if (!context.Input.Name.Equals("result"))
                            {
                                throw new RdfParseException("Unable to Parse a SPARQL Result Set as reached the end of the input before a closing </result> element was found");
                            }

                            // Add to results set
                            result.SetVariableOrdering(context.Variables);
                            if (!context.Handler.HandleResult(result)) ParserHelper.Stop();
                        }
                    }

                    if (!context.Input.Name.Equals("results"))
                    {
                        throw new RdfParseException("Unable to Parse a SPARQL Result Set as reached the end of the input before the closing </results> element was found");
                    }
                }
                else if (context.Input.Name.Equals("boolean"))
                {
                    // Can't be any <variable> elements
                    if (context.Variables.Count > 0)
                    {
                        throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <boolean> element is specified but the <head> contained one/more <variable> elements which is not permitted!");
                    }

                    try
                    {
                        // Get the value of the <boolean> element as a Boolean
                        Boolean b = Boolean.Parse(context.Input.ReadInnerXml());
                        context.Handler.HandleBooleanResult(b);
                    }
                    catch (Exception)
                    {
                        throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <boolean> element contained a value that could not be understood as a Boolean value!");
                    }
                }
                else
                {
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set since the second Child Node of the <sparql> element is not the required <results> or <boolean> element!");
                }

                context.Handler.EndResults(true);
            }
            catch (RdfParsingTerminatedException)
            {
                context.Handler.EndResults(true);
            }
            catch
            {
                // Some other Error
                context.Handler.EndResults(false);
                throw;
            }
        }

        /// <summary>
        /// Internal Helper method which parses the child element of a &lt;binding&gt; element into an <see cref="INode">INode</see>
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <returns></returns>
        private INode ParseValue(SparqlXmlParserContext context)
        {
            if (context.Input.Name.Equals("uri"))
            {
                return ParserHelper.TryResolveUri(context, context.Input.ReadElementContentAsString());
            }
            else if (context.Input.Name.Equals("literal"))
            {
                if (context.Input.AttributeCount == 0)
                {
                    // Literal with no Data Type/Language Specifier
                    return context.Handler.CreateLiteralNode(HttpUtility.HtmlDecode(context.Input.ReadInnerXml()));
                }
                else if (context.Input.AttributeCount >= 1)
                {
                    String lang = null;
                    Uri dt = null;
                    while (context.Input.MoveToNextAttribute())
                    {
                        if (context.Input.Name.Equals("xml:lang"))
                        {
                            // Language is specified
                            lang = context.Input.Value;
                        }
                        else if (context.Input.Name.Equals("datatype"))
                        {
                            // Data Type is specified
                            dt = ((IUriNode) ParserHelper.TryResolveUri(context, context.Input.Value)).Uri;
                        }
                        else
                        {
                            RaiseWarning("SPARQL Result Set has a <literal> element with an unknown attribute '" + context.Input.Name + "'!");
                        }
                    }

                    if (lang != null && dt != null) throw new RdfParseException("Cannot have both a 'xml:lang' and a 'datatype' attribute on a <literal> element");

                    context.Input.MoveToContent();
                    if (lang != null)
                    {
                        return context.Handler.CreateLiteralNode(context.Input.ReadElementContentAsString(), lang);
                    } 
                    else if (dt != null)
                    {
                        return context.Handler.CreateLiteralNode(context.Input.ReadElementContentAsString(), dt);
                    }
                    else
                    {
                        // Just a plain literal with lots of custom attributes
                        return context.Handler.CreateLiteralNode(HttpUtility.HtmlDecode(context.Input.ReadInnerXml()));
                    }
                }
                else
                {
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <literal> element has too many Attributes, only 1 of 'xml:lang' or 'datatype' may be specified!");
                }
            }
            else if (context.Input.Name.Equals("bnode"))
            {
                String bnodeID = context.Input.ReadElementContentAsString();
                if (bnodeID.StartsWith("_:"))
                {
                    return context.Handler.CreateBlankNode(bnodeID.Substring(2));
                }
                else if (bnodeID.Contains("://"))
                {
                    return context.Handler.CreateBlankNode(bnodeID.Substring(bnodeID.IndexOf("://") + 3));
                }
                else if (bnodeID.Contains(":"))
                {
                    return context.Handler.CreateBlankNode(bnodeID.Substring(bnodeID.LastIndexOf(':') + 1));
                }
                else
                {
                    return context.Handler.CreateBlankNode(bnodeID);
                }
            }
            else if (context.Input.Name.Equals("unbound"))
            {
                // HACK: This is a really ancient feature of the SPARQL Results XML format (from Working Draft in 2005) which we support to ensure compatability with old pre-standardisation SPARQL endpoints (like 3store based ones)
                context.Input.ReadInnerXml();
                return null;
            }
            else
            {
                throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <binding> element contains an unexpected element <" + context.Input.Name + ">!");
            }
        }

        /// <summary>
        /// Helper Method which raises the Warning event when a non-fatal issue with the SPARQL Results being parsed is detected
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            SparqlWarning d = Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event raised when a non-fatal issue with the SPARQL Results being parsed is detected
        /// </summary>
        public event SparqlWarning Warning;

        /// <summary>
        /// Gets the String representation of the Parser which is a description of the syntax it parses
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "SPARQL Results XML";
        }
    }
}
