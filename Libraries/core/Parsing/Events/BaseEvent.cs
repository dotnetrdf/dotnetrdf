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
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Events
{
    /// <summary>
    /// Abstract Base Class for IRdfXmlEvent implementations
    /// </summary>
    public abstract class BaseEvent : IRdfXmlEvent
    {
        private int _eventtype;
        private String _sourcexml;
        private PositionInfo _pos;

        /// <summary>
        /// Creates an Event and fills in its Values
        /// </summary>
        /// <param name="eventType">Type of the Event</param>
        /// <param name="sourceXml">Source XML that generated the Event</param>
        /// <param name="pos">Position of the XML Event</param>
        protected BaseEvent(int eventType, String sourceXml, PositionInfo pos)
        {
            this._eventtype = eventType;
            this._sourcexml = sourceXml;
            this._pos = pos;
        }

        /// <summary>
        /// Creates an Event and fills in its Values
        /// </summary>
        /// <param name="eventType">Type of the Event</param>
        /// <param name="sourceXml">Source XML that generated the Event</param>
        protected BaseEvent(int eventType, String sourceXml)
            : this(eventType, sourceXml, null) { }

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
        /// Gets the XML that this Event was generated from
        /// </summary>
        public string SourceXml
        {
            get 
            {
                return this._sourcexml;
            }
        }

        /// <summary>
        /// Gets the Position Information (if any)
        /// </summary>
        /// <remarks>
        /// Availability of Position Information depends on the how the XML was parsed
        /// </remarks>
        public PositionInfo Position
        {
            get
            {
                return this._pos;
            }
        }
    }
}
