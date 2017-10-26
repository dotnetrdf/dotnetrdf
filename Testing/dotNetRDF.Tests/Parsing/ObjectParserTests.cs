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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Update;

namespace VDS.RDF.Parsing
{

    public class ObjectParserTests
    {
        [Fact]
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

        [Fact]
        public void ParsingObjectsQueryTypeCheck()
        {
            Type target = typeof(SparqlQueryParser);
            MimeTypeDefinition def = MimeTypesHelper.Definitions.FirstOrDefault(d => d.CanParseObject<SparqlQuery>());
            Assert.NotNull(def);
            Assert.Equal(target, def.GetObjectParserType<SparqlQuery>());
        }

        [Fact]
        public void ParsingObjectsUpdateTypeCheck()
        {
            Type target = typeof(SparqlUpdateParser);
            MimeTypeDefinition def = MimeTypesHelper.Definitions.FirstOrDefault(d => d.CanParseObject<SparqlUpdateCommandSet>());
            Assert.NotNull(def);
            Assert.Equal(target, def.GetObjectParserType<SparqlUpdateCommandSet>());
        }

        [Fact]
        public void ParsingObjectsQueryParserCheck()
        {
            Type target = typeof(SparqlQueryParser);
            MimeTypeDefinition def = MimeTypesHelper.Definitions.FirstOrDefault(d => d.CanParseObject<SparqlQuery>());
            Assert.NotNull(def);
            Assert.Equal(target, def.GetObjectParserType<SparqlQuery>());

            IObjectParser<SparqlQuery> parser = def.GetObjectParser<SparqlQuery>();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { ?s ?p ?o }");
            Console.WriteLine(q.ToString());
        }

        [Fact]
        public void ParsingObjectsUpdateParserCheck()
        {
            Type target = typeof(SparqlUpdateParser);
            MimeTypeDefinition def = MimeTypesHelper.Definitions.FirstOrDefault(d => d.CanParseObject<SparqlUpdateCommandSet>());
            Assert.NotNull(def);
            Assert.Equal(target, def.GetObjectParserType<SparqlUpdateCommandSet>());

            IObjectParser<SparqlUpdateCommandSet> parser = def.GetObjectParser<SparqlUpdateCommandSet>();
            SparqlUpdateCommandSet cmds = parser.ParseFromString("CLEAR DEFAULT");
            Console.WriteLine(cmds.ToString());
        }
    }
}
