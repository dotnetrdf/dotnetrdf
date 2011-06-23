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
using System.Text;
using System.IO;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;

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
    public class NQuadsParser : IStoreReader, ITraceableTokeniser
    {
        private bool _tracetokeniser = false;

        /// <summary>
        /// Default Graph Uri for default graphs parsed from NQuads input
        /// </summary>
        public const String DefaultGraphURI = "nquads:default-graph";

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

        /// <summary>
        /// Loads a RDF Dataset from the NQuads input into the given Triple Store
        /// </summary>
        /// <param name="store">Triple Store to load into</param>
        /// <param name="parameters">Parameters indicating the Stream to read from</param>
        public void Load(ITripleStore store, IStoreParams parameters)
        {
            if (store == null) throw new RdfParseException("Cannot read a RDF dataset into a null Store");

            this.Load(new StoreHandler(store), parameters);
        }

        /// <summary>
        /// Loads a RDF Dataset from the NQuads input using a RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="parameters">Parameters indicating the Stream to read from</param>
        public void Load(IRdfHandler handler, IStoreParams parameters)
        {
            if (handler == null) throw new ArgumentNullException("handler", "Cannot parse an RDF Dataset using a null RDF Handler");
            if (parameters == null) throw new ArgumentNullException("parameters", "Cannot parse an RDF Dataset using null Parameters");

            //Try and get the Input from the parameters
            TextReader input = null;
            if (parameters is StreamParams)
            {
                //Get Input Stream
                input = ((StreamParams)parameters).StreamReader;

#if !SILVERLIGHT
                //Issue a Warning if the Encoding of the Stream is not ASCII
                if (!((StreamReader)input).CurrentEncoding.Equals(Encoding.ASCII))
                {
                    this.RaiseWarning("Expected Input Stream to be encoded as ASCII but got a Stream encoded as " + ((StreamReader)input).CurrentEncoding.EncodingName + " - Please be aware that parsing errors may occur as a result");
                }
#endif
            } 
            else if (parameters is TextReaderParams)
            {
                input = ((TextReaderParams)parameters).TextReader;
            }

            if (input != null)
            {
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
            else
            {
                throw new RdfStorageException("Parameters for the NQuadsParser must be of the type StreamParams/TextReaderParams");
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
                        context = new UriNode(null, new Uri(next.Value));
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
                            context = new LiteralNode(null, next.Value, new Uri(temp.Value.Substring(1, temp.Value.Length - 2)));
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
                    return new Uri("nquads:bnode:" + context.GetHashCode());
                }
                else if (context.NodeType == NodeType.Literal)
                {
                    return new Uri("nquads:literal:" + context.GetHashCode());
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
                    obj = handler.CreateLiteralNode(o.Value, new Uri(dtUri.Substring(1, dtUri.Length - 2)));
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
