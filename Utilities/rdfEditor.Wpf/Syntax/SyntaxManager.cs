using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Rendering;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Validation;
using VDS.RDF.Writing;
using VDS.RDF.Utilities.Editor.AutoComplete;

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
            new SyntaxDefinition("RdfJson", "rdfjson.xshd", new String[] { ".rj", ".json" }, new RdfJsonParser(), new RdfJsonWriter(), new RdfSyntaxValidator(new RdfJsonParser())),
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

        private static ITextRunConstructionContext _colourContext = new FakeTextRunContext();

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
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Syntax Highlighting for " + def.Name + " will not be available as the Highlight Definition was malformed:\n" + ex.Message);
                    allOk = false;
                }
            }

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

            //Now customise colours if applicable
            SyntaxManager.UpdateHighlightingColours();

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
                    Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("VDS.RDF.Utilities.Editor.Syntax." + filename);
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

        public static void UpdateHighlightingColours()
        {
            //Only applicable if not using customised XSHD Files
            if (!Properties.Settings.Default.UseCustomisedXshdFiles)
            {
                IHighlightingDefinition h;

                //Apply XML Format Colours
                h = HighlightingManager.Instance.GetDefinition("XML");
                if (h != null)
                {
                    foreach (HighlightingColor c in h.NamedHighlightingColors)
                    {
                        switch (c.Name)
                        {
                            case "Comment":
                                AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourXmlComments);
                                break;
                            case "CData":
                                AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourXmlCData);
                                break;
                            case "DocType":
                            case "XmlDeclaration":
                                AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourXmlDocType);
                                break;
                            case "XmlTag":
                                AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourXmlTag);
                                break;
                            case "AttributeName":
                                AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourXmlAttrName);
                                break;
                            case "AttributeValue":
                                AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourXmlAttrValue);
                                break;
                            case "Entity":
                                AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourXmlEntity);
                                break;
                            case "BrokenEntity":
                                AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourXmlBrokenEntity);
                                break;

                        }
                    }
                }

                //Apply non-XML format colours
                foreach (SyntaxDefinition def in _builtinDefs)
                {
                    if (!def.IsXmlFormat)
                    {
                        h = def.Highlighter;
                        if (h != null)
                        {
                            foreach (HighlightingColor c in h.NamedHighlightingColors)
                            {
                                switch (c.Name)
                                {
                                    case "BNode":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourBNode);
                                        break;
                                    case "Comment":
                                    case "Comments":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourComment);
                                        break;
                                    case "EscapedChar":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourEscapedChar);
                                        break;
                                    case "Keyword":
                                    case "Keywords":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourKeyword);
                                        break;
                                    case "LangSpec":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourLangSpec);
                                        break;
                                    case "Numbers":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourNumbers);
                                        break;
                                    case "Punctuation":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourPunctuation);
                                        break;
                                    case "QName":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourQName);
                                        break;
                                    case "String":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourString);
                                        break;
                                    case "URI":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourURI);
                                        break;
                                    case "Variable":
                                        AdjustHighlightingColour(c, Properties.Settings.Default.SyntaxColourVariables);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void AdjustHighlightingColour(HighlightingColor current, Color desired)
        {
            if (!desired.Equals(current.Foreground.GetColor(_colourContext)))
            {
                current.Foreground = new CustomHighlightingBrush(desired);
            }
        }
    }
}
