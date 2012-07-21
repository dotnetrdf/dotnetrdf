/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

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
