/*

Copyright Robert Vesse 2009-11
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

#if UNFINISHED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Events.RdfA
{
    /// <summary>
    /// Static Class which defines the Event Types for RDFa Events
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Constants for Event Types
        /// </summary>
        public const int Element = 1,
                         EndElement = -1,

                         Text = 5,
                         XmlLiteral = 10;
    }

    public class ElementEvent : BaseRdfAEvent
    {
        private String _name;

        public ElementEvent(String name, IEnumerable<KeyValuePair<String, String>> attributes, PositionInfo pos)
            : base(Event.Element, pos, attributes)
        {
            this._name = name;
        }

        public ElementEvent(String name, IEnumerable<KeyValuePair<String, String>> attributes)
            : this(name, attributes, null) { }

        public String Name
        {
            get
            {
                return this._name;
            }
        }

        public override string ToString()
        {
            return "[Element] " + this._name;
        }
    }

    public class EndElementEvent : BaseRdfAEvent
    {
        public EndElementEvent(PositionInfo pos)
            : base(Event.EndElement, pos, Enumerable.Empty<KeyValuePair<String, String>>()) { }

        public EndElementEvent()
            : this(null) { }

        public override string ToString()
        {
            return "[End Element]";
        }
    }

    public class XmlLiteralEvent : BaseRdfAEvent
    {
        private String _literal;

        public XmlLiteralEvent(String literal, PositionInfo pos)
            : base(Event.XmlLiteral, pos, Enumerable.Empty<KeyValuePair<String, String>>())
        {
            this._literal = literal;
        }

        public XmlLiteralEvent(String literal)
            : this(literal, null) { }

        public String XmlLiteral
        {
            get
            {
                return this._literal;
            }
        }
    }

    public class TextEvent : BaseRdfAEvent
    {
        private String _text;

        public TextEvent(String text, PositionInfo pos)
            : base(Event.Text, pos, Enumerable.Empty<KeyValuePair<String, String>>())
        {
            this._text = text;        
        }

        public TextEvent(String text)
            : this(text, null) { }

        public String Text
        {
            get
            {
                return this._text;
            }
        }

        public override string ToString()
        {
            return "[Text] " + this._text;
        }
    }
}

#endif