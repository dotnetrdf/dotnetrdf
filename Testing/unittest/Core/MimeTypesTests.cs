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
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Writing;

namespace VDS.RDF.Core
{
    [TestClass]
    public class MimeTypesTests
    {
        [TestInitialize]
        public void Setup()
        {
            MimeTypesHelper.ResetDefinitions();
        }

        [TestCleanup]
        public void Teardown()
        {
            MimeTypesHelper.ResetDefinitions();
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsAll()
        {
            int count = MimeTypesHelper.Definitions.Count();
            Console.WriteLine(count + " Definitions registered");
            Assert.AreEqual(30, count);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeAny()
        {
            int count = MimeTypesHelper.GetDefinitions(MimeTypesHelper.Any).Count();
            Console.WriteLine(count + " Definitions registered");
            Assert.AreEqual(30, count);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeNotation3_1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/n3");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(Notation3Parser), d.RdfParserType);
            Assert.AreEqual(typeof(Notation3Writer), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedNotation3Parser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNotation3Writer), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeNotation3_2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/rdf+n3");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(Notation3Parser), d.RdfParserType);
            Assert.AreEqual(typeof(Notation3Writer), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedNotation3Parser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNotation3Writer), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtNotation3_1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".n3");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(Notation3Parser), d.RdfParserType);
            Assert.AreEqual(typeof(Notation3Writer), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtNotation3_2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("n3");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(Notation3Parser), d.RdfParserType);
            Assert.AreEqual(typeof(Notation3Writer), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtNotation3_3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".n3.gz");
            Assert.AreEqual(1, defs.Count());

#if !NO_COMPRESSION
            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedNotation3Parser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNotation3Writer), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtNotation3_4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("n3.gz");
            Assert.AreEqual(1, defs.Count());

#if !NO_COMPRESSION
            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedNotation3Parser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNotation3Writer), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeTurtle1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/turtle");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeTurtle2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/x-turtle");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeTurtle3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/turtle");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtTurtle1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".ttl");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtTurtle2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("ttl");
            Assert.AreEqual(1, defs.Count());

#if !NO_COMPRESSION
            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtTurtle3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".ttl.gz");
            Assert.AreEqual(1, defs.Count());

#if !NO_COMPRESSION
            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtTurtle4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("ttl.gz");
            Assert.AreEqual(1, defs.Count());

#if !NO_COMPRESSION
            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeNTriples1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/rdf-triples");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(NTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(NTriplesWriter), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedNTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNTriplesWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeNTriples2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/plain");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(NTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(NTriplesWriter), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedNTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNTriplesWriter), d.RdfWriterType);
#endif
        }
       
        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeNTriples3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/ntriples");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(NTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(NTriplesWriter), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedNTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNTriplesWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeNTriples4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/ntriples+turtle");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(NTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(NTriplesWriter), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedNTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNTriplesWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeNTriples5()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/x-ntriples");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(NTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(NTriplesWriter), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedNTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNTriplesWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtNTriples1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".nt");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(NTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(NTriplesWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtNTriples2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("nt");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(NTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(NTriplesWriter), d.RdfWriterType);
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetDefinitionsByExtNTriples3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".nt.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedNTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNTriplesWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtNTriples4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("nt.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedNTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNTriplesWriter), d.RdfWriterType);
        }
#endif

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeRdfXml1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/rdf+xml");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfXmlWriter), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedRdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfXmlWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeRdfXml2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/xml");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfXmlWriter), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedRdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfXmlWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeRdfXml3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/xml");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfXmlWriter), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedRdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfXmlWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfXml1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".rdf");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfXmlWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfXml2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("rdf");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfXmlWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfXml3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".rdf.gz");
            Assert.AreEqual(1, defs.Count());

#if !NO_COMPRESSION
            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfXmlWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfXml4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("rdf.gz");
            Assert.AreEqual(1, defs.Count());

#if !NO_COMPRESSION
            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfXmlWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeRdfJson1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/json");
            Assert.AreEqual(2, defs.Count());

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

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeRdfJson2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/json");
            Assert.AreEqual(2, defs.Count());

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

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfJson1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".rj");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfJsonWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfJson2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("rj");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfJsonWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfJson3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".rj.gz");
            Assert.AreEqual(1, defs.Count());

#if !NO_COMPRESSION
            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfJsonWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfJson4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("rj.gz");
            Assert.AreEqual(1, defs.Count());

#if !NO_COMPRESSION
            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfJsonWriter), d.RdfWriterType);
#endif
        }

#if !NO_HTMLAGILITYPACK
        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeRdfA1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/xhtml+xml");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(HtmlWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedRdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfAWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeRdfA2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/html");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(HtmlWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedRdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfAWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfA1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".html");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(HtmlWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfA2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("html");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(HtmlWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfA3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".html.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfAWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfA4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("html.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfAWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfA5()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".htm");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(HtmlWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfA6()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("htm");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(HtmlWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfA7()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".htm.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfAWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfA8()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("htm.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfAWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfA9()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".xhtml");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(HtmlWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfA10()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("xhtml");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(HtmlWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfA11()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".xhtml.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfAWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfA12()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("xhtml.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfAWriter), d.RdfWriterType);
        }
#endif

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeSparqlXml1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/sparql-results+xml");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlXmlParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlXmlWriter), d.SparqlResultsWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedSparqlXmlParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlXmlWriter), d.SparqlResultsWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlXml1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".srx");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlXmlParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlXmlWriter), d.SparqlResultsWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlXml2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("srx");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlXmlParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlXmlWriter), d.SparqlResultsWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlXml3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".srx.gz");
            Assert.AreEqual(1, defs.Count());

#if !NO_COMPRESSION
            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlXmlParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlXmlWriter), d.SparqlResultsWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlXml4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("srx.gz");
            Assert.AreEqual(1, defs.Count());

#if !NO_COMPRESSION
            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlXmlParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlXmlWriter), d.SparqlResultsWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeSparqlJson1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/sparql-results+json");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlJsonParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlJsonWriter), d.SparqlResultsWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedSparqlJsonParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlJsonWriter), d.SparqlResultsWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlJson1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".srj");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlJsonParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlJsonWriter), d.SparqlResultsWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlJson2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("srj");
            Assert.AreEqual(1, defs.Count());

#if !NO_COMPRESSION
            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlJsonParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlJsonWriter), d.SparqlResultsWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlJson3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".srj.gz");
            Assert.AreEqual(1, defs.Count());

#if !NO_COMPRESSION
            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlJsonParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlJsonWriter), d.SparqlResultsWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlJson4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("srj.gz");
            Assert.AreEqual(1, defs.Count());

#if !NO_COMPRESSION
            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlJsonParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlJsonWriter), d.SparqlResultsWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeSparqlCsv1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/csv");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlCsvWriter), d.SparqlResultsWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedSparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlCsvWriter), d.SparqlResultsWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeSparqlCsv2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/comma-separated-values");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlCsvWriter), d.SparqlResultsWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedSparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlCsvWriter), d.SparqlResultsWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlCsv1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".csv");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlCsvWriter), d.SparqlResultsWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlCsv2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("csv");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlCsvWriter), d.SparqlResultsWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlCsv3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".csv.gz");
            Assert.AreEqual(1, defs.Count());

#if !NO_COMPRESSION
            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlCsvWriter), d.SparqlResultsWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlCsv4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("csv.gz");
            Assert.AreEqual(1, defs.Count());

#if !NO_COMPRESSION
            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlCsvWriter), d.SparqlResultsWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeSparqlTsv1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/tab-separated-values");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlTsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlTsvWriter), d.SparqlResultsWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedSparqlTsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlTsvWriter), d.SparqlResultsWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlTsv1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".tsv");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlTsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlTsvWriter), d.SparqlResultsWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlTsv2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("tsv");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlTsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlTsvWriter), d.SparqlResultsWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlTsv3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".tsv.gz");
            Assert.AreEqual(1, defs.Count());

#if !NO_COMPRESSION
            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlTsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlTsvWriter), d.SparqlResultsWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlTsv4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("tsv.gz");
            Assert.AreEqual(1, defs.Count());

#if !NO_COMPRESSION
            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlTsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlTsvWriter), d.SparqlResultsWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeCaseSensitivity1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("TEXT/TURTLE");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeCaseSensitivity2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("TEXT/turtle");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeCaseSensitivity3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("TeXt/TuRtLe");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtCaseSensitivity1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".TTL");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtCaseSensitivity2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".tTl");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeExtraParams1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/turtle; charset=utf-8");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByTypeExtraParams2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/turtle; q=1.0");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);

#if !NO_COMPRESSION
            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
#endif
        }

        [TestMethod]
        public void MimeTypesGetParserByTypeNTriples1()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("text/plain");
            Assert.IsInstanceOfType(parser, typeof(NTriplesParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByTypeNTriples2()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("text/ntriples");
            Assert.IsInstanceOfType(parser, typeof(NTriplesParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByTypeNTriples3()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("text/ntriples+turtle");
            Assert.IsInstanceOfType(parser, typeof(NTriplesParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByTypeNTriples4()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("application/rdf-triples");
            Assert.IsInstanceOfType(parser, typeof(NTriplesParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByTypeNTriples5()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("application/x-ntriples");
            Assert.IsInstanceOfType(parser, typeof(NTriplesParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByTypeTurtle1()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("text/turtle");
            Assert.IsInstanceOfType(parser, typeof(TurtleParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByTypeTurtle2()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("application/x-turtle");
            Assert.IsInstanceOfType(parser, typeof(TurtleParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByTypeTurtle3()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("application/turtle");
            Assert.IsInstanceOfType(parser, typeof(TurtleParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByTypeNotation3_1()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("text/n3");
            Assert.IsInstanceOfType(parser, typeof(Notation3Parser));
        }

        [TestMethod]
        public void MimeTypesGetParserByTypeNotation3_2()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("text/rdf+n3");
            Assert.IsInstanceOfType(parser, typeof(Notation3Parser));
        }

        [TestMethod]
        public void MimeTypesGetParserByTypeRdfXml1()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("text/xml");
            Assert.IsInstanceOfType(parser, typeof(RdfXmlParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByTypeRdfXml2()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("application/rdf+xml");
            Assert.IsInstanceOfType(parser, typeof(RdfXmlParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByTypeRdfXml3()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("application/xml");
            Assert.IsInstanceOfType(parser, typeof(RdfXmlParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByTypeRdfJson1()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("text/json");
            Assert.IsInstanceOfType(parser, typeof(RdfJsonParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByTypeRdfJson2()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("application/json");
            Assert.IsInstanceOfType(parser, typeof(RdfJsonParser));
        }

#if !NO_HTMLAGILITYPACK
        [TestMethod]
        public void MimeTypesGetParserByTypeRdfA1()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("text/html");
            Assert.IsInstanceOfType(parser, typeof(RdfAParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByTypeRdfA2()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("application/xhtml+xml");
            Assert.IsInstanceOfType(parser, typeof(RdfAParser));
        }
#endif

        [TestMethod, ExpectedException(typeof(RdfParserSelectionException))]
        public void MimeTypesGetParserByTypeUnknown()
        {
            IRdfReader reader = MimeTypesHelper.GetParser("application/unknown");
        }

        [TestMethod]
        public void MimeTypesGetParserByExtNTriples1()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".nt");
            Assert.IsInstanceOfType(parser, typeof(NTriplesParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtNTriples2()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("nt");
            Assert.IsInstanceOfType(parser, typeof(NTriplesParser));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetParserByExtNTriples3()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".nt.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedNTriplesParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtNTriples4()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("nt.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedNTriplesParser));
        }
#endif

        [TestMethod]
        public void MimeTypesGetParserByExtTurtle1()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".ttl");
            Assert.IsInstanceOfType(parser, typeof(TurtleParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtTurtle2()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("ttl");
            Assert.IsInstanceOfType(parser, typeof(TurtleParser));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetParserByExtTurtle3()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".ttl.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedTurtleParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtTurtle4()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("ttl.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedTurtleParser));
        }
#endif

        [TestMethod]
        public void MimeTypesGetParserByExtNotation3_1()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".n3");
            Assert.IsInstanceOfType(parser, typeof(Notation3Parser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtNotation3_2()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("n3");
            Assert.IsInstanceOfType(parser, typeof(Notation3Parser));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetParserByExtNotation3_3()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".n3.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedNotation3Parser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtNotation3_4()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("n3.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedNotation3Parser));
        }
#endif

        [TestMethod]
        public void MimeTypesGetParserByExtRdfXml1()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".rdf");
            Assert.IsInstanceOfType(parser, typeof(RdfXmlParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtRdfXml2()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("rdf");
            Assert.IsInstanceOfType(parser, typeof(RdfXmlParser));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetParserByExtRdfXml3()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".rdf.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfXmlParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtRdfXml4()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("rdf.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfXmlParser));
        }
#endif

        [TestMethod]
        public void MimeTypesGetParserByExtRdfJson1()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".rj");
            Assert.IsInstanceOfType(parser, typeof(RdfJsonParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtRdfJson2()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("rj");
            Assert.IsInstanceOfType(parser, typeof(RdfJsonParser));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetParserByExtRdfJson3()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".rj.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfJsonParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtRdfJson4()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("rj.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfJsonParser));
        }
#endif

#if !NO_HTMLAGILITYPACK
        [TestMethod]
        public void MimeTypesGetParserByExtRdfA1()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".html");
            Assert.IsInstanceOfType(parser, typeof(RdfAParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtRdfA2()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("html");
            Assert.IsInstanceOfType(parser, typeof(RdfAParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtRdfA3()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".html.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfAParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtRdfA4()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("html.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfAParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtRdfA5()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".htm");
            Assert.IsInstanceOfType(parser, typeof(RdfAParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtRdfA6()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("htm");
            Assert.IsInstanceOfType(parser, typeof(RdfAParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtRdfA7()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".htm.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfAParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtRdfA8()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("htm.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfAParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtRdfA9()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".xhtml");
            Assert.IsInstanceOfType(parser, typeof(RdfAParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtRdfA10()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("xhtml");
            Assert.IsInstanceOfType(parser, typeof(RdfAParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtRdfA11()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".xhtml.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfAParser));
        }

        [TestMethod]
        public void MimeTypesGetParserByExtRdfA12()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("xhtml.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfAParser));
        }
#endif

        [TestMethod]
        public void MimeTypesGetWriterByTypeUnknown()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/unknown");
            Assert.IsInstanceOfType(writer, typeof(CompressingTurtleWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByTypeAny()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter(MimeTypesHelper.Any);
            Assert.IsInstanceOfType(writer, typeof(NTriplesWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByTypeNTriples1()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("text/plain");
            Assert.IsInstanceOfType(writer, typeof(NTriplesWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByTypeNTriples2()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("text/ntriples");
            Assert.IsInstanceOfType(writer, typeof(NTriplesWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByTypeNTriples3()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("text/ntriples+turtle");
            Assert.IsInstanceOfType(writer, typeof(NTriplesWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByTypeNTriples4()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/rdf-triples");
            Assert.IsInstanceOfType(writer, typeof(NTriplesWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByTypeNTriples5()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/x-ntriples");
            Assert.IsInstanceOfType(writer, typeof(NTriplesWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByTypeTurtle1()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("text/turtle");
            Assert.IsInstanceOfType(writer, typeof(CompressingTurtleWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByTypeTurtle2()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/x-turtle");
            Assert.IsInstanceOfType(writer, typeof(CompressingTurtleWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByTypeTurtle3()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/turtle");
            Assert.IsInstanceOfType(writer, typeof(CompressingTurtleWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByTypeNotation3_1()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("text/n3");
            Assert.IsInstanceOfType(writer, typeof(Notation3Writer));
        }

        [TestMethod]
        public void MimeTypesGetWriterByTypeNotation3_2()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("text/rdf+n3");
            Assert.IsInstanceOfType(writer, typeof(Notation3Writer));
        }

        [TestMethod]
        public void MimeTypesGetWriterByTypeRdfXml1()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("text/xml");
            Assert.IsInstanceOfType(writer, typeof(RdfXmlWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByTypeRdfXml2()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/xml");
            Assert.IsInstanceOfType(writer, typeof(RdfXmlWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByTypeRdfXml3()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/rdf+xml");
            Assert.IsInstanceOfType(writer, typeof(RdfXmlWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByTypeRdfJson1()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("text/json");
            Assert.IsInstanceOfType(writer, typeof(RdfJsonWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByTypeRdfJson2()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/json");
            Assert.IsInstanceOfType(writer, typeof(RdfJsonWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByTypeRdfJson3()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/rdf+json");
            Assert.IsInstanceOfType(writer, typeof(RdfJsonWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByTypeRdfA1()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("text/html");
            Assert.IsInstanceOfType(writer, typeof(HtmlWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByTypeRdfA2()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/xhtml+xml");
            Assert.IsInstanceOfType(writer, typeof(HtmlWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByExtNTriples1()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".nt");
            Assert.IsInstanceOfType(parser, typeof(NTriplesWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByExtNTriples2()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("nt");
            Assert.IsInstanceOfType(parser, typeof(NTriplesWriter));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetWriterByExtNTriples3()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".nt.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedNTriplesWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByExtNTriples4()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("nt.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedNTriplesWriter));
        }
#endif

        [TestMethod]
        public void MimeTypesGetWriterByExtTurtle1()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".ttl");
            Assert.IsInstanceOfType(parser, typeof(CompressingTurtleWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByExtTurtle2()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("ttl");
            Assert.IsInstanceOfType(parser, typeof(CompressingTurtleWriter));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetWriterByExtTurtle3()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".ttl.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedTurtleWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByExtTurtle4()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("ttl.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedTurtleWriter));
        }
#endif

        [TestMethod]
        public void MimeTypesGetWriterByExtNotation3_1()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".n3");
            Assert.IsInstanceOfType(parser, typeof(Notation3Writer));
        }

        [TestMethod]
        public void MimeTypesGetWriterByExtNotation3_2()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("n3");
            Assert.IsInstanceOfType(parser, typeof(Notation3Writer));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetWriterByExtNotation3_3()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".n3.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedNotation3Writer));
        }

        [TestMethod]
        public void MimeTypesGetWriterByExtNotation3_4()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("n3.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedNotation3Writer));
        }
#endif

        [TestMethod]
        public void MimeTypesGetWriterByExtRdfXml1()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".rdf");
            Assert.IsInstanceOfType(parser, typeof(RdfXmlWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByExtRdfXml2()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("rdf");
            Assert.IsInstanceOfType(parser, typeof(RdfXmlWriter));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetWriterByExtRdfXml3()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".rdf.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfXmlWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByExtRdfXml4()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("rdf.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfXmlWriter));
        }
#endif

        [TestMethod]
        public void MimeTypesGetWriterByExtRdfJson1()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".rj");
            Assert.IsInstanceOfType(parser, typeof(RdfJsonWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByExtRdfJson2()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("rj");
            Assert.IsInstanceOfType(parser, typeof(RdfJsonWriter));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetWriterByExtRdfJson3()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".rj.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfJsonWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByExtRdfJson4()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("rj.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfJsonWriter));
        }
#endif

        [TestMethod]
        public void MimeTypesGetWriterByExtRdfA1()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".html");
            Assert.IsInstanceOfType(parser, typeof(HtmlWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByExtRdfA2()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("html");
            Assert.IsInstanceOfType(parser, typeof(HtmlWriter));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetWriterByExtRdfA3()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".html.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfAWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByExtRdfA4()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("html.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfAWriter));
        }
#endif

        [TestMethod]
        public void MimeTypesGetWriterByExtRdfA5()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".htm");
            Assert.IsInstanceOfType(parser, typeof(HtmlWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByExtRdfA6()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("htm");
            Assert.IsInstanceOfType(parser, typeof(HtmlWriter));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetWriterByExtRdfA7()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".htm.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfAWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByExtRdfA8()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("htm.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfAWriter));
        }
#endif

        [TestMethod]
        public void MimeTypesGetWriterByExtRdfA9()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".xhtml");
            Assert.IsInstanceOfType(parser, typeof(HtmlWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByExtRdfA10()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("xhtml");
            Assert.IsInstanceOfType(parser, typeof(HtmlWriter));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetWriterByExtRdfA11()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".xhtml.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfAWriter));
        }

        [TestMethod]
        public void MimeTypesGetWriterByExtRdfA12()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("xhtml.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedRdfAWriter));
        }
#endif

        [TestMethod, ExpectedException(typeof(RdfParserSelectionException))]
        public void MimeTypesGetSparqlParserByTypeUnknown()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("application/unknown");
        }

        [TestMethod]
        public void MimeTypesGetSparqlParserByTypeSparqlXml1()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("application/sparql-results+xml");
            Assert.IsInstanceOfType(parser, typeof(SparqlXmlParser));
        }

        [TestMethod]
        public void MimeTypesGetSparqlParserByTypeSparqlXml2()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("application/xml");
            Assert.IsInstanceOfType(parser, typeof(SparqlXmlParser));
        }

        [TestMethod]
        public void MimeTypesGetSparqlParserByTypeSparqlJson1()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("application/sparql-results+json");
            Assert.IsInstanceOfType(parser, typeof(SparqlJsonParser));
        }

        [TestMethod]
        public void MimeTypesGetSparqlParserByTypeSparqlJson2()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("application/json");
            Assert.IsInstanceOfType(parser, typeof(SparqlJsonParser));
        }

        [TestMethod]
        public void MimeTypesGetSparqlParserByTypeSparqlCsv1()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("text/csv");
            Assert.IsInstanceOfType(parser, typeof(SparqlCsvParser));
        }

        [TestMethod]
        public void MimeTypesGetSparqlParserByTypeSparqlCsv2()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("text/comma-separated-values");
            Assert.IsInstanceOfType(parser, typeof(SparqlCsvParser));
        }

        [TestMethod]
        public void MimeTypesGetSparqlParserByTypeSparqlTsv1()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("text/tab-separated-values");
            Assert.IsInstanceOfType(parser, typeof(SparqlTsvParser));
        }

        [TestMethod]
        public void MimeTypesGetSparqlParserByExtSparqlXml1()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".srx");
            Assert.IsInstanceOfType(parser, typeof(SparqlXmlParser));
        }

        [TestMethod]
        public void MimeTypesGetSparqlParserByExtSparqlXml2()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("srx");
            Assert.IsInstanceOfType(parser, typeof(SparqlXmlParser));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetSparqlParserByExtSparqlXml3()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".srx.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedSparqlXmlParser));
        }

        [TestMethod]
        public void MimeTypesGetSparqlParserByExtSparqlXml4()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("srx.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedSparqlXmlParser));
        }
#endif

        [TestMethod]
        public void MimeTypesGetSparqlParserByExtSparqlJson1()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".srj");
            Assert.IsInstanceOfType(parser, typeof(SparqlJsonParser));
        }

        [TestMethod]
        public void MimeTypesGetSparqlParserByExtSparqlJson2()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("srj");
            Assert.IsInstanceOfType(parser, typeof(SparqlJsonParser));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetSparqlParserByExtSparqlJson3()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".srj.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedSparqlJsonParser));
        }

        [TestMethod]
        public void MimeTypesGetSparqlParserByExtSparqlJson4()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("srj.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedSparqlJsonParser));
        }
#endif

        [TestMethod]
        public void MimeTypesGetSparqlParserByExtSparqlTsv1()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".tsv");
            Assert.IsInstanceOfType(parser, typeof(SparqlTsvParser));
        }

        [TestMethod]
        public void MimeTypesGetSparqlParserByExtSparqlTsv2()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("tsv");
            Assert.IsInstanceOfType(parser, typeof(SparqlTsvParser));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetSparqlParserByExtSparqlTsv3()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".tsv.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedSparqlTsvParser));
        }

        [TestMethod]
        public void MimeTypesGetSparqlParserByExtSparqlTsv4()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("tsv.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedSparqlTsvParser));
        }
#endif

        [TestMethod]
        public void MimeTypesGetSparqlParserByExtSparqlCsv1()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".csv");
            Assert.IsInstanceOfType(parser, typeof(SparqlCsvParser));
        }

        [TestMethod]
        public void MimeTypesGetSparqlParserByExtSparqlCsv2()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("csv");
            Assert.IsInstanceOfType(parser, typeof(SparqlCsvParser));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetSparqlParserByExtSparqlCsv3()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".csv.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedSparqlCsvParser));
        }

        [TestMethod]
        public void MimeTypesGetSparqlParserByExtSparqlCsv4()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("csv.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedSparqlCsvParser));
        }
#endif

        [TestMethod]
        public void MimeTypesGetSparqlWriterByTypeUnknown()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("application/unknown");
            Assert.IsInstanceOfType(writer, typeof(SparqlXmlWriter));
        }

        [TestMethod]
        public void MimeTypesGetSparqlWriterByTypeAny()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter(MimeTypesHelper.Any);
            Assert.IsInstanceOfType(writer, typeof(SparqlXmlWriter));
        }

        [TestMethod]
        public void MimeTypesGetSparqlWriterByTypeSparqlXml1()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("application/sparql-results+xml");
            Assert.IsInstanceOfType(writer, typeof(SparqlXmlWriter));
        }

        [TestMethod]
        public void MimeTypesGetSparqlWriterByTypeSparqlXml2()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("application/xml");
            Assert.IsInstanceOfType(writer, typeof(SparqlXmlWriter));
        }

        [TestMethod]
        public void MimeTypesGetSparqlWriterByTypeSparqlJson1()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("application/sparql-results+json");
            Assert.IsInstanceOfType(writer, typeof(SparqlJsonWriter));
        }

        [TestMethod]
        public void MimeTypesGetSparqlWriterByTypeSparqlJson2()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("application/json");
            Assert.IsInstanceOfType(writer, typeof(SparqlJsonWriter));
        }

        [TestMethod]
        public void MimeTypesGetSparqlWriterByTypeSparqlCsv1()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("text/csv");
            Assert.IsInstanceOfType(writer, typeof(SparqlCsvWriter));
        }

        [TestMethod]
        public void MimeTypesGetSparqlWriterByTypeSparqlCsv2()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("text/comma-separated-values");
            Assert.IsInstanceOfType(writer, typeof(SparqlCsvWriter));
        }

        [TestMethod]
        public void MimeTypesGetSparqlWriterByTypeSparqlTsv1()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("text/tab-separated-values");
            Assert.IsInstanceOfType(writer, typeof(SparqlTsvWriter));
        }

        [TestMethod]
        public void MimeTypesGetSparqlWriterByExtSparqlXml1()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".srx");
            Assert.IsInstanceOfType(writer, typeof(SparqlXmlWriter));
        }

        [TestMethod]
        public void MimeTypesGetSparqlWriterByExtSparqlXml2()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("srx");
            Assert.IsInstanceOfType(writer, typeof(SparqlXmlWriter));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetSparqlWriterByExtSparqlXml3()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".srx.gz");
            Assert.IsInstanceOfType(writer, typeof(GZippedSparqlXmlWriter));
        }

        [TestMethod]
        public void MimeTypesGetSparqlWriterByExtSparqlXml4()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("srx.gz");
            Assert.IsInstanceOfType(writer, typeof(GZippedSparqlXmlWriter));
        }
#endif

        [TestMethod]
        public void MimeTypesGetSparqlWriterByExtSparqlJson1()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".srj");
            Assert.IsInstanceOfType(writer, typeof(SparqlJsonWriter));
        }

        [TestMethod]
        public void MimeTypesGetSparqlWriterByExtSparqlJson2()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("srj");
            Assert.IsInstanceOfType(writer, typeof(SparqlJsonWriter));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetSparqlWriterByExtSparqlJson3()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".srj.gz");
            Assert.IsInstanceOfType(writer, typeof(GZippedSparqlJsonWriter));
        }

        [TestMethod]
        public void MimeTypesGetSparqlWriterByExtSparqlJson4()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("srj.gz");
            Assert.IsInstanceOfType(writer, typeof(GZippedSparqlJsonWriter));
        }
#endif

        [TestMethod]
        public void MimeTypesGetSparqlWriterByExtSparqlTsv1()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".tsv");
            Assert.IsInstanceOfType(writer, typeof(SparqlTsvWriter));
        }

        [TestMethod]
        public void MimeTypesGetSparqlWriterByExtSparqlTsv2()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("tsv");
            Assert.IsInstanceOfType(writer, typeof(SparqlTsvWriter));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetSparqlWriterByExtSparqlTsv3()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".tsv.gz");
            Assert.IsInstanceOfType(writer, typeof(GZippedSparqlTsvWriter));
        }

        [TestMethod]
        public void MimeTypesGetSparqlWriterByExtSparqlTsv4()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("tsv.gz");
            Assert.IsInstanceOfType(writer, typeof(GZippedSparqlTsvWriter));
        }
#endif

        [TestMethod]
        public void MimeTypesGetSparqlWriterByExtSparqlCsv1()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".csv");
            Assert.IsInstanceOfType(writer, typeof(SparqlCsvWriter));
        }

        [TestMethod]
        public void MimeTypesGetSparqlWriterByExtSparqlCsv2()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("csv");
            Assert.IsInstanceOfType(writer, typeof(SparqlCsvWriter));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetSparqlWriterByExtSparqlCsv3()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".csv.gz");
            Assert.IsInstanceOfType(writer, typeof(GZippedSparqlCsvWriter));
        }

        [TestMethod]
        public void MimeTypesGetSparqlWriterByExtSparqlCsv4()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("csv.gz");
            Assert.IsInstanceOfType(writer, typeof(GZippedSparqlCsvWriter));
        }
#endif

        [TestMethod, ExpectedException(typeof(RdfParserSelectionException))]
        public void MimeTypesGetStoreParserByTypeUnknown()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParser("application/unknown");
        }

        [TestMethod]
        public void MimeTypesGetStoreParserByTypeNQuads1()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParser("text/x-nquads");
            Assert.IsInstanceOfType(parser, typeof(NQuadsParser));
        }

        [TestMethod]
        public void MimeTypesGetStoreParserByTypeTriG1()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParser("application/x-trig");
            Assert.IsInstanceOfType(parser, typeof(TriGParser));
        }

        [TestMethod]
        public void MimeTypesGetStoreParserByTypeTriX1()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParser("application/trix");
            Assert.IsInstanceOfType(parser, typeof(TriXParser));
        }

        [TestMethod]
        public void MimeTypesGetStoreParserByExtNQuads1()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".nq");
            Assert.IsInstanceOfType(parser, typeof(NQuadsParser));
        }

        [TestMethod]
        public void MimeTypesGetStoreParserByExtNQuads2()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension("nq");
            Assert.IsInstanceOfType(parser, typeof(NQuadsParser));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetStoreParserByExtNQuads3()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".nq.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedNQuadsParser));
        }

        [TestMethod]
        public void MimeTypesGetStoreParserByExtNQuads4()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension("nq.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedNQuadsParser));
        }
#endif

        [TestMethod]
        public void MimeTypesGetStoreParserByExtTriG1()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".trig");
            Assert.IsInstanceOfType(parser, typeof(TriGParser));
        }

        [TestMethod]
        public void MimeTypesGetStoreParserByExtTriG2()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension("trig");
            Assert.IsInstanceOfType(parser, typeof(TriGParser));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetStoreParserByExtTriG3()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".trig.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedTriGParser));
        }

        [TestMethod]
        public void MimeTypesGetStoreParserByExtTriG4()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension("trig.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedTriGParser));
        }
#endif

        [TestMethod]
        public void MimeTypesGetStoreParserByExtTriX1()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".xml");
            Assert.IsInstanceOfType(parser, typeof(TriXParser));
        }

        [TestMethod]
        public void MimeTypesGetStoreParserByExtTriX2()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension("xml");
            Assert.IsInstanceOfType(parser, typeof(TriXParser));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetStoreParserByExtTriX3()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".xml.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedTriXParser));
        }

        [TestMethod]
        public void MimeTypesGetStoreParserByExtTriX4()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension("xml.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedTriXParser));
        }
#endif

        [TestMethod]
        public void MimeTypesGetStoreWriterByTypeUnknown()
        {
            IStoreWriter writer = MimeTypesHelper.GetStoreWriter("application/unknown");
            Assert.IsInstanceOfType(writer, typeof(NQuadsWriter));
        }

        [TestMethod]
        public void MimeTypesGetStoreWriterByTypeNQuads1()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriter("text/x-nquads");
            Assert.IsInstanceOfType(parser, typeof(NQuadsWriter));
        }

        [TestMethod]
        public void MimeTypesGetStoreWriterByTypeTriG1()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriter("application/x-trig");
            Assert.IsInstanceOfType(parser, typeof(TriGWriter));
        }

        [TestMethod]
        public void MimeTypesGetStoreWriterByTypeTriX1()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriter("application/trix");
            Assert.IsInstanceOfType(parser, typeof(TriXWriter));
        }

        [TestMethod]
        public void MimeTypesGetStoreWriterByExtNQuads1()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension(".nq");
            Assert.IsInstanceOfType(parser, typeof(NQuadsWriter));
        }

        [TestMethod]
        public void MimeTypesGetStoreWriterByExtNQuads2()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension("nq");
            Assert.IsInstanceOfType(parser, typeof(NQuadsWriter));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetStoreWriterByExtNQuads3()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension(".nq.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedNQuadsWriter));
        }

        [TestMethod]
        public void MimeTypesGetStoreWriterByExtNQuads4()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension("nq.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedNQuadsWriter));
        }
#endif

        [TestMethod]
        public void MimeTypesGetStoreWriterByExtTriG1()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension(".trig");
            Assert.IsInstanceOfType(parser, typeof(TriGWriter));
        }

        [TestMethod]
        public void MimeTypesGetStoreWriterByExtTriG2()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension("trig");
            Assert.IsInstanceOfType(parser, typeof(TriGWriter));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetStoreWriterByExtTriG3()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension(".trig.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedTriGWriter));
        }

        [TestMethod]
        public void MimeTypesGetStoreWriterByExtTriG4()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension("trig.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedTriGWriter));
        }
#endif

        [TestMethod]
        public void MimeTypesGetStoreWriterByExtTriX1()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension(".xml");
            Assert.IsInstanceOfType(parser, typeof(TriXWriter));
        }

        [TestMethod]
        public void MimeTypesGetStoreWriterByExtTriX2()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension("xml");
            Assert.IsInstanceOfType(parser, typeof(TriXWriter));
        }

#if !NO_COMPRESSION
        [TestMethod]
        public void MimeTypesGetStoreWriterByExtTriX3()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension(".xml.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedTriXWriter));
        }

        [TestMethod]
        public void MimeTypesGetStoreWriterByExtTriX4()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension("xml.gz");
            Assert.IsInstanceOfType(parser, typeof(GZippedTriXWriter));
        }
#endif

        [TestMethod]
        public void MimeTypesApplyWriterOptions1()
        {
            int compressionLevel = Options.DefaultCompressionLevel;
            try
            {
                Options.DefaultCompressionLevel = WriterCompressionLevel.High;
                IRdfWriter writer = MimeTypesHelper.GetWriter("application/turtle");
                Assert.IsInstanceOfType(writer, typeof(CompressingTurtleWriter));
                Assert.AreEqual(WriterCompressionLevel.High, ((ICompressingWriter)writer).CompressionLevel);
            }
            finally
            {
                Options.DefaultCompressionLevel = compressionLevel;
            }
        }

        [TestMethod]
        public void MimeTypesApplyWriterOptions2()
        {
            int compressionLevel = Options.DefaultCompressionLevel;
            try
            {
                Options.DefaultCompressionLevel = WriterCompressionLevel.High;
                IRdfWriter writer = MimeTypesHelper.GetWriterByFileExtension(".ttl");
                Assert.IsInstanceOfType(writer, typeof(CompressingTurtleWriter));
                Assert.AreEqual(WriterCompressionLevel.High, ((ICompressingWriter)writer).CompressionLevel);
            }
            finally
            {
                Options.DefaultCompressionLevel = compressionLevel;
            }
        }

        [TestMethod]
        public void MimeTypesApplyWriterOptions3()
        {
            int compressionLevel = Options.DefaultCompressionLevel;
            try
            {
                Options.DefaultCompressionLevel = WriterCompressionLevel.High;
                IStoreWriter writer = MimeTypesHelper.GetStoreWriter("application/x-trig");
                Assert.IsInstanceOfType(writer, typeof(TriGWriter));
                Assert.AreEqual(WriterCompressionLevel.High, ((ICompressingWriter)writer).CompressionLevel);
            }
            finally
            {
                Options.DefaultCompressionLevel = compressionLevel;
            }
        }

        [TestMethod]
        public void MimeTypesApplyWriterOptions4()
        {
            int compressionLevel = Options.DefaultCompressionLevel;
            try
            {
                Options.DefaultCompressionLevel = WriterCompressionLevel.High;
                IStoreWriter writer = MimeTypesHelper.GetStoreWriterByFileExtension(".trig");
                Assert.IsInstanceOfType(writer, typeof(TriGWriter));
                Assert.AreEqual(WriterCompressionLevel.High, ((ICompressingWriter)writer).CompressionLevel);
            }
            finally
            {
                Options.DefaultCompressionLevel = compressionLevel;
            }
        }

        [TestMethod]
        public void MimeTypesApplyParserOptions1()
        {
            TokenQueueMode queueMode = Options.DefaultTokenQueueMode;
            try
            {
                Options.DefaultTokenQueueMode = TokenQueueMode.AsynchronousBufferDuringParsing;
                IRdfReader parser = MimeTypesHelper.GetParser("application/turtle");
                Assert.IsInstanceOfType(parser, typeof(TurtleParser));
                Assert.AreEqual(TokenQueueMode.AsynchronousBufferDuringParsing, ((ITokenisingParser)parser).TokenQueueMode);
            }
            finally
            {
                Options.DefaultTokenQueueMode = queueMode;
            }
        }

        [TestMethod]
        public void MimeTypesApplyParserOptions2()
        {
            TokenQueueMode queueMode = Options.DefaultTokenQueueMode;
            try
            {
                Options.DefaultTokenQueueMode = TokenQueueMode.AsynchronousBufferDuringParsing;
                IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".ttl");
                Assert.IsInstanceOfType(parser, typeof(TurtleParser));
                Assert.AreEqual(TokenQueueMode.AsynchronousBufferDuringParsing, ((ITokenisingParser)parser).TokenQueueMode);
            }
            finally
            {
                Options.DefaultTokenQueueMode = queueMode;
            }
        }

        [TestMethod]
        public void MimeTypesApplyParserOptions3()
        {
            TokenQueueMode queueMode = Options.DefaultTokenQueueMode;
            try
            {
                Options.DefaultTokenQueueMode = TokenQueueMode.AsynchronousBufferDuringParsing;
                IStoreReader parser = MimeTypesHelper.GetStoreParser("text/x-nquads");
                Assert.IsInstanceOfType(parser, typeof(NQuadsParser));
                Assert.AreEqual(TokenQueueMode.AsynchronousBufferDuringParsing, ((ITokenisingParser)parser).TokenQueueMode);
            }
            finally
            {
                Options.DefaultTokenQueueMode = queueMode;
            }
        }

        [TestMethod]
        public void MimeTypesApplyParserOptions4()
        {
            TokenQueueMode queueMode = Options.DefaultTokenQueueMode;
            try
            {
                Options.DefaultTokenQueueMode = TokenQueueMode.AsynchronousBufferDuringParsing;
                IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".nq");
                Assert.IsInstanceOfType(parser, typeof(NQuadsParser));
                Assert.AreEqual(TokenQueueMode.AsynchronousBufferDuringParsing, ((ITokenisingParser)parser).TokenQueueMode);
            }
            finally
            {
                Options.DefaultTokenQueueMode = queueMode;
            }
        }

        [TestMethod]
        public void MimeTypesContentNegotiation1()
        {
            String[] types = new String[] { "application/turtle" , "application/rdf+xml", "text/plain" };
            MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(types).FirstOrDefault();
            Assert.IsNotNull(def);
            Assert.AreEqual(typeof(TurtleParser), def.RdfParserType);
        }

        [TestMethod]
        public void MimeTypesContentNegotiation2()
        {
            String[] types = new String[] { "application/rdf+xml", "application/turtle", "text/plain" };
            MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(types).FirstOrDefault();
            Assert.IsNotNull(def);
            Assert.AreEqual(typeof(RdfXmlParser), def.RdfParserType);
        }

        [TestMethod]
        public void MimeTypesContentNegotiation3()
        {
            String[] types = new String[] { "text/plain", "application/rdf+xml", "application/turtle" };
            MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(types).FirstOrDefault();
            Assert.IsNotNull(def);
            Assert.AreEqual(typeof(NTriplesParser), def.RdfParserType);
        }

        [TestMethod]
        public void MimeTypesContentNegotiation4()
        {
            MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(MimeTypesHelper.Any).FirstOrDefault();
            Assert.IsNotNull(def);
        }

        [TestMethod]
        public void MimeTypesContentNegotiation5()
        {
            String[] types = new String[] { "application/turtle; q=0.8", "application/rdf+xml", "text/plain; q=0.9" };
            MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(types).FirstOrDefault();
            Assert.IsNotNull(def);
            Assert.AreEqual(typeof(RdfXmlParser), def.RdfParserType);
        }

        private void PrintSelectors(IEnumerable<MimeTypeSelector> selectors)
        {
            foreach (MimeTypeSelector selector in selectors)
            {
                Console.WriteLine(selector.ToString());
            }
        }

        [TestMethod]
        public void MimeTypesSelectors1()
        {
            String[] types = new String[] { "audio/*; q=0.2", "audio/basic" };
            List<MimeTypeSelector> selectors = MimeTypeSelector.CreateSelectors(types).ToList();
            this.PrintSelectors(selectors);

            Assert.IsFalse(selectors[0].IsRange);
            Assert.AreEqual("audio/basic", selectors[0].Type);
            Assert.IsTrue(selectors[1].IsRange);
            Assert.AreEqual("audio/*", selectors[1].Type);
            Assert.AreEqual(0.2d, selectors[1].Quality);
        }

        [TestMethod]
        public void MimeTypesSelectors2()
        {
            String[] types = new String[] { "text/plain; q=0.5", "text/turtle" };
            List<MimeTypeSelector> selectors = MimeTypeSelector.CreateSelectors(types).ToList();
            this.PrintSelectors(selectors);

            Assert.AreEqual("text/turtle", selectors[0].Type);
            Assert.AreEqual("text/plain", selectors[1].Type);
            Assert.AreEqual(0.5d, selectors[1].Quality);
        }

        [TestMethod]
        public void MimeTypesSelectors3()
        {
            String[] types = new String[] { "text/plain", "text/turtle" };
            List<MimeTypeSelector> selectors = MimeTypeSelector.CreateSelectors(types).ToList();
            this.PrintSelectors(selectors);

            Assert.AreEqual("text/plain", selectors[0].Type);
            Assert.AreEqual("text/turtle", selectors[1].Type);
        }

        [TestMethod]
        public void MimeTypesSelectors4()
        {
            String[] types = new String[] { "text/*", "text/html", "*/*" };
            List<MimeTypeSelector> selectors = MimeTypeSelector.CreateSelectors(types).ToList();
            this.PrintSelectors(selectors);

            Assert.AreEqual("text/html", selectors[0].Type);
            Assert.AreEqual("text/*", selectors[1].Type);
            Assert.AreEqual(MimeTypesHelper.Any, selectors[2].Type);
        }

        [TestMethod]
        public void MimeTypesSelectors5()
        {
            String[] types = new String[] { "text/plain; q=0.5", "text/turtle; q=0.5" };
            List<MimeTypeSelector> selectors = MimeTypeSelector.CreateSelectors(types).ToList();
            this.PrintSelectors(selectors);

            Assert.AreEqual("text/plain", selectors[0].Type);
            Assert.AreEqual(0.5d, selectors[0].Quality);
            Assert.AreEqual("text/turtle", selectors[1].Type);
            Assert.AreEqual(0.5d, selectors[1].Quality);
        }

        [TestMethod]
        public void MimeTypesSelectors6()
        {
            String[] types = new String[] { "text/turtle; q=0.5", "text/plain; q=0.5" };
            List<MimeTypeSelector> selectors = MimeTypeSelector.CreateSelectors(types).ToList();
            this.PrintSelectors(selectors);

            Assert.AreEqual("text/turtle", selectors[0].Type);
            Assert.AreEqual(0.5d, selectors[0].Quality);
            Assert.AreEqual("text/plain", selectors[1].Type);
            Assert.AreEqual(0.5d, selectors[1].Quality);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionByUpperCaseExt()
        {
            foreach (MimeTypeDefinition def in MimeTypesHelper.Definitions)
            {
                if (!def.HasFileExtensions) continue;
                String ext = def.CanonicalFileExtension.ToUpper();
                MimeTypeDefinition def2 = MimeTypesHelper.GetDefinitionsByFileExtension(ext).FirstOrDefault();
                Assert.IsNotNull(def2);
                Assert.AreEqual(def.SyntaxName, def2.SyntaxName);
                Assert.AreEqual(def.CanonicalMimeType, def2.CanonicalMimeType);
                Assert.AreEqual(def.HasFileExtensions, def2.HasFileExtensions);
                Assert.AreEqual(def.CanonicalFileExtension, def2.CanonicalFileExtension);
                Assert.AreEqual(def.CanParseRdf, def2.CanParseRdf);
                Assert.AreEqual(def.CanParseRdfDatasets, def2.CanParseRdfDatasets);
                Assert.AreEqual(def.CanParseSparqlResults, def2.CanParseSparqlResults);
                Assert.AreEqual(def.CanWriteRdf, def2.CanWriteRdf);
                Assert.AreEqual(def.CanWriteRdfDatasets, def2.CanWriteRdfDatasets);
                Assert.AreEqual(def.CanWriteSparqlResults, def2.CanWriteSparqlResults);
                Assert.AreEqual(def.RdfParserType, def2.RdfParserType);
                Assert.AreEqual(def.RdfDatasetParserType, def2.RdfDatasetParserType);
                Assert.AreEqual(def.SparqlResultsParserType, def2.SparqlResultsParserType);
                Assert.AreEqual(def.RdfWriterType, def2.RdfWriterType);
                Assert.AreEqual(def.RdfDatasetWriterType, def2.RdfDatasetWriterType);
                Assert.AreEqual(def.SparqlResultsWriterType, def2.SparqlResultsWriterType);
            }
        }
    }
}
