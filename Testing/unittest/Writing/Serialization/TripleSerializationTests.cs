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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace VDS.RDF.Test.Writing.Serialization
{
    [TestClass]
    public class TripleSerializationTests
    {
        [TestMethod]
        public void SerializationXmlTriple()
        {
            StringWriter writer = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(Triple));

            Graph g = new Graph();
            INode s = g.CreateBlankNode();
            INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode o = g.CreateLiteralNode("object");
            Triple t = new Triple(s, p, o);

            Console.WriteLine("Input: " + t.ToString());

            serializer.Serialize(writer, t);
            Console.WriteLine("Serialized Form:");
            Console.WriteLine(writer.ToString());

            Triple t2 = serializer.Deserialize(new StringReader(writer.ToString())) as Triple;
            Assert.IsNotNull(t2, "Triple should not be null");
            Console.WriteLine("Deserialized Form: " + t2.ToString());
            Assert.AreEqual(t, t2, "Triples should be equal");
        }

        [TestMethod]
        public void SerializationBinaryTriple()
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter serializer = new BinaryFormatter();

            Graph g = new Graph();
            INode s = g.CreateBlankNode();
            INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode o = g.CreateLiteralNode("object");
            Triple t = new Triple(s, p, o);

            Console.WriteLine("Input: " + t.ToString());

            serializer.Serialize(stream, t);
            Console.WriteLine("Serialized Form:");
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(stream);
            Console.WriteLine(reader.ReadToEnd());

            stream.Seek(0, SeekOrigin.Begin);
            Triple t2 = serializer.Deserialize(stream) as Triple;

            reader.Close();

            Assert.IsNotNull(t2, "Triple should not be null");
            Console.WriteLine("Deserialized Form: " + t2.ToString());
            Assert.AreEqual(t, t2, "Triples should be equal");

            stream.Dispose();
        }
    }
}
