/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Text;
using System.IO;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Possible NQuads Syntax modes
    /// </summary>
    public enum NQuadsSyntax
    {
        /// <summary>
        /// The original <a href="http://sw.deri.org/2008/07/n-quads/">NQuads specification</a>
        /// </summary>
        Original,

        /// <summary>
        /// Standardized NQuads as specified in the <a href="http://www.w3.org/TR/n-quads/">RDF 1.1 NQuads</a> specification
        /// </summary>
        Rdf11
    }

    /// <summary>
    /// Parser for parsing NQuads (NTriples with an additional Context i.e. Named Graphs)
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Default Graph (if any) will be given the special Uri <strong>nquads:default-graph</strong>
    /// </para>
    /// <para>
    /// NQuads permits Blank Nodes and Literals to be used as Context, since the library only supports Graphs named with URIs these are translated into URIs of the following form:
    /// </para>
    /// <pre>
    /// nquads:bnode:12345678
    /// </pre>
    /// <pre>
    /// nquads:literal:87654321
    /// </pre>
    /// <para>
    /// In these URIs the numbers are the libraries hash codes for the node used as the Context.
    /// </para>
    /// </remarks>
    public class NQuadsParser
        : IRdfReader, ITraceableTokeniser, ITokenisingParser
    {
        /// <summary>
        /// Creates a new NQuads parser
        /// </summary>
        public NQuadsParser()
            : this(NQuadsSyntax.Rdf11) {}

        /// <summary>
        /// Creates a new NQuads parser
        /// </summary>
        /// <param name="syntax">NQuads syntax mode</param>
        public NQuadsParser(NQuadsSyntax syntax)
        {
            this.Syntax = syntax;
            TokenQueueMode = IOOptions.DefaultTokenQueueMode;
            TraceTokeniser = false;
        }

        /// <summary>
        /// Creates a new NQuads parser
        /// </summary>
        /// <param name="queueMode">Token Queue Mode</param>
        public NQuadsParser(TokenQueueMode queueMode)
            : this(NQuadsSyntax.Rdf11)
        {
            this.TokenQueueMode = queueMode;
        }

        /// <summary>
        /// Creates a new NQuads parser
        /// </summary>
        /// <param name="queueMode">Token Queue Mode</param>
        /// <param name="syntax">NQuads syntax mode</param>
        public NQuadsParser(NQuadsSyntax syntax, TokenQueueMode queueMode)
            : this(syntax)
        {
            this.TokenQueueMode = queueMode;
        }

        /// <summary>
        /// Gets/Sets whether Tokeniser Tracing is used
        /// </summary>
        public bool TraceTokeniser { get; set; }

        /// <summary>
        /// Gets/Sets the token queue mode used
        /// </summary>
        public TokenQueueMode TokenQueueMode { get; set; }

        /// <summary>
        /// Gets/Sets the NQuads syntax mode
        /// </summary>
        public NQuadsSyntax Syntax { get; set; }

        /// <summary>
        /// Loads a RDF Dataset from the NQuads input using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">Input to load from</param>
        /// <param name="profile"></param>
        public void Load(IRdfHandler handler, TextReader input, IParserProfile profile)
        {
            if (handler == null) throw new RdfParseException("Cannot parse an RDF Dataset using a null handler");
            if (input == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null input");

            try
            {
                // Check for incorrect stream encoding and issue warning if appropriate
                switch (this.Syntax)
                {
                    case NQuadsSyntax.Original:
#if !SILVERLIGHT
                        input.CheckEncoding(Encoding.ASCII, this.RaiseWarning);
#endif
                        break;
                    default:
                        input.CheckEncoding(Encoding.UTF8, this.RaiseWarning);
                        break;
                }
                profile = profile.EnsureParserProfile();

                TokenisingParserContext context = new TokenisingParserContext(handler, new NTriplesTokeniser(input, AsNTriplesSyntax(this.Syntax)), this.TokenQueueMode, false, this.TraceTokeniser, profile);
                this.Parse(context);
            }
            finally
            {
                input.CloseQuietly();
            }
        }

        /// <summary>
        /// Converts syntax enumeration values from NQuads to NTriples
        /// </summary>
        /// <param name="syntax">NQuads Syntax</param>
        /// <returns></returns>
        internal static NTriplesSyntax AsNTriplesSyntax(NQuadsSyntax syntax)
        {
            switch (syntax)
            {
                case NQuadsSyntax.Original:
                    return NTriplesSyntax.Original;
                default:
                    return NTriplesSyntax.Rdf11;
            }
        }

        private void Parse(TokenisingParserContext context)
        {
            try
            {
                context.Handler.StartRdf();
                ParserHelper.HandleInitialState(context);

                //Expect a BOF token at start
                IToken next = context.Tokens.Dequeue();
                if (next.TokenType != Token.BOF)
                {
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a BOF token at the start of the input", next);
                }

                do
                {
                    next = context.Tokens.Peek();
                    if (next.TokenType == Token.EOF) return;

                    // TODO Move Token -> Node logic into individual parsing methods
                    IToken s = this.TryParseSubject(context);
                    IToken p = this.TryParsePredicate(context);
                    IToken o = this.TryParseObject(context);
                    INode g = this.TryParseGraphName(context);

                    this.TryParseQuad(context, s, p, o, g);

                    next = context.Tokens.Peek();
                } while (next.TokenType != Token.EOF);

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

        private IToken TryParseSubject(TokenisingParserContext context)
        {
            IToken next = context.Tokens.Dequeue();
            switch (next.TokenType)
            {
                case Token.BLANKNODEWITHID:
                case Token.URI:
                    //OK
                    return next;

                default:
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Blank Node/URI as the Subject of a Triple", next);
            }
        }

        private IToken TryParsePredicate(TokenisingParserContext context)
        {
            IToken next = context.Tokens.Dequeue();
            switch (next.TokenType)
            {
                case Token.URI:
                    //OK
                    return next;
                default:
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a URI as the Predicate of a Triple", next);
            }
        }

        private IToken TryParseObject(TokenisingParserContext context)
        {
            IToken next = context.Tokens.Dequeue();
            switch (next.TokenType)
            {
                case Token.BLANKNODEWITHID:
                case Token.LITERALWITHDT:
                case Token.LITERALWITHLANG:
                case Token.URI:
                    //OK
                    return next;

                case Token.LITERAL:
                    //Check for Datatype/Language
                    IToken temp = context.Tokens.Peek();
                    if (temp.TokenType == Token.DATATYPE)
                    {
                        context.Tokens.Dequeue();
                        return new LiteralWithDataTypeToken(next, (DataTypeToken) temp);
                    }
                    if (temp.TokenType == Token.LANGSPEC)
                    {
                        context.Tokens.Dequeue();
                        return new LiteralWithLanguageSpecifierToken(next, (LanguageSpecifierToken) temp);
                    }
                    return next;
                default:
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Blank Node/Literal/URI as the Object of a Triple", next);
            }
        }

        private INode TryParseGraphName(TokenisingParserContext context)
        {
            IToken next = context.Tokens.Dequeue();
            if (next.TokenType == Token.DOT)
            {
                return Quad.DefaultGraphNode;
            }
            INode graph;
            switch (next.TokenType)
            {
                case Token.BLANKNODEWITHID:
                    graph = context.Handler.CreateBlankNode(context.BlankNodeGenerator.GetGuid(next.Value.Substring(2)));
                    break;
                case Token.URI:
                    graph = context.Handler.CreateUriNode(UriFactory.Create(next.Value));
                    break;
                case Token.LITERAL:
                    if (this.Syntax != NQuadsSyntax.Original) throw new RdfParseException("Only a Blank Node/URI may be used as the graph name in RDF NQuads 1.1");

                    //Check for Datatype/Language
                    IToken temp = context.Tokens.Peek();
                    switch (temp.TokenType)
                    {
                        case Token.LANGSPEC:
                            context.Tokens.Dequeue();
                            graph = context.Handler.CreateLiteralNode(next.Value, temp.Value);
                            break;
                        case Token.DATATYPE:
                            context.Tokens.Dequeue();
                            graph = context.Handler.CreateLiteralNode(next.Value, UriFactory.Create(temp.Value.Substring(1, temp.Value.Length - 2)));
                            break;
                        default:
                            graph = context.Handler.CreateLiteralNode(next.Value);
                            break;
                    }
                    break;
                default:
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Blank Node/Literal/URI as the Graph Name for the Quad", next);
            }

            //Ensure we then see a . to terminate the Quad
            next = context.Tokens.Dequeue();
            if (next.TokenType != Token.DOT)
            {
                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Dot Token (Line Terminator) to terminate a Quad", next);
            }

            return graph;
        }

        private void TryParseQuad(TokenisingParserContext context, IToken s, IToken p, IToken o, INode graphName)
        {
            INode subj, pred, obj;

            switch (s.TokenType)
            {
                case Token.BLANKNODEWITHID:
                    subj = context.Handler.CreateBlankNode(context.BlankNodeGenerator.GetGuid(s.Value.Substring(2)));
                    break;
                case Token.URI:
                    subj = TryParseUri(context, s.Value);
                    break;
                default:
                    throw ParserHelper.Error("Unexpected Token '" + s.GetType().ToString() + "' encountered, expected a Blank Node/URI as the Subject of a Triple", s);
            }

            switch (p.TokenType)
            {
                case Token.URI:
                    pred = ParserHelper.TryResolveUri(context, p);
                    break;
                default:
                    throw ParserHelper.Error("Unexpected Token '" + p.GetType().ToString() + "' encountered, expected a URI as the Predicate of a Triple", p);
            }

            switch (o.TokenType)
            {
                case Token.BLANKNODEWITHID:
                    obj = context.Handler.CreateBlankNode(context.BlankNodeGenerator.GetGuid(o.Value.Substring(2)));
                    break;
                case Token.LITERAL:
                    obj = context.Handler.CreateLiteralNode(o.Value);
                    break;
                case Token.LITERALWITHDT:
                    String dtUri = ((LiteralWithDataTypeToken) o).DataType;
                    obj = context.Handler.CreateLiteralNode(o.Value, TryParseUri(context, dtUri.Substring(1, dtUri.Length - 2)).Uri);
                    break;
                case Token.LITERALWITHLANG:
                    obj = context.Handler.CreateLiteralNode(o.Value, ((LiteralWithLanguageSpecifierToken) o).Language);
                    break;
                case Token.URI:
                    obj = TryParseUri(context, o.Value);
                    break;
                default:
                    throw ParserHelper.Error("Unexpected Token '" + o.GetType().ToString() + "' encountered, expected a Blank Node/Literal/URI as the Object of a Triple", o);
            }

            if (!context.Handler.HandleQuad(new Quad(subj, pred, obj, graphName))) ParserHelper.Stop();
        }

        /// <summary>
        /// Tries to parse a URI
        /// </summary>
        /// <param name="context">Parser context</param>
        /// <param name="uri">URI</param>
        /// <returns>URI Node if parsed successfully</returns>
        private static INode TryParseUri(TokenisingParserContext context, String uri)
        {
            try
            {
                INode n = context.Handler.CreateUriNode(UriFactory.Create(uri));
                if (!n.Uri.IsAbsoluteUri)
                    throw new RdfParseException("NQuads does not permit relative URIs");
                return n;
            }
#if SILVERLIGHT
            catch (FormatException uriEx)
#else
            catch (UriFormatException uriEx)
#endif
            {
                throw new RdfParseException("Invalid URI encountered, see inner exception for details", uriEx);
            }
        }

        /// <summary>
        /// Helper method used to raise the Warning event if there is an event handler registered
        /// </summary>
        /// <param name="message">Warning message</param>
        private void RaiseWarning(String message)
        {
            RdfReaderWarning d = this.Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event which Readers can raise when they notice syntax that is ambigious/deprecated etc which can still be parsed
        /// </summary>
        public event RdfReaderWarning Warning;

        /// <summary>
        /// Gets the String representation of the Parser which is a description of the syntax it parses
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "NQuads";
        }
    }
}