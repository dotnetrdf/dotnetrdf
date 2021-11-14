/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.IO;
using System.Text;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Parsing
{


    /// <summary>
    /// Parser for parsing TriG (Turtle with Named Graphs) RDF Syntax into a Triple Store.
    /// </summary>
    /// <remarks>The Default Graph (if any) will be given the special Uri. <strong>trig:default-graph</strong></remarks>
    public class TriGParser
        : IStoreReader, ITraceableTokeniser, ITokenisingParser
    {
        private bool _tracetokeniser = false;
        private TriGSyntax _syntax = TriGSyntax.MemberSubmission;
        // private TokenQueueMode _queueMode = TokenQueueMode.SynchronousBufferDuringParsing;

        /// <summary>
        /// Creates a TriG Parser than uses the default syntax.
        /// </summary>
        public TriGParser()
        { }

        /// <summary>
        /// Creates a TriG Parser which uses the specified syntax.
        /// </summary>
        /// <param name="syntax">Syntax.</param>
        public TriGParser(TriGSyntax syntax)
        {
            _syntax = syntax;
        }

        /// <summary>
        /// Gets/Sets whether Tokeniser Tracing is used.
        /// </summary>
        public bool TraceTokeniser
        {
            get
            {
                return _tracetokeniser;
            }
            set
            {
                _tracetokeniser = value;
            }
        }

        /// <summary>
        /// Gets/Sets the TriG syntax used.
        /// </summary>
        public TriGSyntax Syntax
        {
            get
            {
                return _syntax;
            }
            set
            {
                _syntax = value;
            }
        }

        /// <summary>
        /// Gets/Sets the token queue mode used.
        /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
        public TokenQueueMode TokenQueueMode { get; set; } = Options.DefaultTokenQueueMode; // TokenQueueMode.SynchronousBufferDuringParsing
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Loads the named Graphs from the TriG input into the given Triple Store.
        /// </summary>
        /// <param name="store">Triple Store to load into.</param>
        /// <param name="filename">File to load from.</param>
        public void Load(ITripleStore store, string filename)
        {
            if (filename == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null file");
            Load(store, new StreamReader(File.OpenRead(filename), Encoding.UTF8), new Uri(Path.GetFullPath(filename)));
        }

        /// <summary>
        /// Loads the named Graphs from the TriG input into the given Triple Store.
        /// </summary>
        /// <param name="store">Triple Store to load into.</param>
        /// <param name="input">Input to load from.</param>
        public void Load(ITripleStore store, TextReader input)
        {
            Load(store, input, null);
        }

        /// <summary>
        /// Loads the named Graphs from the TriG input into the given Triple Store.
        /// </summary>
        /// <param name="store">Triple Store to load into.</param>
        /// <param name="input">Input to load from.</param>
        /// <param name="baseUri">Base URI to use when resolving relative URIs in the input.</param>
        public void Load(ITripleStore store, TextReader input, Uri baseUri)
        {
            if (store == null) throw new RdfParseException("Cannot parse an RDF Dataset into a null store");
            if (input == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null input");
            Load(new StoreHandler(store), input, baseUri, store.UriFactory);
        }

        /// <summary>
        /// Loads the named Graphs from the TriG input using the given RDF Handler.
        /// </summary>
        /// <param name="handler">RDF Handler to use.</param>
        /// <param name="filename">File to load from.</param>
        public void Load(IRdfHandler handler, string filename)
        {
            Load(handler, filename, new Uri(Path.GetFullPath(filename)), UriFactory.Root);
        }

        /// <summary>
        /// Loads an RDF dataset using an RDF handler.
        /// </summary>
        /// <param name="handler">RDF handler to use.</param>
        /// <param name="filename">File to load from.</param>
        /// <param name="uriFactory">URI factory to use.</param>
        public void Load(IRdfHandler handler, string filename, IUriFactory uriFactory)
        {
            Load(handler, filename, new Uri(Path.GetFullPath(filename)), uriFactory);
        }


        /// <summary>
        /// Loads an RDF dataset using an RDF handler.
        /// </summary>
        /// <param name="handler">RDF handler to use.</param>
        /// <param name="filename">File to load from.</param>
        /// <param name="baseUri">Base URI to use when resolving relative URIs in the input.</param>
        /// <param name="uriFactory">URI factory to use.</param>
        public void Load(IRdfHandler handler, string filename, Uri baseUri, IUriFactory uriFactory)
        {
            if (filename == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null file");
            Load(handler, new StreamReader(File.OpenRead(filename), Encoding.UTF8), baseUri, uriFactory);
        }

        /// <summary>
        /// Loads the named Graphs from the TriG input using the given RDF Handler.
        /// </summary>
        /// <param name="handler">RDF Handler to use.</param>
        /// <param name="input">Input to load from.</param>
        public void Load(IRdfHandler handler, TextReader input)
        {
            Load(handler, input, UriFactory.Root);
        }

        /// <summary>
        /// Loads an RDF dataset using and RDF handler.
        /// </summary>
        /// <param name="handler">RDF handler to use.</param>
        /// <param name="input">File to load from.</param>
        /// <param name="uriFactory">URI factory to use.</param>
        public void Load(IRdfHandler handler, TextReader input, IUriFactory uriFactory)
        {
            Load(handler, input, null, uriFactory);
        }

        /// <summary>
        /// Loads an RDF dataset using and RDF handler.
        /// </summary>
        /// <param name="handler">RDF handler to use.</param>
        /// <param name="input">File to load from.</param>
        /// <param name="baseUri">Base URI to use when resolving relative URIs in the input.</param>
        /// <param name="uriFactory">URI factory to use.</param>
        public void Load(IRdfHandler handler, TextReader input, Uri baseUri, IUriFactory uriFactory)
        {
            if (handler == null) throw new RdfParseException("Cannot parse an RDF Dataset using a null handler");
            if (input == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null input");
            if (uriFactory == null) throw new ArgumentNullException(nameof(uriFactory));

            try
            {
                // Create the Parser Context and Invoke the Parser
                ITokeniser tokeniser = _syntax switch
                {
                    TriGSyntax.Original or TriGSyntax.MemberSubmission => new TriGTokeniser(input, _syntax),
                    TriGSyntax.Rdf11 => new TurtleTokeniser(input, TurtleSyntax.W3C, withTrig: true),
                    TriGSyntax.Rdf11Star => new TurtleTokeniser(input, TurtleSyntax.Rdf11Star, withTrig: true),
                    _ => throw new RdfParseException($"Syntax mode {_syntax} is not currently supported by the parser.")
                };
                var context = new TriGParserContext(handler, tokeniser, TokenQueueMode, false,
                        _tracetokeniser, uriFactory)
                    { Syntax = _syntax };
                if (baseUri != null) context.BaseUri = baseUri;
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
                    // No catch actions just cleaning up
                }
            }
        }

        private void Parse(TriGParserContext context)
        {
            try
            {
                context.Handler.StartRdf();

                // Expect a BOF Token
                IToken first = context.Tokens.Dequeue();
                if (first.TokenType != Token.BOF)
                {
                    throw ParserHelper.Error("Unexpected Token '" + first.GetType().ToString() + "' encountered, expected a BOF Token", first);
                }

                // Expect either a Directive or a Graph
                IToken next;
                do
                {
                    next = context.Tokens.Peek();
                    switch (next.TokenType)
                    {
                        case Token.COMMENT:
                            // Discard
                            context.Tokens.Dequeue();
                            break;
                        case Token.EOF:
                            // End of File
                            context.Tokens.Dequeue();
                            break;

                        case Token.BASEDIRECTIVE:
                        case Token.PREFIXDIRECTIVE:
                            // Parse a Directive
                            TryParseDirective(context);
                            if (Syntax is TriGSyntax.Original or TriGSyntax.MemberSubmission)
                            {
                                next = context.Tokens.Dequeue();
                                if (next.TokenType != Token.DOT)
                                {
                                    throw ParserHelper.Error(
                                        $"Expected a '.' after an @prefix or @base declaration. Found a {next.GetType()}.",
                                        next);
                                }
                            }
                            break;

                        case Token.AT:
                            context.Tokens.Dequeue();
                            next = context.Tokens.Peek();
                            if (next.TokenType is Token.PREFIXDIRECTIVE or Token.BASEDIRECTIVE)
                            {
                                TryParseDirective(context);
                                // Expect Turtle-style directives to be terminated with a DOT
                                next = context.Tokens.Dequeue();
                                if (next.TokenType != Token.DOT)
                                {
                                    throw ParserHelper.Error(
                                        $"Expected a '.' after an @prefix or @base declaration.", next);
                                }
                            }
                            else
                            {
                                throw ParserHelper.Error(
                                    $"Unexpected token {next.TokenType}. Expected 'prefix' or 'base' after an @.",
                                    next);
                            }
                            break;

                        case Token.QNAME or Token.URI or Token.LEFTSQBRACKET or Token.LEFTBRACKET when context.Syntax is TriGSyntax.Rdf11Star or TriGSyntax.Rdf11:
                            TryParseTriplesOrGraph(context);
                            break;

                        case Token.QNAME or Token.URI or Token.LEFTSQBRACKET:
                        case Token.LEFTCURLYBRACKET:
                        case Token.GRAPH:
                            // Parse a Graph

                            if (context.Syntax != TriGSyntax.Original)
                            {
                                if (next.TokenType == Token.GRAPH)
                                {
                                    // Discardable marker
                                    context.Tokens.Dequeue();

                                    // In TriG 1.1 specification, GRAPH keyword must be followed by a graph name
                                    next = context.Tokens.Peek();
                                    if (Syntax is TriGSyntax.Rdf11 or TriGSyntax.Rdf11Star &&
                                        next.TokenType is not (Token.URI or Token.QNAME or Token.BLANKNODE or Token.BLANKNODEWITHID or Token.LEFTSQBRACKET))
                                    {
                                        throw ParserHelper.Error(
                                            $"Unexpected Token '{next.GetType()} encountered. Expected a URI or blank node to follow the GRAPH keyword.",
                                            next);
                                    }
                                }

                                // We must take care here because @prefix and @base directives may be Graph scoped so anything visible currently
                                // remains visible and must be restored afterwards but anything inside the Graph is not visible outside of it
                                Uri extBase = context.BaseUri;
                                INamespaceMapper nsmap = new NamespaceMapper(context.Namespaces);

                                TryParseGraph(context);

                                // After we parse the Graph restore the state
                                if (!context.Handler.HandleBaseUri(extBase)) ParserHelper.Stop();
                                context.BaseUri = extBase;
                                context.Namespaces.Clear();
                                foreach (var prefix in nsmap.Prefixes)
                                {
                                    if (!context.Handler.HandleNamespace(prefix, nsmap.GetNamespaceUri(prefix))) ParserHelper.Stop();
                                }
                                context.Namespaces.Import(nsmap);
                            }
                            else
                            {
                                // With the old syntax declarations are file scoped so no need to worry about graph scoping
                                TryParseGraph(context);
                            }
                            break;

                        default:
                            throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered", next);
                    }
                } while (next.TokenType != Token.EOF);
                
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
        /// Tries to parse a directive.
        /// </summary>
        /// <param name="context"></param>
        private void TryParseDirective(TriGParserContext context)
        {
            IToken directive = context.Tokens.Dequeue();
            TryParseDirective(context, directive);
        }

        /// <summary>
        /// Tries to parse directives.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="directive"></param>
        /// <remarks>
        /// This overload is needed because in some cases we may dequeue a token before we know it is a directive.
        /// </remarks>
        private void TryParseDirective(TriGParserContext context, IToken directive)
        {
            // See what type of directive it is
            if (directive.TokenType == Token.BASEDIRECTIVE)
            {
                IToken baseUri = context.Tokens.Dequeue();
                if (baseUri.TokenType == Token.URI)
                {
                    try
                    {
                        var newBase = new Uri(Tools.ResolveUri(baseUri.Value, context.BaseUri.ToSafeString()));
                        context.BaseUri = newBase;
                        RaiseWarning("The @base directive is not valid in all versions of the TriG specification, your data may not be compatible with some older tools which do not support this version of TriG");
                        if (!context.Handler.HandleBaseUri(newBase)) ParserHelper.Stop();
                    }
                    catch (UriFormatException)
                    {
                        throw ParserHelper.Error("The URI '" + baseUri.Value + "' given for the Base URI  is not a valid URI", baseUri);
                    }
                }
                else
                {
                    throw ParserHelper.Error("Unexpected Token '" + baseUri.GetType().ToString() + "' encountered, expected a URI Token after a @base directive", baseUri);
                }
            }
            else if (directive.TokenType == Token.PREFIXDIRECTIVE)
            {
                // Prefix Directive
                IToken prefix = context.Tokens.Dequeue();
                if (prefix.TokenType == Token.PREFIX)
                {
                    IToken uri = context.Tokens.Dequeue();
                    if (uri.TokenType == Token.URI)
                    {
                        // Ensure the Uri is absolute
                        try
                        {
                            var u = new Uri(Tools.ResolveUri(uri.Value, context.BaseUri.ToSafeString()));
                            var pre = (prefix.Value.Equals(":")) ? string.Empty : prefix.Value.Substring(0, prefix.Value.Length-1);
                            context.Namespaces.AddNamespace(pre, u);
                            if (!context.Handler.HandleNamespace(pre, u)) ParserHelper.Stop();
                        }
                        catch (UriFormatException)
                        {
                            throw ParserHelper.Error("The URI '" + uri.Value + "' given for the prefix '" + prefix.Value + "' is not a valid URI", uri);
                        }
                    }
                    else
                    {
                        throw ParserHelper.Error("Unexpected Token '" + prefix.GetType().ToString() + "' encountered, expected a URI Token after a Prefix Token", uri);
                    }
                }
                else
                {
                    throw ParserHelper.Error("Unexpected Token '" + prefix.GetType().ToString() + "' encountered, expected a Prefix Token after a Prefix Directive Token", prefix);
                }
            }
            else
            {
                throw ParserHelper.Error("Unexpected Token '" + directive.GetType().ToString() + "' encountered, expected a Base/Prefix Directive Token", directive);
            }

            if (directive.Value.StartsWith("@"))
            {
                // Expect a DOT to terminate Turtle-style prefixes
                IToken dot = context.Tokens.Dequeue();
                if (dot.TokenType != Token.DOT)
                {
                    throw ParserHelper.Error(
                        "Unexpected Token '" + dot.GetType().ToString() +
                        "' encountered, expected a Dot (Line Terminator) Token to terminate a Base/Prefix Directive",
                        dot);
                }
            }
        }

        /// <summary>
        /// Handles the parsing of an IRI or blank node which may be either a graph name or the subject of a triple.
        /// </summary>
        /// <param name="context"></param>
        private void TryParseTriplesOrGraph(TriGParserContext context)
        {
            IToken next = context.Tokens.Dequeue();
            IRefNode graphOrSubjectNode;
            bool subjectIsBlankNodePropertyList = false;
            bool subjectIsCollection = false;
            switch (next.TokenType)
            {
                case Token.QNAME:
                    graphOrSubjectNode = context.Handler.CreateUriNode(
                        context.UriFactory.Create(Tools.ResolveQName(next.Value, context.Namespaces,
                            context.BaseUri)));
                    break;
                case Token.URI:
                    graphOrSubjectNode =
                        context.Handler.CreateUriNode(context.UriFactory.Create(context.BaseUri, next.Value));
                    break;
                case Token.LEFTSQBRACKET:
                    if (context.Tokens.Peek().TokenType == Token.RIGHTSQBRACKET)
                    {
                        next = context.Tokens.Dequeue();
                        graphOrSubjectNode = context.Handler.CreateBlankNode();
                    }
                    else
                    {
                        graphOrSubjectNode = context.Handler.CreateBlankNode();
                        TryParsePredicateObjectList(context, null, graphOrSubjectNode, false);
                        subjectIsBlankNodePropertyList = true;
                    }
                    break;

                case Token.LEFTBRACKET:
                    graphOrSubjectNode= context.Handler.CreateBlankNode();
                    TryParseCollection(context, null, graphOrSubjectNode);
                    subjectIsCollection = true;
                    break;

                default:
                    throw ParserHelper.Error("Expected an IRI or Blank Node", next);
            }

            next = context.Tokens.Peek();
            if (next.TokenType == Token.LEFTCURLYBRACKET)
            {
                if (subjectIsCollection || subjectIsBlankNodePropertyList)
                {
                    throw ParserHelper.Error("Unexpected token '{'. An RDF collection or blank node property list cannot be used as a graph label.", next);
                }
                // parse graph content
                Uri oldBase = context.BaseUri;
                INamespaceMapper nsMap = new NamespaceMapper(context.Namespaces);
                context.Tokens.Dequeue();
                TryParseGraphContent(context, graphOrSubjectNode);
                context.BaseUri = oldBase;
                context.Namespaces.Clear();
                foreach (var prefix in nsMap.Prefixes)
                {
                    if (!context.Handler.HandleNamespace(prefix, nsMap.GetNamespaceUri(prefix))) ParserHelper.Stop();
                }

                context.Namespaces.Import(nsMap);
            }
            else
            {
                // Parse as triples in the default graph
                TryParsePredicateObjectList(context, null, graphOrSubjectNode, subjectIsBlankNodePropertyList);
                // Expect a Dot to Terminate
                if (context.Tokens.LastTokenType != Token.DOT && context.Tokens.LastTokenType != Token.RIGHTCURLYBRACKET)
                {
                    // We only do this if we haven't returned because we already hit the Dot Token/Right Curly Bracket
                    IToken dot = context.Tokens.Dequeue();
                    if (dot.TokenType != Token.DOT && dot.TokenType != Token.RIGHTCURLYBRACKET)
                    {
                        throw ParserHelper.Error("Unexpected Token '" + dot.GetType().ToString() + "' encountered, expected a Dot (Line Terminator) Token to terminate Triples", dot);
                    }
                }
            }
        }

        private void TryParseGraph(TriGParserContext context)
        {
            // Is there a name for the Graph?
            IToken next = context.Tokens.Dequeue();
            IRefNode graphNode;
            switch (next.TokenType)
            {
                case Token.QNAME:
                    // Try to resolve the QName
                    graphNode = context.Handler.CreateUriNode(context.UriFactory.Create(Tools.ResolveQName(next.Value, context.Namespaces, null)));

                    // Get the Next Token
                    next = context.Tokens.Dequeue();
                    break;

                case Token.URI:
                    try
                    {
                        // Ensure an absolute Uri
                        graphNode = context.Handler.CreateUriNode(new Uri(next.Value, UriKind.Absolute));
                    }
                    catch (UriFormatException)
                    {
                        throw ParserHelper.Error("The URI '" + next.Value + "' given as a Graph Name is not a valid Absolute URI", next);
                    }

                    // Get the Next Token
                    next = context.Tokens.Dequeue();
                    break;
                
                case Token.BLANKNODE:
                    graphNode = context.Handler.CreateBlankNode();
                    // Get the Next Token
                    next = context.Tokens.Dequeue();
                    break;
                
                case Token.BLANKNODEWITHID:
                    graphNode = context.Handler.CreateBlankNode(next.Value);
                    // Get the Next Token
                    next = context.Tokens.Dequeue();
                    break;

                case Token.LEFTSQBRACKET:
                    // Must be followed immediately by a ] as a blank node declaration
                    next = context.Tokens.Dequeue();
                    if (next.TokenType != Token.RIGHTSQBRACKET)
                    {
                        throw ParserHelper.Error(
                            $"Expected a ] to end an anonymous blank node graph name declaration. Found {next}.", next);
                    }

                    graphNode = context.Handler.CreateBlankNode();
                    next = context.Tokens.Dequeue();
                    break;

                default:
                    {
                        // No Name so is a Default Graph
                        if (!context.DefaultGraphExists)
                        {
                            graphNode = null;
                        }
                        else
                        {
                            throw new RdfParseException("You cannot specify more than one Default (Unnamed) Graph in a TriG file", next);
                        }

                        break;
                    }
            }

            // Is there a discardable Equals token?
            if (next.TokenType == Token.EQUALS)
            {
                next = context.Tokens.Dequeue();
            }

            // Should the see a Left Curly Bracket
            if (next.TokenType != Token.LEFTCURLYBRACKET) {
                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Left Curly Bracket to start a Graph", next);
            }

            TryParseGraphContent(context, graphNode);
        }

        private void TryParseGraphContent(TriGParserContext context, IRefNode graphNode)
        {
            // Check that the Graph isn't empty i.e. the next token is not a } to close the Graph
            IToken next = context.Tokens.Peek();
            if (next.TokenType == Token.RIGHTCURLYBRACKET)
            {
                // Empty Graph so just discard the }
                context.Tokens.Dequeue();
            }
            else
            {
                // Parse Graph Contents
                TryParseTriples(context, graphNode);
            }

            // May optionally end with a Dot Token
            next = context.Tokens.Peek();
            if (next.TokenType == Token.DOT)
            {
                // Discard
                context.Tokens.Dequeue();
            }
        }

        private void TryParseTriples(TriGParserContext context, IRefNode graphNode)
        {
            do
            {
                // Try to get the Subject
                IToken subj = context.Tokens.Dequeue();
                IRefNode subjNode;
                bool parsedBlankNodePropertyList = false;

                // Turn the Subject Token into a Node
                switch (subj.TokenType)
                {
                    case Token.COMMENT:
                        // Discard and continue
                        continue;

                    case Token.QNAME:
                    case Token.URI:
                        subjNode = ParserHelper.TryResolveUri(context, subj);
                        break;

                    case Token.BLANKNODEWITHID:
                        // Blank Node with ID
                        subjNode = context.Handler.CreateBlankNode(subj.Value.Substring(2));
                        break;

                    case Token.LEFTSQBRACKET:
                        // Blank Node
                        IToken next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTSQBRACKET)
                        {
                            // Anonymous Blank Node
                            context.Tokens.Dequeue();
                            subjNode = context.Handler.CreateBlankNode();
                        }
                        else
                        {
                            // Blank Node Collection
                            subjNode = context.Handler.CreateBlankNode();

                            // Do an extra call to TryParsePredicateObjectList to parse the Blank Node Collection
                            TryParsePredicateObjectList(context, graphNode, subjNode, false);
                            parsedBlankNodePropertyList = true;
                        }
                        break;

                    case Token.LEFTBRACKET:
                        // Collection

                        // Check whether an Empty Collection
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTBRACKET)
                        {
                            // Empty Collection
                            context.Tokens.Dequeue();
                            subjNode = context.Handler.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfListNil));
                        }
                        else
                        {
                            // Collection
                            subjNode = context.Handler.CreateBlankNode();
                            TryParseCollection(context, graphNode, subjNode);
                        }
                        break;

                    case Token.PREFIXDIRECTIVE:
                    case Token.BASEDIRECTIVE:
                        if (context.Syntax is TriGSyntax.Original or TriGSyntax.Rdf11 or TriGSyntax.Rdf11Star)
                        {
                            throw ParserHelper.Error("@base/@prefix directives are not permitted to occur inside a Graph in this version of TriG, other versions of TriG support this feature and may be enabled by changing your syntax setting when you create a TriG Parser", subj);
                        }

                        // Parse the directive then continue
                        TryParseDirective(context, subj);
                        next = context.Tokens.Dequeue();
                        if (next.TokenType != Token.DOT)
                        {
                            throw ParserHelper.Error(
                                $"Expected a `.` to follow a @base or @prefix directive, but found a {next.GetType()}",
                                next);
                        }
                        continue;

                    case Token.EOF:
                        throw ParserHelper.Error("Unexpected End of File while trying to parse Triples", subj);

                    default:
                        // Unexpected Token
                        throw ParserHelper.Error("Unexpected Token '" + subj.GetType().ToString() + "' encountered, expected a URI/QName/Blank Node as the Subject of a Triple", subj);
                }

                
                // Parse the Predicate Object List
                TryParsePredicateObjectList(context, graphNode, subjNode, parsedBlankNodePropertyList);
            

                // Expect a Dot to Terminate
                if (context.Tokens.LastTokenType != Token.DOT && context.Tokens.LastTokenType != Token.RIGHTCURLYBRACKET)
                {
                    // We only do this if we haven't returned because we already hit the Dot Token/Right Curly Bracket
                    IToken dot = context.Tokens.Dequeue();
                    if (dot.TokenType != Token.DOT && dot.TokenType != Token.RIGHTCURLYBRACKET)
                    {
                        throw ParserHelper.Error("Unexpected Token '" + dot.GetType().ToString() + "' encountered, expected a Dot (Line Terminator) Token to terminate Triples", dot);
                    }
                }

                // If we already hit the Right Curly Bracket return
                if (context.Tokens.LastTokenType == Token.RIGHTCURLYBRACKET) return;

            } while (context.Tokens.Peek().TokenType != Token.RIGHTCURLYBRACKET);

            // Discard the ending Right Curly Bracket
            context.Tokens.Dequeue();
        }

        private void TryParsePredicateObjectList(TriGParserContext context, IRefNode graphNode, IRefNode subj, bool emptyOk)
        {
            var ok = false;
            if (emptyOk && context.Tokens.Peek().TokenType is Token.DOT or Token.RIGHTCURLYBRACKET)
            {
                // Empty predicate object list
                return;
            }
            do
            {
                // After our first run through we'll need to discard semicolons here
                if (ok)
                {
                    context.Tokens.Dequeue();

                    // Watch out for Trailing Semicolons
                    if (context.Tokens.Peek().TokenType == Token.RIGHTSQBRACKET)
                    {
                        // Allow trailing semicolons to terminate Blank Node Collections
                        context.Tokens.Dequeue();
                        return;
                    }

                    while (context.Tokens.Peek().TokenType == Token.SEMICOLON)
                    {
                        context.Tokens.Dequeue();
                    }
                }

                // Try to get the Predicate
                IToken pred = context.Tokens.Dequeue();
                IRefNode predNode;

                switch (pred.TokenType)
                {
                    case Token.COMMENT:
                        // Discard and continue
                        ok = false;
                        continue;

                    case Token.QNAME:
                    case Token.URI:
                        predNode = ParserHelper.TryResolveUri(context, pred);
                        break;

                    case Token.KEYWORDA:
                        // 'a' Keyword
                        predNode = context.Handler.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
                        break;

                    case Token.EOF:
                        throw ParserHelper.Error("Unexpected End of File while trying to parse Predicate Object list", pred);

                    case Token.DOT:
                    case Token.RIGHTCURLYBRACKET:
                        if (!ok)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + pred.GetType().ToString() + "' encountered before a Predicate Object list was parsed", pred);
                        }
                        return;

                    case Token.RIGHTSQBRACKET:
                        if (!ok)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + pred.GetType().ToString() + "' encountered before a Predicate Object list of a Blank Node Collection was parsed", pred);
                        }
                        return;

                    default:
                        // Unexpected Token
                        throw ParserHelper.Error("Unexpected Token '" + pred.GetType().ToString() + "' encountered, expected a URI/QName as the Predicate of a Triple", pred);
                }

                ok = true;

                // Parse the Object List
                TryParseObjectList(context, graphNode, subj, predNode);

                // Return if we hit the Dot Token/Right Curly Bracket/Right Square Bracket
                if (context.Tokens.LastTokenType == Token.DOT ||
                    context.Tokens.LastTokenType == Token.RIGHTCURLYBRACKET ||
                    context.Tokens.LastTokenType == Token.RIGHTSQBRACKET)
                {
                    return;
                }

                // Check for End of Blank Node Collections
                if (context.Tokens.Peek().TokenType == Token.RIGHTSQBRACKET)
                {
                    context.Tokens.Dequeue();
                    return;
                }

            } while (context.Tokens.Peek().TokenType == Token.SEMICOLON); //Expect a semicolon if we are to continue
        }

        private void TryParseObjectList(TriGParserContext context, IRefNode graphNode, IRefNode subj, IRefNode pred)
        {
            var ok = false;

            do
            {
                // After the first run through we'll need to discard commas here
                if (ok)
                {
                    context.Tokens.Dequeue();
                }

                // Try to get the Object
                IToken obj = context.Tokens.Dequeue();
                IToken next;
                INode objNode;

                switch (obj.TokenType)
                {
                    case Token.COMMENT:
                        // Discard and Continue
                        ok = false;
                        continue;

                    case Token.QNAME:
                    case Token.URI:
                        objNode = ParserHelper.TryResolveUri(context, obj);
                        break;

                    case Token.LITERAL:
                    case Token.LONGLITERAL:
                        // Literals

                        // See whether we get a Language Specifier/Data Type next
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.LANGSPEC)
                        {
                            // Literal with Language Specifier
                            context.Tokens.Dequeue();
                            objNode = context.Handler.CreateLiteralNode(obj.Value, next.Value);
                        }
                        else if (next.TokenType == Token.HATHAT)
                        {
                            // Literal with DataType
                            context.Tokens.Dequeue();
                            // Now expect a QName/Uri Token
                            next = context.Tokens.Dequeue();
                            if (next.TokenType == Token.QNAME || next.TokenType == Token.URI)
                            {
                                Uri dt = context.UriFactory.Create(Tools.ResolveUriOrQName(next, context.Namespaces, context.BaseUri));
                                objNode = context.Handler.CreateLiteralNode(obj.Value, dt);
                            }
                            else
                            {
                                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a URI/QName Token to specify a Data Type after a ^^ Token", next);
                            }
                        }
                        else if (next.TokenType == Token.DATATYPE)
                        {
                            // In RDF 1.1 mode, the Turtle tokenizer returns a DATATYPE token directly
                            context.Tokens.Dequeue();
                            Uri dt = context.UriFactory.Create(Tools.ResolveUriOrQName(next, context.Namespaces,
                                context.BaseUri));
                            objNode = context.Handler.CreateLiteralNode(obj.Value, dt);
                        }
                        else
                        {
                            // Just a string literal
                            objNode = context.Handler.CreateLiteralNode(obj.Value);
                        }
                        break;

                    case Token.PLAINLITERAL:
                        // Plain Literals
                        Uri plt = TurtleSpecsHelper.InferPlainLiteralType((PlainLiteralToken)obj, TurtleSyntax.Original);
                        objNode = context.Handler.CreateLiteralNode(obj.Value, plt);
                        break;

                    case Token.BLANKNODEWITHID:
                        // Blank Node with ID
                        objNode = context.Handler.CreateBlankNode(obj.Value.Substring(2));
                        break;

                    case Token.LEFTSQBRACKET:
                        // Blank Node
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTSQBRACKET)
                        {
                            // Anonymous Blank Node
                            context.Tokens.Dequeue();
                            objNode = context.Handler.CreateBlankNode();
                        }
                        else
                        {
                            // Blank Node Collection
                            objNode = context.Handler.CreateBlankNode();

                            // Do an extra call to TryParsePredicateObjectList to parse the Blank Node Collection
                            TryParsePredicateObjectList(context, graphNode, (IBlankNode)objNode, false);
                        }
                        break;

                    case Token.RIGHTSQBRACKET:
                        // End of Blank Node Collection
                        if (!ok)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + obj.GetType().ToString() + "' encountered before an Object list of a Blank Node Collection was parsed", obj);
                        }
                        return;

                    case Token.LEFTBRACKET:
                        // Collection

                        // Check whether an Empty Collection
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTBRACKET)
                        {
                            // Empty Collection
                            context.Tokens.Dequeue();
                            objNode = context.Handler.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfListNil));
                        }
                        else
                        {
                            // Collection
                            objNode = context.Handler.CreateBlankNode();
                            TryParseCollection(context, graphNode, (IBlankNode)objNode);
                        }
                        break;

                    case Token.EOF:
                        throw ParserHelper.Error("Unexpected End of File while trying to parse Object List", obj);

                    case Token.DOT:
                    case Token.RIGHTCURLYBRACKET:
                    case Token.SEMICOLON:
                        if (!ok)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + obj.GetType().ToString() + "' encountered before an Object list was parsed", obj);
                        }
                        return;

                    default:
                        // Unexpected Token
                        throw ParserHelper.Error("Unexpected Token '" + obj.GetType().ToString() + "' encountered, expected a URI/QName/Blank Node as the Object of a Triple", obj);
                }

                ok = true;

                if (!context.Handler.HandleTriple(new Triple(subj, pred, objNode, graphNode))) ParserHelper.Stop();

            } while (context.Tokens.Peek().TokenType == Token.COMMA); //Expect a comma if we are to continue
        }

        private void TryParseCollection(TriGParserContext context, IRefNode graphNode, IRefNode subj)
        {
            // Create the Nodes we need
            IUriNode rdfFirst, rdfRest, rdfNil;
            rdfFirst = context.Handler.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfListFirst));
            rdfRest = context.Handler.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfListRest));
            rdfNil = context.Handler.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfListNil));

            IToken next;
            INode item, temp;
            item = null;
            do
            {
                next = context.Tokens.Dequeue();

                // Create a Node for this Token
                switch (next.TokenType)
                {
                    case Token.COMMENT:
                        // Discard and continue;
                        continue;

                    case Token.QNAME:
                    case Token.URI:
                        item = ParserHelper.TryResolveUri(context, next);
                        break;

                    case Token.LITERAL:
                    case Token.LONGLITERAL:
                        IToken obj = next;
                        next = context.Tokens.Peek();
                        switch (next.TokenType)
                        {
                            case Token.LANGSPEC:
                                context.Tokens.Dequeue();
                                item = context.Handler.CreateLiteralNode(obj.Value, next.Value);
                                break;
                            case Token.HATHAT:
                                context.Tokens.Dequeue();
                                next = context.Tokens.Dequeue();
                                if (next.TokenType == Token.QNAME || next.TokenType == Token.URI)
                                {
                                    Uri dt = context.UriFactory.Create(Tools.ResolveUriOrQName(next, context.Namespaces, context.BaseUri));
                                    item = context.Handler.CreateLiteralNode(obj.Value, dt);
                                }
                                else
                                {
                                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a URI/QName Token to specify a Data Type after a ^^ Token", next);
                                }
                                break;
                            default:
                                item = context.Handler.CreateLiteralNode(obj.Value);
                                break;
                        }
                        break;
                    case Token.PLAINLITERAL:
                        Uri plt = TurtleSpecsHelper.InferPlainLiteralType((PlainLiteralToken)next, TurtleSyntax.Original);
                        item = context.Handler.CreateLiteralNode(next.Value, plt);
                        break;

                    case Token.LEFTSQBRACKET:
                        // Check whether an anonymous Blank Node or a Blank Node Collection
                        item = context.Handler.CreateBlankNode();

                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTSQBRACKET)
                        {
                            // Anonymous Blank Node
                            context.Tokens.Dequeue();
                        }
                        else
                        {
                            // Blank Node Collection
                            TryParsePredicateObjectList(context, graphNode, (IBlankNode)item, false);
                        }
                        break;

                    case Token.LEFTBRACKET:
                        // Check whether an Empty Collection
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTBRACKET)
                        {
                            // Empty Collection
                            context.Tokens.Dequeue();
                            item = context.Handler.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfListNil));
                        }
                        else
                        {
                            // Collection
                            item = context.Handler.CreateBlankNode();
                            TryParseCollection(context, graphNode, (IBlankNode)item);
                        }
                        break;

                    case Token.BLANKNODEWITHID:
                        item = context.Handler.CreateBlankNode(next.Value);
                        break;

                    case Token.EOF:
                        throw ParserHelper.Error("Unexpected End of File while trying to parse a Collection", next);

                    default:
                        // Unexpected Token
                        throw ParserHelper.Error(
                            $"Unexpected Token '{next.GetType()}' encountered, expected a URI/QName/Literal/Blank Node as an item in a Collection", next);
                }

                // Create the subj rdf:first item Triple
                if (!context.Handler.HandleTriple((new Triple(subj, rdfFirst, item, graphNode)))) ParserHelper.Stop();

                // Create the rdf:rest Triple
                if (context.Tokens.Peek().TokenType == Token.RIGHTBRACKET)
                {
                    // End of Collection
                    context.Tokens.Dequeue();
                    if (!context.Handler.HandleTriple(new Triple(subj, rdfRest, rdfNil, graphNode))) ParserHelper.Stop();
                    return;
                }
                else
                {
                    // Continuing Collection
                    temp = context.Handler.CreateBlankNode();
                    if (!context.Handler.HandleTriple(new Triple(subj, rdfRest, temp, graphNode))) ParserHelper.Stop();
                    subj = (IBlankNode)temp;
                }
            } while (true);

        }

        /// <summary>
        /// Helper method used to raise the Warning event if there is an event handler registered.
        /// </summary>
        /// <param name="message">Warning message.</param>
        private void RaiseWarning(string message)
        {
            StoreReaderWarning d = Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event which Readers can raise when they notice syntax that is ambigious/deprecated etc which can still be parsed
        /// </summary>
        public event StoreReaderWarning Warning;

        /// <summary>
        /// Gets the String representation of the Parser which is a description of the syntax it parses.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "TriG";
        }
    }
}
