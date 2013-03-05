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
        : IStoreReader, ITraceableTokeniser
    {
        private bool _tracetokeniser = false;

        /// <summary>
        /// Gets/Sets whether Tokeniser Tracing is used
        /// </summary>
        public bool TraceTokeniser
        {
            get
            {
                return this._tracetokeniser;
            }
            set
            {
                this._tracetokeniser = value;
            }
        }

#if !NO_FILE
        /// <summary>
        /// Loads a RDF Dataset from the NQuads input into the given Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="filename">File to load from</param>
        public void Load(ITripleStore store, String filename)
        {
            if (filename == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null file");
#if !SILVERLIGHT
            this.Load(store, new StreamReader(filename, Encoding.ASCII));
#else
            this.Load(store, new StreamReader(filename));
#endif
        }
#endif

        /// <summary>
        /// Loads a RDF Dataset from the NQuads input into the given Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="input">Input to load from</param>
        public void Load(ITripleStore store, TextReader input)
        {
            if (store == null) throw new RdfParseException("Cannot parse an RDF Dataset into a null store");
            if (input == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null input");
            this.Load(new StoreHandler(store), input);
        }

#if !NO_FILE
        /// <summary>
        /// Loads a RDF Dataset from the NQuads input using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="filename">File to load from</param>
        public void Load(IRdfHandler handler, String filename)
        {
            if (filename == null) throw new RdfParseException("Cannot parse an RDF Dataset from a null file");
#if !SILVERLIGHT
            this.Load(handler, new StreamReader(filename, Encoding.ASCII));
#else
            this.Load(handler, new StreamReader(filename));
#endif
        }
#endif

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
                //Setup Token Queue and Tokeniser
                NTriplesTokeniser tokeniser = new NTriplesTokeniser(input);
                tokeniser.NQuadsMode = true;
                TokenQueue tokens = new TokenQueue();
                tokens.Tokeniser = tokeniser;
                tokens.Tracing = this._tracetokeniser;
                tokens.InitialiseBuffer();

                //Invoke the Parser
                this.Parse(handler, tokens);
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

        private void Parse(IRdfHandler handler, ITokenQueue tokens)
        {
            IToken next;
            IToken s, p, o;

            try
            {
                handler.StartRdf();

                //Expect a BOF token at start
                next = tokens.Dequeue();
                if (next.TokenType != Token.BOF)
                {
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a BOF token at the start of the input", next);
                }

                do
                {
                    next = tokens.Peek();
                    if (next.TokenType == Token.EOF) return;

                    s = this.TryParseSubject(tokens);
                    p = this.TryParsePredicate(tokens);
                    o = this.TryParseObject(tokens);
                    Uri context = this.TryParseContext(tokens);

                    this.TryParseTriple(handler, s, p, o, context);

                    next = tokens.Peek();
                } while (next.TokenType != Token.EOF);

                handler.EndRdf(true);
            }
            catch (RdfParsingTerminatedException)
            {
                handler.EndRdf(true);
                //Discard this - it justs means the Handler told us to stop
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
                    //OK
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
                    //OK
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
                    //OK
                    return next;

                case Token.LITERAL:
                    //Check for Datatype/Language
                    IToken temp = tokens.Peek();
                    if (temp.TokenType == Token.DATATYPE)
                    {
                        tokens.Dequeue();
                        return new LiteralWithDataTypeToken(next, (DataTypeToken)temp);
                    }
                    else if (temp.TokenType == Token.LANGSPEC)
                    {
                        tokens.Dequeue();
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

        private Uri TryParseContext(ITokenQueue tokens)
        {
            IToken next = tokens.Dequeue();
            if (next.TokenType == Token.DOT)
            {
                return null;
            }
            else
            {
                INode context;
                switch (next.TokenType)
                {
                    case Token.BLANKNODEWITHID:
                        context = new BlankNode(null, next.Value.Substring(2));
                        break;
                    case Token.URI:
                        context = new UriNode(null, UriFactory.Create(next.Value));
                        break;
                    case Token.LITERAL:
                        //Check for Datatype/Language
                        IToken temp = tokens.Peek();
                        if (temp.TokenType == Token.LANGSPEC)
                        {
                            tokens.Dequeue();
                            context = new LiteralNode(null, next.Value, temp.Value);
                        }
                        else if (temp.TokenType == Token.DATATYPE)
                        {
                            tokens.Dequeue();
                            context = new LiteralNode(null, next.Value, UriFactory.Create(temp.Value.Substring(1, temp.Value.Length - 2)));
                        }
                        else
                        {
                            context = new LiteralNode(null, next.Value);
                        }
                        break;
                    default:
                        throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Blank Node/Literal/URI as the Context of the Triple", next);
                }

                //Ensure we then see a . to terminate the Quad
                next = tokens.Dequeue();
                if (next.TokenType != Token.DOT)
                {
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Dot Token (Line Terminator) to terminate a Triple", next);
                }

                //Finally return the Context URI
                if (context.NodeType == NodeType.Uri)
                {
                    return ((IUriNode)context).Uri;
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
                    subj = ParserHelper.TryResolveUri(handler, s);
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
                    String dtUri = ((LiteralWithDataTypeToken)o).DataType;
                    obj = handler.CreateLiteralNode(o.Value, UriFactory.Create(dtUri.Substring(1, dtUri.Length - 2)));
                    break;
                case Token.LITERALWITHLANG:
                    obj = handler.CreateLiteralNode(o.Value, ((LiteralWithLanguageSpecifierToken)o).Language);
                    break;
                case Token.URI:
                    obj = ParserHelper.TryResolveUri(handler, o);
                    break;
                default:
                        throw ParserHelper.Error("Unexpected Token '" + o.GetType().ToString() + "' encountered, expected a Blank Node/Literal/URI as the Object of a Triple", o);
            }

            if (!handler.HandleTriple(new Triple(subj, pred, obj, graphUri))) ParserHelper.Stop();
        }

        /// <summary>
        /// Helper method used to raise the Warning event if there is an event handler registered
        /// </summary>
        /// <param name="message">Warning message</param>
        private void RaiseWarning(String message)
        {
            StoreReaderWarning d = this.Warning;
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
