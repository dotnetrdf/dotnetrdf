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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using VDS.RDF.Namespaces;
using VDS.RDF.Specifications;

namespace VDS.RDF.Parsing.Events.RdfXml
{
    /// <summary>
    /// A JIT event generator for RDF/XML parsing that uses Streaming parsing to parse the events
    /// </summary>
    /// <remarks>
    /// Currently unimplemented stub class
    /// </remarks>
    public class StreamingEventGenerator
        : IRdfXmlJitEventGenerator
    {
        private readonly XmlReader _reader;
        private bool _requireEndElement = false;
        private IRdfXmlEvent _rootEl;
        private bool _noRead = false;
        private bool _first = true;
        private bool _parseLiteral = false;
        private bool _rdfRootSeen = false;
        private bool _stop = false;
        private readonly bool _hasLineInfo = false;
        private Uri _currentBaseUri = null;
        private readonly Stack<Uri> _baseUris = new Stack<Uri>();

        /// <summary>
        /// Creates a new Streaming Event Generator
        /// </summary>
        /// <param name="stream">Stream</param>
        public StreamingEventGenerator(Stream stream)
        {
            this._reader = XmlReader.Create(stream, this.GetSettings());
            this._hasLineInfo = (this._reader is IXmlLineInfo);
        }

        /// <summary>
        /// Creates a new Streaming Event Generator
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="baseUri">Base URI</param>
        public StreamingEventGenerator(Stream stream, Uri baseUri)
            : this(stream)
        {
            this._currentBaseUri = baseUri;
        }

        /// <summary>
        /// Creates a new Streaming Event Generator
        /// </summary>
        /// <param name="reader">Text Reader</param>
        public StreamingEventGenerator(TextReader reader)
        {
            this._reader = XmlReader.Create(reader, this.GetSettings());
            this._hasLineInfo = (this._reader is IXmlLineInfo);
        }

        /// <summary>
        /// Creates a new Streaming Event Generator
        /// </summary>
        /// <param name="reader">Text Reader</param>
        /// <param name="baseUri">Base URI</param>
        public StreamingEventGenerator(TextReader reader, Uri baseUri)
            : this(reader)
        {
            this._currentBaseUri = baseUri;
        }

        /// <summary>
        /// Initialises the XML Reader settings
        /// </summary>
        /// <returns></returns>
        private XmlReaderSettings GetSettings()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
#if PORTABLE
            settings.DtdProcessing = DtdProcessing.Ignore;
#elif SILVERLIGHT || NET40
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
        /// Gets the next event from the XML stream
        /// </summary>
        /// <returns></returns>
        public IRdfXmlEvent GetNextEvent()
        {
            if (this._stop) return null;

            //If the Root Element is filled then return it
            if (this._rootEl != null)
            {
                IRdfXmlEvent temp = this._rootEl;
                this._rootEl = null;
                return temp;
            }

            //If Literal Parsing flag is set then we've just read an element which had rdf:parseType="Literal"
            if (this._parseLiteral)
            {
                this._requireEndElement = true;
                this._parseLiteral = false;
                this._noRead = true;
                this._reader.MoveToContent();
                String data = this._reader.ReadInnerXml();
                return new TypedLiteralEvent(data, RdfSpecsHelper.RdfXmlLiteral, data, this.GetPosition());
            }

            //If we need to return an end element then do so
            if (this._requireEndElement)
            {
                this._requireEndElement = false;
                this._currentBaseUri = this._baseUris.Pop();
                return new EndElementEvent(this.GetPosition());
            }

            //If at EOF throw an error
            if (this._reader.EOF) throw new RdfParseException("Unable to read further events as the end of the stream has already been reached");

            //Otherwise attempt to read the next node
            bool read = true;
            if (!this._noRead)
            {
                read = this._reader.Read();
            }
            else
            {
                this._noRead = false;
            }
            if (read)
            {
                //Return the appropriate event for the Node Type
                switch (this._reader.NodeType)
                {
                    case XmlNodeType.Element:
                        //Element
                        if (this._first)
                        {
                            this._first = false;
                            this._rdfRootSeen = this.IsName("RDF", NamespaceMapper.RDF);
                            this._rootEl = this.GetElement();
                            RootEvent root = new RootEvent(this.GetBaseUri(), this._reader.Value, this.GetPosition());
                            root.DocumentElement = (ElementEvent)this._rootEl;
                            root.Children.Add((ElementEvent)this._rootEl);

                            if (ReferenceEquals(root.BaseUri, null))
                            {
                                root.BaseUri = this._currentBaseUri;                                
                            }

                            return root;
                        }
                        else
                        {
                            if (!this._first && this.IsName("RDF", NamespaceMapper.RDF))
                            {
                                if (this._rdfRootSeen) throw new RdfParseException("Unexpected nested rdf:RDF node encountered, this is not valid RDF/XML syntax");
                                this._noRead = true;
                                this._first = true;
                                return new ClearQueueEvent();
                            }
                            return this.GetElement();
                        }

                    case XmlNodeType.EndElement:
                        //End of an Element
                        this._currentBaseUri = this._baseUris.Pop();
                        if (this.IsName("RDF", NamespaceMapper.RDF))
                        {
                            this._stop = true;
                        }
                        return new EndElementEvent(this.GetPosition());

                    case XmlNodeType.Attribute:
                        //Attribute
                        throw new RdfParseException("Unexpected Attribute Node encountered");

                    case XmlNodeType.Text:
                        return new TextEvent(this._reader.Value, this._reader.Value, this.GetPosition());

                    case XmlNodeType.CDATA:
                        return new TextEvent(this._reader.Value, this._reader.Value, this.GetPosition());

                    case XmlNodeType.Document:
                    case XmlNodeType.DocumentType:
                    case XmlNodeType.XmlDeclaration:
                    case XmlNodeType.Comment:
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Notation:
                    case XmlNodeType.Whitespace:
                        //Node Types that don't generate events and just indicate to continue reading
                        return this.GetNextEvent();

                    default:
                        throw new RdfParseException("Unexpected XML Node Type " + this._reader.NodeType.ToString() + " encountered");
                }
            }
            else
            {
                return null;
            }
        }

        private Uri GetBaseUri()
        {
            this._baseUris.Push(this._currentBaseUri);
            if (String.IsNullOrEmpty(this._reader.BaseURI))
            {
                return this._currentBaseUri;
            }
            else
            {
                return UriFactory.Create(this._reader.BaseURI);
            }
        }

        private IRdfXmlEvent GetNextAttribute()
        {
            this._reader.MoveToNextAttribute();
            if (this.IsName("lang", XmlSpecsHelper.NamespaceXml))
            {
                //Generate an event for xml:lang
                return new LanguageAttributeEvent(this._reader.Value, this._reader.Value, this.GetPosition());
            }
            else if (this.IsName("base", XmlSpecsHelper.NamespaceXml))
            {
                //Generate an event for xml:base
                return new XmlBaseAttributeEvent(this._reader.Value, this._reader.Value, this.GetPosition());
            }
            else if (this.IsInNamespace(XmlSpecsHelper.NamespaceXmlNamespaces))
            {
                //Return a Namespace Attribute Event
                if (this._reader.LocalName.Equals("xmlns"))
                {
                    return new NamespaceAttributeEvent(String.Empty, UriFactory.Create(this._reader.Value), this._reader.Value, this.GetPosition());
                }
                else
                {
                    return new NamespaceAttributeEvent(this._reader.LocalName, UriFactory.Create(this._reader.Value), this._reader.Value, this.GetPosition());
                }
            }
            else if (this.IsInNamespace(XmlSpecsHelper.NamespaceXml) || (this._reader.NamespaceURI.Equals(String.Empty) && this._reader.Name.StartsWith("xml")))
            {
                //Ignore other XML reserved names
                return null;
            }
            else if (this.IsName("parseType", NamespaceMapper.RDF))
            {
                //Support Parse Type by returning an appropriate event
                switch (this._reader.Value)
                {
                    case "Resource":
                        return new ParseTypeAttributeEvent(RdfXmlParseType.Resource, this._reader.Value, this.GetPosition());
                    case "Collection":
                        return new ParseTypeAttributeEvent(RdfXmlParseType.Collection, this._reader.Value, this.GetPosition());
                    case "Literal":
                    default:
                        this._parseLiteral = true;
                        return new ParseTypeAttributeEvent(RdfXmlParseType.Literal, this._reader.Value, this.GetPosition());
                }
            }
            else
            {
                //Normal attribute
                return new AttributeEvent(this._reader.Name, this._reader.Value, this._reader.Value, this.GetPosition());
            }
        }

        private IRdfXmlEvent GetElement()
        {
            //Generate Element Event
            ElementEvent el = new ElementEvent(this._reader.Name, this.GetBaseUri(), this._reader.Value, this.GetPosition());
            this._requireEndElement = this._reader.IsEmptyElement;

            //Read Attribute Events
            if (this._reader.HasAttributes)
            {
                for (int i = 0; i < this._reader.AttributeCount; i++)
                {
                    IRdfXmlEvent attr = this.GetNextAttribute();
                    if (attr is AttributeEvent)
                    {
                        el.Attributes.Add((AttributeEvent)attr);
                    }
                    else if (attr is NamespaceAttributeEvent)
                    {
                        el.NamespaceAttributes.Add((NamespaceAttributeEvent)attr);
                    }
                    else if (attr is LanguageAttributeEvent)
                    {
                        el.Language = ((LanguageAttributeEvent)attr).Language;
                    }
                    else if (attr is ParseTypeAttributeEvent)
                    {
                        el.ParseType = ((ParseTypeAttributeEvent)attr).ParseType;
                        el.Attributes.Add(new AttributeEvent(this._reader.Name, this._reader.Value, this._reader.Value, this.GetPosition()));
                    }
                    else if (attr is XmlBaseAttributeEvent)
                    {
                        el.BaseUri = UriFactory.Create(((XmlBaseAttributeEvent)attr).BaseUri);
                        this._currentBaseUri = el.BaseUri;
                    }
                }
            }

            //Validate generated Attributes for Namespace Confusion and URIRef encoding
            foreach (AttributeEvent a in el.Attributes)
            {
                //Namespace Confusion should only apply to Attributes without a Namespace specified
                if (a.Namespace.Equals(String.Empty))
                {
                    if (RdfXmlSpecsHelper.IsAmbigiousAttributeName(a.LocalName))
                    {
                        //Can't use any of the RDF terms that mandate the rdf: prefix without it
                        throw new RdfParseException("An Attribute with an ambigious name '" + a.LocalName + "' was encountered.  The following attribute names MUST have the rdf: prefix - about, aboutEach, ID, bagID, type, resource, parseType");
                    }
                }

                //URIRef encoding check
                if (!RdfXmlSpecsHelper.IsValidUriRefEncoding(a.Value))
                {
                    throw new RdfParseException("An Attribute with an incorrectly encoded URIRef was encountered, URIRef's must be encoded in Unicode Normal Form C");
                }
            }

            return el;
        }

        private PositionInfo GetPosition()
        {
            if (this._hasLineInfo)
            {
                return new PositionInfo((IXmlLineInfo)this._reader);
            }
            else
            {
                return null;
            }
        }

        private bool IsName(String localName, String namespaceUri)
        {
            return this._reader.LocalName.Equals(localName) && this._reader.NamespaceURI.Equals(namespaceUri);
        }

        private bool IsInNamespace(String namespaceUri)
        {
            return this._reader.NamespaceURI.Equals(namespaceUri);
        }

        /// <summary>
        /// Gets whether the event generator has finished generating events
        /// </summary>
        public bool Finished
        {
            get
            {
                return this._stop || this._reader.EOF;
            }
        }
    }
}
