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
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;

namespace VDS.Alexandria.Utilities
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
    class StreamingNQuadsParser
    {
        private ITokenQueue _tokens;
        private GraphFactory _factory;
        private IGraph _g = new Graph();
        private bool _bof = true, _eof = false;

        public StreamingNQuadsParser(GraphFactory factory, StreamReader reader)
        {
            this._factory = factory; ;

            NTriplesTokeniser tokeniser = new NTriplesTokeniser(reader);
            tokeniser.NQuadsMode = true;
            this._tokens = new BufferedTokenQueue(tokeniser);
            this._tokens.InitialiseBuffer();
        }

        public Triple GetNextTriple()
        {
            if (this._eof) throw new RdfParseException("Cannot continue parsing when the End of File has already been reached");

            IToken next;
            IToken s, p, o;
            Triple t;

            if (this._bof)
            {
                //Expect a BOF token at start
                next = this._tokens.Dequeue();
                if (next.TokenType != Token.BOF)
                {
                    throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a BOF token at the start of the input", next);
                }
                this._bof = false;
            }

            //Parse Subject
            next = this._tokens.Dequeue();
            switch (next.TokenType)
            {
                case Token.BLANKNODEWITHID:
                case Token.URI:
                    //OK
                    s = next;
                    break;
                default:
                    throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Blank Node/URI as the Subject of a Triple", next);
            }

            //Parse Predicate
            next = this._tokens.Dequeue();
            switch (next.TokenType)
            {
                case Token.URI:
                    //OK
                    p = next;
                    break;
                default:
                    throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a URI as the Predicate of a Triple", next);
            }

            //Parse Object
            next = this._tokens.Dequeue();
            switch (next.TokenType)
            {
                case Token.BLANKNODEWITHID:
                case Token.LITERALWITHDT:
                case Token.LITERALWITHLANG:
                case Token.URI:
                    //OK
                    o = next;
                    break;
                case Token.LITERAL:
                    //Check for Datatype/Language
                    IToken temp = this._tokens.Peek();
                    if (temp.TokenType == Token.DATATYPE)
                    {
                        this._tokens.Dequeue();
                        o = new LiteralWithDataTypeToken(next, (DataTypeToken)temp);
                    }
                    else if (temp.TokenType == Token.LANGSPEC)
                    {
                        this._tokens.Dequeue();
                        o = new LiteralWithLanguageSpecifierToken(next, (LanguageSpecifierToken)temp);
                    }
                    else
                    {
                        o = next;
                    }
                    break;
                default:
                    throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Blank Node/Literal/URI as the Object of a Triple", next);
            }

            next = this._tokens.Dequeue();
            if (next.TokenType == Token.DOT)
            {
                //Terminates a Triple and there is no Context given for the Triple so this
                //Triple is in the Default Graph
                IGraph def = this._factory[null];
                t = this.CreateTriple(s, p, o, def);
            }
            else
            {
                //Parse Context
                INode context;
                switch (next.TokenType)
                {
                    case Token.BLANKNODEWITHID:
                        context = this._g.CreateBlankNode(next.Value.Substring(2));
                        break;
                    case Token.URI:
                        context = this._g.CreateUriNode(new Uri(next.Value));
                        break;
                    case Token.LITERAL:
                        //Check for Datatype/Language
                        IToken temp = this._tokens.Peek();
                        if (temp.TokenType == Token.LANGSPEC)
                        {
                            this._tokens.Dequeue();
                            context = this._g.CreateLiteralNode(next.Value, temp.Value);
                        }
                        else if (temp.TokenType == Token.DATATYPE)
                        {
                            this._tokens.Dequeue();
                            context = this._g.CreateLiteralNode(next.Value, new Uri(temp.Value.Substring(1, temp.Value.Length - 2)));
                        }
                        else
                        {
                            context = this._g.CreateLiteralNode(next.Value);
                        }
                        break;
                    default:
                        throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Blank Node/Literal/URI as the Context of the Triple", next);
                }

                Uri contextUri;
                if (context.NodeType == NodeType.Uri)
                {
                    contextUri = ((IUriNode)context).Uri;
                }
                else if (context.NodeType == NodeType.Blank)
                {
                    contextUri = new Uri("nquads:bnode:" + context.GetHashCode());
                }
                else if (context.NodeType == NodeType.Literal)
                {
                    contextUri = new Uri("nquads:literal:" + context.GetHashCode());
                }
                else
                {
                    throw Error("Cannot turn a Node of type '" + context.GetType().ToString() + "' into a Context URI for a Triple", next);
                }

                IGraph g = this._factory[contextUri];
                t = this.CreateTriple(s, p, o, g);

                next = this._tokens.Dequeue();
                while (next.TokenType == Token.COMMENT)
                {
                    next = this._tokens.Dequeue();
                }
                if (next.TokenType != Token.DOT)
                {
                    throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Dot Token (Line Terminator) to terminate a Triple", next);
                }
            }

            //Check for EOF
            next = this._tokens.Peek();
            if (next.TokenType == Token.EOF)
            {
                this._tokens.Dequeue();
                this._eof = true;
            }

            return t;
        }

        public bool EOF
        {
            get
            {
                return this._eof;
            }
        }

        private Triple CreateTriple(IToken s, IToken p, IToken o, IGraph g)
        {
            INode subj, pred, obj;

            switch (s.TokenType)
            {
                case Token.BLANKNODEWITHID:
                    subj = g.CreateBlankNode(s.Value.Substring(2));
                    break;
                case Token.URI:
                    subj = g.CreateUriNode(new Uri(s.Value));
                    break;
                default:
                    throw Error("Unexpected Token '" + s.GetType().ToString() + "' encountered, expected a Blank Node/URI as the Subject of a Triple", s);
            }

            switch (p.TokenType)
            {
                case Token.URI:
                    pred = g.CreateUriNode(new Uri(p.Value));
                    break;
                default:
                    throw Error("Unexpected Token '" + p.GetType().ToString() + "' encountered, expected a URI as the Predicate of a Triple", p);
            }

            switch (o.TokenType)
            {
                case Token.BLANKNODEWITHID:
                    obj = g.CreateBlankNode(o.Value.Substring(2));
                    break;
                case Token.LITERAL:
                    obj = g.CreateLiteralNode(o.Value);
                    break;
                case Token.LITERALWITHDT:
                    String dtUri = ((LiteralWithDataTypeToken)o).DataType;
                    obj = g.CreateLiteralNode(o.Value, new Uri(dtUri.Substring(1, dtUri.Length - 2)));
                    break;
                case Token.LITERALWITHLANG:
                    obj = g.CreateLiteralNode(o.Value, ((LiteralWithLanguageSpecifierToken)o).Language);
                    break;
                case Token.URI:
                    obj = g.CreateUriNode(new Uri(o.Value));
                    break;
                default:
                        throw Error("Unexpected Token '" + o.GetType().ToString() + "' encountered, expected a Blank Node/Literal/URI as the Object of a Triple", o);
            }

            return new Triple(subj, pred, obj, g);
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
    }
}
