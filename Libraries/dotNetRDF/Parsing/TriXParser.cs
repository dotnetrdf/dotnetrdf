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
using System.Text.RegularExpressions;
using System.Xml;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;
using VDS.RDF.Writing;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Parser for parsing TriX (a named Graph XML format for RDF)
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Default Graph (if any) will be given the special Uri <strong>trix:default-graph</strong>
    /// </para>
    /// <para>
    /// TriX permits Graphs to be named with Blank Node IDs, since the library only supports Graphs named with URIs these are converted to URIs of the form <strong>trix:local:ID</strong>
    /// </para>
    /// </remarks>
    public class TriXParser
        : IStoreReader
    {
        /// <summary>
        /// Current W3C Namespace Uri for TriX
        /// </summary>
        public const String TriXNamespaceURI = "http://www.w3.org/2004/03/trix/trix-1/";


        /// <summary>
        /// Loads the RDF Dataset from the TriX input into the given Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="filename">File to load from</param>
        public void Load(ITripleStore store, String filename)
        {
            if (filename == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null file");
            Load(store, new StreamReader(File.OpenRead(filename), Encoding.UTF8));
        }

        /// <summary>
        /// Loads the RDF Dataset from the TriX input into the given Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="input">Input to load from</param>
        public void Load(ITripleStore store, TextReader input)
        {
            if (store == null) throw new RdfParseException("Cannot parse an RDF Dataset into a null store");
            if (input == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null input");
            Load(new StoreHandler(store), input);
        }


        /// <summary>
        /// Loads the RDF Dataset from the TriX input using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="filename">File to load from</param>
        public void Load(IRdfHandler handler, String filename)
        {
            if (filename == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null file");
            Load(handler, new StreamReader(File.OpenRead(filename), Encoding.UTF8));
        }

        private XmlReaderSettings GetSettings()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
#if !NETCORE
            settings.DtdProcessing = DtdProcessing.Parse;
            //settings.ProhibitDtd = false;
#endif
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            return settings;
        }

        /// <inheritdoc />
        public void Load(IRdfHandler handler, TextReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot parse an RDF Dataset using a null handler");
            if (input == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null input");

            if (input != null)
            {
                // First try and load as XML and apply any stylesheets
                try
                {
                    // Get the reader and start parsing
                    XmlReader reader = XmlReader.Create(input, GetSettings());
                    RaiseWarning("The TriX Parser is operating without XSL support, if your TriX file requires XSL then it will not be parsed successfully");
                    TryParseGraphset(reader, handler);

                    input.Close();
                }
                catch (XmlException xmlEx)
                {
                    try
                    {
                        input.Close();
                    }
                    catch
                    {
                        // No catch actions - just cleaning up
                    }
                    // Wrap in a RDF Parse Exception
                    throw new RdfParseException("Unable to Parse this TriX since the XmlReader encountered an error, see the inner exception for details", xmlEx);
                }
                catch
                {
                    try
                    {
                        input.Close();
                    }
                    catch
                    {
                        // No catch actions - just cleaning up
                    }
                    throw;
                }
            }
            else
            {
                throw new RdfStorageException("Parameters for the TriXParser must be of the type StreamParams/TextReaderParams");
            }
        }

        private void TryParseGraphset(XmlReader reader, IRdfHandler handler)
        {
            try
            {
                handler.StartRdf();

                reader.Read();

                // Skip XML Declaration if present
                if (reader.NodeType == XmlNodeType.XmlDeclaration) reader.Read();

                if (!reader.Name.Equals("TriX"))
                {
                    throw new RdfParseException("Unexpected Document Element '" + reader.Name + "' encountered, expected the Document Element of a TriX Document to be the <TriX> element");
                }

                if (!reader.HasAttributes)
                {
                    throw new RdfParseException("<TriX> fails to define any attributes, the element must define the xmlns attribute to be the TriX namespace");
                }
                else
                {
                    bool trixNSDefined = false;
                    for (int i = 0; i < reader.AttributeCount; i++)
                    {
                        reader.MoveToNextAttribute();

                        if (reader.Name.Equals("xmlns"))
                        {
                            // Ensure that the xmlns attribute is defined and is the TriX namespace
                            if (trixNSDefined) throw new RdfParseException("The xmlns attribute can only occur once on the <TriX> element");
                            if (!reader.Value.Equals(TriXNamespaceURI)) throw new RdfParseException("The xmlns attribute of the <TriX> element must have it's value set to the TriX Namespace URI which is '" + TriXNamespaceURI + "'");
                            trixNSDefined = true;
                        }
                        else if (reader.LocalName.Equals("xmlns"))
                        {
                            // Don't think we need to do anything here
                        }
                    }

                    if (!trixNSDefined) throw new RdfParseException("The <TriX> element fails to define the required xmlns attribute defining the TriX Namespace");
                }

                // Process Child Nodes
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        // For elements we recurse to try and parse a Graph
                        TryParseGraph(reader, handler);
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        // Stop when we hit an end element
                        break;
                    }
                    else
                    {
                        // Stop if we hit an unexpected element
                        break;
                    }
                }

                // Expect the </TriX> element
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (!reader.Name.Equals("TriX")) throw new RdfParseException("Expected </TriX> element was not found, encountered </" + reader.Name + "> instead");
                }
                else
                {
                    throw new RdfParseException("Unexpected Note Type " + reader.NodeType.ToString() + " encountered, expected a </TriX> element");
                }
                handler.EndRdf(true);
            }
            catch (RdfParsingTerminatedException)
            {
                handler.EndRdf(true);
                // Discard this - it justs means the Handler told us to stop
            }
            catch
            {
                handler.EndRdf(false);
                throw;
            }
        }

        private void TryParseGraph(XmlReader reader, IRdfHandler handler)
        {
            // Ensure Node Name is correct
            if (!reader.Name.Equals("graph"))
            {
                throw new RdfParseException("Unexpected Element <" + reader.Name + "> encountered, only <graph> elements are permitted within a <TriX> element");
            }
            // If we've got an empty graph, we can safely ignore it.
            if (!reader.IsEmptyElement)
            {

                // Check whether this Graph is actually asserted
                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToNextAttribute();
                    if (reader.Name.Equals("asserted"))
                    {
                        if (Boolean.TryParse(reader.Value, out bool asserted))
                        {
                            // Don't process this Graph further if it is not being asserted
                            // i.e. it is only (potentially) being quoted
                            if (!asserted)
                            {
                                RaiseWarning("A Graph is marked as not asserted in the TriX input.  This Graph will not be parsed, if you reserialize the input the information contained in it will not be preserved");
                                return;
                            }
                        }
                    }
                }

                // See if we get an <id>/<uri> node to name the Graph
                reader.Read();
                Uri graphUri = null;
                if (reader.NodeType == XmlNodeType.Element)
                {
                    // Process the name into a Graph Uri and create the Graph and add it to the Store
                    if (reader.Name.Equals("uri"))
                    {
                        // TODO: Add support for reading Base Uri from xml:base attributes in the file
                        graphUri = new Uri(Tools.ResolveUri(reader.ReadInnerXml(), String.Empty));
                    }
                }

                // If the next element is a </graph> then this is an empty graph and we should return
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name.Equals("graph"))
                    {
                        return;
                    }
                    else
                    {
                        throw Error("Unexpected </" + reader.Name + "> encountered, either <triple> elements or a </graph> was expected", reader);
                    }
                }

                // Process the Child Nodes of the <graph> element to yield Triples
                do
                {
                    // Remember to ignore anything that isn't an element i.e. comments and processing instructions
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        TryParseTriple(reader, handler, graphUri);
                    }

                    reader.Read();
                } while (reader.NodeType != XmlNodeType.EndElement);

                if (!reader.Name.Equals("graph")) throw Error("Expected a </graph> but a </" + reader.Name + "> was encountered", reader);
            }
        }

        private void TryParseTriple(XmlReader reader, IRdfHandler handler, Uri graphUri)
        {
            // Verify Node Name
            if (!reader.Name.Equals("triple"))
            {
                throw Error("Unexpected Element <" + reader.Name + "> encountered, only an optional <id>/<uri> element followed by zero/more <triple> elements are permitted within a <graph> element", reader);
            }

            // Parse XML Nodes into RDF Nodes
            INode subj, pred, obj;
            subj = TryParseNode(reader, handler, TripleSegment.Subject);
            pred = TryParseNode(reader, handler, TripleSegment.Predicate);
            obj = TryParseNode(reader, handler, TripleSegment.Object);

            if (reader.NodeType != XmlNodeType.EndElement) throw Error("Unexpected element type " + reader.NodeType.ToString() + " encountered, expected the </triple> element", reader);
            if (!reader.Name.Equals("triple")) throw Error("Unexpected </" + reader.Name + "> encountered, expected a </triple> element", reader);

            // Assert the resulting Triple
            if (!handler.HandleTriple(new Triple(subj, pred, obj, graphUri))) ParserHelper.Stop();
        }

        private INode TryParseNode(XmlReader reader, IRdfHandler handler, TripleSegment segment)
        {
            // Only need to Read() if getting the Subject
            // The previous calls will have resulted in us already reading to the start element for this node
            if (segment == TripleSegment.Subject) reader.Read();

            if (reader.NodeType != XmlNodeType.Element)
            {
                if (reader.NodeType == XmlNodeType.EndElement) throw Error("Unexpected end element while trying to parse the nodes of a <triple> element", reader);
            }

            if (reader.Name.Equals("uri"))
            {
                return handler.CreateUriNode(new Uri(reader.ReadInnerXml()));
            }
            else if (reader.Name.Equals("id"))
            {
                if (segment == TripleSegment.Predicate) throw Error("Unexpected element <" + reader.Name + "> encountered, expected a <uri> element as the Predicate of a Triple", reader);

                return handler.CreateBlankNode(reader.ReadInnerXml());
            }
            else if (reader.Name.Equals("plainLiteral"))
            {
                if (segment == TripleSegment.Subject) throw Error("Unexpected element <" + reader.Name + "> encountered, expected a <id>/<uri> element as the Subject of a Triple", reader);

                if (reader.AttributeCount > 0)
                {
                    String lang = String.Empty;
                    for (int i = 0; i < reader.AttributeCount; i++)
                    {
                        reader.MoveToNextAttribute();
                        if (reader.Name.Equals("xml:lang")) lang = reader.Value;
                    }
                    reader.MoveToContent();
                    if (!lang.Equals(String.Empty))
                    {
                        return handler.CreateLiteralNode(reader.ReadElementContentAsString(), lang);
                    }
                    else
                    {
                        return handler.CreateLiteralNode(reader.ReadElementContentAsString());
                    }
                }
                else
                {
                    return handler.CreateLiteralNode(reader.ReadElementContentAsString());
                }
            }
            else if (reader.Name.Equals("typedLiteral"))
            {
                if (reader.AttributeCount > 0)
                {
                    Uri dtUri = null;
                    for (int i = 0; i < reader.AttributeCount; i++)
                    {
                        reader.MoveToNextAttribute();
                        if (reader.Name.Equals("datatype")) dtUri = new Uri(reader.Value);
                    }
                    if (dtUri == null) throw Error("<typedLiteral> element does not have the required datatype attribute", reader);

                    reader.MoveToContent();
                    // KA: Why ReadInnerXml here and not in other places?
                    // return handler.CreateLiteralNode(reader.ReadInnerXml(), dtUri);
                    return handler.CreateLiteralNode(reader.ReadElementContentAsString(), dtUri);
                }
                else
                {
                    throw Error("<typedLiteral> element does not have the required datatype attribute", reader);
                }
            }
            else
            {
                throw Error("Unexpected element <" + reader.Name + "> encountered, expected a <id>/<uri>/<plainLiteral>/<typedLiteral> element as part of a Triple", reader);
            }
        }

        /// <summary>
        /// Helper method for raising informative standardised Parser Errors
        /// </summary>
        /// <param name="msg">The Error Message</param>
        /// <param name="reader">The XML reader being used by the parser</param>
        /// <returns></returns>
        private RdfParseException Error(String msg, XmlReader reader)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("[Source XML]");
            output.AppendLine(reader.Value);
            output.AppendLine();
            output.Append(msg);

            return new RdfParseException(output.ToString());
        }

        /// <summary>
        /// Helper method used to raise the Warning event if there is an event handler registered
        /// </summary>
        /// <param name="message">Warning message</param>
        private void RaiseWarning(string message)
        {
            Warning?.Invoke(message);
        }

        /// <summary>
        /// Event which Readers can raise when they notice syntax that is ambigious/deprecated etc which can still be parsed
        /// </summary>
        public event StoreReaderWarning Warning;

        /// <summary>
        /// Gets the String representation of the Parser which is a description of the syntax it parses
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "TriX";
        }
    }
}
