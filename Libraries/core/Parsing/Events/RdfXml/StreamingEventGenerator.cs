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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using VDS.RDF.Parsing.Contexts;

namespace VDS.RDF.Parsing.Events.RdfXml
{
    /// <summary>
    /// A JIT event generator for RDF/XML parsing that uses Streaming parsing to parse the events
    /// </summary>
    /// <remarks>
    /// Currently unimplemented stub class
    /// </remarks>
    public class StreamingEventGenerator : IRdfXmlJitEventGenerator
    {
        private XmlReader _reader;
        private bool _requireEndElement = false;
        private IRdfXmlEvent _rootEl;
        private bool _noRead = false;
        private bool _first = true;
        private bool _parseLiteral = false;
        private bool _rdfRootSeen = false;
        private bool _stop = false;
        private bool _hasLineInfo = false;
        private String _currentBaseUri = String.Empty;
        private Stack<String> _baseUris = new Stack<string>();

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
        public StreamingEventGenerator(Stream stream, String baseUri)
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
        public StreamingEventGenerator(TextReader reader, String baseUri)
            : this(reader)
        {
            this._currentBaseUri = baseUri;
        }

        /// <summary>
        /// Creates a new Streaming Event Generator
        /// </summary>
        /// <param name="file">Filename</param>
        public StreamingEventGenerator(String file)
        {
            this._reader = XmlReader.Create(new FileStream(file, FileMode.Open), this.GetSettings());
            this._hasLineInfo = (this._reader is IXmlLineInfo);
        }

        /// <summary>
        /// Creates a new Streaming Event Generator
        /// </summary>
        /// <param name="file">Filename</param>
        /// <param name="baseUri">Base URI</param>
        public StreamingEventGenerator(String file, String baseUri)
            : this(file)
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
#if SILVERLIGHT
            settings.DtdProcessing = DtdProcessing.Parse;
            //settings.XmlResolver = new Xml
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
                            root.Children.Add((ElementEvent)this._rootEl);
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

        private String GetBaseUri()
        {
            this._baseUris.Push(this._currentBaseUri);
            if (this._reader.BaseURI.Equals(String.Empty))
            {
                return this._currentBaseUri;
            }
            else
            {
                return this._reader.BaseURI;
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
                    return new NamespaceAttributeEvent(String.Empty, this._reader.Value, this._reader.Value, this.GetPosition());
                }
                else
                {
                    return new NamespaceAttributeEvent(this._reader.LocalName, this._reader.Value, this._reader.Value, this.GetPosition());
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
                        el.BaseUri = ((XmlBaseAttributeEvent)attr).BaseUri;
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
