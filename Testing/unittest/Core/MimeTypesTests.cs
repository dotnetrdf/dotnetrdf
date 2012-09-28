using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Test.Core
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

        [TestMethod]
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

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedNotation3Parser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNotation3Writer), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtNotation3_4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("n3.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedNotation3Parser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNotation3Writer), d.RdfWriterType);
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

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
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

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
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

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
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

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(TurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(CompressingTurtleWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtTurtle3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".ttl.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtTurtle4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("ttl.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
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

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedNTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNTriplesWriter), d.RdfWriterType);
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

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedNTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNTriplesWriter), d.RdfWriterType);
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

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedNTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNTriplesWriter), d.RdfWriterType);
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

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedNTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNTriplesWriter), d.RdfWriterType);
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

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedNTriplesParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedNTriplesWriter), d.RdfWriterType);
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfXmlWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfXml4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("rdf.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfXmlParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfXmlWriter), d.RdfWriterType);
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

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedRdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfJsonWriter), d.RdfWriterType);
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

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedRdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfJsonWriter), d.RdfWriterType);
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

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfJsonWriter), d.RdfWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtRdfJson4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("rj.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedRdfJsonParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedRdfJsonWriter), d.RdfWriterType);
        }

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
    }
}
