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
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Xunit;


namespace VDS.RDF.Writing.Serialization
{
    public class TripleSerializationTests
    {
        [Fact]
        public void SerializationXmlTriple()
        {
            System.IO.StringWriter writer = new System.IO.StringWriter();
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
            Assert.NotNull(t2);
            Console.WriteLine("Deserialized Form: " + t2.ToString());
            Assert.Equal(t, t2);
        }

        [Fact]
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

            Assert.NotNull(t2);
            Console.WriteLine("Deserialized Form: " + t2.ToString());
            Assert.Equal(t, t2);

            stream.Dispose();
        }
    }
}
