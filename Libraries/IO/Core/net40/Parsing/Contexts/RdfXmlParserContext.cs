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
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing.Events;
using VDS.RDF.Parsing.Events.RdfXml;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Parser Context for RDF/XML Parser
    /// </summary>
    public class RdfXmlParserContext :
        BaseParserContext, IEventParserContext<IRdfXmlEvent>
    {
#if !NO_XMLDOM

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="document">XML Document</param>
        public RdfXmlParserContext(IRdfHandler handler, XmlDocument document, IParserProfile profile)
            : this(handler, document, false, profile)
        {
        }

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="document">XML Document</param>
        /// <param name="traceParsing">Whether to Trace Parsing</param>
        public RdfXmlParserContext(IRdfHandler handler, XmlDocument document, bool traceParsing, IParserProfile profile)
            : base(handler, profile)
        {
            IDs = new Dictionary<String, List<INode>>();
            this.Events = new EventQueue<IRdfXmlEvent>(new DomBasedEventGenerator(document));
            if (this.Events.EventGenerator is IRdfXmlPreProcessingEventGenerator)
            {
                ((IRdfXmlPreProcessingEventGenerator) this.Events.EventGenerator).GetAllEvents(this);
            }
        }

#endif

        /// <summary>
        /// Creates a new Parser Context which uses Streaming parsing
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="stream">Stream</param>
        public RdfXmlParserContext(IRdfHandler handler, Stream stream, IParserProfile profile)
            : base(handler, profile)
        {
            IDs = new Dictionary<String, List<INode>>();
            this.Events = new StreamingEventQueue<IRdfXmlEvent>(new StreamingEventGenerator(stream, profile.BaseUri));
        }

        /// <summary>
        /// Creates a new Parser Context which uses Streaming parsing
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="input">Input</param>
        public RdfXmlParserContext(IRdfHandler handler, TextReader input, IParserProfile profile)
            : base(handler, profile)
        {
            IDs = new Dictionary<String, List<INode>>();
            this.Events = new StreamingEventQueue<IRdfXmlEvent>(new StreamingEventGenerator(input, profile.BaseUri));
        }

        /// <summary>
        /// Gets the Event Queue
        /// </summary>
        public IEventQueue<IRdfXmlEvent> Events { get; private set; }

        /// <summary>
        /// Gets the Mapping of in-use IDs
        /// </summary>
        public Dictionary<string, List<INode>> IDs { get; private set; }
    }
}