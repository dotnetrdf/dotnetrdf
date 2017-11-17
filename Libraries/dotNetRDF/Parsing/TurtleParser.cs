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
using System.IO;
using System.Text;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Parser for Turtle syntax
    /// </summary>
    /// <threadsafety instance="true">Designed to be Thread Safe - should be able to call Load from multiple threads on different Graphs without issue</threadsafety>
    public class TurtleParser 
        : IRdfReader, ITraceableParser, ITraceableTokeniser, ITokenisingParser
    {
        private bool _traceParsing = false;
        private bool _traceTokeniser = false;
        private TokenQueueMode _queueMode = Options.DefaultTokenQueueMode;
        private TurtleSyntax _syntax = TurtleSyntax.W3C;

        /// <summary>
        /// Creates a new Turtle Parser
        /// </summary>
        public TurtleParser() { }

        /// <summary>
        /// Creates a new Turtle Parser
        /// </summary>
        /// <param name="syntax">Turtle Syntax</param>
        public TurtleParser(TurtleSyntax syntax) 
        {
            _syntax = syntax;
        }

        /// <summary>
        /// Creates a new Turtle Parser which uses the given Token Queue Mode
        /// </summary>
        /// <param name="queueMode">Queue Mode for Turtle</param>
        public TurtleParser(TokenQueueMode queueMode)
        {
            _queueMode = queueMode;
        }

        /// <summary>
        /// Creates a new Turtle Parser which uses the given Token Queue Mode
        /// </summary>
        /// <param name="queueMode">Queue Mode for Turtle</param>
        /// <param name="syntax">Turtle Syntax</param>
        public TurtleParser(TokenQueueMode queueMode, TurtleSyntax syntax)
            : this(syntax)
        {
            _queueMode = queueMode;
        }

        /// <summary>
        /// Gets/Sets whether Parsing Trace is written to the Console
        /// </summary>
        public bool TraceParsing
        {
            get
            {
                return _traceParsing;
            }
            set
            {
                _traceParsing = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether Tokeniser Trace is written to the Console
        /// </summary>
        public bool TraceTokeniser
        {
            get
            {
                return _traceTokeniser;
            }
            set
            {
                _traceTokeniser = value;
            }
        }

        /// <summary>
        /// Gets/Sets the token queue mode used
        /// </summary>
        public TokenQueueMode TokenQueueMode
        {
            get
            {
                return _queueMode;
            }
            set
            {
                _queueMode = value;
            }
        }

        /// <summary>
        /// Loads a Graph by reading Turtle syntax from the given input
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="input">Stream to read from</param>
        public void Load(IGraph g, StreamReader input)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Loads a Graph by reading Turtle syntax from the given input
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="input">Input to read from</param>
        public void Load(IGraph g, TextReader input)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Loads a Graph by reading Turtle syntax from the given file
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="filename">File to read from</param>
        public void Load(IGraph g, string filename)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");
            Load(g, new StreamReader(File.OpenRead(filename), Encoding.UTF8));
        }

        /// <summary>
        /// Loads RDF by reading Turtle syntax from the given input using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handle to use</param>
        /// <param name="input">Stream to read from</param>
        public void Load(IRdfHandler handler, StreamReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannot read RDF from a null Stream");

            // Issue a Warning if the Encoding of the Stream is not UTF-8
            if (!input.CurrentEncoding.Equals(Encoding.UTF8))
            {
                RaiseWarning("Expected Input Stream to be encoded as UTF-8 but got a Stream encoded as " + input.CurrentEncoding.EncodingName + " - Please be aware that parsing errors may occur as a result");
            }

            Load(handler, (TextReader)input);
        }

        /// <summary>
        /// Loads RDF by reading Turtle syntax from the given input using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handle to use</param>
        /// <param name="input">Input to read from</param>
        public void Load(IRdfHandler handler, TextReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannot read RDF from a null TextReader");

            try
            {
                TurtleParserContext context = new TurtleParserContext(handler, new TurtleTokeniser(input, _syntax), _syntax, _queueMode, _traceParsing, _traceTokeniser);
                Parse(context);
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
                    // Catch is just here in case something goes wrong with closing the stream
                    // This error can be ignored
                }
            }
        }

        /// <summary>
        /// Loads RDF by reading Turtle syntax from the given file using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handle to use</param>
        /// <param name="filename">File to read from</param>
        public void Load(IRdfHandler handler, String filename)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");
            Load(handler, new StreamReader(File.OpenRead(filename), Encoding.UTF8));
        }

        /// <summary>
        /// Internal method which does the parsing of the input
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void Parse(TurtleParserContext context)
        {
            try
            {
                context.Handler.StartRdf();

                // Initialise Buffer and start parsing
                context.Tokens.InitialiseBuffer(10);

                IToken next = context.Tokens.Dequeue();
                if (next.TokenType != Token.BOF)
                {
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a BOF Token", next);
                }

                do
                {
                    next = context.Tokens.Peek();

                    switch (next.TokenType)
                    {
                        case Token.AT:
                            // Turtle style Base/Prefix directive
                            TryParseDirective(context, true);
                            break;

                        case Token.COMMENT:
                            // Discard and ignore
                            context.Tokens.Dequeue();
                            break;

                        case Token.BLANKNODE:
                        case Token.BLANKNODEWITHID:
                        case Token.LEFTBRACKET:
                        case Token.LEFTSQBRACKET:
                        case Token.QNAME:
                        case Token.URI:
                            // Valid Subject of a Triple
                            TryParseTriples(context);
                            break;

                        case Token.LITERAL:
                        case Token.LITERALWITHDT:
                        case Token.LITERALWITHLANG:
                        case Token.LONGLITERAL:
                            // Literals not valid as Subjects
                            throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, Literals are not valid as Subjects in Turtle", next);

                        case Token.KEYWORDA:
                            // 'a' Keyword only valid as Predicate
                            throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, the 'a' Keyword is only valid as a Predicate in Turtle", next);

                        case Token.PREFIXDIRECTIVE:
                        case Token.BASEDIRECTIVE:
                            // SPARQL style Base/Prefix directive
                            TryParseDirective(context, false);
                            break;

                        case Token.EOF:
                            // OK - the loop will now terminate since we've seen the End of File
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
        /// Tries to parse Base/Prefix declarations
        /// </summary>
        /// <param name="context">Parse Context</param>
        /// <param name="turtleStyle">Whether declarations are Turtle style (if false SPARQL style is assumed)</param>
        private void TryParseDirective(TurtleParserContext context, bool turtleStyle)
        {
            if (context.TraceParsing)
            {
                Console.WriteLine("Attempting to parse a Base/Prefix Declaration");
            }

            if (!turtleStyle && context.Syntax == TurtleSyntax.Original) throw ParserHelper.Error("SPARQL style BASE/PREFIX declarations are not permitted in this Turtle syntax", context.Tokens.Dequeue());

            // If we've been called with turtleStyle set then an AT token has been encountered which we can discard
            if (turtleStyle) context.Tokens.Dequeue();

            // Then we expect either a Base Directive/Prefix Directive
            IToken directive = context.Tokens.Dequeue();
            if (directive.TokenType == Token.BASEDIRECTIVE)
            {
                // Then expect a Uri for the Base Uri
                IToken u = context.Tokens.Dequeue();
                if (u.TokenType == Token.URI)
                {
                    // Set the Base Uri resolving against the current Base if any
                    try
                    {
                        Uri baseUri = UriFactory.Create(Tools.ResolveUri(u.Value, context.BaseUri.ToSafeString()));
                        context.BaseUri = baseUri;
                        if (!context.Handler.HandleBaseUri(baseUri)) ParserHelper.Stop();
                    }
                    catch (RdfException rdfEx)
                    {
                        throw new RdfParseException("Unable to set the Base URI to '" + u.Value + "' due to the following error:\n" + rdfEx.Message, u, rdfEx);
                    }
                }
                else
                {
                    throw ParserHelper.Error("Unexpected Token '" + u.GetType().ToString() + "' encountered, expected a URI after a Base Directive", u);
                }
            }
            else if (directive.TokenType == Token.PREFIXDIRECTIVE)
            {
                // Expect a Prefix then a Uri
                IToken pre = context.Tokens.Dequeue();
                if (pre.TokenType == Token.PREFIX)
                {
                    IToken ns = context.Tokens.Dequeue();
                    if (ns.TokenType == Token.URI)
                    {
                        // Register a Namespace resolving the Namespace Uri against the Base Uri
                        try
                        {
                            Uri nsUri = UriFactory.Create(Tools.ResolveUri(ns.Value, context.BaseUri.ToSafeString()));
                            String nsPrefix = (pre.Value.Length > 1) ? pre.Value.Substring(0, pre.Value.Length - 1) : String.Empty;
                            context.Namespaces.AddNamespace(nsPrefix, nsUri);
                            if (!context.Handler.HandleNamespace(nsPrefix, nsUri)) ParserHelper.Stop();
                        }
                        catch (RdfException rdfEx)
                        {
                            throw new RdfParseException("Unable to resolve the Namespace URI '" + ns.Value + "' due to the following error:\n" + rdfEx.Message, ns, rdfEx);
                        }
                    }
                    else
                    {
                        throw ParserHelper.Error("Unexpected Token '" + ns.GetType().ToString() + "' encountered, expected a URI after a Prefix Directive", pre);
                    }
                }
                else
                {
                    throw ParserHelper.Error("Unexpected Token '" + pre.GetType().ToString() + "' encountered, expected a Prefix after a Prefix Directive", pre);
                }
            }
            else
            {
                throw ParserHelper.Error("Unexpected Token '" + directive.GetType().ToString() + "' encountered, expected a Base/Prefix Directive after an @ symbol", directive);
            }

            // Turtle sytle declarations are terminated with a Dot
            IToken terminator = context.Tokens.Peek();
            if (terminator.TokenType != Token.DOT)
            {
                // If Turtle style the terminating . is required
                if (turtleStyle) throw ParserHelper.Error("Unexpected Token '" + terminator.GetType().ToString() + "' encountered, expected a . to terminate a Base/Prefix Directive", terminator);
            }
            else if (turtleStyle)
            {
                // Discard the . terminator for Turtle sytle declarations
                context.Tokens.Dequeue();
            }
            else
            {
                // For SPARQL style the . terminator is forbidden
                throw ParserHelper.Error("Unexpected Token '" + terminator.GetType().ToString() + "' encountered, a . is not permitted/required to terminate a SPARQL style Base/Prefix Directive", terminator);
            }
        }

        /// <summary>
        /// Tries to parse Triples
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void TryParseTriples(TurtleParserContext context)
        {
            IToken subjToken = context.Tokens.Dequeue();
            IToken next;
            INode subj;

            if (context.TraceParsing)
            {
                Console.WriteLine("Attempting to parse Triples from the Subject Token '" + subjToken.GetType().ToString() + "'");
            }

            switch (subjToken.TokenType)
            {
                case Token.BLANKNODE:
                    subj = context.Handler.CreateBlankNode();
                    break;

                case Token.BLANKNODEWITHID:
                    subj = context.Handler.CreateBlankNode(subjToken.Value.Substring(2));
                    break;

                case Token.LEFTBRACKET:
                    // Start of a collection so create a new Blank Node to be it's first subject
                    next = context.Tokens.Peek();
                    if (next.TokenType == Token.RIGHTBRACKET)
                    {
                        // An Empty Collection => rdf:nil
                        context.Tokens.Dequeue();
                        subj = context.Handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListNil));
                    }
                    else
                    {
                        subj = context.Handler.CreateBlankNode();
                        TryParseCollection(context, subj);
                    }
                    break;

                case Token.LEFTSQBRACKET:
                    // Start of a Blank Node collection?
                    next = context.Tokens.Peek();
                    if (next.TokenType == Token.RIGHTSQBRACKET)
                    {
                        // An anoynmous Blank Node
                        context.Tokens.Dequeue();
                        subj = context.Handler.CreateBlankNode();
                    }
                    else
                    {
                        // Start of a Blank Node Collection
                        subj = context.Handler.CreateBlankNode();
                        TryParsePredicateObjectList(context, subj, true);

                        // In W3C Turtle we are allowed to have a dot to terminate a top level blank node predicate list
                        if (_syntax == TurtleSyntax.W3C)
                        {
                            next = context.Tokens.Peek();
                            if (next.TokenType == Token.DOT)
                            {
                                context.Tokens.Dequeue();
                                return;
                            }
                        }
                    }
                    break;

                case Token.QNAME:
                case Token.URI:
                    subj = ParserHelper.TryResolveUri(context, subjToken, false, context.QNameUnescapeFunction);
                    break;

                default:
                    throw ParserHelper.Error("Unexpected Token '" + subjToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a Triple", subjToken);
            }

            TryParsePredicateObjectList(context, subj, false);
        }

        /// <summary>
        /// Tries to parse Predicate Object lists
        /// </summary>
        /// <param name="context">Parse Context</param>
        /// <param name="subj">Subject of the Triples</param>
        /// <param name="bnodeList">Whether this is a Blank Node Predicate Object list</param>
        private void TryParsePredicateObjectList(TurtleParserContext context, INode subj, bool bnodeList)
        {
            IToken predToken;
            INode pred = null;

            do
            {
                predToken =  context.Tokens.Dequeue();

                if (context.TraceParsing)
                {
                    Console.WriteLine("Attempting to parse Predicate Object List from the Predicate Token '" + predToken.GetType().ToString() + "'");
                }

                switch (predToken.TokenType)
                {
                    case Token.COMMENT:
                        // Discard and continue
                        continue;

                    case Token.KEYWORDA:
                        // 'a' Keyword
                        pred = context.Handler.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "type"));
                        break;

                    case Token.RIGHTSQBRACKET:
                        // If the last token was a semicolon and we're parsing a Blank Node Predicate Object list
                        // then a trailing semicolon is permitted
                        if (bnodeList)
                        {
                            if (context.Tokens.LastTokenType == Token.SEMICOLON)
                            {
                                return;
                            }
                            else
                            {
                                // If Predicate is not null then we've seen at least one valid Triple and this is just the end of the Blank Node Predicate Object list
                                if (pred != null)
                                {
                                    return;
                                }
                                else
                                {
                                    throw ParserHelper.Error("Unexpected Right Square Bracket encountered while trying to parse a Blank Node Predicate Object list, expected a valid Predicate", predToken);
                                }
                            }
                        }
                        else
                        {
                            throw ParserHelper.Error("Unexpected Right Square Bracket encountered while trying to parse a Predicate Object list", predToken);
                        }

                    case Token.QNAME:
                    case Token.URI:
                        pred = ParserHelper.TryResolveUri(context, predToken, false, context.QNameUnescapeFunction);
                        break;

                    case Token.EOF:
                        throw ParserHelper.Error("Unexpected end of file while trying to parse a Predicate Object list", predToken);

                    case Token.SEMICOLON:
                        if (_syntax == TurtleSyntax.Original) goto default;

                        // May get a sequence of semicolons
                        IToken next = context.Tokens.Peek();
                        while (next.TokenType == Token.SEMICOLON)
                        {
                            context.Tokens.Dequeue();
                            next = context.Tokens.Peek();
                        }
                        // Bail out of these are followed by a DOT
                        if (next.TokenType == Token.DOT && !bnodeList)
                        {
                            context.Tokens.Dequeue();
                            return;
                        }
                        TryParsePredicateObjectList(context, subj, bnodeList);
                        return;

                    default:
                        throw ParserHelper.Error("Unexpected Token '" + predToken.GetType().ToString() + "' encountered while trying to parse a Predicate Object list", predToken);

                }

                TryParseObjectList(context, subj, pred, bnodeList);
                if (context.Tokens.LastTokenType == Token.DOT && !bnodeList) return; //Dot terminates a normal Predicate Object list
                if (context.Tokens.LastTokenType == Token.RIGHTSQBRACKET && bnodeList) return; //Trailing semicolon may terminate a Blank Node Predicate Object list
                if (context.Tokens.LastTokenType == Token.SEMICOLON && context.Tokens.Peek().TokenType == Token.DOT)
                {
                    // Dot terminates a Predicate Object list with a trailing semicolon
                    context.Tokens.Dequeue();
                    return; 
                }
            } while (true);
        }

        /// <summary>
        /// Tries to parse Object lists
        /// </summary>
        /// <param name="context">Parse Context</param>
        /// <param name="subj">Subject of the Triples</param>
        /// <param name="pred">Predicate of the Triples</param>
        /// <param name="bnodeList">Whether this is a Blank Node Object list</param>
        private void TryParseObjectList(TurtleParserContext context, INode subj, INode pred, bool bnodeList)
        {
            IToken objToken, next;
            INode obj = null;

            do
            {
                objToken = context.Tokens.Dequeue();

                if (context.TraceParsing)
                {
                    Console.WriteLine("Attempting to parse an Object List from the Object Token '" + objToken.GetType().ToString() + "'");
                }

                switch (objToken.TokenType)
                {
                    case Token.BLANKNODE:
                        obj = context.Handler.CreateBlankNode();
                        break;

                    case Token.BLANKNODEWITHID:
                        obj = context.Handler.CreateBlankNode(objToken.Value.Substring(2));
                        break;

                    case Token.COMMA:
                        // Discard and continue - set object to null so we know we're expected to complete a triple
                        if (obj != null)
                        {
                            obj = null;
                            continue;
                        }
                        else
                        {
                            throw ParserHelper.Error("Unexpected Comma Triple terminator encountered, expected a valid Object for the current Triple", objToken);
                        }

                    case Token.COMMENT:
                        // Discard and ignore
                        continue;

                    case Token.DOT:
                        if (obj != null)
                        {
                            // OK to return if we've seen a valid Triple
                            return;
                        }
                        else
                        {
                            throw ParserHelper.Error("Unexpected Dot Triple terminator encountered, expected a valid Object for the current Triple", objToken);
                        }

                    case Token.LEFTBRACKET:
                        // Start of a collection so create a new Blank Node to be it's first subject
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTBRACKET)
                        {
                            // Empty Collection => rdf:nil
                            context.Tokens.Dequeue();
                            obj = context.Handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListNil));
                        }
                        else
                        {
                            obj = context.Handler.CreateBlankNode();
                            TryParseCollection(context, obj);
                        }
                        break;

                    case Token.LEFTSQBRACKET:
                        // Start of a Blank Node collection?
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTSQBRACKET)
                        {
                            // An anonymous Blank Node
                            context.Tokens.Dequeue();
                            obj = context.Handler.CreateBlankNode();
                        }
                        else
                        {
                            // Start of a Blank Node Collection
                            obj = context.Handler.CreateBlankNode();
                            TryParsePredicateObjectList(context, obj, true);
                        }
                        break;

                    case Token.LITERAL:
                    case Token.LITERALWITHDT:
                    case Token.LITERALWITHLANG:
                    case Token.LONGLITERAL:
                    case Token.PLAINLITERAL:
                        obj = TryParseLiteral(context, objToken);
                        break;

                    case Token.RIGHTSQBRACKET:
                        if (bnodeList)
                        {
                            if (obj != null)
                            {
                                // Ok to return if we've seen a Triple
                                return;
                            }
                            else
                            {
                                throw ParserHelper.Error("Unexpected Right Square Bracket encountered, expecting a valid object for the current Blank Node Predicate Object list", objToken);
                            }
                        }
                        else
                        {
                            throw ParserHelper.Error("Unexpected Right Square Bracket encountered but not expecting the end of a Blank Node Predicate Object list", objToken);
                        }

                    case Token.SEMICOLON:
                        if (obj != null)
                        {
                            // Ok to return if we've seen a Triple
                            return;
                        }
                        else
                        {
                            throw ParserHelper.Error("Unexpected Semicolon Triple terminator encountered, expected a valid Object for the current Triple", objToken);
                        }

                    case Token.QNAME:
                    case Token.URI:
                        obj = ParserHelper.TryResolveUri(context, objToken, false, context.QNameUnescapeFunction);
                        break;

                    case Token.EOF:
                        throw ParserHelper.Error("Unexpected end of file while trying to parse an Object list", objToken);

                    default:
                        throw ParserHelper.Error("Unexpected Token '" + objToken.GetType().ToString() + "' encountered while trying to parse an Object list", objToken);
                }

                // Assert the Triple
                if (!context.Handler.HandleTriple(new Triple(subj, pred, obj))) ParserHelper.Stop();

                // Expect a comma/semicolon/dot terminator if we are to continue
                next = context.Tokens.Peek();
                if (bnodeList)
                {
                    // If in a Blank Node list a dot is not permitted but a ] is
                    if (next.TokenType != Token.COMMA && next.TokenType != Token.SEMICOLON && next.TokenType != Token.RIGHTSQBRACKET)
                    {
                        throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered while trying to parse a Blank Node Object List, expected a comma, semicolon or ] to terminate the current Triple/list", next);
                    } 
                } 
                else if (next.TokenType != Token.COMMA && next.TokenType != Token.SEMICOLON && next.TokenType != Token.DOT)
                {
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered while trying to parse an Object list, expected a comma, semicolon or dot to terminate the current Triple", next);
                }
            } while (true);
        }

        /// <summary>
        /// Tries to parse Collections
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="firstSubj">Blank Node which is the head of the collection</param>
        private void TryParseCollection(TurtleParserContext context, INode firstSubj)
        {
            // The opening bracket of the collection will already have been discarded when we get called
            IToken next;
            INode subj = firstSubj;
            INode obj = null, nextSubj;
            INode rdfFirst = context.Handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListFirst));
            INode rdfRest = context.Handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListRest));
            INode rdfNil = context.Handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListNil));

            do
            {
                next = context.Tokens.Dequeue();

                if (context.TraceParsing)
                {
                    Console.WriteLine("Trying to parse a Collection item from Token '" + next.GetType().ToString() + "'");
                }

                switch (next.TokenType)
                {
                    case Token.BLANKNODE:
                        obj = context.Handler.CreateBlankNode();
                        break;
                    case Token.BLANKNODEWITHID:
                        obj = context.Handler.CreateBlankNode(next.Value.Substring(2));
                        break;
                    case Token.COMMENT:
                        // Discard and continue
                        continue;
                    case Token.LEFTBRACKET:
                        // Start of a collection so create a new Blank Node to be it's first subject
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTBRACKET)
                        {
                            // Empty Collection => rdf:nil
                            context.Tokens.Dequeue();
                            obj = context.Handler.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListNil));
                        }
                        else
                        {
                            obj = context.Handler.CreateBlankNode();
                            TryParseCollection(context, obj);
                        }
                        break;

                    case Token.LEFTSQBRACKET:
                        // Allowed Blank Node Collections as part of a Collection
                        IToken temp = context.Tokens.Peek();
                        if (temp.TokenType == Token.RIGHTSQBRACKET)
                        {
                            // Anonymous Blank Node
                            context.Tokens.Dequeue();
                            obj = context.Handler.CreateBlankNode();
                        }
                        else
                        {
                            // Blank Node Collection
                            obj = context.Handler.CreateBlankNode();
                            TryParsePredicateObjectList(context, obj, true);
                        }
                        break;
                    case Token.LITERAL:
                    case Token.LITERALWITHDT:
                    case Token.LITERALWITHLANG:
                    case Token.LONGLITERAL:
                    case Token.PLAINLITERAL:
                        obj = TryParseLiteral(context, next);
                        break;

                    case Token.QNAME:
                    case Token.URI:
                        obj = ParserHelper.TryResolveUri(context, next, false, context.QNameUnescapeFunction);
                        break;

                    case Token.RIGHTBRACKET:
                        // We might terminate here if someone put a comment before the end of the Collection
                        if (!context.Handler.HandleTriple(new Triple(subj, rdfFirst, obj))) ParserHelper.Stop();
                        if (!context.Handler.HandleTriple(new Triple(subj, rdfRest, rdfNil))) ParserHelper.Stop();
                        return;

                    default:
                        throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered while trying to parse a Collection", next);
                }

                // Assert the relevant Triples
                if (!context.Handler.HandleTriple(new Triple(subj, rdfFirst, obj))) ParserHelper.Stop();
                if (context.Tokens.Peek().TokenType == Token.RIGHTBRACKET)
                {
                    // End of the Collection
                    context.Tokens.Dequeue();
                    if (!context.Handler.HandleTriple(new Triple(subj, rdfRest, rdfNil))) ParserHelper.Stop();
                    return;
                }
                else
                {
                    // More stuff in the collection
                    nextSubj = context.Handler.CreateBlankNode();
                    if (!context.Handler.HandleTriple(new Triple(subj, rdfRest, nextSubj))) ParserHelper.Stop();
                    subj = nextSubj;
                }
            } while (true);
        }

        /// <summary>
        /// Tries to parse Literal Tokens into Literal Nodes
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="lit">Literal Token</param>
        /// <returns></returns>
        private INode TryParseLiteral(TurtleParserContext context, IToken lit)
        {
            IToken next;
            String dturi;

            switch (lit.TokenType)
            {
                case Token.LITERAL:
                case Token.LONGLITERAL:
                    next = context.Tokens.Peek();
                    if (next.TokenType == Token.LANGSPEC)
                    {
                        // Has a Language Specifier
                        next = context.Tokens.Dequeue();
                        return context.Handler.CreateLiteralNode(lit.Value, next.Value);
                    }
                    else if (next.TokenType == Token.DATATYPE)
                    {
                        // Has a Datatype
                        next = context.Tokens.Dequeue();
                        try
                        {
                            if (next.Value.StartsWith("<"))
                            {
                                dturi = next.Value.Substring(1, next.Value.Length - 2);
                                return context.Handler.CreateLiteralNode(lit.Value, UriFactory.Create(Tools.ResolveUri(dturi, context.BaseUri.ToSafeString())));
                            }
                            else
                            {
                                dturi = Tools.ResolveQName(next.Value, context.Namespaces, context.BaseUri);
                                return context.Handler.CreateLiteralNode(lit.Value, UriFactory.Create(dturi));
                            }
                        }
                        catch (RdfException rdfEx)
                        {
                            throw new RdfParseException("Unable to resolve the Datatype '" + next.Value + "' due to the following error:\n" + rdfEx.Message, next, rdfEx);
                        }
                    }
                    else
                    {
                        // Just an untyped Literal
                        return context.Handler.CreateLiteralNode(lit.Value);
                    }

                case Token.LITERALWITHDT:
                    LiteralWithDataTypeToken litdt = (LiteralWithDataTypeToken)lit;
                    try
                    {
                        if (litdt.DataType.StartsWith("<"))
                        {
                            dturi = litdt.DataType.Substring(1, litdt.DataType.Length - 2);
                            return context.Handler.CreateLiteralNode(litdt.Value, UriFactory.Create(Tools.ResolveUri(dturi, context.BaseUri.ToSafeString())));
                        }
                        else
                        {
                            dturi = Tools.ResolveQName(litdt.DataType, context.Namespaces, context.BaseUri);
                            return context.Handler.CreateLiteralNode(litdt.Value, UriFactory.Create(dturi));
                        }
                    }
                    catch (RdfException rdfEx)
                    {
                        throw new RdfParseException("Unable to resolve the Datatype '" + litdt.DataType + "' due to the following error:\n" + rdfEx.Message, litdt, rdfEx);
                    }

                case Token.LITERALWITHLANG:
                    LiteralWithLanguageSpecifierToken langlit = (LiteralWithLanguageSpecifierToken)lit;
                    return context.Handler.CreateLiteralNode(langlit.Value, langlit.Language);

                case Token.PLAINLITERAL:
                    // Attempt to infer Type
                    if (TurtleSpecsHelper.IsValidPlainLiteral(lit.Value, _syntax))
                    {
                        if (TurtleSpecsHelper.IsValidDouble(lit.Value))
                        {
                            return context.Handler.CreateLiteralNode(lit.Value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDouble));
                        }
                        else if (TurtleSpecsHelper.IsValidInteger(lit.Value))
                        {
                            return context.Handler.CreateLiteralNode(lit.Value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger));
                        }
                        else if (TurtleSpecsHelper.IsValidDecimal(lit.Value))
                        {
                            return context.Handler.CreateLiteralNode(lit.Value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDecimal));
                        }
                        else
                        {
                            return context.Handler.CreateLiteralNode(lit.Value.ToLower(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                        }
                    }
                    else
                    {
                        throw ParserHelper.Error("The value '" + lit.Value + "' is not valid as a Plain Literal in Turtle", lit);
                    }
                default:
                    throw ParserHelper.Error("Unexpected Token '" + lit.GetType().ToString() + "' encountered, expected a valid Literal Token to convert to a Node", lit);
            }
        }

        /// <summary>
        /// Helper method which raises the Warning event if there is an event handler registered
        /// </summary>
        /// <param name="message"></param>
        private void RaiseWarning(String message)
        {
            RdfReaderWarning d = Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event which is raised when the parser detects issues with the input which are non-fatal
        /// </summary>
        public event RdfReaderWarning Warning;

        /// <summary>
        /// Gets the String representation of the Parser which is a description of the syntax it parses
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Turtle";
        }
    }
}
