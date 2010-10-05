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
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Writing;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Parser for NTriples in JSON Syntax
    /// </summary>
    /// <threadsafety instance="true">Designed to be Thread Safe - should be able to call Load from multiple threads on different Graphs without issue</threadsafety>
    public class JsonNTriplesParser : IRdfReader 
    {
        /// <summary>
        /// Read NTriples in JSON Syntax from some Stream into a Graph
        /// </summary>
        /// <param name="g">Graph to read into</param>
        /// <param name="input">Stream to read from</param>
        public void Load(IGraph g, StreamReader input)
        {
            try
            {
                if (g.IsEmpty)
                {
                    this.Parse(g, input);
                }
                else
                {
                    Graph h = new Graph();
                    this.Parse(h, input);
                    g.Merge(h);
                }
                input.Close();
            }
            catch
            {
                try
                {
                    input.Close();
                }
                catch
                {
                    //No Catch actions
                }
                throw;
            }
        }

        /// <summary>
        /// Read NTriples in JSON Syntax from some File into a Graph
        /// </summary>
        /// <param name="g">Graph to read into</param>
        /// <param name="filename">File to read from</param>
        public void Load(IGraph g, string filename)
        {
            StreamReader input = new StreamReader(filename);
            this.Load(g, input);
        }

        /// <summary>
        /// Internal top level Parse method which parses the JSON
        /// </summary>
        /// <param name="g">Graph to read into</param>
        /// <param name="input">Stream to read from</param>
        private void Parse(IGraph g, StreamReader input)
        {
            //Create Parser Context and parse
            JsonParserContext context = new JsonParserContext(g, new CommentIgnoringJsonTextReader(input));
            this.ParseTriplesArray(context);
        }

        /// <summary>
        /// Parser method which parses the top level Json Object which represents the overall Graph
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void ParseTriplesArray(JsonParserContext context)
        {
            //Can we read the overall Graph Object
            PositionInfo startPos = context.CurrentPosition;
            if (context.Input.Read())
            {
                if (context.Input.TokenType == JsonToken.StartArray)
                {
                    if (!context.Input.Read()) throw Error(context, "Unexpected End of Input encountered, expected the start of a Triple Object/end of the Triples array");
                    
                    if (context.Input.TokenType == JsonToken.StartObject) this.ParseTriples(context);

                    //Should see an End Array when we get back here
                    if (context.Input.TokenType != JsonToken.EndArray) throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' encountered, end of the JSON Array was expected");
                }
                else
                {
                    throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' encountered, start of the JSON Array was expected", startPos);
                }
            }
            else
            {
                throw Error(context, "Unexpected End of Input while trying to parse start of the JSON Triple Array", startPos);
            }
        }

        /// <summary>
        /// Parser method which parses Json Objects representing Triples
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void ParseTriples(JsonParserContext context)
        {
            PositionInfo startPos = context.CurrentPosition;
            do
            {
                if (context.Input.TokenType == JsonToken.StartObject)
                {
                    INode s, p, o;
                    s = p = o = null;
                    INode temp;
                    TripleSegment segment;

                    //Expect 3 Nodes in a Triple
                    for (int i = 0; i < 3; i++)
                    {
                        temp = this.TryParseNode(context, out segment);
                        switch (segment)
                        {
                            case TripleSegment.Object:
                                if (o == null)
                                {
                                    o = temp;
                                }
                                else
                                {
                                    throw Error(context, "Duplicate object property encountered");
                                }
                                break;
                            case TripleSegment.Predicate:
                                if (p == null)
                                {
                                    p = temp;
                                }
                                else
                                {
                                    throw Error(context, "Duplicate predicate property encountered");
                                }
                                break;
                            case TripleSegment.Subject:
                                if (s == null)
                                {
                                    s = temp;
                                }
                                else
                                {
                                    throw Error(context, "Duplicate Subject property encountered");
                                }
                                break;
                        }
                    }

                    context.Graph.Assert(new Triple(s, p, o));
                }
                else
                {
                    throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' encountered, start of a JSON Object for a Triple was expected");
                }

                //Then expect the end of the Object
                if (context.Input.Read())
                {
                    if (context.Input.TokenType != JsonToken.EndObject) throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + " encountered, expected the end of the JSON Object for a Triple");
                }
                else
                {
                    throw Error(context, "Unexpected End of Input while trying to parse Triples from JSON, end of a Triple Object was expected", startPos);
                }

                //Then expect an End Array/Start Object
                if (!context.Input.Read()) throw Error(context, "Unexpected End of Input while trying to parse Triples from JSON, end of JSON array or start of a JSON Object was expected", startPos);
            } while (context.Input.TokenType == JsonToken.StartObject);
        }

        private INode TryParseNode(JsonParserContext context, out TripleSegment segment)
        {
            if (context.Input.Read())
            {
                if (context.Input.TokenType == JsonToken.PropertyName)
                {
                    //Determine the Triple Segment
                    switch (context.Input.Value.ToString())
                    {
                        case "subject":
                            segment = TripleSegment.Subject;
                            break;
                        case "predicate":
                            segment = TripleSegment.Predicate;
                            break;
                        case "object":
                            segment = TripleSegment.Object;
                            break;
                        default:
                            throw Error(context, "Unexpected Property '" + context.Input.Value.ToString() + "' encountered, expected one of 'subject', 'predicate' or 'object'");
                    }

                    if (context.Input.Read())
                    {
                        String value = context.Input.Value.ToString();
                        return this.TryParseNodeValue(context, value);
                    }
                    else
                    {
                        throw Error(context, "Unexpected End of Input when a Value for a Node of a Triple was expected");
                    }
                }
                else
                {
                    throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' encountered, expected a Property Name for the node of a Triple");
                }
            }
            else
            {
                throw Error(context, "Unexpected End of Input when a Property Value pair for a Node of a Triple was expected");
            }
        }

        private INode TryParseNodeValue(JsonParserContext context, String value)
        {
            try
            {
                if (value.StartsWith("_:"))
                {
                    return context.Graph.CreateBlankNode(value);
                }
                else if (value.StartsWith("<"))
                {
                    return context.Graph.CreateUriNode(new Uri(this.UnescapeValue(value.Substring(1, value.Length - 2))));
                }
                else
                {
                    if (value.EndsWith("\""))
                    {
                        return context.Graph.CreateLiteralNode(this.UnescapeValue(value.Substring(1, value.Length - 2)));
                    }
                    else if (value.EndsWith(">"))
                    {
                        String lit = value.Substring(1, value.LastIndexOf("^^<") - 2);
                        String dt = value.Substring(lit.Length + 5, value.Length - lit.Length - 6);
                        return context.Graph.CreateLiteralNode(this.UnescapeValue(lit), new Uri(this.UnescapeValue(dt)));
                    }
                    else
                    {
                        String lit = value.Substring(1, value.LastIndexOf("\"@") - 1);
                        String lang = value.Substring(lit.Length + 3);
                        return context.Graph.CreateLiteralNode(this.UnescapeValue(lit), this.UnescapeValue(lang));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new RdfParseException("Failed to parse the value '" + value + "' into a valid Node: " + ex.Message, ex);
            }
        }

        private String UnescapeValue(String value)
        {
            String output = value.Replace("\\\\", "\\");
            output = output.Replace("\\n", "\n");
            output = output.Replace("\\r", "\r");
            output = output.Replace("\\t", "\t");
            output = output.Replace("\\\"", "\"");
            if (Regex.IsMatch(output, @"\\u[a-fA-F0-9]{4}"))
            {
                foreach (Match m in Regex.Matches(output, @"\\u([a-fA-F0-9]{4})"))
                {
                    char c = (char)Convert.ToInt32(m.Groups[1].Value, 16);
                    output = output.Replace(m.Value, c.ToString());
                }
            }
            return output;
            //return value.Replace("\\\\\\", "\\");
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
        /// Event which is raised if there's a non-fatal issue with the RDF/Json Syntax
        /// </summary>
        public event RdfReaderWarning Warning;
    }
}
