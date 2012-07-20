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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Validation;
using VDS.RDF.Writing;

namespace VDS.RDF.Utilities.Editor.Syntax
{
    public static class SyntaxManager
    {
        private static bool _init = false;

        private static List<SyntaxDefinition> _builtinDefs = new List<SyntaxDefinition>()
        {
            new SyntaxDefinition("RdfXml", "rdfxml.xshd", new String[] { ".rdf", ".owl" }, new RdfXmlParser(), new RdfXmlWriter(), new RdfSyntaxValidator(new RdfXmlParser())),
            new SyntaxDefinition("Turtle", "turtle.xshd", new String[] { ".ttl", ".n3" }, new TurtleParser(), new CompressingTurtleWriter(WriterCompressionLevel.High), new RdfSyntaxValidator(new TurtleParser())),
            new SyntaxDefinition("NTriples", "ntriples.xshd", new String[] { ".nt", ".nq" }, new NTriplesParser(), new NTriplesWriter(), new RdfSyntaxValidator(new NTriplesParser())),
            new SyntaxDefinition("Notation3", "n3.xshd", new String[] { ".n3" }, new Notation3Parser(), new Notation3Writer(), new RdfSyntaxValidator(new Notation3Parser())),
            new SyntaxDefinition("RdfJson", "rdfjson.xshd", new String[] { ".json" }, new RdfJsonParser(), new RdfJsonWriter(), new RdfSyntaxValidator(new RdfJsonParser())),
            new SyntaxDefinition("XHtmlRdfA", "xhtml-rdfa.xshd", new String[] { ".html", ".xhtml", ".htm", ".shtml" }, new RdfAParser(), new HtmlWriter(), new RdfStrictSyntaxValidator(new RdfAParser())),

            new SyntaxDefinition("SparqlQuery10", "sparql-query.xshd", new String[] { ".rq", ".sparql", }, new SparqlQueryValidator(SparqlQuerySyntax.Sparql_1_0)),
            new SyntaxDefinition("SparqlQuery11", "sparql-query-11.xshd", new String[] { ".rq", ".sparql" }, new SparqlQueryValidator(SparqlQuerySyntax.Sparql_1_1)),

            new SyntaxDefinition("SparqlResultsXml", "sparql-results-xml.xshd", new String[] { ".srx" }, new SparqlResultsValidator(new SparqlXmlParser())),
            new SyntaxDefinition("SparqlResultsJson", "sparql-results-json.xshd", new String[] { }, new SparqlResultsValidator(new SparqlJsonParser())),

            new SyntaxDefinition("SparqlUpdate11", "sparql-update.xshd", new String[] { }, new SparqlUpdateValidator()),

            new SyntaxDefinition("NQuads", "nquads.xshd", new String[] { ".nq" }, new RdfDatasetSyntaxValidator(new NQuadsParser())),
            new SyntaxDefinition("TriG", "trig.xshd", new String[] { ".trig" }, new RdfDatasetSyntaxValidator(new TriGParser())),
            new SyntaxDefinition("TriX", "trix.xshd", new String[] { ".xml" }, new RdfDatasetSyntaxValidator(new TriXParser()))
        };

        public static void Initialise()
        {
            if (_init) return;
            _init = true;

            //Set Comment Settings
            SetCommentCharacters("RdfXml", null, "<!--", "-->");
            SetCommentCharacters("Turtle", "#", null, null);
            SetCommentCharacters("NTriples", "Turtle");
            SetCommentCharacters("Notation3", "Turtle");
            SetCommentCharacters("RdfJson", "//", "/*", "*/");
            SetCommentCharacters("XHtmlRdfA", "RdfXml");
            SetCommentCharacters("SparqlQuery10", "Turtle");
            SetCommentCharacters("SparqlQuery11", "Turtle");
            SetCommentCharacters("SparqlResultsXml", "RdfXml");
            SetCommentCharacters("SparqlResultsJson", "RdfJson");
            SetCommentCharacters("SparqlUpdate11", "Turtle");
            SetCommentCharacters("NQuads", "Turtle");
            SetCommentCharacters("TriG", "Turtle");
            SetCommentCharacters("TriX", "RdfXml");

            //Set XML Formats
            SetXmlFormat("RdfXml");
            SetXmlFormat("XHtmlRdfA");
            SetXmlFormat("SparqlResultsXml");
            SetXmlFormat("TriX");
        }

        public static IEnumerable<SyntaxDefinition> Definitions
        {
            get
            {
                if (!_init) Initialise();
                return _builtinDefs;
            }
        }

        public static ISyntaxValidator GetValidator(String name)
        {
            foreach (SyntaxDefinition def in _builtinDefs)
            {
                if (def.Name.Equals(name)) return def.Validator;
            }
            return null;
        }

        public static IRdfReader GetParser(String name)
        {
            foreach (SyntaxDefinition def in _builtinDefs)
            {
                if (def.Name.Equals(name)) return def.DefaultParser;
            }
            return null;
        }

        public static IRdfWriter GetWriter(String name)
        {
            foreach (SyntaxDefinition def in _builtinDefs)
            {
                if (def.Name.Equals(name)) return def.DefaultWriter;
            }
            return null;
        }

        public static SyntaxDefinition GetDefinition(String name)
        {
            foreach (SyntaxDefinition def in _builtinDefs)
            {
                if (def.Name.Equals(name)) return def;
            }
            return null;
        }

        public static void SetCommentCharacters(String name, String singleLineComment, String multiLineCommentStart, String multiLineCommentEnd)
        {
            SyntaxDefinition def = GetDefinition(name);
            if (def != null)
            {
                def.SingleLineComment = singleLineComment;
                def.MultiLineCommentStart = multiLineCommentStart;
                def.MultiLineCommentEnd = multiLineCommentEnd;
            }
        }

        public static void SetCommentCharacters(String name, String copyFrom)
        {
            SyntaxDefinition def = GetDefinition(name);
            SyntaxDefinition sourceDef = GetDefinition(copyFrom);
            if (def != null && sourceDef != null)
            {
                def.SingleLineComment = sourceDef.SingleLineComment;
                def.MultiLineCommentStart = sourceDef.MultiLineCommentStart;
                def.MultiLineCommentEnd = sourceDef.MultiLineCommentEnd;
            }
        }

        public static void SetXmlFormat(String name)
        {
            SyntaxDefinition def = GetDefinition(name);
            if (def != null)
            {
                def.IsXmlFormat = true;
            }
        }
    }
}
