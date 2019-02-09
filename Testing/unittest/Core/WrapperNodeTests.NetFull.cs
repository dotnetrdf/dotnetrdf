/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

namespace VDS.RDF
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Xml;
    using System.Xml.Serialization;
    using Xunit;

    public partial class WrapperNodeTests
    {
        [Fact]
        public void Doesnt_implement_GetObjectData()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);
            var serializer = new BinaryFormatter(null, default(StreamingContext));

            using (var stream = new MemoryStream())
            {
                Assert.Throws<NotImplementedException>(() =>
                    serializer.Serialize(stream, wrapper));
            }
        }

        [Fact]
        public void Doesnt_implement_GetSchema()
        {
            var node = new NodeFactory().CreateBlankNode();
            IXmlSerializable wrapper = new MockWrapperNode(node);

            Assert.Throws<NotImplementedException>(() =>
                wrapper.GetSchema());
        }

        [Fact]
        public void Doesnt_implement_ReadXml()
        {
            var node = new NodeFactory().CreateBlankNode();
            IXmlSerializable wrapper = new MockWrapperNode(node);

            Assert.Throws<NotImplementedException>(() =>
                wrapper.ReadXml(XmlReader.Create(Stream.Null)));
        }

        [Fact]
        public void Doesnt_implement_WriteXml()
        {
            var node = new NodeFactory().CreateBlankNode();
            IXmlSerializable wrapper = new MockWrapperNode(node);

            Assert.Throws<NotImplementedException>(() =>
                wrapper.WriteXml(XmlWriter.Create(Stream.Null)));
        }
    }
}