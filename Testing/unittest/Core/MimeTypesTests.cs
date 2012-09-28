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

        [TestMethod]
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

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlXmlParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlXmlWriter), d.SparqlResultsWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlXml4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("srx.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlXmlParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlXmlWriter), d.SparqlResultsWriterType);
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

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedSparqlJsonParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlJsonWriter), d.SparqlResultsWriterType);
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

            //Check normal definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(SparqlJsonParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(SparqlJsonWriter), d.SparqlResultsWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlJson3()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension(".srj.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlJsonParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlJsonWriter), d.SparqlResultsWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlJson4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("srj.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlJsonParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlJsonWriter), d.SparqlResultsWriterType);
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

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedSparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlCsvWriter), d.SparqlResultsWriterType);
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

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedSparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlCsvWriter), d.SparqlResultsWriterType);
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

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlCsvWriter), d.SparqlResultsWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlCsv4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("csv.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlCsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlCsvWriter), d.SparqlResultsWriterType);
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

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedSparqlTsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlTsvWriter), d.SparqlResultsWriterType);
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

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlTsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlTsvWriter), d.SparqlResultsWriterType);
        }

        [TestMethod]
        public void MimeTypesGetDefinitionsByExtSparqlTsv4()
        {
            IEnumerable<MimeTypeDefinition> defs = MimeTypesHelper.GetDefinitionsByFileExtension("tsv.gz");
            Assert.AreEqual(1, defs.Count());

            //Check GZipped definition
            MimeTypeDefinition d = defs.First();
            Assert.AreEqual(typeof(GZippedSparqlTsvParser), d.SparqlResultsParserType);
            Assert.AreEqual(typeof(GZippedSparqlTsvWriter), d.SparqlResultsWriterType);
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

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
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

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
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

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
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

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
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

            //Check GZipped definition
            d = defs.Last();
            Assert.AreEqual(typeof(GZippedTurtleParser), d.RdfParserType);
            Assert.AreEqual(typeof(GZippedTurtleWriter), d.RdfWriterType);
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

        [TestMethod]
        public void MimeTypesGetWriterByTypeUnknown()
        {
            IRdfWriter writer = MimeTypesHelper.GetWriter("application/unknown");
            Assert.IsInstanceOfType(writer, typeof(NTriplesWriter));
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
    }
}
