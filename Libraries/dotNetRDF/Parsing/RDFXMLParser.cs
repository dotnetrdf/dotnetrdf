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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Events;
using VDS.RDF.Parsing.Events.RdfXml;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Possible RDF/XML Parse Types
    /// </summary>
    public enum RdfXmlParseType : int
    {
        /// <summary>
        /// No specific Parse Type is specified (Default Parsing Rules will be used)
        /// </summary>
        None = -1,
        /// <summary>
        /// Literal Parse Type
        /// </summary>
        Literal = 0,
        /// <summary>
        /// Resource Parse Type
        /// </summary>
        Resource = 1,
        /// <summary>
        /// Collection Parse Type
        /// </summary>
        Collection = 2,
        /// <summary>
        /// Other Parse Type
        /// </summary>
        /// <remarks>This is never used since any other Parse Type encountered is assumed to be Literal as per the RDF/XML Specification</remarks>
        Other = 3
    }

    /// <summary>
    /// Possible RDF/XML Parser Modes
    /// </summary>
    public enum RdfXmlParserMode
    {
        /// <summary>
        /// Uses DOM Based parsing (not fully supported under .NET Standard/Core)
        /// </summary>
        DOM,
        /// <summary>
        /// Uses Streaming Based parsing (default)
        /// </summary>
        Streaming
    }

    /// <summary>
    /// Parser for RDF/XML syntax
    /// </summary>
    public class RdfXmlParser
        : IRdfReader, ITraceableParser
    {

        #region Variables and Properties

        private bool _traceparsing = false;
        private RdfXmlParserMode _mode = RdfXmlParserMode.Streaming;

        /// <summary>
        /// Controls whether Parser progress will be traced by writing output to the Console
        /// </summary>
        public bool TraceParsing
        {
            get
            {
                return _traceparsing;
            }
            set
            {
                _traceparsing = value;
            }
        }

        #endregion

        /// <summary>
        /// Creates a new RDF/XML Parser
        /// </summary>
        public RdfXmlParser()
        {

        }

        /// <summary>
        /// Creates a new RDF/XML Parser which uses the given parsing mode
        /// </summary>
        /// <param name="mode">RDF/XML Parse Mode</param>
        public RdfXmlParser(RdfXmlParserMode mode)
        {
            _mode = mode;
        }

        #region Load Method Implementations

        /// <summary>
        /// Reads RDF/XML syntax from some Stream into the given Graph
        /// </summary>
        /// <param name="g">Graph to create Triples in</param>
        /// <param name="input">Input Stream</param>
        public void Load(IGraph g, StreamReader input)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Reads RDF/XML syntax from some Input into the given Graph
        /// </summary>
        /// <param name="g">Graph to create Triples in</param>
        /// <param name="input">Input to read from</param>
        public void Load(IGraph g, TextReader input)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Reads RDF/XML syntax from some File into the given Graph
        /// </summary>
        /// <param name="g">Graph to create Triples in</param>
        /// <param name="filename">Filename of File containg XML/RDF</param>
        /// <remarks>Simply opens a Stream for the File then calls the other version of Load to do the actual parsing</remarks>
        public void Load(IGraph g, string filename)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");

            // Open a Stream for the File and call other variant of Load
            StreamReader input = new StreamReader(File.OpenRead(filename), Encoding.UTF8);
            Load(g, input);
        }

        /// <summary>
        /// Reads RDF/XML syntax from some Stream using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">Input Stream</param>
        public void Load(IRdfHandler handler, StreamReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannot read RDF from a null Stream");

            // Issue a Warning if the Encoding of the Stream is not UTF-8
            if (!input.CurrentEncoding.Equals(Encoding.UTF8))
            {
                RaiseWarning("Expected Input Stream to be encoded as UTF-8 but got a Stream encoded as " + input.CurrentEncoding.EncodingName + " - Please be aware that parsing errors may occur as a result");
            }

            Load(handler, (TextReader)input);
        }

        /// <summary>
        /// Reads RDF/XML syntax from some Input using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">Input to read from</param>
        public void Load(IRdfHandler handler, TextReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannot read RDF from a null TextReader");

            try
            {
                // Silverlight only supports XmlReader not the full XmlDocument API
                if (_mode == RdfXmlParserMode.DOM)
                {
                    // Load XML from Stream
                    XmlDocument doc = new XmlDocument();
                    doc.Load(input);

                    // Create a new Parser Context and Parse
                    RdfXmlParserContext context = new RdfXmlParserContext(handler, doc, _traceparsing);
                    Parse(context);
                }
                else
                {
                    RdfXmlParserContext context = new RdfXmlParserContext(handler, input);
                    Parse(context);
                }
            }
            catch (XmlException xmlEx)
            {
                // Wrap in a RDF Parse Exception
                throw new RdfParseException("Unable to Parse this RDF/XML since System.Xml was unable to parse the document, see Inner Exception for details of the XML exception that occurred", new PositionInfo(xmlEx.LineNumber, xmlEx.LinePosition), xmlEx);
            }
            catch (IOException ioEx)
            {
                // Wrap in a RDF Parse Exception
                throw new RdfParseException("Unable to Parse this RDF/XML due to an IO Exception, see Inner Exception for details of the IO exception that occurred", ioEx);
            }
            finally
            {
                try
                {
                    input.Close();
                }
                catch
                {
                    // Ignore exceptions here - just trying to clean up properly
                }
            }
        }

        /// <summary>
        /// Reads RDF/XML syntax from a file using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="filename">File to read from</param>
        public void Load(IRdfHandler handler, String filename)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");
            Load(handler, new StreamReader(File.OpenRead(filename), Encoding.UTF8));
        }

        /// <summary>
        /// Reads RDF/XML from the given XML Document
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="document">XML Document</param>
        public void Load(IGraph g, XmlDocument document)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            if (document == null) throw new RdfParseException("Cannot read RDF from a null XML Document");

            try 
            {
                // Create a new Parser Context and Parse
                RdfXmlParserContext context = new RdfXmlParserContext(g, document, _traceparsing);
                Parse(context);
            }
            catch (XmlException xmlEx)
            {
                // Wrap in a RDF Parse Exception
                throw new RdfParseException("Unable to Parse this RDF/XML since System.Xml was unable to parse the document into a DOM Tree", xmlEx);
            }
            catch (IOException ioEx)
            {
                // Wrap in a RDF Parse Exception
                throw new RdfParseException("Unable to Parse this RDF/XML due to an IO Exception", ioEx);
            }
            catch (Exception)
            {
                // Throw unexpected errors upwards as-is
                throw;
            }
        }

        #endregion

        /// <summary>
        /// Helper Method for raising the <see cref="RdfXmlParser.Warning">Warning</see> event
        /// </summary>
        /// <param name="warning">Warning Message</param>
        private void RaiseWarning(String warning)
        {
            RdfReaderWarning d = Warning;
            if (d != null)
            {
                d(warning);
            }
        }

        /// <summary>
        /// Event which Readers can raise when they notice syntax that is ambigious/deprecated etc which can still be parsed
        /// </summary>
        public event RdfReaderWarning Warning;

        /// <summary>
        /// Function which does the actual Parsing by invoking the various steps of the Parser
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void Parse(RdfXmlParserContext context)
        {
            try
            {
                context.Handler.StartRdf();

                // Trace Parser Information
                if (_traceparsing)
                {
                    Console.WriteLine("Trace Format is as follows:");
                    Console.WriteLine("NestingLevel EventType [Description]");
                    Console.WriteLine();
                }

                // Define XML namespace
                context.Handler.HandleNamespace("xml", UriFactory.Create(XmlSpecsHelper.NamespaceXml));
                context.Namespaces.AddNamespace("xml", UriFactory.Create(XmlSpecsHelper.NamespaceXml));

                // Process the Queue
                ProcessEventQueue(context);

                context.Handler.EndRdf(true);
            }
            catch (RdfParsingTerminatedException)
            {
                context.Handler.EndRdf(true);
                // Discard this - it justs means the Handler told us to stop
            }
            catch
            {
                context.Handler.EndRdf(false);
                throw;
            }
        }

        #region Queue Processing

        /// <summary>
        /// Starts the Parsing of the flattened Event Tree by calling the appropriate Grammar Production based on the type of the First Event in the Queue
        /// </summary>
        private void ProcessEventQueue(RdfXmlParserContext context)
        {
            // Get First Event
            IRdfXmlEvent first = context.Events.Dequeue();
            bool setBaseUri = (context.BaseUri == null);
            Uri baseUri;

            if (first is RootEvent)
            {
                GrammarProductionDoc(context, (RootEvent)first);
                if (setBaseUri && !((RootEvent)first).BaseUri.Equals(String.Empty))
                {
                    baseUri = UriFactory.Create(Tools.ResolveUri(((RootEvent)first).BaseUri, String.Empty));
                    context.BaseUri = baseUri;
                    if (!context.Handler.HandleBaseUri(baseUri)) ParserHelper.Stop();
                }
            }
            else
            {
                GrammarProductionRDF(context, (ElementEvent)first);
                if (setBaseUri && !((ElementEvent)first).BaseUri.Equals(String.Empty))
                {
                    baseUri = UriFactory.Create(Tools.ResolveUri(((ElementEvent)first).BaseUri, String.Empty));
                    context.BaseUri = baseUri;
                    if (!context.Handler.HandleBaseUri(baseUri)) ParserHelper.Stop();
                }
            }

        }

        #endregion

        #region Grammar Productions

        /// <summary>
        /// Implementation of the RDF/XML Grammar Production 'doc'
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="root">Root Event to start applying Productions from</param>
        private void GrammarProductionDoc(RdfXmlParserContext context, RootEvent root)
        {
            // Tracing
            if (_traceparsing)
            {
                ProductionTrace("Doc");
            }

            // Call the RDF Production on the first child if it's an rdf:RDF element
            // if (root.Children[0].QName.Equals("rdf:RDF") || root.Children[0].QName.Equals(":RDF"))
            String localName = root.Children[0].LocalName;
            String prefix = root.Children[0].Namespace;
            if (localName.Equals("RDF") && 
                ((context.Namespaces.HasNamespace(prefix) && context.Namespaces.GetNamespaceUri(prefix).AbsoluteUri.Equals(NamespaceMapper.RDF)) 
                 || root.DocumentElement.NamespaceAttributes.Any(ns => ns.Prefix.Equals(prefix) && ns.Uri.Equals(NamespaceMapper.RDF))))
            {
                GrammarProductionRDF(context, root.Children[0]);
            }
            else
            {
                // No rdf:RDF element
                // Drop first element from Queue (which will be a RootEvent)
                // Skip straight to NodeElementList production
                // context.Events.Dequeue();
                GrammarProductionNodeElementList(context, context.Events);
            }
        }

        /// <summary>
        /// Implementation of the RDF/XML Grammar Production 'RDF'
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="element">RDF Element to apply Production to</param>
        private void GrammarProductionRDF(RdfXmlParserContext context, ElementEvent element)
        {
            // Tracing
            if (_traceparsing)
            {
                ProductionTrace("RDF", element);
            }

            // Check Uri is correct
            String localName = element.LocalName;
            String prefix = element.Namespace;
            if (localName.Equals("RDF") || 
                ((context.Namespaces.HasNamespace(prefix) && context.Namespaces.GetNamespaceUri(prefix).AbsoluteUri.Equals(NamespaceMapper.RDF)) 
                 || element.NamespaceAttributes.Any(ns => ns.Prefix.Equals(prefix) && ns.Uri.Equals(NamespaceMapper.RDF))))
            {
                // This is OK
            }
            else
            {
                throw ParserHelper.Error("Unexpected Node '" + element.QName + "', an 'rdf:RDF' node was expected", "RDF", element);
            }
            // Check has no Attributes
            if (element.Attributes.Count > 0)
            {
                throw ParserHelper.Error("Root Node should not contain any attributes other than XML Namespace Declarations", "RDF", element);
            }

            // Start a new namespace scope
            ApplyNamespaces(context, element);

            // Make sure we discard the current ElementEvent which will be at the front of the queue
            context.Events.Dequeue();

            // Build a virtual Sublist of all Nodes up to the matching EndElement to avoid copying the events around too much
            IEventQueue<IRdfXmlEvent> subevents = new SublistEventQueue<IRdfXmlEvent>(context.Events, 1);

            // Call the NodeElementList Grammer Production
            GrammarProductionNodeElementList(context, subevents);

            // Next Event in queue should be an EndElementEvent or we Error
            IRdfXmlEvent next = context.Events.Dequeue();
            if (!(next is EndElementEvent))
            {
                throw ParserHelper.Error("Unexpected Event '" + next.GetType().ToString() + "', an EndElementEvent was expected", "RDF", element);
            }

            // Exit the current namespace scope
            PopNamespaces(context);
        }

        /// <summary>
        /// Implementation of the RDF/XML Grammar Production 'nodeElementList'
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="eventlist">Queue of Events to apply the Production to</param>
        private void GrammarProductionNodeElementList(RdfXmlParserContext context, IEventQueue<IRdfXmlEvent> eventlist)
        {
            // Tracing
            if (_traceparsing)
            {
                ProductionTrace("Node Element List");
            }

            IRdfXmlEvent next;

            // Want to break up into a number of sublists
            while (eventlist.Count > 0) 
            {
                // Create a new Sublist
                IEventQueue<IRdfXmlEvent> subevents = new EventQueue<IRdfXmlEvent>();
                int nesting = 0;

                // Gather the Sublist taking account of nesting
                do
                {
                    next = eventlist.Dequeue();
                    subevents.Enqueue(next);

                    if (next is ElementEvent)
                    {
                        nesting++;
                    }
                    else if (next is EndElementEvent)
                    {
                        nesting--;
                    }
                } while (nesting > 0);

                // Call the next Grammar Production
                GrammarProductionNodeElement(context, subevents);
            }
        }

        /// <summary>
        /// Implementation of the RDF/XML Grammar Production 'nodeElement'
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="eventlist">Queue of Events that make up the Node Element and its Children to apply the Production to</param>
        private void GrammarProductionNodeElement(RdfXmlParserContext context, IEventQueue<IRdfXmlEvent> eventlist)
        {
            // Tracing
            if (_traceparsing)
            {
                ProductionTracePartial("Node Element");
            }

            // Get First Event in the Queue
            IRdfXmlEvent first = eventlist.Dequeue();

            // Check it's an ElementEvent
            if (!(first is ElementEvent))
            {
                // Unexpected Event
                throw ParserHelper.Error("Expected an ElementEvent but encountered a '" + first.GetType().ToString() + "'", "Node Element", first);
            }

            // Check it has a valid Uri
            ElementEvent element = (ElementEvent)first;
            if (_traceparsing) ProductionTracePartial(element);
            // Start a new namespace scope
            ApplyNamespaces(context, element);
            if (!RdfXmlSpecsHelper.IsNodeElementUri(context.Namespaces.GetNamespaceUri(element.Namespace), element.LocalName))
            {
                throw ParserHelper.Error("A Node Element was encountered with an invalid URI '" + element.QName + "' \nCore Syntax Terms, Old Syntax Terms and rdf:li cannot be used as Node Element URIs", "Node Element", element);
            }

            // Check the set of Attributes is Valid
            int limitedAttributesFound = 0;
            String ID = String.Empty;
            foreach (AttributeEvent attr in element.Attributes)
            {
                if (RdfXmlSpecsHelper.IsIDAttribute(attr, context.Namespaces))
                {
                    ID = attr.Value;
                    limitedAttributesFound++;

                    // Set the Subject
                    element.Subject = new UriReferenceEvent("#" + attr.Value, attr.SourceXml);
                }
                else if (RdfXmlSpecsHelper.IsNodeIDAttribute(attr, context.Namespaces))
                {
                    limitedAttributesFound++;

                    // Validate the Node ID
                    if (!XmlSpecsHelper.IsName(attr.Value))
                    {
                        throw ParserHelper.Error("The value '" + attr.Value + "' for rdf:nodeID is not valid, RDF Node IDs can only be valid Names as defined by the W3C XML Specification", "Node Element", attr);
                    }

                    // Set the Subject
                    element.Subject = new BlankNodeIDEvent(attr.Value, attr.SourceXml);
                }
                else if (RdfXmlSpecsHelper.IsAboutAttribute(attr, context.Namespaces))
                {
                    limitedAttributesFound++;

                    // Set the Subject
                    element.Subject = new UriReferenceEvent(attr.Value, attr.SourceXml);
                }
                else if (RdfXmlSpecsHelper.IsPropertyAttribute(attr, context.Namespaces))
                {
                    // Don't need to do anything here yet
                }
                else
                {
                    // Unknown and Unexpected Attribute Type
                    throw ParserHelper.Error("Unexpected Attribute '" + attr.QName + "' was encountered!", "Node Element", element);
                }

                // Can't have more than 1 of ID, Node ID or About Attributes
                if (limitedAttributesFound > 1)
                {
                    throw ParserHelper.Error("A Node Element can only have 1 of the following attributes: rdf:id, rdf:nodeID, rdf:about", "Node Element", element);
                }
            }

            // Generate a Blank Node ID if our Subject is empty
            if (element.Subject == null)
            {
                element.Subject = new BlankNodeIDEvent(element.SourceXml);
            }

            // Add statements as necessary
            INode subj, pred, obj;
            if (element.SubjectNode == null)
            {
                // Don't always want to drop in here since the SubjectNode may already be set elsewhere
                if (element.Subject is UriReferenceEvent)
                {
                    UriReferenceEvent uri = (UriReferenceEvent)element.Subject;
                    subj = Resolve(context, uri, element.BaseUri);
                }
                else if (element.Subject is BlankNodeIDEvent)
                {
                    BlankNodeIDEvent blank = (BlankNodeIDEvent)element.Subject;

                    // Select whether we need to generate an ID or if there's one given for the Blank Node
                    // Note that we let the Graph class handle generation of IDs
                    if (blank.Identifier.Equals(String.Empty))
                    {
                        subj = context.Handler.CreateBlankNode();
                    }
                    else
                    {
                        subj = context.Handler.CreateBlankNode(blank.Identifier);
                    }
                }
                else
                {
                    throw ParserHelper.Error("Unexpected Subject generated for a Triple", "Node Element", element.Subject);
                }
            } 
            else
            {
                subj = element.SubjectNode;
            }

            // Set the Subject Node property of the Event for later reuse
            element.SubjectNode = subj;

            // Validate the ID (if any)
            if (!ID.Equals(String.Empty))
            {
                ValidateID(context, ID, subj);
            }

            //if (!element.QName.Equals("rdf:Description") && !element.QName.Equals(":Description"))
            if (!ElementHasName(element, "Description", NamespaceMapper.RDF, context))
            {
                // Assert a Triple regarding Type
                pred = context.Handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
                obj = Resolve(context, element);//context.Handler.CreateUriNode(element.QName);
                if (!context.Handler.HandleTriple(new Triple(subj, pred, obj))) ParserHelper.Stop();
            }

            // Go back over Attributes looking for property attributes
            foreach (AttributeEvent attr in element.Attributes)
            {
                if (RdfXmlSpecsHelper.IsPropertyAttribute(attr, context.Namespaces))
                {
                    if (attr.Namespace.Equals(NamespaceMapper.RDF) && attr.LocalName.Equals("type"))
                    //if (attr.QName.Equals("rdf:type"))
                    {
                        // Generate a Type Triple
                        pred = context.Handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));

                        // Resolve URIRef into a Uri Node
                        UriReferenceEvent uriref = new UriReferenceEvent(attr.Value, attr.SourceXml);
                        obj = Resolve(context, uriref, element.BaseUri);

                        if (!context.Handler.HandleTriple(new Triple(subj, pred, obj))) ParserHelper.Stop();
                    }
                    else
                    {
                        // Generate a Property Triple
                        pred = context.Handler.CreateUriNode(UriFactory.Create(Tools.ResolveQName(attr.QName, context.Namespaces, null)));

                        // Add Language to Literal if necessary
                        if (element.Language.Equals(String.Empty))
                        {
                            obj = context.Handler.CreateLiteralNode(attr.Value);
                        }
                        else
                        {
                            obj = context.Handler.CreateLiteralNode(attr.Value, element.Language);
                        }

                        if (!context.Handler.HandleTriple(new Triple(subj, pred, obj))) ParserHelper.Stop();
                    }
                }
            }

            // Handle Child Elements
            IEventQueue<IRdfXmlEvent> children = new EventQueue<IRdfXmlEvent>();
            while (eventlist.Count > 1)
            {
                children.Enqueue(eventlist.Dequeue());
            }
            if (children.Count > 0) GrammarProductionPropertyElementList(context, children, element);

            // Check Last Event in queue is an EndElement event
            IRdfXmlEvent last = eventlist.Dequeue();
            if (!(last is EndElementEvent))
            {
                throw ParserHelper.Error("Unexpected Event '" + last.GetType().ToString() + "', expected an EndElement Event", "NodeElement", last);
            }

            // Exit the current namespace scope
            PopNamespaces(context);
        }

        /// <summary>
        /// Implementation of the RDF/XML Grammar Production 'propertyEltList'
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="eventlist">Queue of Events to apply the Production to</param>
        /// <param name="parent">Parent Event (ie. Node) of the Property Elements</param>
        private void GrammarProductionPropertyElementList(RdfXmlParserContext context, IEventQueue<IRdfXmlEvent> eventlist, IRdfXmlEvent parent)
        {
            // Tracing
            if (_traceparsing)
            {
                ProductionTrace("Property Element List");
            }

            IRdfXmlEvent next;

            // Want to break up into a number of sublists
            while (eventlist.Count > 0)
            {
                // Create a new Sublist
                IEventQueue<IRdfXmlEvent> subevents = new EventQueue<IRdfXmlEvent>();
                int nesting = 0;

                // Gather the Sublist taking account of nesting
                do
                {
                    next = eventlist.Dequeue();
                    subevents.Enqueue(next);

                    if (next is ElementEvent)
                    {
                        nesting++;
                    }
                    else if (next is EndElementEvent)
                    {
                        nesting--;
                    }
                } while (nesting > 0);

                // Call the next Grammar Production
                if (subevents.Count > 0) GrammarProductionPropertyElement(context, subevents, parent);
            }
        }

        /// <summary>
        /// Implementation of the RDF/XML Grammar Production 'propertyElt'
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="eventlist">Queue of Events that make up the Property Element and its Children</param>
        /// <param name="parent">Parent Event (ie. Node) of the Property Element</param>
        private void GrammarProductionPropertyElement(RdfXmlParserContext context, IEventQueue<IRdfXmlEvent> eventlist, IRdfXmlEvent parent)
        {
            // Tracing
            if (_traceparsing)
            {
                ProductionTracePartial("Property Element");
            }

            // Get first thing from the Queue
            IRdfXmlEvent first = eventlist.Dequeue();
            ElementEvent element;

            // Must be an ElementEvent
            if (!(first is ElementEvent))
            {
                // Unexpected Event
                throw ParserHelper.Error("Expected an ElementEvent but encountered a '" + first.GetType().ToString() + "'", "PropertyElement", first);
            }

            // Validate the Uri
            element = (ElementEvent)first;
            if (_traceparsing) ProductionTracePartial(element);

            // Start a new namespace scope
            ApplyNamespaces(context, element);
            if (!RdfXmlSpecsHelper.IsPropertyElement(element, context.Namespaces))
            {
                // Invalid Uri
                throw ParserHelper.Error("A Property Element was encountered with an invalid URI '" + element.QName + "'\nCore Syntax Terms, Old Syntax Terms and rdf:Description cannot be used as Property Element URIs", "PropertyElement", element);
            }

            // List Expansion
            if (RdfXmlSpecsHelper.IsLiElement(element, context.Namespaces))
            {
                var u = ListExpand(parent, context.Namespaces);
                element.SetUri(u, context.Namespaces);
            }

            // Need to select what to do based on the Type of Property Element
            IRdfXmlEvent next = eventlist.Peek();

            // This call inserts the first element back at the head of the queue
            // Most of the sub-productions here need this
            // Would ideally use Stacks instead of Queues but Queues make more sense for most of the Parsing
            QueueJump(eventlist, first);

            if (element.ParseType == RdfXmlParseType.None)
            {
                // A Resource/Literal Property Element

                if (next is ElementEvent)
                {
                    // Resource
                    GrammarProductionResourcePropertyElement(context, eventlist, parent);
                }
                else if (next is TextEvent)
                {
                    // Literal
                    GrammarProductionLiteralPropertyElement(context, eventlist, parent);
                }
                else if (next is EndElementEvent)
                {
                    // An Empty Property Element
                    GrammarProductionEmptyPropertyElement(context, element, parent);
                }
                else
                {
                    // Error
                    throw ParserHelper.Error("An Element which should be Parsed with the Default Parsing Rules was encountered without a valid subsequent Event - Parser cannot proceed!", "Property Element", element);
                }
            }
            else if (element.ParseType == RdfXmlParseType.Literal)
            {
                // A rdf:parseType="Literal" Property Element

                GrammarProductionParseTypeLiteralPropertyElement(context, eventlist, parent);
            }
            else if (element.ParseType == RdfXmlParseType.Collection)
            {
                // A rdf:parseType="Collection" Property Element

                GrammarProductionParseTypeCollectionPropertyElement(context, eventlist, parent);
            }
            else if (element.ParseType == RdfXmlParseType.Resource)
            {
                // A rdf:parseType="Resource" Property Element

                GrammarProductionParseTypeResourcePropertyElement(context, eventlist, parent);
            }
            else if (next is EndElementEvent)
            {
                // An Empty Property Element
                GrammarProductionEmptyPropertyElement(context, element, parent);
            }
            else
            {
                // Error
                throw ParserHelper.Error("An Element without a known Parse Type was encountered Or the Parser was unable to determine what to do based on the subsequent event - Parser cannot proceed!", "Node Element", element);
            }
            PopNamespaces(context);
        }

        /// <summary>
        /// Implementation of the RDF/XML Grammar Production 'resourcePropertyElt'
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="eventlist">Queue of Events that make up the Resource Property Element and its Children</param>
        /// <param name="parent">Parent Event (ie. Node) of the Property Element</param>
        private void GrammarProductionResourcePropertyElement(RdfXmlParserContext context, IEventQueue<IRdfXmlEvent> eventlist, IRdfXmlEvent parent)
        {
            // Tracing
            if (_traceparsing)
            {
                ProductionTracePartial("Resource Property Element");
            }

            // Cast to an ElementEvent
            // We don't validate type here since we know this will be an ElementEvent because the calling function
            // will have done this validation previously
            IRdfXmlEvent first = eventlist.Dequeue();
            IRdfXmlEvent next = eventlist.Peek();
            ElementEvent element = (ElementEvent)first;
            if (_traceparsing) ProductionTracePartial(element);

            // Start new namespace scope
            ApplyNamespaces(context, element);

            // Only allowed one attribute max which must be an ID attribute
            String ID = String.Empty;
            if (element.Attributes.Count > 1)
            {
                throw ParserHelper.Error("A Resource Property Element contains too many Attributes, only rdf:ID is permitted", element);
            }
            else if (element.Attributes.Count == 1)
            {
                if (!RdfXmlSpecsHelper.IsIDAttribute(element.Attributes.First(), context.Namespaces))
                {
                    throw ParserHelper.Error("A Resource Property Element was encountered with a single attribute which was not rdf:ID, only rdf:ID is permitted", element);
                }
                else
                {
                    ID = element.Attributes.First().Value;
                }
            }

            // Next must be an ElementEvent
            if (!(next is ElementEvent))
            {
                throw ParserHelper.Error("Unexpected Event '" + next.GetType().ToString() + "', expected an ElementEvent as the first Event in a Resource Property Elements Event list", next);
            }

            // Get list of Sub Events
            IEventQueue<IRdfXmlEvent> subevents = new EventQueue<IRdfXmlEvent>();
            while (eventlist.Count > 1)
            {
                subevents.Enqueue(eventlist.Dequeue());
            }
            GrammarProductionNodeElement(context, subevents);

            // Check Last is an EndElementEvent
            IRdfXmlEvent last = eventlist.Dequeue();
            if (!(last is EndElementEvent))
            {
                throw ParserHelper.Error("Unexpected Event '" + last.GetType().ToString() + "', expected an EndElement Event", last);
            }

            // Now we can generate the relevant RDF
            INode subj, pred, obj;

            // Validate the Type of the Parent
            if (!(parent is ElementEvent))
            {
                throw ParserHelper.Error("Unexpected Parent Event '" + parent.GetType().ToString() + "', expected an ElementEvent", parent);
            }
            ElementEvent parentEl = (ElementEvent)parent;

            // Get the Subject Node from the Parent
            subj = parentEl.SubjectNode;

            // Validate the ID (if any)
            if (!ID.Equals(String.Empty))
            {
                ValidateID(context, ID, subj);
            }

            // Create a Predicate from this Element
            pred = Resolve(context, element);//context.Handler.CreateUriNode(element.QName);

            // Get the Object Node from the Child Node
            ElementEvent child = (ElementEvent)next;
            obj = child.SubjectNode;

            // Assert the Triple
            if (!context.Handler.HandleTriple(new Triple(subj, pred, obj))) ParserHelper.Stop();

            // Add Reification where appropriate
            if (element.Attributes.Count == 1)
            {
                // Must be an rdf:ID attribute as we've validated this earlier

                // Get the Attribute Event and generate a Uri from it
                AttributeEvent attr = element.Attributes.First();
                UriReferenceEvent uriref = new UriReferenceEvent("#" + attr.Value, attr.SourceXml);
                IUriNode uri = Resolve(context, uriref, element.BaseUri);

                Reify(context, uri, subj, pred, obj);
            }

            // End current namespace scope
            PopNamespaces(context);
        }

        /// <summary>
        /// Implementation of the RDF/XML Grammar Production 'literalPropertyElt'
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="eventlist">Queue of Events that make up the Literal Property Element and its Children</param>
        /// <param name="parent">Parent Event (ie. Node) of the Property Element</param>
        private void GrammarProductionLiteralPropertyElement(RdfXmlParserContext context, IEventQueue<IRdfXmlEvent> eventlist, IRdfXmlEvent parent)
        {
            // Tracing
            if (_traceparsing)
            {
                ProductionTracePartial("Literal Property Element");
            }

            // Get the 3 Events (should only be three)
            IRdfXmlEvent first, middle, last;
            first = eventlist.Dequeue();
            middle = eventlist.Dequeue();
            last = eventlist.Dequeue();

            // If Queue is non-empty then Error
            if (eventlist.Count > 0)
            {
                throw ParserHelper.Error("Too many events encountered while trying to parse a Literal Property Element", first);
            }

            ElementEvent element = (ElementEvent)first;
            if (_traceparsing) ProductionTracePartial(element);

            // Start a new namespace sscope
            ApplyNamespaces(context, element);

            // Validate that the middle event is a TextEvent
            if (!(middle is TextEvent))
            {
                throw ParserHelper.Error("Unexpected event '" + middle.GetType().ToString() + "', expected a TextEvent in a Literal Property Element", middle);
            }
            TextEvent text = (TextEvent)middle;

            // Validate the Attributes
            String ID = String.Empty;
            String datatype = String.Empty;
            if (element.Attributes.Count > 2)
            {
                throw ParserHelper.Error("A Literal Property Element contains too many attributes, only rdf:ID and rdf:datatype are permitted", element);
            }
            else
            {
                // Only rdf:ID and rdf:datatype allowed
                foreach (AttributeEvent a in element.Attributes)
                {
                    if (RdfXmlSpecsHelper.IsIDAttribute(a, context.Namespaces)) {
                        ID = "#" + a.Value;
                    }
                    else if (RdfXmlSpecsHelper.IsDataTypeAttribute(a, context.Namespaces))
                    {
                        datatype = a.Value;
                    } 
                    else 
                    {
                        throw ParserHelper.Error("A Literal Property Element contains an unexpected attribute, only rdf:ID and rdf:datatype are permitted", element);
                    }
                }
            }

            // Create the Nodes for the Graph
            INode subj, pred, obj;
            
            // Get the Subject from the Parent
            ElementEvent parentEl = (ElementEvent)parent;
            subj = parentEl.SubjectNode;

            // Validate the ID (if any)
            if (!ID.Equals(String.Empty))
            {
                ValidateID(context, ID.Substring(1), subj);
            }

            // Create a Predicate from this Element
            pred = Resolve(context, element);//context.Handler.CreateUriNode(element.QName);

            // Create an Object from the Text Event
            if (datatype.Equals(String.Empty))
            {
                // No Type with possible Language
                if (element.Language.Equals(String.Empty))
                {
                    obj = context.Handler.CreateLiteralNode(text.Value);
                }
                else
                {
                    obj = context.Handler.CreateLiteralNode(text.Value, element.Language);
                }
            }
            else
            {
                // Typed

                // Resolve the Datatype Uri
                UriReferenceEvent dtref = new UriReferenceEvent(datatype, String.Empty);
                IUriNode dturi = Resolve(context, dtref, element.BaseUri);

                obj = context.Handler.CreateLiteralNode(text.Value, dturi.Uri);
            }

            // Assert the Triple
            if (!context.Handler.HandleTriple(new Triple(subj, pred, obj))) ParserHelper.Stop();

            // Reify if applicable
            if (!ID.Equals(String.Empty))
            {
                // Resolve the Uri
                UriReferenceEvent uriref = new UriReferenceEvent(ID, String.Empty);
                IUriNode uri = Resolve(context, uriref,element.BaseUri);

                Reify(context, uri, subj, pred, obj);
            }

            // End namespace scope
            PopNamespaces(context);
        }

        /// <summary>
        /// Implementation of the RDF/XML Grammar Production 'parseTypeLiteralPropertyElt'
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="eventlist">Queue of Events that make up the Literal Parse Type Property Element and its Children</param>
        /// <param name="parent">Parent Event (ie. Node) of the Property Element</param>
        private void GrammarProductionParseTypeLiteralPropertyElement(RdfXmlParserContext context, IEventQueue<IRdfXmlEvent> eventlist, IRdfXmlEvent parent)
        {
            // Tracing
            if (_traceparsing)
            {
                ProductionTracePartial("Parse Type Literal Property Element");
            }

            // Get the first Event, should be an ElementEvent
            // Type checking is done by the Parent Production
            IRdfXmlEvent first = eventlist.Dequeue();
            ElementEvent element = (ElementEvent)first;
            if (_traceparsing) ProductionTracePartial(element);

            // Start new namespace scope
            ApplyNamespaces(context, element);

            // Validate Attributes
            String ID = String.Empty;
            if (element.Attributes.Count > 2)
            {
                // Can't be more than 2 Attributes, only allowed an optional rdf:ID and a required rdf:parseType
                throw ParserHelper.Error("An Property Element with Parse Type 'Literal' was encountered with too many Attributes.  Only rdf:ID and rdf:parseType are allowed on Property Elements with Parse Type 'Literal'", "Parse Type Literal Property Element", element);
            }
            else
            {
                // Check the attributes that do exist
                foreach (AttributeEvent a in element.Attributes)
                {
                    if (RdfXmlSpecsHelper.IsIDAttribute(a, context.Namespaces))
                    {
                        ID = "#" + a.Value;
                    }
                    else if (RdfXmlSpecsHelper.IsParseTypeAttribute(a, context.Namespaces))
                    {
                        // OK
                    }
                    else
                    {
                        // Invalid Attribute
                        throw ParserHelper.Error("Unexpected Attribute '" + a.QName + "' was encountered on a Property Element with Parse Type 'Literal'.  Only rdf:ID and rdf:parseType are allowed on Property Elements with Parse Type 'Literal'", "Parse Type Literal Property Element", element);
                    }
                }
            }

            // Get the next event in the Queue which should be a TypedLiteralEvent
            // Validate this
            IRdfXmlEvent lit = eventlist.Dequeue();
            if (!(lit is TypedLiteralEvent))
            {
                throw ParserHelper.Error("Unexpected Event '" + lit.GetType().ToString() + "', expected a TypedLiteralEvent after a Property Element with Parse Type 'Literal'", "Parse Type Literal Property Element", lit);
            }

            // Get the Subject from the Parent
            INode subj, pred, obj;
            ElementEvent parentEl = (ElementEvent) parent;
            subj = parentEl.SubjectNode;

            // Validate the ID (if any)
            if (!ID.Equals(String.Empty))
            {
                ValidateID(context, ID.Substring(1), subj);
            }

            // Create the Predicate from the Element
            pred = Resolve(context, element);//context.Handler.CreateUriNode(element.QName);

            // Create the Object from the Typed Literal
            TypedLiteralEvent tlit = (TypedLiteralEvent)lit;
            // At the moment we're just going to ensure that we normalize it to Unicode Normal Form C
            String xmllit = tlit.Value;
            xmllit = xmllit.Normalize();
            obj = context.Handler.CreateLiteralNode(xmllit, UriFactory.Create(tlit.DataType));

            // Assert the Triple
            if (!context.Handler.HandleTriple(new Triple(subj, pred, obj))) ParserHelper.Stop();

            // Reify if applicable
            if (!ID.Equals(String.Empty))
            {
                // Resolve the Uri
                UriReferenceEvent uriref = new UriReferenceEvent(ID, String.Empty);
                IUriNode uri = Resolve(context, uriref,element.BaseUri);

                Reify(context, uri, subj, pred, obj);
            }

            // Check for the last thing being an EndElement Event
            IRdfXmlEvent next = eventlist.Dequeue();
            if (!(next is EndElementEvent))
            {
                throw ParserHelper.Error("Unexpected Event '" + next.GetType().ToString() + "', expected an EndElementEvent to terminate a Parse Type Literal Property Element!", "Parse Type Literal Property Element", next);
            }

            // End the namespace scope
            PopNamespaces(context);
        }

        /// <summary>
        /// Implementation of the RDF/XML Grammar Production 'parseTypeResourcePropertyElt'
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="eventlist">Queue of Events that make up the Resource Parse Type Property Element and its Children</param>
        /// <param name="parent">Parent Event (ie. Node) of the Property Element</param>
        private void GrammarProductionParseTypeResourcePropertyElement(RdfXmlParserContext context, IEventQueue<IRdfXmlEvent> eventlist, IRdfXmlEvent parent)
        {
            // Tracing
            if (_traceparsing)
            {
                ProductionTracePartial("Parse Type Resource Property Element");
            }

            // Get the first Event, should be an ElementEvent
            // Type checking is done by the Parent Production
            IRdfXmlEvent first = eventlist.Dequeue();
            ElementEvent element = (ElementEvent)first;
            if (_traceparsing) ProductionTracePartial(element);

            // Start a new namespace scope
            ApplyNamespaces(context, element);

            // Validate Attributes
            String ID = String.Empty;
            if (element.Attributes.Count > 2)
            {
                // Can't be more than 2 Attributes, only allowed an optional rdf:ID and a required rdf:parseType
                throw ParserHelper.Error("An Property Element with Parse Type 'Resource' was encountered with too many Attributes.  Only rdf:ID and rdf:parseType are allowed on Property Elements with Parse Type 'Resource'", "Parse Type Resource Property Element", element);
            }
            else
            {
                // Check the attributes that do exist
                foreach (AttributeEvent a in element.Attributes)
                {
                    if (RdfXmlSpecsHelper.IsIDAttribute(a, context.Namespaces))
                    {
                        ID = "#" + a.Value;
                    }
                    else if (RdfXmlSpecsHelper.IsParseTypeAttribute(a, context.Namespaces))
                    {
                        // OK
                    }
                    else
                    {
                        // Invalid Attribute
                        throw ParserHelper.Error("Unexpected Attribute '" + a.QName + "' was encountered on a Property Element with Parse Type 'Resource'.  Only rdf:ID and rdf:parseType are allowed on Property Elements with Parse Type 'Resource'", "Parse Type Resource Property Element", element);
                    }
                }
            }

            // Add a Triple about this
            INode subj, pred, obj;

            // Get the Subject from the Parent
            ElementEvent parentEvent = (ElementEvent)parent;
            subj = parentEvent.SubjectNode;

            // Validate the ID (if any)
            if (!ID.Equals(String.Empty))
            {
                ValidateID(context, ID.Substring(1), subj);
            }

            // Create the Predicate from the Element
            pred = Resolve(context, element);//context.Handler.CreateUriNode(element.QName);

            // Generate a Blank Node ID for the Object
            obj = context.Handler.CreateBlankNode();

            // Assert
            if (!context.Handler.HandleTriple(new Triple(subj, pred, obj))) ParserHelper.Stop();

            // Reify if applicable
            if (!ID.Equals(String.Empty))
            {
                // Resolve the Uri
                UriReferenceEvent uriref = new UriReferenceEvent(ID, String.Empty);
                IUriNode uri = Resolve(context, uriref,element.BaseUri);

                Reify(context, uri, subj, pred, obj);
            }

            // Get the next event in the Queue which should be either an Element Event or a End Element Event
            // Validate this
            IRdfXmlEvent next = eventlist.Dequeue();
            if (next is EndElementEvent)
            {
                // Content is Empty so nothing else to do
            }
            else if (next is ElementEvent)
            {
                // Non-Empty Content so need to build a sequence of new events
                IEventQueue<IRdfXmlEvent> subEvents = new EventQueue<IRdfXmlEvent>();

                // Create an rdf:Description event as the container
                var prefix = context.Namespaces.GetPrefix(new Uri(NamespaceMapper.RDF));
                var descrip = new ElementEvent(prefix+":Description", element.BaseUri, string.Empty);
                descrip.Subject = new BlankNodeIDEvent(string.Empty);
                descrip.SubjectNode = obj;
                subEvents.Enqueue(descrip);

                // Add the current element we were looking at
                subEvents.Enqueue(next);

                // Add rest of events in list (except the last)
                while (eventlist.Count > 1)
                {
                    subEvents.Enqueue(eventlist.Dequeue());
                }

                // Terminate with an EndElement Event
                subEvents.Enqueue(new EndElementEvent());

                // Process with Node Element Production
                GrammarProductionNodeElement(context, subEvents);

                // Get the last thing in the List
                next = eventlist.Dequeue();
            }
            else
            {
                throw ParserHelper.Error("Unexpected Event '" + next.GetType().ToString() + "', expected an ElementEvent or EndElementEvent after a Parse Type Resource Property Element!", "Parse Type Resource Property Element", next);
            }

            // Check for the last thing being an EndElement Event
            if (!(next is EndElementEvent))
            {
                throw ParserHelper.Error("Unexpected Event '" + next.GetType().ToString() + "', expected an EndElementEvent to terminate a Parse Type Resource Property Element!", "Parse Type Resource Property Element", next);
            }

            // End the namespace scope
            PopNamespaces(context);
        }

        /// <summary>
        /// Implementation of the RDF/XML Grammar Production 'parseTypeCollectionPropertyElt'
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="eventlist">Queue of Events that make up the Collection Parse Type Property Element and its Children</param>
        /// <param name="parent">Parent Event (ie. Node) of the Property Element</param>
        private void GrammarProductionParseTypeCollectionPropertyElement(RdfXmlParserContext context, IEventQueue<IRdfXmlEvent> eventlist, IRdfXmlEvent parent)
        {
            // Tracing
            if (_traceparsing)
            {
                ProductionTracePartial("Parse Type Collection Property Element");
            }

            // Get the first Event, should be an ElementEvent
            // Type checking is done by the Parent Production
            IRdfXmlEvent first = eventlist.Dequeue();
            ElementEvent element = (ElementEvent)first;
            if (_traceparsing) ProductionTracePartial(element);

            // Start a new namespace scope
            ApplyNamespaces(context, element);

            // Validate Attributes
            String ID = String.Empty;
            if (element.Attributes.Count > 2)
            {
                // Can't be more than 2 Attributes, only allowed an optional rdf:ID and a required rdf:parseType
                throw ParserHelper.Error("An Property Element with Parse Type 'Collection' was encountered with too many Attributes.  Only rdf:ID and rdf:parseType are allowed on Property Elements with Parse Type 'Collection'", "Parse Type Collection Property Element", element);
            }
            else
            {
                // Check the attributes that do exist
                foreach (AttributeEvent a in element.Attributes)
                {
                    if (RdfXmlSpecsHelper.IsIDAttribute(a, context.Namespaces))
                    {
                        ID = "#" + a.Value;
                    }
                    else if (RdfXmlSpecsHelper.IsParseTypeAttribute(a, context.Namespaces))
                    {
                        // OK
                    }
                    else
                    {
                        // Invalid Attribute
                        throw ParserHelper.Error("Unexpected Attribute '" + a.QName + "' was encountered on a Property Element with Parse Type 'Collection'.  Only rdf:ID and rdf:parseType are allowed on Property Elements with Parse Type 'Collection'", "Parse Type Collection Property Element", element);
                    }
                }
            }

            // Build sequence of Blank Nodes
            IRdfXmlEvent next;
            IRdfXmlEvent nodeElement;

            Queue<ElementEvent> seqNodes = new Queue<ElementEvent>();
            while (eventlist.Count > 1)
            {
                #region Node Element Processing
                // Need to process the Node Element first

                // Create a new Sublist
                IEventQueue<IRdfXmlEvent> subevents = new EventQueue<IRdfXmlEvent>();
                int nesting = 0;
                nodeElement = eventlist.Peek();

                // Add Node Element to sequence
                seqNodes.Enqueue((ElementEvent)nodeElement);

                // Gather the Sublist taking account of nesting
                do
                {
                    next = eventlist.Dequeue();
                    subevents.Enqueue(next);

                    if (next is ElementEvent)
                    {
                        nesting++;
                    }
                    else if (next is EndElementEvent)
                    {
                        nesting--;
                    }
                } while (nesting > 0);

                // Call the next Grammar Production
                GrammarProductionNodeElement(context, subevents);

                #endregion
            }

            // Build a triple expressing the start of the list (which may be an empty list)
            INode subj, pred, obj;
            INode firstPred, restPred;
            INode b1, b2;

            // Subject comes from Parent
            ElementEvent parentElement = (ElementEvent)parent;
            subj = parentElement.SubjectNode;

            // Validate the ID (if any)
            if (!ID.Equals(String.Empty))
            {
                ValidateID(context, ID.Substring(1), subj);
            }

            // Predicate from the Element
            pred = Resolve(context, element);//context.Handler.CreateUriNode(element.QName);

            if (seqNodes.Count > 0)
            {
                // Non-empty list
                ElementEvent node;

                // Get first Element from the Queue
                node = seqNodes.Dequeue();

                // Object is first thing in the Sequence which we create a Blank Node for
                b1 = context.Handler.CreateBlankNode();

                // Assert
                if (!context.Handler.HandleTriple(new Triple(subj, pred, b1))) ParserHelper.Stop();

                // Reify if applicable
                if (!ID.Equals(String.Empty))
                {
                    // Resolve the Uri
                    UriReferenceEvent uriref = new UriReferenceEvent(ID, String.Empty);
                    IUriNode uri = Resolve(context, uriref, element.BaseUri);

                    Reify(context, uri, subj, pred, b1);
                }

                // Set the first element in the list
                subj = b1;
                firstPred = context.Handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListFirst));
                if (!context.Handler.HandleTriple(new Triple(subj, firstPred, node.SubjectNode))) ParserHelper.Stop();

                // Middle elements of the list
                restPred = context.Handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListRest));
                while (seqNodes.Count >= 1)
                {
                    node = seqNodes.Dequeue();

                    // Set Node 2 to be the rest of the previous items list
                    b2 = context.Handler.CreateBlankNode();
                    if (!context.Handler.HandleTriple(new Triple(b1, restPred, b2))) ParserHelper.Stop();

                    // Set Node 2 to be the start of it's own list
                    if (!context.Handler.HandleTriple(new Triple(b2, firstPred, node.SubjectNode))) ParserHelper.Stop();

                    b1 = b2;
                }

                // Set last element of the list to have its rest as nil
                if (!context.Handler.HandleTriple(new Triple(b1, restPred, context.Handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListNil))))) ParserHelper.Stop();
            }
            else
            {
                // Empty list

                // Object is therefore rdf:nil
                obj = context.Handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListNil));

                // Assert
                if (!context.Handler.HandleTriple(new Triple(subj, pred, obj))) ParserHelper.Stop();

                // Reify if applicable
                if (!ID.Equals(String.Empty))
                {
                    // Resolve the Uri
                    UriReferenceEvent uriref = new UriReferenceEvent(ID, String.Empty);
                    IUriNode uri = Resolve(context, uriref, element.BaseUri);

                    Reify(context, uri, subj, pred, obj);
                }
            }

            // Check last event is an EndElementEvent
            next = eventlist.Dequeue();
            if (!(next is EndElementEvent))
            {
                throw ParserHelper.Error("Unexpected Event '" + next.GetType().ToString() + "', expected an EndElementEvent to terminate a Parse Type Collection Property Element!", "Parse Type Collection Property Element", next);
            }

            // End the current namespace scope
            PopNamespaces(context);
        }

        /// <summary>
        /// Implementation of the RDF/XML Grammar Production 'emptyPropertyElt'
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="element">Element Event for the Empty Property Element</param>
        /// <param name="parent">Parent Event (ie. Node) of the Property Element</param>
        private void GrammarProductionEmptyPropertyElement(RdfXmlParserContext context, ElementEvent element, IRdfXmlEvent parent)
        {
            // Tracing
            if (_traceparsing)
            {
                ProductionTrace("Empty Property Element");
            }

            // Start a new namespace scope
            ApplyNamespaces(context, element);

            INode subj, pred, obj;
            ElementEvent parentEl;

            // Are there any attributes OR Only a rdf:ID attribute?
            if (element.Attributes.Count == 0 || (element.Attributes.Count == 1 && RdfXmlSpecsHelper.IsIDAttribute(element.Attributes[0], context.Namespaces)))
            {
                // No Attributes/Only rdf:ID

                // Get the Subject Node from the Parent
                parentEl = (ElementEvent)parent;
                subj = parentEl.SubjectNode;

                // Create the Predicate from the Element
                pred = Resolve(context, element);//context.Handler.CreateUriNode(element.QName);

                // Create the Object
                if (!element.Language.Equals(String.Empty))
                {
                    obj = context.Handler.CreateLiteralNode(String.Empty, element.Language);
                }
                else
                {
                    obj = context.Handler.CreateLiteralNode(String.Empty);
                }

                // Make the Assertion
                if (!context.Handler.HandleTriple(new Triple(subj, pred, obj))) ParserHelper.Stop();

                // Reifiy if applicable
                if (element.Attributes.Count == 1)
                {
                    // Validate the ID
                    ValidateID(context, element.Attributes[0].Value, subj);

                    // Resolve the Uri
                    UriReferenceEvent uriref = new UriReferenceEvent("#" + element.Attributes[0].Value, String.Empty);
                    IUriNode uri = Resolve(context, uriref, element.BaseUri);

                    Reify(context, uri, subj, pred, obj);
                }

            }
            else if (element.Attributes.Count > 0 && element.Attributes.Where(a => RdfXmlSpecsHelper.IsDataTypeAttribute(a, context.Namespaces)).Count() == 1)
            {
                // Should be processed as a Typed Literal Event instead
                EventQueue<IRdfXmlEvent> temp = new EventQueue<IRdfXmlEvent>();
                temp.Enqueue(element);
                temp.Enqueue(new TextEvent(String.Empty, String.Empty));
                temp.Enqueue(new EndElementEvent());
                GrammarProductionLiteralPropertyElement(context, temp, parent);
            }
            else
            {

                // Check through attributes
                IRdfXmlEvent res = null;

                // Check through attributes to decide the Subject of the Triple(s)
                String ID = String.Empty;
                int limitedAttributes = 0;
                foreach (AttributeEvent a in element.Attributes)
                {
                    if (RdfXmlSpecsHelper.IsResourceAttribute(a, context.Namespaces))
                    {
                        // An rdf:resource attribute so a Uri Reference
                        res = new UriReferenceEvent(a.Value, a.SourceXml);
                        limitedAttributes++;
                    }
                    else if (RdfXmlSpecsHelper.IsNodeIDAttribute(a, context.Namespaces))
                    {
                        // An rdf:nodeID attribute so a Blank Node

                        // Validate the Node ID
                        if (!XmlSpecsHelper.IsName(a.Value))
                        {
                            // Invalid nodeID
                            throw ParserHelper.Error("The value '" + a.Value + "' for rdf:nodeID is not valid, RDF Node IDs can only be valid Names as defined by the W3C XML Specification", "Empty Property Element", a);
                        }
                        res = new BlankNodeIDEvent(a.Value, a.SourceXml);
                        limitedAttributes++;
                    }
                    else if (RdfXmlSpecsHelper.IsIDAttribute(a, context.Namespaces))
                    {
                        // Set the ID for later use in reification
                        ID = "#" + a.Value;
                    }

                    // Check we haven't got more than 1 of the Limited Attributes
                    if (limitedAttributes > 1)
                    {
                        throw ParserHelper.Error("A Property Element can only have 1 of the following attributes: rdf:nodeID or rdf:resource", "Empty Property Element", element);
                    }
                }
                if (res == null)
                {
                    // No relevant attributes so an anonymous Blank Node
                    res = new BlankNodeIDEvent(String.Empty);
                }

                // Now create the actual Subject Node
                if (res is UriReferenceEvent)
                {
                    // Resolve the Uri Reference
                    UriReferenceEvent uriref = (UriReferenceEvent)res;
                    subj = Resolve(context, uriref, element.BaseUri);
                }
                else if (res is BlankNodeIDEvent)
                {
                    BlankNodeIDEvent blank = (BlankNodeIDEvent)res;
                    if (blank.Identifier.Equals(String.Empty))
                    {
                        // Have the Graph generate a Blank Node ID
                        subj = context.Handler.CreateBlankNode();
                    }
                    else
                    {
                        // Use the supplied Blank Node ID
                        subj = context.Handler.CreateBlankNode(blank.Identifier);
                    }
                }
                else
                {
                    // Should never hit this case but required to get the Code to Compile
                    // Have the Graph generate a Blank Node ID
                    subj = context.Handler.CreateBlankNode();
                }

                // Validate the ID (if any)
                if (!ID.Equals(String.Empty))
                {
                    ValidateID(context, ID.Substring(1), subj);
                }

                // Relate the Property element to its parent
                parentEl = (ElementEvent)parent;
                pred = Resolve(context, element);//context.Handler.CreateUriNode(element.QName);
                if (!context.Handler.HandleTriple(new Triple(parentEl.SubjectNode, pred, subj))) ParserHelper.Stop();

                // Reify if applicable
                if (!ID.Equals(String.Empty))
                {
                    // Resolve the Uri
                    UriReferenceEvent uriref = new UriReferenceEvent(ID, String.Empty);
                    IUriNode uri = Resolve(context, uriref, element.BaseUri);

                    Reify(context, uri, parentEl.SubjectNode, pred, subj);
                }

                // Process the rest of the Attributes
                foreach (AttributeEvent a in element.Attributes)
                {
                    if (RdfXmlSpecsHelper.IsTypeAttribute(a, context.Namespaces))
                    {
                        // A Property Attribute giving a Type

                        // Assert a Type Triple
                        UriReferenceEvent type = new UriReferenceEvent(a.Value, a.SourceXml);
                        pred = context.Handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
                        obj = Resolve(context, type, element.BaseUri);

                        if (!context.Handler.HandleTriple(new Triple(parentEl.SubjectNode, pred, obj))) ParserHelper.Stop();
                    }
                    else if (RdfXmlSpecsHelper.IsPropertyAttribute(a, context.Namespaces))
                    {
                        // A Property Attribute

                        // Validate the Normalization of the Attribute Value
                        if (!a.Value.IsNormalized())
                        {
                            throw ParserHelper.Error("Encountered a Property Attribute '" + a.QName + "' whose value was not correctly normalized in Unicode Normal Form C", "Empty Property Element", a);
                        }
                        else
                        {
                            // Create the Predicate from the Attribute QName
                            pred = context.Handler.CreateUriNode(UriFactory.Create(Tools.ResolveQName(a.QName, context.Namespaces, null)));

                            // Create the Object from the Attribute Value
                            if (element.Language.Equals(String.Empty))
                            {
                                obj = context.Handler.CreateLiteralNode(a.Value);
                            }
                            else
                            {
                                obj = context.Handler.CreateLiteralNode(a.Value, element.Language);
                            }

                            // Assert the Property Triple
                            if (!context.Handler.HandleTriple(new Triple(subj, pred, obj))) ParserHelper.Stop();
                        }
                    }
                    else if (RdfXmlSpecsHelper.IsIDAttribute(a, context.Namespaces) || RdfXmlSpecsHelper.IsNodeIDAttribute(a, context.Namespaces) || RdfXmlSpecsHelper.IsResourceAttribute(a, context.Namespaces))
                    {
                        // These have already been processed
                        // We test for them so that we can then throw ParserHelper.Errors in the final case for unexpected attributes
                    }
                    else
                    {
                        // Unexpected Attribute
                        throw ParserHelper.Error("Unexpected Attribute '" + a.QName + "' encountered on a Property Element!  Only rdf:ID, rdf:resource, rdf:nodeID and Property Attributes are permitted on Property Elements", "Empty Property Element", element);
                    }
                }
            }
            // End the current namespace scope
            PopNamespaces(context);
        }

        // Useful Functions defined as part of the Grammar
        #region Useful Grammar Helper Functions

        /// <summary>
        /// Applies the Namespace Attributes of an Element Event to the Namespace Map
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="evt">Element Event</param>
        private static void ApplyNamespaces(RdfXmlParserContext context, ElementEvent evt)
        {
            context.Namespaces.IncrementNesting();
            if (!evt.BaseUri.Equals(String.Empty))
            {
                Uri baseUri = UriFactory.Create(Tools.ResolveUri(evt.BaseUri, context.BaseUri.ToSafeString()));
                context.BaseUri = baseUri;
                if (!context.Handler.HandleBaseUri(baseUri)) ParserHelper.Stop();
            }
            foreach (NamespaceAttributeEvent ns in evt.NamespaceAttributes)
            {
                if (!context.Namespaces.HasNamespace(ns.Prefix) || !context.Namespaces.GetNamespaceUri(ns.Prefix).AbsoluteUri.Equals(ns.Uri))
                {
                    context.Namespaces.AddNamespace(ns.Prefix, UriFactory.Create(ns.Uri));
                    if (!context.Handler.HandleNamespace(ns.Prefix, UriFactory.Create(ns.Uri))) ParserHelper.Stop();
                }
            }
        }

        private static void PopNamespaces(RdfXmlParserContext context)
        {
            context.Namespaces.DecrementNesting();
        }

        /// <summary>
        /// Resolves a Uri Reference into a Uri Node against a given Base Uri
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="uriref">Uri Reference to Resolve</param>
        /// <param name="baseUri">Base Uri to Resolve against</param>
        /// <returns></returns>
        private IUriNode Resolve(RdfXmlParserContext context, UriReferenceEvent uriref, String baseUri)
        {
            try
            {
                if (baseUri.Equals(String.Empty)) baseUri = context.BaseUri.ToSafeString();
                if (string.Empty.Equals(uriref.Identifier))
                    baseUri = Tools.StripUriFragment(UriFactory.Create(baseUri)).ToSafeString();
                IUriNode u = context.Handler.CreateUriNode(UriFactory.Create(Tools.ResolveUri(uriref.Identifier, baseUri)));
                return u;
            }
            catch (Exception ex)
            {
                // Catch the error so we can wrap in in our own error function
                // If it fails then we know we got an error caused by this Event
                throw ParserHelper.Error(ex.Message, uriref);
            }
        }

        private IUriNode Resolve(RdfXmlParserContext context, ElementEvent el)
        {
            try
            {
                IUriNode u = context.Handler.CreateUriNode(UriFactory.Create(Tools.ResolveQName(el.QName, context.Namespaces, null)));
                return u;
            }
            catch (Exception ex)
            {
                throw ParserHelper.Error(ex.Message, el);
            }
        }

        /// <summary>
        /// Reifies a Triple
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="uriref">Uri Reference for the Reified Triple</param>
        /// <param name="subj">Subject of the Triple</param>
        /// <param name="pred">Predicate of the Triple</param>
        /// <param name="obj">Object of the Triple</param>
        private void Reify(RdfXmlParserContext context, IUriNode uriref, INode subj, INode pred, INode obj)
        {
            if (!context.Handler.HandleTriple(new Triple(uriref, context.Handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfSubject)), subj))) ParserHelper.Stop();
            if (!context.Handler.HandleTriple(new Triple(uriref, context.Handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfPredicate)), pred))) ParserHelper.Stop();
            if (!context.Handler.HandleTriple(new Triple(uriref, context.Handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfObject)), obj))) ParserHelper.Stop();
            if (!context.Handler.HandleTriple(new Triple(uriref, context.Handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType)), context.Handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfStatement))))) ParserHelper.Stop();
        }

        /// <summary>
        /// Helper function which inserts an Element back on the front of a Queue
        /// </summary>
        /// <param name="eventlist">Queue to insert onto the Front of</param>
        /// <param name="evt">Event to put on the front of the Queue</param>
        private void QueueJump(IEventQueue<IRdfXmlEvent> eventlist, IRdfXmlEvent evt)
        {
            Stack<IRdfXmlEvent> temp = new Stack<IRdfXmlEvent>();
            temp.Push(evt);

            while (eventlist.Count > 0)
            {
                temp.Push(eventlist.Dequeue());
            }

            foreach (IRdfXmlEvent e in temp.Reverse())
            {
                eventlist.Enqueue(e);
            }
        }

        /// <summary>
        /// Applies List Expansion to the given Event
        /// </summary>
        /// <param name="evt">Element to apply List Expansion to</param>
        /// <returns>Uri Reference for the List Item</returns>
        /// <remarks>List Expansion only works on Element Events</remarks>
        private UriReferenceEvent ListExpand(IRdfXmlEvent evt, INamespaceMapper nsMapper)
        {
            if (evt is ElementEvent)
            {
                // Cast to an ElementEvent
                var e = (ElementEvent)evt;

                var nsPrefix = nsMapper.GetPrefix(new Uri(NamespaceMapper.RDF));
                // Form a new Uri Reference
                var u = new UriReferenceEvent(nsPrefix + ":_" + e.ListCounter, string.Empty);

                // Increment the List Counter
                e.ListCounter = e.ListCounter + 1;

                // Return the new Uri Reference
                return u;
            }
            else
            {
                throw ParserHelper.Error("Cannot perform List Expansion on an Event which is not an ElementEvent", evt);
            }
        }

        /// <summary>
        /// Validates that an ID is correctly formed and has only been used once in the context of a given Subject
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="id">ID to Validate</param>
        /// <param name="subj">Subject that the ID pertains to</param>
        private void ValidateID(RdfXmlParserContext context, String id, INode subj)
        {
            // Validate the actual ID value
            if (!XmlSpecsHelper.IsName(id))
            {
                throw new RdfParseException("The value '" + id + "' for rdf:ID is not valid, RDF IDs can only be valid Names as defined by the W3C XML Specification");
            }

            // Validate that the ID hasn't been used more than once in the same Base Uri context
            if (context.IDs.ContainsKey(id))
            {
                if (context.IDs[id].Contains(subj))
                {
                    throw new RdfParseException("An rdf:ID must be unique to a Node within a File, the rdf:ID '" + id + "' has already been used for a Node in this RDF/XML File!");
                }
                else
                {
                    context.IDs[id].Add(subj);
                }
            }
            else
            {
                context.IDs.Add(id, new List<INode>() { subj });
            }
        }

        #endregion

        #endregion

        #region Tracing Methods

        /// <summary>
        /// Tracing function used when Parse Tracing is enabled
        /// </summary>
        /// <param name="production">Production</param>
        private void ProductionTrace(String production)
        {
            Console.WriteLine("Production '" + production + "' called");
        }

        private void ProductionTracePartial(String production)
        {
            Console.Write("Production '" + production + "' called");
        }

        private void ProductionTracePartial(ElementEvent evt)
        {
            Console.WriteLine(" on element <" + evt.QName + ">" + (evt.Position != null ? " at Line " + evt.Position.StartLine + " Column " + evt.Position.StartPosition : String.Empty));
        }

        /// <summary>
        /// Tracing function used when Parse Tracing is enabled
        /// </summary>
        /// <param name="production">Production</param>
        /// <param name="evt"></param>
        private void ProductionTrace(String production, ElementEvent evt)
        {
            Console.WriteLine("Production '" + production + "' called on element <" + evt.QName + ">" + (evt.Position != null ? " at Line " + evt.Position.StartLine + " Column " + evt.Position.StartPosition : String.Empty));
        }

        #endregion

        /// <summary>
        /// Gets the String representation of the Parser which is a description of the syntax it parses
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            switch (_mode)
            {
                case RdfXmlParserMode.DOM:
                    return "RDF/XML (DOM)";
                case RdfXmlParserMode.Streaming:
                default:
                    return "RDF/XML (Streaming)";
            }
        }
        private static bool ElementHasName(ElementEvent el, string localName, string namespaceUri, RdfXmlParserContext context)
        {
            return el.LocalName.Equals(localName) && 
                   context.Namespaces.HasNamespace(el.Namespace) && 
                   context.Namespaces.GetNamespaceUri(el.Namespace).ToString().Equals(namespaceUri);
        }
    }
}
