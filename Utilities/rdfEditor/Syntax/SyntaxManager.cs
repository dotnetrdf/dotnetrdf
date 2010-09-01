using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using rdfEditor.AutoComplete;

namespace rdfEditor.Syntax
{
    public static class SyntaxManager
    {
        private static bool _init = false;

        private static List<SyntaxDefinition> _builtinDefs = new List<SyntaxDefinition>()
        {
            new SyntaxDefinition("RdfXml", "rdfxml.xshd", new String[] { ".rdf", ".owl" }, new RdfXmlParser(), new FastRdfXmlWriter(), new RdfSyntaxValidator(new RdfXmlParser())),
            new SyntaxDefinition("NTriples", "ntriples.xshd", new String[] { ".nt", ".nq" }, new NTriplesParser(), new NTriplesWriter(), new RdfSyntaxValidator(new NTriplesParser())),
            new SyntaxDefinition("Turtle", "turtle.xshd", new String[] { ".ttl", ".n3" }, new TurtleParser(), new CompressingTurtleWriter(WriterCompressionLevel.High), new RdfSyntaxValidator(new TurtleParser())),
            new SyntaxDefinition("Notation3", "n3.xshd", new String[] { ".n3" }, new Notation3Parser(), new Notation3Writer(), new RdfSyntaxValidator(new Notation3Parser())),
            new SyntaxDefinition("RdfJson", "rdfjson.xshd", new String[] { ".json" }, new RdfJsonParser(), new RdfJsonWriter(), new RdfSyntaxValidator(new RdfJsonParser())),
            new SyntaxDefinition("XHtmlRdfA", "xhtml-rdfa.xshd", new String[] { ".html", ".xhtml", ".htm", ".shtml" }, new RdfAParser(), new HtmlWriter(), new RdfStrictSyntaxValidator(new RdfAParser())),

            new SyntaxDefinition("SparqlQuery10", "sparql-query.xshd", new String[] { ".rq", ".sparql", }, new SparqlQueryValidator(SparqlQuerySyntax.Sparql_1_0)),
            new SyntaxDefinition("SparqlQuery11", "sparql-query-11.xshd", new String[] { ".rq", ".sparql" }, new SparqlQueryValidator(SparqlQuerySyntax.Sparql_1_1)),

            new SyntaxDefinition("SparqlResultsXml", "sparql-results-xml.xshd", new String[] { ".srx" }, new SparqlResultsValidator(new SparqlXmlParser())),
            new SyntaxDefinition("SparqlResultsJson", "sparql-results-json.xshd", new String[] { }, new SparqlResultsValidator(new SparqlJsonParser())),

            new SyntaxDefinition("SparqlUpdate11", "sparql-update.xshd", new String[] { }, new SparqlUpdateValidator())
        };

        public static bool Initialise()
        {
            if (_init) return true;
            _init = true;

            //Load in our Highlighters
            bool allOk = true;
            foreach (SyntaxDefinition def in _builtinDefs)
            {
                try
                {
                    HighlightingManager.Instance.RegisterHighlighting(def.Name, def.FileExtensions, def.Highlighter);
                }
                catch
                {
                    allOk = false;
                }
            }
            return allOk;
        }

        public static IHighlightingDefinition LoadHighlighting(String filename)
        {
            if (File.Exists(Path.Combine("syntax/", filename)))
            {
                return HighlightingLoader.Load(XmlReader.Create(Path.Combine("syntax/", filename)), HighlightingManager.Instance);
            }
            else
            {
                return HighlightingLoader.Load(XmlReader.Create(filename), HighlightingManager.Instance);
            }
        }

        public static IHighlightingDefinition LoadHighlighting(String filename, bool useResourceIfAvailable)
        {
            if (useResourceIfAvailable)
            {
                //If the user has specified to use customised XSHD files then we'll use the files from
                //the Syntax directory instead of the embedded resources
                if (!Properties.Settings.Default.UseCustomisedXshdFiles)
                {

                    //Try and load it from an embedded resource
                    Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("rdfEditor.Syntax." + filename);
                    if (resource != null)
                    {
                        return HighlightingLoader.Load(XmlReader.Create(resource), HighlightingManager.Instance);
                    }
                }
                else
                {
                    return LoadHighlighting(filename);
                }
            }

            //If no resource available try and load from file
            return HighlightingLoader.Load(XmlReader.Create(filename), HighlightingManager.Instance);
        }

        public static IHighlightingDefinition GetHighlighter(String name)
        {
            foreach (SyntaxDefinition def in _builtinDefs)
            {
                if (def.Name.Equals(name)) return def.Highlighter;
            }
            return null;
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
    }
}
