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

#define NO_XMLDOM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for serialzing Triple Stores in the TriX format
    /// </summary>
    public class TriXWriter : IStoreWriter
    {
#if !NO_XMLDOM

        /// <summary>
        /// Saves a Store in TriX format
        /// </summary>
        /// <param name="store">Store to save</param>
        /// <param name="parameters">Parameters indicating a Stream to write to</param>
        public void Save(ITripleStore store, IStoreParams parameters)
        {
            if (parameters is StreamParams)
            {
                StreamWriter output = ((StreamParams)parameters).StreamWriter;

                try
                {
                    //Setup the XML document
                    XmlDocument doc = new XmlDocument();
                    XmlElement trix = doc.CreateElement("TriX", TriXParser.TriXNamespaceURI);
                    doc.AppendChild(trix);
                    XmlAttribute ns = doc.CreateAttribute("xmlns");
                    ns.Value = TriXParser.TriXNamespaceURI;
                    trix.Attributes.Append(ns);

                    //Output Graphs as XML <graph> elements
                    foreach (IGraph g in store.Graphs) 
                    {
                        this.GraphToTriX(g, doc, trix);
                    }

                    //Save the XML to disk
                    InternalXmlWriter writer = new InternalXmlWriter();
                    writer.Save(output, doc);
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
                        //Just cleaning up
                    }
                    throw;
                }
            }
            else
            {
                throw new RdfStorageException("Parameters for the TriXWriter must be of the type StreamParams");
            }
        }

        private void GraphToTriX(IGraph g, XmlDocument doc, XmlElement trix)
        {
            //Create the <graph> element
            XmlElement graph = doc.CreateElement("graph");
            trix.AppendChild(graph);

            //Is the Graph Named?
            if (!WriterHelper.IsDefaultGraph(g.BaseUri))
            {
                if (!g.BaseUri.ToString().StartsWith("trix:local:"))
                {
                    XmlElement uri = doc.CreateElement("uri");
                    uri.InnerText = WriterHelper.EncodeForXml(g.BaseUri.ToString());
                    graph.AppendChild(uri);
                }
                else
                {
                    XmlElement id = doc.CreateElement("id");
                    id.InnerText = WriterHelper.EncodeForXml(g.BaseUri.ToString().Substring(11));
                    graph.AppendChild(id);
                }
            }

            //Output the Triples
            foreach (Triple t in g.Triples)
            {
                XmlElement triple = doc.CreateElement("triple");
                graph.AppendChild(triple);

                this.NodeToTriX(t.Subject, doc, triple);
                this.NodeToTriX(t.Predicate, doc, triple);
                this.NodeToTriX(t.Object, doc, triple);
            }
        }

        private void NodeToTriX(INode n, XmlDocument doc, XmlElement triple)
        {
            XmlElement node;

            switch (n.NodeType)
            {
                case NodeType.Blank:
                    node = doc.CreateElement("id");
                    node.InnerText = ((BlankNode)n).InternalID;
                    break;
                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("TriX"));
                case NodeType.Literal:
                    LiteralNode lit = (LiteralNode)n;
                    if (lit.DataType != null)
                    {
                        node = doc.CreateElement("typedLiteral");
                        if (lit.DataType.ToString().Equals(RdfSpecsHelper.RdfXmlLiteral))
                        {
                            XmlCDataSection cdata = doc.CreateCDataSection(lit.Value);
                            node.AppendChild(cdata);
                        }
                        else
                        {
                            node.InnerText = lit.Value;
                        }
                        XmlAttribute type = doc.CreateAttribute("datatype");
                        type.Value = WriterHelper.EncodeForXml(lit.DataType.ToString());
                        node.Attributes.Append(type);
                    }
                    else
                    {
                        node = doc.CreateElement("plainLiteral");
                        node.InnerText = lit.Value;

                        if (!lit.Language.Equals(String.Empty))
                        {
                            XmlAttribute lang = doc.CreateAttribute("xml:lang");
                            lang.Value = lit.Language;
                            node.Attributes.Append(lang);
                        }
                    }
                    break;
                case NodeType.Uri:
                    node = doc.CreateElement("uri");
                    node.InnerText = WriterHelper.EncodeForXml(((UriNode)n).StringUri);
                    break;
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("TriX"));
            }

            triple.AppendChild(node);
        }

#else
        private XmlWriterSettings GetSettings()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.CloseOutput = true;
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;
#if SILVERLIGHT
            settings.NamespaceHandling = NamespaceHandling.OmitDuplicates;
#endif
            settings.NewLineHandling = NewLineHandling.None;
            settings.OmitXmlDeclaration = false;
            return settings;
        }

        /// <summary>
        /// Saves a Store in TriX format
        /// </summary>
        /// <param name="store">Store to save</param>
        /// <param name="parameters">Parameters indicating a Stream to write to</param>
        public void Save(ITripleStore store, IStoreParams parameters)
        {
            if (parameters is StreamParams)
            {
                StreamWriter output = ((StreamParams)parameters).StreamWriter;

                try
                {
                    //Setup the XML document
                    XmlWriter writer = XmlWriter.Create(output, this.GetSettings());
                    writer.WriteStartDocument();
                    writer.WriteStartElement("TriX", TriXParser.TriXNamespaceURI);
                    writer.WriteStartAttribute("xmlns");
                    writer.WriteRaw(TriXParser.TriXNamespaceURI);
                    writer.WriteEndAttribute();

                    //Output Graphs as XML <graph> elements
                    foreach (IGraph g in store.Graphs) 
                    {
                        this.GraphToTriX(g, writer);
                    }

                    //Save the XML to disk
                    writer.WriteEndDocument();
                    writer.Close();
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
                        //Just cleaning up
                    }
                    throw;
                }
            }
            else
            {
                throw new RdfStorageException("Parameters for the TriXWriter must be of the type StreamParams");
            }
        }

        private void GraphToTriX(IGraph g, XmlWriter writer)
        {
            //Create the <graph> element
            writer.WriteStartElement("graph");

            //Is the Graph Named?
            if (!WriterHelper.IsDefaultGraph(g.BaseUri))
            {
                if (!g.BaseUri.ToString().StartsWith("trix:local:"))
                {
                    writer.WriteStartElement("uri");
                    writer.WriteRaw(WriterHelper.EncodeForXml(g.BaseUri.ToString()));
                    writer.WriteEndElement();
                }
                else
                {
                    writer.WriteStartElement("id");
                    writer.WriteRaw(WriterHelper.EncodeForXml(g.BaseUri.ToString().Substring(11)));
                    writer.WriteEndElement();
                }
            }

            //Output the Triples
            foreach (Triple t in g.Triples)
            {
                writer.WriteStartElement("triple");

                this.NodeToTriX(t.Subject, writer);
                this.NodeToTriX(t.Predicate, writer);
                this.NodeToTriX(t.Object, writer);

                //</triple>
                writer.WriteEndElement();
            }

            //</graph>
            writer.WriteEndElement();
        }

        private void NodeToTriX(INode n, XmlWriter writer)
        {
            XmlElement node;

            switch (n.NodeType)
            {
                case NodeType.Blank:
                    writer.WriteStartElement("id");
                    writer.WriteRaw(WriterHelper.EncodeForXml(((BlankNode)n).InternalID));
                    writer.WriteEndElement();
                    break;
                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("TriX"));
                case NodeType.Literal:
                    LiteralNode lit = (LiteralNode)n;
                    if (lit.DataType != null)
                    {
                        writer.WriteStartElement("typedLiteral");
                        writer.WriteStartAttribute("datatype");
                        writer.WriteRaw(WriterHelper.EncodeForXml(lit.DataType.ToString()));
                        writer.WriteEndAttribute();
                        if (lit.DataType.ToString().Equals(RdfSpecsHelper.RdfXmlLiteral))
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
                    writer.WriteRaw(WriterHelper.EncodeForXml(((UriNode)n).StringUri));
                    writer.WriteEndElement();
                    break;
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("TriX"));
            }
        }
#endif

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
            if (this.Warning == null)
            {
                //Do Nothing
            }
            else
            {
                this.Warning(message);
            }
        }
    }
}
