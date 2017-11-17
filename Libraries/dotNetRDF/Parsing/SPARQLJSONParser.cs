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
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Parser for SPARQL Results JSON Format
    /// </summary>
    public class SparqlJsonParser : ISparqlResultsReader
    {
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
                Parse(input, handler);
            }
            finally
            {
                try
                {
                    input.Close();
                }
// ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                    // No catch actions - just trying to cleanup
                }
            }
        }

        /// <summary>
        /// Loads a Result Set from an Input Stream using a Results Handler
        /// </summary>
        /// <param name="handler">Results Handler to use</param>
        /// <param name="input">Input Stream to read from</param>
        public void Load(ISparqlResultsHandler handler, StreamReader input)
        {
            Load(handler, (TextReader) input);
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
        /// Parser method which parses the Stream as Json
        /// </summary>
        /// <param name="input">Input Stream</param>
        /// <param name="handler">Results Handler</param>
        private void Parse(TextReader input, ISparqlResultsHandler handler)
        {
            ParseResultSetObject(new SparqlJsonParserContext(new CommentIgnoringJsonTextReader(input), handler));
        }

        /// <summary>
        /// Parser method which parses the top level Json Object which represents the overall Result Set
        /// </summary>
        private void ParseResultSetObject(SparqlJsonParserContext context)
        {
            try
            {
                context.Handler.StartResults();

                // Can we read the overall Graph Object
                if (context.Input.Read())
                {
                    if (context.Input.TokenType == JsonToken.StartObject)
                    {
                        // Parse the Header and the Body
                        if (ParseHeader(context))
                        {
                            // For SPARQL Results JSON with an unexpected ordering the act of parsing the header may cause us to parse the body
                            ParseBody(context);

                            // Check we now get the End of the Result Set Object
                            context.Input.Read();
                            if (context.Input.TokenType != JsonToken.EndObject)
                            {
                                throw Error(context, "Unexpected Token '" + context.Input.TokenType + "' with value '" + context.Input.Value + "' encountered, end of the JSON Result Set Object was expected");
                            }
                        }
                    }
                    else
                    {
                        throw Error(context, "Unexpected Token '" + context.Input.TokenType + "' with value '" + context.Input.Value + "' encountered, start of the JSON Result Set Object was expected");
                    }
                }
                else
                {
                    throw new RdfParseException("Unexpected End of Input while trying to parse start of the JSON Result Set Object");
                }

                context.Handler.EndResults(true);
            }
            catch (RdfParsingTerminatedException)
            {
                context.Handler.EndResults(true);
            }
            catch
            {
                context.Handler.EndResults(false);
                throw;
            }
        }

        /// <summary>
        /// Parser method which parses the 'head' property of the top level Json Object which represents the Header of the Result Set
        /// </summary>
        private bool ParseHeader(SparqlJsonParserContext context)
        {
            return ParseHeader(context, false);
        }

        /// <summary>
        /// Parser method which parses the 'head' property of the top level Json Object which represents the Header of the Result Set
        /// </summary>
        private bool ParseHeader(SparqlJsonParserContext context, bool bodySeen)
        {
            // Can we read the Head Property
            if (!context.Input.Read()) throw new RdfParseException("Unexpected End of Input while trying to parse the Head property of the JSON Result Set Object");
            if (context.Input.TokenType != JsonToken.PropertyName) throw Error(context, "Unexpected Token '" + context.Input.TokenType + "' with value '" + context.Input.Value + "' encountered, the 'head'" + (bodySeen ? String.Empty : "'results'/'boolean'") + " property of the JSON Result Set Object was expected");

            // Check the Property Name is head/results/boolean
            String propName = context.Input.Value.ToString();
            switch (propName)
            {
                case "head":
                    ParseHeaderObject(context, bodySeen);
                    return true;
                case "results":
                    // We've seen the results before the header, still expect to see a header afterwards
                    ParseResults(context, false);
                    ParseHeader(context, true);
                    return false;
                case "boolean":
                    // We've seen the boolean result before the header, still expect to see a header afterwards
                    ParseBoolean(context);
                    ParseHeader(context, false);
                    return false;
                default:
                    // TODO Technically we should probably allow and ignore unknown objects
                    throw Error(context, "Unexpected Property Name '" + propName + "' encountered, expected the 'head' property of the JSON Result Set Object");
            }
        }

        /// <summary>
        /// Parser method which parses the Header Object of the Result Set
        /// </summary>
        private void ParseHeaderObject(SparqlJsonParserContext context, bool bodySeen)
        {
            // Can we read the Head Object
            if (context.Input.Read())
            {
                switch (context.Input.TokenType)
                {
                    case JsonToken.Null:
                        break;
                    case JsonToken.StartObject:
                        ParseHeaderProperties(context, bodySeen);
                        if (context.Input.TokenType != JsonToken.EndObject)
                        {
                            throw Error(context, "Unexpected Token '" + context.Input.TokenType + "' with value '" + context.Input.Value + "' encountered, end of the Header Object of the JSON Result Set was expected");
                        }
                        break;
                }
            }
            else
            {
                throw new RdfParseException("Unexpected End of Input while trying to parse the Header Object of the JSON Result Set");
            }
        }

        /// <summary>
        /// Parser method which parses the Properties of the Header Object
        /// </summary>
        private void ParseHeaderProperties(SparqlJsonParserContext context, bool bodySeen)
        {
            bool varsSeen = false;

            // Can we read the Header properties
            if (context.Input.Read())
            {
                while (context.Input.TokenType != JsonToken.EndObject)
                {
                    if (context.Input.TokenType == JsonToken.PropertyName)
                    {
                        // Only expecting Property Name Tokens
                        String propName = context.Input.Value.ToString();

                        if (propName.Equals("vars"))
                        {
                            // Variables property
                            if (varsSeen) throw Error(context, "Unexpected Property Name 'vars' encountered, a 'vars' property has already been seen in the Header Object of the JSON Result Set");
                            varsSeen = true;

                            ParseVariables(context, bodySeen);
                        }
                        else if (propName.Equals("link"))
                        {
                            // Link property
                            ParseLink(context);
                        }
                        else
                        {
                            // Invalid property
                            throw Error(context, "Unexpected Property Name '" + propName + "' encountered, expected a 'link' or 'vars' property of the Header Object of the JSON Result Set");
                        }
                    }
                    else
                    {
                        throw Error(context, "Unexpected Token '" + context.Input.TokenType + "' with value '" + context.Input.Value + "' encountered, expected a Property Name for a property of the Header Object of the JSON Result Set");
                    }

                    // Read next Token
                    context.Input.Read();
                }
            }
            else
            {
                throw new RdfParseException("Unexpected End of Input while trying to parse the properties of the Header Object of the JSON Result Set");
            }
        }

        /// <summary>
        /// Parser method which parses the 'vars' property of the Header Object
        /// </summary>
        private void ParseVariables(SparqlJsonParserContext context, bool bodySeen)
        {
            // Can we read the Variable Array
            if (context.Input.Read())
            {
                if (context.Input.TokenType == JsonToken.StartArray)
                {
                    context.Input.Read();
                    List<String> vars = new List<string>();
                    while (context.Input.TokenType != JsonToken.EndArray)
                    {
                        if (context.Input.TokenType == JsonToken.String)
                        {
                            // Add to Variables
                            if (!context.Handler.HandleVariable(context.Input.Value.ToString())) ParserHelper.Stop();
                            if (bodySeen)
                            {
                                // We've already seen the body in which case store locally for now
                                vars.Add(context.Input.Value.ToString());
                            }
                            else
                            {
                                // We're seeing the header first so just add to list of variables
                                context.Variables.Add(context.Input.Value.ToString());
                            }
                        }
                        else
                        {
                            throw Error(context, "Unexpected Token '" + context.Input.TokenType + "' with value '" + context.Input.Value + "' encountered, expected a String giving the name of a Variable for the Result Set");
                        }
                        context.Input.Read();
                    }

                    // If we've already seen the body check for variable conflicts
                    if (!bodySeen) return;
                    foreach (String var in context.Variables)
                    {
                        if (!vars.Contains(var)) throw new RdfParseException("Unable to Parse a SPARQL Result Set since a Binding Object binds a value to the variable '" + var + "' which is not defined in the Header Object in the value for the 'vars' property!");
                    }
                    foreach (String var in context.Variables)
                    {
                        if (!context.Handler.HandleVariable(var)) ParserHelper.Stop();
                    }
                }
                else
                {
                    throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' with value '" + context.Input.Value + "' encountered, expected the Start of an Array giving the list of Variables for the 'vars' property of the Header Object of the JSON Result Set");
                }
            }
            else
            {
                throw new RdfParseException("Unexpected End of Input while trying to parse the 'vars' property of the Header Object of the JSON Result Set");
            }
        }

        /// <summary>
        /// Parser method which parses the 'link' property of the Header Object
        /// </summary>
        private void ParseLink(SparqlJsonParserContext context)
        {
            // Expect an array - Discard this information as we don't use it
            if (context.Input.Read())
            {
                while (context.Input.TokenType != JsonToken.EndArray)
                {
                    if (!context.Input.Read())
                    {
                        throw new RdfParseException("Unexpected End of Input while trying to parse the 'link' property of the Header Object of the JSON Result Set");
                    }
                }
            }
            else
            {
                throw new RdfParseException("Unexpected End of Input while trying to parse the 'link' property of the Header Object of the JSON Result Set");
            }
        }

        /// <summary>
        /// Parser method which parses the Body of the Result Set which may be either a 'results' or 'boolean' property of the top level Json Object
        /// </summary>
        private void ParseBody(SparqlJsonParserContext context)
        {
            // Can we read the Boolean/Results Property
            if (context.Input.Read())
            {
                if (context.Input.TokenType == JsonToken.PropertyName)
                {
                    // Check the Property Name is head
                    String propName = context.Input.Value.ToString();
                    if (propName.Equals("results"))
                    {
                        ParseResults(context, true);
                    }
                    else if (propName.Equals("boolean"))
                    {
                        ParseBoolean(context);
                    }
                    else
                    {
                        throw Error(context, "Unexpected Property Name '" + propName + "' encountered, expected the 'results' or 'boolean' property of the JSON Result Set Object");
                    }
                }
                else
                {
                    throw Error(context, "Unexpected Token '" + context.Input.TokenType + "' with value '" + context.Input.Value + "' encountered, the 'results' or 'boolean' property of the JSON Result Set Object was expected");
                }
            }
            else
            {
                throw new RdfParseException("Unexpected End of Input while trying to parse the Body of the JSON Result Set Object");
            }
        }

        /// <summary>
        /// Parser method which parses the Results Object of the Result Set
        /// </summary>
        private void ParseResults(SparqlJsonParserContext context, bool headSeen)
        {
            // Can we read the Results Object
            if (context.Input.Read())
            {
                // Should get a Start Object
                if (context.Input.TokenType == JsonToken.StartObject)
                {
                    // Should be a Property Name for 'bindings' next
                    context.Input.Read();
                    if (context.Input.TokenType == JsonToken.PropertyName)
                    {
                        String propName = context.Input.Value.ToString();

                        // Wait till we get the bindings property
                        // There's a couple of deprecated properties we need to support
                        while (!propName.Equals("bindings"))
                        {
                            // Distinct and Ordered properties are no longer in the Specifcation but have to support them for compatability
                            if (propName.Equals("distinct") || propName.Equals("ordered"))
                            {
                                // Should then be a Boolean
                                context.Input.Read();
                                if (context.Input.TokenType != JsonToken.Boolean)
                                {
                                    throw Error(context, "Deprecated Property '" + propName + "' used incorrectly, only a Boolean may be given for this property and this property should no longer be used according to the W3C Specification");
                                }

                                // Hopefully then we get another Property Name else the Results object is invalid
                                context.Input.Read();
                                if (context.Input.TokenType == JsonToken.PropertyName)
                                {
                                    propName = context.Input.Value.ToString();
                                }
                                else
                                {
                                    throw Error(context, "Unexpected Token '" + context.Input.TokenType + "' with value '" + context.Input.Value + "' encountered, expected the 'bindings' property for the Results Object");
                                }
                            }
                            else
                            {
                                // Unexpected Property
                                throw Error(context, "Unexpected Property Name '" + propName + "' encountered, expected the 'bindings' property of the Results Object");
                            }
                        }

                        // Then should get the start of an array
                        context.Input.Read();
                        if (context.Input.TokenType == JsonToken.StartArray)
                        {
                            ParseBindings(context, headSeen);
                        }
                        else
                        {
                            throw Error(context, "Unexpected Token '" + context.Input.TokenType + "' with value '" + context.Input.Value + "' encountered, expected the start of an Array for the 'bindings' property of the Results Object");
                        }
                    }
                    else
                    {
                        throw Error(context, "Unexpected Token '" + context.Input.TokenType + "' with value '" + context.Input.Value + "' encountered, expected the 'bindings' property for the Results Object");
                    }

                    // Skip to the End of the Results Object
                    SkipToEndOfObject(context, true);
                }
                else
                {
                    throw Error(context, "Unexpected Token '" + context.Input.TokenType + "' with value '" + context.Input.Value + "' encountered, expected the start of the Results Object");
                }
            }
            else
            {
                throw new RdfParseException("Unexpected End of Input while trying to parse the 'results' property of the JSON Result Set Object");
            }
        }

        /// <summary>
        /// Parser method which parses the 'bindings' property of the Results Object
        /// </summary>
        private void ParseBindings(SparqlJsonParserContext context, bool headSeen)
        {
            // Can we start reading Objects
            if (context.Input.Read())
            {
                while (context.Input.TokenType != JsonToken.EndArray)
                {
                    if (context.Input.TokenType == JsonToken.StartObject)
                    {
                        ParseBinding(context, headSeen);
                    }
                    else
                    {
                        throw Error(context, "Unexpected Token '" + context.Input.TokenType + "' with value '" + context.Input.Value + "' encountered, expected the start of a Binding Object");
                    }

                    if (!context.Input.Read())
                    {
                        throw new RdfParseException("Unexpected End of Input while trying to parse the Binding Objects");
                    }
                }
            }
            else
            {
                throw new RdfParseException("Unexpected End of Input while trying to parse the Binding Objects");
            }
        }

        /// <summary>
        /// Parser method which parses a Binding Object which occurs in the array of Bindings
        /// </summary>
        private void ParseBinding(SparqlJsonParserContext context, bool headSeen)
        {
            // Can we read some properties
            if (context.Input.Read())
            {
                SparqlResult result = new SparqlResult();
                while (context.Input.TokenType != JsonToken.EndObject)
                {
                    if (context.Input.TokenType == JsonToken.PropertyName)
                    {
                        // Each Property Name should be for a variable
                        ParseBoundVariable(context, context.Input.Value.ToString(), result, headSeen);
                    }
                    else
                    {
                        throw Error(context, "Unexpected Token '" + context.Input.TokenType + "' with value '" + context.Input.Value + "' encountered, expected a Property Name giving the Binding for a Variable for this Result");
                    }

                    // Get Next Token
                    if (!context.Input.Read())
                    {
                        throw new RdfParseException("Unexpected End of Input while trying to parse a Binding Object");
                    }
                }

                // Check that all Variables are bound for a given result binding nulls where appropriate
                foreach (String v in context.Variables)
                {
                    if (!result.HasValue(v))
                    {
                        result.SetValue(v, null);
                    }
                }

                // Add to Results
                result.SetVariableOrdering(context.Variables);
                if (!context.Handler.HandleResult(result)) ParserHelper.Stop();
            }
            else
            {
                throw new RdfParseException("Unexpected End of Input while trying to parse a Binding Object");
            }
        }

        /// <summary>
        /// Parser method which parses a Bound Variable Object which occurs within a Binding Object
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="var">Variable Name</param>
        /// <param name="r">Result Object that is being constructed from the Binding Object</param>
        /// <param name="headSeen"></param>
        private void ParseBoundVariable(SparqlJsonParserContext context, string var, SparqlResult r, bool headSeen)
        {
            String nodeType, nodeLang, nodeDatatype;
            string nodeValue = nodeType = nodeLang = nodeDatatype = null;

            // Can we read the start of an Object
            if (context.Input.Read())
            {
                if (context.Input.TokenType == JsonToken.StartObject)
                {
                    context.Input.Read();

                    while (context.Input.TokenType != JsonToken.EndObject)
                    {
                        String token = context.Input.Value.ToString().ToLower();

                        // Check that we get a Property Value as a String
                        context.Input.Read();
                        if (!IsValidValue(context))
                        {
                            throw Error(context, "Unexpected Token '" + context.Input.TokenType + "' with value '" + context.Input.Value + "' encountered, expected a Property Value describing one of the properties of an Variable Binding");
                        }

                        // Extract the Information from the Object
                        if (token.Equals("value"))
                        {
                            nodeValue = context.Input.Value.ToString();
                        }
                        else if (token.Equals("type"))
                        {
                            nodeType = context.Input.Value.ToString().ToLower();
                        }
                        else if (token.Equals("lang") || token.Equals("xml:lang"))
                        {
                            if (nodeLang == null && nodeDatatype == null)
                            {
                                nodeLang = context.Input.Value.ToString();
                            }
                            else
                            {
                                throw Error(context, "Unexpected Language Property specified for an Object Node where a Language or Datatype has already been specified");
                            }
                        }
                        else if (token.Equals("datatype"))
                        {
                            if (nodeDatatype == null && nodeLang == null)
                            {
                                nodeDatatype = context.Input.Value.ToString();
                            }
                            else
                            {
                                throw Error(context, "Unexpected Datatype Property specified for an Object Node where a Language or Datatype has already been specified");
                            }
                        }
                        else
                        {
                            throw Error(context, "Unexpected Property '" + token + "' specified for an Object Node, only 'value', 'type', 'lang' and 'datatype' are valid properties");
                        }

                        // Get Next Token
                        if (!context.Input.Read())
                        {
                            throw new RdfParseException("Unexpected End of Input while trying to parse a Bound Variable Object");
                        }
                    }

                    // Validate the Information
                    if (nodeType == null)
                    {
                        throw new RdfParseException("Cannot parse a Node from the JSON where no 'type' property was specified in the JSON Object representing the Node");
                    }
                    if (nodeValue == null)
                    {
                        throw new RdfParseException("Cannot parse a Node from the JSON where no 'value' property was specified in the JSON Object representing the Node");
                    }

                    // Turn this information into a Node
                    INode n;
                    if (nodeType.Equals("uri"))
                    {
                        n = ParserHelper.TryResolveUri(context, nodeValue);
                    }
                    else if (nodeType.Equals("bnode"))
                    {
                        if (nodeValue.StartsWith("_:"))
                        {
                            n = context.Handler.CreateBlankNode(nodeValue.Substring(2));
                        }
                        else if (nodeValue.Contains("://"))
                        {
                            n = context.Handler.CreateBlankNode(nodeValue.Substring(nodeValue.IndexOf("://", StringComparison.Ordinal) + 3));
                        }
                        else if (nodeValue.Contains(":"))
                        {
                            n = context.Handler.CreateBlankNode(nodeValue.Substring(nodeValue.LastIndexOf(':') + 1));
                        }
                        else
                        {
                            n = context.Handler.CreateBlankNode(nodeValue);
                        }
                    }
                    else if (nodeType.Equals("literal") || nodeType.Equals("typed-literal"))
                    {
                        if (nodeLang != null)
                        {
                            n = context.Handler.CreateLiteralNode(nodeValue, nodeLang);
                        }
                        else if (nodeDatatype != null)
                        {
                            Uri dtUri = ((IUriNode) ParserHelper.TryResolveUri(context, nodeDatatype)).Uri;
                            n = context.Handler.CreateLiteralNode(nodeValue, dtUri);
                        }
                        else
                        {
                            n = context.Handler.CreateLiteralNode(nodeValue);
                        }
                    }
                    else
                    {
                        throw new RdfParseException("Cannot parse a Node from the JSON where the 'type' property has a value of '" + nodeType + "' which is not one of the permitted values 'uri', 'bnode', 'literal' or 'typed-literal' in the JSON Object representing the Node");
                    }

                    // Check that the Variable was defined in the Header
                    if (!context.Variables.Contains(var))
                    {
                        if (headSeen) throw new RdfParseException("Unable to Parse a SPARQL Result Set since a Binding Object attempts to bind a value to the variable '" + var + "' which is not defined in the Header Object in the value for the 'vars' property!");
                        context.Variables.Add(var);
                    }

                    // Add to the result
                    r.SetValue(var, n);
                }
                else
                {
                    throw Error(context, "Unexpected Token '" + context.Input.TokenType + "' with value '" + context.Input.Value + "' encountered, expected the start of a Bound Variable Object");
                }
            }
            else
            {
                throw Error(context, "Unexpected End of Input while trying to parse a Bound Variable Object");
            }
        }

        /// <summary>
        /// Parser method which parses the 'boolean' property of the Result Set
        /// </summary>
        private static void ParseBoolean(SparqlJsonParserContext context)
        {
            // Expect a Boolean
            if (context.Input.Read())
            {
                if (context.Input.TokenType == JsonToken.Boolean)
                {
                    Boolean result = Boolean.Parse(context.Input.Value.ToString());
                    context.Handler.HandleBooleanResult(result);
                }
                else
                {
                    throw Error(context, "Unexpected Token '" + context.Input.TokenType + "' with value '" + context.Input.Value + "' encountered, expected a Boolean value for the 'boolean' property of the JSON Result Set");
                }
            }
            else
            {
                throw new RdfParseException("Unexpected End of Input while trying to parse the 'boolean' property of the JSON Result Set Object");
            }
        }

        /// <summary>
        /// Checks whether a JSON Token is valid as the value for a RDF term
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns></returns>
        private static bool IsValidValue(SparqlJsonParserContext context)
        {
            switch (context.Input.TokenType)
            {
                case JsonToken.String:
                case JsonToken.Date:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Skips to the end of the current object
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="issueWarning">True if a warning should be issued</param>
        private void SkipToEndOfObject(SparqlJsonParserContext context, bool issueWarning)
        {
            if (issueWarning)
            {
                RaiseWarning("Found extra JSON property " + context.Input.Value + " which will be ignored and discarded");
            }

            int depth = 1;
            while (depth > 0)
            {
                // Try to read next token
                try
                {
                    if (!context.Input.Read()) throw Error(context, "Unexpected EOF while trying to skip to end of JSON object");
                }
                catch (JsonReaderException ex)
                {
                    throw Error(context, ex.Message);
                }

                // Decide how to skip based on next token
                switch (context.Input.TokenType)
                {
                    case JsonToken.Boolean:
                    case JsonToken.Comment:
                    case JsonToken.Bytes:
                    case JsonToken.Date:
                    case JsonToken.Float:
                    case JsonToken.Integer:
                    case JsonToken.Null:
                    case JsonToken.PropertyName:
                    case JsonToken.Raw:
                    case JsonToken.String:
                        // Ignore and continue
                        continue;

                    case JsonToken.StartArray:
                        // Need to separately skip the array
                        SkipToEndOfArray(context, false);
                        break;

                    case JsonToken.EndArray:
                        // Illegal syntax
                        throw Error(context, "Illegal end of array while trying to skip to end of JSON object");

                    case JsonToken.StartObject:
                        // Increment depth to avoid recursion
                        depth++;
                        break;

                    case JsonToken.EndObject:
                        // Decrement depth and exit if we've reached end of object
                        depth--;
                        if (depth == 0) return;
                        break;

                    default:
                        // Anything else is illegal syntax
                        throw Error(context, "Illegal JSON token of type " + context.Input.TokenType + " with value " + context.Input.Value + " while trying to skip to end of JSON object");
                }
            }
        }

        private void SkipToEndOfArray(SparqlJsonParserContext context, bool issueWarning)
        {
            if (issueWarning)
            {
                RaiseWarning("Found extra JSON array which will be ignored and discarded");
            }

            int depth = 1;
            while (depth > 0)
            {
                // Try to read next token
                try
                {
                    if (!context.Input.Read()) throw Error(context, "Unexpected EOF while trying to skip to end of JSON array");
                }
                catch (JsonReaderException ex)
                {
                    throw Error(context, ex.Message);
                }

                // Decide how to skip based on next token
                switch (context.Input.TokenType)
                {
                    case JsonToken.Boolean:
                    case JsonToken.Comment:
                    case JsonToken.Bytes:
                    case JsonToken.Date:
                    case JsonToken.Float:
                    case JsonToken.Integer:
                    case JsonToken.Null:
                    case JsonToken.PropertyName:
                    case JsonToken.Raw:
                    case JsonToken.String:
                        // Ignore and continue
                        continue;

                    case JsonToken.StartArray:
                        // Increment depth to avoid recursion
                        depth++;
                        break;

                    case JsonToken.EndArray:
                        // Decrement depth and exit if we've reached end of object
                        depth--;
                        if (depth == 0) return;
                        break;

                    case JsonToken.StartObject:
                        // Need to separately skip the object
                        SkipToEndOfObject(context, false);
                        break;

                    case JsonToken.EndObject:
                        // Illegal syntax
                        throw Error(context, "Illegal end of object while trying to skip to end of JSON array");

                    default:
                        // Anything else is illegal syntax
                        throw Error(context, "Illegal JSON token of type " + context.Input.TokenType + " with value " + context.Input.Value + " while trying to skip to end of JSON array");
                }
            }
        }

        /// <summary>
        /// Helper method for raising Error messages with attached Line Information
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="message">Error Message</param>
        /// <returns></returns>
        private static RdfParseException Error(SparqlJsonParserContext context, String message)
        {
            StringBuilder error = new StringBuilder();
            if (context.Input.HasLineInfo())
            {
                error.Append("[Line " + context.Input.LineNumber + " Column " + context.Input.LinePosition + "] ");
            }
            error.AppendLine(context.Input.TokenType.ToString());
            error.Append(message);
            if (context.Input.HasLineInfo())
            {
               return new RdfParseException(error.ToString(), context.Input.LineNumber, context.Input.LinePosition);
            }
            return new RdfParseException(error.ToString());
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
            return "SPARQL Results JSON";
        }
    }
}