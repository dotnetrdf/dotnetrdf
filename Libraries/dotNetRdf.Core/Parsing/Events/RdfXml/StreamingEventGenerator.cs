/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Xml;

namespace VDS.RDF.Parsing.Events.RdfXml;

/// <summary>
/// A JIT event generator for RDF/XML parsing that uses Streaming parsing to parse the events.
/// </summary>
/// <remarks>
/// Currently unimplemented stub class.
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
    private string _currentBaseUri = string.Empty;
    private Stack<string> _baseUris = new Stack<string>();

    /// <summary>
    /// Creates a new Streaming Event Generator.
    /// </summary>
    /// <param name="stream">Stream.</param>
    /// <param name="baseUri">Base URI.</param>
    /// <param name="xmlReaderSettings">The settings to pass through to the underlying XMLReader instance.</param>
    public StreamingEventGenerator(Stream stream, string baseUri = null, XmlReaderSettings xmlReaderSettings = null)
    {
        _reader = XmlReader.Create(stream, xmlReaderSettings ?? GetSettings());
        _hasLineInfo = (_reader is IXmlLineInfo);
        _currentBaseUri = baseUri ?? String.Empty;
    }

    /// <summary>
    /// Creates a new Streaming Event Generator.
    /// </summary>
    /// <param name="reader">Text Reader.</param>
    /// <param name="baseUri">Base URI.</param>
    /// <param name="xmlReaderSettings">The settings to pass through to the underlying XMLReader instance.</param>
    public StreamingEventGenerator(TextReader reader, string baseUri = null,XmlReaderSettings xmlReaderSettings = null)
    {
        _reader = XmlReader.Create(reader, xmlReaderSettings ?? GetSettings());
        _hasLineInfo = (_reader is IXmlLineInfo);
        _currentBaseUri = baseUri ?? string.Empty;
    }

    /// <summary>
    /// Creates a new Streaming Event Generator.
    /// </summary>
    /// <param name="file">Filename.</param>
    /// <param name="baseUri">Base URI.</param>
    /// <param name="xmlReaderSettings">Settings to pass through to the underlying XML parser.</param>
    public StreamingEventGenerator(string file, string baseUri = null, XmlReaderSettings xmlReaderSettings = null)
    {
        _reader = XmlReader.Create(new FileStream(file, FileMode.Open), xmlReaderSettings ?? GetSettings());
        _hasLineInfo = (_reader is IXmlLineInfo);
        _currentBaseUri = baseUri ?? string.Empty;
    }

    /// <summary>
    /// Returns the default XML Reader settings.
    /// </summary>
    /// <returns></returns>
    private XmlReaderSettings GetSettings()
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Parse,
            ConformanceLevel = ConformanceLevel.Document,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true,
        };
        return settings;
    }

    /// <summary>
    /// Gets the next event from the XML stream.
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
            var data = _reader.ReadInnerXml();
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
        var read = true;
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
                        var root = new RootEvent(GetBaseUri(), _reader.Value, GetPosition());
                        root.DocumentElement = (ElementEvent)_rootEl;
                        root.Children.Add((ElementEvent)_rootEl);

                        if (root.BaseUri.Equals(string.Empty))
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
                    throw new RdfParseException("Unexpected XML Node Type " + _reader.NodeType + " encountered");
            }
        }
        else
        {
            return null;
        }
    }

    private string GetBaseUri()
    {
        _baseUris.Push(_currentBaseUri);
        if (_reader.BaseURI.Equals(string.Empty))
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
                return new NamespaceAttributeEvent(string.Empty, _reader.Value, _reader.Value, GetPosition());
            }
            else
            {
                return new NamespaceAttributeEvent(_reader.LocalName, _reader.Value, _reader.Value, GetPosition());
            }
        }
        else if (IsInNamespace(XmlSpecsHelper.NamespaceXml) || (_reader.NamespaceURI.Equals(string.Empty) && _reader.Name.StartsWith("xml")))
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
        var el = new ElementEvent(_reader.Name, GetBaseUri(), _reader.Value, GetPosition());
        _requireEndElement = _reader.IsEmptyElement;

        // Read Attribute Events
        if (_reader.HasAttributes)
        {
            for (var i = 0; i < _reader.AttributeCount; i++)
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

    private bool IsName(string localName, string namespaceUri)
    {
        return _reader.LocalName.Equals(localName) && _reader.NamespaceURI.Equals(namespaceUri);
    }

    private bool IsInNamespace(string namespaceUri)
    {
        return _reader.NamespaceURI.Equals(namespaceUri);
    }

    /// <summary>
    /// Gets whether the event generator has finished generating events.
    /// </summary>
    public bool Finished
    {
        get
        {
            return _stop || _reader.EOF;
        }
    }
}
