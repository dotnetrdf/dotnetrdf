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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VDS.RDF.Parsing.Contexts;

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
        /// Read RDF/Json Syntax from some File into a Graph
        /// </summary>
        /// <param name="g">Graph to read into</param>
        /// <param name="filename">File to read from</param>
        public void Load(IGraph g, string filename)
        {
            StreamReader input = new StreamReader(filename);
            this.Load(g, input);
        }

        /// <summary>
        /// Internal top level Parse method which parses the Json
        /// </summary>
        /// <param name="g">Graph to read into</param>
        /// <param name="input">Stream to read from</param>
        private void Parse(IGraph g, StreamReader input)
        {
            //Create Parser Context and parse
            JsonParserContext context = new JsonParserContext(g, new CommentIgnoringJsonTextReader(input));
            this.ParseGraphObject(context);
        }

        /// <summary>
        /// Parser method which parses the top level Json Object which represents the overall Graph
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void ParseGraphObject(JsonParserContext context)
        {
            //Can we read the overall Graph Object
            if (context.Input.Read())
            {
                if (context.Input.TokenType == JsonToken.StartObject)
                {
                    this.ParseTriples(context);

                    //When we get control back we should have already read the last token which should be an End Object
                    if (context.Input.TokenType != JsonToken.EndObject)
                    {
                        throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' encountered, end of the JSON Graph Object was expected");
                    }
                }
                else
                {
                    throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' encountered, start of the JSON Graph Object was expected");
                }
            }
            else
            {
                throw new RdfParseException("Unexpected End of Input while trying to parse start of the JSON Graph Object");
            }
        }

        /// <summary>
        /// Parser method which parses Json Objects representing Triples
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void ParseTriples(JsonParserContext context)
        {
            if (context.Input.Read())
            {
                while (context.Input.TokenType != JsonToken.EndObject)
                {
                    //Expect Property Names for Subjects
                    if (context.Input.TokenType == JsonToken.PropertyName)
                    {
                        String subjValue = context.Input.Value.ToString();
                        INode subjNode;
                        if (subjValue.StartsWith("_:"))
                        {
                            subjNode = context.Graph.CreateBlankNode(subjValue.Substring(subjValue.IndexOf(':') + 1));
                        }
                        else
                        {
                            subjNode = context.Graph.CreateUriNode(new Uri(subjValue));
                        }

                        this.ParsePredicateObjectList(context, subjNode);
                    }

                    context.Input.Read();
                }
            }
            else
            {
                throw new RdfParseException("Unexpected End of Input while trying to parse Triples from the JSON");
            }
        }

        /// <summary>
        /// Parser method which parses Json Objects representing Predicate Object Lists
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="subj">Subject of Triples which comes from the parent Json Object</param>
        private void ParsePredicateObjectList(JsonParserContext context, INode subj)
        {
            if (context.Input.Read())
            {
                if (context.Input.TokenType == JsonToken.StartObject)
                {

                    context.Input.Read();
                    while (context.Input.TokenType != JsonToken.EndObject)
                    {
                        //Expect Property Names for Predicates
                        if (context.Input.TokenType == JsonToken.PropertyName)
                        {
                            String predValue = context.Input.Value.ToString();
                            INode predNode = context.Graph.CreateUriNode(new Uri(predValue));

                            this.ParseObjectList(context, subj, predNode);
                        }
                        else
                        {
                            throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' encountered, expected a Property Name which represents a Predicate");
                        }

                        context.Input.Read();
                    }
                }
                else
                {
                    throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' encountered, expected the start of a JSON Object to represent a Predicate Object List");
                }
            }
            else
            {
                throw new RdfParseException("Unexpected End of Input while trying to parse a Predicate Object List from the JSON");
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
            if (context.Input.Read())
            {
                //Expect an Array for the Object List
                if (context.Input.TokenType == JsonToken.StartArray)
                {
                    while (context.Input.TokenType != JsonToken.EndArray)
                    {
                        //Try to parse an 'Object' Object!!
                        this.ParseObject(context, subj, pred);
                    }
                }
                else
                {
                    throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' encountered, expected the start of a JSON Array to represent an Object List");
                }
            }
            else
            {
                throw new RdfParseException("Unexpected End of Input while trying to parse an Object List from the JSON");
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

                            //Check that we get a Property Value as a String
                            context.Input.Read();
                            if (context.Input.TokenType != JsonToken.String) {
                                throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' encountered, expected a Property Value describing one of the properties of an Object Node");
                            }

                            //Extract the Information from the Object
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
                        }
                        else
                        {
                            throw Error(context, "Unexpected Token '" + context.Input.TokenType.ToString() + "' encountered, expected a Property Name describing one of the properties of an Object Node");
                        }

                        context.Input.Read();
                    }

                    //Validate the Information
                    if (nodeType == null)
                    {
                        throw new RdfParseException("Cannot parse an Object Node from the JSON where no 'type' property was specified in the JSON Object representing the Node");
                    }
                    if (nodeValue == null)
                    {
                        throw new RdfParseException("Cannot parse an Object Node from the JSON where no 'value' property was specified in the JSON Object representing the Node");
                    }

                    //Turn this information into a Node
                    INode obj;
                    if (nodeType.Equals("uri"))
                    {
                        obj = context.Graph.CreateUriNode(new Uri(nodeValue));
                    }
                    else if (nodeType.Equals("bnode"))
                    {
                        obj = context.Graph.CreateBlankNode(nodeValue.Substring(nodeValue.IndexOf(':') + 1));
                    }
                    else if (nodeType.Equals("literal"))
                    {
                        if (nodeLang != null)
                        {
                            obj = context.Graph.CreateLiteralNode(nodeValue, nodeLang);
                        }
                        else if (nodeDatatype != null)
                        {
                            obj = context.Graph.CreateLiteralNode(nodeValue, new Uri(nodeDatatype));
                        }
                        else
                        {
                            obj = context.Graph.CreateLiteralNode(nodeValue);
                        }
                    }
                    else
                    {
                        throw new RdfParseException("Cannot parse an Object Node from the JSON where the 'type' property is not set to one of the permitted values 'uri', 'bnode' or 'literal' in the JSON Object representing the Node");
                    }

                    //Assert as a Triple
                    context.Graph.Assert(new Triple(subj, pred, obj));
                }
            }
            else
            {
                throw new RdfParseException("Unexpected End of Input while trying to parse an Object Node from the JSON");
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
            if (context.Input.HasLineInfo()) {
                error.Append("[Line " + context.Input.LineNumber + " Column " + context.Input.LinePosition + "] ");
            }
            error.AppendLine(context.Input.TokenType.GetType().Name);
            error.Append(message);
            throw new RdfParseException(error.ToString());
        }

        /// <summary>
        /// Event which is raised if there's a non-fatal issue with the RDF/Json Syntax
        /// </summary>
        public event RdfReaderWarning Warning;
    }
}
