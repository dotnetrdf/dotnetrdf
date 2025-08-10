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

using System.Collections.Generic;
using System.IO;
using System.Xml;
using VDS.RDF.Parsing.Events;
using VDS.RDF.Parsing.Events.RdfXml;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing.Contexts;

/// <summary>
/// Parser Context for RDF/XML Parser.
/// </summary>
public class RdfXmlParserContext : BaseParserContext, IEventParserContext<IRdfXmlEvent>
{
    private IEventQueue<IRdfXmlEvent> _queue;
    private Dictionary<string, List<INode>> _usedIDs = new Dictionary<string, List<INode>>();

    /// <summary>
    /// Creates a new Parser Context.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <param name="document">XML Document.</param>
    public RdfXmlParserContext(IGraph g, XmlDocument document)
        : this(g, document, false) { }

    /// <summary>
    /// Creates a new Parser Context.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <param name="document">XML Document.</param>
    /// <param name="traceParsing">Whether to Trace Parsing.</param>
    public RdfXmlParserContext(IGraph g, XmlDocument document, bool traceParsing)
        : base(g) 
    {
        _queue = new EventQueue<IRdfXmlEvent>(new DomBasedEventGenerator(document));
        if (_queue.EventGenerator is IRdfXmlPreProcessingEventGenerator generator)
        {
            generator.GetAllEvents(this);
        }
    }

    /// <summary>
    /// Creates a new Parser Context.
    /// </summary>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="document">XML Document.</param>
    public RdfXmlParserContext(IRdfHandler handler, XmlDocument document)
        : this(handler, document, false) { }

    /// <summary>
    /// Creates a new Parser Context.
    /// </summary>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="document">XML Document.</param>
    /// <param name="traceParsing">Whether to Trace Parsing.</param>
    /// <param name="uriFactory">URI factory to use.</param>
    public RdfXmlParserContext(IRdfHandler handler, XmlDocument document, bool traceParsing, IUriFactory uriFactory = null)
        : base(handler, traceParsing, uriFactory ?? RDF.UriFactory.Root)
    {
        _queue = new EventQueue<IRdfXmlEvent>(new DomBasedEventGenerator(document));
        if (_queue.EventGenerator is IRdfXmlPreProcessingEventGenerator)
        {
            ((IRdfXmlPreProcessingEventGenerator)_queue.EventGenerator).GetAllEvents(this);
        }
    }

    /// <summary>
    /// Creates a new Parser Context which uses Streaming parsing.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <param name="stream">Stream.</param>
    public RdfXmlParserContext(IGraph g, Stream stream)
        : base(g)
    {
        _queue = new StreamingEventQueue<IRdfXmlEvent>(new StreamingEventGenerator(stream, g.BaseUri.ToSafeString()));
    }

    /// <summary>
    /// Creates a new Parser Context which uses Streaming parsing.
    /// </summary>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="stream">Stream.</param>
    public RdfXmlParserContext(IRdfHandler handler, Stream stream)
        : base(handler)
    {
        _queue = new StreamingEventQueue<IRdfXmlEvent>(new StreamingEventGenerator(stream, handler.GetBaseUri().ToSafeString()));
    }

    /// <summary>
    /// Creates a new Parser Context which uses Streaming parsing.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <param name="input">Input.</param>
    public RdfXmlParserContext(IGraph g, TextReader input)
        : base(g)
    {
        _queue = new StreamingEventQueue<IRdfXmlEvent>(new StreamingEventGenerator(input, g.BaseUri.ToSafeString()));
    }

    /// <summary>
    /// Creates a new Parser Context which uses Streaming parsing.
    /// </summary>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="input">Input.</param>
    /// <param name="uriFactory">URI Factory to use.</param>
    /// <param name="xmlReaderSettings">The settngs to pass through to the underlying XML parser.</param>
    public RdfXmlParserContext(IRdfHandler handler, TextReader input, IUriFactory uriFactory= null, XmlReaderSettings xmlReaderSettings = null)
        : base(handler, false, uriFactory ?? RDF.UriFactory.Root)
    {
        _queue = new StreamingEventQueue<IRdfXmlEvent>(new StreamingEventGenerator(input, handler.GetBaseUri().ToSafeString(), xmlReaderSettings));
    }

    /// <summary>
    /// Gets the Event Queue.
    /// </summary>
    public IEventQueue<IRdfXmlEvent> Events
    {
        get
        {
            return _queue;
        }
    }

    /// <summary>
    /// Gets the Mapping of in-use IDs.
    /// </summary>
    public Dictionary<string, List<INode>> IDs
    {
        get
        {
            return _usedIDs;
        }
    }
}
