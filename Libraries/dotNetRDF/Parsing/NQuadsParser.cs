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
        : IStoreReader, ITraceableTokeniser, ITokenisingParser
    {
        /// <summary>
        /// Creates a new NQuads parser
        /// </summary>
        public NQuadsParser()
            : this(NQuadsSyntax.Rdf11)
        {
        }

        /// <summary>
        /// Creates a new NQuads parser
        /// </summary>
        /// <param name="syntax">NQuads syntax mode</param>
        public NQuadsParser(NQuadsSyntax syntax)
        {
            Syntax = syntax;
            TokenQueueMode = Options.DefaultTokenQueueMode;
            TraceTokeniser = false;
        }

        /// <summary>
        /// Creates a new NQuads parser
        /// </summary>
        /// <param name="queueMode">Token Queue Mode</param>
        public NQuadsParser(TokenQueueMode queueMode)
            : this(NQuadsSyntax.Rdf11)
        {
            TokenQueueMode = queueMode;
        }

        /// <summary>
        /// Creates a new NQuads parser
        /// </summary>
        /// <param name="queueMode">Token Queue Mode</param>
        /// <param name="syntax">NQuads syntax mode</param>
        public NQuadsParser(NQuadsSyntax syntax, TokenQueueMode queueMode)
            : this(syntax)
        {
            TokenQueueMode = queueMode;
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
        /// Loads a RDF Dataset from the NQuads input into the given Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="filename">File to load from</param>
        public void Load(ITripleStore store, String filename)
        {
            if (filename == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null file");
            if (store == null) throw new RdfParseException("Cannot parse an RDF Dataset into a null store");

            Load(new StoreHandler(store), filename);
        }

        /// <summary>
        /// Loads a RDF Dataset from the NQuads input into the given Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="input">Input to load from</param>
        public void Load(ITripleStore store, TextReader input)
        {
            if (store == null) throw new RdfParseException("Cannot parse an RDF Dataset into a null store");
            if (input == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null input");
            Load(new StoreHandler(store), input);
        }

        /// <summary>
        /// Loads a RDF Dataset from the NQuads input using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="filename">File to load from</param>
        public void Load(IRdfHandler handler, String filename)
        {
            if (filename == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null file");

            // Can only open Streams as ASCII when not running under Silverlight as Silverlight has no ASCII support
            // However if we are parsing RDF 1.1 NTriples then we use UTF-8 anyway so that doesn't matter
            StreamReader input;
            switch (Syntax)
            {
                case NQuadsSyntax.Original:
                    // Original NQuads uses ASCII encoding
                    input = new StreamReader(File.OpenRead(filename), Encoding.ASCII);
                    break;
                default:
                    // RDF 1.1 NQuads uses UTF-8 encoding
                    input = new StreamReader(File.OpenRead(filename), Encoding.UTF8);
                    break;
            }

            Load(handler, input);
        }

        /// <summary>
        /// Loads a RDF Dataset from the NQuads input using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">Input to load from</param>
        public void Load(IRdfHandler handler, TextReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot parse an RDF Dataset using a null handler");
            if (input == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null input");

            // Check for incorrect stream encoding and issue warning if appropriate
            if (input is StreamReader)
            {
                switch (Syntax)
                {
                    case NQuadsSyntax.Original:
                        // Issue a Warning if the Encoding of the Stream is not ASCII
                        if (!((StreamReader) input).CurrentEncoding.Equals(Encoding.ASCII))
                        {
                            RaiseWarning("Expected Input Stream to be encoded as ASCII but got a Stream encoded as " + ((StreamReader) input).CurrentEncoding.EncodingName + " - Please be aware that parsing errors may occur as a result");
                        }
                        break;
                    default:
                        if (!((StreamReader) input).CurrentEncoding.Equals(Encoding.UTF8))
                        {
                            RaiseWarning("Expected Input Stream to be encoded as UTF-8 but got a Stream encoded as " + ((StreamReader) input).CurrentEncoding.EncodingName + " - Please be aware that parsing errors may occur as a result");
                        }
                        break;
                }
            }

            try
            {
                // Setup Token Queue and Tokeniser
                NTriplesTokeniser tokeniser = new NTriplesTokeniser(input, AsNTriplesSyntax(Syntax));
                tokeniser.NQuadsMode = true;
                ITokenQueue tokens;
                switch (TokenQueueMode)
                {
                    case TokenQueueMode.AsynchronousBufferDuringParsing:
                        tokens = new AsynchronousBufferedTokenQueue(tokeniser);
                        break;
                    case TokenQueueMode.QueueAllBeforeParsing:
                        tokens = new TokenQueue(tokeniser);
                        break;
                    case TokenQueueMode.SynchronousBufferDuringParsing:
                    default:
                        tokens = new BufferedTokenQueue(tokeniser);
                        break;
                }
                tokens.Tracing = TraceTokeniser;
                tokens.InitialiseBuffer();

                // Invoke the Parser
                Parse(handler, tokens);
            }
            finally
            {
                try
                {
                    input.Close();
                }
                catch
                {
                    // No catch actions - just cleaning up
                }
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

        private void Parse(IRdfHandler handler, ITokenQueue tokens)
        {
            IToken next;
            IToken s, p, o;

            try
            {
                handler.StartRdf();

                // Expect a BOF token at start
                next = tokens.Dequeue();
                if (next.TokenType != Token.BOF)
                {
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a BOF token at the start of the input", next);
                }

                do
                {
                    next = tokens.Peek();
                    if (next.TokenType == Token.EOF) return;

                    s = TryParseSubject(tokens);
                    p = TryParsePredicate(tokens);
                    o = TryParseObject(tokens);
                    Uri context = TryParseContext(handler, tokens);

                    TryParseTriple(handler, s, p, o, context);

                    next = tokens.Peek();
                } while (next.TokenType != Token.EOF);

                handler.EndRdf(true);
            }
            catch (RdfParsingTerminatedException)
            {
                handler.EndRdf(true);
                // Discard this - it justs means the Handler told us to stop
            }
            catch
            {
                handler.EndRdf(false);
                throw;
            }
        }

        private IToken TryParseSubject(ITokenQueue tokens)
        {
            IToken next = tokens.Dequeue();
            switch (next.TokenType)
            {
                case Token.BLANKNODEWITHID:
                case Token.URI:
                    // OK
                    return next;

                default:
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Blank Node/URI as the Subject of a Triple", next);
            }
        }

        private IToken TryParsePredicate(ITokenQueue tokens)
        {
            IToken next = tokens.Dequeue();
            switch (next.TokenType)
            {
                case Token.URI:
                    // OK
                    return next;
                default:
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a URI as the Predicate of a Triple", next);
            }
        }

        private IToken TryParseObject(ITokenQueue tokens)
        {
            IToken next = tokens.Dequeue();
            switch (next.TokenType)
            {
                case Token.BLANKNODEWITHID:
                case Token.LITERALWITHDT:
                case Token.LITERALWITHLANG:
                case Token.URI:
                    // OK
                    return next;

                case Token.LITERAL:
                    // Check for Datatype/Language
                    IToken temp = tokens.Peek();
                    if (temp.TokenType == Token.DATATYPE)
                    {
                        tokens.Dequeue();
                        return new LiteralWithDataTypeToken(next, (DataTypeToken) temp);
                    }
                    else if (temp.TokenType == Token.LANGSPEC)
                    {
                        tokens.Dequeue();
                        return new LiteralWithLanguageSpecifierToken(next, (LanguageSpecifierToken) temp);
                    }
                    else
                    {
                        return next;
                    }
                default:
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Blank Node/Literal/URI as the Object of a Triple", next);
            }
        }

        private Uri TryParseContext(IRdfHandler handler, ITokenQueue tokens)
        {
            IToken next = tokens.Dequeue();
            if (next.TokenType == Token.DOT)
            {
                return null;
            }
            INode context;
            switch (next.TokenType)
            {
                case Token.BLANKNODEWITHID:
                    context = handler.CreateBlankNode(next.Value.Substring(2));
                    break;
                case Token.URI:
                    context = TryParseUri(handler, next.Value);
                    break;
                case Token.LITERAL:
                    if (Syntax != NQuadsSyntax.Original) throw new RdfParseException("Only a Blank Node/URI may be used as the graph name in RDF NQuads 1.1");

                    // Check for Datatype/Language
                    IToken temp = tokens.Peek();
                    switch (temp.TokenType)
                    {
                        case Token.LANGSPEC:
                            tokens.Dequeue();
                            context = handler.CreateLiteralNode(next.Value, temp.Value);
                            break;
                        case Token.DATATYPE:
                            tokens.Dequeue();
                            context = handler.CreateLiteralNode(next.Value, ((IUriNode) TryParseUri(handler, temp.Value.Substring(1, temp.Value.Length - 2))).Uri);
                            break;
                        default:
                            context = handler.CreateLiteralNode(next.Value);
                            break;
                    }
                    break;
                default:
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Blank Node/Literal/URI as the Context of the Triple", next);
            }

            // Ensure we then see a . to terminate the Quad
            next = tokens.Dequeue();
            if (next.TokenType != Token.DOT)
            {
                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Dot Token (Line Terminator) to terminate a Triple", next);
            }

            // Finally return the Context URI
            if (context.NodeType == NodeType.Uri)
            {
                return ((IUriNode) context).Uri;
            }
            else if (context.NodeType == NodeType.Blank)
            {
                return UriFactory.Create("nquads:bnode:" + context.GetHashCode());
            }
            else if (context.NodeType == NodeType.Literal)
            {
                return UriFactory.Create("nquads:literal:" + context.GetHashCode());
            }
            else
            {
                throw ParserHelper.Error("Cannot turn a Node of type '" + context.GetType().ToString() + "' into a Context URI for a Triple", next);
            }
        }

        private void TryParseTriple(IRdfHandler handler, IToken s, IToken p, IToken o, Uri graphUri)
        {
            INode subj, pred, obj;

            switch (s.TokenType)
            {
                case Token.BLANKNODEWITHID:
                    subj = handler.CreateBlankNode(s.Value.Substring(2));
                    break;
                case Token.URI:
                    subj = TryParseUri(handler, s.Value);
                    break;
                default:
                    throw ParserHelper.Error("Unexpected Token '" + s.GetType().ToString() + "' encountered, expected a Blank Node/URI as the Subject of a Triple", s);
            }

            switch (p.TokenType)
            {
                case Token.URI:
                    pred = ParserHelper.TryResolveUri(handler, p);
                    break;
                default:
                    throw ParserHelper.Error("Unexpected Token '" + p.GetType().ToString() + "' encountered, expected a URI as the Predicate of a Triple", p);
            }

            switch (o.TokenType)
            {
                case Token.BLANKNODEWITHID:
                    obj = handler.CreateBlankNode(o.Value.Substring(2));
                    break;
                case Token.LITERAL:
                    obj = handler.CreateLiteralNode(o.Value);
                    break;
                case Token.LITERALWITHDT:
                    String dtUri = ((LiteralWithDataTypeToken) o).DataType;
                    obj = handler.CreateLiteralNode(o.Value, ((IUriNode) TryParseUri(handler, dtUri.Substring(1, dtUri.Length - 2))).Uri);
                    break;
                case Token.LITERALWITHLANG:
                    obj = handler.CreateLiteralNode(o.Value, ((LiteralWithLanguageSpecifierToken) o).Language);
                    break;
                case Token.URI:
                    obj = TryParseUri(handler, o.Value);
                    break;
                default:
                    throw ParserHelper.Error("Unexpected Token '" + o.GetType().ToString() + "' encountered, expected a Blank Node/Literal/URI as the Object of a Triple", o);
            }

            if (!handler.HandleTriple(new Triple(subj, pred, obj, graphUri))) ParserHelper.Stop();
        }

        /// <summary>
        /// Tries to parse a URI
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="uri">URI</param>
        /// <returns>URI Node if parsed successfully</returns>
        private static INode TryParseUri(IRdfHandler handler, String uri)
        {
            try
            {
                IUriNode n = handler.CreateUriNode(UriFactory.Create(uri));
                if (!n.Uri.IsAbsoluteUri)
                    throw new RdfParseException("NQuads does not permit relative URIs");
                return n;
            }
            catch (UriFormatException uriEx)
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
        /// Gets the String representation of the Parser which is a description of the syntax it parses
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "NQuads";
        }
    }
}