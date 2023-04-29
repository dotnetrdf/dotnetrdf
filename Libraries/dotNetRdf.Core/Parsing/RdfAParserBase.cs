/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2023 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Base class for the framework-specific RDFa parser implementations.
    /// </summary>
    /// <typeparam name="THtmlDocument"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <typeparam name="TNode"></typeparam>
    /// <typeparam name="TAttribute"></typeparam>
    public abstract class RdfAParserBase<THtmlDocument, TElement, TNode, TAttribute> : IRdfReader
        where TElement : class, TNode
    {
        /// <summary>
        /// XHTML Vocab Namespace.
        /// </summary>
        public const string XHtmlVocabNamespace = "http://www.w3.org/1999/xhtml/vocab#";
        /// <summary>
        /// URI for the XHTML+RDFa DTD.
        /// </summary>
        public const string XHtmlPlusRdfADoctype = "http://www.w3.org/MarkUp/DTD/xhtml-rdfa-1.dtd";
        /// <summary>
        /// Namespace URI for XHTML.
        /// </summary>
        public const string XHtmlNamespace = "http://www.w3.org/1999/xhtml";
        /// <summary>
        /// Namespace URI for RDFa.
        /// </summary>
        public const string RdfANamespace = "http://www.w3.org/ns/rdfa#";

        /// <summary>
        /// RDFa Version Constants.
        /// </summary>
        public const string XHtmlPlusRdfA11Version = "XHTML+RDFa 1.1",
                             HtmlPlusRdfA11Version = "HTML+RDFa 1.1",
                             XHtmlPlusRdfA10Version = "XHTML+RDFa 1.0",
                             HtmlPlusRdfA10Version = "HTML+RDFa 1.0";

        private readonly RdfASyntax _syntax = RdfASyntax.AutoDetectLegacy;
        private readonly RdfAParserOptions _options = null;


        /// <summary>
        /// Creates a new RDFa Parser which will auto-detect which RDFa version to use (assumes 1.1 if none explicitly specified).
        /// </summary>
        protected RdfAParserBase()
        {

        }

        /// <summary>
        /// Creates a new RDFa Parser which will use the specified RDFa syntax.
        /// </summary>
        /// <param name="syntax">RDFa Syntax Version.</param>
        protected RdfAParserBase(RdfASyntax syntax)
        {
            _syntax = syntax;
        }

        /// <summary>
        /// Creates a new RDFa Parser configured with the provided parser options.
        /// </summary>
        /// <param name="options">The parser options settings.</param>
        protected RdfAParserBase(RdfAParserOptions options)
        {
            _syntax = options.Syntax;
            _options = options;
        }

        /// <summary>
        /// Parses RDFa by extracting it from the HTML from the given input.
        /// </summary>
        /// <param name="g">Graph to load into.</param>
        /// <param name="input">Stream to read from.</param>
        public void Load(IGraph g, StreamReader input)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Parses RDFa by extracting it from the HTML from the given input.
        /// </summary>
        /// <param name="g">Graph to load into.</param>
        /// <param name="input">Input to read from.</param>
        public void Load(IGraph g, TextReader input)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Parses RDFa by extracting it from the HTML from the given file.
        /// </summary>
        /// <param name="g">Graph to load into.</param>
        /// <param name="filename">File to read from.</param>
        public void Load(IGraph g, string filename)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");
            Load(new GraphHandler(g), filename);
        }

        /// <summary>
        /// Parses RDFa by extracting it from the HTML from the given input.
        /// </summary>
        /// <param name="handler">RDF Handler to use.</param>
        /// <param name="input">Stream to read from.</param>
        public void Load(IRdfHandler handler, StreamReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannot read RDF from a null Stream");
            Load(handler, (TextReader)input, UriFactory.Root);
        }

        /// <summary>
        /// Method for Loading RDF using a RDF Handler from some Concrete RDF Syntax via some arbitrary Stream.
        /// </summary>
        /// <param name="handler">RDF Handler to use.</param>
        /// <param name="input">The reader to read input from.</param>
        /// <param name="uriFactory">URI factory to use.</param>
        /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF.</exception>
        /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input.</exception>
        /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the Stream.</exception>
        public void Load(IRdfHandler handler, StreamReader input, IUriFactory uriFactory)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannot read RDF from a null Stream");
            if (uriFactory == null) throw new ArgumentNullException(nameof(uriFactory));
            Load(handler, (TextReader)input, uriFactory);

        }

        /// <summary>
        /// Parses RDFa by extracting it from the HTML from the given input.
        /// </summary>
        /// <param name="handler">RDF Handler to use.</param>
        /// <param name="input">Input to read from.</param>
        public void Load(IRdfHandler handler, TextReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannot read RDF from a null TextReader");
            Load(handler, input, UriFactory.Root);
        }

        /// <summary>
        /// Method for Loading RDF using a RDF Handler from some Concrete RDF Syntax via some arbitrary Stream.
        /// </summary>
        /// <param name="handler">RDF Handler to use.</param>
        /// <param name="input">The reader to read input from.</param>
        /// <param name="uriFactory">URI factory to use.</param>
        /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF.</exception>
        /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input.</exception>
        /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the Stream.</exception>
        public void Load(IRdfHandler handler, TextReader input, IUriFactory uriFactory)
        {
            try

            {
                THtmlDocument doc = LoadAndParse(input);

                var context = new RdfAParserContext<THtmlDocument>(handler, doc, uriFactory);
                Parse(context);
            }
            finally
            {
                try
                {
                    input.Close();
                }
                catch
                {
                    // Catch is just here in case something goes wrong with closing the stream
                    // This error can be ignored
                }
            }
        }

        /// <summary>
        /// Parses RDFa by extracting it from the HTML from the given input.
        /// </summary>
        /// <param name="handler">RDF Handler to use.</param>
        /// <param name="filename">File to read from.</param>
        public void Load(IRdfHandler handler, string filename)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");
            Load(handler, File.OpenText(filename));
        }

        /// <summary>
        /// Parses RDFa by extracting it from the HTML from the given input.
        /// </summary>
        /// <param name="handler">RDF Handler to use.</param>
        /// <param name="filename">File to read from.</param>
        /// <param name="uriFactory">URI Factory to use.</param>
        public void Load(IRdfHandler handler, string filename, IUriFactory uriFactory)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");
            Load(handler, File.OpenText(filename), uriFactory);
        }


        /// <summary>
        /// Parse the input stream as an HTML document.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected abstract THtmlDocument LoadAndParse(TextReader input);

        /// <summary>
        /// Determine if an element has a particular attribute.
        /// </summary>
        /// <param name="element">The element to check.</param>
        /// <param name="attributeName">The name of the attribute to check for.</param>
        /// <returns>True if the element has an attribute named <paramref name="attributeName"/>, false otherwise.</returns>
        protected abstract bool HasAttribute(TElement element, string attributeName);

        /// <summary>
        /// Get the value of a particular attribute of an element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="attributeName">The name of the attribute on the element.</param>
        /// <returns>The value of the attribute.</returns>
        protected abstract string GetAttribute(TElement element, string attributeName);

        /// <summary>
        /// Set the value of a particular attribute of an element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="attributeName">The name of the attribute to set/update.</param>
        /// <param name="value">The new value for the attribute.</param>
        protected abstract void SetAttribute(TElement element, string attributeName, string value);

        /// <summary>
        /// Get the base element of the specified document.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        protected abstract TElement GetBaseElement(THtmlDocument document);

        /// <summary>
        /// Determine if the HTML document can have an xml:base element.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        protected abstract bool IsXmlBaseIsPermissible(THtmlDocument document);

        /// <summary>
        /// Get the html element of the document.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        protected abstract TElement GetHtmlElement(THtmlDocument document);

        /// <summary>
        /// Process the content of an HTML document.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="evalContext"></param>
        protected abstract void ProcessDocument(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext);

        /// <summary>
        /// Get all attributes of an element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected abstract IEnumerable<TAttribute> GetAttributes(TElement element);

        /// <summary>
        /// Get the name of an attribute.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        protected abstract string GetAttributeName(TAttribute attribute);

        /// <summary>
        /// Get the value of an attribute.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        protected abstract string GetAttributeValue(TAttribute attribute);

        /// <summary>
        /// Get the name of an element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected abstract string GetElementName(TElement element);

        /// <summary>
        /// Return the children of an element (in order).
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected abstract IEnumerable<TNode> GetChildren(TElement element);

        /// <summary>
        /// Get the inner text of an element or a text node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected abstract string GetInnerText(TNode node);

        /// <summary>
        /// Get the HTML contained within an element as a string.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected abstract string GetInnerHtml(TElement element);

        /// <summary>
        /// Determine if an element has children.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>True if the element has children, false otherwise.</returns>
        protected abstract bool HasChildren(TElement element);

        /// <summary>
        /// Determine if a node in the parsed Html document tree is a text node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns>True if <paramref name="node"/> is a text node, false otherwise.</returns>
        protected abstract bool IsTextNode(TNode node);

        /// <summary>
        /// Determine if a node in the parsed HTML document tree is the root document node.
        /// </summary>
        /// <param name="node">The node to evaluate/</param>
        /// <returns>True if <paramref name="node"/> is a root node, false otherwise.</returns>
        protected abstract bool IsRoot(TNode node);

        protected abstract bool IsElement(TNode node);

        private void Parse(RdfAParserContext<THtmlDocument> context)
        {
            try
            {
                context.Handler.StartRdf();

                // Setup the basic evaluation context and start processing
                var evalContext = new RdfAEvaluationContext(_options?.Base ?? context.BaseUri);
                evalContext.NamespaceMap.AddNamespace(string.Empty, context.UriFactory.Create(XHtmlVocabNamespace));

                // Set the Default and Local Vocabulary
                context.DefaultVocabulary = new XHtmlRdfAVocabulary();
                evalContext.LocalVocabulary = new TermMappings();

                // If there's a base element this permanently changes the Base URI
                TElement baseEl = GetBaseElement(context.Document);
                if (baseEl != null)
                {
                    var uri = GetAttribute(baseEl, "href");
                    if (!string.IsNullOrEmpty(uri))
                    {
                        if (uri.Contains("?"))
                        {
                            evalContext.BaseUri = context.UriFactory.Create(uri.Substring(0, uri.IndexOf('?')));
                        }
                        else if (uri.Contains("#"))
                        {
                            evalContext.BaseUri = context.UriFactory.Create(uri.Substring(0, uri.IndexOf('#')));
                        }
                        else
                        {
                            evalContext.BaseUri = context.UriFactory.Create(GetAttribute(baseEl, "href"));
                        }
                    }
                }

                // Check whether xml:base is permissible
                context.XmlBaseAllowed = IsXmlBaseIsPermissible(context.Document);

                // Select the Syntax Version to use
                context.Syntax = _syntax;
                if (context.Syntax == RdfASyntax.AutoDetect || context.Syntax == RdfASyntax.AutoDetectLegacy)
                {
                    TElement docNode = GetHtmlElement(context.Document);
                    if (docNode != null && HasAttribute(docNode, "version"))
                    {
                        var version = GetAttribute(docNode, "version");
                        switch (version)
                        {
                            case XHtmlPlusRdfA10Version:
                            case HtmlPlusRdfA10Version:
                                context.Syntax = RdfASyntax.RDFa_1_0;
                                break;
                            case XHtmlPlusRdfA11Version:
                            case HtmlPlusRdfA11Version:
                                context.Syntax = RdfASyntax.RDFa_1_1;
                                break;
                            default:
                                if (context.Syntax == RdfASyntax.AutoDetect)
                                {
                                    context.Syntax = RdfASyntax.RDFa_1_1;
                                    OnWarning("The value '" + version + "' is not a known value for the @version attribute - assuming RDFa 1.1");
                                }
                                else
                                {
                                    context.Syntax = RdfASyntax.RDFa_1_0;
                                    OnWarning("The value '" + version + "' is not a known value for the @version attribute - assuming RDFa 1.0");
                                }
                                break;
                        }
                    }
                    else if (context.Syntax == RdfASyntax.AutoDetect)
                    {
                        context.Syntax = RdfASyntax.RDFa_1_1;
                        OnWarning("No @version attribute on document node - assuming RDFa 1.1");
                    }
                    else if (context.Syntax == RdfASyntax.AutoDetectLegacy)
                    {
                        context.Syntax = RdfASyntax.RDFa_1_0;
                        OnWarning("No @version attribute on document node - assuming RDFa 1.0");
                    }
                    else
                    {
                        context.Syntax = RdfASyntax.RDFa_1_1;
                        OnWarning("No @version attribute on document node - assuming RDFa 1.1");
                    }
                }

                ProcessDocument(context, evalContext);

                context.Handler.EndRdf(true);
            }
            catch (RdfParsingTerminatedException)
            {
                context.Handler.EndRdf(true);
                // Discard this - it justs means the Handler told us to stop
            }
            catch
            {
                context.Handler.EndRdf(false);
                throw;
            }
        }

        /// <summary>
        /// Process the content of an element of the document.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="evalContext"></param>
        /// <param name="currElement"></param>
        protected void ProcessElement(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext, TElement currElement)
        {
            bool recurse = true, skip = false;
            bool rel = false,
                 rev = false,
                 about = false,
                 src = false,
                 href = false,
                 inlist = false,
                 property = false,
                 type = false,
                 resource = false,
                 content = false,
                 datatype = false;
            var noDefaultNamespace = false;

            INode newSubj = null, currObj = null;
            var explicitNewSubj = false;
            var incomplete = new List<IncompleteTriple>();
            var inScopePrefixes = new List<string>();
            Dictionary<string, Uri> hiddenPrefixes = null;
            var lang = evalContext.Language;
            Uri oldBase = evalContext.BaseUri;
            var baseChanged = false;
            var oldLang = lang;
            var langChanged = false;
            var baseUri = (evalContext.BaseUri == null) ? string.Empty : evalContext.BaseUri.AbsoluteUri;
            Dictionary<INode, List<INode>> listMapping = evalContext.ListMapping;

            #region Steps 2-5 of the RDFa Processing Rules

            // Locate namespaces and other relevant attributes
            foreach (TAttribute attr in GetAttributes(currElement))
            {
                string uri;
                if (GetAttributeName(attr).StartsWith("xmlns:"))
                {
                    uri = Tools.ResolveUri(GetAttributeValue(attr), baseUri);
                    if (!(uri.EndsWith("/") || uri.EndsWith("#"))) uri += "#";
                    var prefix = GetAttributeName(attr).Substring(GetAttributeName(attr).IndexOf(':') + 1);
                    if (evalContext.NamespaceMap.HasNamespace(prefix))
                    {
                        if (hiddenPrefixes == null) hiddenPrefixes = new Dictionary<string, Uri>();
                        hiddenPrefixes.Add(prefix, evalContext.NamespaceMap.GetNamespaceUri(prefix));
                    }
                    evalContext.NamespaceMap.AddNamespace(prefix, context.UriFactory.Create(uri));
                    inScopePrefixes.Add(prefix);
                }
                else
                {
                    switch (GetAttributeName(attr))
                    {
                        case "xml:lang":
                        case "lang":
                            // @lang and @xml:lang have the same affect
                            if (!langChanged)
                            {
                                oldLang = lang;
                                lang = GetAttributeValue(attr);
                                langChanged = true;
                            }
                            break;
                        case "xml:base":
                            // @xml:base may be permitted in some cases
                            if (context.XmlBaseAllowed)
                            {
                                baseUri = Tools.ResolveUri(GetAttributeValue(attr), baseUri);
                                if (!(baseUri.EndsWith("/") || baseUri.EndsWith("#"))) baseUri += "#";
                                oldBase = evalContext.BaseUri;
                                baseChanged = true;
                                evalContext.BaseUri = context.UriFactory.Create(baseUri);
                                if (evalContext.ParentSubject == null ||
                                    (evalContext.ParentSubject is IUriNode parentUriNode &&
                                     parentUriNode.Uri.Equals(oldBase)))
                                {
                                    evalContext.ParentSubject = context.Handler.CreateUriNode(evalContext.BaseUri);
                                }
                            }
                            break;
                        case "xmlns":
                            // Can use @xmlns to override the default namespace
                            /*
                             * - Breaks test suite 0063, 0087 and others when processing XHTML documents
                            uri = GetAttributeValue(attr);
                            if (!(uri.EndsWith("/") || uri.EndsWith("#"))) uri += "#";
                            if (evalContext.NamespaceMap.HasNamespace(string.Empty))
                            {
                                if (hiddenPrefixes == null) hiddenPrefixes = new Dictionary<string, Uri>();
                                hiddenPrefixes.Add(string.Empty, evalContext.NamespaceMap.GetNamespaceUri(string.Empty));
                            }
                            evalContext.NamespaceMap.AddNamespace(string.Empty, context.UriFactory.Create(uri));
                            inScopePrefixes.Add(string.Empty);
                            noDefaultNamespace = true;
                            */
                            break;
                        case "prefix":
                            // Can use @prefix to set multiple namespaces with one attribute
                            if (context.Syntax == RdfASyntax.RDFa_1_0)
                            {
                                OnWarning("Cannot use the @prefix attribute to define prefixes in RDFa 1.0");
                            }
                            else
                            {
                                ParsePrefixAttribute(context, evalContext, attr, baseUri, ref hiddenPrefixes, inScopePrefixes);
                            }
                            break;
                        case "rel":
                            rel = true;
                            break;
                        case "rev":
                            rev = true;
                            break;
                        case "about":
                            about = true;
                            break;
                        case "src":
                            src = true;
                            break;
                        case "href":
                            href = true;
                            break;
                        case "inlist":
                            inlist = true;
                            break;
                        case "resource":
                            resource = true;
                            break;
                        case "typeof":
                            type = true;
                            break;
                        case "content":
                            content = true;
                            break;
                        case "datatype":
                            datatype = true;
                            break;
                        case "property":
                            property = true;
                            break;
                        case "profile":
                            if (context.Syntax == RdfASyntax.RDFa_1_0)
                            {
                                OnWarning("Cannot use the @profile attribute in RDFa 1.0");
                            }
                            else
                            {
                                if (ParseProfileAttribute(context, evalContext, attr))
                                {
                                    foreach (KeyValuePair<string, string> ns in evalContext.LocalVocabulary.Namespaces)
                                    {
                                        uri = Tools.ResolveUri(ns.Value, baseUri);
                                        // if (!(uri.EndsWith("/") || uri.EndsWith("#"))) uri += "#";
                                        if (evalContext.NamespaceMap.HasNamespace(ns.Key))
                                        {
                                            if (hiddenPrefixes == null) hiddenPrefixes = new Dictionary<string, Uri>();
                                            hiddenPrefixes.Add(ns.Key, evalContext.NamespaceMap.GetNamespaceUri(ns.Key));
                                        }
                                        evalContext.NamespaceMap.AddNamespace(ns.Key, context.UriFactory.Create(uri));
                                        inScopePrefixes.Add(ns.Key);
                                    }
                                }
                                else
                                {
                                    OnWarning("Unable to resolve a Profile document specified by the @profile attribute on the element <" + GetElementName(currElement) + "> - ignoring the DOM subtree of this element");
                                    return;
                                }
                            }
                            break;
                        case "vocab":
                            if (context.Syntax == RdfASyntax.RDFa_1_0)
                            {
                                OnWarning("Cannot use the @vocab attribute in RDFa 1.0");
                            }
                            else
                            {
                                ParseVocabAttribute(context, evalContext, attr);
                            }
                            break;
                    }
                }
            }

#endregion

#region Steps 6-7 of the RDFa Processing Rules

            // If we hit an invalid CURIE/URI at any point then ResolveUriOrCurie will return a null and 
            // later processing steps will be skipped for this element
            // Calls to Tools.ResolveUri which error will still cause the parser to halt
            if (!rel && !rev)
            {
                // No @rel or @rev attributes
                if (about && !GetAttribute(currElement, "about").Equals("[]"))
                {
                    // New Subject is the URI
                    newSubj = ResolveUriOrCurie(context, evalContext, GetAttribute(currElement, "about"));
                    explicitNewSubj = newSubj != null;
                }
                else if (src)
                {
                    // New Subject is the URI
                    newSubj = context.Handler.CreateUriNode(context.UriFactory.Create(Tools.ResolveUri(GetAttribute(currElement, "src"), baseUri)));
                    explicitNewSubj = newSubj != null;
                }
                else if (resource && !GetAttribute(currElement, "resource").Equals("[]"))
                {
                    // New Subject is the URI
                    newSubj = ResolveUriOrCurie(context, evalContext, GetAttribute(currElement, "resource"));
                    explicitNewSubj = newSubj != null;
                }
                else if (href)
                {
                    // New Subject is the URI
                    newSubj = context.Handler.CreateUriNode(context.UriFactory.Create(Tools.ResolveUri(GetAttribute(currElement, "href"), baseUri)));
                    explicitNewSubj = newSubj != null;
                }
                else if (IsRoot(currElement) || GetElementName(currElement).Equals("head") || GetElementName(currElement).Equals("body"))
                {
                    // New Subject is the Base URI
                    try
                    {
                        newSubj = context.Handler.CreateUriNode(context.UriFactory.Create(Tools.ResolveUri(string.Empty, baseUri)));
                        explicitNewSubj = newSubj != null;
                    }
                    catch (RdfException)
                    {
                        if (baseUri.Equals(string.Empty))
                        {
                            OnWarning("Unable to generate a valid Subject for a Triple since the Base URI should be used but there is no in-scope Base URI");
                            newSubj = null;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else if (type)
                {
                    // New Subject is a Blank Node
                    newSubj = context.Handler.CreateBlankNode();
                    explicitNewSubj = newSubj != null;
                }
                else if (evalContext.ParentObject != null)
                {
                    // New Subject is the Parent Object and will skip if no property attributes
                    newSubj = evalContext.ParentObject;
                    if (!property) skip = true;
                }
            }
            else
            {
                // A @rel or @rev attribute was encountered

                // For this we first set the Subject
                if (about && !GetAttribute(currElement, "about").Equals("[]"))
                {
                    // New Subject is the URI
                    newSubj = ResolveUriOrCurie(context, evalContext, GetAttribute(currElement, "about"));
                    explicitNewSubj = newSubj != null;
                }
                else if (src)
                {
                    // New Subject is the URI
                    newSubj = context.Handler.CreateUriNode(context.UriFactory.Create(Tools.ResolveUri(GetAttribute(currElement, "src"), baseUri)));
                    explicitNewSubj = newSubj != null;
                }
                else if (IsRoot(currElement) || GetElementName(currElement).Equals("head") || GetElementName(currElement).Equals("body"))
                {
                    // New Subject is the Base URI
                    try
                    {
                        newSubj = context.Handler.CreateUriNode(context.UriFactory.Create(Tools.ResolveUri(string.Empty, baseUri)));
                        explicitNewSubj = newSubj != null;
                    }
                    catch (RdfException)
                    {
                        if (baseUri.Equals(string.Empty))
                        {
                            OnWarning("Unable to generate a valid Subject for a Triple since the Base URI should be used but there is no in-scope Base URI");
                            newSubj = null;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else if (type)
                {
                    // New Subject is a Blank Node
                    newSubj = context.Handler.CreateBlankNode();
                    explicitNewSubj = newSubj != null;
                }
                else if (evalContext.ParentObject != null)
                {
                    // New Subject is the Parent Object and will skip if no property attributes
                    newSubj = evalContext.ParentObject;
                }

                // Then we set the Object as well
                if (resource && !GetAttribute(currElement, "resource").Equals("[]"))
                {
                    // New Object is the URI
                    currObj = ResolveUriOrCurie(context, evalContext, GetAttribute(currElement, "resource"));
                }
                else if (href)
                {
                    // New Object is the URI
                    currObj = context.Handler.CreateUriNode(context.UriFactory.Create(Tools.ResolveUri(GetAttribute(currElement, "href"), baseUri)));
                }
            }

            #endregion

            #region Step 8 of the RDFa Processing Rules

            // If the Subject is not a null then we'll generate type triples if there's any @typeof attributes
            if (newSubj != null && type)
            {
                INode rdfType = context.Handler.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
                foreach (INode dtObj in ParseComplexAttribute(context, evalContext, GetAttribute(currElement, "typeof")))
                {
                    if (!context.Handler.HandleTriple(new Triple(newSubj, rdfType, dtObj)))
                    {
                        ParserHelper.Stop();
                    }
                }
            }

            #endregion

            if (newSubj != null && !newSubj.Equals(evalContext.ParentSubject))
            {
                listMapping = new Dictionary<INode, List<INode>>();
            }

            #region Steps 9-10 of the RDFa Processing Rules

            // If the Object is not null we'll generate triples
            if (newSubj != null && currObj != null)
            {
                // We can generate some complete triples
                if (rel)
                {
                    foreach (INode pred in ParseComplexAttribute(context, evalContext, GetAttribute(currElement, "rel")))
                    {
                        if (inlist)
                        {
                            if (explicitNewSubj)
                            {
                                this.EmitList(context, newSubj, pred, new []{currObj});
                            }
                            else
                            {
                                if (!listMapping.ContainsKey(pred))
                                {
                                    listMapping[pred] = new List<INode>();
                                }

                                listMapping[pred].Add(currObj);
                            }
                        } 
                        else if (!context.Handler.HandleTriple(new Triple(newSubj, pred, currObj)))
                        {
                            ParserHelper.Stop();
                        }
                    }
                }
                if (rev)
                {
                    foreach (INode pred in ParseComplexAttribute(context, evalContext, GetAttribute(currElement, "rev")))
                    {
                        if (!context.Handler.HandleTriple(new Triple(currObj, pred, newSubj)))
                        {
                            ParserHelper.Stop();
                        }
                    }
                }
            }
            else
            {
                // We can generate some incomplete triples
                var preds = false;
                if (rel)
                {
                    foreach (INode pred in ParseComplexAttribute(context, evalContext, GetAttribute(currElement, "rel")))
                    {
                        preds = true;
                        if (inlist)
                        {
                            if (!listMapping.ContainsKey(pred))
                            {
                                listMapping[pred] = new List<INode>();
                            }
                            incomplete.Add(new IncompleteTriple(listMapping[pred], IncompleteTripleDirection.None));
                        }
                        else
                        {
                            incomplete.Add(new IncompleteTriple(pred, IncompleteTripleDirection.Forward));
                        }
                    }
                }
                if (rev)
                {
                    foreach (INode pred in ParseComplexAttribute(context, evalContext, GetAttribute(currElement, "rev")))
                    {
                        preds = true;
                        incomplete.Add(new IncompleteTriple(pred, IncompleteTripleDirection.Reverse));
                    }
                }

                if (preds)
                {
                    // Current Object becomes a Blank Node only if there were predicates
                    currObj = context.Handler.CreateBlankNode();
                }
            }

#endregion

#region Step 11 of the RDFa Processing Rules

            // Get the Current Object Literal
            if (newSubj != null && property)
            {
                // We only look for this if there is a property attribute
                INode currLiteral = null;
                Uri dt;
                INode dtNode = null;

                if (datatype && !GetAttribute(currElement, "datatype").Equals(string.Empty))
                {
                    // Some kind of Typed Literal
                    // Resolve the Datatype attribute into URIs
                    try
                    {
                        dtNode = ResolveTermOrCurieOrAbsUri(context, evalContext, GetAttribute(currElement, "datatype"));
                    }
                    catch (RdfException)
                    {
                        OnWarning("Unable to resolve a valid Datatype for the Literal since the value '" + GetAttribute(currElement, "datatype") + "' is not a valid CURIE or it cannot be resolved into a URI given the in-scope namespace prefixes and Base URI - assuming a Plain Literal instead");
                    }
                }
                if (dtNode != null)
                {
                    // We can only process this Triple if we were able to get a valid URI Node for the Datatype
                    if (dtNode.NodeType != NodeType.Uri) throw new RdfParseException("Cannot use a non-URI Node as a Dataype");
                    dt = ((IUriNode)dtNode).Uri;

                    if (!dt.AbsoluteUri.Equals(RdfSpecsHelper.RdfXmlLiteral))
                    {
                        // There's a Datatype and it's not XML Literal
                        if (content)
                        {
                            // Content attribute is used as the value
                            currLiteral = context.Handler.CreateLiteralNode(GetAttribute(currElement, "content"), dt);
                        }
                        else
                        {
                            // Value is concatenation of child text nodes
                            var lit = new StringBuilder();
                            foreach (TNode n in GetChildren(currElement))
                            {
                                GrabText(lit, n);
                            }
                            currLiteral = context.Handler.CreateLiteralNode(HttpUtility.HtmlDecode(lit.ToString()), dt);
                        }
                    }
                    else
                    {
                        // It's an explicitly declared XML Literal
                        foreach (TElement child in GetChildren(currElement).OfType<TElement>().Where(c=>!IsTextNode(c)))
                        {
                            ProcessXmlLiteral(evalContext, child, noDefaultNamespace);
                        }
                        currLiteral = context.Handler.CreateLiteralNode(GetInnerHtml(currElement), dt);
                    }
                }

                if (dtNode == null)
                {
                    // A Plain Literal
                    if (content)
                    {
                        // Content attribute is used as the value
                        currLiteral = String.IsNullOrEmpty(lang)
                            ? context.Handler.CreateLiteralNode(GetAttribute(currElement, "content"))
                            : context.Handler.CreateLiteralNode(GetAttribute(currElement, "content"), lang);
                    }
                    else if (!HasChildren(currElement))
                    {
                        // Value is content of the element (if any)
                        currLiteral = string.IsNullOrEmpty(lang)
                            ? context.Handler.CreateLiteralNode(HttpUtility.HtmlDecode(GetInnerText(currElement)))
                            : context.Handler.CreateLiteralNode(HttpUtility.HtmlDecode(GetInnerText(currElement)),
                                lang);
                    }
                    else if (GetChildren(currElement).All(IsTextNode))
                    {
                        currLiteral = ProcessPlainLiteral(context, currElement, lang);
                    }
                    else if (!datatype || (datatype && GetAttribute(currElement, "datatype").Equals(string.Empty)))
                    {
                        if (context.Syntax == RdfASyntax.RDFa_1_0)
                        {
                            // Value is an XML Literal
                            foreach (TElement child in GetChildren(currElement).OfType<TElement>()
                                         .Where(c => !IsTextNode(c)))
                            {
                                ProcessXmlLiteral(evalContext, child, noDefaultNamespace);
                            }

                            currLiteral = context.Handler.CreateLiteralNode(GetInnerHtml(currElement),
                                context.UriFactory.Create(RdfSpecsHelper.RdfXmlLiteral));
                        }
                        else
                        {
                            // Value is a plain literal
                            currLiteral = ProcessPlainLiteral(context, currElement, lang);
                        }
                    }
                }

                // Get the Properties which we are connecting this literal with
                if (currLiteral != null)
                {
                    foreach (INode predicateNode in ParseComplexAttribute(context, evalContext, GetAttribute(currElement, "property")))
                    {
                        if (predicateNode is IBlankNode)
                        {
                            OnWarning("Ignoring blank node predicate for " + newSubj.ToString());
                        }
                        else if (inlist)
                        {
                            if (!listMapping.ContainsKey(predicateNode))
                            {
                                listMapping[predicateNode] = new List<INode>();
                            }
                            listMapping[predicateNode].Add(currLiteral);
                        }
                        else if (!context.Handler.HandleTriple(new Triple(newSubj, predicateNode, currLiteral)))
                        {
                            ParserHelper.Stop();
                        }
                    }
                }
            }

#endregion

#region Step 12 of the RDFa Processing Rules

            // Complete incomplete Triples if this is possible
            if (!skip && newSubj != null && evalContext.ParentSubject != null)
            {
                foreach (IncompleteTriple i in evalContext.IncompleteTriples)
                {
                    switch (i.Direction)
                    {
                        case IncompleteTripleDirection.None:
                            i.List.Add(newSubj);
                            break;
                        case IncompleteTripleDirection.Forward:
                            {
                                if (!context.Handler.HandleTriple(new Triple(evalContext.ParentSubject, i.Predicate, newSubj)))
                                {
                                    ParserHelper.Stop();
                                }

                                break;
                            }
                        case IncompleteTripleDirection.Reverse:
                            {
                                if (!context.Handler.HandleTriple(new Triple(newSubj, i.Predicate, evalContext.ParentSubject)))
                                {
                                    ParserHelper.Stop();
                                }

                                break;
                            }
                    }
                }
            }

#endregion

#region Step 13 of the RDFa Processing Rules

            // Recurse if necessary
            if (recurse)
            {
                if (HasChildren(currElement))
                {
                    // Generate the new Evaluation Context (if applicable)
                    RdfAEvaluationContext newEvalContext;
                    if (skip)
                    {
                        newEvalContext = evalContext;
                        newEvalContext.Language = lang;
                    }
                    else
                    {
                        Uri newBase = (baseUri.Equals(string.Empty)) ? null : context.UriFactory.Create(baseUri);
                        newEvalContext = new RdfAEvaluationContext(newBase, evalContext.NamespaceMap);
                        // Set the Parent Subject for the new Context
                        if (newSubj != null)
                        {
                            newEvalContext.ParentSubject = newSubj;
                        }
                        else
                        {
                            newEvalContext.ParentSubject = evalContext.ParentSubject;
                        }
                        // Set the Parent Object for the new Context
                        if (currObj != null)
                        {
                            newEvalContext.ParentObject = currObj;
                        }
                        else if (newSubj != null)
                        {
                            newEvalContext.ParentObject = newSubj;
                        }
                        else
                        {
                            newEvalContext.ParentObject = evalContext.ParentSubject;
                        }

                        newEvalContext.ListMapping = listMapping;
                        newEvalContext.IncompleteTriples.AddRange(incomplete);
                        newEvalContext.Language = lang;
                    }

                    newEvalContext.LocalVocabulary = new TermMappings(evalContext.LocalVocabulary);

                    // Iterate over the Nodes
                    foreach (TElement element in GetChildren(currElement).OfType<TElement>().Where(IsElement))
                    {
                        ProcessElement(context, newEvalContext, element);
                    }
                }
            }

            #endregion
            #region Step 14 of RDFa Processing Rules

            foreach (KeyValuePair<INode, List<INode>> entry in listMapping)
            {
                if (evalContext.ListMapping.TryGetValue(entry.Key, out List<INode> ecList) && ecList.SequenceEqual(entry.Value))
                {
                    break;
                }
                if (entry.Value.Count == 0)
                {
                    EmitTriple(context, newSubj, entry.Key,
                        context.Handler.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfListNil)));
                }
                else
                {
                    EmitList(context, newSubj, entry.Key, entry.Value);
                }
            }
            #endregion
            // Now any in-scope prefixes go out of scope
            foreach (var prefix in inScopePrefixes)
            {
                evalContext.NamespaceMap.RemoveNamespace(prefix);
                // If they were hiding another prefix then that comes back into scope
                if (hiddenPrefixes != null)
                {
                    if (hiddenPrefixes.TryGetValue(prefix, out Uri hiddenPrefix))
                    {
                        evalContext.NamespaceMap.AddNamespace(prefix, hiddenPrefix);
                    }
                }
            }

            // And the Base URI resets if it was changed
            if (baseChanged)
            {
                evalContext.BaseUri = oldBase;
            }

            // And the Language resets if it was changed
            if (langChanged)
            {
                evalContext.Language = oldLang;
            }
        }

        private void EmitTriple(IParserContext context, INode subject, INode predicate, INode obj)
        {
            if (!context.Handler.HandleTriple(new Triple(subject, predicate, obj)))
            {
                ParserHelper.Stop();
            }
        }

        private void EmitList(IParserContext context, INode subject, INode predicate, IReadOnlyList<INode> listMembers)
        {
            INode nextNode = null;
            INode first = context.Handler.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfListFirst));
            INode rest = context.Handler.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfListRest));
            for (var i = listMembers.Count - 1; i >= 0; i--)
            {
                IBlankNode listNode = context.Handler.CreateBlankNode();
                EmitTriple(context, listNode, first, listMembers[i]);
                EmitTriple(context, listNode, rest, nextNode ?? context.Handler.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfListNil)));
                nextNode = listNode;
            }
            EmitTriple(context, subject, predicate, nextNode);
        }

        private INode ProcessPlainLiteral(IParserContext context, TElement element, string lang)
        {
            // Value is concatenation of all Text Child Nodes
            var lit = new StringBuilder();
            foreach (TNode n in GetChildren(element))
            {
                lit.Append(GetInnerText(n));
            }

            INode literalNode = string.IsNullOrEmpty(lang)
                ? context.Handler.CreateLiteralNode(HttpUtility.HtmlDecode(lit.ToString()))
                : context.Handler.CreateLiteralNode(HttpUtility.HtmlDecode(lit.ToString()), lang);
            return literalNode;
        }

        /// <summary>
        /// Resolves a CURIE to a Node.
        /// </summary>
        /// <param name="context">Parser Context.</param>
        /// <param name="evalContext">Evaluation Context.</param>
        /// <param name="curie">CURIE.</param>
        /// <returns></returns>
        private INode ResolveCurie(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext, string curie)
        {
            if (curie.StartsWith("_:"))
            {
                // The CURIE is for a Blank Node
                return context.Handler.CreateBlankNode(curie.Equals("_:") ? "_" : curie.Substring(2));
            }

            // CURIE is for a URI
            if (context.Syntax == RdfASyntax.RDFa_1_0)
            {
                // RDFa 1.0
                if (curie.StartsWith(":"))
                {
                    return context.Handler.CreateUriNode(context.UriFactory.Create(XHtmlVocabNamespace + curie.Substring(1)));
                }

                if (curie.Contains(":"))
                {
                    return context.Handler.CreateUriNode(
                        context.UriFactory.Create(
                            evalContext.LocalVocabulary.ResolveCurie(curie, evalContext.BaseUri)));
                    // return context.Handler.CreateUriNode(context.UriFactory.Create(Tools.ResolveQName(curie, evalContext.NamespaceMap, evalContext.BaseUri)));
                }

                throw new RdfParseException("The value '" + curie + "' is not valid as a CURIE as it does not have a prefix");
            }

            // RDFa 1.1
            var resolvedIri = ResolveQName(context, evalContext, curie);
            if (resolvedIri != null)
            {
                return context.Handler.CreateUriNode(context.UriFactory.Create(resolvedIri));
            }

            throw new RdfParseException($"Could not resolve the value '{curie}' as a CURIE.");
        }

        private string ResolveQName(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext,
            string curie)
        {
            var parts = curie.Split(new[] { ':' }, 2);
            if (parts.Length == 1) return evalContext.BaseUri + parts[0];
            if (evalContext.NamespaceMap.HasNamespace(parts[0]))
            {
                return evalContext.NamespaceMap.GetNamespaceUri(parts[0]).AbsoluteUri + parts[1];
            }

            if (context.DefaultVocabulary.NamespaceMap.HasNamespace(parts[0]))
            {
                return context.DefaultVocabulary.NamespaceMap.GetNamespaceUri(parts[0]).AbsoluteUri + parts[1];
            }

            return null;
        }
        /// <summary>
        /// Resolves an Attribute which may be a CURIE/URI to a Node.
        /// </summary>
        /// <param name="context">Parser Context.</param>
        /// <param name="evalContext">Evaluation Context.</param>
        /// <param name="uriref">URI/CURIE.</param>
        /// <returns></returns>
        private INode ResolveUriOrCurie(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext, string uriref)
        {
            try
            {
                if (uriref.StartsWith("["))
                {
                    // CURIE
                    var curie = uriref.Substring(1, uriref.Length - 2);
                    return ResolveCurie(context, evalContext, curie);
                }

                if (IsCurie(context, evalContext, uriref))
                {
                    // CURIE
                    return ResolveCurie(context, evalContext, uriref);
                }

                if (IsBlankNode(uriref))
                {
                    return context.Handler.CreateBlankNode(uriref.Substring(2));
                }

                // URI
                return context.Handler.CreateUriNode(context.UriFactory.Create(Tools.ResolveUri(uriref, evalContext.BaseUri.ToSafeString())));
            }
            catch (RdfException)
            {
                OnWarning("Unable to resolve a URI or CURIE since the value '" + uriref + "' does not contain a valid URI/CURIE or it cannot be resolved to a URI given the in-scope namespace prefixes and Base URI");
                return null;
            }
        }

        /// <summary>
        /// Resolves an Attribute which may be a Term/CURIE/URI to a Node where one/more of the values may be special values permissible in a complex attribute.
        /// </summary>
        /// <param name="context">Parser Context.</param>
        /// <param name="evalContext">Evaluation Context.</param>
        /// <param name="curie">URI/CURIE/Term.</param>
        /// <returns></returns>
        private INode ResolveTermOrCurie(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext, string curie)
        {
            if (context.Syntax == RdfASyntax.RDFa_1_0)
            {
                // RDFa 1.0
                var vocab = new XHtmlRdfAVocabulary();
                if (curie.StartsWith(":"))
                {
                    return context.Handler.CreateUriNode(context.UriFactory.Create(vocab.ResolveTerm(curie.Substring(1))));
                }

                if (curie.Contains(":"))
                {
                    return ResolveCurie(context, evalContext, curie);
                }

                if (vocab.HasTerm(curie))
                {
                    return context.Handler.CreateUriNode(context.UriFactory.Create(vocab.ResolveTerm(curie)));
                }

                throw new RdfParseException("Cannot use an unprefixed CURIE in RDFa 1.0 - only reserved XHTML terms are permitted");
            }

            // RDFa 1.1
            if (curie.StartsWith(":"))
            {
                if (evalContext.LocalVocabulary != null)
                {
                    if (evalContext.LocalVocabulary.HasTerm(curie.Substring(1)) || !evalContext.LocalVocabulary.VocabularyUri.Equals(string.Empty))
                    {
                        return context.Handler.CreateUriNode(context.UriFactory.Create(evalContext.LocalVocabulary.ResolveTerm(curie.Substring(1))));
                    }

                    if (context.DefaultVocabulary != null && context.DefaultVocabulary.HasTerm(curie.Substring(1)))
                    {
                        return context.Handler.CreateUriNode(context.UriFactory.Create(context.DefaultVocabulary.ResolveTerm(curie.Substring(1))));
                    }

                    return ResolveCurie(context, evalContext, curie);
                }

                if (context.DefaultVocabulary != null && context.DefaultVocabulary.HasTerm(curie.Substring(1)))
                {
                    return context.Handler.CreateUriNode(context.UriFactory.Create(context.DefaultVocabulary.ResolveTerm(curie.Substring(1))));
                }

                return ResolveCurie(context, evalContext, curie);
            }

            if (evalContext.LocalVocabulary != null)
            {
                if (evalContext.LocalVocabulary.HasTerm(curie) || !evalContext.LocalVocabulary.VocabularyUri.Equals(string.Empty))
                {
                    return context.Handler.CreateUriNode(context.UriFactory.Create(evalContext.LocalVocabulary.ResolveTerm(curie)));
                }

                if (context.DefaultVocabulary != null && context.DefaultVocabulary.HasTerm(curie))
                {
                    return context.Handler.CreateUriNode(context.UriFactory.Create(context.DefaultVocabulary.ResolveTerm(curie)));
                }

                throw new RdfParseException("Unable to resolve a Term since there is no appropriate Local/Default Vocabulary in scope");
            }

            if (context.DefaultVocabulary != null)
            {
                return context.Handler.CreateUriNode(context.UriFactory.Create(context.DefaultVocabulary.ResolveTerm(curie)));
            }

            throw new RdfParseException("Unable to resolve a Term since there is no appropriate Local/Default Vocabulary in scope");
        }

        private INode ResolveTermOrCurieOrAbsUri(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext, string value)
        {
            if (IsTerm(value))
            {
                return ResolveTermOrCurie(context, evalContext, value);
            }

            if (IsBlankNode(value))
            {
                return context.Handler.CreateBlankNode(value.Substring(2));
            }

            if (IsCurie(context, evalContext, value))
            {
                return ResolveCurie(context, evalContext, value);
            }

            try
            {
                Uri uri = context.UriFactory.Create(value);
                if (uri.IsAbsoluteUri) return context.Handler.CreateUriNode(uri);
            }
            catch (UriFormatException)
            {
                // Fall through to raise the RdfParseException
            }

            throw new RdfParseException("Unable to resolve the value '" + value + "' as a Term, CURIE or absolute URI");
        }

        /// <summary>
        /// Parses an complex attribute into a number of Nodes.
        /// </summary>
        /// <param name="context">Parser Context.</param>
        /// <param name="evalContext">Evaluation Context.</param>
        /// <param name="value">Attribute Value.</param>
        /// <returns></returns>
        /// <remarks>
        /// A complex attribute is any attribute which accepts multiple URIs, CURIEs or Terms.
        /// </remarks>
        private List<INode> ParseComplexAttribute(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext, string value)
        {
            var nodes = new List<INode>();
            if (string.IsNullOrWhiteSpace(value)) return nodes;

            var values = GetAttributeTokens(value).ToArray();
            foreach (var val in values)
            {
                try
                {
                    INode n = ResolveTermOrCurieOrAbsUri(context, evalContext, val);
                    nodes.Add(n);
                }
                catch
                {
                    // Errors are ignored, they don't produce a URI
                    // Raise a warning anyway
                    OnWarning("Ignoring the value '" + val + "' since this is not a valid Term/CURIE/URI or it cannot be resolved into a URI given the in-scope Namespace Prefixes");
                }
            }

            return nodes;
        }

        /*
        /// <summary>
        /// Parses an attribute into a number of Nodes from the CURIEs contained in the Attribute.
        /// </summary>
        /// <param name="context">Parser Context.</param>
        /// <param name="evalContext">Evaluation Context.</param>
        /// <param name="value">Attribute Value.</param>
        /// <returns></returns>
        private List<INode> ParseAttribute(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext, string value)
        {
            var nodes = new List<INode>();

            var values = GetAttributeTokens(value).ToArray();
            foreach (var val in values)
            {
                try
                {
                    INode n = ResolveCurie(context, evalContext, val);
                    nodes.Add(n);
                }
                catch
                {
                    // Errors are ignored, they don't produce a URI
                    // Raise a warning anyway
                    OnWarning("Ignoring the value '" + val + "' since this is not a valid CURIE or it cannot be resolved into a URI given the in-scope Namespace Prefixes and Base URI");
                }
            }

            return nodes;
        }
        */

        private IEnumerable<string> GetAttributeTokens(string value)
        {
            MatchCollection matches = _tokenListRegex.Matches(value);
            foreach (Match match in matches)
            {
                yield return match.Groups["token"].Value;
            }
        }
        private readonly Regex _tokenListRegex = new(@"(?<token>[^ \t\r\n]+)", RegexOptions.Compiled);

        private readonly Regex _prefixRegex = new(@"\s*(?<prefix>[^\s]*):\s+(?<url>[^\s]+)", RegexOptions.Compiled);
        private readonly Regex _curieRegex = new(@"(?<prefix>[^:]+):(?<suffix>[^\s]+)", RegexOptions.Compiled);

        private void ParsePrefixAttribute(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext, TAttribute attr, string baseUri, ref Dictionary<string, Uri> hiddenPrefixes, List<string> inScopePrefixes)
        {
            // Do nothing if the @prefix attribute is empty
            if (GetAttributeValue(attr).Equals(string.Empty)) return;
            var attrValue = GetAttributeValue(attr);
            MatchCollection matches = _prefixRegex.Matches(attrValue);
            foreach (Match match in matches)
            {
                var prefix = match.Groups["prefix"].Value;
                if (string.Empty.Equals(prefix))
                {
                    OnWarning("Ignoring empty prefix mapping in 'prefix' attribute.");
                    return;
                }
                var u = match.Groups["url"].Value;
                var uri = Tools.ResolveUri(u, baseUri);
                if (evalContext.NamespaceMap.HasNamespace(prefix))
                {
                    hiddenPrefixes ??= new Dictionary<string, Uri>();
                    hiddenPrefixes.Add(prefix, context.UriFactory.Create(uri));
                    evalContext.NamespaceMap.RemoveNamespace(prefix);
                }
                evalContext.NamespaceMap.AddNamespace(prefix, context.UriFactory.Create(uri));
                inScopePrefixes.Add(prefix);
            }

            if (matches.Count == 0)
            {
                if (!string.IsNullOrWhiteSpace(attrValue))
                {
                    OnWarning("Failed to parse prefix attribute: " + attrValue);
                }
            }
        }

        private IEnumerable<KeyValuePair<string, string>> GetPrefixMappings(RdfAParserContext<THtmlDocument> context, IGraph g)
        {
            var prefix = new UriNode(context.UriFactory.Create(RdfANamespace + "prefix"));
            var uri = new UriNode(context.UriFactory.Create(RdfANamespace + "uri"));
            return GetMappings(prefix, uri, g);
        }

        private IEnumerable<KeyValuePair<string, string>> GetTermMappings(RdfAParserContext<THtmlDocument> context,
            IGraph g)
        {
            var term = new UriNode(context.UriFactory.Create(RdfANamespace + "term"));
            var uri = new UriNode(context.UriFactory.Create(RdfANamespace + "uri"));
            return GetMappings(term, uri, g);
        }

        private IEnumerable<KeyValuePair<string, string>> GetMappings(INode keyPredicateNode, INode valuePredicateNode,
            IGraph graph)
        {
            IDictionary<INode, INode> prefixByMapNode = graph.GetTriplesWithPredicate(keyPredicateNode)
                .GroupBy(t => t.Subject)
                .Where(grouping => grouping.Count() == 1)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.First().Object);

            IDictionary<INode, INode> uriByMapNode = graph.GetTriplesWithPredicate(valuePredicateNode)
                .GroupBy(t => t.Subject)
                .Where(g => g.Count() == 1)
                .ToDictionary(g => g.Key, g => g.First().Object);

            foreach (INode mappingNode in prefixByMapNode.Keys.Intersect(uriByMapNode.Keys))
            {
                INode prefixNode = prefixByMapNode[mappingNode];
                INode nsNode = uriByMapNode[mappingNode];
                if (prefixNode is ILiteralNode prefixLiteralNode &&
                    nsNode is ILiteralNode nsListLiteralNode)
                {
                    yield return new KeyValuePair<string, string>(
                        prefixLiteralNode.Value.ToLower(),
                        nsListLiteralNode.Value);
                }
            }
        }

        private bool ParseProfileAttribute(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext, TAttribute attr)
        {
            var attrValue = GetAttributeValue(attr);
            var profiles = GetAttributeTokens(attrValue).ToArray();

            //var prefixQuery = "PREFIX rdfa: <" + RdfANamespace + "> SELECT SAMPLE(?prefix) AS ?NamespacePrefix SAMPLE(?uri) AS ?NamespaceURI WHERE { ?s rdfa:prefix ?prefix ; rdfa:uri ?uri } GROUP BY ?s HAVING (COUNT(?prefix) = 1 && COUNT(?uri) = 1)";
            //var termQuery = "PREFIX rdfa: <" + RdfANamespace + "> SELECT SAMPLE(?term) AS ?Term SAMPLE(?uri) AS ?URI WHERE {?s rdfa:term ?term ; rdfa:uri ?uri } GROUP BY ?s HAVING (COUNT(?term) = 1 && COUNT(?uri) = 1)";
            using var httpClient = new HttpClient();
            var loader = new Loader(httpClient);
            foreach (var profile in profiles)
            {
                try
                {
                    var g = new Graph();

                    if (profile.Equals(XHtmlVocabNamespace) || profile.Equals(XHtmlVocabNamespace.Substring(0, XHtmlVocabNamespace.Length - 1)))
                    {
                        // XHTML Vocabulary is a fixed vocabulary
                        evalContext.LocalVocabulary.Merge(new XHtmlRdfAVocabulary());
                    }
                    else
                    {
                        try
                        {
                            loader.LoadGraph(g, context.UriFactory.Create(profile));
                        }
                        catch
                        {
                            // If we fail then we return false which indicates that the DOM subtree is ignored
                            OnWarning("Unable to retrieve a Profile document which the library could parse from the URI '" + profile + "'");
                            return false;
                        }

                        // Namespace Mappings
                        foreach (KeyValuePair<string, string> mapping in GetPrefixMappings(context, g))
                        {
                            evalContext.LocalVocabulary.AddNamespace(mapping.Key, mapping.Value);
                        }
                        //var results = g.ExecuteQuery(prefixQuery);
                        //if (results is SparqlResultSet)
                        //{
                        //    var rset = (SparqlResultSet)results;
                        //    foreach (SparqlResult r in rset.Results)
                        //    {
                        //        INode prefixNode = r["NamespacePrefix"];
                        //        INode nsNode = r["NamespaceURI"];
                        //        if (prefixNode.NodeType == NodeType.Literal && nsNode.NodeType == NodeType.Literal)
                        //        {
                        //            var prefix = ((ILiteralNode)prefixNode).Value.ToLower();
                        //            var ns = ((ILiteralNode)nsNode).Value;
                        //            evalContext.LocalVocabulary.AddNamespace(prefix, ns);
                        //        }
                        //    }
                        //}

                        // Term Mappings
                        foreach (KeyValuePair<string, string> termMapping in GetTermMappings(context, g))
                        {
                            evalContext.LocalVocabulary.AddTerm(termMapping.Key, termMapping.Value);
                        }
                        //results = g.ExecuteQuery(termQuery);
                        //if (results is SparqlResultSet)
                        //{
                        //    var rset = (SparqlResultSet)results;
                        //    foreach (SparqlResult r in rset.Results)
                        //    {
                        //        INode termNode = r["Term"];
                        //        INode uriNode = r["URI"];
                        //        if (termNode.NodeType == NodeType.Literal && uriNode.NodeType == NodeType.Literal)
                        //        {
                        //            var term = ((ILiteralNode)termNode).Value;
                        //            var uri = ((ILiteralNode)uriNode).Value;
                        //            evalContext.LocalVocabulary.AddTerm(term, uri);
                        //        }
                        //    }
                        //}
                    }
                }
                catch
                {
                    // Ignore errors and continue processing
                    OnWarning("Ignoring the value '" + profile + "' since this is not a valid URI or a profile document was not successfully retrieved and parsed from this URI");
                    return false;
                }
            }

            return true;
        }

        private void ParseVocabAttribute(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext, TAttribute attr)
        {
            if (GetAttributeValue(attr).Equals(string.Empty))
            {
                // Reset Local Vocabulary
                evalContext.LocalVocabulary = new TermMappings(context.DefaultVocabulary);
            }
            else
            {
                evalContext.LocalVocabulary.VocabularyUri = GetAttributeValue(attr);
                // Record the use of the new vocabulary
                context.Handler.HandleTriple(new Triple(
                    context.Handler.CreateUriNode(evalContext.BaseUri),
                    context.Handler.CreateUriNode(context.UriFactory.Create(RdfANamespace + "usesVocabulary")),
                    context.Handler.CreateUriNode(context.UriFactory.Create(evalContext.LocalVocabulary.VocabularyUri))));
            }
        }

        /// <summary>
        /// Get the text content of a node and add it to the provided output buffer.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="node"></param>
        protected abstract void GrabText(StringBuilder output, TNode node);

        private readonly string[] _curieAttributes =
            new[] { "about", "resource", "datatype", "property", "typeof", "rel", "rev" };
        private void ProcessXmlLiteral(RdfAEvaluationContext evalContext, TElement n, bool noDefaultNamespace)
        {
            // Add Default Namespace as XHTML Namespace unless this would override an existing namespace
            if (!HasAttribute(n, "xmlns"))
            {
                if (!noDefaultNamespace) SetAttribute(n, "xmlns", XHtmlNamespace);
            }
            else
            {
                noDefaultNamespace = true;
            }

            // Add specific namespaces if necessary
            if (GetElementName(n).Contains(":"))
            {
                var prefix = GetElementName(n).Substring(0, GetElementName(n).IndexOf(':'));
                if (HasAttribute(n, "xmlns:" + prefix))
                {
                    // If the Node itself declares the Namespace then we don't need to do anything
                }
                else if (evalContext.NamespaceMap.HasNamespace(prefix))
                {
                    // If the Node doesn't declare the Namespace
                    SetAttribute(n, "xmlns:" + prefix, evalContext.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri);
                }
                else
                {
                    throw new RdfParseException("Malformed XML Literal - the undefined namespace prefix '" + prefix + "' is used");
                }
            }
            // Add Language (but don't override existing language)
            if (!evalContext.Language.Equals(string.Empty))
            {
                if (!HasAttribute(n, "xml:lang"))
                {
                    SetAttribute(n, "xml:lang", evalContext.Language);
                }
            }
            // Add namespaces from the current context
            foreach (var prefix in evalContext.NamespaceMap.Prefixes)
            {
                if (string.Empty.Equals(prefix)) continue;
                if (!HasAttribute(n, "xmlns:" + prefix))
                {
                    SetAttribute(n, "xmlns:"+ prefix, evalContext.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri);
                }
            }
            // Add namespaces for any CURIE-valued properties
            //foreach (var attrName in _curieAttributes)
            //{
            //    if (HasAttribute(n, attrName))
            //    {
            //        MatchCollection curieMatches = _curieRegex.Matches(GetAttribute(n, attrName));
            //        foreach (Match curieMatch in curieMatches)
            //        {
            //            var prefix = curieMatch.Groups["prefix"].Value;
            //            if (!string.IsNullOrEmpty(prefix) &&
            //                evalContext.NamespaceMap.HasNamespace(prefix) &&
            //                !HasAttribute(n, "xmlns:" + prefix))
            //            {
            //                SetAttribute(n, "xmlns:" + prefix, evalContext.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri);
            //            }
            //        }
            //    }
            //}

            // Recurse on any child nodes
            foreach (TElement child in GetChildren(n).OfType<TElement>().Where(c=>!IsTextNode(c)))
            {
                ProcessXmlLiteral(evalContext, child, noDefaultNamespace);
            }
        }

        private bool IsTerm(string value)
        {
            return XmlSpecsHelper.IsNCName(value);
        }

        private bool IsBlankNode(string value)
        {
            return value.StartsWith("_:");
        }

        private bool IsCurie(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext, string value)
        {
            if (value.StartsWith(":"))
            {
                var reference = value.Substring(1);
                return evalContext.NamespaceMap.HasNamespace(string.Empty) && IriSpecsHelper.IsIrelativeRef(reference);
            }
            else if (value.Contains(':'))
            {
                var prefix = value.Substring(0, value.IndexOf(':'));
                var reference = value.Substring(value.IndexOf(':') + 1);
                return (XmlSpecsHelper.IsNCName(prefix) || prefix.Equals("_")) && 
                       (evalContext.NamespaceMap.HasNamespace(prefix) || context.DefaultVocabulary.NamespaceMap.HasNamespace(prefix))  && 
                       IriSpecsHelper.IsIrelativeRef(reference);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Internal Helper for raising the Warning Event.
        /// </summary>
        /// <param name="message">Warning Message.</param>
        private void OnWarning(string message)
        {
            Warning?.Invoke(message);
        }

        /// <summary>
        /// Event which is raised when there is a non-fatal error with the input being read
        /// </summary>
        public event RdfReaderWarning Warning;
    }
}
