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
    /// Parser for Notation 3 syntax
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <threadsafety instance="true">Designed to be Thread Safe - should be able to call Load from multiple threads on different Graphs without issue</threadsafety>
    public class Notation3Parser 
        : IRdfReader, ITraceableParser, ITraceableTokeniser, ITokenisingParser
    {
        private bool _traceParsing = false;
        private bool _traceTokeniser = false;
        private TokenQueueMode _queueMode = Options.DefaultTokenQueueMode;

        /// <summary>
        /// The Uri for log:implies
        /// </summary>
        private const String ImpliesUri = "http://www.w3.org/2000/10/swap/log#implies";
        /// <summary>
        /// The Uri for owl:sameAs
        /// </summary>
        private const String SameAsUri = "http://www.w3.org/2002/07/owl#sameAs";

        /// <summary>
        /// Creates a new Notation 3 Parser
        /// </summary>
        public Notation3Parser() { }

        /// <summary>
        /// Creates a new Notation 3 Parser which uses the given Token Queue Mode
        /// </summary>
        /// <param name="queueMode">Queue Mode for Tokenising</param>
        public Notation3Parser(TokenQueueMode queueMode)
            : this()
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
        /// Loads a Graph by reading Notation 3 syntax from the given input
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="input">Stream to read from</param>
        public void Load(IGraph g, StreamReader input)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Loads a Graph by reading Notation 3 syntax from the given input
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="input">Input to read from</param>
        public void Load(IGraph g, TextReader input)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Loads a Graph by reading Notation 3 syntax from the given file
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="filename">File to read from</param>
        public void Load(IGraph g, string filename)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");
            using (var reader = new StreamReader(File.OpenRead(filename), Encoding.UTF8))
            {
                Load(g, reader);
            }
        }

        /// <summary>
        /// Loads RDF using a RDF handler by reading Notation 3 syntax from the given input
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
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
        /// Loads RDF using a RDF handler by reading Notation 3 syntax from the given input
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">Input to read from</param>
        public void Load(IRdfHandler handler, TextReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannot read RDF from a null TextReader");

            try
            {
                Notation3ParserContext context = new Notation3ParserContext(handler, new Notation3Tokeniser(input), _queueMode, _traceParsing, _traceTokeniser);
                Parse(context);

                input.Close();
            }
            catch
            {
                try
                {
                    input.Close();
                }
                catch
                {
                    // No catch actions, just trying to clean up
                }
                throw;
            }
        }

        /// <summary>
        /// Loads RDF using a RDF handler by reading Notation 3 syntax from the given file
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="filename">File to read from</param>
        public void Load(IRdfHandler handler, String filename)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");
            using (var reader = new StreamReader(File.OpenRead(filename), Encoding.UTF8))
            {
                Load(handler, reader);
            }
        }

        /// <summary>
        /// Internal method which does the parsing of the input
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void Parse(Notation3ParserContext context)
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
                        case Token.BASEDIRECTIVE:
                        case Token.PREFIXDIRECTIVE:
                        case Token.KEYWORDDIRECTIVE:
                            TryParseDirective(context);
                            break;

                        case Token.FORALL:
                            TryParseForAll(context);
                            break;

                        case Token.FORSOME:
                            TryParseForSome(context);
                            break;

                        case Token.COMMENT:
                            // Discard and ignore
                            context.Tokens.Dequeue();
                            break;

                        case Token.BLANKNODE:
                        case Token.BLANKNODEWITHID:
                        case Token.LEFTBRACKET:
                        case Token.LEFTCURLYBRACKET:
                        case Token.LEFTSQBRACKET:
                        case Token.LITERAL:
                        case Token.LITERALWITHDT:
                        case Token.LITERALWITHLANG:
                        case Token.LONGLITERAL:
                        case Token.PLAINLITERAL:
                        case Token.QNAME:
                        case Token.URI:
                            // Valid Subject of a Triple
                            TryParseTriples(context);
                            break;

                        case Token.KEYWORDA:
                            // 'a' Keyword only valid as Predicate
                            throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, the 'a' Keyword is only valid as a Predicate in Notation 3", next);

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
        /// Tries to parse declarations
        /// </summary>
        /// <param name="context">Parse Context</param>
        private void TryParseDirective(Notation3ParserContext context)
        {
            if (context.TraceParsing)
            {
                Console.WriteLine("Attempting to parse a Directive");
            }

            // We expect either a Base Directive/Prefix/Keywords Directive
            IToken directive = context.Tokens.Dequeue();
            if (directive.TokenType == Token.BASEDIRECTIVE)
            {
                // Then expect a Uri for the Base Uri
                IToken u = context.Tokens.Dequeue();
                if (u.TokenType == Token.URI)
                {
                    // Set the Base Uri resolving against the current Base if any
                    Uri baseUri = ((IUriNode)ParserHelper.TryResolveUri(context, u, true)).Uri;
                    context.BaseUri = baseUri;
                    if (!context.Handler.HandleBaseUri(baseUri)) ParserHelper.Stop();
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
                        Uri nsUri = ((IUriNode)ParserHelper.TryResolveUri(context, ns, true)).Uri;
                        String nsPrefix = (pre.Value.Length > 1) ? pre.Value.Substring(0, pre.Value.Length-1) : String.Empty;
                        context.Namespaces.AddNamespace(nsPrefix, nsUri);
                        if (!context.Handler.HandleNamespace(pre.Value.Substring(0, pre.Value.Length - 1), nsUri)) ParserHelper.Stop();
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
            else if (directive.TokenType == Token.KEYWORDDIRECTIVE)
            {
                // Expect zero/more keywords followed by a dot
                context.KeywordsMode = true;

                IToken next = context.Tokens.Dequeue();
                while (next.TokenType != Token.DOT)
                {
                    // Error if not a Keyword Definition
                    if (next.TokenType != Token.KEYWORDDEF)
                    {
                        throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered when a CustomKeywordDefinitionToken was expected", next);
                    }

                    // Add to Keywords List
                    if (!context.Keywords.Contains(next.Value))
                    {
                        context.Keywords.Add(next.Value);
                    }

                    next = context.Tokens.Dequeue();
                }
                return;
            }
            else
            {
                throw ParserHelper.Error("Unexpected Token '" + directive.GetType().ToString() + "' encountered, expected a Base/Prefix/Keywords Directive after an @ symbol", directive);
            }

            // All declarations are terminated with a Dot
            IToken terminator = context.Tokens.Dequeue();
            if (terminator.TokenType != Token.DOT)
            {
                throw ParserHelper.Error("Unexpected Token '" + terminator.GetType().ToString() + "' encountered, expected a Dot Line Terminator to terminate a Prefix/Base Directive", terminator);
            }
        }

        /// <summary>
        /// Tries to parse forAll quantifiers
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void TryParseForAll(Notation3ParserContext context)
        {
            // We know the Token we've just got off the Queue was a ForAllQuantifierToken
            // Therefore the next Token(s) should be QNames/URIs leading to a DotToken

            // Create a new Variable Context if one doesn't currently exist
            if (context.VariableContext.Type == VariableContextType.None)
            {
                context.VariableContext = new VariableContext(VariableContextType.Universal);
            }
            else
            {
                context.VariableContext.InnerContext = new VariableContext(VariableContextType.Universal);
            }

            context.Tokens.Dequeue();
            IToken next = context.Tokens.Dequeue();
            while (next.TokenType != Token.DOT)
            {
                // Get Variables
                switch (next.TokenType)
                {
                    case Token.QNAME:
                    case Token.URI:
                        context.VariableContext.AddVariable(ParserHelper.TryResolveUri(context, next, true));
                        break;

                    default:
                        throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a URI/QName for a Universal Variable as part of a @forAll directive", next);
                }

                next = context.Tokens.Dequeue();

                // May optionally have a COMMA between each QName/URI
                if (next.TokenType == Token.COMMA)
                {
                    next = context.Tokens.Dequeue();
                }
            }
        }

        /// <summary>
        /// Tries to parse forSome quantifiers
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void TryParseForSome(Notation3ParserContext context)
        {
            // We know the Token we've just got off the Queue was a ForSomeQuantifierToken
            // Therefore the next Token(s) should be QNames/URIs leading to a DotToken

            // Create a new Variable Context if one doesn't currently exist
            if (context.VariableContext.Type == VariableContextType.None)
            {
                context.VariableContext = new VariableContext(VariableContextType.Existential);
            }
            else
            {
                context.VariableContext.InnerContext = new VariableContext(VariableContextType.Existential);
            }

            context.Tokens.Dequeue();
            IToken next = context.Tokens.Dequeue();
            while (next.TokenType != Token.DOT)
            {
                // Get Variables
                switch (next.TokenType)
                {
                    case Token.QNAME:
                    case Token.URI:
                        context.VariableContext.AddVariable(ParserHelper.TryResolveUri(context, next, true));
                        break;

                    default:
                        throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a URI/QName for an Existential Variable as part of a @forSome directive", next);
                }

                next = context.Tokens.Dequeue();

                // May optionally have a COMMA between each QName/URI
                if (next.TokenType == Token.COMMA)
                {
                    next = context.Tokens.Dequeue();
                }
            }

            RaiseWarning("Parser does not know how to evaluate forSome Quantifiers");
        }

        /// <summary>
        /// Tries to parse Triples
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void TryParseTriples(Notation3ParserContext context)
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
                        subj = context.Handler.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "nil"));
                    }
                    else
                    {
                        subj = context.Handler.CreateBlankNode();
                        TryParseCollection(context, subj);
                    }
                    break;

                case Token.LEFTCURLYBRACKET:
                    // Start of a Graph Literal
                    next = context.Tokens.Peek();
                    if (next.TokenType == Token.RIGHTCURLYBRACKET)
                    {
                        // An Empty Graph Literal
                        context.Tokens.Dequeue();
                        subj = context.Handler.CreateGraphLiteralNode();
                    }
                    else
                    {
                        subj = TryParseGraphLiteral(context);
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
                    }
                    break;

                case Token.LITERAL:
                case Token.LITERALWITHDT:
                case Token.LITERALWITHLANG:
                case Token.LONGLITERAL:
                case Token.PLAINLITERAL:
                    // Literal Subjects valid in N3
                    subj = TryParseLiteral(context, subjToken);
                    break;

                case Token.QNAME:
                case Token.URI:
                    subj = ParserHelper.TryResolveUri(context, subjToken, true);
                    break;

                case Token.VARIABLE:
                    subj = context.Handler.CreateVariableNode(subjToken.Value.Substring(1));
                    break;

                default:
                    throw ParserHelper.Error("Unexpected Token '" + subjToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a Triple", subjToken);
            }

            if (context.Tokens.LastTokenType == Token.RIGHTSQBRACKET || context.Tokens.LastTokenType == Token.RIGHTCURLYBRACKET)
            {
                // Allowed to state a Blank Node collection or a Graph Literal on it's own in N3
                next = context.Tokens.Peek();
                if (next.TokenType == Token.DOT)
                {
                    context.Tokens.Dequeue();
                    return;
                }
            }

            TryParsePredicateObjectList(context, subj, false);
        }

        /// <summary>
        /// Tries to parse Predicate Object lists
        /// </summary>
        /// <param name="context">Parse Context</param>
        /// <param name="subj">Subject of the Triples</param>
        /// <param name="bnodeList">Whether this is a Blank Node Predicate Object list</param>
        private void TryParsePredicateObjectList(Notation3ParserContext context, INode subj, bool bnodeList)
        {
            IToken predToken, next;
            INode pred = null;
            bool reverse = false;

            do
            {
                predToken =  context.Tokens.Dequeue();

                if (context.TraceParsing)
                {
                    Console.WriteLine("Attempting to parse Predicate Object List from the Predicate Token '" + predToken.GetType().ToString() + "'");
                }

                switch (predToken.TokenType)
                {
                    case Token.BLANKNODE:
                        pred = context.Handler.CreateBlankNode();
                        break;

                    case Token.BLANKNODEWITHID:
                        pred = context.Handler.CreateBlankNode(predToken.Value.Substring(2));
                        break;

                    case Token.COMMENT:
                        // Discard and continue
                        continue;

                    case Token.EQUALS:
                        // = Keyword
                        pred = context.Handler.CreateUriNode(UriFactory.Create(SameAsUri));
                        break;

                    case Token.EXCLAMATION:
                    case Token.HAT:
                        // Path
                        subj = TryParsePath(context, subj);
                        TryParsePredicateObjectList(context, subj, bnodeList);
                        return;

                    case Token.IMPLIEDBY:
                        // <= keyword
                        pred = context.Handler.CreateUriNode(UriFactory.Create(ImpliesUri));
                        reverse = true;
                        break;

                    case Token.IMPLIES:
                        // => keyword
                        pred = context.Handler.CreateUriNode(UriFactory.Create(ImpliesUri));
                        break;

                    case Token.KEYWORDA:
                        // 'a' Keyword
                        pred = context.Handler.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "type"));
                        break;

                    case Token.LEFTBRACKET:
                        // Start of a collection so create a new Blank Node to be it's first subject
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTBRACKET)
                        {
                            // An Empty Collection => rdf:nil
                            context.Tokens.Dequeue();
                            pred = context.Handler.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "nil"));
                        }
                        else
                        {
                            pred = context.Handler.CreateBlankNode();
                            TryParseCollection(context, pred);
                        }
                        break;

                    case Token.LEFTCURLYBRACKET:
                        // Graph Literals not allowed as Predicates
                        throw ParserHelper.Error("Unexpected Left Curly Bracket encountered, Graph Literals are not valid as Predicates in Notation 3", predToken);

                    case Token.LEFTSQBRACKET:
                        // Start of a Blank Node collection?
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTSQBRACKET)
                        {
                            // An anoynmous Blank Node
                            context.Tokens.Dequeue();
                            pred = context.Handler.CreateBlankNode();
                        }
                        else
                        {
                            // Start of a Blank Node Collection
                            pred = context.Handler.CreateBlankNode();
                            TryParsePredicateObjectList(context, pred, true);
                        }
                        break;

                    case Token.RIGHTCURLYBRACKET:
                        if (context.GraphLiteralMode)
                        {
                            return;
                        }
                        else
                        {
                            throw ParserHelper.Error("Unexpected Right Curly Bracket encountered but not expecting the end of a Graph Literal", predToken);
                        }

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
                        pred = ParserHelper.TryResolveUri(context, predToken, true);
                        break;

                    case Token.VARIABLE:
                        pred = context.Handler.CreateVariableNode(predToken.Value.Substring(1));
                        break;

                    case Token.EOF:
                        throw ParserHelper.Error("Unexpected end of file while trying to parse a Predicate Object list", predToken);

                    default:
                        throw ParserHelper.Error("Unexpected Token '" + predToken.GetType().ToString() + "' encountered while trying to parse a Predicate Object list", predToken);

                }

                TryParseObjectList(context, subj, pred, bnodeList, reverse);
                if (context.Tokens.LastTokenType == Token.DOT && !bnodeList) return; //Dot terminates a normal Predicate Object list
                if (context.Tokens.LastTokenType == Token.RIGHTSQBRACKET && bnodeList) return; //Trailing semicolon may terminate a Blank Node Predicate Object list
                if (context.Tokens.LastTokenType == Token.SEMICOLON && context.Tokens.Peek().TokenType == Token.DOT)
                {
                    // Dot terminates a Predicate Object list with a trailing semicolon
                    context.Tokens.Dequeue();
                    return; 
                }
                if (context.Tokens.LastTokenType == Token.RIGHTCURLYBRACKET && context.GraphLiteralMode) return; //Right Curly Bracket terminates a Graph Literal
            } while (true);
        }

        /// <summary>
        /// Tries to parse Object lists
        /// </summary>
        /// <param name="context">Parse Context</param>
        /// <param name="subj">Subject of the Triples</param>
        /// <param name="pred">Predicate of the Triples</param>
        /// <param name="bnodeList">Whether this is a Blank Node Object list</param>
        /// <param name="reverse">Indicates whether the asserted triples should have it's subject and object swapped</param>
        private void TryParseObjectList(Notation3ParserContext context, INode subj, INode pred, bool bnodeList, bool reverse)
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

                    case Token.EXCLAMATION:
                    case Token.HAT:
                        // Path
                        pred = TryParsePath(context, pred);
                        TryParseObjectList(context, subj, pred, bnodeList, reverse);
                        return;

                    case Token.LEFTBRACKET:
                        // Start of a collection so create a new Blank Node to be it's first subject
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTBRACKET)
                        {
                            // Empty Collection => rdf:nil
                            context.Tokens.Dequeue();
                            obj = context.Handler.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "nil"));
                        }
                        else
                        {
                            obj = context.Handler.CreateBlankNode();
                            TryParseCollection(context, obj);
                        }
                        break;

                    case Token.LEFTCURLYBRACKET:
                        // Start of a Graph Literal
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTCURLYBRACKET)
                        {
                            // An Empty Graph Literal
                            context.Tokens.Dequeue();
                            obj = context.Handler.CreateGraphLiteralNode();
                        }
                        else
                        {
                            obj = TryParseGraphLiteral(context);
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

                    case Token.RIGHTCURLYBRACKET:
                        if (context.GraphLiteralMode)
                        {
                            return;
                        }
                        else
                        {
                            throw ParserHelper.Error("Unexpected Right Curly Bracket encountered but not expecting the end of a Graph Literal", objToken);
                        }

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
                        obj = ParserHelper.TryResolveUri(context, objToken, true);
                        break;

                    case Token.VARIABLE:
                        obj = context.Handler.CreateVariableNode(objToken.Value.Substring(1));
                        break;

                    case Token.EOF:
                        throw ParserHelper.Error("Unexpected end of file while trying to parse an Object list", objToken);

                    default:
                        throw ParserHelper.Error("Unexpected Token '" + objToken.GetType().ToString() + "' encountered while trying to parse an Object list", objToken);
                }

                // Watch out for Paths
                next = context.Tokens.Peek();
                if (next.TokenType == Token.EXCLAMATION || next.TokenType == Token.HAT)
                {
                    context.Tokens.Dequeue();
                    obj = TryParsePath(context, obj);
                }

                // Assert the Triple
                if (!reverse)
                {
                    if (!context.Handler.HandleTriple(new Triple(subj, pred, obj, context.VariableContext))) ParserHelper.Stop();
                }
                else
                {
                    // When reversed this means the predicate was Implied By (<=)
                    if (!context.Handler.HandleTriple(new Triple(obj, pred, subj, context.VariableContext))) ParserHelper.Stop();
                }

                // Expect a comma/semicolon/dot terminator if we are to continue
                next = context.Tokens.Peek();
                if (context.GraphLiteralMode)
                {
                    if (next.TokenType != Token.COMMA && next.TokenType != Token.SEMICOLON && next.TokenType != Token.DOT && next.TokenType != Token.RIGHTCURLYBRACKET)
                    {
                        throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered while trying to parse an Object list, expected a comma, semicolon or dot to terminate the current Triple or a } to terminate the Graph Literal", next);
                    }
                }
                else if (bnodeList)
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
        private void TryParseCollection(Notation3ParserContext context, INode firstSubj)
        {
            // The opening bracket of the collection will already have been discarded when we get called
            IToken next, temp;
            INode subj = firstSubj;
            INode obj = null, nextSubj;
            INode rdfFirst = context.Handler.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "first"));
            INode rdfRest = context.Handler.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "rest"));
            INode rdfNil = context.Handler.CreateUriNode(UriFactory.Create(NamespaceMapper.RDF + "nil"));

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
                        // Nested Collection?
                        temp = context.Tokens.Peek();
                        if (temp.TokenType == Token.RIGHTBRACKET)
                        {
                            // Empty Collection => rdf:nil
                            context.Tokens.Dequeue();
                            obj = rdfNil;
                        }
                        else
                        {
                            // Collection
                            obj = context.Handler.CreateBlankNode();
                            TryParseCollection(context, obj);
                        }
                        break;

                    case Token.LEFTCURLYBRACKET:
                        // Graph Literal
                        temp = context.Tokens.Peek();
                        if (temp.TokenType == Token.RIGHTCURLYBRACKET)
                        {
                            // Empty Graph Literal
                            context.Tokens.Dequeue();
                            obj = context.Handler.CreateGraphLiteralNode();
                        }
                        else
                        {
                            // Graph Literal
                            obj = TryParseGraphLiteral(context);
                        }
                        break;

                    case Token.LEFTSQBRACKET:
                        // Allowed Blank Node Collections as part of a Collection
                        temp = context.Tokens.Peek();
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

                    case Token.RIGHTBRACKET:
                        // We might terminate here if someone put a comment before the end of the Collection
                        if (!context.Handler.HandleTriple(new Triple(subj, rdfFirst, obj, context.VariableContext))) ParserHelper.Stop();
                        if (!context.Handler.HandleTriple(new Triple(subj, rdfRest, rdfNil, context.VariableContext))) ParserHelper.Stop();
                        return;

                    case Token.QNAME:
                    case Token.URI:
                        obj = ParserHelper.TryResolveUri(context, next, true);
                        break;

                    case Token.VARIABLE:
                        obj = context.Handler.CreateVariableNode(next.Value.Substring(1));
                        break;

                    default:
                        throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered while trying to parse a Collection", next);
                }

                // Watch out for Paths
                next = context.Tokens.Peek();
                if (next.TokenType == Token.EXCLAMATION || next.TokenType == Token.HAT)
                {
                    context.Tokens.Dequeue();
                    obj = TryParsePath(context, obj);
                }

                // Assert the relevant Triples
                if (!context.Handler.HandleTriple(new Triple(subj, rdfFirst, obj, context.VariableContext))) ParserHelper.Stop();
                if (context.Tokens.Peek().TokenType == Token.RIGHTBRACKET)
                {
                    // End of the Collection
                    context.Tokens.Dequeue();
                    if (!context.Handler.HandleTriple(new Triple(subj, rdfRest, rdfNil, context.VariableContext))) ParserHelper.Stop();
                    return;
                }
                else
                {
                    // More stuff in the collection
                    nextSubj = context.Handler.CreateBlankNode();
                    if (!context.Handler.HandleTriple(new Triple(subj, rdfRest, nextSubj, context.VariableContext))) ParserHelper.Stop();
                    subj = nextSubj;
                }
            } while (true);
        }

        /// <summary>
        /// Tries to parse a Graph Literal
        /// </summary>
        /// <param name="context"></param>
        private INode TryParseGraphLiteral(Notation3ParserContext context)
        {
            context.PushGraph();

            // Watch out for nesting
            // This counter starts as zero since the last right curly bracket will be discarded by the 
            // parser when it stops parsing the Graph Literal in one of the other functions
            int nesting = 1;
            IToken next = context.Tokens.Peek();
            while (next.TokenType == Token.LEFTCURLYBRACKET)
            {
                context.Tokens.Dequeue();
                next = context.Tokens.Peek();
                nesting++;
            }

            do
            {
                next = context.Tokens.Peek();
                if (next.TokenType == Token.FORALL)
                {
                    TryParseForAll(context);
                }
                else if (next.TokenType == Token.FORSOME)
                {
                    TryParseForSome(context);
                }
                else
                {
                    TryParseTriples(context);
                }

                // If we've just seen a Right Curly bracket we've been terminated
                if (context.Tokens.LastTokenType == Token.RIGHTCURLYBRACKET)
                {
                    // Decrease the amount of expected nesting
                    nesting--;
                    break;
                }

                next = context.Tokens.Peek();
            } while (next.TokenType != Token.RIGHTCURLYBRACKET);

            IGraph subgraph = context.SubGraph;
            context.PopGraph();

            // Expect the correct number of closing brackets
            while (nesting > 0)
            {
                next = context.Tokens.Dequeue();
                if (next.TokenType != Token.RIGHTCURLYBRACKET)
                {
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Right Curly Bracket to terminate a Graph Literal", next);
                }
                nesting--;
            }

            return context.Handler.CreateGraphLiteralNode(subgraph);
        }

        private INode TryParsePath(Notation3ParserContext context, INode firstItem)
        {
            IToken next;
            INode secondItem;
            bool forward = (context.Tokens.LastTokenType == Token.EXCLAMATION);

            // Actual path is represented by a new Blank Node
            INode path = context.Handler.CreateBlankNode();
            INode pathHead = path;

            do
            {
                // Get next thing which should be a Path Item
                next = context.Tokens.Dequeue();
                switch (next.TokenType)
                {
                    case Token.QNAME:
                        secondItem = context.Handler.CreateUriNode(UriFactory.Create(Tools.ResolveQName(next.Value, context.Namespaces, context.BaseUri, true)));
                        break;
                    case Token.LITERAL:
                    case Token.LONGLITERAL:
                    case Token.LITERALWITHDT:
                    case Token.LITERALWITHLANG:
                        secondItem = TryParseLiteral(context, next);
                        break;
                    case Token.URI:
                        secondItem = context.Handler.CreateUriNode(UriFactory.Create(Tools.ResolveUri(next.Value, context.BaseUri.ToSafeString())));
                        break;

                    default:
                        throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, only Literals, QNames and URIs are valid as Path Items", next);
                }

                if (forward)
                {
                    if (!context.Handler.HandleTriple(new Triple(firstItem, secondItem, path, context.VariableContext))) ParserHelper.Stop();
                }
                else
                {
                    if (!context.Handler.HandleTriple(new Triple(path, secondItem, firstItem, context.VariableContext))) ParserHelper.Stop();
                }

                // Does the Path continue?
                next = context.Tokens.Peek();
                if (next.TokenType == Token.EXCLAMATION || next.TokenType == Token.HAT)
                {
                    context.Tokens.Dequeue();
                    firstItem = path;
                    path = context.Handler.CreateBlankNode();
                    forward = (context.Tokens.LastTokenType == Token.EXCLAMATION);
                }
                else
                {
                    return pathHead;
                }
            } while (true);
        }

        /// <summary>
        /// Tries to parse Literal Tokens into Literal Nodes
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="lit">Literal Token</param>
        /// <returns></returns>
        private INode TryParseLiteral(Notation3ParserContext context, IToken lit)
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
                    else if (next.TokenType == Token.HATHAT)
                    {
                        // Discard the ^^
                        next = context.Tokens.Dequeue();

                        // Has a Datatype
                        next = context.Tokens.Dequeue();
                        if (next.TokenType == Token.DATATYPE)
                        {
                            try
                            {
                                if (next.Value.StartsWith("<"))
                                {
                                    dturi = next.Value.Substring(1, next.Value.Length - 2);
                                    return context.Handler.CreateLiteralNode(lit.Value, UriFactory.Create(Tools.ResolveUri(dturi, context.BaseUri.ToSafeString())));
                                }
                                else
                                {
                                    dturi = Tools.ResolveQName(next.Value, context.Namespaces, context.BaseUri, true);
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
                            throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Datatype Token after a ^^ datatype specifier", next);
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
                            dturi = Tools.ResolveQName(litdt.DataType, context.Namespaces, context.BaseUri, true);
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
                    if (TurtleSpecsHelper.IsValidPlainLiteral(lit.Value, TurtleSyntax.Original))
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
                            return context.Handler.CreateLiteralNode(lit.Value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
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
            return "Notation 3";
        }
    }
}
