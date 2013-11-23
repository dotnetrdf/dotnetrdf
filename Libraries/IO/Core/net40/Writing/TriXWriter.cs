/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;
using System.Text;
using System.Xml;
using VDS.RDF.Graphs;
using VDS.RDF.Namespaces;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Specifications;
using VDS.RDF.Writing.Contexts;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for serialzing Triple Stores in the TriX format
    /// </summary>
    public class TriXWriter
        : BaseGraphStoreWriter
    {
        private XmlWriterSettings GetSettings()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.CloseOutput = true;
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.Encoding = new UTF8Encoding(IOOptions.UseBomForUtf8);
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
        /// <param name="output">Writer to save to</param>
        public override void Save(IGraphStore store, TextWriter output)
        {
            if (store == null) throw new RdfOutputException("Cannot output a null Triple Store");
            if (output == null) throw new RdfOutputException("Cannot output to a null writer");

            try
            {
                INamespaceMapper namespaces = WriterHelper.ExtractNamespaces(store);
                TriXWriterContext context = new TriXWriterContext(store, namespaces, output, XmlWriter.Create(output, this.GetSettings()));

                //Setup the XML document
                context.XmlWriter.WriteStartDocument();
                context.XmlWriter.WriteStartElement("TriX", TriXParser.TriXNamespaceUri);
                context.XmlWriter.WriteStartAttribute("xmlns");
                context.XmlWriter.WriteRaw(TriXParser.TriXNamespaceUri);
                context.XmlWriter.WriteEndAttribute();

                //Output Graphs as XML <graph> elements
                foreach (INode graphName in context.GraphStore.GraphNames)
                {
                    context.CurrentGraphName = graphName;
                    context.CurrentGraph = context.GraphStore[graphName];
                    this.GraphToTriX(context);
                }

                //Save the XML to disk
                context.XmlWriter.WriteEndDocument();
                context.XmlWriter.Flush();
                context.XmlWriter.Close();
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

        private void GraphToTriX(TriXWriterContext context)
        {
            //Create the <graph> element
            context.XmlWriter.WriteStartElement("graph");

            //Is the Graph Named?
            // TODO Provide a static method to check whether a Graph name denotes the default graph
            if (!ReferenceEquals(context.CurrentGraphName, null))
            {
                switch (context.CurrentGraphName.NodeType)
                {
                    case NodeType.Uri:
                        context.XmlWriter.WriteStartElement("uri");
                        context.XmlWriter.WriteRaw(WriterHelper.EncodeForXml(context.CurrentGraphName.Uri.AbsoluteUri));
                        context.XmlWriter.WriteEndElement();
                        break;
                    case NodeType.Blank:
                        context.XmlWriter.WriteStartElement("id");
                        context.XmlWriter.WriteRaw(WriterHelper.EncodeForXml(context.BlankNodeMapper.GetOutputId(context.CurrentGraphName.AnonID)));
                        context.XmlWriter.WriteEndElement();
                        break;
                    default:
                        throw new RdfOutputException("Unsupport graph name type for TriX output");
                }
            }

            //Output the Triples
            foreach (Triple t in context.CurrentGraph.Triples)
            {
                context.XmlWriter.WriteStartElement("triple");

                this.NodeToTriX(t.Subject, context);
                this.NodeToTriX(t.Predicate, context);
                this.NodeToTriX(t.Object, context);

                //</triple>
                context.XmlWriter.WriteEndElement();
            }

            //</graph>
            context.XmlWriter.WriteEndElement();
        }

        private void NodeToTriX(INode n, TriXWriterContext context)
        {
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    context.XmlWriter.WriteStartElement("id");
                    context.XmlWriter.WriteRaw(WriterHelper.EncodeForXml(context.BlankNodeMapper.GetOutputId((n).AnonID)));
                    context.XmlWriter.WriteEndElement();
                    break;
                case NodeType.GraphLiteral:
                    throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("TriX"));
                case NodeType.Literal:
                    if (n.HasDataType && !n.HasLanguage)
                    {
                        context.XmlWriter.WriteStartElement("typedLiteral");
                        context.XmlWriter.WriteStartAttribute("datatype");
                        context.XmlWriter.WriteRaw(WriterHelper.EncodeForXml(n.DataType.AbsoluteUri));
                        context.XmlWriter.WriteEndAttribute();
                        if (n.DataType.AbsoluteUri.Equals(RdfSpecsHelper.RdfXmlLiteral))
                        {
                            context.XmlWriter.WriteCData(n.Value);
                        }
                        else
                        {
                            context.XmlWriter.WriteRaw(WriterHelper.EncodeForXml(n.Value));
                        }
                        context.XmlWriter.WriteEndElement();
                    }
                    else
                    {
                        context.XmlWriter.WriteStartElement("plainLiteral");
                        if (n.HasLanguage)
                        {
                            context.XmlWriter.WriteAttributeString("xml", "lang", null, n.Language);
                        }
                        context.XmlWriter.WriteRaw(WriterHelper.EncodeForXml(n.Value));
                        context.XmlWriter.WriteEndElement();
                    }
                    break;
                case NodeType.Uri:
                    context.XmlWriter.WriteStartElement("uri");
                    context.XmlWriter.WriteRaw(WriterHelper.EncodeForXml(((INode)n).Uri.AbsoluteUri));
                    context.XmlWriter.WriteEndElement();
                    break;
                default:
                    throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("TriX"));
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
