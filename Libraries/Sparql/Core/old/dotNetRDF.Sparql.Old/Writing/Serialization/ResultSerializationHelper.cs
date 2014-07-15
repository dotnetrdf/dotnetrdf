using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using VDS.RDF.Query;

namespace VDS.RDF.Writing.Serialization
{
    class ResultSerializationHelper
    {
        private static XmlSerializer _resultSerializer = new XmlSerializer(typeof(SparqlResult));

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
                throw new RdfException("Failed to deserialize a SPARQL Result correctly");
            }
        }
    }
}
