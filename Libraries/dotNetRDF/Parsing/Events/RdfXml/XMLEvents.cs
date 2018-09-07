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
using System.Linq;

namespace VDS.RDF.Parsing.Events.RdfXml
{
    /// <summary>
    /// Static Class which defines the Event Types for RDF/XML Events
    /// </summary>
    public static class RdfXmlEvent
    {
        /// <summary>
        /// Constants for Event Types
        /// </summary>
        public const int Root = 0,
                         Clear = 1,

                         Element = 10,
                         EndElement = 11,

                         Attribute = 20,
                         NamespaceAttribute = 21,
                         LanguageAttribute = 22,
                         ParseTypeAttribute = 23,
                         XmlBaseAttribute = 24,

                         Text = 25,

                         UriReference = 30,
                         QName = 31,

                         BlankNodeID = 35,

                         Literal = 40,
                         TypedLiteral = 41;

    }

    /// <summary>
    /// Event representing the Root Node of the Document
    /// </summary>
    public class RootEvent : BaseRdfXmlEvent
    {
        private ElementEvent _docelement;
        private List<ElementEvent> _children = new List<ElementEvent>();
        private String _baseuri = String.Empty;
        private String _language = String.Empty;

        /// <summary>
        /// Creates a new Root Event
        /// </summary>
        /// <param name="baseUri">Base Uri of the Document</param>
        /// <param name="sourceXml">Source XML of the Document</param>
        /// <param name="pos">Position Info</param>
        public RootEvent(String baseUri, String sourceXml, PositionInfo pos) 
            : base(RdfXmlEvent.Root, sourceXml, pos)
        {
            _baseuri = baseUri;
        }

        /// <summary>
        /// Creates a new Root Event
        /// </summary>
        /// <param name="baseUri">Base Uri of the Document</param>
        /// <param name="sourceXml">Source XML of the Document</param>
        public RootEvent(String baseUri, String sourceXml)
            : this(baseUri, sourceXml, null) { }

        /// <summary>
        /// Gets/Sets the ElementEvent that represents the actual DocumentElement
        /// </summary>
        public ElementEvent DocumentElement
        {
            get
            {
                return _docelement;
            }
            set
            {
                _docelement = value;
            }
        }

        /// <summary>
        /// Gets all the Child ElementEvents of the Document Root
        /// </summary>
        public List<ElementEvent> Children
        {
            get
            {
                return _children;
            }
        }

        /// <summary>
        /// Gets the Base Uri of the Node
        /// </summary>
        public String BaseUri
        {
            get
            {
                return _baseuri;
            }
            set
            {
                _baseuri = value;
            }
        }

        /// <summary>
        /// Gets the Language of the Node
        /// </summary>
        public String Language
        {
            get
            {
                return _language;
            }
            set
            {
                _language = value;
            }
        }
        
    }

    /// <summary>
    /// Event representing a Node from the XML Document
    /// </summary>
    public class ElementEvent : BaseRdfXmlEvent 
    {
        private List<IRdfXmlEvent> _children = new List<IRdfXmlEvent>();
        private String _baseuri = String.Empty;
        private String _localname, _namespace;
        private List<AttributeEvent> _attributes = new List<AttributeEvent>();
        private List<NamespaceAttributeEvent> _namespaces = new List<NamespaceAttributeEvent>();
        private String _language = String.Empty;
        private int _listcounter = 1;
        private IRdfXmlEvent _subject = null;
        private RdfXmlParseType _parsetype = RdfXmlParseType.None;
        private INode _subjectNode = null;

        /// <summary>
        /// Creates a new Element Event
        /// </summary>
        /// <param name="qname">QName of the XML Node</param>
        /// <param name="baseUri">Base Uri of the XML Node</param>
        /// <param name="sourceXml">Source XML of the XML Node</param>
        /// <param name="pos">Position Info</param>
        public ElementEvent(String qname, String baseUri, String sourceXml, PositionInfo pos)
            : base(RdfXmlEvent.Element, sourceXml, pos) {
            _baseuri = baseUri;

            if (qname.Contains(':'))
            {
                // Has a Namespace
                // Split the QName into Namespace and Local Name
                String[] parts = qname.Split(':');
                _namespace = parts[0];
                _localname = parts[1];
            }
            else
            {
                // Is in the Default Namespace
                _namespace = String.Empty;
                _localname = qname;
            }
        }

        /// <summary>
        /// Creates a new Element Event
        /// </summary>
        /// <param name="qname">QName of the XML Node</param>
        /// <param name="baseUri">Base Uri of the XML Node</param>
        /// <param name="sourceXml">Source XML of the XML Node</param>
        public ElementEvent(String qname, String baseUri, String sourceXml)
            : this(qname, baseUri, sourceXml, (PositionInfo)null) { }

        /// <summary>
        /// Creates new Element Event
        /// </summary>
        /// <param name="localname">Local Name of the XML Node</param>
        /// <param name="ns">Namespace Prefix of the XML Node</param>
        /// <param name="baseUri">Base Uri of the XML Node</param>
        /// <param name="sourceXml">Source XML of the XML Node</param>
        /// <param name="pos">Position Info</param>
        public ElementEvent(String localname, String ns, String baseUri, String sourceXml, PositionInfo pos)
            : base(RdfXmlEvent.Element, sourceXml, pos)
        {
            _baseuri = baseUri;
            _localname = localname;
            _namespace = ns;
        }

        /// <summary>
        /// Creates new Element Event
        /// </summary>
        /// <param name="localname">Local Name of the XML Node</param>
        /// <param name="ns">Namespace Prefix of the XML Node</param>
        /// <param name="baseUri">Base Uri of the XML Node</param>
        /// <param name="sourceXml">Source XML of the XML Node</param>
        public ElementEvent(String localname, String ns, String baseUri, String sourceXml)
            : this(localname, ns, baseUri, sourceXml, null) { }

        /// <summary>
        /// Gets the Local Name of this Element Event
        /// </summary>
        public String LocalName
        {
            get
            {
                return _localname;
            }
        }

        /// <summary>
        /// Gets the Namespace of this Element Event
        /// </summary>
        public String Namespace
        {
            get
            {
                return _namespace;
            }
        }

        /// <summary>
        /// Gets the QName of this Element Event
        /// </summary>
        public String QName
        {
            get
            {
                return _namespace + ":" + _localname;
            }
        }

        /// <summary>
        /// Gets the Child Element Events 
        /// </summary>
        /// <remarks>These correspond to the Child Nodes of the XML Node</remarks>
        public List<IRdfXmlEvent> Children
        {
            get
            {
                return _children;
            }
        }

        /// <summary>
        /// Gets/Sets the Base Uri of the XML Node
        /// </summary>
        public String BaseUri
        {
            get
            {
                return _baseuri;
            }
            set
            {
                _baseuri = value;
            }
        }

        /// <summary>
        /// Gets the Attribute Events
        /// </summary>
        /// <remarks>These correspond to the Attributes of the XML Node (with some exceptions as defined in the RDF/XML specification)</remarks>
        public List<AttributeEvent> Attributes
        {
            get
            {
                return _attributes;
            }
        }

        /// <summary>
        /// Gets the Namespace Attribute Events
        /// </summary>
        /// <remarks>
        /// These correspond to all the Namespace Attributes of the XML Node
        /// </remarks>
        public List<NamespaceAttributeEvent> NamespaceAttributes
        {
            get
            {
                return _namespaces;
            }
        }

        /// <summary>
        /// Gets/Sets the List Counter
        /// </summary>
        public int ListCounter
        {
            get
            {
                return _listcounter;
            }
            set
            {
                _listcounter = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Language of this Event
        /// </summary>
        public String Language
        {
            get
            {
                return _language;
            }
            set
            {
                _language = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Subject Event of this Event
        /// </summary>
        /// <remarks>Will be assigned according to the Parsing rules during the Parsing process and later used to generate a Subject Node</remarks>
        public IRdfXmlEvent Subject
        {
            get
            {
                return _subject;
            }
            set
            {
                _subject = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Subject Node of this Event
        /// </summary>
        /// <remarks>Will be created from the Subject at some point during the Parsing process</remarks>
        public INode SubjectNode
        {
            get
            {
                return _subjectNode;
            }
            set
            {
                _subjectNode = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Parse Type for this Event
        /// </summary>
        public RdfXmlParseType ParseType
        {
            get
            {
                return _parsetype;
            }
            set
            {
                _parsetype = value;
            }
        }

        /// <summary>
        /// Method which sets the Uri for this Element Event
        /// </summary>
        /// <param name="u">Uri Reference to set Uri from</param>
        /// <remarks>This can only be used on Elements which are rdf:li and thus need expanding into actual list elements according to List Expansion rules.  Attempting to set the Uri on any other Element Event will cause an Error message.</remarks>
        public void SetUri(UriReferenceEvent u)
        {
            if (QName.Equals("rdf:li"))
            {
                // Split the QName into Namespace and Local Name
                String qname = u.Identifier;
                String[] parts = qname.Split(':');
                _namespace = parts[0];
                _localname = parts[1];
            }
            else
            {
                throw new RdfParseException("It is forbidden to change the URI of an Element Event unless it is a rdf:li Element and thus needs expanding to the form rdf:_X according to List Expansion rules");
            }
        }

        /// <summary>
        /// Method which sets the Uri for this Element Event
        /// </summary>
        /// <param name="u">Uri Reference to set Uri from</param>
        /// <param name="nsMapper">Namespace prefix mappings to use for resolving the QName of this element event</param>
        /// <remarks>This can only be used on Elements which are rdf:li and thus need expanding into actual list elements according to List Expansion rules.  Attempting to set the Uri on any other Element Event will cause an Error message.</remarks>
        public void SetUri(UriReferenceEvent u, INamespaceMapper nsMapper)
        {
            if (RdfXmlSpecsHelper.IsLiElement(this, nsMapper))
            {
                // Split the QName into Namespace and Local Name
                String qname = u.Identifier;
                String[] parts = qname.Split(':');
                _namespace = parts[0];
                _localname = parts[1];
            }
            else
            {
                throw new RdfParseException("It is forbidden to change the URI of an Element Event unless it is a rdf:li Element and thus needs expanding to the form rdf:_X according to List Expansion rules");
            }
        }

        /// <summary>
        /// Gets the String representation of the Event
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[Element] " + _namespace + ":" + _localname;
        }

    }

    /// <summary>
    /// An Event for representing the End of Elements
    /// </summary>
    public class EndElementEvent : BaseRdfXmlEvent
    {
        /// <summary>
        /// Creates a new EndElementEvent
        /// </summary>
        public EndElementEvent(PositionInfo pos) 
            : base(RdfXmlEvent.EndElement, String.Empty, pos) { }

        /// <summary>
        /// Creates a new EndElementEvent
        /// </summary>
        public EndElementEvent()
            : this(null) { }
    }

    /// <summary>
    /// An Event for representing Attributes of XML Node
    /// </summary>
    public class AttributeEvent : BaseRdfXmlEvent
    {
        private String _localname, _namespace;
        private String _value;

        /// <summary>
        /// Creates a new Attribute Event from an XML Attribute
        /// </summary>
        /// <param name="qname">QName of the Attribute</param>
        /// <param name="value">Value of the Attribute</param>
        /// <param name="sourceXml">Source XML of the Attribute</param>
        /// <param name="pos">Position Info</param>
        public AttributeEvent(String qname, String value, String sourceXml, PositionInfo pos)
            : base(RdfXmlEvent.Attribute, sourceXml, pos)
        {
            _value = value;
            if (qname.Contains(':'))
            {
                // Has a Namespace
                // Split the QName into Namespace and Local Name
                String[] parts = qname.Split(':');
                _namespace = parts[0];
                _localname = parts[1];
            }
            else
            {
                // Is in the Default Namespace
                _namespace = String.Empty;
                _localname = qname;
            }
        }

        /// <summary>
        /// Creates a new Attribute Event from an XML Attribute
        /// </summary>
        /// <param name="qname">QName of the Attribute</param>
        /// <param name="value">Value of the Attribute</param>
        /// <param name="sourceXml">Source XML of the Attribute</param>
        public AttributeEvent(String qname, String value, String sourceXml)
            : this(qname, value, sourceXml, (PositionInfo)null) { }

        /// <summary>
        /// Creates a new Attribute Event from an XML Attribute
        /// </summary>
        /// <param name="localname">Local Name of the Attribute</param>
        /// <param name="ns">Namespace Prefix of the Attribute</param>
        /// <param name="value">Value of the Attribute</param>
        /// <param name="sourceXml">Source XML of the Attribute</param>
        /// <param name="pos">Position Info</param>
        public AttributeEvent(String localname, String ns, String value, String sourceXml, PositionInfo pos)
            : base(RdfXmlEvent.Attribute, sourceXml, pos)
        {
            _value = value;
            _localname = localname;
            _namespace = ns;
        }

        /// <summary>
        /// Creates a new Attribute Event from an XML Attribute
        /// </summary>
        /// <param name="localname">Local Name of the Attribute</param>
        /// <param name="ns">Namespace Prefix of the Attribute</param>
        /// <param name="value">Value of the Attribute</param>
        /// <param name="sourceXml">Source XML of the Attribute</param>
        public AttributeEvent(String localname, String ns, String value, String sourceXml)
            : this(localname, ns, value, sourceXml, null) { }

        /// <summary>
        /// Gets the Local Name of the Attribute
        /// </summary>
        public String LocalName
        {
            get
            {
                return _localname;
            }
        }

        /// <summary>
        /// Gets the Namespace Prefix of the Attribute
        /// </summary>
        public String Namespace
        {
            get
            {
                return _namespace;
            }
        }

        /// <summary>
        /// Gets the QName of the Attribute
        /// </summary>
        public String QName
        {
            get
            {
                return _namespace + ":" + _localname;
            }
        }

        /// <summary>
        /// Gets the Value of the Attribute
        /// </summary>
        public String Value
        {
            get
            {
                return _value;
            }
        }
    }

    /// <summary>
    /// An Event for representing Namespace Attributes of an XML Node
    /// </summary>
    public class NamespaceAttributeEvent : BaseRdfXmlEvent
    {
        private String _prefix, _uri;

        /// <summary>
        /// Creates a new Namespace Attribute Event
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="uri">Namespace Uri</param>
        /// <param name="sourceXml">Source XML</param>
        /// <param name="pos">Position Info</param>
        public NamespaceAttributeEvent(String prefix, String uri, String sourceXml, PositionInfo pos)
            : base(RdfXmlEvent.NamespaceAttribute, sourceXml, pos)
        {
            _prefix = prefix;
            _uri = uri;
        }

        /// <summary>
        /// Creates a new Namespace Attribute Event
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="uri">Namespace Uri</param>
        /// <param name="sourceXml">Source XML</param>
        public NamespaceAttributeEvent(String prefix, String uri, String sourceXml)
            : this(prefix, uri, sourceXml, null) { }

        /// <summary>
        /// Gets the Namespace Prefix
        /// </summary>
        public String Prefix
        {
            get
            {
                return _prefix;
            }
        }

        /// <summary>
        /// Gets the Namespace Uri
        /// </summary>
        public String Uri
        {
            get
            {
                return _uri;
            }
        }
    }

    /// <summary>
    /// An Event for representing Language Attributes of an XML Node
    /// </summary>
    public class LanguageAttributeEvent : BaseRdfXmlEvent
    {
        private String _lang;

        /// <summary>
        /// Creates a new Language Attribute Event
        /// </summary>
        /// <param name="lang">Language</param>
        /// <param name="sourceXml">Source XML</param>
        /// <param name="pos">Position Info</param>
        public LanguageAttributeEvent(String lang, String sourceXml, PositionInfo pos)
            : base(RdfXmlEvent.LanguageAttribute, sourceXml, pos)
        {
            _lang = lang;
        }

        /// <summary>
        /// Creates a new Language Attribute Event
        /// </summary>
        /// <param name="lang">Language</param>
        /// <param name="sourceXml">Source XML</param>
        public LanguageAttributeEvent(String lang, String sourceXml)
            : this(lang, sourceXml, null) { }

        /// <summary>
        /// Gets the Language
        /// </summary>
        public String Language
        {
            get
            {
                return _lang;
            }
        }
    }

    /// <summary>
    /// An Event for representing rdf:parseType Attributes of an XML Node
    /// </summary>
    public class ParseTypeAttributeEvent : BaseRdfXmlEvent
    {
        private RdfXmlParseType _type;

        /// <summary>
        /// Creates a new Parse Type Attribute Event
        /// </summary>
        /// <param name="type">Parse Type</param>
        /// <param name="sourceXml">Source XML</param>
        /// <param name="pos">Position Info</param>
        public ParseTypeAttributeEvent(RdfXmlParseType type, String sourceXml, PositionInfo pos)
            : base(RdfXmlEvent.ParseTypeAttribute, sourceXml, pos)
        {
            _type = type;
        }

        /// <summary>
        /// Creates a new Parse Type Attribute Event
        /// </summary>
        /// <param name="type">Parse Type</param>
        /// <param name="sourceXml">Source XML</param>
        public ParseTypeAttributeEvent(RdfXmlParseType type, String sourceXml)
            : this(type, sourceXml, null) { }

        /// <summary>
        /// Gets the Parse Type
        /// </summary>
        public RdfXmlParseType ParseType
        {
            get
            {
                return _type;
            }
        }
    }

    /// <summary>
    /// An Event for representing xml:base attributes of XML Nodes
    /// </summary>
    public class XmlBaseAttributeEvent : BaseRdfXmlEvent
    {
        private String _baseUri;

        /// <summary>
        /// Creates a new XML Base Attribute
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <param name="sourceXml">Source XML</param>
        /// <param name="pos">Position Info</param>
        public XmlBaseAttributeEvent(String baseUri, String sourceXml, PositionInfo pos)
            : base(RdfXmlEvent.XmlBaseAttribute, sourceXml, pos)
        {
            _baseUri = baseUri;
        }

        /// <summary>
        /// Creates a new XML Base Attribute
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <param name="sourceXml">Source XML</param>
        public XmlBaseAttributeEvent(String baseUri, String sourceXml)
            : this(baseUri, sourceXml, null) { }

        /// <summary>
        /// Gets the Base URI
        /// </summary>
        public String BaseUri
        {
            get
            {
                return _baseUri;
            }
        }
    }

    /// <summary>
    /// Event for representing plain text content (XML Text Nodes)
    /// </summary>
    public class TextEvent : BaseRdfXmlEvent
    {
        private String _value;

        /// <summary>
        /// Creates a new Text Node
        /// </summary>
        /// <param name="value">Textual Content of the XML Text Node</param>
        /// <param name="sourceXml">Source XML of the Node</param>
        /// <param name="pos">Position Info</param>
        public TextEvent(String value, String sourceXml, PositionInfo pos)
            : base(RdfXmlEvent.Text, sourceXml, pos)
        {
            _value = value;
        }

        /// <summary>
        /// Creates a new Text Node
        /// </summary>
        /// <param name="value">Textual Content of the XML Text Node</param>
        /// <param name="sourceXml">Source XML of the Node</param>
        public TextEvent(String value, String sourceXml)
            : this(value, sourceXml, null) { }

        /// <summary>
        /// Gets the Textual Content of the Event
        /// </summary>
        public String Value
        {
            get
            {
                return _value;
            }
        }

        /// <summary>
        /// Gets the String representation of the Event
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[Text] " + _value;
        }
    }

    /// <summary>
    /// Event for representing URIRefs
    /// </summary>
    public class UriReferenceEvent : BaseRdfXmlEvent
    {
        private String _id;

        /// <summary>
        /// Creates a new URIRef Event from a URIRef in an XML Attribute value or similar
        /// </summary>
        /// <param name="identifier">URIRef</param>
        /// <param name="sourceXml">Source XML of the URIRef</param>
        /// <param name="pos">Position Info</param>
        public UriReferenceEvent(String identifier, String sourceXml, PositionInfo pos)
            : base(RdfXmlEvent.UriReference, sourceXml, pos)
        {
            _id = identifier;
        }

        /// <summary>
        /// Creates a new URIRef Event from a URIRef in an XML Attribute value or similar
        /// </summary>
        /// <param name="identifier">URIRef</param>
        /// <param name="sourceXml">Source XML of the URIRef</param>
        public UriReferenceEvent(String identifier, String sourceXml)
            : this(identifier, sourceXml, null) { }

        /// <summary>
        /// Gets the URIRef
        /// </summary>
        public String Identifier
        {
            get
            {
                return _id;
            }
        }
    }

    /// <summary>
    /// Event for representing QNames
    /// </summary>
    public class QNameEvent : BaseRdfXmlEvent
    {
        private String _qname;

        /// <summary>
        /// Creates a new QName Event
        /// </summary>
        /// <param name="qname">QName</param>
        /// <param name="sourceXml">Source XML of the QName</param>
        /// <param name="pos">Position Info</param>
        public QNameEvent(String qname, String sourceXml, PositionInfo pos)
            : base(RdfXmlEvent.QName, sourceXml, pos)
        {
            _qname = qname;
        }

        /// <summary>
        /// Creates a new QName Event
        /// </summary>
        /// <param name="qname">QName</param>
        /// <param name="sourceXml">Source XML of the QName</param>
        public QNameEvent(String qname, String sourceXml)
            : this(qname, sourceXml, null) { }

        /// <summary>
        /// Gets the QName
        /// </summary>
        public String QName
        {
            get
            {
                return _qname;
            }
        }

    }

    /// <summary>
    /// Event for representing the need for a Blank Node
    /// </summary>
    public class BlankNodeIDEvent : BaseRdfXmlEvent
    {
        private String _id;

        /// <summary>
        /// Creates a new Blank Node ID Event for a named Blank Node
        /// </summary>
        /// <param name="identifier">Node ID for the Blank Node</param>
        /// <param name="sourceXml">Source XML</param>
        /// <param name="pos">Position Info</param>
        public BlankNodeIDEvent(String identifier, String sourceXml, PositionInfo pos)
            : base(RdfXmlEvent.BlankNodeID, sourceXml, pos)
        {
            _id = identifier;
        }

        /// <summary>
        /// Creates a new Blank Node ID Event for a named Blank Node
        /// </summary>
        /// <param name="identifier">Node ID for the Blank Node</param>
        /// <param name="sourceXml">Source XML</param>
        public BlankNodeIDEvent(String identifier, String sourceXml)
            : this(identifier, sourceXml, null) { }

        /// <summary>
        /// Creates a new Blank Node ID Event for an anonymous Blank Node
        /// </summary>
        /// <param name="sourceXml">Source XML</param>
        /// <param name="pos">Position Info</param>
        public BlankNodeIDEvent(String sourceXml, PositionInfo pos)
            : base(RdfXmlEvent.BlankNodeID, sourceXml, pos)
        {
            _id = String.Empty;
        }

        /// <summary>
        /// Creates a new Blank Node ID Event for an anonymous Blank Node
        /// </summary>
        /// <param name="sourceXml">Source XML</param>
        public BlankNodeIDEvent(String sourceXml)
            : this(sourceXml, (PositionInfo)null) { }

        /// <summary>
        /// Gets the Blank Node ID (if any)
        /// </summary>
        public String Identifier
        {
            get
            {
                return _id;
            }
        }
    }

    /// <summary>
    /// An Event for representing Plain Literals
    /// </summary>
    public class PlainLiteralEvent : BaseRdfXmlEvent
    {
        private String _value;
        private String _language;

        /// <summary>
        /// Creates a new Plain Literal Event
        /// </summary>
        /// <param name="value">Value of the Literal</param>
        /// <param name="language">Language Specifier of the Literal</param>
        /// <param name="sourceXml">Source XML of the Event</param>
        /// <param name="pos">Position Info</param>
        public PlainLiteralEvent(String value, String language, String sourceXml, PositionInfo pos)
            : base(RdfXmlEvent.Literal, sourceXml, pos)
        {
            _value = value;
            _language = language;
        }

        /// <summary>
        /// Creates a new Plain Literal Event
        /// </summary>
        /// <param name="value">Value of the Literal</param>
        /// <param name="language">Language Specifier of the Literal</param>
        /// <param name="sourceXml">Source XML of the Event</param>
        public PlainLiteralEvent(String value, String language, String sourceXml)
            : this(value, language, sourceXml, null) { }

        /// <summary>
        /// Gets the Value of the Plain Literal
        /// </summary>
        public String Value
        {
            get
            {
                return _value;
            }
        }

        /// <summary>
        /// Gets the Langugage Specifier of the Plain Literal
        /// </summary>
        public String Language
        {
            get
            {
                return _language;
            }
            set
            {
                _language = value;
            }
        }
    }

    /// <summary>
    /// An Event for representing Typed Literals
    /// </summary>
    public class TypedLiteralEvent : BaseRdfXmlEvent
    {
        private String _value;
        private String _datatype;

        /// <summary>
        /// Creates a new Typed Literal Event
        /// </summary>
        /// <param name="value">Value of the Literal</param>
        /// <param name="datatype">DataType Uri of the Literal</param>
        /// <param name="sourceXml">Source XML of the Event</param>
        /// <param name="pos">Position Info</param>
        public TypedLiteralEvent(String value, String datatype, String sourceXml, PositionInfo pos)
            : base(RdfXmlEvent.TypedLiteral, sourceXml, pos)
        {
            _value = value;
            _datatype = datatype;
        }

        /// <summary>
        /// Creates a new Typed Literal Event
        /// </summary>
        /// <param name="value">Value of the Literal</param>
        /// <param name="datatype">DataType Uri of the Literal</param>
        /// <param name="sourceXml">Source XML of the Event</param>
        public TypedLiteralEvent(String value, String datatype, String sourceXml)
            : this(value, datatype, sourceXml, null) { }

        /// <summary>
        /// Gets the Value of the Typed Literal
        /// </summary>
        public String Value
        {
            get
            {
                return _value;
            }
        }

        /// <summary>
        /// Gets the DataType of the Typed Literal
        /// </summary>
        public String DataType
        {
            get
            {
                return _datatype;
            }
            set
            {
                _datatype = value;
            }
        }

        /// <summary>
        /// Gets the String representation of the Event
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[Literal] " + _value.ToString();
        }
    }

    /// <summary>
    /// An Event for representing that the Event Queue should be cleared of previously queued events
    /// </summary>
    internal class ClearQueueEvent : BaseRdfXmlEvent
    {
        /// <summary>
        /// Creates a new Clear Queue Event
        /// </summary>
        public ClearQueueEvent()
            : base(RdfXmlEvent.Clear, String.Empty) { }
    }
}