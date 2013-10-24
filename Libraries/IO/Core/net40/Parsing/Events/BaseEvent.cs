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

namespace VDS.RDF.Parsing.Events
{
    /// <summary>
    /// Abstract Base Class for <see cref="IEvent">IEvent</see> implementations
    /// </summary>
    public abstract class BaseEvent 
        : IEvent
    {
        private int _eventtype;
        private PositionInfo _pos;

        /// <summary>
        /// Creates a new Event
        /// </summary>
        /// <param name="eventType">Event Type</param>
        /// <param name="info">Position Information</param>
        public BaseEvent(int eventType, PositionInfo info)
        {
            this._eventtype = eventType;
            this._pos = info;
        }

        /// <summary>
        /// Creates a new Event
        /// </summary>
        /// <param name="eventType">Event Type</param>
        public BaseEvent(int eventType)
            : this(eventType, null) { }

        /// <summary>
        /// Gets the Type for this Event
        /// </summary>
        public int EventType
        {
            get
            {
                return this._eventtype;
            }
        }

        /// <summary>
        /// Gets the Position Information (if any)
        /// </summary>
        /// <remarks>
        /// Availability of Position Information depends on the how the source document was parsed
        /// </remarks>
        public PositionInfo Position
        {
            get
            {
                return this._pos;
            }
        }
    }

    /// <summary>
    /// Abstract Base Class for <see cref="IRdfXmlEvent">IRdfXmlEvent</see> implementations
    /// </summary>
    public abstract class BaseRdfXmlEvent 
        : BaseEvent, IRdfXmlEvent
    {
        private String _sourcexml;

        /// <summary>
        /// Creates an Event and fills in its Values
        /// </summary>
        /// <param name="eventType">Type of the Event</param>
        /// <param name="sourceXml">Source XML that generated the Event</param>
        /// <param name="pos">Position of the XML Event</param>
        public BaseRdfXmlEvent(int eventType, String sourceXml, PositionInfo pos)
            : base(eventType, pos)
        {
            this._sourcexml = sourceXml;
        }

        /// <summary>
        /// Creates an Event and fills in its Values
        /// </summary>
        /// <param name="eventType">Type of the Event</param>
        /// <param name="sourceXml">Source XML that generated the Event</param>
        public BaseRdfXmlEvent(int eventType, String sourceXml)
            : this(eventType, sourceXml, null) { }

        /// <summary>
        /// Gets the XML that this Event was generated from
        /// </summary>
        public string SourceXml
        {
            get 
            {
                return this._sourcexml;
            }
        }
    }
}
