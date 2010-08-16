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

#if !NO_XMLDOM

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
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;

//REQ: Implement an XmlReader based version of the TrixParser

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

        /// <summary>
        /// Loads the named Graphs from the NQuads input into the given Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="parameters">Parameters indicating the Stream to read from</param>
        public void Load(ITripleStore store, IStoreParams parameters)
        {
            if (parameters is StreamParams)
            {
                //Get Input Stream
                StreamReader input = ((StreamParams)parameters).StreamReader;

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
                    this.TryParseGraphset(inputDoc, store);

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
                    throw new RdfParseException("Unable to Parse this TriX since System.Xml was unable to parse the document into a DOM Tree", xmlEx);
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
                }
            }
            else
            {
                throw new RdfStorageException("Parameters for the TriXParser must be of type StreamParams");
            }
        }

        private void TryParseGraphset(XmlDocument doc, ITripleStore store)
        {
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
                    this.TryParseGraph(graph, store);
                }
            }
        }

        private void TryParseGraph(XmlNode graphEl, ITripleStore store)
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
                        this.OnWarning("A Graph is marked as not asserted in the TriX input.  This Graph will not be parsed, if you reserialize the input the information contained in it will not be preserved");
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
            IGraph g;
            if (nameEl.Name.Equals("uri"))
            {
                //TODO: Add support for reading Base Uri from xml:base attributes in the file
                Uri graphUri = new Uri(Tools.ResolveUri(nameEl.InnerText, String.Empty));
                if (store.HasGraph(graphUri))
                {
                    throw new RdfParseException("Cannot parse a Graph with the URI '" + graphUri.ToString() + "' since this Graph already exists in the Store");
                }
                g = new Graph();
                g.BaseUri = graphUri;
                store.Add(g);
            }
            else if (nameEl.Name.Equals("id"))
            {
                Uri localUri = new Uri("trix:local:" + nameEl.InnerText);
                if (store.HasGraph(localUri))
                {
                    throw new RdfParseException("Cannot parse a Graph with the Local ID '" + nameEl.InnerText + "' since this Graph already exists in the Store");
                }
                g = new Graph();
                g.BaseUri = localUri;
                store.Add(g);
            }
            else
            {
                skipFirst = false;

                //Create the Default Graph - error if already exists
                if (store.HasGraph(new Uri(TriXParser.DefaultGraphURI)))
                {
                    throw new RdfParseException("Cannot parse an unamed Graph since there is already a default Graph in the Store");
                }
                g = new Graph();
                g.BaseUri = new Uri(TriXParser.DefaultGraphURI);
                store.Add(g);
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
                    this.TryParseTriple(triple, g);
                }
            }
        }

        private void TryParseTriple(XmlNode tripleEl, IGraph g)
        {
            //Verify Node Name
            if (!tripleEl.Name.Equals("triple"))
            {
                throw new RdfParseException("Unexpected Element <" + tripleEl.Name + "> encountered, only an optional <id>/<uri> element followed by zero/more <triple> elements are permitted within a <graph> element");
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
                subj = g.CreateUriNode(new Uri(subjEl.InnerText));
            }
            else if (subjEl.Name.Equals("id"))
            {
                subj = g.CreateBlankNode(subjEl.InnerText);
            }
            else
            {
                throw Error("Unexpected element <" + subjEl.Name + "> encountered, expected a <id>/<uri> element as the Subject of a Triple", subjEl);
            }

            if (predEl.Name.Equals("uri"))
            {
                pred = g.CreateUriNode(new Uri(predEl.InnerText));
            }
            else
            {
                throw Error("Unexpected element <" + predEl.Name + "> encountered, expected a <uri> element as the Predicate of a Triple", subjEl);
            }

            if (objEl.Name.Equals("uri"))
            {
                obj = g.CreateUriNode(new Uri(objEl.InnerText));
            }
            else if (objEl.Name.Equals("id"))
            {
                obj = g.CreateBlankNode(objEl.InnerText);
            }
            else if (objEl.Name.Equals("plainLiteral"))
            {
                if (objEl.Attributes.GetNamedItem("xml:lang") != null)
                {
                    obj = g.CreateLiteralNode(objEl.InnerText, objEl.Attributes["xml:lang"].Value);
                }
                else
                {
                    obj = g.CreateLiteralNode(objEl.InnerText);
                }
            }
            else if (objEl.Name.Equals("typedLiteral"))
            {
                if (objEl.Attributes.GetNamedItem("datatype") != null)
                {
                    Uri dtUri = new Uri(objEl.Attributes["datatype"].Value);
                    if (objEl.FirstChild.NodeType == XmlNodeType.Text)
                    {
                        obj = g.CreateLiteralNode(objEl.InnerText, dtUri);
                    }
                    else if (objEl.FirstChild.NodeType == XmlNodeType.CDATA)
                    {
                        obj = g.CreateLiteralNode(objEl.FirstChild.InnerXml, dtUri);
                    }
                    else
                    {
                        obj = g.CreateLiteralNode(objEl.InnerText, dtUri);
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
            g.Assert(new Triple(subj, pred, obj));
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

        /// <summary>
        /// Helper method used to raise the Warning event if there is an event handler registered
        /// </summary>
        /// <param name="message">Warning message</param>
        private void OnWarning(String message)
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
    }
}

#endif