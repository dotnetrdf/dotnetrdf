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
using System.Linq;
using System.Xml;
using VDS.RDF.Parsing.Contexts;

namespace VDS.RDF.Parsing.Events.RdfXml
{
    /// <summary>
    /// A DOM Based event generator for RDF/XML parser that uses System.Xml DOM to parse events
    /// </summary>
    public class DomBasedEventGenerator : IRdfXmlPreProcessingEventGenerator
    {
        private XmlDocument _document;

        /// <summary>
        /// Creates a new DOM Based event generator
        /// </summary>
        /// <param name="document">XML Document</param>
        public DomBasedEventGenerator(XmlDocument document)
        {
            _document = document;
        }

        /// <summary>
        /// Creates a new DOM Based event generator
        /// </summary>
        /// <param name="input">Input Stream</param>
        public DomBasedEventGenerator(StreamReader input)
        {
            _document = new XmlDocument();
            _document.Load(input);
        }

        /// <summary>
        /// Creates a new DOM Based event generator
        /// </summary>
        /// <param name="file">Input File</param>
        public DomBasedEventGenerator(String file)
        {
            _document = new XmlDocument();
            _document.Load(File.OpenRead(file));
        }

        /// <summary>
        /// Gets all events from the XML DOM
        /// </summary>
        /// <param name="context">Parser Context</param>
        public void GetAllEvents(RdfXmlParserContext context)
        {
            RootEvent root;
            XmlNodeList nodes = _document.GetElementsByTagName("rdf:RDF");
            if (nodes.Count == 0)
            {
                // Not using rdf:RDF
                root = GenerateEventTree(context, _document.DocumentElement);
            }
            else if (nodes.Count > 1)
            {
                throw new RdfParseException("XML contains multiple rdf:RDF Nodes and is therefore not a valid RDF/XML Document");
            }
            else
            {
                // Must thus be only 1 rdf:RDF element
                root = GenerateEventTree(context, nodes[0]);
            }

            FlattenEventTree(context, root, 0);
        }

        /// <summary>
        /// Given an XML Node that is the Root of the RDF/XML section of the Document Tree creates the RootEvent and generates the rest of the Event Tree by recursive calls to the <see cref="GenerateEvents">GenerateEvents</see> method
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="docEl">XML Node that is the Root of the RDF/XML section of the Document Tree</param>
        /// <returns></returns>
        private RootEvent GenerateEventTree(RdfXmlParserContext context, XmlNode docEl)
        {
            // Get the Document Element
            // XmlNode docEl = document.DocumentElement;

            // Generate a Root Event and Element Event from it
            RootEvent root = new RootEvent(docEl.BaseURI, docEl.OuterXml);
            if (docEl.BaseURI.Equals(String.Empty))
            {
                if (context.BaseUri != null)
                {
                    root.BaseUri = context.BaseUri.AbsoluteUri;
                }
            }
            ElementEvent element = new ElementEvent(docEl.LocalName, docEl.Prefix, root.BaseUri, docEl.OuterXml);

            // Initialise Language Settings
            root.Language = String.Empty;
            element.Language = root.Language;

            // Set as Document Element and add as only Child
            root.DocumentElement = element;
            root.Children.Add(element);

        #region Attribute Processing

            // Go through attributes looking for XML Namespace Declarations
            foreach (XmlAttribute attr in docEl.Attributes)
            {
                if (attr.Prefix.Equals("xmlns") || attr.Name == "xmlns")
                {
                    // Define a Namespace
                    String prefix = attr.LocalName;
                    if (prefix.Equals("xmlns")) prefix = String.Empty;
                    String uri;
                    if (attr.Value.StartsWith("http://"))
                    {
                        // Absolute Uri
                        uri = attr.Value;
                    }
                    else if (!root.BaseUri.Equals(String.Empty))
                    {
                        // Relative Uri with a Base Uri to resolve against
                        // uri = root.BaseUri + attr.Value;
                        uri = Tools.ResolveUri(attr.Value, root.BaseUri);
                    }
                    else
                    {
                        // Relative Uri with no Base Uri
                        throw new RdfParseException("Cannot resolve a Relative Namespace URI since there is no in-scope Base URI");
                    }
                    context.Namespaces.AddNamespace(prefix, UriFactory.Create(uri));
                }
                else if (attr.Name == "xml:base")
                {
                    // Set the Base Uri
                    String baseUri = attr.Value;

                    if (RdfXmlSpecsHelper.IsAbsoluteURI(baseUri))
                    {
                        // Absolute Uri
                        root.BaseUri = baseUri;
                    }
                    else if (!element.BaseUri.Equals(String.Empty))
                    {
                        // Relative Uri with a Base Uri to resolve against
                        // root.BaseUri += baseUri;
                        root.BaseUri = Tools.ResolveUri(baseUri, root.BaseUri);
                    }
                    else
                    {
                        // Relative Uri with no Base Uri
                        throw new RdfParseException("Cannot resolve a Relative Base URI since there is no in-scope Base URI");
                    }
                    element.BaseUri = root.BaseUri;
                }
            }

        #endregion

            // Iterate over Children
            foreach (XmlNode child in docEl.ChildNodes)
            {
                // Ignore Irrelevant Node Types
                if (IsIgnorableNode(child))
                {
                    continue;
                }

                // Generate an Event for the Child Node
                ElementEvent childEvent = GenerateEvents(context, child, element);
                element.Children.Add(childEvent);
            }

            // Return the Root Event
            return root;
        }

        /// <summary>
        /// Given an XML Node creates the relevant RDF/XML Events for it and recurses as necessary
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="node">The Node to create Event(s) from</param>
        /// <param name="parent">The Parent Node of the given Node</param>
        /// <returns></returns>
        private ElementEvent GenerateEvents(RdfXmlParserContext context, XmlNode node, IRdfXmlEvent parent)
        {
            // Get the Base Uri
            String baseUri = String.Empty;
            if (parent is ElementEvent)
            {
                baseUri = ((ElementEvent)parent).BaseUri;
            }
            // Create an ElementEvent for the Node
            ElementEvent element = new ElementEvent(node.LocalName, node.Prefix, baseUri, node.OuterXml);

            // Set the initial Language from the Parent
            ElementEvent parentEl = (ElementEvent)parent;
            element.Language = parentEl.Language;

        #region Attribute Processing

            // Iterate over Attributes
            bool parseTypeLiteral = false;
            foreach (XmlAttribute attr in node.Attributes)
            {
                // Watch out for special attributes
                if (attr.Name == "xml:lang")
                {
                    // Set Language
                    element.Language = attr.Value;
                }
                else if (attr.Name == "xml:base")
                {
                    // Set Base Uri

                    if (RdfXmlSpecsHelper.IsAbsoluteURI(attr.Value))
                    {
                        // Absolute Uri
                        element.BaseUri = attr.Value;
                    }
                    else if (!element.BaseUri.Equals(String.Empty))
                    {
                        // Relative Uri with a Base Uri to resolve against
                        // element.BaseUri += attr.Value;
                        element.BaseUri = Tools.ResolveUri(attr.Value, element.BaseUri);
                    }
                    else
                    {
                        // Relative Uri with no Base Uri
                        throw new RdfParseException("Cannot resolve a Relative Base URI since there is no in-scope Base URI");
                    }
                }
                else if (attr.Prefix == "xmlns")
                {
                    // Register a Namespace
                    String uri;
                    if (attr.Value.StartsWith("http://"))
                    {
                        // Absolute Uri
                        uri = attr.Value;
                    }
                    else if (!element.BaseUri.Equals(String.Empty))
                    {
                        // Relative Uri with a Base Uri to resolve against
                        // uri = element.BaseUri + attr.Value;
                        uri = Tools.ResolveUri(attr.Value, element.BaseUri);
                    }
                    else
                    {
                        // Relative Uri with no Base Uri
                        throw new RdfParseException("Cannot resolve a Relative Namespace URI since there is no in-scope Base URI");
                    }
                    NamespaceAttributeEvent ns = new NamespaceAttributeEvent(attr.LocalName, uri, attr.OuterXml);
                    element.NamespaceAttributes.Add(ns);
                }
                else if (attr.Prefix == String.Empty && attr.Name == "xmlns")
                {
                    // Register a Default Namespace (Empty Prefix)
                    String uri;

                    if (attr.Value.StartsWith("http://"))
                    {
                        // Absolute Uri
                        uri = attr.Value;
                    }
                    else if (!element.BaseUri.Equals(String.Empty))
                    {
                        // Relative Uri with a Base Uri to resolve against
                        // uri = element.BaseUri + attr.Value;
                        uri = Tools.ResolveUri(attr.Value, element.BaseUri);
                    }
                    else
                    {
                        // Relative Uri with no Base Uri
                        throw new RdfParseException("Cannot resolve a Relative Namespace URI since there is no in-scope Base URI");
                    }
                    NamespaceAttributeEvent ns = new NamespaceAttributeEvent(String.Empty, uri, attr.OuterXml);
                    element.NamespaceAttributes.Add(ns);
                }
                else if (attr.Prefix == "xml" || (attr.Prefix == String.Empty && attr.LocalName.StartsWith("xml")))
                {
                    // Ignore other Reserved XML Names
                }
                else if (RdfXmlSpecsHelper.IsRdfNamespace(attr.NamespaceURI) && attr.LocalName == "parseType" && attr.Value == "Literal")
                {
                    // Literal Parse Type
                    parseTypeLiteral = true;

                    // Create the Attribute
                    AttributeEvent attrEvent = new AttributeEvent(attr.LocalName, attr.Prefix, attr.Value, attr.OuterXml);
                    element.Attributes.Add(attrEvent);

                    // Set ParseType property correctly
                    element.ParseType = RdfXmlParseType.Literal;
                }
                else if (RdfXmlSpecsHelper.IsRdfNamespace(attr.NamespaceURI) && attr.LocalName == "parseType")
                {
                    // Some other Parse Type

                    // Create the Attribute
                    AttributeEvent attrEvent = new AttributeEvent(attr.LocalName, attr.Prefix, attr.Value, attr.OuterXml);
                    element.Attributes.Add(attrEvent);

                    // Set the ParseType property correctly
                    if (attr.Value == "Resource")
                    {
                        element.ParseType = RdfXmlParseType.Resource;
                    }
                    else if (attr.Value == "Collection")
                    {
                        element.ParseType = RdfXmlParseType.Collection;
                    }
                    else
                    {
                        // Have to assume Literal
                        element.ParseType = RdfXmlParseType.Literal;
                        parseTypeLiteral = true;

                        // Replace the Parse Type attribute with one saying it is Literal
                        element.Attributes.Remove(attrEvent);
                        attrEvent = new AttributeEvent(attr.LocalName, attr.Prefix, "Literal", attr.OuterXml);
                    }
                }
                else
                {
                    // Normal Attribute which we generate an Event from
                    AttributeEvent attrEvent = new AttributeEvent(attr.LocalName, attr.Prefix, attr.Value, attr.OuterXml);
                    element.Attributes.Add(attrEvent);
                }
            }

            // Validate generated Attributes for Namespace Confusion and URIRef encoding
            foreach (AttributeEvent a in element.Attributes)
            {
                // Namespace Confusion should only apply to Attributes without a Namespace specified
                if (a.Namespace.Equals(string.Empty) && element.NamespaceAttributes.All(na => na.Prefix != string.Empty) && !context.Namespaces.HasNamespace(string.Empty))
                {
                    if (RdfXmlSpecsHelper.IsAmbigiousAttributeName(a.LocalName))
                    {
                        // Can't use any of the RDF terms that mandate the rdf: prefix without it
                        throw ParserHelper.Error("An Attribute with an ambiguous name '" + a.LocalName + "' was encountered.  The following attribute names MUST have the rdf: prefix - about, aboutEach, ID, bagID, type, resource, parseType", element);
                    }
                }

                // URIRef encoding check
                if (!RdfXmlSpecsHelper.IsValidUriRefEncoding(a.Value))
                {
                    throw ParserHelper.Error("An Attribute with an incorrectly encoded URIRef was encountered, URIRef's must be encoded in Unicode Normal Form C", a);
                }
            }

        #endregion

            // Don't proceed if Literal Parse Type is on
            if (parseTypeLiteral)
            {
                // Generate an XMLLiteral from its Inner Xml (if any)
                TypedLiteralEvent lit = new TypedLiteralEvent(node.InnerXml.Normalize(), RdfSpecsHelper.RdfXmlLiteral, node.InnerXml);
                element.Children.Add(lit);
                return element;
            }

            // Are there Child Nodes?
            if (node.HasChildNodes)
            {
                // Take different actions depending on the Number and Type of Child Nodes
                if (node.ChildNodes.Count > 1)
                {
                    // Multiple Child Nodes

                    // Iterate over Child Nodes
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        // Ignore Irrelevant Node Types
                        if (IsIgnorableNode(child))
                        {
                            continue;
                        }

                        // Generate an Event for the Child Node
                        ElementEvent childEvent = GenerateEvents(context, child, element);
                        element.Children.Add(childEvent);
                    }
                }
                else if (node.ChildNodes[0].NodeType == XmlNodeType.Text)
                {
                    // Single Child which is a Text Node

                    // Generate a Text Event
                    TextEvent text = new TextEvent(node.InnerText.Normalize(), node.OuterXml);
                    element.Children.Add(text);
                }
                else if (node.ChildNodes[0].NodeType == XmlNodeType.CDATA)
                {
                    // Single Child which is a CData Node

                    TextEvent text = new TextEvent(node.InnerXml.Normalize(), node.OuterXml);
                    element.Children.Add(text);
                }
                else
                {
                    // Single Child which is not a Text Node

                    // Recurse on the single Child Node
                    if (!IsIgnorableNode(node.ChildNodes[0]))
                    {
                        ElementEvent childEvent = GenerateEvents(context, node.ChildNodes[0], element);
                        element.Children.Add(childEvent);
                    }
                }
            }

            return element;
        }

        /// <summary>
        /// Checks whether a given XML Node can be discarded as it does not have any equivalent Event in the RDF/XML Syntax model
        /// </summary>
        /// <param name="node">XML Node to test</param>
        /// <returns>True if the Node can be ignored</returns>
        /// <remarks>Comment and Text Nodes are ignored.  Text Nodes will actually become Text Events but we'll access the Text using the InnerText property of the Element Nodes instead</remarks>
        private bool IsIgnorableNode(XmlNode node)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Comment:
                case XmlNodeType.Text:
                case XmlNodeType.ProcessingInstruction:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Takes the Event Tree and Flattens it into a Queue as per the rules laid out in the RDF/XML Specification
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="evt">Event which is the Root of the Tree (not necessarily a RootEvent)</param>
        /// <param name="nesting">A numeric value used for Parser Tracing to indicate nesting levels of the Event Tree</param>
        private void FlattenEventTree(RdfXmlParserContext context, IRdfXmlEvent evt, int nesting)
        {
            // Add to Queue
            context.Events.Enqueue(evt);

            if (context.TraceParsing)
            {
                Console.Write(nesting + " " + evt.GetType().ToString());
            }

            // Iterate over Children where present
            if (evt is RootEvent)
            {
                RootEvent root = (RootEvent)evt;
                if (context.TraceParsing)
                {
                    Console.WriteLine("");
                }
                foreach (IRdfXmlEvent childEvent in root.Children)
                {
                    FlattenEventTree(context, childEvent, nesting + 1);
                }

                // No End after a RootEvent
                return;
            }
            else if (evt is ElementEvent)
            {
                ElementEvent element = (ElementEvent)evt;
                if (context.TraceParsing)
                {
                    Console.WriteLine(" " + element.Namespace + ":" + element.LocalName);
                }
                if (element.Children.Count > 0)
                {
                    foreach (IRdfXmlEvent childEvent in element.Children)
                    {
                        FlattenEventTree(context, childEvent, nesting + 1);
                    }
                }
            }
            else if (evt is TextEvent)
            {
                TextEvent text = (TextEvent)evt;
                if (context.TraceParsing)
                {
                    Console.WriteLine(" " + text.Value);
                }

                // No additional End after a Text Event
                return;
            }
            else if (evt is TypedLiteralEvent)
            {
                TypedLiteralEvent tlit = (TypedLiteralEvent)evt;
                if (context.TraceParsing)
                {
                    Console.WriteLine();
                }

                // No additional End after a Text Event
                return;
            }

            // Add an End Element Event to the Queue
            EndElementEvent end = new EndElementEvent();
            context.Events.Enqueue(end);
            if (context.TraceParsing)
            {
                String endDescrip = String.Empty;
                if (evt is ElementEvent)
                {
                    ElementEvent temp = (ElementEvent)evt;
                    endDescrip = " " + temp.QName;
                }
                Console.WriteLine(nesting + " " + end.GetType().ToString() + endDescrip);
            }
        }
    }
    }
