using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Update;

namespace VDS.RDF.Test.Parsing
{
    [TestClass]
    public class ObjectParserTests
    {
        [TestMethod]
        public void ParsingObjectsListAvailable()
        {
            foreach (MimeTypeDefinition def in MimeTypesHelper.Definitions)
            {
                Console.WriteLine("Syntax: " + def.SyntaxName);
                foreach (KeyValuePair<Type, Type> kvp in def.ObjectParserTypes)
                {
                    Console.WriteLine("Parsed " + kvp.Key.Name + " using " + kvp.Value.Name);
                }
            }
        }

        [TestMethod]
        public void ParsingObjectsQueryTypeCheck()
        {
            Type target = typeof(SparqlQueryParser);
            MimeTypeDefinition def = MimeTypesHelper.Definitions.FirstOrDefault(d => d.CanParseObject<SparqlQuery>());
            Assert.AreNotEqual(null, def, "Should get a valid definition");
            Assert.AreEqual(target, def.GetObjectParserType<SparqlQuery>());
        }

        [TestMethod]
        public void ParsingObjectsUpdateTypeCheck()
        {
            Type target = typeof(SparqlUpdateParser);
            MimeTypeDefinition def = MimeTypesHelper.Definitions.FirstOrDefault(d => d.CanParseObject<SparqlUpdateCommandSet>());
            Assert.AreNotEqual(null, def, "Should get a valid definition");
            Assert.AreEqual(target, def.GetObjectParserType<SparqlUpdateCommandSet>());
        }

        [TestMethod]
        public void ParsingObjectsQueryParserCheck()
        {
            Type target = typeof(SparqlQueryParser);
            MimeTypeDefinition def = MimeTypesHelper.Definitions.FirstOrDefault(d => d.CanParseObject<SparqlQuery>());
            Assert.AreNotEqual(null, def, "Should get a valid definition");
            Assert.AreEqual(target, def.GetObjectParserType<SparqlQuery>());

            IObjectParser<SparqlQuery> parser = def.GetObjectParser<SparqlQuery>();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { ?s ?p ?o }");
            Console.WriteLine(q.ToString());
        }

        [TestMethod]
        public void ParsingObjectsUpdateParserCheck()
        {
            Type target = typeof(SparqlUpdateParser);
            MimeTypeDefinition def = MimeTypesHelper.Definitions.FirstOrDefault(d => d.CanParseObject<SparqlUpdateCommandSet>());
            Assert.AreNotEqual(null, def, "Should get a valid definition");
            Assert.AreEqual(target, def.GetObjectParserType<SparqlUpdateCommandSet>());

            IObjectParser<SparqlUpdateCommandSet> parser = def.GetObjectParser<SparqlUpdateCommandSet>();
            SparqlUpdateCommandSet cmds = parser.ParseFromString("CLEAR DEFAULT");
            Console.WriteLine(cmds.ToString());
        }
    }
}
