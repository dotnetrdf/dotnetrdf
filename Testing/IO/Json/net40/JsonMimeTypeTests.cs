using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace net40
{
    [TestFixture]
    public class JsonMimeTypeTests
    {
        [Test]
        public void MimeTypesGetDefinitionsByTypeRdfJson1()
        {
            IEnumerable<MimeTypeDefinition> defs = IOManager.GetDefinitions("application/json");
#if PORTABLE
            Assert.AreEqual(1, defs.Count());
#else
            Assert.AreEqual(2, defs.Count());
#endif

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfJsonWriter), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedRdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfJsonWriter), d.RdfWriterType);
#endif
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeRdfJson2()
        {
            IEnumerable<MimeTypeDefinition> defs = IOManager.GetDefinitions("text/json");
#if PORTABLE
            Assert.AreEqual(1, defs.Count());
#else
            Assert.AreEqual(2, defs.Count());
#endif

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfJsonWriter), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedRdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfJsonWriter), d.RdfWriterType);
#endif
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfJson1()
        {
            IEnumerable<MimeTypeDefinition> defs = IOManager.GetDefinitionsByFileExtension(".rj");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfJsonWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfJson2()
        {
            IEnumerable<MimeTypeDefinition> defs = IOManager.GetDefinitionsByFileExtension("rj");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfJsonWriter), d.RdfWriterType);
        }

#if !NO_COMPRESSION
        [Test]
        public void MimeTypesGetDefinitionsByExtRdfJson3()
        {
            IEnumerable<MimeTypeDefinition> defs = IOManager.GetDefinitionsByFileExtension(".rj.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfJsonWriter), d.RdfWriterType);
        }
#endif

#if !NO_COMPRESSION
        [Test]
        public void MimeTypesGetDefinitionsByExtRdfJson4()
        {
            IEnumerable<MimeTypeDefinition> defs = IOManager.GetDefinitionsByFileExtension("rj.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfJsonWriter), d.RdfWriterType);
        }
#endif

        [Test]
        public void MimeTypesGetParserByTypeRdfJson1()
        {
            IRdfReader parser = IOManager.GetParser("text/json");
            Assert.IsInstanceOf<RdfJsonParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByTypeRdfJson2()
        {
            IRdfReader parser = IOManager.GetParser("application/json");
            Assert.IsInstanceOf<RdfJsonParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfJson1()
        {
            IRdfReader parser = IOManager.GetParserByFileExtension(".rj");
            Assert.IsInstanceOf<RdfJsonParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfJson2()
        {
            IRdfReader parser = IOManager.GetParserByFileExtension("rj");
            Assert.IsInstanceOf<RdfJsonParser>(parser);
        }

#if !NO_COMPRESSION
        [Test]
        public void MimeTypesGetParserByExtRdfJson3()
        {
            IRdfReader parser = IOManager.GetParserByFileExtension(".rj.gz");
            Assert.IsInstanceOf<GZippedRdfJsonParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfJson4()
        {
            IRdfReader parser = IOManager.GetParserByFileExtension("rj.gz");
            Assert.IsInstanceOf<GZippedRdfJsonParser>(parser);
        }
#endif

        [Test]
        public void MimeTypesGetWriterByTypeRdfJson1()
        {
            IRdfWriter writer = IOManager.GetWriter("text/json");
            Assert.IsInstanceOf<RdfJsonWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeRdfJson2()
        {
            IRdfWriter writer = IOManager.GetWriter("application/json");
            Assert.IsInstanceOf<RdfJsonWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeRdfJson3()
        {
            IRdfWriter writer = IOManager.GetWriter("application/rdf+json");
            Assert.IsInstanceOf<RdfJsonWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfJson1()
        {
            IRdfWriter parser = IOManager.GetWriterByFileExtension(".rj");
            Assert.IsInstanceOf<RdfJsonWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfJson2()
        {
            IRdfWriter parser = IOManager.GetWriterByFileExtension("rj");
            Assert.IsInstanceOf<RdfJsonWriter>(parser);
        }

#if !NO_COMPRESSION
        [Test]
        public void MimeTypesGetWriterByExtRdfJson3()
        {
            IRdfWriter parser = IOManager.GetWriterByFileExtension(".rj.gz");
            Assert.IsInstanceOf<GZippedRdfJsonWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfJson4()
        {
            IRdfWriter parser = IOManager.GetWriterByFileExtension("rj.gz");
            Assert.IsInstanceOf<GZippedRdfJsonWriter>(parser);
        }
#endif
    }
}
