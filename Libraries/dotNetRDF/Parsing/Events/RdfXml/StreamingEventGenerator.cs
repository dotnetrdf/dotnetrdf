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
using System.Xml;

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
            _reader = XmlReader.Create(stream, GetSettings());
            _hasLineInfo = (_reader is IXmlLineInfo);
        }

        /// <summary>
        /// Creates a new Streaming Event Generator
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="baseUri">Base URI</param>
        public StreamingEventGenerator(Stream stream, String baseUri)
            : this(stream)
        {
            _currentBaseUri = baseUri;
        }

        /// <summary>
        /// Creates a new Streaming Event Generator
        /// </summary>
        /// <param name="reader">Text Reader</param>
        public StreamingEventGenerator(TextReader reader)
        {
            _reader = XmlReader.Create(reader, GetSettings());
            _hasLineInfo = (_reader is IXmlLineInfo);
        }

        /// <summary>
        /// Creates a new Streaming Event Generator
        /// </summary>
        /// <param name="reader">Text Reader</param>
        /// <param name="baseUri">Base URI</param>
        public StreamingEventGenerator(TextReader reader, String baseUri)
            : this(reader)
        {
            _currentBaseUri = baseUri;
        }

        /// <summary>
        /// Creates a new Streaming Event Generator
        /// </summary>
        /// <param name="file">Filename</param>
        public StreamingEventGenerator(String file)
        {
            _reader = XmlReader.Create(new FileStream(file, FileMode.Open), GetSettings());
            _hasLineInfo = (_reader is IXmlLineInfo);
        }

        /// <summary>
        /// Creates a new Streaming Event Generator
        /// </summary>
        /// <param name="file">Filename</param>
        /// <param name="baseUri">Base URI</param>
        public StreamingEventGenerator(String file, String baseUri)
            : this(file)
        {
            _currentBaseUri = baseUri;
        }

        /// <summary>
        /// Initialises the XML Reader settings
        /// </summary>
        /// <returns></returns>
        private XmlReaderSettings GetSettings()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
#if NETCORE 
            settings.DtdProcessing = DtdProcessing.Ignore;
#elif NET40 || NETSTANDARD2_0
            settings.DtdProcessing = DtdProcessing.Parse;
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
            if (_stop) return null;

            // If the Root Element is filled then return it
            if (_rootEl != null)
            {
                IRdfXmlEvent temp = _rootEl;
                _rootEl = null;
                return temp;
            }

            // If Literal Parsing flag is set then we've just read an element which had rdf:parseType="Literal"
            if (_parseLiteral)
            {
                _requireEndElement = true;
                _parseLiteral = false;
                _noRead = true;
                _reader.MoveToContent();
                String data = _reader.ReadInnerXml();
                return new TypedLiteralEvent(data, RdfSpecsHelper.RdfXmlLiteral, data, GetPosition());
            }

            // If we need to return an end element then do so
            if (_requireEndElement)
            {
                _requireEndElement = false;
                _currentBaseUri = _baseUris.Pop();
                return new EndElementEvent(GetPosition());
            }

            // If at EOF throw an error
            if (_reader.EOF) throw new RdfParseException("Unable to read further events as the end of the stream has already been reached");

            // Otherwise attempt to read the next node
            bool read = true;
            if (!_noRead)
            {
                read = _reader.Read();
            }
            else
            {
                _noRead = false;
            }
            if (read)
            {
                // Return the appropriate event for the Node Type
                switch (_reader.NodeType)
                {
                    case XmlNodeType.Element:
                        // Element
                        if (_first)
                        {
                            _first = false;
                            _rdfRootSeen = IsName("RDF", NamespaceMapper.RDF);
                            _rootEl = GetElement();
                            RootEvent root = new RootEvent(GetBaseUri(), _reader.Value, GetPosition());
                            root.DocumentElement = (ElementEvent)_rootEl;
                            root.Children.Add((ElementEvent)_rootEl);

                            if (root.BaseUri.Equals(String.Empty))
                            {
                                root.BaseUri = _currentBaseUri;                                
                            }

                            return root;
                        }
                        else
                        {
                            if (!_first && IsName("RDF", NamespaceMapper.RDF))
                            {
                                if (_rdfRootSeen) throw new RdfParseException("Unexpected nested rdf:RDF node encountered, this is not valid RDF/XML syntax");
                                _noRead = true;
                                _first = true;
                                return new ClearQueueEvent();
                            }
                            return GetElement();
                        }

                    case XmlNodeType.EndElement:
                        // End of an Element
                        _currentBaseUri = _baseUris.Pop();
                        if (IsName("RDF", NamespaceMapper.RDF))
                        {
                            _stop = true;
                        }
                        return new EndElementEvent(GetPosition());

                    case XmlNodeType.Attribute:
                        // Attribute
                        throw new RdfParseException("Unexpected Attribute Node encountered");

                    case XmlNodeType.Text:
                        return new TextEvent(_reader.Value, _reader.Value, GetPosition());

                    case XmlNodeType.CDATA:
                        return new TextEvent(_reader.Value, _reader.Value, GetPosition());

                    case XmlNodeType.Document:
                    case XmlNodeType.DocumentType:
                    case XmlNodeType.XmlDeclaration:
                    case XmlNodeType.Comment:
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Notation:
                    case XmlNodeType.Whitespace:
                        // Node Types that don't generate events and just indicate to continue reading
                        return GetNextEvent();

                    default:
                        throw new RdfParseException("Unexpected XML Node Type " + _reader.NodeType.ToString() + " encountered");
                }
            }
            else
            {
                return null;
            }
        }

        private String GetBaseUri()
        {
            _baseUris.Push(_currentBaseUri);
            if (_reader.BaseURI.Equals(String.Empty))
            {
                return _currentBaseUri;
            }
            else
            {
                return _reader.BaseURI;
            }
        }

        private IRdfXmlEvent GetNextAttribute()
        {
            _reader.MoveToNextAttribute();
            if (IsName("lang", XmlSpecsHelper.NamespaceXml))
            {
                // Generate an event for xml:lang
                return new LanguageAttributeEvent(_reader.Value, _reader.Value, GetPosition());
            }
            else if (IsName("base", XmlSpecsHelper.NamespaceXml))
            {
                // Generate an event for xml:base
                return new XmlBaseAttributeEvent(_reader.Value, _reader.Value, GetPosition());
            }
            else if (IsInNamespace(XmlSpecsHelper.NamespaceXmlNamespaces))
            {
                // Return a Namespace Attribute Event
                if (_reader.LocalName.Equals("xmlns"))
                {
                    return new NamespaceAttributeEvent(String.Empty, _reader.Value, _reader.Value, GetPosition());
                }
                else
                {
                    return new NamespaceAttributeEvent(_reader.LocalName, _reader.Value, _reader.Value, GetPosition());
                }
            }
            else if (IsInNamespace(XmlSpecsHelper.NamespaceXml) || (_reader.NamespaceURI.Equals(String.Empty) && _reader.Name.StartsWith("xml")))
            {
                // Ignore other XML reserved names
                return null;
            }
            else if (IsName("parseType", NamespaceMapper.RDF))
            {
                // Support Parse Type by returning an appropriate event
                switch (_reader.Value)
                {
                    case "Resource":
                        return new ParseTypeAttributeEvent(RdfXmlParseType.Resource, _reader.Value, GetPosition());
                    case "Collection":
                        return new ParseTypeAttributeEvent(RdfXmlParseType.Collection, _reader.Value, GetPosition());
                    case "Literal":
                    default:
                        _parseLiteral = true;
                        return new ParseTypeAttributeEvent(RdfXmlParseType.Literal, _reader.Value, GetPosition());
                }
            }
            else
            {
                if (string.IsNullOrEmpty(_reader.NamespaceURI) &&
                    RdfXmlSpecsHelper.IsAmbigiousAttributeName(_reader.LocalName))
                {
                    throw new RdfParseException("An Attribute with an ambiguous name '" + _reader.LocalName + "' was encountered.  The following attribute names MUST have the rdf: prefix - about, aboutEach, ID, bagID, type, resource, parseType");
                }
                // Normal attribute
                return new AttributeEvent(_reader.Name, _reader.Value, _reader.Value, GetPosition());
            }
        }

        private IRdfXmlEvent GetElement()
        {
            // Generate Element Event
            ElementEvent el = new ElementEvent(_reader.Name, GetBaseUri(), _reader.Value, GetPosition());
            _requireEndElement = _reader.IsEmptyElement;

            // Read Attribute Events
            if (_reader.HasAttributes)
            {
                for (int i = 0; i < _reader.AttributeCount; i++)
                {
                    IRdfXmlEvent attr = GetNextAttribute();
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
                        el.Attributes.Add(new AttributeEvent( _reader.LocalName, _reader.Prefix, _reader.Value, _reader.Value, GetPosition()));
                    }
                    else if (attr is XmlBaseAttributeEvent)
                    {
                        el.BaseUri = ((XmlBaseAttributeEvent)attr).BaseUri;
                        _currentBaseUri = el.BaseUri;
                    }
                }
            }

            // Validate generated Attributes for Namespace Confusion and URIRef encoding
            foreach (AttributeEvent a in el.Attributes)
            {
                // KA - Cannot perform ambiguous attribute verification without the current element's namespace map which is not available here
                /*
                // Namespace Confusion should only apply to Attributes without a Namespace specified
                if (a.Namespace.Equals(String.Empty))
                {
                    if (RdfXmlSpecsHelper.IsAmbigiousAttributeName(a.LocalName))
                    {
                        // Can't use any of the RDF terms that mandate the rdf: prefix without it
                        throw new RdfParseException("An Attribute with an ambigious name '" + a.LocalName + "' was encountered.  The following attribute names MUST have the rdf: prefix - about, aboutEach, ID, bagID, type, resource, parseType");
                    }
                }
                */
                // URIRef encoding check
                if (!RdfXmlSpecsHelper.IsValidUriRefEncoding(a.Value))
                {
                    throw new RdfParseException("An Attribute with an incorrectly encoded URIRef was encountered, URIRef's must be encoded in Unicode Normal Form C");
                }
            }

            return el;
        }

        private PositionInfo GetPosition()
        {
            if (_hasLineInfo)
            {
                return new PositionInfo((IXmlLineInfo)_reader);
            }
            else
            {
                return null;
            }
        }

        private bool IsName(String localName, String namespaceUri)
        {
            return _reader.LocalName.Equals(localName) && _reader.NamespaceURI.Equals(namespaceUri);
        }

        private bool IsInNamespace(String namespaceUri)
        {
            return _reader.NamespaceURI.Equals(namespaceUri);
        }

        /// <summary>
        /// Gets whether the event generator has finished generating events
        /// </summary>
        public bool Finished
        {
            get
            {
                return _stop || _reader.EOF;
            }
        }
    }
}
