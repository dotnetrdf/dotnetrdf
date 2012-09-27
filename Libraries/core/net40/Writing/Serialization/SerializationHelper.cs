/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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

#if !SILVERLIGHT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Writing.Serialization
{
    /// <summary>
    /// Helper Class for use in serialization and deserialization
    /// </summary>
    static class SerializationHelper
    {
        private static Dictionary<Type, XmlSerializer> _nodeSerializers = new Dictionary<Type, XmlSerializer>();
        private static Dictionary<String, XmlSerializer> _nodeDeserializers = new Dictionary<String, XmlSerializer>();
        private static XmlSerializer _graphSerializer = new XmlSerializer(typeof(Graph));
        private static XmlSerializer _resultSerializer = new XmlSerializer(typeof(SparqlResult));
        private static bool _init = false;

        private static void Init()
        {
            if (_init) return;

            if (!_nodeDeserializers.ContainsKey("bnode")) _nodeDeserializers.Add("bnode", new XmlSerializer(typeof(BlankNode)));
            if (!_nodeDeserializers.ContainsKey("literal")) _nodeDeserializers.Add("literal", new XmlSerializer(typeof(LiteralNode)));
            if (!_nodeDeserializers.ContainsKey("uri")) _nodeDeserializers.Add("uri", new XmlSerializer(typeof(UriNode)));
            if (!_nodeDeserializers.ContainsKey("graphliteral")) _nodeDeserializers.Add("graphliteral", new XmlSerializer(typeof(GraphLiteralNode)));
            if (!_nodeDeserializers.ContainsKey("variable")) _nodeDeserializers.Add("variable", new XmlSerializer(typeof(VariableNode)));

            _init = true;
        }

        internal static void SerializeNode(this INode n, XmlWriter writer)
        {
            Type t = n.GetType();
            //Get the Serializer if necessary
            if (!_nodeSerializers.ContainsKey(t))
            {
                _nodeSerializers.Add(t, new XmlSerializer(t));
                String el = t.GetCustomAttributes(typeof(XmlRootAttribute), false).FirstOrDefault().ToSafeString();
                if (el.Equals(String.Empty)) el = t.Name;
                if (!_nodeDeserializers.ContainsKey(el))
                {
                    _nodeDeserializers.Add(el, _nodeSerializers[t]);
                }
            }
            //Do the serialization
            _nodeSerializers[t].Serialize(writer, n);
        }

        internal static INode DeserializeNode(this XmlReader reader)
        {
            String el = reader.Name;
            //Get the deserializer if necessary
            if (!_nodeDeserializers.ContainsKey(el))
            {
                if (!_init) Init();
            }
            if (!_nodeDeserializers.ContainsKey(el)) throw new RdfParseException("No deserializer is known for elements named '" + el + "'");
            //Do the deserialization
            Object temp = _nodeDeserializers[el].Deserialize(reader);
            if (temp is INode)
            {
                return (INode)temp;
            }
            else
            {
                throw new RdfParseException("Failed to deserialize a node correctly");
            }
        }

        internal static void SerializeGraph(this IGraph g, XmlWriter writer)
        {
            _graphSerializer.Serialize(writer, g);
        }

        internal static IGraph DeserializeGraph(this XmlReader reader)
        {
            Object temp = _graphSerializer.Deserialize(reader);
            if (temp is IGraph)
            {
                return (IGraph)temp;
            }
            else
            {
                throw new RdfParseException("Failed to deserialize a graph correctly");
            }
        }

        internal static void SerializeResult(this SparqlResult result, XmlWriter writer)
        {
            _resultSerializer.Serialize(writer, result);
        }

        internal static SparqlResult DeserializeResult(this XmlReader reader)
        {
            Object temp = _resultSerializer.Deserialize(reader);
            if (temp is SparqlResult)
            {
                return (SparqlResult)temp;
            }
            else
            {
                throw new RdfParseException("Failed to deserialize a SPARQL Result correctly");
            }
        }
    }
}


#endif