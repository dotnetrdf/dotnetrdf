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
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query;
using VDS.RDF.Query.Paths;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Internal class which parses SPARQL Paths into path expressions
    /// </summary>
    class SparqlPathParser
    {
        public ISparqlPath Parse(SparqlQueryParserContext context, IToken first, out String lengthVar)
        {
            lengthVar = String.Empty;

            //Need to gather up all the Tokens which make up the path
            int openBrackets = 0;
            Queue<IToken> tokens = new Queue<IToken>();
            IToken next;
            LastPathItemType lastItem = LastPathItemType.None;
            int lastSequencer = -1;

            //Add the first token and set the initial last item type
            tokens.Enqueue(first);
            switch (first.TokenType)
            {
                case Token.LEFTBRACKET:
                    openBrackets = 1;
                    lastItem = LastPathItemType.Predicate;
                    break;
                case Token.QNAME:
                case Token.URI:
                case Token.KEYWORDA:
                    lastItem = LastPathItemType.Predicate;
                    break;
                case Token.HAT:
                    lastItem = LastPathItemType.Sequencer;
                    break;
                default:
                    throw new RdfParseException("Unexpected Token '" + first.GetType().ToString() + "' encountered, this is not valid as the start of a property path");
            } 


            while (true)
            {
                next = context.Tokens.Peek();
                if (openBrackets > 0)
                {
                    context.Tokens.Dequeue();
                    tokens.Enqueue(next);

                    if (next.TokenType == Token.RIGHTBRACKET)
                    {
                        openBrackets--;
                        //Groups are considered predicates for purposes of last item
                        lastItem = LastPathItemType.Predicate;
                    }
                }
                else
                {
                    switch (next.TokenType)
                    {
                        case Token.LEFTBRACKET:
                            //Path Group

                            if (lastItem == LastPathItemType.Predicate || lastItem == LastPathItemType.Modifier)
                            {
                                //If it follows a Predicate/Modifier then this is likely a collection as the object of a 
                                //Triple pattern so we stop
                                lastItem = LastPathItemType.End;
                            }
                            else if (lastItem == LastPathItemType.Sequencer)
                            {
                                //This is a new Path Group if it follows a sequencer
                                openBrackets++;
                                context.Tokens.Dequeue();
                                tokens.Enqueue(next);
                                lastItem = LastPathItemType.Predicate;
                            } 
                            else 
                            {
                                throw new RdfParseException("Path Groups can only follow path sequencing tokens");
                            }
                                break;

                        case Token.LEFTCURLYBRACKET:
                            //Explicit cardinality modifiers

                            if (lastItem != LastPathItemType.Predicate)
                            {
                                throw new RdfParseException("Cardinality Modifiers can only follow Predicates/Path Groups");
                            }

                            //Add the opening { to the tokens
                            context.Tokens.Dequeue();
                            tokens.Enqueue(next);
                            next = context.Tokens.Peek();

                            //Grab everything up to the next }
                            while (next.TokenType != Token.RIGHTCURLYBRACKET)
                            {
                                //If we see another { this is an error
                                if (next.TokenType == Token.LEFTCURLYBRACKET)
                                {
                                    throw new RdfParseException("Nested Cardinality Modifiers for Paths are not permitted");
                                }

                                context.Tokens.Dequeue();
                                tokens.Enqueue(next);
                                next = context.Tokens.Peek();
                            }
                            //Add the trailing } to the tokens
                            context.Tokens.Dequeue();
                            tokens.Enqueue(next);
                            lastItem = LastPathItemType.Modifier;
                            break;

                        case Token.PLUS:
                        case Token.QUESTION:
                        case Token.MULTIPLY:
                            //Other Cardinality modifiers are permitted

                            if (lastItem != LastPathItemType.Predicate)
                            {
                                throw new RdfParseException("Cardinality Modifiers can only follow Predicates/Path Groups");
                            }

                            context.Tokens.Dequeue();
                            tokens.Enqueue(next);
                            lastItem = LastPathItemType.Modifier;
                            break;

                        case Token.BITWISEOR:
                        case Token.DIVIDE:
                        case Token.HAT:
                            //Path sequencing

                            if (lastItem != LastPathItemType.Predicate && lastItem != LastPathItemType.Modifier)
                            {
                                if (lastItem == LastPathItemType.Sequencer && next.TokenType == Token.HAT && lastSequencer == Token.DIVIDE)
                                {
                                    // / ^ is a valid sequencing
                                }
                                else
                                {
                                    throw new RdfParseException("Path sequencing tokens can only follow Predicates/Path Groups");
                                }
                            }

                            context.Tokens.Dequeue();
                            tokens.Enqueue(next);
                            lastItem = LastPathItemType.Sequencer;
                            lastSequencer = next.TokenType;
                            break;

                        case Token.QNAME:
                        case Token.URI:
                        case Token.KEYWORDA:
                            //Predicates

                            if (lastItem != LastPathItemType.None && lastItem != LastPathItemType.Sequencer)
                            {
                                //This appears to be the end of the path since we've encountered 
                                lastItem = LastPathItemType.End;
                            }
                            else
                            {
                                context.Tokens.Dequeue();
                                tokens.Enqueue(next);
                                lastItem = LastPathItemType.Predicate;
                            }
                            break;

                        default:
                            if (lastItem != LastPathItemType.None && lastItem != LastPathItemType.Sequencer)
                            {
                                //Appears to be the end of the path since we've encountered an unexpected token after seeing
                                //a Predicate
                                lastItem = LastPathItemType.End;
                            }
                            else
                            {
                                throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encounted, this is not a valid token for a Path");
                            }
                            break;
                    }

                    if (lastItem == LastPathItemType.End) break;
                }
            }

            ISparqlPath path = this.TryParsePath(context, tokens);

            //Has the user asked for the Length of the Path to be bound to a variable?
            next = context.Tokens.Peek();
            if (next.TokenType == Token.LENGTH)
            {
                context.Tokens.Dequeue();
                next = context.Tokens.Peek();
                if (next.TokenType == Token.VARIABLE)
                {
                    context.Tokens.Dequeue();
                    lengthVar = next.Value;
                }
                else
                {
                    throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Variable after a LENGTH keyword to bind the length of the Path to");
                }
            }

            return path;
        }

        private ISparqlPath TryParsePath(SparqlQueryParserContext context, Queue<IToken> tokens)
        {
            return this.TryParsePathAlternative(context, tokens);
        }

        private ISparqlPath TryParsePathAlternative(SparqlQueryParserContext context, Queue<IToken> tokens)
        {
            ISparqlPath path = this.TryParsePathSequence(context, tokens);
            IToken next;
            while (tokens.Count > 0)
            {
                next = tokens.Dequeue();
                if (next.TokenType == Token.BITWISEOR)
                {
                    path = new AlternativePath(path, this.TryParsePathSequence(context, tokens));
                }
                else
                {
                    throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a valid path sequence/alternative token");
                }
            }

            return path;
        }

        private ISparqlPath TryParsePathSequence(SparqlQueryParserContext context, Queue<IToken> tokens)
        {
            ISparqlPath path = this.TryParsePathEltOrInverse(context, tokens);
            IToken next;
            while (tokens.Count > 0)
            {
                next = tokens.Peek();
                switch (next.TokenType)
                {
                    case Token.DIVIDE:
                        tokens.Dequeue();
                        path = new SequencePath(path, this.TryParsePathEltOrInverse(context, tokens));
                        break;
                    case Token.HAT:
                        tokens.Dequeue();
                        path = new SequencePath(path, new InversePath(this.TryParsePathElt(context, tokens)));
                        break;
                    default:
                        return path;
                }
            }

            return path;
        }

        private ISparqlPath TryParsePathEltOrInverse(SparqlQueryParserContext context, Queue<IToken> tokens)
        {
            IToken next = tokens.Peek();
            if (next.TokenType == Token.HAT)
            {
                tokens.Dequeue();
                return new InversePath(this.TryParsePathElt(context, tokens));
            }
            else
            {
                return this.TryParsePathElt(context, tokens);
            }
        }

        private ISparqlPath TryParsePathElt(SparqlQueryParserContext context, Queue<IToken> tokens)
        {
            return this.TryParsePathPrimary(context, tokens);
        }

        private ISparqlPath TryParsePathPrimary(SparqlQueryParserContext context, Queue<IToken> tokens)
        {
            IToken next = tokens.Dequeue();
            ISparqlPath path;
            switch (next.TokenType)
            {
                case Token.URI:
                case Token.QNAME:
                    path =  new Property(new UriNode(null, new Uri(Tools.ResolveUriOrQName(next, context.Query.NamespaceMap, context.Query.BaseUri))));
                    break;

                case Token.KEYWORDA:
                    path =  new Property(new UriNode(null, new Uri(RdfSpecsHelper.RdfType)));
                    break;

                case Token.LEFTBRACKET:
                    Queue<IToken> subtokens = new Queue<IToken>();
                    int openBrackets = 1;
                    do {
                        next = tokens.Dequeue();
                        if (next.TokenType == Token.LEFTBRACKET) 
                        {
                            openBrackets++;
                        } 
                        else if (next.TokenType == Token.RIGHTBRACKET) 
                        {
                            openBrackets--;
                        }

                        if (openBrackets > 0) subtokens.Enqueue(next);
                    } while (openBrackets > 0);

                    path = this.TryParsePath(context, subtokens);
                    break;

                default:
                    throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a URI/QName, the 'a' keyword or the start of a group path expression");
            }

            //See if there's a Path Modifier
            if (tokens.Count > 0)
            {
                next = tokens.Peek();
                switch (next.TokenType)
                {
                    case Token.MULTIPLY:
                    case Token.PLUS:
                    case Token.QUESTION:
                    case Token.LEFTCURLYBRACKET:
                        path = this.TryParsePathMod(context, tokens, path);
                        break;
                }
            }

            return path;
        }

        private ISparqlPath TryParsePathMod(SparqlQueryParserContext context, Queue<IToken> tokens, ISparqlPath path)
        {
            IToken next = tokens.Dequeue();
            switch (next.TokenType)
            {
                case Token.MULTIPLY:
                    return new ZeroOrMore(path);
                case Token.QUESTION:
                    return new ZeroOrOne(path);
                case Token.PLUS:
                    return new OneOrMore(path);
                case Token.LEFTCURLYBRACKET:
                    next = tokens.Dequeue();
                    int min, max;
                    if (next.TokenType == Token.PLAINLITERAL)
                    {
                        if (Int32.TryParse(next.Value, out min))
                        {
                            next = tokens.Dequeue();
                            if (next.TokenType == Token.COMMA)
                            {
                                next = tokens.Dequeue();
                                if (next.TokenType == Token.PLAINLITERAL)
                                {
                                    if (Int32.TryParse(next.Value, out max))
                                    {
                                        next = tokens.Dequeue();
                                        if (next.TokenType != Token.RIGHTCURLYBRACKET) throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a } to terminate a Path Cardinality modifier");
                                        return new NToM(path, min, max);
                                    }
                                    else
                                    {
                                        throw new RdfParseException("The value '" + next.Value + "' is not valid for use as a Path Cardinality modifier");
                                    }
                                }
                                else if (next.TokenType == Token.RIGHTCURLYBRACKET)
                                {
                                    return new NOrMore(path, min);
                                }
                                else
                                {
                                    throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected an Integer Plain Literal as part of a Path Cardinality modifier");
                                }
                            }
                            else if (next.TokenType == Token.RIGHTCURLYBRACKET)
                            {
                                return new FixedCardinality(path, min);
                            }
                            else
                            {
                                throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a valid token to continue the Path Cardinality modifier");
                            }
                        }
                        else
                        {
                            throw new RdfParseException("The value '" + next.Value + "' is not valid for use as a Path Cardinality modifier");
                        }
                    }
                    else if (next.TokenType == Token.COMMA)
                    {
                        next = tokens.Dequeue();
                        if (next.TokenType == Token.PLAINLITERAL)
                        {
                            if (Int32.TryParse(next.Value, out max))
                            {
                                next = tokens.Dequeue();
                                if (next.TokenType != Token.RIGHTCURLYBRACKET) throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a } to terminate a Path Cardinality modifier");
                                return new ZeroToN(path, max);
                            } 
                            else 
                            {
                                throw new RdfParseException("The value '" + next.Value + "' is not valid for use as a Path Cardinality modifier");
                            }
                        }
                        else
                        {
                            throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected an Integer Plain Literal as part of a Path Cardinality modifier");
                        }
                    }
                    else
                    {
                        throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected an Integer Plain Literal/Comma as part of a Path Cardinality modifier");
                    }
                default:
                    throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a token which is valid as a Path Cardinality modifier");
            }
        }
    }

    enum LastPathItemType
    {
        None,
        Predicate,
        Modifier,
        Sequencer,
        End
    }
}
