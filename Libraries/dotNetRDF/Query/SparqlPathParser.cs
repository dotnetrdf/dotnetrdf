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
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Paths;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Internal class which parses SPARQL Paths into path expressions
    /// </summary>
    class SparqlPathParser
    {
        public ISparqlPath Parse(SparqlQueryParserContext context, IToken first)
        {
            // Need to gather up all the Tokens which make up the path
            int openBrackets = 0;
            Queue<IToken> tokens = new Queue<IToken>();
            IToken next;
            LastPathItemType lastItem = LastPathItemType.None;
            int lastSequencer = -1;

            // Add the first token and set the initial last item type
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
                case Token.NEGATION:
                    lastItem = LastPathItemType.Negation;
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
                        // Groups are considered predicates for purposes of last item
                        lastItem = LastPathItemType.Predicate;
                    }
                    else if (next.TokenType == Token.LEFTBRACKET)
                    {
                        openBrackets++;
                    }
                }
                else
                {
                    switch (next.TokenType)
                    {
                        case Token.LEFTBRACKET:
                            // Path Group

                            if (lastItem == LastPathItemType.Predicate || lastItem == LastPathItemType.Modifier)
                            {
                                // If it follows a Predicate/Modifier then this is likely a collection as the object of a 
                                // Triple pattern so we stop
                                lastItem = LastPathItemType.End;
                            }
                            else if (lastItem == LastPathItemType.Sequencer || lastItem == LastPathItemType.Negation)
                            {
                                // This is a new Path Group if it follows a sequencer/negation
                                openBrackets++;
                                context.Tokens.Dequeue();
                                tokens.Enqueue(next);
                                lastItem = LastPathItemType.Predicate;
                            } 
                            else 
                            {
                                throw new RdfParseException("Path Groups can only follow path sequencing tokens", next);
                            }
                            break;

                        case Token.LEFTCURLYBRACKET:
                            // Explicit cardinality modifiers

                            if (context.SyntaxMode == SparqlQuerySyntax.Sparql_1_1) throw new RdfParseException("The {} forms for property paths are not supported in SPARQL 1.1", next);
                            if (lastItem != LastPathItemType.Predicate)
                            {
                                throw new RdfParseException("Cardinality Modifiers can only follow Predicates/Path Groups", next);
                            }

                            // Add the opening { to the tokens
                            context.Tokens.Dequeue();
                            tokens.Enqueue(next);
                            next = context.Tokens.Peek();

                            // Grab everything up to the next }
                            while (next.TokenType != Token.RIGHTCURLYBRACKET)
                            {
                                // If we see another { this is an error
                                if (next.TokenType == Token.LEFTCURLYBRACKET)
                                {
                                    throw new RdfParseException("Nested Cardinality Modifiers for Paths are not permitted", next);
                                }

                                context.Tokens.Dequeue();
                                tokens.Enqueue(next);
                                next = context.Tokens.Peek();
                            }
                            // Add the trailing } to the tokens
                            context.Tokens.Dequeue();
                            tokens.Enqueue(next);
                            lastItem = LastPathItemType.Modifier;
                            break;

                        case Token.PLUS:
                        case Token.QUESTION:
                        case Token.MULTIPLY:
                            // Other Cardinality modifiers are permitted

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
                            // Path sequencing

                            if (lastItem != LastPathItemType.Predicate && lastItem != LastPathItemType.Modifier)
                            {
                                if (lastItem == LastPathItemType.Sequencer && next.TokenType == Token.HAT && (lastSequencer == Token.DIVIDE || lastSequencer == Token.BITWISEOR))
                                {
                                    // / ^ or | ^ is a valid sequencing
                                }
                                else
                                {
                                    throw new RdfParseException("Path sequencing tokens can only follow Predicates/Path Groups", next);
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
                            // Predicates

                            if (lastItem != LastPathItemType.None && lastItem != LastPathItemType.Sequencer && lastItem != LastPathItemType.Negation)
                            {
                                // This appears to be the end of the path since we've encountered something that could be 
                                // an Object
                                lastItem = LastPathItemType.End;
                            }
                            else
                            {
                                context.Tokens.Dequeue();
                                tokens.Enqueue(next);
                                lastItem = LastPathItemType.Predicate;
                            }
                            break;

                        case Token.NEGATION:
                            if (lastItem == LastPathItemType.Sequencer)
                            {
                                context.Tokens.Dequeue();
                                tokens.Enqueue(next);
                                lastItem = LastPathItemType.Negation;
                            }
                            else
                            {
                                throw new RdfParseException("Negated Property Sets can only follow path sequencing tokens", next);
                            }
                            break;

                        default:
                            if (lastItem != LastPathItemType.None && lastItem != LastPathItemType.Sequencer)
                            {
                                // Appears to be the end of the path since we've encountered an unexpected token after seeing
                                // a Predicate
                                lastItem = LastPathItemType.End;
                            }
                            else
                            {
                                throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encounted, this is not a valid token for a Path", next);
                            }
                            break;
                    }

                    if (lastItem == LastPathItemType.End) break;
                }
            }

            ISparqlPath path = TryParsePath(context, tokens);
            return path;
        }

        private ISparqlPath TryParsePath(SparqlQueryParserContext context, Queue<IToken> tokens)
        {
            return TryParsePathAlternative(context, tokens);
        }

        private ISparqlPath TryParsePathAlternative(SparqlQueryParserContext context, Queue<IToken> tokens)
        {
            ISparqlPath path = TryParsePathSequence(context, tokens);
            IToken next;
            while (tokens.Count > 0)
            {
                next = tokens.Dequeue();
                if (next.TokenType == Token.BITWISEOR)
                {
                    path = new AlternativePath(path, TryParsePathSequence(context, tokens));
                }
                else
                {
                    throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a valid path sequence/alternative token", next);
                }
            }

            return path;
        }

        private ISparqlPath TryParsePathSequence(SparqlQueryParserContext context, Queue<IToken> tokens)
        {
            ISparqlPath path = TryParsePathEltOrInverse(context, tokens);
            IToken next;
            while (tokens.Count > 0)
            {
                next = tokens.Peek();
                switch (next.TokenType)
                {
                    case Token.DIVIDE:
                        tokens.Dequeue();
                        path = new SequencePath(path, TryParsePathEltOrInverse(context, tokens));
                        break;
                    case Token.HAT:
                        tokens.Dequeue();
                        path = new SequencePath(path, new InversePath(TryParsePathElt(context, tokens)));
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
                return new InversePath(TryParsePathElt(context, tokens));
            }
            else
            {
                return TryParsePathElt(context, tokens);
            }
        }

        private ISparqlPath TryParsePathElt(SparqlQueryParserContext context, Queue<IToken> tokens)
        {
            return TryParsePathPrimary(context, tokens);
        }

        private ISparqlPath TryParsePathPrimary(SparqlQueryParserContext context, Queue<IToken> tokens)
        {
            IToken next = tokens.Dequeue();
            ISparqlPath path;
            switch (next.TokenType)
            {
                case Token.URI:
                case Token.QNAME:
                    path =  new Property(new UriNode(null, UriFactory.Create(Tools.ResolveUriOrQName(next, context.Query.NamespaceMap, context.Query.BaseUri))));
                    break;

                case Token.KEYWORDA:
                    path =  new Property(new UriNode(null, UriFactory.Create(RdfSpecsHelper.RdfType)));
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

                    path = TryParsePath(context, subtokens);
                    break;

                case Token.NEGATION:
                    path = TryParseNegatedPropertySet(context, tokens);
                    break;

                default:
                    throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a URI/QName, the 'a' keyword or the start of a group path expression", next);
            }

            // See if there's a Path Modifier
            if (tokens.Count > 0)
            {
                next = tokens.Peek();
                switch (next.TokenType)
                {
                    case Token.MULTIPLY:
                    case Token.PLUS:
                    case Token.QUESTION:
                    case Token.LEFTCURLYBRACKET:
                        path = TryParsePathMod(context, tokens, path);
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
                            if (min < 0) throw new RdfParseException("Cannot specify the minimum cardinality of a path as less than zero", next);
                            next = tokens.Dequeue();
                            if (next.TokenType == Token.COMMA)
                            {
                                next = tokens.Dequeue();
                                if (next.TokenType == Token.PLAINLITERAL)
                                {
                                    if (Int32.TryParse(next.Value, out max))
                                    {
                                        if (max < min) throw new RdfParseException("Cannot specify the maximum cardinality of a path as less than the minimum", next);
                                        next = tokens.Dequeue();
                                        if (next.TokenType != Token.RIGHTCURLYBRACKET) throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a } to terminate a Path Cardinality modifier", next);
                                        if (min == max)
                                        {
                                            return new FixedCardinality(path, min);
                                        }
                                        else
                                        {
                                            return new NToM(path, min, max);
                                        }
                                    }
                                    else
                                    {
                                        throw new RdfParseException("The value '" + next.Value + "' is not valid for use as a Path Cardinality modifier", next);
                                    }
                                }
                                else if (next.TokenType == Token.RIGHTCURLYBRACKET)
                                {
                                    if (min == 0)
                                    {
                                        return new ZeroOrMore(path);
                                    }
                                    else if (min == 1)
                                    {
                                        return new OneOrMore(path);
                                    }
                                    else
                                    {
                                        return new NOrMore(path, min);
                                    }
                                }
                                else
                                {
                                    throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected an Integer Plain Literal as part of a Path Cardinality modifier", next);
                                }
                            }
                            else if (next.TokenType == Token.RIGHTCURLYBRACKET)
                            {
                                return new FixedCardinality(path, min);
                            }
                            else
                            {
                                throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a valid token to continue the Path Cardinality modifier", next);
                            }
                        }
                        else
                        {
                            throw new RdfParseException("The value '" + next.Value + "' is not valid for use as a Path Cardinality modifier", next);
                        }
                    }
                    else if (next.TokenType == Token.COMMA)
                    {
                        next = tokens.Dequeue();
                        if (next.TokenType == Token.PLAINLITERAL)
                        {
                            if (Int32.TryParse(next.Value, out max))
                            {
                                if (max <= 0) throw new RdfParseException("Cannot specify the maximum cardinality for a path as being less than the minimum", next);
                                next = tokens.Dequeue();
                                if (next.TokenType != Token.RIGHTCURLYBRACKET) throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a } to terminate a Path Cardinality modifier", next);
                                if (max == 1)
                                {
                                    return new ZeroOrOne(path);
                                }
                                else
                                {
                                    return new ZeroToN(path, max);
                                }
                            } 
                            else 
                            {
                                throw new RdfParseException("The value '" + next.Value + "' is not valid for use as a Path Cardinality modifier", next);
                            }
                        }
                        else
                        {
                            throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected an Integer Plain Literal as part of a Path Cardinality modifier", next);
                        }
                    }
                    else
                    {
                        throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected an Integer Plain Literal/Comma as part of a Path Cardinality modifier", next);
                    }
                default:
                    throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a token which is valid as a Path Cardinality modifier", next);
            }
        }

        private ISparqlPath TryParseNegatedPropertySet(SparqlQueryParserContext context, Queue<IToken> tokens)
        {
            IToken next = tokens.Peek();
            bool inverse;
            Property p;

            switch (next.TokenType)
            {
                case Token.QNAME:
                case Token.URI:
                case Token.KEYWORDA:
                case Token.HAT:
                    p = TryParsePathOneInPropertySet(context, tokens, out inverse);
                    if (inverse)
                    {
                        return new NegatedSet(Enumerable.Empty<Property>(), p.AsEnumerable());
                    }
                    else
                    {
                        return new NegatedSet(p.AsEnumerable(), Enumerable.Empty<Property>());
                    }

                case Token.LEFTBRACKET:
                    List<Property> ps = new List<Property>();
                    List<Property> inverses = new List<Property>();

                    tokens.Dequeue();
                    next = tokens.Peek();
                    while (next.TokenType != Token.RIGHTBRACKET)
                    {
                        // Parse the next item in the set
                        p = TryParsePathOneInPropertySet(context, tokens, out inverse);
                        if (inverse)
                        {
                            inverses.Add(p);
                        }
                        else
                        {
                            ps.Add(p);
                        }

                        // Then there may be an optional | which we should discard
                        next = tokens.Peek();
                        if (next.TokenType == Token.BITWISEOR)
                        {
                            tokens.Dequeue();
                            next = tokens.Peek();
                            // If we see this we can't then see a ) immediately
                            if (next.TokenType == Token.RIGHTBRACKET) throw new RdfParseException("Unexpected ) to end a negated property set encountered immediately after a | which should indicate that further items are in the set", next);
                        }
                    }
                    tokens.Dequeue();

                    if (ps.Count == 0 && inverses.Count == 0)
                    {
                        throw new RdfParseException("Unexpected empty negated property set encountered", next);
                    }
                    else
                    {
                        return new NegatedSet(ps, inverses);
                    }

                default:
                    throw new RdfParseException("Unexpected Token Type '" + next.GetType().Name + "' encountered, expected the first path in a negated property set", next);

            }
        }

        private Property TryParsePathOneInPropertySet(SparqlQueryParserContext context, Queue<IToken> tokens, out bool inverse)
        {
            IToken next = tokens.Dequeue();
            inverse = false;
            switch (next.TokenType)
            {
                case Token.URI:
                case Token.QNAME:
                    return new Property(new UriNode(null, UriFactory.Create(Tools.ResolveUriOrQName(next, context.Query.NamespaceMap, context.Query.BaseUri))));

                case Token.KEYWORDA:
                    return new Property(new UriNode(null, UriFactory.Create(RdfSpecsHelper.RdfType)));

                case Token.HAT:
                    next = tokens.Dequeue();
                    inverse = true;
                    switch (next.TokenType)
                    {
                        case Token.URI:
                        case Token.QNAME:
                            return new Property(new UriNode(null, UriFactory.Create(Tools.ResolveUriOrQName(next, context.Query.NamespaceMap, context.Query.BaseUri))));

                        case Token.KEYWORDA:
                            return new Property(new UriNode(null, UriFactory.Create(RdfSpecsHelper.RdfType)));

                        default:
                            throw new RdfParseException("Unexpected Token Type '" + next.GetType().Name + "' encountered, expected a QName/URI or the 'a' Keyword after an inverse operator in a negated property set", next);
                    }

                default:
                    throw new RdfParseException("Unexpected Token Type '" + next.GetType().Name + "' encountered, expected a QName/URI or the 'a' Keyword or the inverse operator '^' to define a path in a negated property set", next);
            }
        }
    }

    enum LastPathItemType
    {
        None,
        Predicate,
        Modifier,
        Sequencer,
        Negation,
        End
    }
}
