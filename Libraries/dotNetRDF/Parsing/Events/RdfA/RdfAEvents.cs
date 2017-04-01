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