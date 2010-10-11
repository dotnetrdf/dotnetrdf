using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;

namespace VDS.Alexandria.Utilities
{
    class StreamingNTriplesParser
    {
        private ITokenQueue _tokens;
        private bool _eof = false;
        private IGraph _g;

        public StreamingNTriplesParser(IGraph g, StreamReader reader)
        {
            this._g = g;
            NTriplesTokeniser tokeniser = new NTriplesTokeniser(reader);
            this._tokens = new BufferedTokenQueue(tokeniser);
            this._tokens.InitialiseBuffer();

            //Expect a BOF
            IToken start = this._tokens.Dequeue();
            if (start.TokenType != Token.BOF)
            {
                throw ParserHelper.Error("Unexpected Token '" + start.GetType().ToString() + "' encountered, expected a Beginning of File Token", start);
            }
            IToken next = this._tokens.Peek();
            if (next.TokenType == Token.EOF) this._eof = true;
        }

        public bool EOF
        {
            get
            {
                return this._eof;
            }
        }

        public Triple GetNextTriple()
        {
            //Expect Triples
            Triple temp = this.TryParseTriple();

            IToken next = this._tokens.Peek();
            if (next.TokenType == Token.EOF) this._eof = true;

            return temp;
        }

        private Triple TryParseTriple()
        {
            //Get the Subject, Predicate and Object
            INode subj = this.TryParseSubject();
            INode pred = this.TryParsePredicate();
            INode obj = this.TryParseObject();

            //Ensure we're terminated by a DOT
            this.TryParseLineTerminator();

            //Return the Triple
            return new Triple(subj, pred, obj);
        }

        private INode TryParseSubject()
        {
            IToken subjToken = this._tokens.Dequeue();

            //Discard Comments
            while (subjToken.TokenType == Token.COMMENT)
            {
                subjToken = this._tokens.Dequeue();
            }

            switch (subjToken.TokenType)
            {
                case Token.BLANKNODE:
                    return this._g.CreateBlankNode();
                case Token.BLANKNODEWITHID:
                    return this._g.CreateBlankNode(subjToken.Value.Substring(2));
                case Token.URI:
                    return this._g.CreateUriNode(new Uri(subjToken.Value));
                case Token.LITERAL:
                case Token.LITERALWITHDT:
                case Token.LITERALWITHLANG:
                    throw ParserHelper.Error("Subject cannot be a Literal in NTriples", subjToken);
                default:
                    throw ParserHelper.Error("Unexpected Token '" + subjToken.GetType().ToString() + "' encountered, expected a Blank Node or URI for the Subject of a Triple", subjToken);
            }
        }

        private INode TryParsePredicate()
        {
            IToken predToken = this._tokens.Dequeue();

            //Discard Comments
            while (predToken.TokenType == Token.COMMENT)
            {
                predToken = this._tokens.Dequeue();
            }

            switch (predToken.TokenType)
            {
                case Token.BLANKNODE:
                case Token.BLANKNODEWITHID:
                    throw ParserHelper.Error("Predicate cannot be a Blank Node in NTriples", predToken);
                case Token.URI:
                    return this._g.CreateUriNode(new Uri(predToken.Value));
                //return this.ConvertToNode(g, predToken);
                case Token.LITERAL:
                case Token.LITERALWITHDT:
                case Token.LITERALWITHLANG:
                    throw ParserHelper.Error("Predicate cannot be a Literal in NTriples", predToken);
                default:
                    throw ParserHelper.Error("Unexpected Token '" + predToken.GetType().ToString() + "' encountered, expected a URI for the Predicate of a Triple", predToken);
            }
        }

        private INode TryParseObject()
        {
            IToken objToken = this._tokens.Dequeue();
            String dt;

            //Discard Comments
            while (objToken.TokenType == Token.COMMENT)
            {
                objToken = this._tokens.Dequeue();
            }

            switch (objToken.TokenType)
            {
                case Token.BLANKNODE:
                    return this._g.CreateBlankNode();
                case Token.BLANKNODEWITHID:
                    return this._g.CreateBlankNode(objToken.Value.Substring(2));
                case Token.URI:
                    return this._g.CreateUriNode(new Uri(objToken.Value));
                case Token.LITERALWITHDT:
                    dt = ((LiteralWithDataTypeToken)objToken).DataType;
                    dt = dt.Substring(1, dt.Length - 2);
                    return this._g.CreateLiteralNode(objToken.Value, new Uri(dt));
                case Token.LITERALWITHLANG:
                    return this._g.CreateLiteralNode(objToken.Value, ((LiteralWithLanguageSpecifierToken)objToken).Language);
                case Token.LITERAL:
                    IToken next = this._tokens.Peek();
                    //Is there a Language Specifier or Data Type?
                    if (next.TokenType == Token.LANGSPEC)
                    {
                        this._tokens.Dequeue();
                        return this._g.CreateLiteralNode(objToken.Value, next.Value);
                    }
                    else if (next.TokenType == Token.URI)
                    {
                        this._tokens.Dequeue();
                        return this._g.CreateLiteralNode(objToken.Value, new Uri(Tools.ResolveUriOrQName(next, this._g.NamespaceMap, this._g.BaseUri)));
                    }
                    else
                    {
                        return this._g.CreateLiteralNode(objToken.Value);
                    }

                default:
                    throw ParserHelper.Error("Unexpected Token '" + objToken.GetType().ToString() + "' encountered, expected a Blank Node, Literal or URI for the Object of a Triple", objToken);
            }
        }

        private void TryParseLineTerminator()
        {
            IToken next = this._tokens.Dequeue();

            //Discard Comments
            while (next.TokenType == Token.COMMENT)
            {
                next = this._tokens.Dequeue();
            }

            //Ensure we finish with a Dot terminator
            if (next.TokenType != Token.DOT)
            {
                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Dot Line Terminator to terminate a Triple", next);
            }
        }
    }
}
