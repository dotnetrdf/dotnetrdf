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

//Uncomment this when making changes that need to work with the non-XML DOM code
//#define NO_XMLDOM

using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
#if !NO_XSL
using System.Xml.Xsl;
#endif
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing.Contexts;
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
        : IRdfReader
    {
        /// <summary>
        /// Current W3C Namespace Uri for TriX
        /// </summary>
        public const String TriXNamespaceUri = "http://www.w3.org/2004/03/trix/trix-1/";

#if !NO_XMLDOM

        /// <summary>
        /// Loads the RDF Dataset from the TriX input using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">Input to load from</param>
        /// <param name="profile"></param>
        public void Load(IRdfHandler handler, TextReader input, IParserProfile profile)
        {
            if (handler == null) throw new RdfParseException("Cannot parse an RDF Dataset using a null handler");
            if (input == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null input");

            //First try and load as XML and apply any stylesheets
            try
            {
                input.CheckEncoding(Encoding.UTF8, this.RaiseWarning);
                profile = profile.EnsureParserProfile();

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
                    if (child.NodeType != XmlNodeType.ProcessingInstruction) continue;
                    if (child.Name != "xml-stylesheet") continue;

                    //Load in the XML a 2nd time so we can transform it properly if needed
                    if (!inputReady)
                    {
                        inputDoc.LoadXml(doc.OuterXml);
                        inputReady = true;
                    }

                    Regex getStylesheetUri = new Regex("href=\"([^\"]+)\"");
                    String stylesheetUri = getStylesheetUri.Match(child.Value).Groups[1].Value;

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
#else
                this.RaiseWarning("XSLT is not supported on your platform, if your TriX data uses the XSLT based extension mechanism it may not be parsed correctly");
#endif

                //Start parsing
                if (!inputReady) inputDoc = doc;
                TriXParserContext context = new TriXParserContext(handler, profile);
                this.TryParseGraphset(inputDoc, context);

                input.Close();
            }
            catch (XmlException xmlEx)
            {
                //Wrap in a RDF Parse Exception
                throw new RdfParseException("Unable to Parse this TriX since System.Xml was unable to parse the document into a DOM Tree, see the inner exception for details", xmlEx);
            }
            finally
            {
                input.CloseQuietly();
            }
        }

        private void TryParseGraphset(XmlDocument doc, TriXParserContext context)
        {
            try
            {
                context.Handler.StartRdf();
                ParserHelper.HandleInitialState(context);

                XmlElement graphsetEl = doc.DocumentElement;
                if (!graphsetEl.Name.Equals("TriX"))
                {
                    throw new RdfParseException("Unexpected Document Element '" + graphsetEl.Name + "' encountered, expected the Document Element of a TriX Document to be the <TriX> element");
                }

                if (!graphsetEl.HasAttributes)
                {
                    throw new RdfParseException("<TriX> fails to define any attributes, the element must define the xmlns attribute to be the TriX namespace");
                }

                bool trixNsDefined = false;
                foreach (XmlAttribute attr in graphsetEl.Attributes)
                {
                    if (attr.Name.Equals("xmlns"))
                    {
                        //Ensure that the xmlns attribute is defined and is the TriX namespace
                        if (trixNsDefined) throw new RdfParseException("The xmlns attribute can only occur once on the <TriX> element");
                        if (!attr.Value.Equals(TriXNamespaceUri)) throw new RdfParseException("The xmlns attribute of the <TriX> element must have it's value set to the TriX Namespace URI which is '" + TriXNamespaceUri + "'");
                        trixNsDefined = true;
                    }
                    else if (attr.LocalName.Equals("xmlns"))
                    {
                        //Don't think we need to do anything here
                    }
                }

                if (!trixNsDefined) throw new RdfParseException("The <TriX> element fails to define the required xmlns attribute defining the TriX Namespace");

                //Process Child Nodes
                foreach (XmlNode graph in graphsetEl.ChildNodes)
                {
                    //Ignore non-Element nodes
                    if (graph.NodeType == XmlNodeType.Element)
                    {
                        this.TryParseGraph(graph, context);
                    }
                }

                context.Handler.EndRdf(true);
            }
            catch (RdfParsingTerminatedException)
            {
                context.Handler.EndRdf(true);
                //Discard this - it justs means the Handler told us to stop
            }
            catch
            {
                context.Handler.EndRdf(false);
                throw;
            }
        }

        private void TryParseGraph(XmlNode graphEl, TriXParserContext context)
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
            INode graphName;
            if (nameEl.Name.Equals("uri"))
            {
                //TODO: Add support for reading Base Uri from xml:base attributes in the file
                graphName = context.Handler.CreateUriNode(UriFactory.ResolveUri(nameEl.InnerText, null));
            }
            else if (nameEl.Name.Equals("id"))
            {
                // TODO Generate a Blank Node name for the graph
                throw new NotSupportedException("Blank node names for TriX graphs are not yet supported");
            }
            else
            {
                skipFirst = false;
                graphName = Quad.DefaultGraphNode;
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
                    this.TryParseTriple(triple, context, graphName);
                }
            }
        }

        private void TryParseTriple(XmlNode tripleEl, TriXParserContext context, INode graphName)
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
            XmlNode subjEl = tripleEl.ChildNodes[0];
            XmlNode predEl = tripleEl.ChildNodes[1];
            XmlNode objEl = tripleEl.ChildNodes[2];

            //Parse XML Nodes into RDF Nodes
            INode subj, pred, obj;
            if (subjEl.Name.Equals("uri"))
            {
                subj = context.Handler.CreateUriNode(UriFactory.Create(subjEl.InnerText));
            }
            else if (subjEl.Name.Equals("id"))
            {
                subj = context.Handler.CreateBlankNode(context.BlankNodeGenerator.GetGuid(subjEl.InnerText));
            }
            else
            {
                throw Error("Unexpected element <" + subjEl.Name + "> encountered, expected a <id>/<uri> element as the Subject of a Triple", subjEl);
            }

            if (predEl.Name.Equals("uri"))
            {
                pred = context.Handler.CreateUriNode(UriFactory.Create(predEl.InnerText));
            }
            else
            {
                throw Error("Unexpected element <" + predEl.Name + "> encountered, expected a <uri> element as the Predicate of a Triple", subjEl);
            }

            if (objEl.Name.Equals("uri"))
            {
                obj = context.Handler.CreateUriNode(UriFactory.Create(objEl.InnerText));
            }
            else if (objEl.Name.Equals("id"))
            {
                obj = context.Handler.CreateBlankNode(context.BlankNodeGenerator.GetGuid(objEl.InnerText));
            }
            else if (objEl.Name.Equals("plainLiteral"))
            {
                obj = objEl.Attributes != null && objEl.Attributes.GetNamedItem("xml:lang") != null ? context.Handler.CreateLiteralNode(objEl.InnerText, objEl.Attributes["xml:lang"].Value) : context.Handler.CreateLiteralNode(objEl.InnerText);
            }
            else if (objEl.Name.Equals("typedLiteral"))
            {
                if (objEl.Attributes != null && objEl.Attributes.GetNamedItem("datatype") != null)
                {
                    Uri dtUri = UriFactory.Create(objEl.Attributes["datatype"].Value);
                    if (objEl.FirstChild.NodeType == XmlNodeType.Text)
                    {
                        obj = context.Handler.CreateLiteralNode(objEl.InnerText, dtUri);
                    }
                    else if (objEl.FirstChild.NodeType == XmlNodeType.CDATA)
                    {
                        obj = context.Handler.CreateLiteralNode(objEl.FirstChild.InnerXml, dtUri);
                    }
                    else
                    {
                        obj = context.Handler.CreateLiteralNode(objEl.InnerText, dtUri);
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
            if (!context.Handler.HandleQuad(new Quad(subj, pred, obj, graphName))) ParserHelper.Stop(); ;
        }

        /// <summary>
        /// Helper method for raising informative standardised Parser Errors
        /// </summary>
        /// <param name="msg">The Error Message</param>
        /// <param name="n">The Node that is the cause of the Error</param>
        /// <returns></returns>
        private static RdfParseException Error(String msg, XmlNode n)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("[Source XML]");
            output.AppendLine(n.OuterXml);
            output.AppendLine();
            output.Append(msg);

            return new RdfParseException(output.ToString());
        }

#else
        private static XmlReaderSettings GetSettings()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
#if PORTABLE
            settings.DtdProcessing = DtdProcessing.Ignore;
#elif SILVERLIGHT
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

        public void Load(IRdfHandler handler, TextReader input, IParserProfile profile)
        {
            if (handler == null) throw new RdfParseException("Cannot parse an RDF Dataset using a null handler");
            if (input == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null input");

            //First try and load as XML and apply any stylesheets
            try
            {
                input.CheckEncoding(Encoding.UTF8, this.RaiseWarning);

                //Get the reader and start parsing
                XmlReader reader = XmlReader.Create(input, GetSettings());
                this.RaiseWarning("The TriX Parser is operating without XSL support, if your TriX file requires XSL then it will not be parsed successfully");
                TriXStreamingParserContext context = new TriXStreamingParserContext(handler, reader);
                this.TryParseGraphset(context);

                input.Close();
            }
            catch (XmlException xmlEx)
            {
                //Wrap in a RDF Parse Exception
                throw new RdfParseException("Unable to Parse this TriX since the XmlReader encountered an error, see the inner exception for details", xmlEx);
            }
            finally
            {
                input.CloseQuietly();
            }
        }

        private void TryParseGraphset(TriXStreamingParserContext context)
        {
            try
            {
                context.Handler.StartRdf();

                context.XmlReader.Read();

                //Skip XML Declaration if present
                if (context.XmlReader.NodeType == XmlNodeType.XmlDeclaration) context.XmlReader.Read();

                if (!context.XmlReader.Name.Equals("TriX"))
                {
                    throw new RdfParseException("Unexpected Document Element '" + context.XmlReader.Name + "' encountered, expected the Document Element of a TriX Document to be the <TriX> element");
                }

                if (!context.XmlReader.HasAttributes)
                {
                    throw new RdfParseException("<TriX> fails to define any attributes, the element must define the xmlns attribute to be the TriX namespace");
                }
                bool trixNsDefined = false;
                for (int i = 0; i < context.XmlReader.AttributeCount; i++)
                {
                    context.XmlReader.MoveToNextAttribute();

                    if (context.XmlReader.Name.Equals("xmlns"))
                    {
                        //Ensure that the xmlns attribute is defined and is the TriX namespace
                        if (trixNsDefined) throw new RdfParseException("The xmlns attribute can only occur once on the <TriX> element");
                        if (!context.XmlReader.Value.Equals(TriXNamespaceUri)) throw new RdfParseException("The xmlns attribute of the <TriX> element must have it's value set to the TriX Namespace URI which is '" + TriXNamespaceUri + "'");
                        trixNsDefined = true;
                    }
                    else if (context.XmlReader.LocalName.Equals("xmlns"))
                    {
                        //Don't think we need to do anything here
                    }
                }

                if (!trixNsDefined) throw new RdfParseException("The <TriX> element fails to define the required xmlns attribute defining the TriX Namespace");

                //Process Child Nodes
                while (context.XmlReader.Read())
                {
                    if (context.XmlReader.NodeType == XmlNodeType.Element)
                    {
                        //For elements we recurse to try and parse a Graph
                        this.TryParseGraph(context);
                    }
                    else if (context.XmlReader.NodeType == XmlNodeType.EndElement)
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
                if (context.XmlReader.NodeType == XmlNodeType.EndElement)
                {
                    if (!context.XmlReader.Name.Equals("TriX")) throw new RdfParseException("Expected </TriX> element was not found, encountered </" + context.XmlReader.Name + "> instead");
                }
                else
                {
                    throw new RdfParseException("Unexpected Note Type " + context.XmlReader.NodeType.ToString() + " encountered, expected a </TriX> element");
                }
                context.Handler.EndRdf(true);
            }
            catch (RdfParsingTerminatedException)
            {
                context.Handler.EndRdf(true);
                //Discard this - it justs means the Handler told us to stop
            }
            catch
            {
                context.Handler.EndRdf(false);
                throw;
            }
        }

        private void TryParseGraph(TriXStreamingParserContext context)
        {
            //Ensure Node Name is correct
            if (!context.XmlReader.Name.Equals("graph"))
            {
                throw new RdfParseException("Unexpected Element <" + context.XmlReader.Name + "> encountered, only <graph> elements are permitted within a <TriX> element");
            }

            //Check whether this Graph is actually asserted
            for (int i = 0; i < context.XmlReader.AttributeCount; i++)
            {
                context.XmlReader.MoveToNextAttribute();
                if (!context.XmlReader.Name.Equals("asserted")) continue;
                bool asserted = true;
                if (!Boolean.TryParse(context.XmlReader.Value, out asserted)) continue;
                //Don't process this Graph further if it is not being asserted
                //i.e. it is only (potentially) being quoted
                if (asserted) continue;
                this.RaiseWarning("A Graph is marked as not asserted in the TriX input.  This Graph will not be parsed, if you reserialize the input the information contained in it will not be preserved");
                return;
            }

            //See if we get an <id>/<uri> node to name the Graph
            context.XmlReader.Read();
            IGraph g = null;
            INode graphName = null;
            if (context.XmlReader.NodeType == XmlNodeType.Element)
            {
                //Process the name into a Graph Uri and create the Graph and add it to the Store
                if (context.XmlReader.Name.Equals("uri"))
                {
                    //TODO: Add support for reading Base Uri from xml:base attributes in the file
                    graphName = context.Handler.CreateUriNode(UriFactory.ResolveUri(context.XmlReader.ReadInnerXml(), null));
                }
            }

            //If the next element is a </graph> then this is an empty graph and we should return
            if (context.XmlReader.NodeType == XmlNodeType.EndElement)
            {
                if (context.XmlReader.Name.Equals("graph"))
                {
                    return;
                }
                throw Error("Unexpected </" + context.XmlReader.Name + "> encountered, either <triple> elements or a </graph> was expected", context.XmlReader);
            }

            //Process the Child Nodes of the <graph> element to yield Triples
            do
            {
                //Remember to ignore anything that isn't an element i.e. comments and processing instructions
                if (context.XmlReader.NodeType == XmlNodeType.Element)
                {
                    this.TryParseTriple(context, graphName);
                }

                context.XmlReader.Read();
            } while (context.XmlReader.NodeType != XmlNodeType.EndElement);

            if (!context.XmlReader.Name.Equals("graph")) throw Error("Expected a </graph> but a </" + context.XmlReader.Name + "> was encountered", context.XmlReader);
        }

        private void TryParseTriple(TriXStreamingParserContext context, INode graphName)
        {
            //Verify Node Name
            if (!context.XmlReader.Name.Equals("triple"))
            {
                throw Error("Unexpected Element <" + context.XmlReader.Name + "> encountered, only an optional <id>/<uri> element followed by zero/more <triple> elements are permitted within a <graph> element", context.XmlReader);
            }

            //Parse XML Nodes into RDF Nodes
            INode subj = this.TryParseNode(context, QuadSegment.Subject);
            INode pred = this.TryParseNode(context, QuadSegment.Predicate);
            INode obj = this.TryParseNode(context, QuadSegment.Object);

            if (context.XmlReader.NodeType != XmlNodeType.EndElement) throw Error("Unexpected element type " + context.XmlReader.NodeType.ToString() + " encountered, expected the </triple> element", context.XmlReader);
            if (!context.XmlReader.Name.Equals("triple")) throw Error("Unexpected </" + context.XmlReader.Name + "> encountered, expected a </triple> element", context.XmlReader);

            //Assert the resulting Triple
            if (!context.Handler.HandleQuad(new Quad(subj, pred, obj, graphName))) ParserHelper.Stop();
        }

        private INode TryParseNode(TriXStreamingParserContext context, QuadSegment segment)
        {
            //Only need to Read() if getting the Subject
            //The previous calls will have resulted in us already reading to the start element for this node
            if (segment == QuadSegment.Subject) context.XmlReader.Read();

            if (context.XmlReader.NodeType != XmlNodeType.Element)
            {
                if (context.XmlReader.NodeType == XmlNodeType.EndElement) throw Error("Unexpected end element while trying to parse the nodes of a <triple> element", context.XmlReader);
            }

            if (context.XmlReader.Name.Equals("uri"))
            {
                return context.Handler.CreateUriNode(new Uri(context.XmlReader.ReadInnerXml()));
            }
            if (context.XmlReader.Name.Equals("id"))
            {
                if (segment == QuadSegment.Predicate) throw Error("Unexpected element <" + context.XmlReader.Name + "> encountered, expected a <uri> element as the Predicate of a Triple", context.XmlReader);

                return context.BlankNodeGenerator.GetGuid(context.XmlReader.ReadInnerXml());
            }
            if (context.XmlReader.Name.Equals("plainLiteral"))
            {
                if (segment == QuadSegment.Subject) throw Error("Unexpected element <" + context.XmlReader.Name + "> encountered, expected a <id>/<uri> element as the Subject of a Triple", context.XmlReader);

                if (context.XmlReader.AttributeCount > 0)
                {
                    String lang = String.Empty;
                    for (int i = 0; i < context.XmlReader.AttributeCount; i++)
                    {
                        context.XmlReader.MoveToNextAttribute();
                        if (context.XmlReader.Name.Equals("xml:lang")) lang = context.XmlReader.Value;
                    }
                    context.XmlReader.MoveToContent();
                    if (!lang.Equals(String.Empty))
                    {
                        return context.Handler.CreateLiteralNode(context.XmlReader.ReadElementContentAsString(), lang);
                    }
                    return context.Handler.CreateLiteralNode(context.XmlReader.ReadElementContentAsString());
                }
                return context.Handler.CreateLiteralNode(context.XmlReader.ReadElementContentAsString());
            }
            if (!context.XmlReader.Name.Equals("typedLiteral"))
            {
                throw Error("Unexpected element <" + context.XmlReader.Name + "> encountered, expected a <id>/<uri>/<plainLiteral>/<typedLiteral> element as part of a Triple", context.XmlReader);
            }
            if (context.XmlReader.AttributeCount <= 0)
            {
                throw Error("<typedLiteral> element does not have the required datatype attribute", context.XmlReader);
            }
            Uri dtUri = null;
            for (int i = 0; i < context.XmlReader.AttributeCount; i++)
            {
                context.XmlReader.MoveToNextAttribute();
                if (context.XmlReader.Name.Equals("datatype")) dtUri = new Uri(context.XmlReader.Value);
            }
            if (dtUri == null) throw Error("<typedLiteral> element does not have the required datatype attribute", context.XmlReader);

            context.XmlReader.MoveToContent();
            return context.Handler.CreateLiteralNode(context.XmlReader.ReadInnerXml(), dtUri);
        }

        /// <summary>
        /// Helper method for raising informative standardised Parser Errors
        /// </summary>
        /// <param name="msg">The Error Message</param>
        /// <param name="reader">XML Reader</param>
        /// <returns></returns>
        private RdfParseException Error(String msg, XmlReader reader)
        {
            StringBuilder output = new StringBuilder();
            if (reader is IXmlLineInfo)
            {
                IXmlLineInfo info = (IXmlLineInfo) reader;
                output.AppendLine(string.Format("[Source XML - Line {0} Column {1}]", info.LineNumber, info.LinePosition));
            }
            else
            {
                output.AppendLine("[Source XML]");
            }
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
            RdfReaderWarning d = this.Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event which Readers can raise when they notice syntax that is ambigious/deprecated etc which can still be parsed
        /// </summary>
        public event RdfReaderWarning Warning;

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