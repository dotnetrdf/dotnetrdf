/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
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
    /// <summary>
    /// The Syntax Manager is the central registry of supported syntaxes
    /// </summary>
    public static class SyntaxManager
    {
        private static bool _init = false;

        private static readonly List<SyntaxDefinition> _builtinDefs = new List<SyntaxDefinition>()
        {
            new SyntaxDefinition("RdfXml", "rdfxml.xshd", new String[] { ".rdf", ".owl" }, new RdfXmlParser(), new RdfXmlWriter(), new RdfSyntaxValidator(new RdfXmlParser())),
            new SyntaxDefinition("Turtle", "turtle.xshd", new String[] { ".ttl", ".n3" }, new TurtleParser(TurtleSyntax.Original), new CompressingTurtleWriter(WriterCompressionLevel.High, TurtleSyntax.Original), new RdfSyntaxValidator(new TurtleParser(TurtleSyntax.Original))),
            new SyntaxDefinition("Turtle11", "turtle11.xshd", new string[] { ".ttl", ".n3"}, new TurtleParser(TurtleSyntax.W3C), new CompressingTurtleWriter(WriterCompressionLevel.High, TurtleSyntax.W3C), new RdfSyntaxValidator(new TurtleParser(TurtleSyntax.W3C))),
            new SyntaxDefinition("NTriples", "ntriples.xshd", new String[] { ".nt", ".nq" }, new NTriplesParser(NTriplesSyntax.Original), new NTriplesWriter(NTriplesSyntax.Original), new RdfSyntaxValidator(new NTriplesParser(NTriplesSyntax.Original))),
            new SyntaxDefinition("NTriples11", "ntriples.xshd", new String[] { ".nt", ".nq" }, new NTriplesParser(NTriplesSyntax.Rdf11), new NTriplesWriter(NTriplesSyntax.Rdf11), new RdfSyntaxValidator(new NTriplesParser(NTriplesSyntax.Rdf11))),
            new SyntaxDefinition("Notation3", "n3.xshd", new String[] { ".n3" }, new Notation3Parser(), new Notation3Writer(), new RdfSyntaxValidator(new Notation3Parser())),
            new SyntaxDefinition("RdfJson", "rdfjson.xshd", new String[] { ".json" }, new RdfJsonParser(), new RdfJsonWriter(), new RdfSyntaxValidator(new RdfJsonParser())),
            new SyntaxDefinition("XHtmlRdfA", "xhtml-rdfa.xshd", new String[] { ".html", ".xhtml", ".htm", ".shtml" }, new RdfAParser(), new HtmlWriter(), new RdfStrictSyntaxValidator(new RdfAParser())),

            new SyntaxDefinition("SparqlQuery10", "sparql-query.xshd", new String[] { ".rq", ".sparql", }, new SparqlQueryValidator(SparqlQuerySyntax.Sparql_1_0)),
            new SyntaxDefinition("SparqlQuery11", "sparql-query-11.xshd", new String[] { ".rq", ".sparql" }, new SparqlQueryValidator(SparqlQuerySyntax.Sparql_1_1)),

            new SyntaxDefinition("SparqlResultsXml", "sparql-results-xml.xshd", new String[] { ".srx" }, new SparqlResultsValidator(new SparqlXmlParser())),
            new SyntaxDefinition("SparqlResultsJson", "sparql-results-json.xshd", new String[] { }, new SparqlResultsValidator(new SparqlJsonParser())),

            new SyntaxDefinition("SparqlUpdate11", "sparql-update.xshd", new String[] { }, new SparqlUpdateValidator()),

            new SyntaxDefinition("NQuads", "nquads.xshd", new String[] { ".nq" }, new RdfDatasetSyntaxValidator(new NQuadsParser(NQuadsSyntax.Original))),
            new SyntaxDefinition("NQuads11", "nquads11.xshd", new String[] { ".nq" }, new RdfDatasetSyntaxValidator(new NQuadsParser(NQuadsSyntax.Rdf11))),
            new SyntaxDefinition("TriG", "trig.xshd", new String[] { ".trig" }, new RdfDatasetSyntaxValidator(new TriGParser())),
            new SyntaxDefinition("TriX", "trix.xshd", new String[] { ".xml" }, new RdfDatasetSyntaxValidator(new TriXParser()))
        };

        /// <summary>
        /// Initialize the Syntax Manager
        /// </summary>
        public static void Initialise()
        {
            lock (_builtinDefs)
            {
                if (_init) return;
                _init = true;

                //Set Comment Settings
                SetCommentCharacters("RdfXml", null, "<!--", "-->");
                SetCommentCharacters("Turtle", "#", null, null);
                SetCommentCharacters("Turtle11", "#", null, null);
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
        }

        /// <summary>
        /// Gets the available syntax definitions
        /// </summary>
        public static IEnumerable<SyntaxDefinition> Definitions
        {
            get
            {
                if (!_init) Initialise();
                return _builtinDefs;
            }
        }

        /// <summary>
        /// Gets the syntax validator associated with a given syntax (if any)
        /// </summary>
        /// <param name="name">Syntax Name</param>
        /// <returns>Validator if available, null otherwise</returns>
        public static ISyntaxValidator GetValidator(String name)
        {
            foreach (SyntaxDefinition def in _builtinDefs)
            {
                if (def.Name.Equals(name)) return def.Validator;
            }
            return null;
        }

        /// <summary>
        /// Gets the RDF parser associated with a given syntax (if any)
        /// </summary>
        /// <param name="name">Syntax Name</param>
        /// <returns>RDF Parser if available, null otherwise</returns>
        public static IRdfReader GetParser(String name)
        {
            foreach (SyntaxDefinition def in _builtinDefs)
            {
                if (def.Name.Equals(name)) return def.DefaultParser;
            }
            return null;
        }

        /// <summary>
        /// Gets the RDF writer associated with a given syntax (if any)
        /// </summary>
        /// <param name="name">Syntax Name</param>
        /// <returns>RDF Writer if available, null otherwise</returns>
        public static IRdfWriter GetWriter(String name)
        {
            foreach (SyntaxDefinition def in _builtinDefs)
            {
                if (def.Name.Equals(name)) return def.DefaultWriter;
            }
            return null;
        }

        /// <summary>
        /// Gets a syntax definition
        /// </summary>
        /// <param name="name">Syntax Name</param>
        /// <returns>Definition or null if the syntax name is not known</returns>
        public static SyntaxDefinition GetDefinition(String name)
        {
            foreach (SyntaxDefinition def in _builtinDefs)
            {
                if (def.Name.Equals(name)) return def;
            }
            return null;
        }

        /// <summary>
        /// Sets the comment characters for a given syntax
        /// </summary>
        /// <param name="name">Syntax Name</param>
        /// <param name="singleLineComment">Single Line comment</param>
        /// <param name="multiLineCommentStart">Multi-line comment start</param>
        /// <param name="multiLineCommentEnd">Multi-line comment end</param>
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

        /// <summary>
        /// Sets the comment characters for a given syntax copying from another syntax
        /// </summary>
        /// <param name="name">Syntax Name</param>
        /// <param name="copyFrom">Syntax to copy from</param>
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

        /// <summary>
        /// Sets that a syntax is an XML format
        /// </summary>
        /// <param name="name">Syntax Name</param>
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
