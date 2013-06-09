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
using NUnit.Framework;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Writing;

namespace VDS.RDF
{
    [TestFixture]
    public class MimeTypesTests
    {
        [SetUp]
        public void Setup()
        {
            MimeTypesHelper.ResetDefinitions();
        }

        [TearDown]
        public void Teardown()
        {
            MimeTypesHelper.ResetDefinitions();
        }

        [Test]
        public void MimeTypesGetDefinitionsAll()
        {
            int count = MimeTypesHelper.Definitions.Count();
            Console.WriteLine(count + " Definitions registered");
            Assert.AreEqual(30, count);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeAny()
        {
            int count = MimeTypesHelper.GetDefinitions(MimeTypesHelper.Any).Count();
            Console.WriteLine(count + " Definitions registered");
            Assert.AreEqual(30, count);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeNotation3_1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/n3");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(Notation3Parser), d.RdfParserType);
            Assert.AreEqual(typeof(Notation3Writer), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedNotation3Parser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNotation3Writer), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeNotation3_2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/rdf+n3");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(Notation3Parser), d.RdfParserType);
            Assert.AreEqual(typeof(Notation3Writer), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedNotation3Parser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNotation3Writer), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtNotation3_1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".n3");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(Notation3Parser), d.RdfParserType);
            Assert.AreEqual(typeof(Notation3Writer), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtNotation3_2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("n3");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(Notation3Parser), d.RdfParserType);
            Assert.AreEqual(typeof(Notation3Writer), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtNotation3_3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".n3.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedNotation3Parser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNotation3Writer), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtNotation3_4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("n3.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedNotation3Parser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNotation3Writer), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeTurtle1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/turtle");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeTurtle2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/x-turtle");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeTurtle3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/turtle");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtTurtle1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".ttl");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtTurtle2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("ttl");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtTurtle3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".ttl.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtTurtle4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("ttl.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeNTriples1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/rdf-triples");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(NTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(NTriplesWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedNTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNTriplesWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeNTriples2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/plain");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(NTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(NTriplesWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedNTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNTriplesWriter), d.RdfWriterType);
        }
       
        [Test]
        public void MimeTypesGetDefinitionsByTypeNTriples3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/ntriples");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(NTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(NTriplesWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedNTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNTriplesWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeNTriples4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/ntriples+turtle");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(NTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(NTriplesWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedNTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNTriplesWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeNTriples5()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/x-ntriples");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(NTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(NTriplesWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedNTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNTriplesWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtNTriples1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".nt");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(NTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(NTriplesWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtNTriples2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("nt");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(NTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(NTriplesWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtNTriples3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".nt.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedNTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNTriplesWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtNTriples4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("nt.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedNTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNTriplesWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeRdfXml1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/rdf+xml");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfXmlWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedRdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfXmlWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeRdfXml2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/xml");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfXmlWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedRdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfXmlWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeRdfXml3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/xml");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfXmlWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedRdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfXmlWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfXml1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".rdf");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfXmlWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfXml2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("rdf");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfXmlWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfXml3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".rdf.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfXmlWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfXml4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("rdf.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfXmlWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeRdfJson1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/json");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfJsonWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedRdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfJsonWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeRdfJson2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/json");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfJsonWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedRdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfJsonWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfJson1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".rj");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfJsonWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfJson2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("rj");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(RdfJsonWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfJson3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".rj.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfJsonWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfJson4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("rj.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfJsonWriter), d.RdfWriterType);
        }

        [Test]
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

        [Test]
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

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfA1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".html");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(HtmlWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfA2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("html");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(HtmlWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfA3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".html.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfAWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfA4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("html.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfAWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfA5()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".htm");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(HtmlWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfA6()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("htm");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(HtmlWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfA7()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".htm.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfAWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfA8()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("htm.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfAWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfA9()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".xhtml");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(HtmlWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfA10()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("xhtml");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(RdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(HtmlWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfA11()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".xhtml.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfAWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtRdfA12()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("xhtml.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfAParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfAWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeSparqlXml1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/sparql-results+xml");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlXmlParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlXmlWriter), d.SparqlResultsWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedSparqlXmlParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlXmlWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtSparqlXml1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".srx");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlXmlParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlXmlWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtSparqlXml2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("srx");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlXmlParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlXmlWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtSparqlXml3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".srx.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlXmlParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlXmlWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtSparqlXml4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("srx.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlXmlParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlXmlWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeSparqlJson1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("application/sparql-results+json");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlJsonParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlJsonWriter), d.SparqlResultsWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedSparqlJsonParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlJsonWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtSparqlJson1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".srj");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlJsonParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlJsonWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtSparqlJson2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("srj");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlJsonParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlJsonWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtSparqlJson3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".srj.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlJsonParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlJsonWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtSparqlJson4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("srj.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlJsonParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlJsonWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeSparqlCsv1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/csv");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlCsvWriter), d.SparqlResultsWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedSparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlCsvWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeSparqlCsv2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/comma-separated-values");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlCsvWriter), d.SparqlResultsWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedSparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlCsvWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtSparqlCsv1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".csv");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlCsvWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtSparqlCsv2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("csv");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlCsvWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtSparqlCsv3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".csv.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlCsvWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtSparqlCsv4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("csv.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlCsvWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeSparqlTsv1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/tab-separated-values");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlTsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlTsvWriter), d.SparqlResultsWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedSparqlTsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlTsvWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtSparqlTsv1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".tsv");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlTsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlTsvWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtSparqlTsv2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("tsv");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlTsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlTsvWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtSparqlTsv3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".tsv.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlTsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlTsvWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtSparqlTsv4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("tsv.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlTsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlTsvWriter), d.SparqlResultsWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeCaseSensitivity1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("TEXT/TURTLE");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeCaseSensitivity2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("TEXT/turtle");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeCaseSensitivity3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("TeXt/TuRtLe");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtCaseSensitivity1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".TTL");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByExtCaseSensitivity2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".tTl");
            Assert.AreEqual(1, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeExtraParams1()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/turtle; charset=utf-8");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetDefinitionsByTypeExtraParams2()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitions("text/turtle; q=1.0");
            Assert.AreEqual(2, defs.Count());

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
        }

        [Test]
        public void MimeTypesGetParserByTypeNTriples1()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("text/plain");
            Assert.IsInstanceOf<NTriplesParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByTypeNTriples2()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("text/ntriples");
            Assert.IsInstanceOf<NTriplesParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByTypeNTriples3()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("text/ntriples+turtle");
            Assert.IsInstanceOf<NTriplesParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByTypeNTriples4()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("application/rdf-triples");
            Assert.IsInstanceOf<NTriplesParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByTypeNTriples5()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("application/x-ntriples");
            Assert.IsInstanceOf<NTriplesParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByTypeTurtle1()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("text/turtle");
            Assert.IsInstanceOf<TurtleParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByTypeTurtle2()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("application/x-turtle");
            Assert.IsInstanceOf<TurtleParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByTypeTurtle3()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("application/turtle");
            Assert.IsInstanceOf<TurtleParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByTypeNotation3_1()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("text/n3");
            Assert.IsInstanceOf<Notation3Parser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByTypeNotation3_2()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("text/rdf+n3");
            Assert.IsInstanceOf<Notation3Parser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByTypeRdfXml1()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("text/xml");
            Assert.IsInstanceOf<RdfXmlParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByTypeRdfXml2()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("application/rdf+xml");
            Assert.IsInstanceOf<RdfXmlParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByTypeRdfXml3()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("application/xml");
            Assert.IsInstanceOf<RdfXmlParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByTypeRdfJson1()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("text/json");
            Assert.IsInstanceOf<RdfJsonParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByTypeRdfJson2()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("application/json");
            Assert.IsInstanceOf<RdfJsonParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByTypeRdfA1()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("text/html");
            Assert.IsInstanceOf<RdfAParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByTypeRdfA2()
        {
            IRdfReader parser = MimeTypesHelper.GetParser("application/xhtml+xml");
            Assert.IsInstanceOf<RdfAParser>(parser);
        }

        [Test, ExpectedException(typeof(RdfParserSelectionException))]
        public void MimeTypesGetParserByTypeUnknown()
        {
            IRdfReader reader = MimeTypesHelper.GetParser("application/unknown");
        }

        [Test]
        public void MimeTypesGetParserByExtNTriples1()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".nt");
            Assert.IsInstanceOf<NTriplesParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtNTriples2()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("nt");
            Assert.IsInstanceOf<NTriplesParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtNTriples3()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".nt.gz");
            Assert.IsInstanceOf<GZippedNTriplesParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtNTriples4()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("nt.gz");
            Assert.IsInstanceOf<GZippedNTriplesParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtTurtle1()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".ttl");
            Assert.IsInstanceOf<TurtleParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtTurtle2()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("ttl");
            Assert.IsInstanceOf<TurtleParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtTurtle3()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".ttl.gz");
            Assert.IsInstanceOf<GZippedTurtleParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtTurtle4()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("ttl.gz");
            Assert.IsInstanceOf<GZippedTurtleParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtNotation3_1()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".n3");
            Assert.IsInstanceOf<Notation3Parser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtNotation3_2()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("n3");
            Assert.IsInstanceOf<Notation3Parser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtNotation3_3()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".n3.gz");
            Assert.IsInstanceOf<GZippedNotation3Parser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtNotation3_4()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("n3.gz");
            Assert.IsInstanceOf<GZippedNotation3Parser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfXml1()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".rdf");
            Assert.IsInstanceOf<RdfXmlParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfXml2()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("rdf");
            Assert.IsInstanceOf<RdfXmlParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfXml3()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".rdf.gz");
            Assert.IsInstanceOf<GZippedRdfXmlParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfXml4()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("rdf.gz");
            Assert.IsInstanceOf<GZippedRdfXmlParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfJson1()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".rj");
            Assert.IsInstanceOf<RdfJsonParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfJson2()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("rj");
            Assert.IsInstanceOf<RdfJsonParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfJson3()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".rj.gz");
            Assert.IsInstanceOf<GZippedRdfJsonParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfJson4()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("rj.gz");
            Assert.IsInstanceOf<GZippedRdfJsonParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfA1()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".html");
            Assert.IsInstanceOf<RdfAParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfA2()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("html");
            Assert.IsInstanceOf<RdfAParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfA3()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".html.gz");
            Assert.IsInstanceOf<GZippedRdfAParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfA4()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("html.gz");
            Assert.IsInstanceOf<GZippedRdfAParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfA5()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".htm");
            Assert.IsInstanceOf<RdfAParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfA6()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("htm");
            Assert.IsInstanceOf<RdfAParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfA7()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".htm.gz");
            Assert.IsInstanceOf<GZippedRdfAParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfA8()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("htm.gz");
            Assert.IsInstanceOf<GZippedRdfAParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfA9()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".xhtml");
            Assert.IsInstanceOf<RdfAParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfA10()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("xhtml");
            Assert.IsInstanceOf<RdfAParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfA11()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".xhtml.gz");
            Assert.IsInstanceOf<GZippedRdfAParser>(parser);
        }

        [Test]
        public void MimeTypesGetParserByExtRdfA12()
        {
            IRdfReader parser = MimeTypesHelper.GetParserByFileExtension("xhtml.gz");
            Assert.IsInstanceOf<GZippedRdfAParser>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByTypeUnknown()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/unknown");
            Assert.IsInstanceOf<CompressingTurtleWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeAny()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter(MimeTypesHelper.Any);
            Assert.IsInstanceOf<NTriplesWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeNTriples1()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("text/plain");
            Assert.IsInstanceOf<NTriplesWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeNTriples2()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("text/ntriples");
            Assert.IsInstanceOf<NTriplesWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeNTriples3()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("text/ntriples+turtle");
            Assert.IsInstanceOf<NTriplesWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeNTriples4()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/rdf-triples");
            Assert.IsInstanceOf<NTriplesWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeNTriples5()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/x-ntriples");
            Assert.IsInstanceOf<NTriplesWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeTurtle1()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("text/turtle");
            Assert.IsInstanceOf<CompressingTurtleWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeTurtle2()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/x-turtle");
            Assert.IsInstanceOf<CompressingTurtleWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeTurtle3()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/turtle");
            Assert.IsInstanceOf<CompressingTurtleWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeNotation3_1()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("text/n3");
            Assert.IsInstanceOf<Notation3Writer>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeNotation3_2()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("text/rdf+n3");
            Assert.IsInstanceOf<Notation3Writer>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeRdfXml1()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("text/xml");
            Assert.IsInstanceOf<RdfXmlWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeRdfXml2()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/xml");
            Assert.IsInstanceOf<RdfXmlWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeRdfXml3()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/rdf+xml");
            Assert.IsInstanceOf<RdfXmlWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeRdfJson1()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("text/json");
            Assert.IsInstanceOf<RdfJsonWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeRdfJson2()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/json");
            Assert.IsInstanceOf<RdfJsonWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeRdfJson3()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/rdf+json");
            Assert.IsInstanceOf<RdfJsonWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeRdfA1()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("text/html");
            Assert.IsInstanceOf<HtmlWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByTypeRdfA2()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/xhtml+xml");
            Assert.IsInstanceOf<HtmlWriter>(writer);
        }

        [Test]
        public void MimeTypesGetWriterByExtNTriples1()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".nt");
            Assert.IsInstanceOf<NTriplesWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtNTriples2()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("nt");
            Assert.IsInstanceOf<NTriplesWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtNTriples3()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".nt.gz");
            Assert.IsInstanceOf<GZippedNTriplesWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtNTriples4()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("nt.gz");
            Assert.IsInstanceOf<GZippedNTriplesWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtTurtle1()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".ttl");
            Assert.IsInstanceOf<CompressingTurtleWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtTurtle2()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("ttl");
            Assert.IsInstanceOf<CompressingTurtleWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtTurtle3()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".ttl.gz");
            Assert.IsInstanceOf<GZippedTurtleWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtTurtle4()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("ttl.gz");
            Assert.IsInstanceOf<GZippedTurtleWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtNotation3_1()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".n3");
            Assert.IsInstanceOf<Notation3Writer>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtNotation3_2()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("n3");
            Assert.IsInstanceOf<Notation3Writer>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtNotation3_3()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".n3.gz");
            Assert.IsInstanceOf<GZippedNotation3Writer>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtNotation3_4()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("n3.gz");
            Assert.IsInstanceOf<GZippedNotation3Writer>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfXml1()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".rdf");
            Assert.IsInstanceOf<RdfXmlWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfXml2()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("rdf");
            Assert.IsInstanceOf<RdfXmlWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfXml3()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".rdf.gz");
            Assert.IsInstanceOf<GZippedRdfXmlWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfXml4()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("rdf.gz");
            Assert.IsInstanceOf<GZippedRdfXmlWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfJson1()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".rj");
            Assert.IsInstanceOf<RdfJsonWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfJson2()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("rj");
            Assert.IsInstanceOf<RdfJsonWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfJson3()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".rj.gz");
            Assert.IsInstanceOf<GZippedRdfJsonWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfJson4()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("rj.gz");
            Assert.IsInstanceOf<GZippedRdfJsonWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfA1()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".html");
            Assert.IsInstanceOf<HtmlWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfA2()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("html");
            Assert.IsInstanceOf<HtmlWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfA3()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".html.gz");
            Assert.IsInstanceOf<GZippedRdfAWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfA4()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("html.gz");
            Assert.IsInstanceOf<GZippedRdfAWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfA5()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".htm");
            Assert.IsInstanceOf<HtmlWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfA6()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("htm");
            Assert.IsInstanceOf<HtmlWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfA7()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".htm.gz");
            Assert.IsInstanceOf<GZippedRdfAWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfA8()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("htm.gz");
            Assert.IsInstanceOf<GZippedRdfAWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfA9()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".xhtml");
            Assert.IsInstanceOf<HtmlWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfA10()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("xhtml");
            Assert.IsInstanceOf<HtmlWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfA11()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension(".xhtml.gz");
            Assert.IsInstanceOf<GZippedRdfAWriter>(parser);
        }

        [Test]
        public void MimeTypesGetWriterByExtRdfA12()
        {
            IRdfWriter parser = MimeTypesHelper.GetWriterByFileExtension("xhtml.gz");
            Assert.IsInstanceOf<GZippedRdfAWriter>(parser);
        }

        [Test, ExpectedException(typeof(RdfParserSelectionException))]
        public void MimeTypesGetSparqlParserByTypeUnknown()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("application/unknown");
        }

        [Test]
        public void MimeTypesGetSparqlParserByTypeSparqlXml1()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("application/sparql-results+xml");
            Assert.IsInstanceOf<SparqlXmlParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByTypeSparqlXml2()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("application/xml");
            Assert.IsInstanceOf<SparqlXmlParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByTypeSparqlJson1()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("application/sparql-results+json");
            Assert.IsInstanceOf<SparqlJsonParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByTypeSparqlJson2()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("application/json");
            Assert.IsInstanceOf<SparqlJsonParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByTypeSparqlCsv1()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("text/csv");
            Assert.IsInstanceOf<SparqlCsvParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByTypeSparqlCsv2()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("text/comma-separated-values");
            Assert.IsInstanceOf<SparqlCsvParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByTypeSparqlTsv1()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser("text/tab-separated-values");
            Assert.IsInstanceOf<SparqlTsvParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByExtSparqlXml1()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".srx");
            Assert.IsInstanceOf<SparqlXmlParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByExtSparqlXml2()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("srx");
            Assert.IsInstanceOf<SparqlXmlParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByExtSparqlXml3()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".srx.gz");
            Assert.IsInstanceOf<GZippedSparqlXmlParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByExtSparqlXml4()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("srx.gz");
            Assert.IsInstanceOf<GZippedSparqlXmlParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByExtSparqlJson1()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".srj");
            Assert.IsInstanceOf<SparqlJsonParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByExtSparqlJson2()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("srj");
            Assert.IsInstanceOf<SparqlJsonParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByExtSparqlJson3()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".srj.gz");
            Assert.IsInstanceOf<GZippedSparqlJsonParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByExtSparqlJson4()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("srj.gz");
            Assert.IsInstanceOf<GZippedSparqlJsonParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByExtSparqlTsv1()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".tsv");
            Assert.IsInstanceOf<SparqlTsvParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByExtSparqlTsv2()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("tsv");
            Assert.IsInstanceOf<SparqlTsvParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByExtSparqlTsv3()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".tsv.gz");
            Assert.IsInstanceOf<GZippedSparqlTsvParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByExtSparqlTsv4()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("tsv.gz");
            Assert.IsInstanceOf<GZippedSparqlTsvParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByExtSparqlCsv1()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".csv");
            Assert.IsInstanceOf<SparqlCsvParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByExtSparqlCsv2()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("csv");
            Assert.IsInstanceOf<SparqlCsvParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByExtSparqlCsv3()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension(".csv.gz");
            Assert.IsInstanceOf<GZippedSparqlCsvParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlParserByExtSparqlCsv4()
        {
            ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParserByFileExtension("csv.gz");
            Assert.IsInstanceOf<GZippedSparqlCsvParser>(parser);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByTypeUnknown()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("application/unknown");
            Assert.IsInstanceOf<SparqlXmlWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByTypeAny()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter(MimeTypesHelper.Any);
            Assert.IsInstanceOf<SparqlXmlWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByTypeSparqlXml1()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("application/sparql-results+xml");
            Assert.IsInstanceOf<SparqlXmlWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByTypeSparqlXml2()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("application/xml");
            Assert.IsInstanceOf<SparqlXmlWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByTypeSparqlJson1()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("application/sparql-results+json");
            Assert.IsInstanceOf<SparqlJsonWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByTypeSparqlJson2()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("application/json");
            Assert.IsInstanceOf<SparqlJsonWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByTypeSparqlCsv1()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("text/csv");
            Assert.IsInstanceOf<SparqlCsvWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByTypeSparqlCsv2()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("text/comma-separated-values");
            Assert.IsInstanceOf<SparqlCsvWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByTypeSparqlTsv1()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter("text/tab-separated-values");
            Assert.IsInstanceOf<SparqlTsvWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByExtSparqlXml1()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".srx");
            Assert.IsInstanceOf<SparqlXmlWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByExtSparqlXml2()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("srx");
            Assert.IsInstanceOf<SparqlXmlWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByExtSparqlXml3()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".srx.gz");
            Assert.IsInstanceOf<GZippedSparqlXmlWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByExtSparqlXml4()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("srx.gz");
            Assert.IsInstanceOf<GZippedSparqlXmlWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByExtSparqlJson1()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".srj");
            Assert.IsInstanceOf<SparqlJsonWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByExtSparqlJson2()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("srj");
            Assert.IsInstanceOf<SparqlJsonWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByExtSparqlJson3()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".srj.gz");
            Assert.IsInstanceOf<GZippedSparqlJsonWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByExtSparqlJson4()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("srj.gz");
            Assert.IsInstanceOf<GZippedSparqlJsonWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByExtSparqlTsv1()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".tsv");
            Assert.IsInstanceOf<SparqlTsvWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByExtSparqlTsv2()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("tsv");
            Assert.IsInstanceOf<SparqlTsvWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByExtSparqlTsv3()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".tsv.gz");
            Assert.IsInstanceOf<GZippedSparqlTsvWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByExtSparqlTsv4()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("tsv.gz");
            Assert.IsInstanceOf<GZippedSparqlTsvWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByExtSparqlCsv1()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".csv");
            Assert.IsInstanceOf<SparqlCsvWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByExtSparqlCsv2()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("csv");
            Assert.IsInstanceOf<SparqlCsvWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByExtSparqlCsv3()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension(".csv.gz");
            Assert.IsInstanceOf<GZippedSparqlCsvWriter>(writer);
        }

        [Test]
        public void MimeTypesGetSparqlWriterByExtSparqlCsv4()
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriterByFileExtension("csv.gz");
            Assert.IsInstanceOf<GZippedSparqlCsvWriter>(writer);
        }

        [Test, ExpectedException(typeof(RdfParserSelectionException))]
        public void MimeTypesGetStoreParserByTypeUnknown()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParser("application/unknown");
        }

        [Test]
        public void MimeTypesGetStoreParserByTypeNQuads1()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParser("text/x-nquads");
            Assert.IsInstanceOf<NQuadsParser>(parser);
        }

        [Test]
        public void MimeTypesGetStoreParserByTypeTriG1()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParser("application/x-trig");
            Assert.IsInstanceOf<TriGParser>(parser);
        }

        [Test]
        public void MimeTypesGetStoreParserByTypeTriX1()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParser("application/trix");
            Assert.IsInstanceOf<TriXParser>(parser);
        }

        [Test]
        public void MimeTypesGetStoreParserByExtNQuads1()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".nq");
            Assert.IsInstanceOf<NQuadsParser>(parser);
        }

        [Test]
        public void MimeTypesGetStoreParserByExtNQuads2()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension("nq");
            Assert.IsInstanceOf<NQuadsParser>(parser);
        }

        [Test]
        public void MimeTypesGetStoreParserByExtNQuads3()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".nq.gz");
            Assert.IsInstanceOf<GZippedNQuadsParser>(parser);
        }

        [Test]
        public void MimeTypesGetStoreParserByExtNQuads4()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension("nq.gz");
            Assert.IsInstanceOf<GZippedNQuadsParser>(parser);
        }

        [Test]
        public void MimeTypesGetStoreParserByExtTriG1()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".trig");
            Assert.IsInstanceOf<TriGParser>(parser);
        }

        [Test]
        public void MimeTypesGetStoreParserByExtTriG2()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension("trig");
            Assert.IsInstanceOf<TriGParser>(parser);
        }

        [Test]
        public void MimeTypesGetStoreParserByExtTriG3()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".trig.gz");
            Assert.IsInstanceOf<GZippedTriGParser>(parser);
        }

        [Test]
        public void MimeTypesGetStoreParserByExtTriG4()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension("trig.gz");
            Assert.IsInstanceOf<GZippedTriGParser>(parser);
        }

        [Test]
        public void MimeTypesGetStoreParserByExtTriX1()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".xml");
            Assert.IsInstanceOf<TriXParser>(parser);
        }

        [Test]
        public void MimeTypesGetStoreParserByExtTriX2()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension("xml");
            Assert.IsInstanceOf<TriXParser>(parser);
        }

        [Test]
        public void MimeTypesGetStoreParserByExtTriX3()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".xml.gz");
            Assert.IsInstanceOf<GZippedTriXParser>(parser);
        }

        [Test]
        public void MimeTypesGetStoreParserByExtTriX4()
        {
            IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension("xml.gz");
            Assert.IsInstanceOf<GZippedTriXParser>(parser);
        }

        [Test]
        public void MimeTypesGetStoreWriterByTypeUnknown()
        {
            IStoreWriter writer = MimeTypesHelper.GetStoreWriter("application/unknown");
            Assert.IsInstanceOf<NQuadsWriter>(writer);
        }

        [Test]
        public void MimeTypesGetStoreWriterByTypeNQuads1()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriter("text/x-nquads");
            Assert.IsInstanceOf<NQuadsWriter>(parser);
        }

        [Test]
        public void MimeTypesGetStoreWriterByTypeTriG1()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriter("application/x-trig");
            Assert.IsInstanceOf<TriGWriter>(parser);
        }

        [Test]
        public void MimeTypesGetStoreWriterByTypeTriX1()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriter("application/trix");
            Assert.IsInstanceOf<TriXWriter>(parser);
        }

        [Test]
        public void MimeTypesGetStoreWriterByExtNQuads1()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension(".nq");
            Assert.IsInstanceOf<NQuadsWriter>(parser);
        }

        [Test]
        public void MimeTypesGetStoreWriterByExtNQuads2()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension("nq");
            Assert.IsInstanceOf<NQuadsWriter>(parser);
        }

        [Test]
        public void MimeTypesGetStoreWriterByExtNQuads3()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension(".nq.gz");
            Assert.IsInstanceOf<GZippedNQuadsWriter>(parser);
        }

        [Test]
        public void MimeTypesGetStoreWriterByExtNQuads4()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension("nq.gz");
            Assert.IsInstanceOf<GZippedNQuadsWriter>(parser);
        }

        [Test]
        public void MimeTypesGetStoreWriterByExtTriG1()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension(".trig");
            Assert.IsInstanceOf<TriGWriter>(parser);
        }

        [Test]
        public void MimeTypesGetStoreWriterByExtTriG2()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension("trig");
            Assert.IsInstanceOf<TriGWriter>(parser);
        }

        [Test]
        public void MimeTypesGetStoreWriterByExtTriG3()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension(".trig.gz");
            Assert.IsInstanceOf<GZippedTriGWriter>(parser);
        }

        [Test]
        public void MimeTypesGetStoreWriterByExtTriG4()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension("trig.gz");
            Assert.IsInstanceOf<GZippedTriGWriter>(parser);
        }

        [Test]
        public void MimeTypesGetStoreWriterByExtTriX1()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension(".xml");
            Assert.IsInstanceOf<TriXWriter>(parser);
        }

        [Test]
        public void MimeTypesGetStoreWriterByExtTriX2()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension("xml");
            Assert.IsInstanceOf<TriXWriter>(parser);
        }

        [Test]
        public void MimeTypesGetStoreWriterByExtTriX3()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension(".xml.gz");
            Assert.IsInstanceOf<GZippedTriXWriter>(parser);
        }

        [Test]
        public void MimeTypesGetStoreWriterByExtTriX4()
        {
            IStoreWriter parser = MimeTypesHelper.GetStoreWriterByFileExtension("xml.gz");
            Assert.IsInstanceOf<GZippedTriXWriter>(parser);
        }

        [Test]
        public void MimeTypesApplyWriterOptions1()
        {
            int compressionLevel = Options.DefaultCompressionLevel;
            try
            {
                Options.DefaultCompressionLevel = WriterCompressionLevel.High;
                IRdfWriter writer = MimeTypesHelper.GetWriter("application/turtle");
                Assert.IsInstanceOf<CompressingTurtleWriter>(writer);
                Assert.AreEqual(WriterCompressionLevel.High, ((ICompressingWriter)writer).CompressionLevel);
            }
            finally
            {
                Options.DefaultCompressionLevel = compressionLevel;
            }
        }

        [Test]
        public void MimeTypesApplyWriterOptions2()
        {
            int compressionLevel = Options.DefaultCompressionLevel;
            try
            {
                Options.DefaultCompressionLevel = WriterCompressionLevel.High;
                IRdfWriter writer = MimeTypesHelper.GetWriterByFileExtension(".ttl");
                Assert.IsInstanceOf<CompressingTurtleWriter>(writer);
                Assert.AreEqual(WriterCompressionLevel.High, ((ICompressingWriter)writer).CompressionLevel);
            }
            finally
            {
                Options.DefaultCompressionLevel = compressionLevel;
            }
        }

        [Test]
        public void MimeTypesApplyWriterOptions3()
        {
            int compressionLevel = Options.DefaultCompressionLevel;
            try
            {
                Options.DefaultCompressionLevel = WriterCompressionLevel.High;
                IStoreWriter writer = MimeTypesHelper.GetStoreWriter("application/x-trig");
                Assert.IsInstanceOf<TriGWriter>(writer);
                Assert.AreEqual(WriterCompressionLevel.High, ((ICompressingWriter)writer).CompressionLevel);
            }
            finally
            {
                Options.DefaultCompressionLevel = compressionLevel;
            }
        }

        [Test]
        public void MimeTypesApplyWriterOptions4()
        {
            int compressionLevel = Options.DefaultCompressionLevel;
            try
            {
                Options.DefaultCompressionLevel = WriterCompressionLevel.High;
                IStoreWriter writer = MimeTypesHelper.GetStoreWriterByFileExtension(".trig");
                Assert.IsInstanceOf<TriGWriter>(writer);
                Assert.AreEqual(WriterCompressionLevel.High, ((ICompressingWriter)writer).CompressionLevel);
            }
            finally
            {
                Options.DefaultCompressionLevel = compressionLevel;
            }
        }

        [Test]
        public void MimeTypesApplyParserOptions1()
        {
            TokenQueueMode queueMode = Options.DefaultTokenQueueMode;
            try
            {
                Options.DefaultTokenQueueMode = TokenQueueMode.AsynchronousBufferDuringParsing;
                IRdfReader parser = MimeTypesHelper.GetParser("application/turtle");
                Assert.IsInstanceOf<TurtleParser>(parser);
                Assert.AreEqual(TokenQueueMode.AsynchronousBufferDuringParsing, ((ITokenisingParser)parser).TokenQueueMode);
            }
            finally
            {
                Options.DefaultTokenQueueMode = queueMode;
            }
        }

        [Test]
        public void MimeTypesApplyParserOptions2()
        {
            TokenQueueMode queueMode = Options.DefaultTokenQueueMode;
            try
            {
                Options.DefaultTokenQueueMode = TokenQueueMode.AsynchronousBufferDuringParsing;
                IRdfReader parser = MimeTypesHelper.GetParserByFileExtension(".ttl");
                Assert.IsInstanceOf<TurtleParser>(parser);
                Assert.AreEqual(TokenQueueMode.AsynchronousBufferDuringParsing, ((ITokenisingParser)parser).TokenQueueMode);
            }
            finally
            {
                Options.DefaultTokenQueueMode = queueMode;
            }
        }

        [Test]
        public void MimeTypesApplyParserOptions3()
        {
            TokenQueueMode queueMode = Options.DefaultTokenQueueMode;
            try
            {
                Options.DefaultTokenQueueMode = TokenQueueMode.AsynchronousBufferDuringParsing;
                IStoreReader parser = MimeTypesHelper.GetStoreParser("text/x-nquads");
                Assert.IsInstanceOf<NQuadsParser>(parser);
                Assert.AreEqual(TokenQueueMode.AsynchronousBufferDuringParsing, ((ITokenisingParser)parser).TokenQueueMode);
            }
            finally
            {
                Options.DefaultTokenQueueMode = queueMode;
            }
        }

        [Test]
        public void MimeTypesApplyParserOptions4()
        {
            TokenQueueMode queueMode = Options.DefaultTokenQueueMode;
            try
            {
                Options.DefaultTokenQueueMode = TokenQueueMode.AsynchronousBufferDuringParsing;
                IStoreReader parser = MimeTypesHelper.GetStoreParserByFileExtension(".nq");
                Assert.IsInstanceOf<NQuadsParser>(parser);
                Assert.AreEqual(TokenQueueMode.AsynchronousBufferDuringParsing, ((ITokenisingParser)parser).TokenQueueMode);
            }
            finally
            {
                Options.DefaultTokenQueueMode = queueMode;
            }
        }

        [Test]
        public void MimeTypesContentNegotiation1()
        {
            String[] types = new String[] { "application/turtle" , "application/rdf+xml", "text/plain" };
            MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(types).FirstOrDefault();
            Assert.IsNotNull(def);
            Assert.AreEqual(typeof(TurtleParser), def.RdfParserType);
        }

        [Test]
        public void MimeTypesContentNegotiation2()
        {
            String[] types = new String[] { "application/rdf+xml", "application/turtle", "text/plain" };
            MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(types).FirstOrDefault();
            Assert.IsNotNull(def);
            Assert.AreEqual(typeof(RdfXmlParser), def.RdfParserType);
        }

        [Test]
        public void MimeTypesContentNegotiation3()
        {
            String[] types = new String[] { "text/plain", "application/rdf+xml", "application/turtle" };
            MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(types).FirstOrDefault();
            Assert.IsNotNull(def);
            Assert.AreEqual(typeof(NTriplesParser), def.RdfParserType);
        }

        [Test]
        public void MimeTypesContentNegotiation4()
        {
            MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(MimeTypesHelper.Any).FirstOrDefault();
            Assert.IsNotNull(def);
        }

        [Test]
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

        [Test]
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

        [Test]
        public void MimeTypesSelectors2()
        {
            String[] types = new String[] { "text/plain; q=0.5", "text/turtle" };
            List<MimeTypeSelector> selectors = MimeTypeSelector.CreateSelectors(types).ToList();
            this.PrintSelectors(selectors);

            Assert.AreEqual("text/turtle", selectors[0].Type);
            Assert.AreEqual("text/plain", selectors[1].Type);
            Assert.AreEqual(0.5d, selectors[1].Quality);
        }

        [Test]
        public void MimeTypesSelectors3()
        {
            String[] types = new String[] { "text/plain", "text/turtle" };
            List<MimeTypeSelector> selectors = MimeTypeSelector.CreateSelectors(types).ToList();
            this.PrintSelectors(selectors);

            Assert.AreEqual("text/plain", selectors[0].Type);
            Assert.AreEqual("text/turtle", selectors[1].Type);
        }

        [Test]
        public void MimeTypesSelectors4()
        {
            String[] types = new String[] { "text/*", "text/html", "*/*" };
            List<MimeTypeSelector> selectors = MimeTypeSelector.CreateSelectors(types).ToList();
            this.PrintSelectors(selectors);

            Assert.AreEqual("text/html", selectors[0].Type);
            Assert.AreEqual("text/*", selectors[1].Type);
            Assert.AreEqual(MimeTypesHelper.Any, selectors[2].Type);
        }

        [Test]
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

        [Test]
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

        [Test]
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
