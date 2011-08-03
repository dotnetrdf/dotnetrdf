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

//Uncomment this when making changes that need to work with the non-XML DOM code
//#define NO_XMLDOM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
#if !NO_XSL
using System.Xml.Xsl;
#endif
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;
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
    public class TriXParser : IStoreReader
    {
        /// <summary>
        /// Default Graph Uri for default graphs parsed from TriX input
        /// </summary>
        public const String DefaultGraphURI = "trix:default-graph";
        /// <summary>
        /// Current W3C Namespace Uri for TriX
        /// </summary>
        public const String TriXNamespaceURI = "http://www.w3.org/2004/03/trix/trix-1/";

#if !NO_XMLDOM

        /// <summary>
        /// Loads the RDF Dataset from the TriX input into the given Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="parameters">Parameters indicating the input to read from</param>
        public void Load(ITripleStore store, IStoreParams parameters)
        {
            if (store == null) throw new RdfParseException("Cannot read a RDF dataset into a null Store");
            this.Load(new StoreHandler(store), parameters);
        }

        /// <summary>
        /// Loads the RDF Dataset from the TriX input using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="parameters">Parameters indicating the input to read from</param>
        public void Load(IRdfHandler handler, IStoreParams parameters)
        {
            if (handler == null) throw new ArgumentNullException("handler", "Cannot parse an RDF Dataset using a null RDF Handler");
            if (parameters == null) throw new ArgumentNullException("parameters", "Cannot parse an RDF Dataset using null Parameters");

            //Try and get the Input from the parameters
            TextReader input = null;
            if (parameters is StreamParams)
            {
                //Get Input Stream
                input = ((StreamParams)parameters).StreamReader;

                //Issue a Warning if the Encoding of the Stream is not UTF-8
                if (!((StreamReader)input).CurrentEncoding.Equals(Encoding.UTF8))
                {
                    this.RaiseWarning("Expected Input Stream to be encoded as UTF-8 but got a Stream encoded as " + ((StreamReader)input).CurrentEncoding.EncodingName + " - Please be aware that parsing errors may occur as a result");
                }
            } 
            else if (parameters is TextReaderParams)
            {
                input = ((TextReaderParams)parameters).TextReader;
            }

            if (input != null)
            {
                //First try and load as XML and apply any stylesheets
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(input);

                    input.Close();

                    XmlDocument inputDoc = new XmlDocument();
                    bool inputReady = false;

                    //If XSL isn't supported we can't apply it
#if !NO_XSL

                    //Try and apply any stylesheets (which are used to extend TriX) to get basic TriX syntax
                    foreach (XmlNode child in doc.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.ProcessingInstruction)
                        {
                            if (child.Name == "xml-stylesheet")
                            {
                                //Load in the XML a 2nd time so we can transform it properly if needed
                                if (!inputReady)
                                {
                                    inputDoc.LoadXml(doc.OuterXml);
                                    inputReady = true;
                                }

                                Regex getStylesheetURI = new Regex("href=\"([^\"]+)\"");
                                String stylesheetUri = getStylesheetURI.Match(child.Value).Groups[1].Value;

                                //Load the Transform
                                XslCompiledTransform transform = new XslCompiledTransform();
                                XsltSettings settings = new XsltSettings();
                                transform.Load(stylesheetUri, settings, null);

                                //Apply the Transform
                                MemoryStream temp = new MemoryStream();
                                transform.Transform(inputDoc, XmlWriter.Create(temp));
                                temp.Flush();
                                temp.Seek(0, SeekOrigin.Begin);
                                inputDoc.Load(temp);
                            }
                        }
                    }

#endif

                    //Start parsing
                    if (!inputReady) inputDoc = doc;
                    this.TryParseGraphset(inputDoc, handler);

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
                        //No catch actions - just cleaning up
                    }
                    //Wrap in a RDF Parse Exception
                    throw new RdfParseException("Unable to Parse this TriX since System.Xml was unable to parse the document into a DOM Tree, see the inner exception for details", xmlEx);
                }
                catch
                {
                    try
                    {
                        input.Close();
                    }
                    catch
                    {
                        //No catch actions - just cleaning up
                    }
                    throw;
                }
            }
            else
            {
                throw new RdfStorageException("Parameters for the TriXParser must be of the type StreamParams/TextReaderParams");
            }
        }

        private void TryParseGraphset(XmlDocument doc, IRdfHandler handler)
        {
            try
            {
                handler.StartRdf();

                XmlElement graphsetEl = doc.DocumentElement;
                if (!graphsetEl.Name.Equals("TriX"))
                {
                    throw new RdfParseException("Unexpected Document Element '" + graphsetEl.Name + "' encountered, expected the Document Element of a TriX Document to be the <TriX> element");
                }

                if (!graphsetEl.HasAttributes)
                {
                    throw new RdfParseException("<TriX> fails to define any attributes, the element must define the xmlns attribute to be the TriX namespace");
                }
                else
                {
                    bool trixNSDefined = false;
                    foreach (XmlAttribute attr in graphsetEl.Attributes)
                    {
                        if (attr.Name.Equals("xmlns"))
                        {
                            //Ensure that the xmlns attribute is defined and is the TriX namespace
                            if (trixNSDefined) throw new RdfParseException("The xmlns attribute can only occur once on the <TriX> element");
                            if (!attr.Value.Equals(TriXNamespaceURI)) throw new RdfParseException("The xmlns attribute of the <TriX> element must have it's value set to the TriX Namespace URI which is '" + TriXNamespaceURI + "'");
                            trixNSDefined = true;
                        }
                        else if (attr.LocalName.Equals("xmlns"))
                        {
                            //Don't think we need to do anything here
                        }
                    }

                    if (!trixNSDefined) throw new RdfParseException("The <TriX> element fails to define the required xmlns attribute defining the TriX Namespace");
                }

                //Process Child Nodes
                foreach (XmlNode graph in graphsetEl.ChildNodes)
                {
                    //Ignore non-Element nodes
                    if (graph.NodeType == XmlNodeType.Element)
                    {
                        //OPT: Do this multi-threaded?
                        this.TryParseGraph(graph, handler);
                    }
                }

                handler.EndRdf(true);
            }
            catch (RdfParsingTerminatedException)
            {
                handler.EndRdf(true);
                //Discard this - it justs means the Handler told us to stop
            }
            catch
            {
                handler.EndRdf(false);
                throw;
            }
        }

        private void TryParseGraph(XmlNode graphEl, IRdfHandler handler)
        {
            //Ensure Node Name is correct
            if (!graphEl.Name.Equals("graph"))
            {
                throw new RdfParseException("Unexpected Element <" + graphEl.Name + "> encountered, only <graph> elements are permitted within a <TriX> element");
            }

            //Check whether this Graph is actually asserted
            if (graphEl.Attributes.GetNamedItem("asserted") != null)
            {
                bool asserted = true;
                if (Boolean.TryParse(graphEl.Attributes["asserted"].Value, out asserted))
                {
                    //Don't process this Graph further if it is not being asserted
                    //i.e. it is only (potentially) being quoted
                    if (!asserted)
                    {
                        this.RaiseWarning("A Graph is marked as not asserted in the TriX input.  This Graph will not be parsed, if you reserialize the input the information contained in it will not be preserved");
                        return;
                    }
                }
            }

            //See if we get an <id>/<uri> node to name the Graph
            bool skipFirst = true;
            XmlNode nameEl = graphEl.FirstChild;

            //Watch out for Comments and other non-Element nodes
            if (nameEl.NodeType != XmlNodeType.Element)
            {
                foreach (XmlNode n in graphEl.ChildNodes)
                {
                    if (n.NodeType == XmlNodeType.Element)
                    {
                        nameEl = n;
                        break;
                    }
                }
            }

            //Process the name into a Graph Uri and create the Graph and add it to the Store
            Uri graphUri;
            if (nameEl.Name.Equals("uri"))
            {
                //TODO: Add support for reading Base Uri from xml:base attributes in the file
                graphUri = new Uri(Tools.ResolveUri(nameEl.InnerText, String.Empty));
            }
            else
            {
                skipFirst = false;
                graphUri = null;
            }

            //Process the Child Nodes of the <graph> element to yield Triples
            foreach (XmlNode triple in graphEl.ChildNodes)
            {
                //Remember to ignore anything that isn't an element i.e. comments and processing instructions
                if (triple.NodeType == XmlNodeType.Element)
                {
                    if (skipFirst)
                    {
                        skipFirst = false;
                        continue;
                    }
                    this.TryParseTriple(triple, handler, graphUri);
                }
            }
        }

        private void TryParseTriple(XmlNode tripleEl, IRdfHandler handler, Uri graphUri)
        {
            //Verify Node Name
            if (!tripleEl.Name.Equals("triple"))
            {
                throw new RdfParseException("Unexpected Element <" + tripleEl.Name + "> encountered, only an optional <uri> element followed by zero/more <triple> elements are permitted within a <graph> element");
            }
            //Verify number of Child Nodes
            if (!tripleEl.HasChildNodes) throw new RdfParseException("<triple> element has no child nodes - 3 child nodes are expected");
            if (tripleEl.ChildNodes.Count < 3) throw new RdfParseException("<triple> element has too few child nodes (" + tripleEl.ChildNodes.Count + ") - 3 child nodes are expected");
            if (tripleEl.ChildNodes.Count > 3) throw new RdfParseException("<triple> element has too many child nodes (" + tripleEl.ChildNodes.Count + ") - 3 child nodes are expected");

            //Get the 3 Child Nodes
            XmlNode subjEl, predEl, objEl;
            subjEl = tripleEl.ChildNodes[0];
            predEl = tripleEl.ChildNodes[1];
            objEl = tripleEl.ChildNodes[2];

            //Parse XML Nodes into RDF Nodes
            INode subj, pred, obj;
            if (subjEl.Name.Equals("uri"))
            {
                subj = handler.CreateUriNode(new Uri(subjEl.InnerText));
            }
            else if (subjEl.Name.Equals("id"))
            {
                subj = handler.CreateBlankNode(subjEl.InnerText);
            }
            else
            {
                throw Error("Unexpected element <" + subjEl.Name + "> encountered, expected a <id>/<uri> element as the Subject of a Triple", subjEl);
            }

            if (predEl.Name.Equals("uri"))
            {
                pred = handler.CreateUriNode(new Uri(predEl.InnerText));
            }
            else
            {
                throw Error("Unexpected element <" + predEl.Name + "> encountered, expected a <uri> element as the Predicate of a Triple", subjEl);
            }

            if (objEl.Name.Equals("uri"))
            {
                obj = handler.CreateUriNode(new Uri(objEl.InnerText));
            }
            else if (objEl.Name.Equals("id"))
            {
                obj = handler.CreateBlankNode(objEl.InnerText);
            }
            else if (objEl.Name.Equals("plainLiteral"))
            {
                if (objEl.Attributes.GetNamedItem("xml:lang") != null)
                {
                    obj = handler.CreateLiteralNode(objEl.InnerText, objEl.Attributes["xml:lang"].Value);
                }
                else
                {
                    obj = handler.CreateLiteralNode(objEl.InnerText);
                }
            }
            else if (objEl.Name.Equals("typedLiteral"))
            {
                if (objEl.Attributes.GetNamedItem("datatype") != null)
                {
                    Uri dtUri = new Uri(objEl.Attributes["datatype"].Value);
                    if (objEl.FirstChild.NodeType == XmlNodeType.Text)
                    {
                        obj = handler.CreateLiteralNode(objEl.InnerText, dtUri);
                    }
                    else if (objEl.FirstChild.NodeType == XmlNodeType.CDATA)
                    {
                        obj = handler.CreateLiteralNode(objEl.FirstChild.InnerXml, dtUri);
                    }
                    else
                    {
                        obj = handler.CreateLiteralNode(objEl.InnerText, dtUri);
                    }
                }
                else
                {
                    throw Error("<typedLiteral> element does not have the required datatype attribute", objEl);
                }
            }
            else
            {
                throw Error("Unexpected element <" + objEl.Name + "> encountered, expected a <id>/<uri>/<plainLiteral>/<typedLiteral> element as the Object of a Triple", subjEl);
            }

            //Assert the resulting Triple
            if (!handler.HandleTriple(new Triple(subj, pred, obj, graphUri))) ParserHelper.Stop(); ;
        }

        /// <summary>
        /// Helper method for raising informative standardised Parser Errors
        /// </summary>
        /// <param name="msg">The Error Message</param>
        /// <param name="n">The Node that is the cause of the Error</param>
        /// <returns></returns>
        private RdfParseException Error(String msg, XmlNode n)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("[Source XML]");
            output.AppendLine(n.OuterXml);
            output.AppendLine();
            output.Append(msg);

            return new RdfParseException(output.ToString());
        }

#else
        private XmlReaderSettings GetSettings()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
#if SILVERLIGHT
            settings.DtdProcessing = DtdProcessing.Parse;
#else
            settings.ProhibitDtd = false;
#endif
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            return settings;
        }

        /// <summary>
        /// Loads the named Graphs from the NQuads input into the given Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="parameters">Parameters indicating the Stream to read from</param>
        public void Load(ITripleStore store, IStoreParams parameters)
        {
            if (store == null) throw new RdfParseException("Cannot read a RDF dataset into a null Store");
            this.Load(new StoreHandler(store), parameters);
        }

        public void Load(IRdfHandler handler, IStoreParams parameters)
        {
            if (handler == null) throw new ArgumentNullException("handler", "Cannot parse an RDF Dataset using a null RDF Handler");
            if (parameters == null) throw new ArgumentNullException("parameters", "Cannot parse an RDF Dataset using null Parameters");

            //Try and get the Input from the parameters
            TextReader input = null;
            if (parameters is StreamParams)
            {
                //Get Input Stream
                input = ((StreamParams)parameters).StreamReader;
            }
            else if (parameters is TextReaderParams)
            {
                input = ((TextReaderParams)parameters).TextReader;
            }

            if (input != null)
            {
                //First try and load as XML and apply any stylesheets
                try
                {
                    //Get the reader and start parsing
                    XmlReader reader = XmlReader.Create(input, this.GetSettings());
                    this.RaiseWarning("The TriX Parser is operating without XSL support, if your TriX file requires XSL then it will not be parsed successfully");
                    this.TryParseGraphset(reader, handler);

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
                        //No catch actions - just cleaning up
                    }
                    //Wrap in a RDF Parse Exception
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
                        //No catch actions - just cleaning up
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

                //Skip XML Declaration if present
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
                            //Ensure that the xmlns attribute is defined and is the TriX namespace
                            if (trixNSDefined) throw new RdfParseException("The xmlns attribute can only occur once on the <TriX> element");
                            if (!reader.Value.Equals(TriXNamespaceURI)) throw new RdfParseException("The xmlns attribute of the <TriX> element must have it's value set to the TriX Namespace URI which is '" + TriXNamespaceURI + "'");
                            trixNSDefined = true;
                        }
                        else if (reader.LocalName.Equals("xmlns"))
                        {
                            //Don't think we need to do anything here
                        }
                    }

                    if (!trixNSDefined) throw new RdfParseException("The <TriX> element fails to define the required xmlns attribute defining the TriX Namespace");
                }

                //Process Child Nodes
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        //For elements we recurse to try and parse a Graph
                        this.TryParseGraph(reader, handler);
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        //Stop when we hit an end element
                        break;
                    }
                    else
                    {
                        //Stop if we hit an unexpected element
                        break;
                    }
                }

                //Expect the </TriX> element
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
                //Discard this - it justs means the Handler told us to stop
            }
            catch
            {
                handler.EndRdf(false);
                throw;
            }
        }

        private void TryParseGraph(XmlReader reader, IRdfHandler handler)
        {
            //Ensure Node Name is correct
            if (!reader.Name.Equals("graph"))
            {
                throw new RdfParseException("Unexpected Element <" + reader.Name + "> encountered, only <graph> elements are permitted within a <TriX> element");
            }

            //Check whether this Graph is actually asserted
            for (int i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToNextAttribute();
                if (reader.Name.Equals("asserted"))
                {
                    bool asserted = true;
                    if (Boolean.TryParse(reader.Value, out asserted))
                    {
                        //Don't process this Graph further if it is not being asserted
                        //i.e. it is only (potentially) being quoted
                        if (!asserted)
                        {
                            this.RaiseWarning("A Graph is marked as not asserted in the TriX input.  This Graph will not be parsed, if you reserialize the input the information contained in it will not be preserved");
                            return;
                        }
                    }
                }
            }

            //See if we get an <id>/<uri> node to name the Graph
            reader.Read();
            IGraph g = null;
            Uri graphUri = null;
            if (reader.NodeType == XmlNodeType.Element)
            {
                //Process the name into a Graph Uri and create the Graph and add it to the Store
                if (reader.Name.Equals("uri"))
                {
                    //TODO: Add support for reading Base Uri from xml:base attributes in the file
                    graphUri = new Uri(Tools.ResolveUri(reader.ReadInnerXml(), String.Empty));
                }
            }

            //If the next element is a </graph> then this is an empty graph and we should return
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

            //Process the Child Nodes of the <graph> element to yield Triples
            do
            {
                //Remember to ignore anything that isn't an element i.e. comments and processing instructions
                if (reader.NodeType == XmlNodeType.Element)
                {
                    this.TryParseTriple(reader, handler, graphUri);
                }

                reader.Read();
            } while (reader.NodeType != XmlNodeType.EndElement);

            if (!reader.Name.Equals("graph")) throw Error("Expected a </graph> but a </" + reader.Name + "> was encountered", reader);
        }

        private void TryParseTriple(XmlReader reader, IRdfHandler handler, Uri graphUri)
        {
            //Verify Node Name
            if (!reader.Name.Equals("triple"))
            {
                throw Error("Unexpected Element <" + reader.Name + "> encountered, only an optional <id>/<uri> element followed by zero/more <triple> elements are permitted within a <graph> element", reader);
            }

            //Parse XML Nodes into RDF Nodes
            INode subj, pred, obj;
            subj = this.TryParseNode(reader, handler, TripleSegment.Subject);
            pred = this.TryParseNode(reader, handler, TripleSegment.Predicate);
            obj = this.TryParseNode(reader, handler, TripleSegment.Object);

            if (reader.NodeType != XmlNodeType.EndElement) throw Error("Unexpected element type " + reader.NodeType.ToString() + " encountered, expected the </triple> element", reader);
            if (!reader.Name.Equals("triple")) throw Error("Unexpected </" + reader.Name + "> encountered, expected a </triple> element", reader);

            //Assert the resulting Triple
            if (!handler.HandleTriple(new Triple(subj, pred, obj, graphUri))) ParserHelper.Stop();
        }

        private INode TryParseNode(XmlReader reader, IRdfHandler handler, TripleSegment segment)
        {
            //Only need to Read() if getting the Subject
            //The previous calls will have resulted in us already reading to the start element for this node
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
                        return handler.CreateLiteralNode(reader.ReadInnerXml(), lang);
                    }
                    else
                    {
                        return handler.CreateLiteralNode(reader.ReadInnerXml());
                    }
                }
                else
                {
                    return handler.CreateLiteralNode(reader.ReadInnerXml());
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
                    return handler.CreateLiteralNode(reader.ReadInnerXml(), dtUri);
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
        /// <param name="n">The Node that is the cause of the Error</param>
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
#endif

        /// <summary>
        /// Helper method used to raise the Warning event if there is an event handler registered
        /// </summary>
        /// <param name="message">Warning message</param>
        private void RaiseWarning(String message)
        {
            StoreReaderWarning d = this.Warning;
            if (d != null)
            {
                d(message);
            }
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
