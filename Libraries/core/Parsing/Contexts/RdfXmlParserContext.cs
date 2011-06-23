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
using VDS.RDF.Parsing.Events;
using VDS.RDF.Parsing.Events.RdfXml;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Parser Context for RDF/XML Parser
    /// </summary>
    public class RdfXmlParserContext : BaseParserContext, IEventParserContext<IRdfXmlEvent>
    {
        private IEventQueue<IRdfXmlEvent> _queue;
        private Dictionary<String, List<INode>> _usedIDs = new Dictionary<String, List<INode>>();

#if !NO_XMLDOM

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="document">XML Document</param>
        public RdfXmlParserContext(IGraph g, XmlDocument document)
            : this(g, document, false) { }

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="document">XML Document</param>
        /// <param name="traceParsing">Whether to Trace Parsing</param>
        public RdfXmlParserContext(IGraph g, XmlDocument document, bool traceParsing)
            : base(g) 
        {
            this._queue = new EventQueue<IRdfXmlEvent>(new DomBasedEventGenerator(document));
            if (this._queue.EventGenerator is IRdfXmlPreProcessingEventGenerator)
            {
                ((IRdfXmlPreProcessingEventGenerator)this._queue.EventGenerator).GetAllEvents(this);
            }
        }

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="document">XML Document</param>
        public RdfXmlParserContext(IRdfHandler handler, XmlDocument document)
            : this(handler, document, false) { }

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="document">XML Document</param>
        /// <param name="traceParsing">Whether to Trace Parsing</param>
        public RdfXmlParserContext(IRdfHandler handler, XmlDocument document, bool traceParsing)
            : base(handler)
        {
            this._queue = new EventQueue<IRdfXmlEvent>(new DomBasedEventGenerator(document));
            if (this._queue.EventGenerator is IRdfXmlPreProcessingEventGenerator)
            {
                ((IRdfXmlPreProcessingEventGenerator)this._queue.EventGenerator).GetAllEvents(this);
            }
        }

#endif

        /// <summary>
        /// Creates a new Parser Context which uses Streaming parsing
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="stream">Stream</param>
        public RdfXmlParserContext(IGraph g, Stream stream)
            : base(g)
        {
            this._queue = new StreamingEventQueue<IRdfXmlEvent>(new StreamingEventGenerator(stream, g.BaseUri.ToSafeString()));
        }

        /// <summary>
        /// Creates a new Parser Context which uses Streaming parsing
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="stream">Stream</param>
        public RdfXmlParserContext(IRdfHandler handler, Stream stream)
            : base(handler)
        {
            this._queue = new StreamingEventQueue<IRdfXmlEvent>(new StreamingEventGenerator(stream, String.Empty));
        }

        /// <summary>
        /// Creates a new Parser Context which uses Streaming parsing
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="input">Input</param>
        public RdfXmlParserContext(IGraph g, TextReader input)
            : base(g)
        {
            this._queue = new StreamingEventQueue<IRdfXmlEvent>(new StreamingEventGenerator(input, g.BaseUri.ToSafeString()));
        }

        /// <summary>
        /// Creates a new Parser Context which uses Streaming parsing
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="input">Input</param>
        public RdfXmlParserContext(IRdfHandler handler, TextReader input)
            : base(handler)
        {
            this._queue = new StreamingEventQueue<IRdfXmlEvent>(new StreamingEventGenerator(input, String.Empty));
        }

        /// <summary>
        /// Gets the Event Queue
        /// </summary>
        public IEventQueue<IRdfXmlEvent> Events
        {
            get
            {
                return this._queue;
            }
        }

        /// <summary>
        /// Gets the Mapping of in-use IDs
        /// </summary>
        public Dictionary<String, List<INode>> IDs
        {
            get
            {
                return this._usedIDs;
            }
        }
    }
}
