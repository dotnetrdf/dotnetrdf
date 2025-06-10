/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.Parsing;

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

    private readonly RdfAParserOptions _options = new () { Syntax = RdfASyntax.AutoDetectLegacy };
    private readonly Regex _tokenListRegex = new(@"(?<token>[^ \t\r\n]+)", RegexOptions.Compiled);
    private readonly Regex _prefixRegex = new(@"\s*(?<prefix>[^\s]*):\s+(?<url>[^\s]+)", RegexOptions.Compiled);


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
        _options.Syntax = syntax;
    }

    /// <summary>
    /// Creates a new RDFa Parser configured with the provided parser options.
    /// </summary>
    /// <param name="options">The parser options settings.</param>
    protected RdfAParserBase(RdfAParserOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
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
        if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
        if (input == null) throw new RdfParseException("Cannot read RDF from a null TextReader");
        uriFactory ??= UriFactory.Root;

        if (_options.PropertyCopyEnabled)
        {
            handler = new RdfAPatternCopyingHandler(handler);
        }
        try

        {
            THtmlDocument doc = LoadAndParse(input);

            var context =
                new RdfAParserContext<THtmlDocument>(handler, doc, uriFactory) { BaseUri = _options.Base ?? handler.BaseUri };

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
        Load(handler, filename, UriFactory.Root);
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
        if (_options.Base == null)
        {
            _options.Base = new Uri(Path.GetFullPath(filename));
        }
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
    /// <param name="node">The node to evaluate.</param>
    /// <returns>True if <paramref name="node"/> is a root node, false otherwise.</returns>
    protected abstract bool IsRoot(TNode node);

    /// <summary>
    /// Determine if a node in the parsed HTML document tree is an element node.
    /// </summary>
    /// <param name="node">The node to evaluate.</param>
    /// <returns>True if <paramref name="node"/> is an element node, false otherwise.</returns>
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
            context.DefaultContext = this._options?.DefaultContext ?? StaticRdfAContexts.XhtmlRdfAContext;
            evalContext.LocalContext = new RdfAContext();

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
            context.Syntax = _options?.Syntax ?? RdfASyntax.AutoDetectLegacy;
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
            // Discard this - it just means the Handler told us to stop
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
    /// <param name="currentElement"></param>
    protected void ProcessElement(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext, TElement currentElement)
    {
        bool recurse = true, skip = false;
        bool rel = false,
             rev = false,
             about = false,
             src = false,
             href = false,
             inList = false,
             property = false,
             type = false,
             resource = false,
             content = false,
             datatype = false,
             datetime = false;
        var noDefaultNamespace = false;

        INode newSubj = null, currentObj = null;
        var explicitNewSubj = false;
        var incomplete = new List<IncompleteTriple>();
        var inScopePrefixes = new List<string>();
        Dictionary<string, Uri> hiddenPrefixes = null;
        var lang = evalContext.Language;
        Uri oldBase = evalContext.BaseUri;
        var baseChanged = false;
        var oldLang = lang;
        var langChanged = false;
        var baseUri = evalContext.BaseUri == null ? string.Empty : evalContext.BaseUri.AbsoluteUri;
        Dictionary<INode, List<INode>> listMapping = evalContext.ListMapping;

        #region Steps 2-5 of the RDFa Processing Rules

        // Process xmlns attribute first.
        // These can be overridden by @prefix later
        foreach (TAttribute attr in GetAttributes(currentElement)
                     .Where(attr => GetAttributeName(attr).StartsWith("xmlns:")))
        {
            // Namespace URIs are resolved against the document URI, not any declared base
            var uri = Tools.ResolveUri(GetAttributeValue(attr), context.BaseUri?.AbsoluteUri ?? string.Empty);
            var prefix = GetAttributeName(attr).Substring(GetAttributeName(attr).IndexOf(':') + 1);
            if (evalContext.NamespaceMap.HasNamespace(prefix))
            {
                hiddenPrefixes ??= new Dictionary<string, Uri>();
                hiddenPrefixes.Add(prefix, evalContext.NamespaceMap.GetNamespaceUri(prefix));
            }
            evalContext.NamespaceMap.AddNamespace(prefix, context.UriFactory.Create(uri));
            inScopePrefixes.Add(prefix);
        }

        // Process other relevant attributes
        foreach (TAttribute attr in GetAttributes(currentElement))
        {
            if (GetAttributeName(attr).StartsWith("xmlns:"))
            {
                continue;
            }

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
                case "prefix":
                    // Can use @prefix to set multiple namespaces with one attribute
                    if (context.Syntax == RdfASyntax.RDFa_1_0)
                    {
                        OnWarning("Cannot use the @prefix attribute to define prefixes in RDFa 1.0");
                    }
                    else
                    {
                        ParsePrefixAttribute(context, evalContext, attr, context.BaseUri.AbsoluteUri, ref hiddenPrefixes, inScopePrefixes);
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
                    inList = true;
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
                case "datetime":
                    if (context.Syntax == RdfASyntax.RDFa_1_0)
                    {
                        OnWarning("Ignoring the @datetime attribute in RDFa 1.1");
                    }
                    else
                    {
                        datetime = true;
                    }
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
                            foreach (var prefix in evalContext.LocalContext.NamespaceMap.Prefixes)
                            {
                                var uri = Tools.ResolveUri(evalContext.NamespaceMap.GetNamespaceUri(prefix).ToString(), baseUri);
                                if (evalContext.NamespaceMap.HasNamespace(prefix))
                                {
                                    hiddenPrefixes ??= new Dictionary<string, Uri>();
                                    hiddenPrefixes.Add(prefix, evalContext.NamespaceMap.GetNamespaceUri(prefix));
                                }
                                evalContext.NamespaceMap.AddNamespace(prefix, context.UriFactory.Create(uri));
                                inScopePrefixes.Add(prefix);
                            }
                        }
                        else
                        {
                            OnWarning("Unable to resolve a Profile document specified by the @profile attribute on the element <" + GetElementName(currentElement) + "> - ignoring the DOM subtree of this element");
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

        #endregion

        // For HTML+RDF if @rel/@rev and @property are on the same element, ignore CURIE and URI values in @rel/rev and if they are then empty, ignore them
        if (property)
        {
            if (rel && !ParseComplexAttribute(context, evalContext, GetAttribute(currentElement, "rel"), true).Any())
            {
                rel = false;
            }
            if (rev && !ParseComplexAttribute(context, evalContext, GetAttribute(currentElement, "rev"), true).Any())
            {
                rev = false;
            }
        }
        #region Step 5 - 7

        INode typedResource = null;

        // If we hit an invalid CURIE/URI at any point then ResolveUriOrCurie will return a null and 
        // later processing steps will be skipped for this element
        // Calls to Tools.ResolveUri which error will still cause the parser to halt
        if (!rel && !rev)
        {
            #region Step 5

            if (property && !content && !datatype)
            {
                // 5.1
                if (about && !GetAttribute(currentElement, "about").Equals("[]"))
                {
                    newSubj = ResolveUriOrCurie(context, evalContext, GetAttribute(currentElement, "about"));
                    explicitNewSubj = newSubj != null;
                    typedResource = newSubj;
                }
                else if ((GetElementName(currentElement).Equals("head") || GetElementName(currentElement).Equals("body")) && evalContext.ParentObject != null)
                {
                    // HTML+RDFa 3.1
                    newSubj = evalContext.ParentObject;
                }
                else if (IsRoot(currentElement))
                {
                    newSubj = context.Handler.CreateUriNode(
                        context.UriFactory.Create(Tools.ResolveUri(string.Empty, baseUri)));
                    explicitNewSubj = newSubj != null;
                    typedResource = newSubj;
                }
                else if (evalContext.ParentObject != null)
                {
                    newSubj = evalContext.ParentObject;
                }

                if (type && typedResource == null)
                {
                    if (resource && !GetAttribute(currentElement, "resource").Equals("[]"))
                    {
                        typedResource = ResolveUriOrCurie(context, evalContext, GetAttribute(currentElement, "resource"));
                    } else if (href)
                    {
                        typedResource = context.Handler.CreateUriNode(context.UriFactory.Create(Tools.ResolveUri(GetAttribute(currentElement, "href"), baseUri)));
                    } else if (src)
                    {
                        typedResource = context.Handler.CreateUriNode(context.UriFactory.Create(Tools.ResolveUri(GetAttribute(currentElement, "src"), baseUri)));
                    }
                    else
                    {
                        typedResource = context.Handler.CreateBlankNode();
                    }

                    currentObj = typedResource;
                }
            }
            else
            {
                // 5.2
                if (about && !GetAttribute(currentElement, "about").Equals("[]"))
                {
                    // New Subject is the URI
                    newSubj = ResolveUriOrCurie(context, evalContext, GetAttribute(currentElement, "about"));
                    explicitNewSubj = newSubj != null;
                }
                else if (resource && !GetAttribute(currentElement, "resource").Equals("[]"))
                {
                    // New Subject is the URI
                    newSubj = ResolveUriOrCurie(context, evalContext, GetAttribute(currentElement, "resource"));
                    explicitNewSubj = newSubj != null;
                }
                else if (href)
                {
                    // New Subject is the URI
                    newSubj = context.Handler.CreateUriNode(context.UriFactory.Create(Tools.ResolveUri(GetAttribute(currentElement, "href"), baseUri)));
                    explicitNewSubj = newSubj != null;
                }
                else if (src)
                {
                    // New Subject is the URI
                    newSubj = context.Handler.CreateUriNode(
                        context.UriFactory.Create(Tools.ResolveUri(GetAttribute(currentElement, "src"), baseUri)));
                    explicitNewSubj = newSubj != null;
                }
                else if ((GetElementName(currentElement).Equals("head") || GetElementName(currentElement).Equals("body")) && evalContext.ParentObject != null)
                {
                    // HTML+RDFa 3.1
                    newSubj = evalContext.ParentObject;
                }
                else if (IsRoot(currentElement))
                {
                    if (!string.IsNullOrEmpty(baseUri))
                    {
                        newSubj = context.Handler.CreateUriNode(
                            context.UriFactory.Create(Tools.ResolveUri(string.Empty, baseUri)));
                        explicitNewSubj = newSubj != null;
                    }
                }
                else if (type)
                {
                    newSubj = context.Handler.CreateBlankNode();
                }
                else if (evalContext.ParentObject != null)
                {
                    newSubj = evalContext.ParentObject;
                    if (!property)
                    {
                        skip = true;
                    }
                }

                if (type)
                {
                    typedResource = newSubj;
                }
            }
            #endregion
        }
        else
        {
            // Step 6: A @rel or @rev attribute was encountered

            // For this we first set the Subject
            if (about && !GetAttribute(currentElement, "about").Equals("[]"))
            {
                // New Subject is the URI
                newSubj = ResolveUriOrCurie(context, evalContext, GetAttribute(currentElement, "about"));
                if (type) { typedResource = newSubj; }
                explicitNewSubj = newSubj != null;
            }
            else if ((GetElementName(currentElement).Equals("head") || GetElementName(currentElement).Equals("body")) && evalContext.ParentObject != null)
            {
                // HTML+RDFa 3.1
                newSubj = evalContext.ParentObject;
            }
            else if (IsRoot(currentElement))
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
            else if (evalContext.ParentObject != null)
            {
                // New Subject is the Parent Object and will skip if no property attributes
                newSubj = evalContext.ParentObject;
            }

            // Then we set the Object as well
            if (resource && !GetAttribute(currentElement, "resource").Equals("[]"))
            {
                // New Object is the URI
                currentObj = ResolveUriOrCurie(context, evalContext, GetAttribute(currentElement, "resource"));
            }
            else if (href)
            {
                // New Object is the URI
                currentObj = context.Handler.CreateUriNode(context.UriFactory.Create(Tools.ResolveUri(GetAttribute(currentElement, "href"), baseUri)));
            }
            else if (src)
            {
                currentObj = context.Handler.CreateUriNode(
                    context.Handler.UriFactory.Create(Tools.ResolveUri(GetAttribute(currentElement, "src"), baseUri)));
            }
            else if (type && !about)
            {
                currentObj = context.Handler.CreateBlankNode();
            }

            // Set the typed resource
            if (type && !about)
            {
                typedResource = currentObj;
            }
        }

        #endregion

        #region Step 7

        // If the Subject is not a null then we'll generate type triples if there's any @typeof attributes
        if (type && typedResource != null)
        {
            INode rdfType = context.Handler.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
            foreach (INode dtObj in ParseComplexAttribute(context, evalContext, GetAttribute(currentElement, "typeof"), false))
            {
                if (!context.Handler.HandleTriple(new Triple(typedResource, rdfType, dtObj)))
                {
                    ParserHelper.Stop();
                }
            }
        }

        #endregion

        #region Step 8
        if (newSubj != null && !newSubj.Equals(evalContext.ParentSubject))
        {
            listMapping = new Dictionary<INode, List<INode>>();
        }
        #endregion

        #region Steps 9-10 of the RDFa Processing Rules

        // If the Object is not null we'll generate triples
        if (newSubj != null && currentObj != null)
        {
            // We can generate some complete triples
            if (rel)
            {
                foreach (INode predicateNode in ParseComplexAttribute(context, evalContext, GetAttribute(currentElement, "rel"), property))
                {
                    if (inList)
                    {
                        if (explicitNewSubj)
                        {
                            EmitList(context, newSubj, predicateNode, new []{currentObj});
                        }
                        else
                        {
                            if (!listMapping.ContainsKey(predicateNode))
                            {
                                listMapping[predicateNode] = new List<INode>();
                            }

                            listMapping[predicateNode].Add(currentObj);
                        }
                    }
                    else if (!context.Handler.HandleTriple(new Triple(newSubj, predicateNode, currentObj)))
                    {
                        ParserHelper.Stop();
                    }
                }
            }
            if (rev)
            {
                foreach (INode predicateNode in ParseComplexAttribute(context, evalContext, GetAttribute(currentElement, "rev"), property))
                {
                    if (!context.Handler.HandleTriple(new Triple(currentObj, predicateNode, newSubj)))
                    {
                        ParserHelper.Stop();
                    }
                }
            }
        }
        else
        {
            // We can generate some incomplete triples
            var hasPredicates = false;
            if (rel)
            {
                foreach (INode predicateNode in ParseComplexAttribute(context, evalContext, GetAttribute(currentElement, "rel"), property))
                {
                    hasPredicates = true;
                    if (inList)
                    {
                        if (!listMapping.ContainsKey(predicateNode))
                        {
                            listMapping[predicateNode] = new List<INode>();
                        }
                        incomplete.Add(new IncompleteTriple(listMapping[predicateNode], IncompleteTripleDirection.None));
                    }
                    else
                    {
                        incomplete.Add(new IncompleteTriple(predicateNode, IncompleteTripleDirection.Forward));
                    }
                }
            }
            if (rev)
            {
                foreach (INode predicateNode in ParseComplexAttribute(context, evalContext, GetAttribute(currentElement, "rev"), property))
                {
                    hasPredicates = true;
                    incomplete.Add(new IncompleteTriple(predicateNode, IncompleteTripleDirection.Reverse));
                }
            }

            if (hasPredicates)
            {
                // Current Object becomes a Blank Node only if there were predicates
                currentObj = context.Handler.CreateBlankNode();
            }
        }

        #endregion

        #region Step 11

        if (newSubj != null && property)
        {
            INode currentPropertyValue = null;
            if (datatype)
            {
                var datatypeValue = GetAttribute(currentElement, "datatype");
                if (string.Empty.Equals(datatypeValue))
                {
                    currentPropertyValue = ProcessLiteral(context, currentElement, content, datetime, null, lang);
                }
                else
                {
                    try
                    {
                        INode dt = ResolveTermOrCurieOrAbsUri(context, evalContext, datatypeValue);
                        if (dt is not UriNode uriNode)
                        {
                            throw new RdfException("Expected @datatype to resolve to a URI.");
                        }

                        if (uriNode.Uri.AbsoluteUri.Equals(RdfSpecsHelper.RdfXmlLiteral))
                        {
                            // It's an explicitly declared XML Literal
                            foreach (TElement child in GetChildren(currentElement).OfType<TElement>()
                                         .Where(c => !IsTextNode(c)))
                            {
                                ProcessXmlLiteral(evalContext, child, noDefaultNamespace);
                            }

                            currentPropertyValue = context.Handler.CreateLiteralNode(GetInnerHtml(currentElement),
                                context.Handler.UriFactory.Create(RdfSpecsHelper.RdfXmlLiteral));
                        }
                        else
                        {
                            currentPropertyValue = ProcessLiteral(context, currentElement, content, datetime,
                                uriNode.Uri, lang);
                        }
                    }
                    catch (RdfException)
                    {
                        OnWarning("Unable to resolve a valid Datatype for the Literal since the value '" +
                                  GetAttribute(currentElement, "datatype") +
                                  "' is not a valid CURIE or it cannot be resolved into a URI given the in-scope namespace prefixes and Base URI - assuming a Plain Literal instead");
                        currentPropertyValue = ProcessPlainLiteral(context, currentElement, lang);
                    }
                }
            }
            else if (content)
            {
                currentPropertyValue = CreateLiteralNode(context, GetAttribute(currentElement, "content"), null, lang);
            }
            else if (datetime)
            {
                currentPropertyValue = ProcessDateTimeLiteral(context, GetAttribute(currentElement, "datetime"), null, lang);
            }
            else if (GetElementName(currentElement) == "time" && context.Syntax != RdfASyntax.RDFa_1_0)
            {
                currentPropertyValue = ProcessDateTimeLiteral(context, GetInnerText(currentElement),null, lang);
            }
            else if (!(rel || rev || content) && (resource || href || src))
            {
                if (resource && !GetAttribute(currentElement, "resource").Equals("[]"))
                {
                    currentPropertyValue = ResolveUriOrCurie(context, evalContext, GetAttribute(currentElement, "resource"));
                }
                else if (href)
                {
                    currentPropertyValue = ResolveUriOrCurie(context, evalContext, GetAttribute(currentElement, "href"));
                }
                else if (src)
                {
                    currentPropertyValue = ResolveUriOrCurie(context, evalContext, GetAttribute(currentElement, "src"));
                }
            }
            else if (type && !about)
            {
                currentPropertyValue = typedResource;
            }
            else
            {
                currentPropertyValue = ProcessPlainLiteral(context, currentElement, lang);
            }
            // Get the Properties which we are connecting this literal with
            if (currentPropertyValue != null)
            {
                foreach (INode predicateNode in ParseComplexAttribute(context, evalContext, GetAttribute(currentElement, "property"), false))
                {
                    if (predicateNode is IBlankNode)
                    {
                        OnWarning("Ignoring blank node predicate for " + newSubj.ToString());
                    }
                    else if (inList)
                    {
                        if (!listMapping.ContainsKey(predicateNode))
                        {
                            listMapping[predicateNode] = new List<INode>();
                        }
                        listMapping[predicateNode].Add(currentPropertyValue);
                    }
                    else if (!context.Handler.HandleTriple(new Triple(newSubj, predicateNode, currentPropertyValue)))
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
            if (HasChildren(currentElement))
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
                    Uri newBase = baseUri.Equals(string.Empty) ? null : context.UriFactory.Create(baseUri);
                    newEvalContext = new RdfAEvaluationContext(newBase, evalContext.NamespaceMap)
                    {
                        // Set the Parent Subject for the new Context
                        ParentSubject = newSubj ?? evalContext.ParentSubject,
                    };

                    // Set the Parent Object for the new Context
                    if (currentObj != null)
                    {
                        newEvalContext.ParentObject = currentObj;
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

                newEvalContext.LocalContext = new RdfAContext(evalContext.LocalContext);

                // Iterate over the Nodes
                foreach (TElement element in GetChildren(currentElement).OfType<TElement>().Where(IsElement))
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

    private static void EmitTriple(IParserContext context, INode subject, INode predicate, INode obj)
    {
        if (!context.Handler.HandleTriple(new Triple(subject, predicate, obj)))
        {
            ParserHelper.Stop();
        }
    }

    private static void EmitList(IParserContext context, INode subject, INode predicate, IReadOnlyList<INode> listMembers)
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

    private static INode ProcessDateTimeLiteral(IParserContext context, string literalValue, Uri dt, string lang)
    {
        if (dt == null)
        {
            // Attempt to guess datatype from literal value
            if (XmlSpecsHelper.XmlSchemaDurationRegex.IsMatch(literalValue))
            {
                dt = context.Handler.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDuration);
            }
            else if (XmlSpecsHelper.XmlSchemaDateTimeRegex.IsMatch(literalValue))
            {
                dt = context.Handler.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDateTime);
            }
            else if (XmlSpecsHelper.XmlSchemaDateRegex.IsMatch(literalValue))
            {
                dt = context.Handler.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate);
            }
            else if (XmlSpecsHelper.XmlSchemaTimeRegex.IsMatch(literalValue))
            {
                dt = context.Handler.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeTime);
            }
            else if (XmlSpecsHelper.XmlSchemaYearMonthRegex.IsMatch(literalValue))
            {
                dt = context.Handler.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeYearMonth);
            }
            else if (XmlSpecsHelper.XmlSchemaYearRegex.IsMatch(literalValue))
            {
                dt = context.Handler.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeYear);
            }
        }

        if (dt != null)
        {
            return context.Handler.CreateLiteralNode(literalValue, dt);
        }
        return string.IsNullOrEmpty(lang) ? context.Handler.CreateLiteralNode(literalValue) : context.Handler.CreateLiteralNode(literalValue, lang);
    }

    private INode ProcessLiteral(RdfAParserContext<THtmlDocument> context, TElement element, bool content, bool datetime, Uri dt, string lang)
    {
        if (content)
        {
            return CreateLiteralNode(context,GetAttribute(element, "content"), dt, lang);
        }

        if (datetime)
        {
            return ProcessDateTimeLiteral(context, GetAttribute(element, "datetime"), dt, lang);
        }

        if (GetElementName(element) == "time" && context.Syntax != RdfASyntax.RDFa_1_0)
        {
            return ProcessDateTimeLiteral(context, GetInnerText(element), dt, lang);
        }
        return CreateLiteralNode(context, GetInnerText(element), dt, lang);
    }

    private static INode CreateLiteralNode(IParserContext context, string literalValue, Uri dt, string lang)
    {
        literalValue = HttpUtility.HtmlDecode(literalValue);
        if (dt != null)
        {
            return context.Handler.CreateLiteralNode(literalValue, dt);
        }

        if (!string.IsNullOrEmpty(lang))
        {
            return context.Handler.CreateLiteralNode(literalValue, lang);
        }

        return context.Handler.CreateLiteralNode(literalValue);
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
    private static INode ResolveCurie(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext, string curie)
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
                var resolved = ResolveQName(context, evalContext, curie);
                if (resolved != null)
                {
                    return context.Handler.CreateUriNode(context.UriFactory.Create(resolved));
                }

                throw new RdfParseException($"Could not resolve the value '{curie}' as a CURIE.");
                //return context.Handler.CreateUriNode(
                //    context.UriFactory.Create(
                //        evalContext.LocalContext.ResolveCurie(curie, evalContext.BaseUri)));
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

    private static string ResolveQName(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext,
        string curie)
    {
        var parts = curie.Split(new[] { ':' }, 2);
        if (parts.Length == 1) return evalContext.BaseUri + parts[0];
        if (evalContext.NamespaceMap.HasNamespace(parts[0]))
        {
            return evalContext.NamespaceMap.GetNamespaceUri(parts[0]).AbsoluteUri + parts[1];
        }

        if (context.DefaultContext.NamespaceMap.HasNamespace(parts[0]))
        {
            return context.DefaultContext.NamespaceMap.GetNamespaceUri(parts[0]).AbsoluteUri + parts[1];
        }

        return null;
    }

    /// <summary>
    /// Resolves an Attribute which may be a CURIE/URI to a Node.
    /// </summary>
    /// <param name="context">Parser Context.</param>
    /// <param name="evalContext">Evaluation Context.</param>
    /// <param name="uriRef">URI/CURIE.</param>
    /// <returns></returns>
    private INode ResolveUriOrCurie(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext, string uriRef)
    {
        try
        {
            if (uriRef.StartsWith("["))
            {
                // CURIE
                var curie = uriRef.Substring(1, uriRef.Length - 2);
                return ResolveCurie(context, evalContext, curie);
            }

            if (IsCurie(context, evalContext, uriRef))
            {
                // CURIE
                return ResolveCurie(context, evalContext, uriRef);
            }

            if (IsBlankNode(uriRef))
            {
                return context.Handler.CreateBlankNode(uriRef.Substring(2));
            }

            // URI
            return context.Handler.CreateUriNode(context.UriFactory.Create(Tools.ResolveUri(uriRef, evalContext.BaseUri.ToSafeString())));
        }
        catch (RdfException)
        {
            OnWarning("Unable to resolve a URI or CURIE since the value '" + uriRef + "' does not contain a valid URI/CURIE or it cannot be resolved to a URI given the in-scope namespace prefixes and Base URI");
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
    private static INode ResolveTermOrCurie(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext, string curie)
    {
        if (context.Syntax == RdfASyntax.RDFa_1_0)
        {
            // RDFa 1.0
            if (curie.StartsWith(":"))
            {
                return context.Handler.CreateUriNode(context.UriFactory.Create(context.DefaultContext.ResolveTerm(curie.Substring(1))));
            }

            if (curie.Contains(":"))
            {
                return ResolveCurie(context, evalContext, curie);
            }

            if (context.DefaultContext.HasTerm(curie))
            {
                return context.Handler.CreateUriNode(context.UriFactory.Create(context.DefaultContext.ResolveTerm(curie)));
            }

            throw new RdfParseException("Cannot use an unprefixed CURIE in RDFa 1.0 - only reserved XHTML terms are permitted");
        }

        // RDFa 1.1
        if (curie.StartsWith(":"))
        {
            if (evalContext.LocalContext != null)
            {
                if (evalContext.LocalContext.HasTerm(curie.Substring(1)) || !evalContext.LocalContext.VocabularyUri.Equals(string.Empty))
                {
                    return context.Handler.CreateUriNode(context.UriFactory.Create(evalContext.LocalContext.ResolveTerm(curie.Substring(1))));
                }

                if (context.DefaultContext != null && context.DefaultContext.HasTerm(curie.Substring(1)))
                {
                    return context.Handler.CreateUriNode(context.UriFactory.Create(context.DefaultContext.ResolveTerm(curie.Substring(1))));
                }

                return ResolveCurie(context, evalContext, curie);
            }

            if (context.DefaultContext != null && context.DefaultContext.HasTerm(curie.Substring(1)))
            {
                return context.Handler.CreateUriNode(context.UriFactory.Create(context.DefaultContext.ResolveTerm(curie.Substring(1))));
            }

            return ResolveCurie(context, evalContext, curie);
        }

        if (evalContext.LocalContext != null)
        {
            if (evalContext.LocalContext.HasTerm(curie) || !evalContext.LocalContext.VocabularyUri.Equals(string.Empty))
            {
                return context.Handler.CreateUriNode(context.UriFactory.Create(evalContext.LocalContext.ResolveTerm(curie)));
            }

            if (context.DefaultContext != null && context.DefaultContext.HasTerm(curie))
            {
                return context.Handler.CreateUriNode(context.UriFactory.Create(context.DefaultContext.ResolveTerm(curie)));
            }

            throw new RdfParseException("Unable to resolve a Term since there is no appropriate Local/Default Vocabulary in scope");
        }

        if (context.DefaultContext != null)
        {
            return context.Handler.CreateUriNode(context.UriFactory.Create(context.DefaultContext.ResolveTerm(curie)));
        }

        throw new RdfParseException("Unable to resolve a Term since there is no appropriate Local/Default Vocabulary in scope");
    }

    private static INode ResolveTermOrCurieOrAbsUri(RdfAParserContext<THtmlDocument> context,
        RdfAEvaluationContext evalContext, string value)
    {
        return IsTerm(value) ? ResolveTermOrCurie(context, evalContext, value) : ResolveCurieOrAbsUri(context, evalContext, value);
    }

    private static INode ResolveCurieOrAbsUri(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext, string value)
    {
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
    /// <param name="skipTerms">Return only the results of processing terms in the attribute value.</param>
    /// <returns></returns>
    /// <remarks>
    /// A complex attribute is any attribute which accepts multiple URIs, CURIEs or Terms.
    /// </remarks>
    private List<INode> ParseComplexAttribute(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext, string value, bool skipTerms)
    {
        var nodes = new List<INode>();
        if (string.IsNullOrWhiteSpace(value)) return nodes;

        var values = GetAttributeTokens(value).ToArray();
        foreach (var val in values)
        {
            try
            {

                INode n = skipTerms ? ResolveCurieOrAbsUri(context, evalContext, val) : ResolveTermOrCurieOrAbsUri(context, evalContext, val);
                nodes.Add(n);
            }
            catch
            {
                // Errors are ignored, they don't produce a URI
                // Raise a warning anyway
                OnWarning(skipTerms
                    ? $"Ignoring the value '{val}' since this is not a valid CURIE/URI or it cannot be resolve into a URI given the in-scope namespace prefixes."
                    : $"Ignoring the value '{val}' since this is not a valid Term/CURIE/URI or it cannot be resolved into a URI given the in-scope Namespace Prefixes");
            }
        }

        return nodes;
    }

    private IEnumerable<string> GetAttributeTokens(string value)
    {
        MatchCollection matches = _tokenListRegex.Matches(value);
        foreach (Match match in matches)
        {
            yield return match.Groups["token"].Value;
        }
    }

    private void ParsePrefixAttribute(IParserContext context, RdfAEvaluationContext evalContext, TAttribute attr, string baseUri, ref Dictionary<string, Uri> hiddenPrefixes, ICollection<string> inScopePrefixes)
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
                if (!hiddenPrefixes.ContainsKey(prefix))
                {
                    // If hiddenPrefixes already has a mapping, leave it intact as it records the original prefix mapping before processing xmlns: attributes on this element
                    hiddenPrefixes.Add(prefix, evalContext.NamespaceMap.GetNamespaceUri(prefix));
                }
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

    private bool ParseProfileAttribute(IParserContext context, RdfAEvaluationContext evalContext, TAttribute attr)
    {
        var attrValue = GetAttributeValue(attr);
        var profiles = GetAttributeTokens(attrValue).ToArray();
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
                    evalContext.LocalContext.Merge(StaticRdfAContexts.XhtmlRdfAContext);
                }
                else
                {
                    try
                    {
                        loader.LoadGraph(g, context.UriFactory.Create(profile));
                        IRdfAContext profileContext = RdfAContext.Load(g);
                        evalContext.LocalContext.Merge(profileContext);
                    }
                    catch
                    {
                        // If we fail then we return false which indicates that the DOM subtree is ignored
                        OnWarning("Unable to retrieve a Profile document which the library could parse from the URI '" + profile + "'");
                        return false;
                    }
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
            evalContext.LocalContext = new RdfAContext(context.DefaultContext);
        }
        else
        {
            evalContext.LocalContext.VocabularyUri = GetAttributeValue(attr);
            // Record the use of the new vocabulary
            context.Handler.HandleTriple(new Triple(
                context.Handler.CreateUriNode(evalContext.BaseUri),
                context.Handler.CreateUriNode(context.UriFactory.Create(RdfANamespace + "usesVocabulary")),
                context.Handler.CreateUriNode(context.UriFactory.Create(evalContext.LocalContext.VocabularyUri))));
        }
    }

    /// <summary>
    /// Get the text content of a node and add it to the provided output buffer.
    /// </summary>
    /// <param name="output"></param>
    /// <param name="node"></param>
    protected abstract void GrabText(StringBuilder output, TNode node);

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

        // Recurse on any child nodes
        foreach (TElement child in GetChildren(n).OfType<TElement>().Where(c=>!IsTextNode(c)))
        {
            ProcessXmlLiteral(evalContext, child, noDefaultNamespace);
        }
    }

    private static bool IsTerm(string value)
    {
        return XmlSpecsHelper.IsNCName(value);
    }

    private static bool IsBlankNode(string value)
    {
        return value.StartsWith("_:");
    }

    private static bool IsCurie(RdfAParserContext<THtmlDocument> context, RdfAEvaluationContext evalContext, string value)
    {
        if (value.StartsWith(":"))
        {
            var reference = value.Substring(1);
            return evalContext.NamespaceMap.HasNamespace(string.Empty) && IriSpecsHelper.IsIrelativeRef(reference);
        }

        if (value.Contains(':'))
        {
            var prefix = value.Substring(0, value.IndexOf(':'));
            var reference = value.Substring(value.IndexOf(':') + 1);
            return (XmlSpecsHelper.IsNCName(prefix) || prefix.Equals("_")) &&
                   (evalContext.NamespaceMap.HasNamespace(prefix) || context.DefaultContext.NamespaceMap.HasNamespace(prefix))  && 
                   (context.Syntax != RdfASyntax.RDFa_1_0 || IriSpecsHelper.IsIrelativeRef(reference));
        }

        return false;
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
