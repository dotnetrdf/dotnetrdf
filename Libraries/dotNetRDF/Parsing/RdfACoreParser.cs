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

#if UNFINISHED

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Events;
using VDS.RDF.Parsing.Events.RdfA;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Possible attribute parsing modes for RDFa
    /// </summary>
    enum RdfACurieMode
    {
        /// <summary>
        /// Attribute must contain a URI
        /// </summary>
        Uri,
        /// <summary>
        /// Attributes must contain a Safe CURIE, CURIE or URI
        /// </summary>
        SafeCurieOrCurieOrUri,
        /// <summary>
        /// Attributes must contain a Term, CURIE or Absolute URI
        /// </summary>
        TermOrCurieOrAbsUri
    }

    /// <summary>
    /// Abstract Parser for RDFa 1.1 Core
    /// </summary>
    public abstract class RdfACoreParser : IRdfReader
    {
        private char[] _whitespace = new char[] { (char)0x20, (char)0x9, (char)0xD, (char)0xA };

        public void Load(IGraph g, StreamReader input)
        {
            if (g == null) throw new RdfParseException("Cannot parse RDF into a null Graph");
            this.Load(new GraphHandler(g), input);
        }

        public void Load(IGraph g, TextReader input)
        {
            if (g == null) throw new RdfParseException("Cannot parse RDF into a null Graph");
            this.Load(new GraphHandler(g), input);
        }

        public void Load(IGraph g, string filename)
        {
            if (g == null) throw new RdfParseException("Cannot parse RDF into a null Graph");
            if (filename == null) throw new RdfParseException("Cannot parser RDF from a null file");
            this.Load(new GraphHandler(g), new StreamReader(filename));
        }

        public void Load(IRdfHandler handler, StreamReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot parse RDF using a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannpt parse RDF from a null input");
            this.Load(handler, (TextReader)input);
        }

        public void Load(IRdfHandler handler, TextReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot parse RDF using a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannpt parse RDF from a null input");
            try
            {
                RdfACoreParserContext context = this.GetParserContext(handler, input);

                // Before we start parsing check if a Version has been specified
                if (context.HostLanguage.Version != null)
                {
                    switch (context.HostLanguage.Version)
                    {
                        case RdfAParser.XHtmlPlusRdfA10Version:
                        case RdfAParser.HtmlPlusRdfA10Version:
                            // If using RDFa 1.0 then should use the old parser instead
                            RdfAParser parser = new RdfAParser();
                            parser.Load(context.Handler, input);
                            return;
                    }
                }

                // For any other RDFa Version use this parser
                this.Parse(context);
            }
            catch
            {
                try
                {
                    input.Close();
                }
                catch
                {
                    // No catch actions just trying to clean up
                }
                throw;
            }
        }

        public void Load(IRdfHandler handler, string filename)
        {
            if (filename == null) throw new RdfParseException("Cannot parser RDF from a null file");
            this.Load(handler, new StreamReader(filename));
        }

        /// <summary>
        /// Abstract method which must be overridden by derived classes
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="reader">Input to read from</param>
        /// <returns></returns>
        protected abstract RdfACoreParserContext GetParserContext(IRdfHandler handler, TextReader reader);

        private void Parse(RdfACoreParserContext context)
        {
            try
            {
                context.Handler.StartRdf();

                // Initialise Term Mappings
                context.HostLanguage.InitTermMappings(context);

                IRdfAEvent current;
                bool first = true;
                int nesting = 0;
                INode rdfType = context.Handler.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
                List<IncompleteTriple> localIncompletes = new List<IncompleteTriple>();

                do
                {
                    localIncompletes.Clear();

                    // Dequeue the next event and increment/decrement nesting as appropriate
                    current = context.Events.Dequeue();
                    switch (current.EventType)
                    {
                        case Event.Element:
                            nesting++;
                            context.IncrementNesting();
                            break;
                        case Event.EndElement:
                            context.DecrementNesting();
                            nesting--;
                            continue;
                        default:
                            // Otherwise skip the event and continue
                            continue;
                            // throw new RdfParseException("Encountered an unexpected event of type '" + current.GetType().ToString() + "' when an Element/End Element event was expected", current.Position);
                    }
                    // Stop when nesting level returns to 0
                    if (nesting == 0) break;

                    // Step 1 - Initialisation
                    bool skip = false;
                    INode newSubj = null, currObjResource = null, currObjLiteral = null;

                    // Before carrying out further steps allow host specific special actions
                    context.HostLanguage.ParseExtensions(context, current);

                    // Step 2 - @profile parsing
                    if (current.HasAttribute("profile"))
                    {
                        if (!this.ParseProfileAttribute(context, current))
                        {
                            // If an @profile attribute fails to parse then must ignore the current subtree
                            int i = 0;
                            IRdfAEvent next;
                            do
                            {
                                next = context.Events.Dequeue();
                                // Keep track of nesting in the subtree
                                if (next.EventType == Event.Element) i++;
                                if (next.EventType == Event.EndElement) i--;
                            } while (i > 0 && next.EventType != Event.EndElement && context.Events.Count > 0);

                            // If couldn't ignore a valid subtree then error
                            if (context.Events.Count == 0) throw new RdfParseException("Encountered an @profile attribute which pointed to a malformed profile which meant the relevant subtree of the document should be ignored but the document does not contain a well formed subtree");

                            // If a bad @profile then continue after we've ignored the subtree
                            continue;
                        }
                    }

                    // Step 3 - @vocab parsing
                    if (current.HasAttribute("vocab")) this.ParseVocabAttribute(context, current);

                    // Step 4 - @prefix parsing (plus any other mechanisms defined by the host language)
                    if (current.HasAttribute("prefix")) this.ParsePrefixAttribute(context, current);
                    context.HostLanguage.ParsePrefixMappings(context, current);

                    // Step 5 - Language parsing
                    context.HostLanguage.ParseLiteralLanguage(context, current);

                    if (!current.HasAttribute("rel") && !current.HasAttribute("rev"))
                    {
                        // Step 6 - If no @rel or @rev establish a subject
                        newSubj = this.ParseSubject(context, current, nesting == 1, out skip);
                    }
                    else
                    {
                        // Step 7 - Use @rel or @rev to establish a subject and an object resource
                        newSubj = this.ParseRelOrRevSubject(context, current, nesting == 1);
                        currObjResource = this.ParseObjectResource(context, current);
                    }

                    // Step 8 - If there is a non-null subject process @typeof
                    if (newSubj != null)
                    {
                        if (current.HasAttribute("typeof"))
                        {
                            foreach (INode n in this.ParseTypeofAttribute(context, current))
                            {
                                if (!context.Handler.HandleTriple(new Triple(newSubj, rdfType, n))) ParserHelper.Stop();
                            }
                        }
                    }

                    if (currObjResource != null)
                    {
                        // Step 9 - If there is a non-null object resource generate triples
                        if (current.HasAttribute("rel"))
                        {
                            foreach (INode n in this.ParseRelAttribute(context, current))
                            {
                                if (!context.Handler.HandleTriple(new Triple(newSubj, n, currObjResource))) ParserHelper.Stop();
                            }
                        }
                        if (current.HasAttribute("rev"))
                        {
                            foreach (INode n in this.ParseRevAttribute(context, current))
                            {
                                if (!context.Handler.HandleTriple(new Triple(currObjResource, n, newSubj))) ParserHelper.Stop();
                            }
                        }
                    }
                    else if (newSubj != null)
                    {
                        // Step 10 - If there is no object resource but there are predicates generate incomplete triples
                        currObjResource = context.Handler.CreateBlankNode();
                        if (current.HasAttribute("rel"))
                        {
                            foreach (INode n in this.ParseRelAttribute(context, current))
                            {
                                localIncompletes.Add(new IncompleteTriple(n, IncompleteTripleDirection.Forward));
                            }
                        }
                        if (current.HasAttribute("rev"))
                        {
                            foreach (INode n in this.ParseRevAttribute(context, current))
                            {
                                localIncompletes.Add(new IncompleteTriple(n, IncompleteTripleDirection.Reverse));
                            }
                        }
                    }

                    // Step 11 - Establish the current object literal
                    if (newSubj != null && current.HasAttribute("property"))
                    {
                        // Must be an @property attribute in order for any triples to be generated
                        List<INode> ps = this.ParsePropertyAttribute(context, current).ToList();

                        if (ps.Count > 0)
                        {
                            Uri dtUri;
                            if (current.HasAttribute("content"))
                            {
                                // If @content is present then either a plain/typed literal
                                if (current.HasAttribute("datatype"))
                                {
                                    // @datatype is present so typed literal
                                    dtUri = this.ParseUri(context, current["datatype"], RdfACurieMode.TermOrCurieOrAbsUri);
                                    currObjLiteral = context.Handler.CreateLiteralNode(current["content"], dtUri);
                                }
                                else
                                {
                                    // Plain literal
                                    currObjLiteral = context.Handler.CreateLiteralNode(current["content"], context.Language);
                                }
                            }
                            else if (current.HasAttribute("datatype"))
                            {
                                // Typed literal
                                dtUri = this.ParseUri(context, current["datatype"], RdfACurieMode.TermOrCurieOrAbsUri);
                                if (dtUri != null)
                                {
                                    if (dtUri.ToString().Equals(RdfSpecsHelper.RdfXmlLiteral))
                                    {
                                        // XML Literal using element content
                                        currObjLiteral = context.Handler.CreateLiteralNode(this.ParseXmlContent(context), dtUri);
                                    }
                                    else
                                    {
                                        // Typed Literal using element content
                                        currObjLiteral = context.Handler.CreateLiteralNode(this.ParseTextContent(context), dtUri);
                                    }
                                }
                                else
                                {
                                    // If datatype does not resolve fall back to plain literal using element content
                                    currObjLiteral = context.Handler.CreateLiteralNode(this.ParseTextContent(context), context.Language);
                                }
                            }
                            else
                            {
                                // Plain Literal using element content
                                currObjLiteral = context.Handler.CreateLiteralNode(this.ParseTextContent(context), context.Language);
                            }

                            // Generate the relevant triples
                            foreach (INode p in ps)
                            {
                                if (!context.Handler.HandleTriple(new Triple(newSubj, p, currObjLiteral))) ParserHelper.Stop();
                            }
                        }
                    }

                    // Step 12 - Complete any existing incomplete triples
                    if (!skip && newSubj != null)
                    {
                        foreach (IncompleteTriple t in context.IncompleteTriples)
                        {
                            if (t.Direction == IncompleteTripleDirection.Forward)
                            {
                                if (!context.Handler.HandleTriple(new Triple(context.ParentSubject, t.Predicate, newSubj))) ParserHelper.Stop();
                            }
                            else
                            {
                                if (!context.Handler.HandleTriple(new Triple(newSubj, t.Predicate, context.ParentSubject))) ParserHelper.Stop();
                            }
                        }
                        context.IncompleteTriples.Clear();
                    }

                    // Step 13 - Set up for processing child elements if necessary
                    // Can skip this if the next event is an end element
                    if (context.Events.Peek().EventType != Event.EndElement)
                    {
                        context.ParentSubject = newSubj;
                        if (currObjResource != null)
                        {
                            context.ParentObject = currObjResource;
                        }
                        else if (newSubj != null)
                        {
                            context.ParentObject = newSubj;
                        }
                        else
                        {
                            context.ParentObject = context.ParentSubject;
                        }
                        context.IncompleteTriples.AddRange(localIncompletes);
                    }
                } while (context.Events.Count > 0);

                if (context.Events.Count > 0) throw new RdfParseException("Source document is malformed! Encountered an end element which closes the document element but there are still more elements to be parsed", current.Position);

                context.Handler.EndRdf(true);
            }
            catch (RdfParsingTerminatedException)
            {
                context.Handler.EndRdf(true);
            }
            catch
            {
                context.Handler.EndRdf(false);
                throw;
            }
        }

        private bool ParseProfileAttribute(RdfACoreParserContext context, IRdfAEvent evt)
        {
            foreach (Uri u in this.ParseUris(context, evt["profile"]))
            {
                try
                {
                    Graph g = new Graph();
                    UriLoader.Load(g, u);

                    String prefixQuery = "PREFIX rdfa: <" + RdfAParser.RdfANamespace + "> SELECT SAMPLE(?prefix) AS ?NamespacePrefix SAMPLE(?uri) AS ?NamespaceURI WHERE { ?s rdfa:prefix ?prefix ; rdfa:uri ?uri } GROUP BY ?s HAVING (COUNT(?prefix) = 1 && COUNT(?uri) = 1)";
                    String termQuery = "PREFIX rdfa: <" + RdfAParser.RdfANamespace + "> SELECT SAMPLE(?term) AS ?Term SAMPLE(?uri) AS ?URI WHERE {?s rdfa:term ?term ; rdfa:uri ?uri } GROUP BY ?s HAVING (COUNT(?term) = 1 && COUNT(?uri) = 1)";

                    // Namespace Mappings
                    Object results = g.ExecuteQuery(prefixQuery);
                    if (results is SparqlResultSet)
                    {
                        SparqlResultSet rset = (SparqlResultSet)results;
                        foreach (SparqlResult r in rset.Results)
                        {
                            INode prefixNode = r["NamespacePrefix"];
                            INode nsNode = r["NamespaceURI"];
                            if (prefixNode.NodeType == NodeType.Literal && nsNode.NodeType == NodeType.Literal)
                            {
                                String prefix = ((ILiteralNode)prefixNode).Value.ToLower();
                                String ns = ((ILiteralNode)nsNode).Value;
                                context.Namespaces.AddNamespace(prefix, new Uri(ns));
                            }
                        }
                    }

                    // Term Mappings
                    results = g.ExecuteQuery(termQuery);
                    if (results is SparqlResultSet)
                    {
                        SparqlResultSet rset = (SparqlResultSet)results;
                        foreach (SparqlResult r in rset.Results)
                        {
                            INode termNode = r["Term"];
                            INode uriNode = r["URI"];
                            if (termNode.NodeType == NodeType.Literal && uriNode.NodeType == NodeType.Literal)
                            {
                                String term = ((ILiteralNode)termNode).Value;
                                String uri = ((ILiteralNode)uriNode).Value;
                                if (XmlSpecsHelper.IsNCName(term))
                                {
                                    context.Terms.AddNamespace(term, new Uri(uri));
                                }
                            }
                        }
                    }

                    // Vocabulary Setting
                    INode vocabNode = g.GetTriplesWithPredicate(g.CreateUriNode(new Uri(RdfAParser.RdfANamespace + "vocabulary"))).Select(t => t.Object).FirstOrDefault();
                    if (vocabNode != null)
                    {
                        if (vocabNode.NodeType == NodeType.Literal)
                        {
                            context.DefaultVocabularyUri = new Uri(((ILiteralNode)vocabNode).Value);
                        }
                        else if (vocabNode.NodeType == NodeType.Uri)
                        {
                            context.DefaultVocabularyUri = ((IUriNode)vocabNode).Uri;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        private void ParseVocabAttribute(RdfACoreParserContext context, IRdfAEvent evt)
        {
            context.DefaultVocabularyUri = this.ParseUri(context, evt["vocab"], RdfACurieMode.Uri);
        }

        private void ParsePrefixAttribute(RdfACoreParserContext context, IRdfAEvent evt)
        {
            String[] values = evt["prefix"].Split(this._whitespace);
            for (int i = 0; i < values.Length - 1; i += 2)
            {
                String prefix = values[i];
                Uri uri = this.ResolveAbsoluteUri(values[i + 1]);

                if (uri == null) continue;
                context.Namespaces.AddNamespace(prefix, uri);
            }
        }

        private INode ParseSubject(RdfACoreParserContext context, IRdfAEvent evt, bool isRoot, out bool skip)
        {
            skip = false;
            if (evt.HasAttribute("about"))
            {
                return this.ParseAboutAttribute(context, evt);
            }
            else if (evt.HasAttribute("src"))
            {
                return this.ParseSrcAttribute(context, evt);
            }
            else if (evt.HasAttribute("resource"))
            {
                return this.ParseResourceAttibute(context, evt);
            }
            else if (evt.HasAttribute("href"))
            {
                return this.ParseHrefAttribute(context, evt);
            }
            else if (isRoot || context.HostLanguage.IsRootElement(evt))
            {
                return this.UriToNode(context, this.ParseUri(context, String.Empty, RdfACurieMode.SafeCurieOrCurieOrUri));
            }
            else if (evt.HasAttribute("typeof"))
            {
                return context.Handler.CreateBlankNode();
            }
            else
            {
                if (evt.HasAttribute("property")) skip = true;
                return context.ParentObject;
            }
        }

        private INode ParseRelOrRevSubject(RdfACoreParserContext context, IRdfAEvent evt, bool isRoot)
        {
            if (evt.HasAttribute("about"))
            {
                return this.ParseAboutAttribute(context, evt);
            }
            else if (evt.HasAttribute("src"))
            {
                return this.ParseSrcAttribute(context, evt);
            }
            else if (isRoot || context.HostLanguage.IsRootElement(evt))
            {
                return this.UriToNode(context, this.ParseUri(context, String.Empty, RdfACurieMode.SafeCurieOrCurieOrUri));
            }
            else if (evt.HasAttribute("typeof"))
            {
                return context.Handler.CreateBlankNode();
            }
            else
            {
                return context.ParentObject;
            }
        }

        private INode ParseObjectResource(RdfACoreParserContext context, IRdfAEvent evt)
        {
            if (evt.HasAttribute("resource"))
            {
                return this.ParseResourceAttibute(context, evt);
            }
            else if (evt.HasAttribute("href"))
            {
                return this.ParseHrefAttribute(context, evt);
            }
            else
            {
                return null;
            }
        }

        private INode ParseAboutAttribute(RdfACoreParserContext context, IRdfAEvent evt)
        {
            return this.UriToNode(context, this.ParseUri(context, evt["about"], RdfACurieMode.SafeCurieOrCurieOrUri));
        }

        private INode ParseSrcAttribute(RdfACoreParserContext context, IRdfAEvent evt)
        {
            return this.UriToNode(context, this.ParseUri(context, evt["src"], RdfACurieMode.Uri));
        }

        private INode ParseResourceAttibute(RdfACoreParserContext context, IRdfAEvent evt)
        {
            return this.UriToNode(context, this.ParseUri(context, evt["resource"], RdfACurieMode.SafeCurieOrCurieOrUri));
        }

        private INode ParseHrefAttribute(RdfACoreParserContext context, IRdfAEvent evt)
        {
            return this.UriToNode(context, this.ParseUri(context, evt["href"], RdfACurieMode.Uri));
        }

        private IEnumerable<INode> ParseTypeofAttribute(RdfACoreParserContext context, IRdfAEvent evt)
        {
            return (from u in this.ParseUris(context, evt["typeof"])
                    select this.UriToNode(context, u));
        }

        private IEnumerable<INode> ParseRelAttribute(RdfACoreParserContext context, IRdfAEvent evt)
        {
            return (from u in this.ParseUris(context, evt["rel"])
                    select this.UriToNode(context, u));
        }

        private IEnumerable<INode> ParseRevAttribute(RdfACoreParserContext context, IRdfAEvent evt)
        {
            return (from u in this.ParseUris(context, evt["rev"])
                    select this.UriToNode(context, u));
        }

        private IEnumerable<INode> ParsePropertyAttribute(RdfACoreParserContext context, IRdfAEvent evt)
        {
            return (from u in this.ParseUris(context, evt["property"])
                    select this.UriToNode(context, u));
        }

        private String ParseTextContent(RdfACoreParserContext context)
        {
            int i = 1;
            IRdfAEvent next;
            StringBuilder output = new StringBuilder();
            List<IRdfAEvent> events = new List<IRdfAEvent>();
            do
            {
                next = context.Events.Dequeue();
                switch (next.EventType)
                {
                    case Event.Element:
                        i++;
                        break;
                    case Event.EndElement:
                        i--;
                        break;
                    case Event.Text:
                        output.Append(((TextEvent)next).Text);
                        break;
                }
                events.Add(next);
            } while (i > 0);

            foreach (IRdfAEvent evt in events)
            {
                context.Requeue(evt);
            }

            return output.ToString();
        }

        private String ParseXmlContent(RdfACoreParserContext context)
        {
            int i = 1;
            IRdfAEvent next;
            Stack<String> openElements = new Stack<string>();
            StringBuilder output = new StringBuilder();
            do
            {
                next = context.Events.Peek();
                switch (next.EventType)
                {
                    case Event.Element:
                        i++;
                        output.Append("<" + ((ElementEvent)next).Name);
                        openElements.Push(((ElementEvent)next).Name);
                        foreach (KeyValuePair<String, String> attr in next.Attributes)
                        {
                            output.Append(" " + attr.Key + "=\"" + attr.Value + "\"");
                        }
                        context.Events.Dequeue();
                        if (context.Events.Peek().EventType == Event.EndElement)
                        {
                            context.Events.Dequeue();
                            i--;
                            output.Append(" />");
                            openElements.Pop();
                        }
                        else
                        {
                            output.Append(">");
                        }
                        continue;

                    case Event.EndElement:
                        i--;
                        if (i == 0)
                        {
                            break;
                        }
                        output.Append("</" + openElements.Pop() + ">");
                        break;

                    case Event.Text:
                        output.Append(((TextEvent)next).Text);
                        break;
                }
                context.Events.Dequeue();
            } while (i > 0);

            return output.ToString();
        }

        #region Helper Functions for parsing Terms, CURIEs and URIs

        private INode UriToNode(RdfACoreParserContext context, Uri u)
        {
            if (u == null)
            {
                return null;
            } 
            else if (u.Scheme.Equals("rdfa"))
            {
                String id = u.ToString().Substring(11);
                if (id.Equals(String.Empty)) id = "rdfa-special-bnode";
                return context.Handler.CreateBlankNode(id);
            }
            else
            {
                return context.Handler.CreateUriNode(u);
            }
        }

        private IEnumerable<Uri> ParseUris(RdfACoreParserContext context, String value)
        {
            String[] values = value.Trim().Split(this._whitespace);
            return (from v in values
                    select this.ParseUri(context, value, RdfACurieMode.TermOrCurieOrAbsUri)).Where(u => u != null);
        }

        private Uri ParseUri(RdfACoreParserContext context, String value, RdfACurieMode mode)
        {
            switch (mode)
            {
                case RdfACurieMode.Uri:
                    // Resolve as a URI which may be relative
                    return this.ResolveUri(context, value);

                case RdfACurieMode.SafeCurieOrCurieOrUri:
                    if (this.IsSafeCurie(value))
                    {
                        // If a Safe CURIE must resolve as a CURIE ignoring if not resolvable
                        return this.ResolveSafeCurie(context, value);
                    }
                    else
                    {
                        // Otherwise try resolving as a CURIE and if not resolvable try as a URI
                        Uri u = this.ResolveCurie(context, value);
                        if (u == null)
                        {
                            // Try resolving as a URI
                            return this.ResolveUri(context, value);
                        }
                        else
                        {
                            // Resolved as a CURIE
                            return u;
                        }
                    }

                case RdfACurieMode.TermOrCurieOrAbsUri:
                    if (XmlSpecsHelper.IsNCName(value))
                    {
                        // If a Term try resolving as a term and ignore if not resolvable
                        return this.ResolveTerm(context, value);
                    }
                    else if (this.IsCurie(value))
                    {
                        // If a CURIE try resolving as a CURIE first
                        Uri u = this.ResolveCurie(context, value);
                        if (u == null)
                        {
                            // If not resolvable as a CURIE try as an absolute URI
                            return this.ResolveAbsoluteUri(value);
                        }
                        else
                        {
                            // Resolved as a CURIE
                            return u;
                        }
                    }
                    else
                    {
                        // Try resolving as an absolute URI
                        return this.ResolveAbsoluteUri(value);
                    }

                default:
                    return null;
            }
        }

        private Uri ResolveAbsoluteUri(String value)
        {
            try
            {
                String uri = Tools.ResolveUri(value, String.Empty);
                return new Uri(uri);
            }
            catch
            {
                return null;
            } 
        }

        private Uri ResolveUri(RdfACoreParserContext context, String value)
        {
            try
            {
                String uri = Tools.ResolveUri(value, context.BaseUri.ToSafeString());
                return new Uri(uri);
            }
            catch
            {
                return null;
            } 
        }

        private Uri ResolveCurie(RdfACoreParserContext context, String value)
        {
            if (value.Contains(":"))
            {
                String prefix = value.Substring(0, value.IndexOf(':'));
                String reference = (prefix.Length + 1 < value.Length ? value.Substring(value.IndexOf(':') + 1) : String.Empty);

                if (prefix.Equals(String.Empty))
                {
                    return new Uri(context.DefaultPrefixMapping.ToString() + reference);
                }
                else if (prefix.Equals("_"))
                {
                    if (reference.Equals(String.Empty))
                    {
                        // Q: Should this throw an error?
                        if (context.SpecialBNodeSeen) return null;
                        context.SpecialBNodeSeen = true;
                    }
                    return new Uri("rdfa:bnode:" + reference);
                }
                else
                {
                    if (context.Namespaces.HasNamespace(prefix))
                    {
                        return new Uri(context.Namespaces.GetNamespaceUri(prefix).ToString() + reference);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }
        }

        private Uri ResolveSafeCurie(RdfACoreParserContext context, String value)
        {
            return this.ResolveCurie(context, value.Substring(1, value.Length - 2));
        }

        private Uri ResolveTerm(RdfACoreParserContext context, String value)
        {
            if (context.Terms.HasNamespace(value))
            {
                return context.Terms.GetNamespaceUri(value);
            }
            else if (context.DefaultVocabularyUri != null)
            {
                return new Uri(context.DefaultVocabularyUri.ToString() + value);
            }
            else
            {
                return null;
            }
        }

        private bool IsSafeCurie(String value)
        {
            return value.StartsWith("[") && value.EndsWith("]");
        }

        private bool IsCurie(String value)
        {
            if (value.Contains(':'))
            {
                String prefix = value.Substring(0, value.IndexOf(':'));
                String reference = (prefix.Length + 1 < value.Length ? value.Substring(value.IndexOf(':') + 1) : String.Empty);
                if (XmlSpecsHelper.IsNCName(prefix))
                {
                    if (reference.Equals(String.Empty))
                    {
                        return true;
                    }
                    else if (IriSpecsHelper.IsIrelativeRef(reference))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return value.Equals(String.Empty) || IriSpecsHelper.IsIrelativeRef(value);
            }
        }

        #endregion

        /// <summary>
        /// Helper method for raising the Warning event
        /// </summary>
        /// <param name="message">Warning message</param>
        protected void RaiseWarning(String message)
        {
            RdfReaderWarning d = this.Warning;
            if (d != null)
            {
                d(message);
            }
        }

        public event RdfReaderWarning Warning;
    }
}

#endif