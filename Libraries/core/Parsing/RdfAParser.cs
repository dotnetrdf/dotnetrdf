/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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
using System.Text;
using System.Text.RegularExpressions;
#if !NO_WEB
using System.Web;
#endif
using HtmlAgilityPack;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

//SLT: Find alternative to HttpUtility.DecodeHtml for Silverlight

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Possible RDFa Syntaxes
    /// </summary>
    public enum RdfASyntax
    {
        /// <summary>
        /// RDFa 1.0
        /// </summary>
        RDFa_1_0,
        /// <summary>
        /// RDFa 1.1
        /// </summary>
        RDFa_1_1,
        /// <summary>
        /// Auto-detect - assumes RDFa 1.1
        /// </summary>
        AutoDetect,
        /// <summary>
        /// Auto-detect - assumes RDFa 1.0
        /// </summary>
        AutoDetectLegacy
    }

    /// <summary>
    /// Class for reading RDF embedded as RDFa from within HTML web pages
    /// </summary>
    /// <remarks>
    /// <para>
    /// The RDFa parser uses a HTML parser (<a href="http://www.codeplex.com/htmlagilitypack">Html Agility Pack</a>) that is highly tolerant of real-world HTML and so is able to extract RDFa from pages that are not strictly valid HTML/XHTML
    /// </para>
    /// </remarks>
    public class RdfAParser : IRdfReader
    {
        /// <summary>
        /// XHTML Vocab Namespace
        /// </summary>
        public const String XHtmlVocabNamespace = "http://www.w3.org/1999/xhtml/vocab#";
        /// <summary>
        /// URI for the XHTML+RDFa DTD
        /// </summary>
        public const String XHtmlPlusRdfADoctype = "http://www.w3.org/MarkUp/DTD/xhtml-rdfa-1.dtd";
        /// <summary>
        /// Namespace URI for XHTML
        /// </summary>
        public const String XHtmlNamespace = "http://www.w3.org/1999/xhtml#";
        /// <summary>
        /// Namespace URI for RDFa
        /// </summary>
        public const String RdfANamespace = "http://www.w3.org/ns/rdfa#";

        /// <summary>
        /// RDFa Version Constants
        /// </summary>
        public const String XHtmlPlusRdfA11Version = "XHTML+RDFa 1.1",
                             HtmlPlusRdfA11Version = "HTML+RDFa 1.1",
                             XHtmlPlusRdfA10Version = "XHTML+RDFa 1.0",
                             HtmlPlusRdfA10Version = "HTML+RDFa 1.0";

        private RdfASyntax _syntax = RdfASyntax.AutoDetectLegacy;

        /// <summary>
        /// Creates a new RDFa Parser which will auto-detect which RDFa version to use (assumes 1.1 if none explicitly specified)
        /// </summary>
        public RdfAParser()
        {

        }

        /// <summary>
        /// Creates a new RDFa Parser which will use the specified RDFa syntax
        /// </summary>
        /// <param name="syntax">RDFa Syntax Version</param>
        public RdfAParser(RdfASyntax syntax)
        {
            this._syntax = syntax;
        }

        /// <summary>
        /// Parses RDFa by extracting it from the HTML from the given input
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="input">Stream to read from</param>
        public void Load(IGraph g, StreamReader input)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            this.Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Parses RDFa by extracting it from the HTML from the given input
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="input">Input to read from</param>
        public void Load(IGraph g, TextReader input)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            this.Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Parses RDFa by extracting it from the HTML from the given file
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="filename">File to read from</param>
        public void Load(IGraph g, string filename)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");
            this.Load(new GraphHandler(g), filename);
        }

        /// <summary>
        /// Parses RDFa by extracting it from the HTML from the given input
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">Stream to read from</param>
        public void Load(IRdfHandler handler, StreamReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannot read RDF from a null Stream");
            this.Load(handler, (TextReader)input);
        }

        /// <summary>
        /// Parses RDFa by extracting it from the HTML from the given input
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">Input to read from</param>
        public void Load(IRdfHandler handler, TextReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannot read RDF from a null TextReader");

            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.Load(input);

                RdfAParserContext context = new RdfAParserContext(handler, doc);
                this.Parse(context);
            }
            catch
            {
                throw;
            }
            finally
            {
                try
                {
                    input.Close();
                }
                catch
                {
                    //Catch is just here in case something goes wrong with closing the stream
                    //This error can be ignored
                }
            }
        }

        /// <summary>
        /// Parses RDFa by extracting it from the HTML from the given input
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="filename">File to read from</param>
        public void Load(IRdfHandler handler, String filename)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");
            this.Load(handler, new StreamReader(filename, Encoding.UTF8));
        }

        private void Parse(RdfAParserContext context)
        {
            try
            {
                context.Handler.StartRdf();

                //Setup the basic evaluation context and start processing
                RdfAEvaluationContext evalContext = new RdfAEvaluationContext(context.BaseUri);
                evalContext.NamespaceMap.AddNamespace(String.Empty, new Uri(XHtmlVocabNamespace));

                //Set the Default and Local Vocabularly
                context.DefaultVocabulary = new XHtmlRdfAVocabulary();
                evalContext.LocalVocabulary = new TermMappings();

                //If there's a base element this permanently changes the Base URI
                HtmlNode baseEl = context.Document.DocumentNode.SelectSingleNode("/html/head/base");
                if (baseEl != null)
                {
                    if (baseEl.Attributes.Contains("href"))
                    {
                        String uri = baseEl.Attributes["href"].Value;
                        if (uri.Contains("?"))
                        {
                            evalContext.BaseUri = new Uri(uri.Substring(0, uri.IndexOf('?')));
                        }
                        else if (uri.Contains("#"))
                        {
                            evalContext.BaseUri = new Uri(uri.Substring(0, uri.IndexOf('#')));
                        }
                        else
                        {
                            evalContext.BaseUri = new Uri(baseEl.Attributes["href"].Value);
                        }
                    }
                }

                //Check whether xml:base is permissible
                HtmlNodeCollection docTypes = context.Document.DocumentNode.SelectNodes("comment()");
                if (docTypes != null)
                {
                    foreach (HtmlNode docType in docTypes)
                    {
                        if (docType.InnerText.StartsWith("<!DOCTYPE"))
                        {
                            //Extract the Document Type
                            Match dtd = Regex.Match(docType.InnerText, "\"([^\"]+)\">");
                            if (dtd.Success)
                            {
                                if (dtd.Groups[1].Value.Equals(XHtmlPlusRdfADoctype))
                                {
                                    //XHTML+RDFa does not permit xml:base
                                    context.XmlBaseAllowed = false;
                                }
                                break;
                            }
                        }
                    }
                }

                //Select the Syntax Version to use
                context.Syntax = this._syntax;
                if (context.Syntax == RdfASyntax.AutoDetect || context.Syntax == RdfASyntax.AutoDetectLegacy)
                {
                    HtmlNode docNode;
                    if (context.Document.DocumentNode.Name.Equals("html"))
                    {
                        docNode = context.Document.DocumentNode;
                    }
                    else
                    {
                        docNode = context.Document.DocumentNode.SelectSingleNode("html");
                    }
                    if (docNode != null && docNode.Attributes.Contains("version"))
                    {
                        String version = docNode.Attributes["version"].Value;
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
                                    this.OnWarning("The value '" + version + "' is not a known value for the @version attribute - assuming RDFa 1.1");
                                }
                                else
                                {
                                    context.Syntax = RdfASyntax.RDFa_1_0;
                                    this.OnWarning("The value '" + version + "' is not a known value for the @version attribute - assuming RDFa 1.0");
                                }
                                break;
                        }
                    }
                    else if (context.Syntax == RdfASyntax.AutoDetect)
                    {
                        context.Syntax = RdfASyntax.RDFa_1_1;
                        this.OnWarning("No @version attribute on document node - assuming RDFa 1.1");
                    }
                    else if (context.Syntax == RdfASyntax.AutoDetectLegacy)
                    {
                        context.Syntax = RdfASyntax.RDFa_1_0;
                        this.OnWarning("No @version attribute on document node - assuming RDFa 1.0");
                    }
                    else
                    {
                        context.Syntax = RdfASyntax.RDFa_1_1;
                        this.OnWarning("No @version attribute on document node - assuming RDFa 1.1");
                    }
                }

                this.ProcessElement(context, evalContext, context.Document.DocumentNode);

                context.Handler.EndRdf(true);
            }
            catch (RdfParsingTerminatedException)
            {
                context.Handler.EndRdf(true);
                //Discard this - it justs means the Handler told us to stop
            }
            catch
            {
                context.Handler.EndRdf(false);
                throw;
            }
        }

        private void ProcessElement(RdfAParserContext context, RdfAEvaluationContext evalContext, HtmlNode currElement)
        {
            bool recurse = true, skip = false;
            bool rel = false,
                 rev = false,
                 about = false,
                 src = false,
                 href = false, 
                 property = false,
                 type = false,
                 resource = false,
                 content = false,
                 datatype = false;
            bool noDefaultNamespace = false;
            
            INode newSubj = null, currObj = null;
            List<IncompleteTriple> incomplete = new List<IncompleteTriple>();
            List<String> inScopePrefixes = new List<string>();
            Dictionary<String, Uri> hiddenPrefixes = null;
            String lang = evalContext.Language;
            Uri oldBase = evalContext.BaseUri;
            bool baseChanged = false;
            String oldLang = lang;
            bool langChanged = false;
            String baseUri = (evalContext.BaseUri == null) ? String.Empty : evalContext.BaseUri.ToString();

            #region Steps 2-5 of the RDFa Processing Rules

            //Locate namespaces and other relevant attributes
            foreach (HtmlAttribute attr in currElement.Attributes)
            {
                String uri;
                if (attr.Name.StartsWith("xmlns:"))
                {
                    uri = Tools.ResolveUri(attr.Value, baseUri);
                    if (!(uri.EndsWith("/") || uri.EndsWith("#"))) uri += "#";
                    String prefix = attr.Name.Substring(attr.Name.IndexOf(':') + 1);
                    if (evalContext.NamespaceMap.HasNamespace(prefix))
                    {
                        if (hiddenPrefixes == null) hiddenPrefixes = new Dictionary<string, Uri>();
                        hiddenPrefixes.Add(prefix, evalContext.NamespaceMap.GetNamespaceUri(prefix));
                    }
                    evalContext.NamespaceMap.AddNamespace(prefix, new Uri(uri));
                    inScopePrefixes.Add(prefix);
                }
                else
                {
                    switch (attr.Name)
                    {
                        case "xml:lang":
                        case "lang":
                            //@lang and @xml:lang have the same affect
                            if (!langChanged)
                            {
                                oldLang = lang;
                                lang = attr.Value;
                                langChanged = true;
                            }
                            break;
                        case "xml:base":
                            //@xml:base may be permitted in some cases
                            if (context.XmlBaseAllowed)
                            {
                                baseUri = Tools.ResolveUri(attr.Value, baseUri);
                                if (!(baseUri.EndsWith("/") || baseUri.EndsWith("#"))) baseUri += "#";
                                oldBase = evalContext.BaseUri;
                                baseChanged = true;
                                evalContext.BaseUri = new Uri(baseUri);
                            }
                            break;
                        case "xmlns":
                            //Can use @xmlns to override the default namespace
                            uri = attr.Value;
                            if (!(uri.EndsWith("/") || uri.EndsWith("#"))) uri += "#";
                            if (evalContext.NamespaceMap.HasNamespace(String.Empty))
                            {
                                if (hiddenPrefixes == null) hiddenPrefixes = new Dictionary<string, Uri>();
                                hiddenPrefixes.Add(String.Empty, evalContext.NamespaceMap.GetNamespaceUri(String.Empty));
                            }
                            evalContext.NamespaceMap.AddNamespace(String.Empty, new Uri(uri));
                            inScopePrefixes.Add(String.Empty);
                            noDefaultNamespace = true;
                            break;
                        case "prefix":
                            //Can use @prefix to set multiple namespaces with one attribute
                            if (context.Syntax == RdfASyntax.RDFa_1_0)
                            {
                                this.OnWarning("Cannot use the @prefix attribute to define prefixes in RDFa 1.0");
                            }
                            else
                            {
                                this.ParsePrefixAttribute(context, evalContext, attr, baseUri, hiddenPrefixes, inScopePrefixes);
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
                                this.OnWarning("Cannot use the @profile attribute in RDFa 1.0");
                            }
                            else
                            {
                                if (this.ParseProfileAttribute(context, evalContext, attr))
                                {
                                    foreach (KeyValuePair<String, String> ns in evalContext.LocalVocabulary.Namespaces)
                                    {
                                        uri = Tools.ResolveUri(ns.Value, baseUri);
                                        if (!(uri.EndsWith("/") || uri.EndsWith("#"))) uri += "#";
                                        if (evalContext.NamespaceMap.HasNamespace(ns.Key))
                                        {
                                            if (hiddenPrefixes == null) hiddenPrefixes = new Dictionary<string, Uri>();
                                            hiddenPrefixes.Add(ns.Key, evalContext.NamespaceMap.GetNamespaceUri(ns.Key));
                                        }
                                        evalContext.NamespaceMap.AddNamespace(ns.Key, new Uri(uri));
                                        inScopePrefixes.Add(ns.Key);
                                    }
                                }
                                else
                                {
                                    this.OnWarning("Unable to resolve a Profile document specified by the @profile attribute on the element <" + currElement.Name + "> - ignoring the DOM subtree of this element");
                                    return;
                                }
                            }
                            break;
                        case "vocab":
                            if (context.Syntax == RdfASyntax.RDFa_1_0)
                            {
                                this.OnWarning("Cannot use the @vocab attribute in RDFa 1.0");
                            }
                            else
                            {
                                this.ParseVocabAttribute(context, evalContext, attr);
                            }
                            break;
                    }
                }
            }

            #endregion

            #region Steps 6-7 of the RDFa Processing Rules

            //If we hit an invalid CURIE/URI at any point then ResolveUriOrCurie will return a null and 
            //later processing steps will be skipped for this element
            //Calls to Tools.ResolveUri which error will still cause the parser to halt
            if (!rel && !rev)
            {
                 //No @rel or @rev attributes
                if (about && !currElement.Attributes["about"].Value.Equals("[]"))
                {
                    //New Subject is the URI
                    newSubj = this.ResolveUriOrCurie(context, evalContext, currElement.Attributes["about"].Value);
                }
                else if (src)
                {
                    //New Subject is the URI
                    newSubj = context.Handler.CreateUriNode(new Uri(Tools.ResolveUri(currElement.Attributes["src"].Value, baseUri)));
                }
                else if (resource && !currElement.Attributes["resource"].Value.Equals("[]"))
                {
                    //New Subject is the URI
                    newSubj = this.ResolveUriOrCurie(context, evalContext, currElement.Attributes["resource"].Value);
                }
                else if (href)
                {
                    //New Subject is the URI
                    newSubj = context.Handler.CreateUriNode(new Uri(Tools.ResolveUri(currElement.Attributes["href"].Value, baseUri)));
                }
                else if (currElement.Name.Equals("head") || currElement.Name.Equals("body"))
                {
                    //New Subject is the Base URI
                    try
                    {
                        newSubj = context.Handler.CreateUriNode(new Uri(Tools.ResolveUri(String.Empty, baseUri)));
                    }
                    catch (RdfException)
                    {
                        if (baseUri.Equals(String.Empty))
                        {
                            this.OnWarning("Unable to generate a valid Subject for a Triple since the Base URI should be used but there is no in-scope Base URI");
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
                    //New Subject is a Blank Node
                    newSubj = context.Handler.CreateBlankNode();
                }
                else if (evalContext.ParentObject != null)
                {
                    //New Subject is the Parent Object and will skip if no property attributes
                    newSubj = evalContext.ParentObject;
                    if (!property) skip = true;
                }
            }
            else
            {
                //A @rel or @rev attribute was encountered

                //For this we first set the Subject
                if (about && !currElement.Attributes["about"].Value.Equals("[]"))
                {
                    //New Subject is the URI
                    newSubj = this.ResolveUriOrCurie(context, evalContext, currElement.Attributes["about"].Value);
                }
                else if (src)
                {
                    //New Subject is the URI
                    newSubj = context.Handler.CreateUriNode(new Uri(Tools.ResolveUri(currElement.Attributes["src"].Value, baseUri)));
                }
                else if (currElement.Name.Equals("head") || currElement.Name.Equals("body"))
                {
                    //New Subject is the Base URI
                    try
                    {
                        newSubj = context.Handler.CreateUriNode(new Uri(Tools.ResolveUri(String.Empty, baseUri)));
                    }
                    catch (RdfException)
                    {
                        if (baseUri.Equals(String.Empty))
                        {
                            this.OnWarning("Unable to generate a valid Subject for a Triple since the Base URI should be used but there is no in-scope Base URI");
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
                    //New Subject is a Blank Node
                    newSubj = context.Handler.CreateBlankNode();
                }
                else if (evalContext.ParentObject != null)
                {
                    //New Subject is the Parent Object and will skip if no property attributes
                    newSubj = evalContext.ParentObject;
                }

                //Then we set the Object as well
                if (resource && !currElement.Attributes["resource"].Value.Equals("[]"))
                {
                    //New Object is the URI
                    currObj = this.ResolveUriOrCurie(context, evalContext, currElement.Attributes["resource"].Value);
                }
                else if (href)
                {
                    //New Object is the URI
                    currObj = context.Handler.CreateUriNode(new Uri(Tools.ResolveUri(currElement.Attributes["href"].Value, baseUri)));
                }
            }

            #endregion

            #region Step 8 of the RDFa Processing Rules

            //If the Subject is not a null then we'll generate type triples if there's any @typeof attributes
            if (newSubj != null)
            {
                INode rdfType = context.Handler.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
                if (type)
                {
                    foreach (INode dtObj in this.ParseComplexAttribute(context, evalContext, currElement.Attributes["typeof"].Value))
                    {
                        if (!context.Handler.HandleTriple(new Triple(newSubj, rdfType, dtObj))) ParserHelper.Stop();
                    }
                }
            }

            #endregion

            #region Steps 9-10 of the RDFa Processing Rules

            //If the Object is not null we'll generate triples
            if (newSubj != null && currObj != null)
            {
                //We can generate some complete triples
                if (rel)
                {
                    foreach (INode pred in this.ParseComplexAttribute(context, evalContext, currElement.Attributes["rel"].Value))
                    {
                        if (!context.Handler.HandleTriple(new Triple(newSubj, pred, currObj))) ParserHelper.Stop();
                    }
                }
                if (rev)
                {
                    foreach (INode pred in this.ParseComplexAttribute(context, evalContext, currElement.Attributes["rev"].Value))
                    {
                        if (!context.Handler.HandleTriple(new Triple(currObj, pred, newSubj))) ParserHelper.Stop();
                    }
                }
            }
            else
            {
                //We can generate some incomplete triples
                bool preds = false;
                if (rel)
                {
                    foreach (INode pred in this.ParseComplexAttribute(context, evalContext, currElement.Attributes["rel"].Value))
                    {
                        preds = true;
                        incomplete.Add(new IncompleteTriple(pred, IncompleteTripleDirection.Forward));
                    }
                }
                if (rev)
                {
                    foreach (INode pred in this.ParseComplexAttribute(context, evalContext, currElement.Attributes["rev"].Value))
                    {
                        preds = true;
                        incomplete.Add(new IncompleteTriple(pred, IncompleteTripleDirection.Reverse));
                    }
                }

                if (preds)
                {
                    //Current Object becomes a Blank Node only if there were predicates
                    currObj = context.Handler.CreateBlankNode();
                }
            }

            #endregion

            #region Step 11 of the RDFa Processing Rules

            //Get the Current Object Literal
            if (newSubj != null && property)
            {
                //We only look for this if there is a property attribute
                INode currLiteral = null;
                Uri dt;
                INode dtNode = null;

                if (datatype && !currElement.Attributes["datatype"].Value.Equals(String.Empty))
                {
                    //Some kind of Typed Literal
                    //Resolve the Datatype attribute into URIs
                    try
                    {
                        dtNode = this.ResolveTermOrCurieOrUri(context, evalContext, currElement.Attributes["datatype"].Value);
                    }
                    catch (RdfException)
                    {
                        this.OnWarning("Unable to resolve a valid Datatype for the Literal since the value '" + currElement.Attributes["datatype"].Value + "' is not a valid CURIE or it cannot be resolved into a URI given the in-scope namespace prefixes and Base URI - assuming a Plain Literal instead");
                    }
                }
                if (dtNode != null)
                {
                    //We can only process this Triple if we were able to get a valid URI Node for the Datatype
                    if (dtNode.NodeType != NodeType.Uri) throw new RdfParseException("Cannot use a non-URI Node as a Dataype");
                    dt = ((IUriNode)dtNode).Uri;

                    if (!dt.ToString().Equals(RdfSpecsHelper.RdfXmlLiteral))
                    {
                        //There's a Datatype and it's not XML Literal
                        if (content)
                        {
                            //Content attribute is used as the value
                            currLiteral = context.Handler.CreateLiteralNode(currElement.Attributes["content"].Value, dt);
                        }
                        else
                        {
                            //Value is concatentation of child text nodes
                            StringBuilder lit = new StringBuilder();
                            foreach (HtmlNode n in currElement.ChildNodes)
                            {
                                this.GrabText(lit, n);
                            }
                            currLiteral = context.Handler.CreateLiteralNode(HttpUtility.HtmlDecode(lit.ToString()), dt);
                        }
                    }
                    else if (context.Syntax == RdfASyntax.RDFa_1_0)
                    {
                        //It's an XML Literal - this is now RDFa 1.0 Only
                        //This is an incompatability with RDFa 1.1
                        foreach (HtmlNode child in currElement.ChildNodes)
                        {
                            this.ProcessXmlLiteral(evalContext, child, noDefaultNamespace);
                        }
                        currLiteral = context.Handler.CreateLiteralNode(currElement.InnerHtml, dt);
                    }
                    else if (context.Syntax == RdfASyntax.RDFa_1_1)
                    {
                        //For RDFa 1.1 we now treat as a plain literal instead
                        //Setting this to null forces us to go into the if that processes plain literals
                        dtNode = null;
                    }
                }
                
                if (dtNode == null)
                {
                    //A Plain Literal
                    if (content)
                    {
                        //Content attribute is used as the value
                        currLiteral = context.Handler.CreateLiteralNode(currElement.Attributes["content"].Value, lang);
                    }
                    else if (!currElement.HasChildNodes)
                    {
                        //Value is content of the element (if any)
                        currLiteral = context.Handler.CreateLiteralNode(HttpUtility.HtmlDecode(currElement.InnerText), lang);
                    }
                    else if (currElement.ChildNodes.All(n => n.NodeType == HtmlNodeType.Text))
                    {
                        //Value is concatenation of all Text Child Nodes
                        StringBuilder lit = new StringBuilder();
                        foreach (HtmlNode n in currElement.ChildNodes)
                        {
                            if (n.NodeType == HtmlNodeType.Text)
                            {
                                lit.Append(n.InnerText);
                            }
                        }
                        currLiteral = context.Handler.CreateLiteralNode(HttpUtility.HtmlDecode(lit.ToString()), lang);
                    }
                    else if (!datatype || (datatype && currElement.Attributes["datatype"].Equals(String.Empty)))
                    {
                        //Value is an XML Literal
                        foreach (HtmlNode child in currElement.ChildNodes)
                        {
                            this.ProcessXmlLiteral(evalContext, child, noDefaultNamespace);
                        }
                        currLiteral = context.Handler.CreateLiteralNode(currElement.InnerHtml, new Uri(RdfSpecsHelper.RdfXmlLiteral));
                    }
                }

                //Get the Properties which we are connecting this literal with
                if (currLiteral != null)
                {
                    foreach (INode pred in this.ParseAttribute(context, evalContext, currElement.Attributes["property"].Value))
                    {
                        if (!context.Handler.HandleTriple(new Triple(newSubj, pred, currLiteral))) ParserHelper.Stop();
                    }
                }
            }

            #endregion

            #region Step 12 of the RDFa Processing Rules

            //Complete incomplete Triples if this is possible
            if (!skip && newSubj != null && evalContext.ParentSubject != null)
            {
                foreach (IncompleteTriple i in evalContext.IncompleteTriples)
                {
                    if (i.Direction == IncompleteTripleDirection.Forward)
                    {
                        if (!context.Handler.HandleTriple(new Triple(evalContext.ParentSubject, i.Predicate, newSubj))) ParserHelper.Stop();
                    }
                    else
                    {
                        if (!context.Handler.HandleTriple(new Triple(newSubj, i.Predicate, evalContext.ParentSubject))) ParserHelper.Stop();
                    }
                }
            }

            #endregion

            #region Step 13 of the RDFa Processing Rules

            //Recurse if necessary
            if (recurse)
            {
                if (currElement.HasChildNodes)
                {
                    //Generate the new Evaluation Context (if applicable)
                    RdfAEvaluationContext newEvalContext;
                    if (skip)
                    {
                        newEvalContext = evalContext;
                        newEvalContext.Language = lang;
                    }
                    else
                    {
                        Uri newBase = (baseUri.Equals(String.Empty)) ? null : new Uri(baseUri);
                        newEvalContext = new RdfAEvaluationContext(newBase, evalContext.NamespaceMap);
                        //Set the Parent Subject for the new Context
                        if (newSubj != null)
                        {
                            newEvalContext.ParentSubject = newSubj;
                        }
                        else
                        {
                            newEvalContext.ParentSubject = evalContext.ParentSubject;
                        }
                        //Set the Parent Object for the new Context
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
                        newEvalContext.IncompleteTriples.AddRange(incomplete);
                        newEvalContext.Language = lang;
                    }

                    newEvalContext.LocalVocabulary = new TermMappings(evalContext.LocalVocabulary);

                    //Iterate over the Nodes
                    foreach (HtmlNode n in currElement.ChildNodes)
                    {
                        if (n.NodeType == HtmlNodeType.Element)
                        {
                            this.ProcessElement(context, newEvalContext, n);
                        }
                    }
                }
            }

            #endregion

            //Now any in-scope prefixes go out of scope
            foreach (String prefix in inScopePrefixes)
            {
                evalContext.NamespaceMap.RemoveNamespace(prefix);
                //If they were hiding another prefix then that comes back into scope
                if (hiddenPrefixes != null)
                {
                    if (hiddenPrefixes.ContainsKey(prefix))
                    {
                        evalContext.NamespaceMap.AddNamespace(prefix, hiddenPrefixes[prefix]);
                    }
                }
            }

            //And the Base URI resets if it was changed
            if (baseChanged)
            {
                evalContext.BaseUri = oldBase;
            }

            //And the Language resets if it was changed
            if (langChanged)
            {
                evalContext.Language = oldLang;
            }
        }

        /// <summary>
        /// Resolves a CURIE to a Node
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="evalContext">Evaluation Context</param>
        /// <param name="curie">CURIE</param>
        /// <returns></returns>
        private INode ResolveCurie(RdfAParserContext context, RdfAEvaluationContext evalContext, String curie)
        {
            if (curie.StartsWith("_:"))
            {
                //The CURIE is for a Blank Node
                if (curie.Equals("_:"))
                {
                    return context.Handler.CreateBlankNode("_");
                }
                else
                {
                    return context.Handler.CreateBlankNode(curie.Substring(2));
                }
            }
            else
            {
                //CURIE is for a URI
                if (context.Syntax == RdfASyntax.RDFa_1_0)
                {
                    //RDFa 1.0
                    if (curie.StartsWith(":"))
                    {
                        return context.Handler.CreateUriNode(new Uri(XHtmlVocabNamespace + curie.Substring(1)));
                    }
                    else if (curie.Contains(":"))
                    {
                        return context.Handler.CreateUriNode(new Uri(Tools.ResolveQName(curie, evalContext.NamespaceMap, evalContext.BaseUri)));
                    }
                    else
                    {
                        throw new RdfParseException("The value '" + curie + "' is not valid as a CURIE as it does not have a prefix");
                    }
                }
                else
                {
                    //RDFa 1.1
                    return context.Handler.CreateUriNode(new Uri(Tools.ResolveQName(curie, evalContext.NamespaceMap, evalContext.BaseUri)));
                }
            }
        }

        /// <summary>
        /// Resolves an Attribute which may be a CURIE/URI to a Node
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="evalContext">Evaluation Context</param>
        /// <param name="uriref">URI/CURIE</param>
        /// <returns></returns>
        private INode ResolveUriOrCurie(RdfAParserContext context, RdfAEvaluationContext evalContext, String uriref)
        {
            try
            {
                if (uriref.StartsWith("["))
                {
                    //CURIE
                    String curie = uriref.Substring(1, uriref.Length - 2);
                    return this.ResolveCurie(context, evalContext, curie);
                }
                else if (this.IsCurie(evalContext, uriref))
                {
                    //CURIE
                    return this.ResolveCurie(context, evalContext, uriref);
                }
                else
                {
                    //URI
                    return context.Handler.CreateUriNode(new Uri(Tools.ResolveUri(uriref, evalContext.BaseUri.ToSafeString())));
                }
            }
            catch (RdfException)
            {
                this.OnWarning("Unable to resolve a URI or CURIE since the value '" + uriref + "' does not contain a valid URI/CURIE or it cannot be resolved to a URI given the in-scope namespace prefixes and Base URI");
                return null;
            }
        }

        /// <summary>
        /// Resolves an Attribute which may be a Term/CURIE/URI to a Node where one/more of the values may be special values permissible in a complex attribute
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="evalContext">Evaluation Context</param>
        /// <param name="curie">URI/CURIE/Term</param>
        /// <returns></returns>
        private INode ResolveTermOrCurie(RdfAParserContext context, RdfAEvaluationContext evalContext, String curie)
        {
            if (context.Syntax == RdfASyntax.RDFa_1_0)
            {
                //RDFa 1.0
                XHtmlRdfAVocabulary vocab = new XHtmlRdfAVocabulary();
                if (curie.StartsWith(":"))
                {
                    return context.Handler.CreateUriNode(new Uri(vocab.ResolveTerm(curie.Substring(1))));
                }
                else if (curie.Contains(":"))
                {
                    return this.ResolveCurie(context, evalContext, curie);
                }
                else
                {
                    if (vocab.HasTerm(curie))
                    {
                        return context.Handler.CreateUriNode(new Uri(vocab.ResolveTerm(curie)));
                    }
                    else
                    {
                        throw new RdfParseException("Cannot use an unprefixed CURIE in RDFa 1.0 - only reserved XHTML terms are permitted");
                    }
                }
            }
            else
            {
                //RDFa 1.1
                if (curie.StartsWith(":"))
                {
                    if (evalContext.LocalVocabulary != null)
                    {
                        if (evalContext.LocalVocabulary.HasTerm(curie.Substring(1)) || !evalContext.LocalVocabulary.VocabularyUri.Equals(String.Empty))
                        {
                            return context.Handler.CreateUriNode(new Uri(evalContext.LocalVocabulary.ResolveTerm(curie.Substring(1))));
                        }
                        else if (context.DefaultVocabulary != null && context.DefaultVocabulary.HasTerm(curie.Substring(1)))
                        {
                            return context.Handler.CreateUriNode(new Uri(context.DefaultVocabulary.ResolveTerm(curie.Substring(1))));
                        }
                        else
                        {
                            return this.ResolveCurie(context, evalContext, curie);
                        }
                    }
                    else if (context.DefaultVocabulary != null && context.DefaultVocabulary.HasTerm(curie.Substring(1)))
                    {
                        return context.Handler.CreateUriNode(new Uri(context.DefaultVocabulary.ResolveTerm(curie.Substring(1))));
                    }
                    else
                    {
                        return this.ResolveCurie(context, evalContext, curie);
                    }
                }
                else
                {
                    if (evalContext.LocalVocabulary != null)
                    {
                        if (evalContext.LocalVocabulary.HasTerm(curie) || !evalContext.LocalVocabulary.VocabularyUri.Equals(String.Empty))
                        {
                            return context.Handler.CreateUriNode(new Uri(evalContext.LocalVocabulary.ResolveTerm(curie)));
                        }
                        else if (context.DefaultVocabulary != null && context.DefaultVocabulary.HasTerm(curie))
                        {
                            return context.Handler.CreateUriNode(new Uri(context.DefaultVocabulary.ResolveTerm(curie)));
                        }
                        else
                        {
                            throw new RdfParseException("Unable to resolve a Term since there is no appropriate Local/Default Vocabulary in scope");
                        }
                    }
                    else if (context.DefaultVocabulary != null)
                    {
                        return context.Handler.CreateUriNode(new Uri(context.DefaultVocabulary.ResolveTerm(curie)));
                    }
                    else
                    {
                        throw new RdfParseException("Unable to resolve a Term since there is no appropriate Local/Default Vocabularly in scope");
                    }
                }
            }
        }

        private INode ResolveTermOrCurieOrUri(RdfAParserContext context, RdfAEvaluationContext evalContext, String value)
        {
            if (this.IsTerm(value))
            {
                return this.ResolveTermOrCurie(context, evalContext, value);
            }
            else if (this.IsCurie(evalContext, value))
            {
                return this.ResolveCurie(context, evalContext, value);
            }
            else
            {
                return context.Handler.CreateUriNode(new Uri(Tools.ResolveUri(value, evalContext.BaseUri.ToSafeString())));
            }
        }

        /// <summary>
        /// Parses an complex attribute into a number of Nodes
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="evalContext">Evaluation Context</param>
        /// <param name="value">Attribute Value</param>
        /// <returns></returns>
        /// <remarks>
        /// A complex attribute is any attribute which accepts multiple URIs, CURIEs or Terms
        /// </remarks>
        private List<INode> ParseComplexAttribute(RdfAParserContext context, RdfAEvaluationContext evalContext, String value)
        {
            List<INode> nodes = new List<INode>();

            String[] values;
            if (value.Contains(" "))
            {
                values = value.Split(' ');
            } 
            else 
            {
                values = new String[] { value };
            }
            foreach (String val in values)
            {
                try
                {
                    INode n = this.ResolveTermOrCurieOrUri(context, evalContext, val);
                    nodes.Add(n);
                }
                catch
                {
                    //Errors are ignored, they don't produce a URI
                    //Raise a warning anyway
                    this.OnWarning("Ignoring the value '" + val + "' since this is not a valid Term/CURIE/URI or it cannot be resolved into a URI given the in-scope Namespace Prefixes and Base URI");
                }
            }

            return nodes;
        }

        /// <summary>
        /// Parses an attribute into a number of Nodes from the CURIEs contained in the Attribute
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="evalContext">Evaluation Context</param>
        /// <param name="value">Attribute Value</param>
        /// <returns></returns>
        private List<INode> ParseAttribute(RdfAParserContext context, RdfAEvaluationContext evalContext, String value)
        {
            List<INode> nodes = new List<INode>();

            String[] values;
            if (value.Contains(" "))
            {
                values = value.Split(' ');
            } 
            else 
            {
                values = new String[] { value };
            }
            foreach (String val in values)
            {
                try
                {
                    INode n = this.ResolveCurie(context, evalContext, val);
                    nodes.Add(n);
                }
                catch
                {
                    //Errors are ignored, they don't produce a URI
                    //Raise a warning anyway
                    this.OnWarning("Ignoring the value '" + val + "' since this is not a valid CURIE or it cannot be resolved into a URI given the in-scope Namespace Prefixes and Base URI");
                }
            }

            return nodes;
        }

        private void ParsePrefixAttribute(RdfAParserContext context, RdfAEvaluationContext evalContext, HtmlAttribute attr, String baseUri, Dictionary<string,Uri> hiddenPrefixes, List<String> inScopePrefixes)
        {
            //Do nothing if the @prefix attribute is empty
            if (attr.Value.Equals(String.Empty)) return;

            StringReader reader = new StringReader(attr.Value);
            char next;
            bool canExit = false;

            do 
            {
                StringBuilder prefixData = new StringBuilder();
                StringBuilder uriData = new StringBuilder();

                //Grab a Prefix - characters up to the next colon
                next = (char)reader.Peek();
                while (next != ':')
                {
                    //Add the Character and discard it
                    prefixData.Append(next);
                    reader.Read();
                    if (reader.Peek() == -1)
                    {
                        this.OnWarning("Aborted parsing a prefix attribute since failed to find a prefix of the form prefix: from the following content: " + prefixData.ToString());
                        return;
                    }
                    else
                    {
                        next = (char)reader.Peek();
                    }
                }

                //Discard the colon
                reader.Read();

                //Discard the whitespace
                next = (char)reader.Peek();
                while (Char.IsWhiteSpace(next))
                {
                    reader.Read();
                    if (reader.Peek() == -1)
                    {
                        this.OnWarning("Aborted parsing a prefix attribute since reached the end of the attribute without finding a URI to go with the prefix '" + prefixData.ToString() + ":'");
                        return;
                    }
                    else
                    {
                        next = (char)reader.Peek();
                    }
                }

                //Grab the URI - characters up to the next whitespace or end of string
                next = (char)reader.Peek();
                while (!Char.IsWhiteSpace(next))
                {
                    uriData.Append(next);
                    reader.Read();
                    if (reader.Peek() == -1)
                    {
                        //End of string so will exit after this
                        canExit = true;
                        break;
                    }
                    else
                    {
                        next = (char)reader.Peek();
                    }
                }

                //Now resolve the URI and apply it
                String uri = Tools.ResolveUri(uriData.ToString(), baseUri);
                if (!(uri.EndsWith("/") || uri.EndsWith("#"))) uri += "#";
                String prefix = prefixData.ToString();
                if (evalContext.NamespaceMap.HasNamespace(prefix))
                {
                    if (hiddenPrefixes == null) hiddenPrefixes = new Dictionary<string, Uri>();
                    hiddenPrefixes.Add(prefix, new Uri(uri));
                }
                evalContext.NamespaceMap.AddNamespace(prefix, new Uri(uri));
                inScopePrefixes.Add(prefix);
            } while (!canExit);
        }

        private bool ParseProfileAttribute(RdfAParserContext context, RdfAEvaluationContext evalContext, HtmlAttribute attr)
        {
            String[] profiles;
            if (attr.Value.Contains(" "))
            {
                profiles = attr.Value.Split(' ');
            }
            else
            {
                profiles = new String[] { attr.Value };
            }

            String prefixQuery = "PREFIX rdfa: <" + RdfANamespace + "> SELECT SAMPLE(?prefix) AS ?NamespacePrefix SAMPLE(?uri) AS ?NamespaceURI WHERE { ?s rdfa:prefix ?prefix ; rdfa:uri ?uri } GROUP BY ?s HAVING (COUNT(?prefix) = 1 && COUNT(?uri) = 1)";
            String termQuery = "PREFIX rdfa: <" + RdfANamespace + "> SELECT SAMPLE(?term) AS ?Term SAMPLE(?uri) AS ?URI WHERE {?s rdfa:term ?term ; rdfa:uri ?uri } GROUP BY ?s HAVING (COUNT(?term) = 1 && COUNT(?uri) = 1)";

            foreach (String profile in profiles)
            {
                try
                {
                    Graph g = new Graph();

                    if (profile.Equals(XHtmlVocabNamespace) || profile.Equals(XHtmlVocabNamespace.Substring(0, XHtmlVocabNamespace.Length-1)))
                    {
                        //XHTML Vocabulary is a fixed vocabulary
                        evalContext.LocalVocabulary.Merge(new XHtmlRdfAVocabulary());
                    }
                    else
                    {
                        try
                        {
#if !SILVERLIGHT
                            UriLoader.Load(g, new Uri(profile));
#else
                            throw new PlatformNotSupportedException("The @profile attribute is not currently supported under Silverlight/Windows Phone 7");
#endif
                        }
                        catch
                        {
                            //If we fail then we return false which indicates that the DOM subtree is ignored
                            this.OnWarning("Unable to retrieve a Profile document which the library could parse from the URI '" + profile + "'");
                            return false;
                        }

                        //Namespace Mappings
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
                                    evalContext.LocalVocabulary.AddNamespace(prefix, ns);
                                }
                            }
                        }

                        //Term Mappings
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
                                    evalContext.LocalVocabulary.AddTerm(term, uri);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    //Ignore errors and continue processing
                    this.OnWarning("Ignoring the value '" + profile + "' since this is not a valid URI or a profile document was not successfully retrieved and parsed from this URI");
                    return false;
                }
            }

            return true;
        }

        private void ParseVocabAttribute(RdfAParserContext context, RdfAEvaluationContext evalContext, HtmlAttribute attr)
        {
            if (attr.Value.Equals(String.Empty))
            {
                //Reset Local Vocabulary
                evalContext.LocalVocabulary = new TermMappings(context.DefaultVocabulary);
            }
            else
            {
                evalContext.LocalVocabulary.VocabularyUri = attr.Value;
            }
        }

        private void GrabText(StringBuilder output, HtmlNode n)
        {
            switch (n.NodeType)
            {
                case HtmlNodeType.Document:
                case HtmlNodeType.Element:
                    foreach (HtmlNode child in n.ChildNodes)
                    {
                        this.GrabText(output, child);
                    }
                    break;
                case HtmlNodeType.Text:
                    output.Append(n.InnerText);
                    break;
            }
        }

        private void ProcessXmlLiteral(RdfAEvaluationContext evalContext, HtmlNode n, bool noDefaultNamespace)
        {
            if (n.NodeType == HtmlNodeType.Element)
            {
                //Add Default Namespace as XHTML Namespace unless this would override an existing namespace
                if (!n.Attributes.Contains("xmlns"))
                {
                    if (!noDefaultNamespace) n.Attributes.Add("xmlns", XHtmlNamespace);
                }
                else
                {
                    noDefaultNamespace = true;
                }

                //Add specific namespaces if necessary
                if (n.Name.Contains(":"))
                {
                    String prefix = n.Name.Substring(0, n.Name.IndexOf(':'));
                    if (n.Attributes.Contains("xmlns:" + prefix))
                    {
                        //If the Node itself declares the Namespace then we don't need to do anything
                    }
                    else if (evalContext.NamespaceMap.HasNamespace(prefix))
                    {
                        //If the Node doesn't declare the Namespace
                        n.Attributes.Add("xmlns:" + prefix, evalContext.NamespaceMap.GetNamespaceUri(prefix).ToString());
                    }
                    else
                    {
                        throw new RdfParseException("Malformed XML Literal - the undefined namespace prefix '" + prefix + "' is used");
                    }
                }
                //Add Language (but don't override existing language)
                if (!evalContext.Language.Equals(String.Empty))
                {
                    if (!n.Attributes.Contains("xml:lang"))
                    {
                        n.Attributes.Add("xml:lang", evalContext.Language);
                    }
                }

                //Recurse on any child nodes
                foreach (HtmlNode child in n.ChildNodes)
                {
                    this.ProcessXmlLiteral(evalContext, child, noDefaultNamespace);
                }
            }
        }

        private bool IsTerm(String value)
        {
            return XmlSpecsHelper.IsNCName(value);
        }

        private bool IsCurie(RdfAEvaluationContext evalContext, String value)
        {
            if (value.StartsWith(":"))
            {
                String reference = value.Substring(1);
                return evalContext.NamespaceMap.HasNamespace(String.Empty) && IriSpecsHelper.IsIrelativeRef(value);
            }
            else if (value.Contains(':'))
            {
                String prefix = value.Substring(0, value.IndexOf(':'));
                String reference = value.Substring(value.IndexOf(':') + 1);
                return (XmlSpecsHelper.IsNCName(prefix) || prefix.Equals("_")) && evalContext.NamespaceMap.HasNamespace(prefix) && IriSpecsHelper.IsIrelativeRef(reference);
            } 
            else 
            {
                return false;
            }
        }

        /// <summary>
        /// Internal Helper for raising the Warning Event
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void OnWarning(String message)
        {
            RdfReaderWarning d = this.Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event which is raised when there is a non-fatal error with the input being read
        /// </summary>
        public event RdfReaderWarning Warning;
    }
}
