/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
        {
            TokenQueueMode = IOOptions.DefaultTokenQueueMode;
            TraceTokeniser = false;
        }

        /// <summary>
        /// Creates a new NQuads parser
        /// </summary>
        /// <param name="queueMode">Token Queue Mode</param>
        public NQuadsParser(TokenQueueMode queueMode)
        {
            TraceTokeniser = false;
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

        public void Load(IRdfHandler handler, StreamReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read RDF into a null RDF Handler");
            if (input == null) throw new RdfParseException("Cannot read RDF from a null Stream");

#if !SILVERLIGHT
            //Issue a Warning if the Encoding of the Stream is not ASCII
            if (!input.CurrentEncoding.Equals(Encoding.ASCII))
            {
                this.RaiseWarning("Expected Input Stream to be encoded as ASCII but got a Stream encoded as " + input.CurrentEncoding.EncodingName + " - Please be aware that parsing errors may occur as a result");
            }
#endif

            this.Load(handler, (TextReader)input);
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

            try
            {
                TokenisingParserContext context = new TokenisingParserContext(handler, new NTriplesTokeniser(input), this.TokenQueueMode, false, this.TraceTokeniser); 
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
                    //No catch actions - just cleaning up
                }
            }
        }

        private void Parse(TokenisingParserContext context)
        {
            try
            {
                context.Handler.StartRdf();

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
                        return new LiteralWithDataTypeToken(next, (DataTypeToken)temp);
                    }
                    else if (temp.TokenType == Token.LANGSPEC)
                    {
                        context.Tokens.Dequeue();
                        return new LiteralWithLanguageSpecifierToken(next, (LanguageSpecifierToken)temp);
                    }
                    else
                    {
                        return next;
                    }
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
            else
            {
                INode graph;
                switch (next.TokenType)
                {
                    case Token.BLANKNODEWITHID:
                        graph = context.BlankNodeGenerator.CreateBlankNode(next.Value.Substring(2));
                        break;
                    case Token.URI:
                        graph = context.Handler.CreateUriNode(UriFactory.Create(next.Value));
                        break;
                    case Token.LITERAL:
                        //Check for Datatype/Language
                        IToken temp = context.Tokens.Peek();
                        if (temp.TokenType == Token.LANGSPEC)
                        {
                            context.Tokens.Dequeue();
                            graph = new LiteralNode(next.Value, temp.Value);
                        }
                        else if (temp.TokenType == Token.DATATYPE)
                        {
                            context.Tokens.Dequeue();
                            graph = new LiteralNode(next.Value, UriFactory.Create(temp.Value.Substring(1, temp.Value.Length - 2)));
                        }
                        else
                        {
                            graph = new LiteralNode(next.Value);
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
        }

        private void TryParseQuad(TokenisingParserContext context, IToken s, IToken p, IToken o, INode graphName)
        {
            INode subj, pred, obj;

            switch (s.TokenType)
            {
                case Token.BLANKNODEWITHID:
                    subj = context.BlankNodeGenerator.CreateBlankNode(s.Value.Substring(2));
                    break;
                case Token.URI:
                    subj = ParserHelper.TryResolveUri(context, s);
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
                    obj = context.BlankNodeGenerator.CreateBlankNode(o.Value.Substring(2));
                    break;
                case Token.LITERAL:
                    obj = context.Handler.CreateLiteralNode(o.Value);
                    break;
                case Token.LITERALWITHDT:
                    String dtUri = ((LiteralWithDataTypeToken)o).DataType;
                    obj = context.Handler.CreateLiteralNode(o.Value, UriFactory.Create(dtUri.Substring(1, dtUri.Length - 2)));
                    break;
                case Token.LITERALWITHLANG:
                    obj = context.Handler.CreateLiteralNode(o.Value, ((LiteralWithLanguageSpecifierToken)o).Language);
                    break;
                case Token.URI:
                    obj = ParserHelper.TryResolveUri(context, o);
                    break;
                default:
                        throw ParserHelper.Error("Unexpected Token '" + o.GetType().ToString() + "' encountered, expected a Blank Node/Literal/URI as the Object of a Triple", o);
            }

            if (!context.Handler.HandleQuad(new Quad(subj, pred, obj, graphName))) ParserHelper.Stop();
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
