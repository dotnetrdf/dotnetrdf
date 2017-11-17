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

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for serialzing Triple Stores in the TriX format
    /// </summary>
    public class TriXWriter
        : IStoreWriter
    {
        private XmlWriterSettings GetSettings(bool leaveOpen)
        {
            var settings = new XmlWriterSettings
            {
                CloseOutput = !leaveOpen,
                ConformanceLevel = ConformanceLevel.Document,
                Encoding = new UTF8Encoding(Options.UseBomForUtf8),
                Indent = true,
                NewLineHandling = NewLineHandling.None,
                OmitXmlDeclaration = false,
            };
            return settings;
        }

        /// <summary>
        /// Saves a Store in TriX format
        /// </summary>
        /// <param name="store">Store to save</param>
        /// <param name="filename">File to save to</param>
        public void Save(ITripleStore store, String filename)
        {
            if (filename == null) throw new RdfOutputException("Cannot output to a null file");
            using (var stream = File.Open(filename, FileMode.Create))
            {
                Save(store, new StreamWriter(stream, new UTF8Encoding(Options.UseBomForUtf8)), false);
            }
        }

        /// <summary>
        /// Saves a Store in TriX format
        /// </summary>
        /// <param name="store">Store to save</param>
        /// <param name="output">Writer to save to</param>
        public void Save(ITripleStore store, TextWriter output)
        {
            Save(store, output , false);
        }

        /// <summary>
        /// Saves a Store in TriX format
        /// </summary>
        /// <param name="store">Store to save</param>
        /// <param name="output">Writer to save to</param>
        /// <param name="leaveOpen">Boolean flag indicating if <paramref name="output"/> should be closed after the store is saved</param>
        public void Save(ITripleStore store, TextWriter output, bool leaveOpen)
        {
            if (store == null) throw new RdfOutputException("Cannot output a null Triple Store");
            if (output == null) throw new RdfOutputException("Cannot output to a null writer");

            try
            {
                // Setup the XML document
                var writer = XmlWriter.Create(output, GetSettings(leaveOpen));
                writer.WriteStartDocument();
                writer.WriteStartElement("TriX", TriXParser.TriXNamespaceURI);
                writer.WriteStartAttribute("xmlns");
                writer.WriteRaw(TriXParser.TriXNamespaceURI);
                writer.WriteEndAttribute();

                // Output Graphs as XML <graph> elements
                foreach (var g in store.Graphs)
                {
                    GraphToTriX(g, writer);
                }

                // Save the XML to disk
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
                if (!leaveOpen)
                {
                    output.Close();
                }
            }
            catch
            {
                try
                {
                    if (!leaveOpen)
                    {
                        output.Close();
                    }
                }
                catch
                {
                    // Just cleaning up
                }
                throw;
            }
        }

        private void GraphToTriX(IGraph g, XmlWriter writer)
        {
            // Create the <graph> element
            writer.WriteStartElement("graph");

            // Is the Graph Named?
            if (g.BaseUri != null)
            {
                if (!g.BaseUri.AbsoluteUri.StartsWith("trix:local:"))
                {
                    writer.WriteStartElement("uri");
                    writer.WriteRaw(WriterHelper.EncodeForXml(g.BaseUri.AbsoluteUri));
                    writer.WriteEndElement();
                }
                else
                {
                    writer.WriteStartElement("id");
                    writer.WriteRaw(WriterHelper.EncodeForXml(g.BaseUri.AbsoluteUri.Substring(11)));
                    writer.WriteEndElement();
                }
            }

            // Output the Triples
            foreach (Triple t in g.Triples)
            {
                writer.WriteStartElement("triple");

                NodeToTriX(t.Subject, writer);
                NodeToTriX(t.Predicate, writer);
                NodeToTriX(t.Object, writer);

                // </triple>
                writer.WriteEndElement();
            }

            // </graph>
            writer.WriteEndElement();
        }

        private void NodeToTriX(INode n, XmlWriter writer)
        {
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    writer.WriteStartElement("id");
                    writer.WriteRaw(WriterHelper.EncodeForXml(((IBlankNode)n).InternalID));
                    writer.WriteEndElement();
                    break;
                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("TriX"));
                case NodeType.Literal:
                    ILiteralNode lit = (ILiteralNode)n;
                    if (lit.DataType != null)
                    {
                        writer.WriteStartElement("typedLiteral");
                        writer.WriteStartAttribute("datatype");
                        writer.WriteRaw(WriterHelper.EncodeForXml(lit.DataType.AbsoluteUri));
                        writer.WriteEndAttribute();
                        if (lit.DataType.AbsoluteUri.Equals(RdfSpecsHelper.RdfXmlLiteral))
                        {
                            writer.WriteCData(lit.Value);
                        }
                        else
                        {
                            writer.WriteRaw(WriterHelper.EncodeForXml(lit.Value));
                        }
                        writer.WriteEndElement();
                    }
                    else
                    {
                        writer.WriteStartElement("plainLiteral");
                        if (!lit.Language.Equals(String.Empty))
                        {
                            writer.WriteAttributeString("xml", "lang", null, lit.Language);
                        }
                        writer.WriteRaw(WriterHelper.EncodeForXml(lit.Value));
                        writer.WriteEndElement();
                    }
                    break;
                case NodeType.Uri:
                    writer.WriteStartElement("uri");
                    writer.WriteRaw(WriterHelper.EncodeForXml(((IUriNode)n).Uri.AbsoluteUri));
                    writer.WriteEndElement();
                    break;
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("TriX"));
            }
        }

        /// <summary>
        /// Event which is raised when there is an issue with the Graphs being serialized that doesn't prevent serialization but the user should be aware of
        /// </summary>
        public event StoreWriterWarning Warning;

        /// <summary>
        /// Internal Helper method which raises the Warning event only if there is an Event Handler registered
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            if (Warning == null)
            {
                // Do Nothing
            }
            else
            {
                Warning(message);
            }
        }

        /// <summary>
        /// Gets the String representation of the writer which is a description of the syntax it produces
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "TriX";
        }
    }
}
