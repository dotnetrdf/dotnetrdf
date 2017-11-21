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
using System.Xml;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for saving Sparql Result Sets to the Sparql Results XML Format
    /// </summary>
    public class SparqlXmlWriter : ISparqlResultsWriter
    {

        /// <summary>
        /// Saves the Result Set to the given File in the Sparql Results XML Format
        /// </summary>
        /// <param name="results">Result Set to save</param>
        /// <param name="filename">File to save to</param>
        public virtual void Save(SparqlResultSet results, String filename)
        {
            using (var stream = File.Open(filename, FileMode.Create))
            {
                Save(results, new StreamWriter(stream, new UTF8Encoding(Options.UseBomForUtf8)));
            }
        }

        private XmlWriterSettings GetSettings()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.CloseOutput = true;
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.Encoding = new UTF8Encoding(Options.UseBomForUtf8);
            settings.Indent = true;
            settings.NewLineHandling = NewLineHandling.None;
            settings.OmitXmlDeclaration = false;
            return settings;
        }

        /// <summary>
        /// Saves the Result Set to the given Stream in the Sparql Results XML Format
        /// </summary>
        /// <param name="results"></param>
        /// <param name="output"></param>
        public virtual void Save(SparqlResultSet results, TextWriter output)
        {
            try
            {
                XmlWriter writer = XmlWriter.Create(output, GetSettings());
                GenerateOutput(results, writer);
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
        /// Method which generates the Sparql Query Results XML Format serialization of the Result Set
        /// </summary>
        /// <returns></returns>
        protected void GenerateOutput(SparqlResultSet resultSet, XmlWriter writer)
        {
            // XML Declaration
            writer.WriteStartDocument();

            // <sparql> element
            writer.WriteStartElement("sparql", SparqlSpecsHelper.SparqlNamespace);

            // <head> element
            writer.WriteStartElement("head");

            // Variables in the Header?
            if (resultSet.ResultsType == SparqlResultsType.VariableBindings)
            {
                foreach (String var in resultSet.Variables)
                {
                    // <variable> element
                    writer.WriteStartElement("variable");
                    writer.WriteAttributeString("name", var);
                    writer.WriteEndElement();
                }

                // </head> Element
                writer.WriteEndElement();

                // <results> Element
                writer.WriteStartElement("results");

                foreach (SparqlResult r in resultSet.Results)
                {
                    // <result> Element
                    writer.WriteStartElement("result");

                    foreach (String var in resultSet.Variables)
                    {
                        if (r.HasValue(var))
                        {
                            INode n = r.Value(var);
                            if (n == null) continue; //NULLs don't get serialized in the XML Format

                            // <binding> Element
                            writer.WriteStartElement("binding");
                            writer.WriteAttributeString("name", var);

                            switch (n.NodeType)
                            {
                                case NodeType.Blank:
                                    // <bnode> element
                                    writer.WriteStartElement("bnode");
                                    writer.WriteRaw(((IBlankNode)n).InternalID);
                                    writer.WriteEndElement();
                                    break;

                                case NodeType.GraphLiteral:
                                    // Error!
                                    throw new RdfOutputException("Result Sets which contain Graph Literal Nodes cannot be serialized in the SPARQL Query Results XML Format");

                                case NodeType.Literal:
                                    // <literal> element
                                    writer.WriteStartElement("literal");
                                    ILiteralNode l = (ILiteralNode)n;

                                    if (!l.Language.Equals(String.Empty))
                                    {
                                        writer.WriteStartAttribute("xml", "lang", XmlSpecsHelper.NamespaceXml);
                                        writer.WriteRaw(l.Language);
                                        writer.WriteEndAttribute();
                                    }
                                    else if (l.DataType != null)
                                    {
                                        writer.WriteStartAttribute("datatype");
                                        writer.WriteRaw(WriterHelper.EncodeForXml(l.DataType.AbsoluteUri));
                                        writer.WriteEndAttribute();
                                    }

                                    // Write the Value and the </literal>
                                    writer.WriteRaw(WriterHelper.EncodeForXml(l.Value));
                                    writer.WriteEndElement();
                                    break;

                                case NodeType.Uri:
                                    // <uri> element
                                    writer.WriteStartElement("uri");
                                    writer.WriteRaw(WriterHelper.EncodeForXml(((IUriNode)n).Uri.AbsoluteUri));
                                    writer.WriteEndElement();
                                    break;

                                default:
                                    throw new RdfOutputException("Result Sets which contain Nodes of unknown Type cannot be serialized in the SPARQL Query Results XML Format");
                            }

                            // </binding> element
                            writer.WriteEndElement();
                        }
                    }

                    // </result> element
                    writer.WriteEndElement();
                }

                // </results>
                writer.WriteEndElement();
            }
            else
            {
                // </head>
                writer.WriteEndElement();

                // <boolean> element
                writer.WriteStartElement("boolean");
                writer.WriteRaw(resultSet.Result.ToString().ToLower());
                writer.WriteEndElement();
            }

            // </sparql> element
            writer.WriteEndElement();

            // End Document
            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();
        }

        /// <summary>
        /// Helper Method which raises the Warning event when a non-fatal issue with the SPARQL Results being written is detected
        /// </summary>
        /// <param name="message">Warning Message</param>
        protected void RaiseWarning(String message)
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
            return "SPARQL Results XML";
        }
    }
}
