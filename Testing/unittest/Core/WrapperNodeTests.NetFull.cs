namespace VDS.RDF
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Xml;
    using Xunit;

    public partial class WrapperNodeTests
    {
        [Fact]
        public void Doesnt_implement_GetObjectData()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);
            var serializer = new BinaryFormatter(null, new StreamingContext());

            using (var stream = new MemoryStream())
            {
                Assert.Throws<NotImplementedException>(() =>
                    serializer.Serialize(stream, wrapper)
                );
            }
        }

        [Fact]
        public void Doesnt_implement_GetSchema()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            Assert.Throws<NotImplementedException>(() =>
                wrapper.GetSchema()
            );
        }

        [Fact]
        public void Doesnt_implement_ReadXml()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            Assert.Throws<NotImplementedException>(() =>
                wrapper.ReadXml(XmlReader.Create(Stream.Null))
            );
        }

        [Fact]
        public void Doesnt_implement_WriteXml()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            Assert.Throws<NotImplementedException>(() =>
                wrapper.WriteXml(XmlWriter.Create(Stream.Null))
            );
        }
    }
}