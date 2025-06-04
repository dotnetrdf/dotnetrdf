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
using System.IO;
using System.Text;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Parsing;

/// <summary>
/// Possible NQuads Syntax modes.
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
    Rdf11,

    /// <summary>
    /// NQuads-Star
    /// </summary>
    Rdf11Star,
}

/// <summary>
/// Parser for parsing NQuads (NTriples with an additional Context i.e. Named Graphs).
/// </summary>
/// <remarks>
/// <para>
/// The Default Graph (if any) will be given the special Uri. <strong>nquads:default-graph</strong>
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
    /// Creates a new NQuads parser.
    /// </summary>
    public NQuadsParser()
        : this(NQuadsSyntax.Rdf11)
    {
    }

    /// <summary>
    /// Creates a new NQuads parser.
    /// </summary>
    /// <param name="syntax">NQuads syntax mode.</param>
    public NQuadsParser(NQuadsSyntax syntax)
    {
        Syntax = syntax;
        TraceTokeniser = false;
    }

    /// <summary>
    /// Creates a new NQuads parser.
    /// </summary>
    /// <param name="queueMode">Token Queue Mode.</param>
    public NQuadsParser(TokenQueueMode queueMode)
        : this(NQuadsSyntax.Rdf11)
    {
        TokenQueueMode = queueMode;
    }

    /// <summary>
    /// Creates a new NQuads parser.
    /// </summary>
    /// <param name="queueMode">Token Queue Mode.</param>
    /// <param name="syntax">NQuads syntax mode.</param>
    public NQuadsParser(NQuadsSyntax syntax, TokenQueueMode queueMode)
        : this(syntax)
    {
        TokenQueueMode = queueMode;
    }

    /// <summary>
    /// Gets/Sets whether Tokeniser Tracing is used.
    /// </summary>
    public bool TraceTokeniser { get; set; }

    /// <summary>
    /// Gets/Sets the token queue mode used.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    public TokenQueueMode TokenQueueMode { get; set; } = Options.DefaultTokenQueueMode; // TokenQueueMode.SynchronousBufferDuringParsing
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Gets/Sets the NQuads syntax mode.
    /// </summary>
    public NQuadsSyntax Syntax { get; set; }

    /// <summary>
    /// Loads a RDF Dataset from the NQuads input into the given Triple Store.
    /// </summary>
    /// <param name="store">Triple Store to load into.</param>
    /// <param name="filename">File to load from.</param>
    public void Load(ITripleStore store, string filename)
    {
        if (filename == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null file");
        if (store == null) throw new RdfParseException("Cannot parse an RDF Dataset into a null store");

        Load(new StoreHandler(store), filename, store.UriFactory);
    }

    /// <summary>
    /// Loads a RDF Dataset from the NQuads input into the given Triple Store.
    /// </summary>
    /// <param name="store">Triple Store to load into.</param>
    /// <param name="input">Input to load from.</param>
    public void Load(ITripleStore store, TextReader input)
    {
        if (store == null) throw new RdfParseException("Cannot parse an RDF Dataset into a null store");
        if (input == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null input");
        Load(new StoreHandler(store), input, store.UriFactory);
    }

    /// <summary>
    /// Loads a RDF Dataset from the NQuads input using a RDF Handler.
    /// </summary>
    /// <param name="handler">RDF Handler to use.</param>
    /// <param name="filename">File to load from.</param>
    public void Load(IRdfHandler handler, string filename)
    {
        Load(handler, filename, UriFactory.Root);
    }

    /// <summary>
    /// Loads an RDF dataset using an RDF handler.
    /// </summary>
    /// <param name="handler">RDF handler to use.</param>
    /// <param name="filename">File to load from.</param>
    /// <param name="uriFactory">URI factory to use.</param>
    public void Load(IRdfHandler handler, string filename, IUriFactory uriFactory)
    { 
        if (filename == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null file");
        if (uriFactory == null) throw new ArgumentNullException(nameof(uriFactory));

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
    /// Loads a RDF Dataset from the NQuads input using a RDF Handler.
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

        if (handler == null) throw new RdfParseException("Cannot parse an RDF Dataset using a null handler");
        if (input == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null input");
        if (uriFactory == null) throw new ArgumentNullException(nameof(uriFactory));
        // Check for incorrect stream encoding and issue warning if appropriate
        if (input is StreamReader reader)
        {
            switch (Syntax)
            {
                case NQuadsSyntax.Original:
                    // Issue a Warning if the Encoding of the Stream is not ASCII
                    if (!reader.CurrentEncoding.Equals(Encoding.ASCII))
                    {
                        RaiseWarning("Expected Input Stream to be encoded as ASCII but got a Stream encoded as " +
                                     reader.CurrentEncoding.EncodingName +
                                     " - Please be aware that parsing errors may occur as a result");
                    }

                    break;
                default:
                    if (!reader.CurrentEncoding.Equals(Encoding.UTF8))
                    {
                        RaiseWarning("Expected Input Stream to be encoded as UTF-8 but got a Stream encoded as " +
                                     reader.CurrentEncoding.EncodingName +
                                     " - Please be aware that parsing errors may occur as a result");
                    }

                    break;
            }
        }

        LoadInternal(handler, input, uriFactory);
    }

    private void LoadInternal(IRdfHandler handler, TextReader input, IUriFactory uriFactory)
    {
        try
        {
            // Setup Token Queue and Tokeniser
            var tokeniser = new NTriplesTokeniser(input, AsNTriplesSyntax(Syntax));
            // Invoke the Parser
            Parse(new TokenisingParserContext(handler, tokeniser, TokenQueueMode, false, TraceTokeniser, uriFactory));
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
    /// Converts syntax enumeration values from NQuads to NTriples.
    /// </summary>
    /// <param name="syntax">NQuads Syntax.</param>
    /// <returns></returns>
    internal static NTriplesSyntax AsNTriplesSyntax(NQuadsSyntax syntax)
    {
        switch (syntax)
        {
            case NQuadsSyntax.Original:
                return NTriplesSyntax.Original;
            case NQuadsSyntax.Rdf11:
                return NTriplesSyntax.Rdf11;
            default:
                return NTriplesSyntax.Rdf11Star;
        }
    }

    private void Parse(TokenisingParserContext context)
    {
        try
        {
            context.Handler.StartRdf();

            context.Tokens.InitialiseBuffer(10);

            // Expect a BOF token at start
            IToken next = context.Tokens.Dequeue();
            if (next.TokenType != Token.BOF)
            {
                throw ParserHelper.Error("Unexpected Token '" + next.GetType() + "' encountered, expected a BOF token at the start of the input", next);
            }

            // Expect Quads
            next = context.Tokens.Peek();
            while (next.TokenType != Token.EOF)
            {
                // Discard Comments
                while (next.TokenType == Token.COMMENT)
                {
                    context.Tokens.Dequeue();
                    next = context.Tokens.Peek();
                }
                if (next.TokenType == Token.EOF) break;

                TryParseQuad(context);

                next = context.Tokens.Peek();
            }

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


    private void TryParseQuad(TokenisingParserContext context)
    {
        INode subj = NTriplesParser.TryParseSubject(context);
        INode predicate = NTriplesParser.TryParsePredicate(context);
        INode obj = NTriplesParser.TryParseObject(context);
        IRefNode graphName = TryParseContext(context);
        if (!context.Handler.HandleQuad(new Triple(subj as IRefNode, predicate as IRefNode, obj), graphName))
        {
            ParserHelper.Stop();
        }
    }


    private IRefNode TryParseContext(TokenisingParserContext context)
    {
        IToken next = context.Tokens.Dequeue();
        if (next.TokenType == Token.DOT)
        {
            return null;
        }
        INode graphContext;
        switch (next.TokenType)
        {
            case Token.BLANKNODEWITHID:
                graphContext = context.Handler.CreateBlankNode(next.Value.Substring(2));
                break;
            case Token.URI:
                graphContext = NTriplesParser.TryParseUri(context, next.Value);
                break;
            case Token.LITERAL:
                if (Syntax != NQuadsSyntax.Original) throw new RdfParseException("Only a Blank Node/URI may be used as the graph name in RDF NQuads 1.1");

                // Check for Datatype/Language
                IToken temp = context.Tokens.Peek();
                switch (temp.TokenType)
                {
                    case Token.LANGSPEC:
                        context.Tokens.Dequeue();
                        graphContext = context.Handler.CreateLiteralNode(next.Value, temp.Value);
                        break;
                    case Token.DATATYPE:
                        context.Tokens.Dequeue();
                        graphContext = context.Handler.CreateLiteralNode(next.Value, ((IUriNode) NTriplesParser.TryParseUri(context, temp.Value.Substring(1, temp.Value.Length - 2))).Uri);
                        break;
                    default:
                        graphContext = context.Handler.CreateLiteralNode(next.Value);
                        break;
                }
                break;
            default:
                throw ParserHelper.Error("Unexpected Token '" + next.GetType() + "' encountered, expected a Blank Node/Literal/URI as the Context of the Triple", next);
        }

        // Ensure we then see a . to terminate the Quad
        next = context.Tokens.Dequeue();
        if (next.TokenType != Token.DOT)
        {
            throw ParserHelper.Error("Unexpected Token '" + next.GetType() + "' encountered, expected a Dot Token (Line Terminator) to terminate a Triple", next);
        }

        // Finally return the Context URI
        return graphContext.NodeType switch
        {
            NodeType.Uri => graphContext as IUriNode,
            NodeType.Blank => graphContext as IBlankNode,
            NodeType.Literal => context.Handler.CreateUriNode(context.UriFactory.Create("nquads:literal:" + graphContext.GetHashCode())),
            _ => throw ParserHelper.Error(
                "Cannot turn a Node of type '" + graphContext.GetType() + "' into a Context URI for a Triple",
                next)
        };
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
        return "NQuads";
    }
}