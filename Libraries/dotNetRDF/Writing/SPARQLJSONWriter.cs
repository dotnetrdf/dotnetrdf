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
using VDS.RDF.Query;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for saving Sparql Result Sets to the SPARQL Results JSON Format
    /// </summary>
    public class SparqlJsonWriter : ISparqlResultsWriter
    {

        /// <summary>
        /// Saves the Result Set to the given File in the SPARQL Results JSON Format
        /// </summary>
        /// <param name="results">Result Set to save</param>
        /// <param name="filename">File to save to</param>
        public void Save(SparqlResultSet results, String filename)
        {
            using (var stream = File.Open(filename, FileMode.Create))
            {
                Save(results, new StreamWriter(stream, new UTF8Encoding(Options.UseBomForUtf8)));
            }
        }

        /// <summary>
        /// Saves the Result Set to the given Stream in the SPARQL Results JSON Format
        /// </summary>
        /// <param name="results">Result Set to save</param>
        /// <param name="output">Stream to save to</param>
        public void Save(SparqlResultSet results, TextWriter output)
        {
            try
            {
                GenerateOutput(results, output);
                output.Close();
            }
            catch
            {
                try
                {
                    output.Close();
                }
                catch
                {
                    // No Catch Actions
                }
                throw;
            }
        }

        /// <summary>
        /// Internal method which generates the SPARQL Query Results JSON output
        /// </summary>
        /// <param name="results">Result Set to save</param>
        /// <param name="output">Stream to save to</param>
        private void GenerateOutput(SparqlResultSet results, TextWriter output)
        {
            JsonTextWriter writer = new JsonTextWriter(output);
            writer.Formatting = Newtonsoft.Json.Formatting.Indented;

            // Start a Json Object for the Result Set
            writer.WriteStartObject();

            // Create the Head Object
            writer.WritePropertyName("head");
            writer.WriteStartObject();

            if (results.ResultsType == SparqlResultsType.VariableBindings)
            {
                // SELECT query results

                // Create the Variables Object
                writer.WritePropertyName("vars");
                writer.WriteStartArray();
                foreach (String var in results.Variables)
                {
                    writer.WriteValue(var);
                }
                writer.WriteEndArray();

                // End Head Object
                writer.WriteEndObject();

                // Create the Result Object
                writer.WritePropertyName("results");
                writer.WriteStartObject();
                writer.WritePropertyName("bindings");
                writer.WriteStartArray();

                foreach (SparqlResult result in results)
                {
                    // Create a Binding Object
                    writer.WriteStartObject();
                    foreach (String var in results.Variables)
                    {
                        if (!result.HasValue(var)) continue; //No output for unbound variables

                        INode value = result.Value(var);
                        if (value == null) continue;

                        // Create an Object for the Variable
                        writer.WritePropertyName(var);
                        writer.WriteStartObject();
                        writer.WritePropertyName("type");

                        switch (value.NodeType)
                        {
                            case NodeType.Blank:
                                // Blank Node
                                writer.WriteValue("bnode");
                                writer.WritePropertyName("value");
                                String id = ((IBlankNode)value).InternalID;
                                id = id.Substring(id.IndexOf(':') + 1);
                                writer.WriteValue(id);
                                break;

                            case NodeType.GraphLiteral:
                                // Error
                                throw new RdfOutputException("Result Sets which contain Graph Literal Nodes cannot be serialized in the SPARQL Query Results JSON Format");

                            case NodeType.Literal:
                                // Literal
                                ILiteralNode lit = (ILiteralNode)value;
                                if (lit.DataType != null)
                                {
                                    writer.WriteValue("typed-literal");
                                }
                                else
                                {
                                    writer.WriteValue("literal");
                                }
                                writer.WritePropertyName("value");

                                writer.WriteValue(lit.Value);
                                if (!lit.Language.Equals(String.Empty))
                                {
                                    writer.WritePropertyName("xml:lang");
                                    writer.WriteValue(lit.Language);
                                }
                                else if (lit.DataType != null)
                                {
                                    writer.WritePropertyName("datatype");
                                    writer.WriteValue(lit.DataType.AbsoluteUri);
                                }
                                break;

                            case NodeType.Uri:
                                // Uri
                                writer.WriteValue("uri");
                                writer.WritePropertyName("value");
                                writer.WriteValue(value.ToString());
                                break;

                            default:
                                throw new RdfOutputException("Result Sets which contain Nodes of unknown Type cannot be serialized in the SPARQL Query Results JSON Format");
                        }

                        // End the Variable Object
                        writer.WriteEndObject();
                    }
                    // End the Binding Object
                    writer.WriteEndObject();
                }

                // End Result Object
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            else
            {
                // ASK query result

                // Set an empty Json Object in the Head
                writer.WriteEndObject();

                // Create a Boolean Property
                writer.WritePropertyName("boolean");
                writer.WriteValue(results.Result);
            }

            // End the Json Object for the Result Set
            writer.WriteEndObject();

        }

        /// <summary>
        /// Helper Method which raises the Warning event when a non-fatal issue with the SPARQL Results being written is detected
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
        /// Event raised when a non-fatal issue with the SPARQL Results being written is detected
        /// </summary>
        public event SparqlWarning Warning;

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "SPARQL Results JSON";
        }
    }
}
