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
    /// Possible NTriples syntax modes
    /// </summary>
    public enum NTriplesSyntax
    {
        /// <summary>
        /// The original NTriples syntax as specified in the original RDF specification <a href="http://www.w3.org/TR/2004/REC-rdf-testcases-20040210/">test cases</a> specification
        /// </summary>
        Original,

        /// <summary>
        /// Standardized NTriples as specified in the <a href="http://www.w3.org/TR/n-triples/">RDF 1.1 NTriples</a> specification
        /// </summary>
        Rdf11
    }

    /// <summary>
    /// Parser for NTriples syntax
    /// </summary>
    /// <threadsafety instance="true">Designed to be Thread Safe - should be able to call Load from multiple threads on different Graphs without issue</threadsafety>
    public class NTriplesParser
        : IRdfReader, ITraceableParser, ITraceableTokeniser, ITokenisingParser
    {
        #region Initialisation, Variables and Properties

        /// <summary>
        /// Creates a new instance of the parser
        /// </summary>
        public NTriplesParser()
            : this(NTriplesSyntax.Rdf11)
        {
        }

        /// <summary>
        /// Creates a new instance of the parser
        /// </summary>
        /// <param name="syntax">NTriples syntax to parse</param>
        public NTriplesParser(NTriplesSyntax syntax)
        {
            TokenQueueMode = Options.DefaultTokenQueueMode;
            TraceParsing = false;
            TraceTokeniser = false;
            Syntax = syntax;
        }

        /// <summary>
        /// Creates a new instance of the parser using the given token queue mode
        /// </summary>
        /// <param name="qmode">Token Queue Mode</param>
        public NTriplesParser(TokenQueueMode qmode)
            : this()
        {
            TokenQueueMode = qmode;
        }

        /// <summary>
        /// Creates a new instance of the parser using the given syntax and token queue mode
        /// </summary>
        /// 
        /// <param name="qmode">Token Queue Mode</param>
        /// <param name="syntax">NTriples syntax to parse</param>
        public NTriplesParser(NTriplesSyntax syntax, TokenQueueMode qmode)
            : this(syntax)
        {
            TokenQueueMode = qmode;
        }

        /// <summary>
        /// Controls whether Tokeniser progress will be traced by writing output to the Console
        /// </summary>
        public bool TraceTokeniser { get; set; }

        /// <summary>
        /// Controls whether Parser progress will be traced by writing output to the Console
        /// </summary>
        public bool TraceParsing { get; set; }

        /// <summary>
        /// Gets/Sets the token queue mode used
        /// </summary>
        public TokenQueueMode TokenQueueMode { get; set; }

        /// <summary>
        /// Gets/Sets the desired NTriples syntax
        /// </summary>
        public NTriplesSyntax Syntax { get; set; }

        #endregion

        /// <summary>
        /// Parses NTriples Syntax from the given Input Stream into Triples in the given Graph
        /// </summary>
        /// <param name="g">Graph to create Triples in</param>
        /// <param name="input">Arbitrary Input Stream to read input from</param>
        public void Load(IGraph g, StreamReader input)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");

            Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Parses NTriples Syntax from the given Input into Triples in the given Graph
        /// </summary>
        /// <param name="g">Graph to create Triples in</param>
        /// <param name="input">Arbitrary Input to read input from</param>
        public void Load(IGraph g, TextReader input)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");

            Load(new GraphHandler(g), input);
        }

        /// <summary>
        /// Parses NTriples Syntax from the given File into Triples in the given Graph
        /// </summary>
        /// <param name="g">Graph to create Triples in</param>
        /// <param name="filename">Name of the file containing Turtle Syntax</param>
        /// <remarks>Simply opens an StreamReader and uses the overloaded version of this function</remarks>
        public void Load(IGraph g, string filename)
        {
            if (g == null) throw new RdfParseException("Cannot read RDF into a null Graph");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");

            // Can only open Streams as ASCII when not running under Silverlight as Silverlight has no ASCII support
            // However if we are parsing RDF 1.1 NTriples then we use UTF-8 anyway so that doesn't matter
            StreamReader input;
            switch (Syntax)
            {
                case NTriplesSyntax.Original:
                    // Original NTriples uses ASCII encoding
                    input = new StreamReader(File.OpenRead(filename), Encoding.ASCII);
                    break;
                default:
                    // RDF 1.1 NTriples uses UTF-8 encoding
                    input = new StreamReader(File.OpenRead(filename), Encoding.UTF8);
                    break;
            }
            Load(g, input);
        }

        /// <summary>
        /// Parses NTriples Syntax from the given Input Stream using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">Input Stream to read input from</param>
        public void Load(IRdfHandler handler, StreamReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannot read RDF from a null Stream");

            // Check for incorrect stream encoding and issue warning if appropriate
            switch (Syntax)
            {
                case NTriplesSyntax.Original:
                    // Issue a Warning if the Encoding of the Stream is not ASCII
                    if (!input.CurrentEncoding.Equals(Encoding.ASCII))
                    {
                        RaiseWarning("Expected Input Stream to be encoded as ASCII but got a Stream encoded as " + input.CurrentEncoding.EncodingName + " - Please be aware that parsing errors may occur as a result");
                    }
                    break;
                default:
                    if (!input.CurrentEncoding.Equals(Encoding.UTF8))
                    {
                        RaiseWarning("Expected Input Stream to be encoded as UTF-8 but got a Stream encoded as " + input.CurrentEncoding.EncodingName + " - Please be aware that parsing errors may occur as a result");
                    }
                    break;
            }

            Load(handler, (TextReader) input);
        }

        /// <summary>
        /// Parses NTriples Syntax from the given Input using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">Input to read input from</param>
        public void Load(IRdfHandler handler, TextReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannot read RDF from a null TextReader");

            try
            {
                TokenisingParserContext context = new TokenisingParserContext(handler, new NTriplesTokeniser(input, Syntax), TokenQueueMode, TraceParsing, TraceTokeniser);
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
        /// Parses NTriples Syntax from the given file using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="filename">File to read from</param>
        public void Load(IRdfHandler handler, String filename)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (filename == null) throw new RdfParseException("Cannot read RDF from a null File");
            Load(handler, new StreamReader(File.OpenRead(filename), Encoding.UTF8));
        }

        private void Parse(TokenisingParserContext context)
        {
            try
            {
                context.Handler.StartRdf();

                // Initialise the Buffer
                context.Tokens.InitialiseBuffer(10);

                // Expect a BOF
                IToken start = context.Tokens.Dequeue();
                if (start.TokenType != Token.BOF)
                {
                    throw Error("Unexpected Token '" + start.GetType().ToString() + "' encountered, expected a Beginning of File Token", start);
                }

                // Expect Triples
                IToken next = context.Tokens.Peek();
                while (next.TokenType != Token.EOF)
                {
                    // Discard Comments
                    while (next.TokenType == Token.COMMENT)
                    {
                        context.Tokens.Dequeue();
                        next = context.Tokens.Peek();
                    }
                    if (next.TokenType == Token.EOF) break;

                    TryParseTriple(context);

                    next = context.Tokens.Peek();
                }

                context.Handler.EndRdf(true);
            }
            catch (RdfParsingTerminatedException)
            {
                context.Handler.EndRdf(true);
                // Discard this - it justs means the Handler told us to stop
            }
            catch (RdfParseException)
            {
                // We hit some Parsing error
                context.Handler.EndRdf(false);
                throw;
            }
        }

        private void TryParseTriple(TokenisingParserContext context)
        {
            // Get the Subject, Predicate and Object
            INode subj = TryParseSubject(context);
            INode pred = TryParsePredicate(context);
            INode obj = TryParseObject(context);

            // Ensure we're terminated by a DOT
            TryParseLineTerminator(context);

            // Assert the Triple
            if (!context.Handler.HandleTriple(new Triple(subj, pred, obj))) ParserHelper.Stop();
        }

        private INode TryParseSubject(TokenisingParserContext context)
        {
            IToken subjToken = context.Tokens.Dequeue();

            // Discard Comments
            while (subjToken.TokenType == Token.COMMENT)
            {
                subjToken = context.Tokens.Dequeue();
            }

            switch (subjToken.TokenType)
            {
                case Token.BLANKNODE:
                    return context.Handler.CreateBlankNode();
                case Token.BLANKNODEWITHID:
                    return context.Handler.CreateBlankNode(subjToken.Value.Substring(2));
                case Token.URI:
                    return TryParseUri(context, subjToken.Value);
                case Token.LITERAL:
                case Token.LITERALWITHDT:
                case Token.LITERALWITHLANG:
                    throw Error("Subject cannot be a Literal in NTriples", subjToken);
                default:
                    throw Error("Unexpected Token '" + subjToken.GetType().ToString() + "' encountered, expected a Blank Node or URI for the Subject of a Triple", subjToken);
            }
        }

        private INode TryParsePredicate(TokenisingParserContext context)
        {
            IToken predToken = context.Tokens.Dequeue();

            // Discard Comments
            while (predToken.TokenType == Token.COMMENT)
            {
                predToken = context.Tokens.Dequeue();
            }

            switch (predToken.TokenType)
            {
                case Token.BLANKNODE:
                case Token.BLANKNODEWITHID:
                    throw Error("Predicate cannot be a Blank Node in NTriples", predToken);
                case Token.URI:
                    return TryParseUri(context, predToken.Value);
                case Token.LITERAL:
                case Token.LITERALWITHDT:
                case Token.LITERALWITHLANG:
                    throw Error("Predicate cannot be a Literal in NTriples", predToken);
                default:
                    throw Error("Unexpected Token '" + predToken.GetType().ToString() + "' encountered, expected a URI for the Predicate of a Triple", predToken);
            }
        }

        private INode TryParseObject(TokenisingParserContext context)
        {
            IToken objToken = context.Tokens.Dequeue();

            // Discard Comments
            while (objToken.TokenType == Token.COMMENT)
            {
                objToken = context.Tokens.Dequeue();
            }

            switch (objToken.TokenType)
            {
                case Token.BLANKNODE:
                    return context.Handler.CreateBlankNode();
                case Token.BLANKNODEWITHID:
                    return context.Handler.CreateBlankNode(objToken.Value.Substring(2));
                case Token.URI:
                    return TryParseUri(context, objToken.Value);
                case Token.LITERALWITHDT:
                    String dt = ((LiteralWithDataTypeToken) objToken).DataType;
                    dt = dt.Substring(1, dt.Length - 2);
                    return context.Handler.CreateLiteralNode(objToken.Value, ((IUriNode)TryParseUri(context, dt)).Uri);
                case Token.LITERALWITHLANG:
                    return context.Handler.CreateLiteralNode(objToken.Value, ((LiteralWithLanguageSpecifierToken) objToken).Language);
                case Token.LITERAL:
                    IToken next = context.Tokens.Peek();
                    // Is there a Language Specifier or Data Type?
                    switch (next.TokenType)
                    {
                        case Token.LANGSPEC:
                            context.Tokens.Dequeue();
                            return context.Handler.CreateLiteralNode(objToken.Value, next.Value);
                        case Token.URI:
                            context.Tokens.Dequeue();
                            return context.Handler.CreateLiteralNode(objToken.Value, ((IUriNode)TryParseUri(context, next.Value)).Uri);
                        default:
                            return context.Handler.CreateLiteralNode(objToken.Value);
                    }

                default:
                    throw Error("Unexpected Token '" + objToken.GetType().ToString() + "' encountered, expected a Blank Node, Literal or URI for the Object of a Triple", objToken);
            }
        }

        private static void TryParseLineTerminator(TokenisingParserContext context)
        {
            IToken next = context.Tokens.Dequeue();

            // Discard Comments
            while (next.TokenType == Token.COMMENT)
            {
                next = context.Tokens.Dequeue();
            }

            // Ensure we finish with a Dot terminator
            if (next.TokenType != Token.DOT)
            {
                throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Dot Line Terminator to terminate a Triple", next);
            }
        }

        /// <summary>
        /// Tries to parse a URI
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="uri">URI</param>
        /// <returns>URI Node if parsed successfully</returns>
        private static INode TryParseUri(TokenisingParserContext context, String uri)
        {
            try
            {
                IUriNode n = context.Handler.CreateUriNode(UriFactory.Create(uri));
                if (!n.Uri.IsAbsoluteUri)
                    throw new RdfParseException("NTriples does not permit relative URIs");
                return n;
            }
            catch (UriFormatException uriEx)
            {
                throw new RdfParseException("Invalid URI encountered, see inner exception for details", uriEx);
            }
        }

        /// <summary>
        /// Helper method for raising informative standardised Parser Errors
        /// </summary>
        /// <param name="msg">The Error Message</param>
        /// <param name="t">The Token that is the cause of the Error</param>
        /// <returns></returns>
        private static RdfParseException Error(String msg, IToken t)
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
        /// Internal Helper method which raises the Warning event if an event handler is registered to it
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            if (Warning != null)
            {
                // Raise Event
                Warning(message);
            }
        }

        /// <summary>
        /// Event which is raised when there is a non-fatal issue with the NTriples being parsed
        /// </summary>
        public event RdfReaderWarning Warning;

        /// <summary>
        /// Gets the String representation of the Parser which is a description of the syntax it parses
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "NTriples";
        }
    }
}