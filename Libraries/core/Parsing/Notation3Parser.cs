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
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Parser for Notation 3 syntax
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a newly implemented parser as of 11/12/2009 - it was rewritten from scratch in order to remove an issue with Blank Node Handling which could not be solved with the old parser.  The code is now around a third the size, parses faster and appears to be bug free so far!
    /// </para>
    /// <para>
    /// As with the previous Parser @forSome and @forAll directives are in effect ignored and variables are treated simply as QNames in the default namespace.
    /// </para>
    /// </remarks>
    /// <threadsafety instance="true">Designed to be Thread Safe - should be able to call Load from multiple threads on different Graphs without issue</threadsafety>
    public class Notation3Parser : IRdfReader, ITraceableParser, ITraceableTokeniser
    {
        private bool _traceParsing = false;
        private bool _traceTokeniser = false;
        private TokenQueueMode _queueMode = TokenQueueMode.SynchronousBufferDuringParsing;

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
        public Notation3Parser()
        {

        }

        /// <summary>
        /// Creates a new Notation 3 Parser which uses the given Token Queue Mode
        /// </summary>
        /// <param name="queueMode">Queue Mode for Tokenising</param>
        public Notation3Parser(TokenQueueMode queueMode)
        {
            this._queueMode = queueMode;
        }

        /// <summary>
        /// Gets/Sets whether Parsing Trace is written to the Console
        /// </summary>
        public bool TraceParsing
        {
            get
            {
                return this._traceParsing;
            }
            set
            {
                this._traceParsing = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether Tokeniser Trace is written to the Console
        /// </summary>
        public bool TraceTokeniser
        {
            get
            {
                return this._traceTokeniser;
            }
            set
            {
                this._traceTokeniser = value;
            }
        }

        /// <summary>
        /// Loads a Graph by reading Notation 3 syntax from the given input
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="input">Stream to read from</param>
        public void Load(IGraph g, StreamReader input)
        {
            try
            {
                if (!g.IsEmpty)
                {
                    //Parse into a new Graph then merge with the existing Graph
                    Graph h = new Graph();
                    h.BaseUri = g.BaseUri;
                    Notation3ParserContext context = new Notation3ParserContext(h, new Notation3Tokeniser(input), this._queueMode, this._traceParsing, this._traceTokeniser);
                    this.Parse(context);
                    g.Merge(h);
                }
                else
                {
                    //Can parse into the Empty Graph
                    Notation3ParserContext context = new Notation3ParserContext(g, new Notation3Tokeniser(input), this._queueMode, this._traceParsing, this._traceTokeniser);
                    this.Parse(context);
                }

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
                    //No catch actions, just trying to clean up
                }
                throw;
            }
        }

        /// <summary>
        /// Loads a Graph by reading Notation 3 syntax from the given file
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="filename">File to read from</param>
        public void Load(IGraph g, string filename)
        {
            this.Load(g, new StreamReader(filename));
        }

        /// <summary>
        /// Internal method which does the parsing of the input
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void Parse(Notation3ParserContext context)
        {
            //Initialise Buffer and start parsing
            context.Tokens.InitialiseBuffer(10);

            IToken next = context.Tokens.Dequeue();
            if (next.TokenType != Token.BOF)
            {
                throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a BOF Token", next);
            }

            do
            {
                next = context.Tokens.Peek();

                switch (next.TokenType)
                {
                    case Token.BASEDIRECTIVE:
                    case Token.PREFIXDIRECTIVE:
                    case Token.KEYWORDDIRECTIVE:
                        this.TryParseDirective(context);
                        break;

                    case Token.FORALL:
                        this.TryParseForAll(context);
                        break;

                    case Token.FORSOME:
                        this.TryParseForSome(context);
                        break;

                    case Token.COMMENT:
                        //Discard and ignore
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
                        //Valid Subject of a Triple
                        this.TryParseTriples(context);
                        break;

                    case Token.KEYWORDA:
                        //'a' Keyword only valid as Predicate
                        throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, the 'a' Keyword is only valid as a Predicate in Notation 3", next);

                    case Token.EOF:
                        //OK - the loop will now terminate since we've seen the End of File
                        break;

                    default:
                        throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered", next);
                }
            } while (next.TokenType != Token.EOF);
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

            //We expect either a Base Directive/Prefix/Keywords Directive
            IToken directive = context.Tokens.Dequeue();
            if (directive.TokenType == Token.BASEDIRECTIVE)
            {
                //Then expect a Uri for the Base Uri
                IToken u = context.Tokens.Dequeue();
                if (u.TokenType == Token.URI)
                {
                    //Set the Base Uri resolving against the current Base if any
                    context.Graph.BaseUri = ((UriNode)this.TryResolveUri(context, u)).Uri;
                }
                else
                {
                    throw Error("Unexpected Token '" + u.GetType().ToString() + "' encountered, expected a URI after a Base Directive", u);
                }
            }
            else if (directive.TokenType == Token.PREFIXDIRECTIVE)
            {
                //Expect a Prefix then a Uri
                IToken pre = context.Tokens.Dequeue();
                if (pre.TokenType == Token.PREFIX)
                {
                    IToken ns = context.Tokens.Dequeue();
                    if (ns.TokenType == Token.URI)
                    {
                        //Register a Namespace resolving the Namespace Uri against the Base Uri
                        Uri nsURI = ((UriNode)this.TryResolveUri(context, ns)).Uri;
                        if (pre.Value.Length > 1)
                        {
                            context.Graph.NamespaceMap.AddNamespace(pre.Value.Substring(0, pre.Value.Length - 1), nsURI);
                        }
                        else
                        {
                            context.Graph.NamespaceMap.AddNamespace(String.Empty, nsURI);
                        }
                    }
                    else
                    {
                        throw Error("Unexpected Token '" + ns.GetType().ToString() + "' encountered, expected a URI after a Prefix Directive", pre);
                    }
                }
                else
                {
                    throw Error("Unexpected Token '" + pre.GetType().ToString() + "' encountered, expected a Prefix after a Prefix Directive", pre);
                }
            }
            else if (directive.TokenType == Token.KEYWORDDIRECTIVE)
            {
                //Expect zero/more keywords followed by a dot
                context.KeywordsMode = true;

                IToken next = context.Tokens.Dequeue();
                while (next.TokenType != Token.DOT)
                {
                    //Error if not a Keyword Definition
                    if (next.TokenType != Token.KEYWORDDEF)
                    {
                        throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered when a CustomKeywordDefinitionToken was expected", next);
                    }

                    //Add to Keywords List
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
                throw Error("Unexpected Token '" + directive.GetType().ToString() + "' encountered, expected a Base/Prefix/Keywords Directive after an @ symbol", directive);
            }

            //All declarations are terminated with a Dot
            IToken terminator = context.Tokens.Dequeue();
            if (terminator.TokenType != Token.DOT)
            {
                throw Error("Unexpected Token '" + terminator.GetType().ToString() + "' encountered, expected a Dot Line Terminator to terminate a Prefix/Base Directive", terminator);
            }
        }

        /// <summary>
        /// Tries to parse forAll quantifiers
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void TryParseForAll(Notation3ParserContext context)
        {
            //We know the Token we've just got off the Queue was a ForAllQuantifierToken
            //Therefore the next Token(s) should be triple items leading to a DotToken

            IToken next = context.Tokens.Dequeue();
            while (next.TokenType != Token.DOT)
            {
                next = context.Tokens.Dequeue();
            }

            this.OnWarning("Parser does not know how to evaluate forAll Quantifiers");
        }

        /// <summary>
        /// Tries to parse forSome quantifiers
        /// </summary>
        /// <param name="context">Parser Context</param>
        private void TryParseForSome(Notation3ParserContext context)
        {
            //We know the Token we've just got off the Queue was a ForSomeQuantifierToken
            //Therefore the next Token(s) should be triple items leading to a DotToken

            IToken next = context.Tokens.Dequeue();
            while (next.TokenType != Token.DOT)
            {
                next = context.Tokens.Dequeue();
            }

            this.OnWarning("Parser does not know how to evaluate forSome Quantifiers");
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
                    subj = context.Graph.CreateBlankNode();
                    break;

                case Token.BLANKNODEWITHID:
                    subj = context.Graph.CreateBlankNode(subjToken.Value.Substring(2));
                    break;

                case Token.LEFTBRACKET:
                    //Start of a collection so create a new Blank Node to be it's first subject
                    next = context.Tokens.Peek();
                    if (next.TokenType == Token.RIGHTBRACKET)
                    {
                        //An Empty Collection => rdf:nil
                        context.Tokens.Dequeue();
                        subj = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDF + "nil"));
                    }
                    else
                    {
                        subj = context.Graph.CreateBlankNode();
                        this.TryParseCollection(context, subj);
                    }
                    break;

                case Token.LEFTCURLYBRACKET:
                    //Start of a Graph Literal
                    next = context.Tokens.Peek();
                    if (next.TokenType == Token.RIGHTCURLYBRACKET)
                    {
                        //An Empty Graph Literal
                        context.Tokens.Dequeue();
                        subj = context.Graph.CreateGraphLiteralNode();
                    }
                    else
                    {
                        subj = this.TryParseGraphLiteral(context);
                    }
                    break;

                case Token.LEFTSQBRACKET:
                    //Start of a Blank Node collection?
                    next = context.Tokens.Peek();
                    if (next.TokenType == Token.RIGHTSQBRACKET)
                    {
                        //An anoynmous Blank Node
                        context.Tokens.Dequeue();
                        subj = context.Graph.CreateBlankNode();
                    }
                    else
                    {
                        //Start of a Blank Node Collection
                        subj = context.Graph.CreateBlankNode();
                        this.TryParsePredicateObjectList(context, subj, true);
                    }
                    break;

                case Token.LITERAL:
                case Token.LITERALWITHDT:
                case Token.LITERALWITHLANG:
                case Token.LONGLITERAL:
                case Token.PLAINLITERAL:
                    //Literal Subjects valid in N3
                    subj = this.TryParseLiteral(context, subjToken);
                    break;

                case Token.QNAME:
                case Token.URI:
                    subj = this.TryResolveUri(context, subjToken);
                    break;

                case Token.VARIABLE:
                    this.OnWarning("Parser treats variables as QNames in the default Namespace");
                    subj = context.Graph.CreateUriNode(":" + subjToken.Value.Substring(1));
                    break;

                default:
                    throw Error("Unexpected Token '" + subjToken.GetType().ToString() + "' encountered, this Token is not valid as the subject of a Triple", subjToken);
            }

            if (context.Tokens.LastTokenType == Token.RIGHTSQBRACKET || context.Tokens.LastTokenType == Token.RIGHTCURLYBRACKET)
            {
                //Allowed to state a Blank Node collection or a Graph Literal on it's own in N3
                next = context.Tokens.Peek();
                if (next.TokenType == Token.DOT)
                {
                    context.Tokens.Dequeue();
                    return;
                }
            }

            this.TryParsePredicateObjectList(context, subj, false);
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
                        pred = context.Graph.CreateBlankNode();
                        break;

                    case Token.BLANKNODEWITHID:
                        pred = context.Graph.CreateBlankNode(predToken.Value.Substring(2));
                        break;

                    case Token.COMMENT:
                        //Discard and continue
                        continue;

                    case Token.EQUALS:
                        //= Keyword
                        pred = context.Graph.CreateUriNode(new Uri(SameAsUri));
                        break;

                    case Token.EXCLAMATION:
                    case Token.HAT:
                        //Path
                        subj = this.TryParsePath(context, subj);
                        this.TryParsePredicateObjectList(context, subj, bnodeList);
                        return;

                    case Token.IMPLIEDBY:
                        //<= keyword
                        pred = context.Graph.CreateUriNode(new Uri(ImpliesUri));
                        reverse = true;
                        break;

                    case Token.IMPLIES:
                        //=> keyword
                        pred = context.Graph.CreateUriNode(new Uri(ImpliesUri));
                        break;

                    case Token.KEYWORDA:
                        //'a' Keyword
                        pred = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDF + "type"));
                        break;

                    case Token.LEFTBRACKET:
                        //Start of a collection so create a new Blank Node to be it's first subject
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTBRACKET)
                        {
                            //An Empty Collection => rdf:nil
                            context.Tokens.Dequeue();
                            pred = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDF + "nil"));
                        }
                        else
                        {
                            pred = context.Graph.CreateBlankNode();
                            this.TryParseCollection(context, pred);
                        }
                        break;

                    case Token.LEFTCURLYBRACKET:
                        //Graph Literals not allowed as Predicates
                        throw Error("Unexpected Left Curly Bracket encountered, Graph Literals are not valid as Predicates in Notation 3", predToken);

                    case Token.LEFTSQBRACKET:
                        //Start of a Blank Node collection?
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTSQBRACKET)
                        {
                            //An anoynmous Blank Node
                            context.Tokens.Dequeue();
                            pred = context.Graph.CreateBlankNode();
                        }
                        else
                        {
                            //Start of a Blank Node Collection
                            pred = context.Graph.CreateBlankNode();
                            this.TryParsePredicateObjectList(context, pred, true);
                        }
                        break;

                    case Token.RIGHTCURLYBRACKET:
                        if (context.GraphLiteralMode)
                        {
                            return;
                        }
                        else
                        {
                            throw Error("Unexpected Right Curly Bracket encountered but not expecting the end of a Graph Literal", predToken);
                        }

                    case Token.RIGHTSQBRACKET:
                        //If the last token was a semicolon and we're parsing a Blank Node Predicate Object list
                        //then a trailing semicolon is permitted
                        if (bnodeList)
                        {
                            if (context.Tokens.LastTokenType == Token.SEMICOLON)
                            {
                                return;
                            }
                            else
                            {
                                //If Predicate is not null then we've seen at least one valid Triple and this is just the end of the Blank Node Predicate Object list
                                if (pred != null)
                                {
                                    return;
                                }
                                else
                                {
                                    throw Error("Unexpected Right Square Bracket encountered while trying to parse a Blank Node Predicate Object list, expected a valid Predicate", predToken);
                                }
                            }
                        }
                        else
                        {
                            throw Error("Unexpected Right Square Bracket encountered while trying to parse a Predicate Object list", predToken);
                        }

                    case Token.QNAME:
                    case Token.URI:
                        pred = this.TryResolveUri(context, predToken);
                        break;

                    case Token.VARIABLE:
                        this.OnWarning("Parser treats variables as QNames in the default Namespace");
                        pred = context.Graph.CreateUriNode(":" + predToken.Value.Substring(1));
                        break;

                    case Token.EOF:
                        throw Error("Unexpected end of file while trying to parse a Predicate Object list", predToken);

                    default:
                        throw Error("Unexpected Token '" + predToken.GetType().ToString() + "' encountered while trying to parse a Predicate Object list", predToken);

                }

                this.TryParseObjectList(context, subj, pred, bnodeList, reverse);
                if (context.Tokens.LastTokenType == Token.DOT && !bnodeList) return; //Dot terminates a normal Predicate Object list
                if (context.Tokens.LastTokenType == Token.RIGHTSQBRACKET && bnodeList) return; //Trailing semicolon may terminate a Blank Node Predicate Object list
                if (context.Tokens.LastTokenType == Token.SEMICOLON && context.Tokens.Peek().TokenType == Token.DOT)
                {
                    //Dot terminates a Predicate Object list with a trailing semicolon
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
                        obj = context.Graph.CreateBlankNode();
                        break;

                    case Token.BLANKNODEWITHID:
                        obj = context.Graph.CreateBlankNode(objToken.Value.Substring(2));
                        break;

                    case Token.COMMA:
                        //Discard and continue - set object to null so we know we're expected to complete a triple
                        if (obj != null)
                        {
                            obj = null;
                            continue;
                        }
                        else
                        {
                            throw Error("Unexpected Comma Triple terminator encountered, expected a valid Object for the current Triple", objToken);
                        }

                    case Token.COMMENT:
                        //Discard and ignore
                        continue;

                    case Token.DOT:
                        if (obj != null)
                        {
                            //OK to return if we've seen a valid Triple
                            return;
                        }
                        else
                        {
                            throw Error("Unexpected Dot Triple terminator encountered, expected a valid Object for the current Triple", objToken);
                        }

                    case Token.EXCLAMATION:
                    case Token.HAT:
                        //Path
                        pred = this.TryParsePath(context, pred);
                        this.TryParseObjectList(context, subj, pred, bnodeList, reverse);
                        return;

                    case Token.LEFTBRACKET:
                        //Start of a collection so create a new Blank Node to be it's first subject
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTBRACKET)
                        {
                            //Empty Collection => rdf:nil
                            context.Tokens.Dequeue();
                            obj = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDF + "nil"));
                        }
                        else
                        {
                            obj = context.Graph.CreateBlankNode();
                            this.TryParseCollection(context, obj);
                        }
                        break;

                    case Token.LEFTCURLYBRACKET:
                        //Start of a Graph Literal
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTCURLYBRACKET)
                        {
                            //An Empty Graph Literal
                            context.Tokens.Dequeue();
                            obj = context.Graph.CreateGraphLiteralNode();
                        }
                        else
                        {
                            obj = this.TryParseGraphLiteral(context);
                        }
                        break;

                    case Token.LEFTSQBRACKET:
                        //Start of a Blank Node collection?
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTSQBRACKET)
                        {
                            //An anonymous Blank Node
                            context.Tokens.Dequeue();
                            obj = context.Graph.CreateBlankNode();
                        }
                        else
                        {
                            //Start of a Blank Node Collection
                            obj = context.Graph.CreateBlankNode();
                            this.TryParsePredicateObjectList(context, obj, true);
                        }
                        break;

                    case Token.LITERAL:
                    case Token.LITERALWITHDT:
                    case Token.LITERALWITHLANG:
                    case Token.LONGLITERAL:
                    case Token.PLAINLITERAL:
                        obj = this.TryParseLiteral(context, objToken);
                        break;

                    case Token.RIGHTCURLYBRACKET:
                        if (context.GraphLiteralMode)
                        {
                            return;
                        }
                        else
                        {
                            throw Error("Unexpected Right Curly Bracket encountered but not expecting the end of a Graph Literal", objToken);
                        }

                    case Token.RIGHTSQBRACKET:
                        if (bnodeList)
                        {
                            if (obj != null)
                            {
                                //Ok to return if we've seen a Triple
                                return;
                            }
                            else
                            {
                                throw Error("Unexpected Right Square Bracket encountered, expecting a valid object for the current Blank Node Predicate Object list", objToken);
                            }
                        }
                        else
                        {
                            throw Error("Unexpected Right Square Bracket encountered but not expecting the end of a Blank Node Predicate Object list", objToken);
                        }
 
                    case Token.SEMICOLON:
                        if (obj != null)
                        {
                            //Ok to return if we've seen a Triple
                            return;
                        }
                        else
                        {
                            throw Error("Unexpected Semicolon Triple terminator encountered, expected a valid Object for the current Triple", objToken);
                        }

                    case Token.QNAME:
                    case Token.URI:
                        obj = this.TryResolveUri(context, objToken);
                        break;

                    case Token.VARIABLE:
                        this.OnWarning("Parser treats variables as QNames in the default Namespace");
                        obj = context.Graph.CreateUriNode(":" + objToken.Value.Substring(1));
                        break;

                    case Token.EOF:
                        throw Error("Unexpected end of file while trying to parse an Object list", objToken);

                    default:
                        throw Error("Unexpected Token '" + objToken.GetType().ToString() + "' encountered while trying to parse an Object list", objToken);
                }

                //Watch out for Paths
                next = context.Tokens.Peek();
                if (next.TokenType == Token.EXCLAMATION || next.TokenType == Token.HAT)
                {
                    context.Tokens.Dequeue();
                    obj = this.TryParsePath(context, obj);
                }

                //Assert the Triple
                if (!reverse)
                {
                    context.Graph.Assert(new Triple(subj, pred, obj));
                }
                else
                {
                    //When reversed this means the predicate was Implies By (<=)
                    context.Graph.Assert(new Triple(obj, pred, subj));
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
            //The opening bracket of the collection will already have been discarded when we get called
            IToken next, temp;
            INode subj = firstSubj;
            INode obj = null, nextSubj;
            INode rdfFirst = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDF + "first"));
            INode rdfRest = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDF + "rest"));
            INode rdfNil = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDF + "nil"));

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
                        obj = context.Graph.CreateBlankNode();
                        break;
                    case Token.BLANKNODEWITHID:
                        obj = context.Graph.CreateBlankNode(next.Value.Substring(2));
                        break;
                    case Token.COMMENT:
                        //Discard and continue
                        continue;
                    case Token.LEFTBRACKET:
                        //Nested Collection?
                        temp = context.Tokens.Peek();
                        if (temp.TokenType == Token.RIGHTBRACKET)
                        {
                            //Empty Collection => rdf:nil
                            context.Tokens.Dequeue();
                            obj = rdfNil;
                        }
                        else
                        {
                            //Collection
                            obj = context.Graph.CreateBlankNode();
                            this.TryParseCollection(context, obj);
                        }
                        break;

                    case Token.LEFTCURLYBRACKET:
                        //Graph Literal
                        temp = context.Tokens.Peek();
                        if (temp.TokenType == Token.RIGHTCURLYBRACKET)
                        {
                            //Empty Graph Literal
                            context.Tokens.Dequeue();
                            obj = context.Graph.CreateGraphLiteralNode();
                        }
                        else
                        {
                            //Graph Literal
                            obj = this.TryParseGraphLiteral(context);
                        }
                        break;

                    case Token.LEFTSQBRACKET:
                        //Allowed Blank Node Collections as part of a Collection
                        temp = context.Tokens.Peek();
                        if (temp.TokenType == Token.RIGHTSQBRACKET)
                        {
                            //Anonymous Blank Node
                            context.Tokens.Dequeue();
                            obj = context.Graph.CreateBlankNode();
                        }
                        else
                        {
                            //Blank Node Collection
                            obj = context.Graph.CreateBlankNode();
                            this.TryParsePredicateObjectList(context, obj, true);
                        }
                        break;
                    case Token.LITERAL:
                    case Token.LITERALWITHDT:
                    case Token.LITERALWITHLANG:
                    case Token.LONGLITERAL:
                    case Token.PLAINLITERAL:
                        obj = this.TryParseLiteral(context, next);
                        break;

                    case Token.RIGHTBRACKET:
                        //We might terminate here if someone put a comment before the end of the Collection
                        context.Graph.Assert(new Triple(subj, rdfFirst, obj));
                        context.Graph.Assert(new Triple(subj, rdfRest, rdfNil));
                        return;

                    case Token.QNAME:
                    case Token.URI:
                        obj = this.TryResolveUri(context, next);
                        break;

                    case Token.VARIABLE:
                        this.OnWarning("Parser treats variables as QNames in the default Namespace");
                        obj = context.Graph.CreateUriNode(":" + next.Value.Substring(1));
                        break;

                    default:
                        throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered while trying to parse a Collection", next);
                }

                //Watch out for Paths
                next = context.Tokens.Peek();
                if (next.TokenType == Token.EXCLAMATION || next.TokenType == Token.HAT)
                {
                    context.Tokens.Dequeue();
                    obj = this.TryParsePath(context, obj);
                }

                //Assert the relevant Triples
                context.Graph.Assert(new Triple(subj, rdfFirst, obj));
                if (context.Tokens.Peek().TokenType == Token.RIGHTBRACKET)
                {
                    //End of the Collection
                    context.Tokens.Dequeue();
                    context.Graph.Assert(new Triple(subj, rdfRest, rdfNil));
                    return;
                }
                else
                {
                    //More stuff in the collection
                    nextSubj = context.Graph.CreateBlankNode();
                    context.Graph.Assert(new Triple(subj, rdfRest, nextSubj));
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

            //Watch out for nesting
            //This counter starts as zero since the last right curly bracket will be discarded by the 
            //parser when it stops parsing the Graph Literal in one of the other functions
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
                this.TryParseTriples(context);

                //If we've just seen a Right Curly bracket we've been terminated
                if (context.Tokens.LastTokenType == Token.RIGHTCURLYBRACKET)
                {
                    //Decrease the amount of expected nesting
                    nesting--;
                    break;
                }

                next = context.Tokens.Peek();
            } while (next.TokenType != Token.RIGHTCURLYBRACKET);

            IGraph subgraph = context.Graph;
            context.PopGraph();


            //Expect the correct number of closing brackets
            while (nesting > 0)
            {
                next = context.Tokens.Dequeue();
                if (next.TokenType != Token.RIGHTCURLYBRACKET)
                {
                    throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Right Curly Bracket to terminate a Graph Literal", next);
                }
                nesting--;
            }

            return context.Graph.CreateGraphLiteralNode(subgraph);
        }

        private INode TryParsePath(Notation3ParserContext context, INode firstItem)
        {
            IToken next;
            INode secondItem;
            bool forward = (context.Tokens.LastTokenType == Token.EXCLAMATION);

            //Actual path is represented by a new Blank Node
            INode path = context.Graph.CreateBlankNode();
            INode pathHead = path;

            do
            {
                //Get next thing which should be a Path Item
                next = context.Tokens.Dequeue();
                switch (next.TokenType)
                {
                    case Token.QNAME:
                        secondItem = context.Graph.CreateUriNode(next.Value);
                        break;
                    case Token.LITERAL:
                    case Token.LONGLITERAL:
                    case Token.LITERALWITHDT:
                    case Token.LITERALWITHLANG:
                        secondItem = this.TryParseLiteral(context, next);
                        break;
                    case Token.URI:
                        String currentBase = (context.Graph.BaseUri == null) ? String.Empty : context.Graph.BaseUri.ToString();
                        secondItem = context.Graph.CreateUriNode(new Uri(Tools.ResolveUri(next.Value, currentBase)));
                        break;

                    default:
                        throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, only Literals, QNames and URIs are valid as Path Items", next);
                }

                if (forward)
                {
                    context.Graph.Assert(new Triple(firstItem, secondItem, path));
                }
                else
                {
                    context.Graph.Assert(new Triple(path, secondItem, firstItem));
                }

                //Does the Path continue?
                next = context.Tokens.Peek();
                if (next.TokenType == Token.EXCLAMATION || next.TokenType == Token.HAT)
                {
                    context.Tokens.Dequeue();
                    firstItem = path;
                    path = context.Graph.CreateBlankNode();
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
            String dturi, currentBase;

            switch (lit.TokenType)
            {
                case Token.LITERAL:
                case Token.LONGLITERAL:
                    next = context.Tokens.Peek();
                    if (next.TokenType == Token.LANGSPEC)
                    {
                        //Has a Language Specifier
                        next = context.Tokens.Dequeue();
                        return context.Graph.CreateLiteralNode(lit.Value, next.Value);
                    }
                    else if (next.TokenType == Token.HATHAT)
                    {
                        //Discard the ^^
                        next = context.Tokens.Dequeue();

                        //Has a Datatype
                        next = context.Tokens.Dequeue();
                        if (next.TokenType == Token.DATATYPE)
                        {
                            try
                            {
                                if (next.Value.StartsWith("<"))
                                {
                                    dturi = next.Value.Substring(1, next.Value.Length - 2);
                                    currentBase = (context.Graph.BaseUri == null) ? String.Empty : context.Graph.BaseUri.ToString();
                                    return context.Graph.CreateLiteralNode(lit.Value, new Uri(Tools.ResolveUri(dturi, currentBase)));
                                }
                                else
                                {
                                    dturi = Tools.ResolveQName(next.Value, context.Graph.NamespaceMap, context.Graph.BaseUri);
                                    return context.Graph.CreateLiteralNode(lit.Value, new Uri(dturi));
                                }
                            }
                            catch (RdfException rdfEx)
                            {
                                throw new RdfParseException("Unable to resolve the Datatype '" + next.Value + "' due to the following error:\n" + rdfEx.Message, next, rdfEx);
                            }
                        }
                        else
                        {
                            throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Datatype Token after a ^^ datatype specifier", next);
                        }
                    }
                    else
                    {
                        //Just an untyped Literal
                        return context.Graph.CreateLiteralNode(lit.Value);
                    }

                case Token.LITERALWITHDT:
                    LiteralWithDataTypeToken litdt = (LiteralWithDataTypeToken)lit;
                    try
                    {
                        if (litdt.DataType.StartsWith("<"))
                        {
                            dturi = litdt.DataType.Substring(1, litdt.DataType.Length - 2);
                            currentBase = (context.Graph.BaseUri == null) ? String.Empty : context.Graph.BaseUri.ToString();
                            return context.Graph.CreateLiteralNode(litdt.Value, new Uri(Tools.ResolveUri(dturi, currentBase)));
                        }
                        else
                        {
                            dturi = Tools.ResolveQName(litdt.DataType, context.Graph.NamespaceMap, context.Graph.BaseUri);
                            return context.Graph.CreateLiteralNode(litdt.Value, new Uri(dturi));
                        }
                    }
                    catch (RdfException rdfEx)
                    {
                        throw new RdfParseException("Unable to resolve the Datatype '" + litdt.DataType + "' due to the following error:\n" + rdfEx.Message, litdt, rdfEx);
                    }

                case Token.LITERALWITHLANG:
                    LiteralWithLanguageSpecifierToken langlit = (LiteralWithLanguageSpecifierToken)lit;
                    return context.Graph.CreateLiteralNode(langlit.Value, langlit.Language);

                case Token.PLAINLITERAL:
                    //Attempt to infer Type
                    if (TurtleSpecsHelper.IsValidPlainLiteral(lit.Value))
                    {
                        if (TurtleSpecsHelper.IsValidDouble(lit.Value))
                        {
                            return context.Graph.CreateLiteralNode(lit.Value, new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble));
                        }
                        else if (TurtleSpecsHelper.IsValidInteger(lit.Value))
                        {
                            return context.Graph.CreateLiteralNode(lit.Value, new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
                        }
                        else if (TurtleSpecsHelper.IsValidDecimal(lit.Value))
                        {
                            return context.Graph.CreateLiteralNode(lit.Value, new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal));
                        }
                        else
                        {
                            return context.Graph.CreateLiteralNode(lit.Value, new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                        }
                    }
                    else
                    {
                        throw Error("The value '" + lit.Value + "' is not valid as a Plain Literal in Turtle", lit);
                    }
                default:
                    throw Error("Unexpected Token '" + lit.GetType().ToString() + "' encountered, expected a valid Literal Token to convert to a Node", lit);
            }
        }

        /// <summary>
        /// Attempts to resolve a QName or URI Token into a URI Node and produces appropriate error messages if this fails
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="t">Token to resolve</param>
        /// <returns></returns>
        private INode TryResolveUri(TokenisingParserContext context, IToken t)
        {
            switch (t.TokenType)
            {
                case Token.QNAME:
                    try
                    {
                        return context.Graph.CreateUriNode(t.Value);
                    }
                    catch (UriFormatException formatEx)
                    {
                        throw new RdfParseException("Unable to resolve the URI '" + t.Value + "' due to the following error:\n" + formatEx.Message, t, formatEx);
                    }
                    catch (RdfException rdfEx)
                    {
                        throw new RdfParseException("Unable to resolve the QName '" + t.Value + "' due to the following error:\n" + rdfEx.Message, t, rdfEx);
                    }

                case Token.URI:
                    try
                    {
                        String uri = Tools.ResolveUri(t.Value, context.Graph.BaseUri.ToSafeString());
                        return context.Graph.CreateUriNode(new Uri(uri));
                    }
                    catch (UriFormatException formatEx)
                    {
                        throw new RdfParseException("Unable to resolve the URI '" + t.Value + "' due to the following error:\n" + formatEx.Message, t, formatEx);
                    }
                    catch (RdfException rdfEx)
                    {
                        throw new RdfParseException("Unable to resolve the URI '" + t.Value + "' due to the following error:\n" + rdfEx.Message, t, rdfEx);
                    }

                default:
                    throw Error("Unexpected Token '" + t.GetType().ToString() + "' encountered, expected a URI/QName Token to resolve into a URI", t);
            }
        }

        /// <summary>
        /// Helper method for raising informative standardised Parser Errors
        /// </summary>
        /// <param name="msg">The Error Message</param>
        /// <param name="t">The Token that is the cause of the Error</param>
        /// <returns></returns>
        private RdfParseException Error(String msg, IToken t)
        {
            StringBuilder output = new StringBuilder();
            output.Append("[");
            output.Append(t.GetType().Name);
            output.Append(" at Line ");
            output.Append(t.StartLine);
            output.Append(" Column ");
            output.Append(t.StartPosition);
            output.Append(" to Line ");
            output.Append(t.EndLine);
            output.Append(" Column ");
            output.Append(t.EndPosition);
            output.Append("] ");
            output.Append(msg);

            return new RdfParseException(output.ToString(), t);
        }

        /// <summary>
        /// Helper method which raises the Warning event if there is an event handler registered
        /// </summary>
        /// <param name="message"></param>
        private void OnWarning(String message)
        {
            RdfReaderWarning d = this.Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event which is raised when the parser detects issues with the input which are non-fatal
        /// </summary>
        public event RdfReaderWarning Warning;
    }
}
