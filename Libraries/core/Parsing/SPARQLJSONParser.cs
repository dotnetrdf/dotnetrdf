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
using VDS.RDF.Query;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Parser for SPARQL Results JSON Format
    /// </summary>
    public class SparqlJsonParser : ISparqlResultsReader
    {
        private IGraph _g;

        /// <summary>
        /// Creates a new SPARQL Results JSON Parser
        /// </summary>
        public SparqlJsonParser()
        {
            //Initialise the Graph
#if !NO_RWLOCK
            this._g = new ThreadSafeGraph();
#else
            this._g = new Graph();
#endif
            Uri sparql = new Uri(SparqlSpecsHelper.SparqlNamespace);
            this._g.BaseUri = sparql;
            this._g.NamespaceMap.AddNamespace("", sparql);
        }

        /// <summary>
        /// Loads a Result Set from an Input Stream
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="input">Input Stream to read from</param>
        public void Load(SparqlResultSet results, StreamReader input)
        {
            if (results == null) throw new RdfParseException("Cannot read SPARQL Results into a null Result Set");
            if (input == null) throw new RdfParseException("Cannot read SPARQL Results from a null Stream");

            //Ensure Empty Result Set
            if (!results.IsEmpty)
            {
                throw new RdfParseException("Cannot load a Result Set from a Stream into a non-empty Result Set");
            }

            try
            {
                this.Parse(input, results);
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
                    //No catch actions - just trying to cleanup
                }
            }
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

            StreamReader input = new StreamReader(filename);
            this.Load(results, input);
        }

        /// <summary>
        /// Parser method which parses the Stream as Json
        /// </summary>
        /// <param name="input">Input Stream</param>
        /// <param name="results">Result Set to parse into</param>
        private void Parse(StreamReader input, SparqlResultSet results)
        {
            this.ParseResultSetObject(new CommentIgnoringJsonTextReader(input), results);
        }

        /// <summary>
        /// Parser method which parses the top level Json Object which represents the overall Result Set
        /// </summary>
        private void ParseResultSetObject(JsonTextReader reader, SparqlResultSet results)
        {
            //Can we read the overall Graph Object
            if (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    //Parse the Header and the Body
                    this.ParseHeader(reader, results);
                    this.ParseBody(reader, results);

                    //Check we now get the End of the Result Set Object
                    reader.Read();
                    if (reader.TokenType != JsonToken.EndObject)
                    {
                        throw Error(reader, "Unexpected Token '" + reader.TokenType.ToString() + "' encountered, end of the JSON Result Set Object was expected");
                    }
                }
                else
                {
                    throw Error(reader, "Unexpected Token '" + reader.TokenType.ToString() + "' encountered, start of the JSON Result Set Object was expected");
                }
            }
            else
            {
                throw new RdfParseException("Unexpected End of Input while trying to parse start of the JSON Result Set Object");
            }
        }

        /// <summary>
        /// Parser method which parses the 'head' property of the top level Json Object which represents the Header of the Result Set
        /// </summary>
        private void ParseHeader(JsonTextReader reader, SparqlResultSet results)
        {
            //Can we read the Head Property
            if (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    //Check the Property Name is head
                    String propName = reader.Value.ToString();
                    if (!propName.Equals("head"))
                    {
                        throw Error(reader, "Unexpected Property Name '" + propName + "' encountered, expected the 'head' property of the JSON Result Set Object");
                    }

                    this.ParseHeaderObject(reader, results);
                }
                else
                {
                    throw Error(reader, "Unexpected Token '" + reader.TokenType.ToString() + "' encountered, the 'head' property of the JSON Result Set Object was expected");
                }
            }
            else
            {
                throw new RdfParseException("Unexpected End of Input while trying to parse the Head property of the JSON Result Set Object");
            }
        }

        /// <summary>
        /// Parser method which parses the Header Object of the Result Set
        /// </summary>
        private void ParseHeaderObject(JsonTextReader reader, SparqlResultSet results)
        {
            //Can we read the Head Object
            if (reader.Read())
            {
                if (reader.TokenType == JsonToken.Null)
                {
                    //Null Header so this must be a Boolean Result Set
                }
                else if (reader.TokenType == JsonToken.StartObject)
                {
                    //Header Object
                    this.ParseHeaderProperties(reader, results);

                    //When we get control back we should have already read the last token which should be an End Object
                    if (reader.TokenType != JsonToken.EndObject)
                    {
                        throw Error(reader, "Unexpected Token '" + reader.TokenType.ToString() + "' encountered, end of the Header Object of the JSON Result Set was expected");
                    }
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
        private void ParseHeaderProperties(JsonTextReader reader, SparqlResultSet results)
        {
            bool varsSeen = false;

            //Can we read the Header properties
            if (reader.Read())
            {
                while (reader.TokenType != JsonToken.EndObject)
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        //Only expecting Property Name Tokens
                        String propName = reader.Value.ToString();

                        if (propName.Equals("vars"))
                        {
                            //Variables property
                            if (varsSeen) throw Error(reader, "Unexpected Property Name 'vars' encountered, a 'vars' property has already been seen in the Header Object of the JSON Result Set");
                            varsSeen = true;

                            this.ParseVariables(reader, results);
                        }
                        else if (propName.Equals("link"))
                        {
                            //Link property
                            this.ParseLink(reader, results);
                        }
                        else
                        {
                            //Invalid property
                            throw Error(reader, "Unexpected Property Name '" + propName + "' encountered, expected a 'link' or 'vars' property of the Header Object of the JSON Result Set");
                        }
                    }
                    else
                    {
                        throw Error(reader, "Unexpected Token '" + reader.TokenType.ToString() + "' encountered, expected a Property Name for a property of the Header Object of the JSON Result Set");
                    }

                    //Read next Token
                    reader.Read();
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
        private void ParseVariables(JsonTextReader reader, SparqlResultSet results)
        {
            //Can we read the Variable Array
            if (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartArray)
                {
                    reader.Read();
                    while (reader.TokenType != JsonToken.EndArray)
                    {
                        if (reader.TokenType == JsonToken.String)
                        {
                            //Add to Variables
                            results.AddVariable(reader.Value.ToString());
                        }
                        else
                        {
                            throw Error(reader, "Unexpected Token '" + reader.TokenType.ToString() + "' encountered, expected a String giving the name of a Variable for the Result Set");
                        }
                        reader.Read();
                    }
                }
                else
                {
                    throw Error(reader, "Unexpected Token '" + reader.TokenType.ToString() + "' encountered, expected the Start of an Array giving the list of Variables for the 'vars' property of the Header Object of the JSON Result Set");
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
        private void ParseLink(JsonTextReader reader, SparqlResultSet results)
        {
            //Expect an array - Discard this information as we don't use it
            if (reader.Read())
            {
                while (reader.TokenType != JsonToken.EndArray)
                {
                    if (!reader.Read())
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
        private void ParseBody(JsonTextReader reader, SparqlResultSet results)
        {
            //Can we read the Boolean/Results Property
            if (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    //Check the Property Name is head
                    String propName = reader.Value.ToString();
                    if (propName.Equals("results"))
                    {
                        this.ParseResults(reader, results);
                    }
                    else if (propName.Equals("boolean"))
                    {
                        this.ParseBoolean(reader, results);
                    }
                    else
                    {
                        throw Error(reader, "Unexpected Property Name '" + propName + "' encountered, expected the 'results' or 'boolean' property of the JSON Result Set Object");
                    }
                }
                else
                {
                    throw Error(reader, "Unexpected Token '" + reader.TokenType.ToString() + "' encountered, the 'results' or 'boolean' property of the JSON Result Set Object was expected");
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
        private void ParseResults(JsonTextReader reader, SparqlResultSet results)
        {
            //Can we read the Results Object
            if (reader.Read())
            {
                //Should get a Start Object
                if (reader.TokenType == JsonToken.StartObject)
                {
                    //Should be a Property Name for 'bindings' next
                    reader.Read();
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        String propName = reader.Value.ToString();

                        //Wait till we get the bindings property
                        //There's a couple of deprecated properties we need to support
                        while (!propName.Equals("bindings"))
                        {
                            //Distinct and Ordered properties are no longer in the Specifcation but have to support them for compatability
                            if (propName.Equals("distinct") || propName.Equals("ordered"))
                            {
                                //Should then be a Boolean
                                reader.Read();
                                if (reader.TokenType != JsonToken.Boolean)
                                {
                                    throw Error(reader, "Deprecated Property '" + propName + "' used incorrectly, only a Boolean may be given for this property and this property should no longer be used according to the W3C Specification");
                                }

                                //Hopefully then we get another Property Name else the Results object is invalid
                                reader.Read();
                                if (reader.TokenType == JsonToken.PropertyName)
                                {
                                    propName = reader.Value.ToString();
                                }
                                else
                                {
                                    throw Error(reader, "Unexpected Token '" + reader.TokenType.ToString() + "' encountered, expected the 'bindings' property for the Results Object");
                                }
                            }
                            else
                            {
                                //Unexpected Property
                                throw Error(reader, "Unexpected Property Name '" + propName + "' encountered, expected the 'bindings' property of the Results Object");
                            }
                        }

                        //Then should get the start of an array
                        reader.Read();
                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            this.ParseBindings(reader, results);
                        }
                        else
                        {
                            throw Error(reader, "Unexpected Token '" + reader.TokenType.ToString() + "' encountered, expected the start of an Array for the 'bindings' property of the Results Object");
                        }
                    }
                    else
                    {
                        throw Error(reader,"Unexpected Token '" + reader.TokenType.ToString() + "' encountered, expected the 'bindings' property for the Results Object");
                    }

                    //Expect the End of the Results Object
                    reader.Read();
                    if (reader.TokenType != JsonToken.EndObject)
                    {
                        throw Error(reader, "Unexpected Token '" + reader.TokenType.ToString() + "' encountered, expected the end of the Results Object");
                    }
                }
                else
                {
                    throw Error(reader, "Unexpected Token '" + reader.TokenType.ToString() + "' encountered, expected the start of the Results Object");
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
        private void ParseBindings(JsonTextReader reader, SparqlResultSet results)
        {
            //Can we start reading Objects
            if (reader.Read())
            {
                while (reader.TokenType != JsonToken.EndArray)
                {
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        this.ParseBinding(reader, results);
                    }
                    else
                    {
                        throw Error(reader, "Unexpected Token '" + reader.TokenType.ToString() + "' encountered, expected the start of a Binding Object");
                    }

                    if (!reader.Read())
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
        private void ParseBinding(JsonTextReader reader, SparqlResultSet results)
        {
            //Can we read some properties
            if (reader.Read())
            {
                SparqlResult result = new SparqlResult();
                while (reader.TokenType != JsonToken.EndObject)
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        //Each Property Name should be for a variable
                        this.ParseBoundVariable(reader, reader.Value.ToString(), result);
                    }
                    else
                    {
                        throw Error(reader, "Unexpected Token '" + reader.TokenType.ToString() + "' encountered, expected a Property Name giving the Binding for a Variable for this Result");
                    }

                    //Get Next Token
                    if (!reader.Read())
                    {
                        throw new RdfParseException("Unexpected End of Input while trying to parse a Binding Object");
                    }
                }

                //Add to Results
                results.AddResult(result);
            }
            else
            {
                throw new RdfParseException("Unexpected End of Input while trying to parse a Binding Object");
            }
        }

        /// <summary>
        /// Parser method which parses a Bound Variable Object which occurs within a Binding Object
        /// </summary>
        /// <param name="reader">Json Text Reader</param>
        /// <param name="var">Variable Name</param>
        /// <param name="r">Result Object that is being constructed from the Binding Object</param>
        private void ParseBoundVariable(JsonTextReader reader, String var, SparqlResult r)
        {
            String token, nodeValue, nodeType, nodeLang, nodeDatatype;
            nodeValue = nodeType = nodeLang = nodeDatatype = null;

            //Can we read the start of an Object
            if (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    reader.Read();

                    while (reader.TokenType != JsonToken.EndObject)
                    {
                        token = reader.Value.ToString().ToLower();

                        //Check that we get a Property Value as a String
                        reader.Read();
                        if (reader.TokenType != JsonToken.String)
                        {
                            throw Error(reader, "Unexpected Token '" + reader.TokenType.ToString() + "' encountered, expected a Property Value describing one of the properties of an Variable Binding");
                        }

                        //Extract the Information from the Object
                        if (token.Equals("value"))
                        {
                            nodeValue = reader.Value.ToString();
                        }
                        else if (token.Equals("type"))
                        {
                            nodeType = reader.Value.ToString().ToLower();
                        }
                        else if (token.Equals("lang") || token.Equals("xml:lang"))
                        {
                            if (nodeLang == null && nodeDatatype == null)
                            {
                                nodeLang = reader.Value.ToString();
                            }
                            else
                            {
                                throw Error(reader, "Unexpected Language Property specified for an Object Node where a Language or Datatype has already been specified");
                            }
                        }
                        else if (token.Equals("datatype"))
                        {
                            if (nodeDatatype == null && nodeLang == null)
                            {
                                nodeDatatype = reader.Value.ToString();
                            }
                            else
                            {
                                throw Error(reader, "Unexpected Datatype Property specified for an Object Node where a Language or Datatype has already been specified");
                            }
                        }
                        else
                        {
                            throw Error(reader, "Unexpected Property '" + token + "' specified for an Object Node, only 'value', 'type', 'lang' and 'datatype' are valid properties");
                        }

                        //Get Next Token
                        if (!reader.Read())
                        {
                            throw new RdfParseException("Unexpected End of Input while trying to parse a Bound Variable Object");
                        }
                    }

                    //Validate the Information
                    if (nodeType == null)
                    {
                        throw new RdfParseException("Cannot parse a Node from the JSON where no 'type' property was specified in the JSON Object representing the Node");
                    }
                    if (nodeValue == null)
                    {
                        throw new RdfParseException("Cannot parse a Node from the JSON where no 'value' property was specified in the JSON Object representing the Node");
                    }

                    //Turn this information into a Node
                    INode n;
                    if (nodeType.Equals("uri"))
                    {
                        n = this._g.CreateUriNode(new Uri(nodeValue));
                    }
                    else if (nodeType.Equals("bnode"))
                    {
                        if (nodeValue.StartsWith("_:"))
                        {
                            n = this._g.CreateBlankNode(nodeValue.Substring(2));
                        }
                        else if (nodeValue.Contains("://"))
                        {
                            n = this._g.CreateBlankNode(nodeValue.Substring(nodeValue.IndexOf("://") + 3));
                        }
                        else if (nodeValue.Contains(":"))
                        {
                            n = this._g.CreateBlankNode(nodeValue.Substring(nodeValue.LastIndexOf(':') + 1));
                        }
                        else
                        {
                            n = this._g.CreateBlankNode(nodeValue);
                        }
                    }
                    else if (nodeType.Equals("literal") || nodeType.Equals("typed-literal"))
                    {
                        if (nodeLang != null)
                        {
                            n = this._g.CreateLiteralNode(nodeValue, nodeLang);
                        }
                        else if (nodeDatatype != null)
                        {
                            n = this._g.CreateLiteralNode(nodeValue, new Uri(nodeDatatype));
                        }
                        else
                        {
                            n = this._g.CreateLiteralNode(nodeValue);
                        }
                    }
                    else
                    {
                        throw new RdfParseException("Cannot parse a Node from the JSON where the 'type' property has a value of '" + nodeType + "' which is not one of the permitted values 'uri', 'bnode', 'literal' or 'typed-literal' in the JSON Object representing the Node");
                    }

                    //Add to the result
                    r.SetValue(var, n);
                }
                else
                {
                    throw Error(reader, "Unexpected Token '" + reader.TokenType.ToString() + "' encountered, expected the start of a Bound Variable Object");
                }
            }
            else
            {
                throw Error(reader, "Unexpected End of Input while trying to parse a Bound Variable Object");
            }
        }

        /// <summary>
        /// Parser method which parses the 'boolean' property of the Result Set
        /// </summary>
        private void ParseBoolean(JsonTextReader reader, SparqlResultSet results)
        {
            //Expect a Boolean
            if (reader.Read())
            {
                if (reader.TokenType == JsonToken.Boolean)
                {
                    Boolean result = Boolean.Parse(reader.Value.ToString());
                    results.SetResult(result);
                }
                else
                {
                    throw Error(reader, "Unexpected Token '" + reader.TokenType.ToString() + "' encountered, expected a Boolean value for the 'boolean' property of the JSON Result Set");
                }
            }
            else
            {
                throw new RdfParseException("Unexpected End of Input while trying to parse the 'boolean' property of the JSON Result Set Object");
            }
        }

        /// <summary>
        /// Helper method for raising Error messages with attached Line Information
        /// </summary>
        /// <param name="reader">Json Text Reader being used to parse the Result Set</param>
        /// <param name="message">Error Message</param>
        /// <returns></returns>
        private RdfParseException Error(JsonTextReader reader, String message)
        {
            StringBuilder error = new StringBuilder();
            if (reader.HasLineInfo())
            {
                error.Append("[Line " + reader.LineNumber + " Column " + reader.LinePosition + "] ");
            }
            error.AppendLine(reader.TokenType.ToString());
            error.Append(message);
            throw new RdfParseException(error.ToString(), reader.LineNumber, reader.LinePosition);
        }

        /// <summary>
        /// Helper Method which raises the Warning event when a non-fatal issue with the SPARQL Results being parsed is detected
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            SparqlWarning d = this.Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event raised when a non-fatal issue with the SPARQL Results being parsed is detected
        /// </summary>
        public event SparqlWarning Warning;
    }
}
