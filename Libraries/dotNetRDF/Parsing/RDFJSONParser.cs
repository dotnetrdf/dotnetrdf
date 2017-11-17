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
using System.Text;
using Newtonsoft.Json;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Parser for RDF/JSON Syntax
    /// </summary>
    /// <threadsafety instance="true">Designed to be Thread Safe - should be able to call Load from multiple threads on different Graphs without issue</threadsafety>
    public class RdfJsonParser : IRdfReader 
    {
        /// <summary>
        /// Read RDF/JSON Syntax from some Stream into a Graph
        /// </summary>
        /// <param name="g">Graph to read into</param>
        /// <param name="input">Stream to read from</param>
        public void Load(IGraph g, StreamReader input)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");

            Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Read RDF/JSON Syntax from some Input into a Graph
        /// </summary>
        /// <param name="g">Graph to read into</param>
        /// <param name="input">Input to read from</param>
        public void Load(IGraph g, TextReader input)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");

            Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Read RDF/Json Syntax from some File into a Graph
        /// </summary>
        /// <param name="g">Graph to read into</param>
        /// <param name="filename">File to read from</param>
        public void Load(IGraph g, string filename)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");
            Load(new GraphHandler(g), filename);
        }

        /// <summary>
        /// Read RDF/JSON Syntax from some Stream using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">Stream to read from</param>
        public void Load(IRdfHandler handler, StreamReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannot read RDF from a null Stream");

            // Issue a Warning if the Encoding of the Stream is not UTF-8
            if (!input.CurrentEncoding.Equals(Encoding.UTF8))
            {
                RaiseWarning("Expected Input Stream to be encoded as UTF-8 but got a Stream encoded as " + input.CurrentEncoding.EncodingName + " - Please be aware that parsing errors may occur as a result");
            }

            Load(handler, (TextReader)input);
        }

        /// <summary>
        /// Read RDF/JSON Syntax from some Input using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">Input to read from</param>
        public void Load(IRdfHandler handler, TextReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannot read RDF from a null Stream");

            try
            {
                Parse(handler, input);
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
                    // Catch is just here in case something goes wrong with closing the stream
                    // This error can be ignored
                }
            }
        }

        /// <summary>
        /// Read RDF/JSON Syntax from a file using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="filename">File to read from</param>
        public void Load(IRdfHandler handler, String filename)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");
            Load(handler, new StreamReader(File.OpenRead(filename), Encoding.UTF8));
        }

        /// <summary>
        /// Internal top level Parse method which parses the Json
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">Stream to read from</param>
        private void Parse(IRdfHandler handler, TextReader input)
        {
            JsonParserContext context = new JsonParserContext(handler, new CommentIgnoringJsonTextReader(input));

            try
            {
                context.Handler.StartRdf();
                ParseGraphObject(context);
                context.Handler.EndRdf(true);
            }
            catch (RdfParsingTerminatedException)
            {
                context.Handler.EndRdf(true);
                // Discard this - it justs means the Handler told us to stop
            }
            catch
            {
                context.Handler.EndRdf(false);
                throw;
            }
        }

        /// <summary>
        /// Parser method which parses the top level Json Object which represents the overall Graph
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void ParseGraphObject(JsonParserContext context)
        {
            // Can we read the overall Graph Object
            PositionInfo startPos = context.CurrentPosition;
            if (context.Input.Read())
            {
                if (context.Input.TokenType == JsonToken.StartObject)
                {
                    ParseTriples(context);

                    // When we get control back we should have already read the last token which should be an End Object
                    // We ignore any content which is beyond the end of the initial object
                    if (context.Input.TokenType != JsonToken.EndObject)
                    {
                        throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' encountered, end of the JSON Graph Object was expected", startPos);
                    }
                }
                else
                {
                    throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' encountered, start of the JSON Graph Object was expected", startPos);
                }
            }
            else
            {
                throw Error(context, "Unexpected End of Input while trying to parse start of the JSON Graph Object", startPos);
            }
        }

        /// <summary>
        /// Parser method which parses Json Objects representing Triples
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void ParseTriples(JsonParserContext context)
        {
            PositionInfo startPos = context.CurrentPosition;
            if (context.Input.Read())
            {
                while (context.Input.TokenType != JsonToken.EndObject)
                {
                    // Expect Property Names for Subjects
                    if (context.Input.TokenType == JsonToken.PropertyName)
                    {
                        String subjValue = context.Input.Value.ToString();
                        INode subjNode;
                        if (subjValue.StartsWith("_:"))
                        {
                            subjNode = context.Handler.CreateBlankNode(subjValue.Substring(subjValue.IndexOf(':') + 1));
                        }
                        else
                        {
                            subjNode = context.Handler.CreateUriNode(UriFactory.Create(subjValue));
                        }

                        ParsePredicateObjectList(context, subjNode);
                    }
                    else
                    {
                        throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' encountered, expected a JSON Property Name to represent the Subject of a Triple", startPos);
                    }
                    context.Input.Read();
                }
            }
            else
            {
                throw Error(context, "Unexpected End of Input while trying to parse Triples from the JSON", startPos);
            }
        }

        /// <summary>
        /// Parser method which parses Json Objects representing Predicate Object Lists
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="subj">Subject of Triples which comes from the parent Json Object</param>
        private void ParsePredicateObjectList(JsonParserContext context, INode subj)
        {
            PositionInfo startPos = context.CurrentPosition;

            if (context.Input.Read())
            {
                if (context.Input.TokenType == JsonToken.StartObject)
                {
                    context.Input.Read();
                    while (context.Input.TokenType != JsonToken.EndObject)
                    {
                        // Expect Property Names for Predicates
                        if (context.Input.TokenType == JsonToken.PropertyName)
                        {
                            String predValue = context.Input.Value.ToString();
                            INode predNode = context.Handler.CreateUriNode(UriFactory.Create(predValue));

                            ParseObjectList(context, subj, predNode);
                        }
                        else
                        {
                            throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' encountered, expected a Property Name which represents a Predicate", startPos);
                        }

                        context.Input.Read();
                    }
                }
                else
                {
                    throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' encountered, expected the start of a JSON Object to represent a Predicate Object List", startPos);
                }
            }
            else
            {
                throw Error(context, "Unexpected End of Input while trying to parse a Predicate Object List from the JSON", startPos);
            }
        }

        /// <summary>
        /// Parser method which parses Json Arrays representing Object Lists
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="subj">Subject of Triples which comes from the Grandparent Json Object</param>
        /// <param name="pred">Predicate of Triples which comes form the Parent Json Object</param>
        private void ParseObjectList(JsonParserContext context, INode subj, INode pred)
        {
            PositionInfo startPos = context.CurrentPosition;

            if (context.Input.Read())
            {
                // Expect an Array for the Object List
                if (context.Input.TokenType == JsonToken.StartArray)
                {
                    while (context.Input.TokenType != JsonToken.EndArray)
                    {
                        // Try to parse an 'Object' Object!!
                        ParseObject(context, subj, pred);
                    }
                }
                else
                {
                    throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' encountered, expected the start of a JSON Array to represent an Object List", startPos);
                }
            }
            else
            {
                throw Error(context, "Unexpected End of Input while trying to parse an Object List from the JSON", startPos);
            }
        }

        /// <summary>
        /// Parser method which parses Json Objects reprsenting Object Nodes
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="subj">Subject of Triples which comes from the Great-Grandparent Json Object</param>
        /// <param name="pred">Predicate of Triples which comes form the Grandparent Json Object</param>
        private void ParseObject(JsonParserContext context, INode subj, INode pred)
        {
            String token, nodeValue, nodeType, nodeLang, nodeDatatype;
            nodeValue = nodeType = nodeLang = nodeDatatype = null;

            PositionInfo startPos = context.CurrentPosition;

            if (context.Input.Read())
            {
                if (context.Input.TokenType == JsonToken.StartObject)
                {
                    context.Input.Read();
                    while (context.Input.TokenType != JsonToken.EndObject)
                    {
                        if (context.Input.TokenType == JsonToken.PropertyName)
                        {
                            token = context.Input.Value.ToString().ToLower();

                            // Check that we get a Property Value as a String
                            context.Input.Read();
                            if (context.Input.TokenType != JsonToken.String)
                            {
                                throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' encountered, expected a Property Value describing one of the properties of an Object Node", startPos);
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
                                    throw Error(context, "Unexpected Language Property specified for an Object Node where a Language or Datatype has already been specified", startPos);
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
                                    throw Error(context, "Unexpected Datatype Property specified for an Object Node where a Language or Datatype has already been specified", startPos);
                                }
                            }
                            else
                            {
                                throw Error(context, "Unexpected Property '" + token + "' specified for an Object Node, only 'value', 'type', 'lang' and 'datatype' are valid properties", startPos);
                            }
                        }
                        else
                        {
                            throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' encountered, expected a Property Name describing one of the properties of an Object Node", startPos);
                        }

                        context.Input.Read();
                    }

                    // Validate the Information
                    if (nodeType == null)
                    {
                        throw Error(context, "Cannot parse an Object Node from the JSON where no 'type' property was specified in the JSON Object representing the Node", startPos);
                    }
                    if (nodeValue == null)
                    {
                        throw Error(context, "Cannot parse an Object Node from the JSON where no 'value' property was specified in the JSON Object representing the Node", startPos);
                    }

                    // Turn this information into a Node
                    INode obj;
                    if (nodeType.Equals("uri"))
                    {
                        obj = context.Handler.CreateUriNode(UriFactory.Create(nodeValue));
                    }
                    else if (nodeType.Equals("bnode"))
                    {
                        obj = context.Handler.CreateBlankNode(nodeValue.Substring(nodeValue.IndexOf(':') + 1));
                    }
                    else if (nodeType.Equals("literal"))
                    {
                        if (nodeLang != null)
                        {
                            obj = context.Handler.CreateLiteralNode(nodeValue, nodeLang);
                        }
                        else if (nodeDatatype != null)
                        {
                            obj = context.Handler.CreateLiteralNode(nodeValue, UriFactory.Create(nodeDatatype));
                        }
                        else
                        {
                            obj = context.Handler.CreateLiteralNode(nodeValue);
                        }
                    }
                    else
                    {
                        throw Error(context, "Cannot parse an Object Node from the JSON where the 'type' property is not set to one of the permitted values 'uri', 'bnode' or 'literal' in the JSON Object representing the Node", startPos);
                    }

                    // Assert as a Triple
                    if (!context.Handler.HandleTriple(new Triple(subj, pred, obj))) ParserHelper.Stop();
                }
            }
            else
            {
                throw Error(context, "Unexpected End of Input while trying to parse an Object Node from the JSON", startPos);
            }
        }

        /// <summary>
        /// Helper method for raising Error messages with attached Line Information
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="message">Error Message</param>
        /// <returns></returns>
        private RdfParseException Error(JsonParserContext context, String message)
        {
            StringBuilder error = new StringBuilder();
            if (context.Input.HasLineInfo()) 
            {
                error.Append("[Line " + context.Input.LineNumber + " Column " + context.Input.LinePosition + "] ");
            }
            error.AppendLine(context.Input.TokenType.GetType().Name);
            error.Append(message);
            throw new RdfParseException(error.ToString(), context.Input.LineNumber, context.Input.LinePosition);
        }

        /// <summary>
        /// Helper method for raising Error messages with attached Position Information
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="message">Error Message</param>
        /// <param name="startPos">Start Position</param>
        /// <returns></returns>
        private RdfParseException Error(JsonParserContext context, String message, PositionInfo startPos)
        {
            PositionInfo info = context.GetPositionRange(startPos);
            StringBuilder error = new StringBuilder();
            error.Append("[Line " + info.StartLine + " Column " + info.StartPosition + " to Line " + info.EndLine + " Column " + info.EndPosition + "] ");
            error.AppendLine(message);
            throw new RdfParseException(error.ToString(), info);
        }

        /// <summary>
        /// Helper Method for raising the <see cref="RdfJsonParser.Warning">Warning</see> event
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            RdfReaderWarning d = Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event which is raised if there's a non-fatal issue with the RDF/Json Syntax
        /// </summary>
        public event RdfReaderWarning Warning;

        /// <summary>
        /// Gets the String representation of the Parser which is a description of the syntax it parses
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "RDF/JSON (Talis Specification)";
        }
    }
}
