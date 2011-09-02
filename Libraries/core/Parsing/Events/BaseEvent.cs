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

    /// <summary>
    /// Abstract Base Class for <see cref="IRdfAEvent">IRdfAEvent</see> implementations
    /// </summary>
    public abstract class BaseRdfAEvent 
        : BaseEvent, IRdfAEvent
    {
        private Dictionary<String, String> _attributes;

        /// <summary>
        /// Creates a new RDFa Event
        /// </summary>
        /// <param name="eventType">Event Type</param>
        /// <param name="pos">Position Info</param>
        /// <param name="attributes">Attributes</param>
        public BaseRdfAEvent(int eventType, PositionInfo pos, IEnumerable<KeyValuePair<String, String>> attributes)
            : base(eventType, pos)
        {
            this._attributes = new Dictionary<string, string>();
            foreach (KeyValuePair<String, String> attr in attributes)
            {
                this._attributes.Add(attr.Key, attr.Value);
            }
        }

        /// <summary>
        /// Gets the attributes of the event i.e. the attributes of the source element
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Attributes
        {
            get 
            {
                return this._attributes; 
            }
        }

        /// <summary>
        /// Gets whether the Event has a given attribute
        /// </summary>
        /// <param name="name">Attribute Name</param>
        /// <returns></returns>
        public bool HasAttribute(String name)
        {
            return this._attributes.ContainsKey(name);
        }

        /// <summary>
        /// Gets the value of a specific attribute
        /// </summary>
        /// <param name="name">Attribute Name</param>
        /// <returns></returns>
        public String this[String name]
        {
            get
            {
                return this._attributes[name];
            }
        }
    }
}
