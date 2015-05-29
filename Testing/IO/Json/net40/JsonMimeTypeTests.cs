/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF
{
    [TestFixture]
    public class JsonMimeTypeTests
    {
        public void Setup()
        {
            IOManager.ScanDefinitions();
        }

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
