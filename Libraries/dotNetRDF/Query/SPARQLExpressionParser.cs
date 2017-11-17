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
using System.Globalization;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Aggregates.Leviathan;
using VDS.RDF.Query.Aggregates.Sparql;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Arithmetic;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions.Sparql;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;
using VDS.RDF.Query.Expressions.Functions.Sparql.DateTime;
using VDS.RDF.Query.Expressions.Functions.Sparql.Hash;
using VDS.RDF.Query.Expressions.Functions.Sparql.Numeric;
using VDS.RDF.Query.Expressions.Functions.Sparql.Set;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Internal Class which parses SPARQL Expressions into Expression Trees
    /// </summary>
    class SparqlExpressionParser
    {
        private NamespaceMapper _nsmapper;
        private Uri _baseUri;
        private bool _allowAggregates = false;
        private SparqlQuerySyntax _syntax = Options.QueryDefaultSyntax;
        private SparqlQueryParser _parser;
        private IEnumerable<ISparqlCustomExpressionFactory> _factories = Enumerable.Empty<ISparqlCustomExpressionFactory>();

        /// <summary>
        /// Creates a new SPARQL Expression Parser
        /// </summary>
        public SparqlExpressionParser() { }

        /// <summary>
        /// Creates a new SPARQL Expression Parser which has a reference back to a Query Parser
        /// </summary>
        /// <param name="parser">Query Parser</param>
        public SparqlExpressionParser(SparqlQueryParser parser)
            : this(parser, false) { }

        /// <summary>
        /// Creates a new SPARQL Expression Parser
        /// </summary>
        /// <param name="allowAggregates">Whether Aggregates are allowed in Expressions</param>
        public SparqlExpressionParser(bool allowAggregates)
            : this(null, allowAggregates) { }

        /// <summary>
        /// Creates a new SPARQL Expression Parser which has a reference back to a Query Parser
        /// </summary>
        /// <param name="parser">Query Parser</param>
        /// <param name="allowAggregates">Whether Aggregates are allowed in Expressions</param>
        public SparqlExpressionParser(SparqlQueryParser parser, bool allowAggregates)
        {
            _parser = parser;
            _allowAggregates = allowAggregates;
        }

        /// <summary>
        /// Sets the Base Uri used to resolve URIs and QNames
        /// </summary>
        public Uri BaseUri
        {
            set
            {
                _baseUri = value;
            }
        }

        /// <summary>
        /// Sets the Namespace Map used to resolve QNames
        /// </summary>
        public NamespaceMapper NamespaceMap
        {
            set
            {
                _nsmapper = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether Aggregates are permitted in Expressions
        /// </summary>
        public bool AllowAggregates
        {
            get
            {
                return _allowAggregates;
            }
            set
            {
                _allowAggregates = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Syntax that should be supported
        /// </summary>
        public SparqlQuerySyntax SyntaxMode
        {
            get
            {
                return _syntax;
            }
            set
            {
                _syntax = value;
            }
        }

        /// <summary>
        /// Sets the Query Parser that the Expression Parser can call back into when needed
        /// </summary>
        public SparqlQueryParser QueryParser
        {
            set
            {
                _parser = value;
            }
        }

        /// <summary>
        /// Gets/Sets the locally scoped custom expression factories
        /// </summary>
        public IEnumerable<ISparqlCustomExpressionFactory> ExpressionFactories
        {
            get
            {
                return _factories;
            }
            set
            {
                if (value != null)
                {
                    _factories = value;
                }
            }
        }

        /// <summary>
        /// Parses a SPARQL Expression
        /// </summary>
        /// <param name="tokens">Tokens that the Expression should be parsed from</param>
        /// <returns></returns>
        public ISparqlExpression Parse(Queue<IToken> tokens)
        {
            try
            {
                return TryParseConditionalOrExpression(tokens);
            }
            catch (InvalidOperationException ex)
            {
                // The Queue was empty
                throw new RdfParseException("Unexpected end of Token Queue while trying to parse an Expression", ex);
            }
        }

        private ISparqlExpression TryParseConditionalOrExpression(Queue<IToken> tokens)
        {
            // Get the first Term in the Expression
            ISparqlExpression firstTerm = TryParseConditionalAndExpression(tokens);

            if (tokens.Count > 0) 
            {
                // Expect an || Token
                IToken next = tokens.Dequeue();
                if (next.TokenType == Token.OR) 
                {
                    return new OrExpression(firstTerm, TryParseConditionalOrExpression(tokens));
                } 
                else 
                {
                    throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered while trying to parse a Conditional Or expression", next);
                }
            } 
            else 
            {
                return firstTerm;
            }
        }

        private ISparqlExpression TryParseConditionalAndExpression(Queue<IToken> tokens)
        {
            // Get the first Term in the Expression
            ISparqlExpression firstTerm = TryParseValueLogical(tokens);

            if (tokens.Count > 0)
            {
                // Expect an && Token
                IToken next = tokens.Peek();
                if (next.TokenType == Token.AND)
                {
                    tokens.Dequeue();
                    return new AndExpression(firstTerm, TryParseConditionalAndExpression(tokens));
                }
                else
                {
                    return firstTerm;
                    // throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered while trying to parse a Conditional And expression");
                }
            }
            else
            {
                return firstTerm;
            }
        }

        private ISparqlExpression TryParseValueLogical(Queue<IToken> tokens)
        {
            return TryParseRelationalExpression(tokens);
        }

        private ISparqlExpression TryParseRelationalExpression(Queue<IToken> tokens)
        {
            // Get the First Term of this Expression
            ISparqlExpression firstTerm = TryParseNumericExpression(tokens);

            if (tokens.Count > 0)
            {
                IToken next = tokens.Peek();
                switch (next.TokenType)
                {
                    case Token.EQUALS:
                        tokens.Dequeue();
                        return new EqualsExpression(firstTerm, TryParseNumericExpression(tokens));
                    case Token.NOTEQUALS:
                        tokens.Dequeue();
                        return new NotEqualsExpression(firstTerm, TryParseNumericExpression(tokens));
                    case Token.LESSTHAN:
                        tokens.Dequeue();
                        return new LessThanExpression(firstTerm, TryParseNumericExpression(tokens));
                    case Token.GREATERTHAN:
                        tokens.Dequeue();
                        return new GreaterThanExpression(firstTerm, TryParseNumericExpression(tokens));
                    case Token.LESSTHANOREQUALTO:
                        tokens.Dequeue();
                        return new LessThanOrEqualToExpression(firstTerm, TryParseNumericExpression(tokens));
                    case Token.GREATERTHANOREQUALTO:
                        tokens.Dequeue();
                        return new GreaterThanOrEqualToExpression(firstTerm, TryParseNumericExpression(tokens));
                    case Token.IN:
                    case Token.NOTIN:
                        return TryParseSetExpression(firstTerm, tokens);
                    default:
                        return firstTerm;
                }
            }
            else
            {
                return firstTerm;
            }
        }

        private ISparqlExpression TryParseNumericExpression(Queue<IToken> tokens)
        {
            return TryParseAdditiveExpression(tokens);
        }

        private ISparqlExpression TryParseAdditiveExpression(Queue<IToken> tokens)
        {
            // Get the First Term of this Expression
            ISparqlExpression firstTerm = TryParseMultiplicativeExpression(tokens);

            if (tokens.Count > 0)
            {
                IToken next = tokens.Peek();
                switch (next.TokenType)
                {
                    case Token.PLUS:
                        tokens.Dequeue();
                        return new AdditionExpression(firstTerm, TryParseMultiplicativeExpression(tokens));
                    case Token.MINUS:
                        tokens.Dequeue();
                        return new SubtractionExpression(firstTerm, TryParseMultiplicativeExpression(tokens));
                    case Token.PLAINLITERAL:
                        tokens.Dequeue();
                        return new AdditionExpression(firstTerm, TryParseNumericLiteral(next, tokens, true));
                    default:
                        return firstTerm;
                }
            }
            else
            {
                return firstTerm;
            }
        }

        private ISparqlExpression TryParseMultiplicativeExpression(Queue<IToken> tokens)
        {
            // Get the First Term of this Expression
            ISparqlExpression firstTerm = TryParseUnaryExpression(tokens);

            if (tokens.Count > 0)
            {
                IToken next = tokens.Peek();
                switch (next.TokenType)
                {
                    case Token.MULTIPLY:
                        tokens.Dequeue();
                        return new MultiplicationExpression(firstTerm, TryParseUnaryExpression(tokens));
                    case Token.DIVIDE:
                        tokens.Dequeue();
                        return new DivisionExpression(firstTerm, TryParseUnaryExpression(tokens));
                    default:
                        return firstTerm;
                }
            }
            else
            {
                return firstTerm;
            }
        }

        private ISparqlExpression TryParseUnaryExpression(Queue<IToken> tokens)
        {
            IToken next = tokens.Peek();

            switch (next.TokenType)
            {
                case Token.NEGATION:
                    tokens.Dequeue();
                    return new NotExpression(TryParsePrimaryExpression(tokens));
                case Token.PLUS:
                    // Semantically Unary Plus does nothing so no special expression class for it
                    tokens.Dequeue();
                    return TryParsePrimaryExpression(tokens);
                case Token.MINUS:
                    tokens.Dequeue();
                    return new MinusExpression(TryParsePrimaryExpression(tokens));
                default:
                    return TryParsePrimaryExpression(tokens);
            }
        }

        private ISparqlExpression TryParsePrimaryExpression(Queue<IToken> tokens)
        {
            IToken next = tokens.Peek();

            switch (next.TokenType)
            {
                case Token.LEFTBRACKET:
                    return TryParseBrackettedExpression(tokens);

                case Token.ABS:
                case Token.BNODE:
                case Token.BOUND:
                case Token.CALL:
                case Token.CEIL:
                case Token.COALESCE:
                case Token.CONCAT:
                case Token.DATATYPEFUNC:
                case Token.DAY:
                case Token.ENCODEFORURI:
                case Token.EXISTS:
                case Token.FLOOR:
                case Token.HOURS:
                case Token.IF:
                case Token.IRI:
                case Token.ISBLANK:
                case Token.ISIRI:
                case Token.ISLITERAL:
                case Token.ISNUMERIC:
                case Token.ISURI:
                case Token.LANG:
                case Token.LANGMATCHES:
                case Token.LCASE:
                case Token.MD5:
                case Token.MINUTES:
                case Token.MONTH:
                case Token.NOTEXISTS:
                case Token.NOW:
                case Token.RAND:
                case Token.REGEX:
                case Token.REPLACE:
                case Token.ROUND:
                case Token.SAMETERM:
                case Token.SECONDS:
                case Token.SHA1:
                case Token.SHA224:
                case Token.SHA256:
                case Token.SHA384:
                case Token.SHA512:
                case Token.STR:
                case Token.STRAFTER:
                case Token.STRBEFORE:
                case Token.CONTAINS:
                case Token.STRDT:
                case Token.STRENDS:
                case Token.STRLANG:
                case Token.STRLEN:
                case Token.STRSTARTS:
                case Token.STRUUID:
                case Token.SUBSTR:
                case Token.TIMEZONE:
                case Token.TZ:
                case Token.UCASE:
                case Token.URIFUNC:
                case Token.UUID:
                case Token.YEAR:
                    if (_syntax == SparqlQuerySyntax.Sparql_1_0 && SparqlSpecsHelper.IsFunctionKeyword11(next.Value)) throw Error("The function " + next.Value + " is not supported in SPARQL 1.0", next);
                    return TryParseBuiltInCall(tokens);

                case Token.AVG:
                case Token.COUNT:
                case Token.GROUPCONCAT:
                case Token.MAX:
                case Token.MEDIAN:
                case Token.MIN:
                case Token.MODE:
                case Token.NMAX:
                case Token.NMIN:
                case Token.SAMPLE:
                case Token.SUM:
                    if (_allowAggregates)
                    {
                        return TryParseAggregateExpression(tokens);
                    }
                    else
                    {
                        throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, Aggregates are not permitted in this Expression or you have attempted to nest aggregates", next);
                    }

                case Token.URI:
                case Token.QNAME:
                    return TryParseIriRefOrFunction(tokens);

                case Token.LITERAL:
                case Token.LONGLITERAL:
                    return TryParseRdfLiteral(tokens);
                    
                case Token.PLAINLITERAL:
                    return TryParseBooleanOrNumericLiteral(tokens);

                case Token.VARIABLE:
                    tokens.Dequeue();
                    return new VariableTerm(next.Value);

                default:
                    throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered while trying to parse a Primary Expression",next);
            }
        }

        private ISparqlExpression TryParseBrackettedExpression(Queue<IToken> tokens)
        {
            return TryParseBrackettedExpression(tokens, true);
        }

        private ISparqlExpression TryParseBrackettedExpression(Queue<IToken> tokens, bool requireOpeningLeftBracket)
        {
            bool temp = false;
            return TryParseBrackettedExpression(tokens, requireOpeningLeftBracket, out temp);
        }

        private ISparqlExpression TryParseBrackettedExpression(Queue<IToken> tokens, bool requireOpeningLeftBracket, out bool commaTerminated)
        {
            bool temp = false;
            return TryParseBrackettedExpression(tokens, requireOpeningLeftBracket, out commaTerminated, out temp);
        }

        private ISparqlExpression TryParseBrackettedExpression(Queue<IToken> tokens, bool requireOpeningLeftBracket, out bool commaTerminated, out bool semicolonTerminated)
        {
            IToken next;

            commaTerminated = false;
            semicolonTerminated = false;

            // Discard the Opening Bracket
            if (requireOpeningLeftBracket)
            {
                next = tokens.Dequeue();
                if (next.TokenType != Token.LEFTBRACKET)
                {
                    throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Left Bracket to start a Bracketted Expression",next);
                }
            }

            int openBrackets = 1;
            Queue<IToken> exprTerms = new Queue<IToken>();

            while (openBrackets > 0)
            {
                // Get next Token
                next = tokens.Peek();

                // Take account of nesting
                if (next.TokenType == Token.LEFTBRACKET)
                {
                    openBrackets++;
                }
                else if (next.TokenType == Token.RIGHTBRACKET)
                {
                    openBrackets--;
                }
                else if (next.TokenType == Token.COMMA && openBrackets == 1)
                {
                    openBrackets--;
                    commaTerminated = true;
                }
                else if (next.TokenType == Token.SEMICOLON && openBrackets == 1)
                {
                    openBrackets--;
                    semicolonTerminated = true;
                }
                else if (next.TokenType == Token.DISTINCT && openBrackets == 1)
                {
                    // DISTINCT can terminate the Tokens that make an expression if it occurs as the first thing and only 1 bracket is open
                    if (tokens.Count == 0)
                    {
                        tokens.Dequeue();
                        commaTerminated = true;
                        return new DistinctModifier();
                    }
                    else
                    {
                        throw Error("Unexpected DISTINCT Keyword Token encountered, DISTINCT modifier keyword may only occur as the first argument to an aggregate function", next);
                    }
                }

                if (openBrackets > 0)
                {
                    exprTerms.Enqueue(next);
                }
                tokens.Dequeue();
            }

            if (exprTerms.Count > 0)
            {
                // Recurse to invoke self
                return Parse(exprTerms);
            }
            else
            {
                return null;
            }
        }

        private ISparqlExpression TryParseBuiltInCall(Queue<IToken> tokens)
        {
            IToken next = tokens.Dequeue();
            bool comma = false, first = true;
            List<ISparqlExpression> args;
            ISparqlExpression strExpr;

            switch (next.TokenType)
            {
                case Token.ABS:
                    return new AbsFunction(TryParseBrackettedExpression(tokens));
                case Token.BNODE:
                    return new BNodeFunction(TryParseBrackettedExpression(tokens));                

                case Token.BOUND:
                    // Expect a Left Bracket, Variable and then a Right Bracket
                    next = tokens.Dequeue();
                    if (next.TokenType == Token.LEFTBRACKET)
                    {
                        next = tokens.Dequeue();
                        if (next.TokenType == Token.VARIABLE)
                        {
                            VariableTerm varExpr = new VariableTerm(next.Value);
                            next = tokens.Dequeue();
                            if (next.TokenType == Token.RIGHTBRACKET)
                            {
                                return new BoundFunction(varExpr);
                            }
                            else
                            {
                                throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, a Right Bracket to end a BOUND function call was expected",next);
                            }
                        }
                        else
                        {
                            throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, a Variable Token for a BOUND function call was expected", next);
                        }
                    }
                    else
                    {
                        throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, a Left Bracket to start a BOUND function call was expected", next);
                    }

                case Token.CALL:
                    if (_syntax != SparqlQuerySyntax.Extended) throw Error("The CALL keyword is only valid when using SPARQL 1.1 Extended syntax", next);
                    args = new List<ISparqlExpression>();
                    do
                    {
                        args.Add(TryParseBrackettedExpression(tokens, first, out comma));
                        first = false;
                    } while (comma);

                    return new CallFunction(args);

                case Token.CEIL:
                    return new CeilFunction(TryParseBrackettedExpression(tokens));

                case Token.COALESCE:
                    // Get as many argument expressions as we can
                    args = new List<ISparqlExpression>();
                    do
                    {
                        args.Add(TryParseBrackettedExpression(tokens, first, out comma));
                        first = false;
                    } while (comma);

                    return new CoalesceFunction(args);

                case Token.CONCAT:
                    // Get as many argument expressions as we can
                    args = new List<ISparqlExpression>();
                    do
                    {
                        args.Add(TryParseBrackettedExpression(tokens, first, out comma));
                        first = false;
                    } while (comma);

                    return new ConcatFunction(args);

                case Token.CONTAINS:
                    return new ContainsFunction(TryParseBrackettedExpression(tokens), TryParseBrackettedExpression(tokens, false));
                case Token.DATATYPEFUNC:
                    if (_syntax == SparqlQuerySyntax.Sparql_1_0)
                    {
                        return new DataTypeFunction(TryParseBrackettedExpression(tokens));
                    }
                    else
                    {
                        return new DataType11Function(TryParseBrackettedExpression(tokens));
                    }
                case Token.DAY:
                    return new DayFunction(TryParseBrackettedExpression(tokens));
                case Token.ENCODEFORURI:
                    return new EncodeForUriFunction(TryParseBrackettedExpression(tokens));
                case Token.FLOOR:
                    return new FloorFunction(TryParseBrackettedExpression(tokens));
                case Token.HOURS:
                    return new HoursFunction(TryParseBrackettedExpression(tokens));
                case Token.IF:
                    return new IfElseFunction(TryParseBrackettedExpression(tokens, true, out comma), TryParseBrackettedExpression(tokens, false, out comma), TryParseBrackettedExpression(tokens, false, out comma));
                case Token.IRI:
                case Token.URIFUNC:
                    return new IriFunction(TryParseBrackettedExpression(tokens));
                case Token.ISBLANK:
                    return new IsBlankFunction(TryParseBrackettedExpression(tokens));
                case Token.ISIRI:
                    return new IsIriFunction(TryParseBrackettedExpression(tokens));
                case Token.ISLITERAL:
                    return new IsLiteralFunction(TryParseBrackettedExpression(tokens));
                case Token.ISNUMERIC:
                    return new IsNumericFunction(TryParseBrackettedExpression(tokens));
                case Token.ISURI:
                    return new IsUriFunction(TryParseBrackettedExpression(tokens));
                case Token.LANG:
                    return new LangFunction(TryParseBrackettedExpression(tokens));
                case Token.LANGMATCHES:
                    return new LangMatchesFunction(TryParseBrackettedExpression(tokens), TryParseBrackettedExpression(tokens, false));
                case Token.LCASE:
                    return new LCaseFunction(TryParseBrackettedExpression(tokens));
                case Token.MD5:
                    return new MD5HashFunction(TryParseBrackettedExpression(tokens));
                case Token.MINUTES:
                    return new MinutesFunction(TryParseBrackettedExpression(tokens));
                case Token.MONTH:
                    return new MonthFunction(TryParseBrackettedExpression(tokens));

                case Token.NOW:
                    // Expect a () after the Keyword Token
                    TryParseNoArgs(tokens, "NOW");
                    return new NowFunction();

                case Token.RAND:
                    // Expect a () after the Keyword Token
                    TryParseNoArgs(tokens, "RAND");
                    return new RandFunction();

                case Token.REGEX:
                    return TryParseRegexExpression(tokens);
                case Token.REPLACE:
                    // REPLACE may have 3/4 arguments
                    strExpr = TryParseBrackettedExpression(tokens);
                    ISparqlExpression patternExpr = TryParseBrackettedExpression(tokens, false);
                    ISparqlExpression replaceExpr = TryParseBrackettedExpression(tokens, false, out comma);
                    if (comma)
                    {
                        ISparqlExpression opsExpr = TryParseBrackettedExpression(tokens, false);
                        return new ReplaceFunction(strExpr, patternExpr, replaceExpr, opsExpr);
                    }
                    else
                    {
                        return new ReplaceFunction(strExpr, patternExpr, replaceExpr);
                    }
                case Token.ROUND:
                    return new RoundFunction(TryParseBrackettedExpression(tokens));
                case Token.SAMETERM:
                    return new SameTermFunction(TryParseBrackettedExpression(tokens), TryParseBrackettedExpression(tokens, false));
                case Token.SECONDS:
                    return new SecondsFunction(TryParseBrackettedExpression(tokens));
                case Token.SHA1:
                    return new Sha1HashFunction(TryParseBrackettedExpression(tokens));
                case Token.SHA256:
                    return new Sha256HashFunction(TryParseBrackettedExpression(tokens));
                case Token.SHA384:
                    return new Sha384HashFunction(TryParseBrackettedExpression(tokens));
                case Token.SHA512:
                    return new Sha512HashFunction(TryParseBrackettedExpression(tokens));
                case Token.STR:
                    return new StrFunction(TryParseBrackettedExpression(tokens));
                case Token.STRAFTER:
                    return new StrAfterFunction(TryParseBrackettedExpression(tokens), TryParseBrackettedExpression(tokens, false));
                case Token.STRBEFORE:
                    return new StrBeforeFunction(TryParseBrackettedExpression(tokens), TryParseBrackettedExpression(tokens, false));
                case Token.STRDT:
                    return new StrDtFunction(TryParseBrackettedExpression(tokens), TryParseBrackettedExpression(tokens, false));
                case Token.STRENDS:
                    return new StrEndsFunction(TryParseBrackettedExpression(tokens), TryParseBrackettedExpression(tokens, false));
                case Token.STRLANG:
                    return new StrLangFunction(TryParseBrackettedExpression(tokens), TryParseBrackettedExpression(tokens, false));
                case Token.STRLEN:
                    return new StrLenFunction(TryParseBrackettedExpression(tokens));
                case Token.STRSTARTS:
                    return new StrStartsFunction(TryParseBrackettedExpression(tokens), TryParseBrackettedExpression(tokens, false));
                case Token.STRUUID:
                    TryParseNoArgs(tokens, "STRUUID");
                    return new StrUUIDFunction();

                case Token.SUBSTR:
                    // SUBSTR may have 2/3 arguments
                    strExpr = TryParseBrackettedExpression(tokens);
                    ISparqlExpression startExpr = TryParseBrackettedExpression(tokens, false, out comma);
                    if (comma)
                    {
                        ISparqlExpression lengthExpr = TryParseBrackettedExpression(tokens, false);
                        return new SubStrFunction(strExpr, startExpr, lengthExpr);
                    }
                    else
                    {
                        return new SubStrFunction(strExpr, startExpr);
                    }

                case Token.TIMEZONE:
                    return new TimezoneFunction(TryParseBrackettedExpression(tokens));
                case Token.TZ:
                    return new TZFunction(TryParseBrackettedExpression(tokens));
                case Token.UCASE:
                    return new UCaseFunction(TryParseBrackettedExpression(tokens));
                case Token.UUID:
                    TryParseNoArgs(tokens, "UUID");
                    return new UUIDFunction();
                case Token.YEAR:
                    return new YearFunction(TryParseBrackettedExpression(tokens));

                case Token.EXISTS:
                case Token.NOTEXISTS:
                    if (_syntax == SparqlQuerySyntax.Sparql_1_0) throw new RdfParseException("EXISTS/NOT EXISTS clauses are not supported in SPARQL 1.0");
                    if (_parser == null) throw new RdfParseException("Unable to parse an EXISTS/NOT EXISTS as there is no Query Parser to call into");

                    // Gather Tokens for the Pattern
                    NonTokenisedTokenQueue temptokens = new NonTokenisedTokenQueue();
                    int openbrackets = 0;
                    bool mustExist = (next.TokenType == Token.EXISTS);
                    do
                    {
                        if (tokens.Count == 0) throw new RdfParseException("Unexpected end of Tokens while trying to parse an EXISTS/NOT EXISTS function");

                        next = tokens.Dequeue();
                        if (next.TokenType == Token.LEFTCURLYBRACKET)
                        {
                            openbrackets++;
                        }
                        else if (next.TokenType == Token.RIGHTCURLYBRACKET)
                        {
                            openbrackets--;
                        }
                        temptokens.Enqueue(next);
                    } while (openbrackets > 0);

                    // Call back into the Query Parser to try and Parse the Graph Pattern for the Function
                    SparqlQueryParserContext tempcontext = new SparqlQueryParserContext(temptokens);
                    tempcontext.Query.NamespaceMap.Import(_nsmapper);
                    tempcontext.Query.BaseUri = _baseUri;
                    return new ExistsFunction(_parser.TryParseGraphPattern(tempcontext, true), mustExist);

                default:
                    throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered while trying to parse a Built-in Function call", next);
            }
        }

        private void TryParseNoArgs(Queue<IToken> tokens, String function)
        {
            IToken next = tokens.Dequeue();
            if (next.TokenType != Token.LEFTBRACKET) throw Error("Expected a Left Bracket after a " + function + " keyword to call the " + function + "() function", next);
            next = tokens.Dequeue();
            if (next.TokenType != Token.RIGHTBRACKET) throw Error("Expected a Right Bracket after " + function + "( since the " + function + "() function does not take any arguments", next);
        }

        private ISparqlExpression TryParseRegexExpression(Queue<IToken> tokens)
        {
            bool hasOptions = false;

            // Get Text and Pattern Expressions
            ISparqlExpression textExpr = TryParseBrackettedExpression(tokens);
            ISparqlExpression patternExpr = TryParseBrackettedExpression(tokens, false, out hasOptions);

            // Check whether we need to get an Options Expression
            if (hasOptions)
            {
                ISparqlExpression optionExpr = TryParseBrackettedExpression(tokens, false);
                return new RegexFunction(textExpr, patternExpr, optionExpr);
            }
            else
            {
                return new RegexFunction(textExpr, patternExpr);
            }
        }

        private ISparqlExpression TryParseIriRefOrFunction(Queue<IToken> tokens)
        {
            // Get the Uri/QName Token
            IToken first = tokens.Dequeue();

            // Resolve the Uri
            Uri u;
            if (first.TokenType == Token.QNAME)
            {
                // Resolve QName
                u = UriFactory.Create(Tools.ResolveQName(first.Value, _nsmapper, _baseUri));
            }
            else
            {
                u = UriFactory.Create(Tools.ResolveUri(first.Value, _baseUri.ToSafeString()));
            }
            
            // Get the Argument List (if any)
            if (tokens.Count > 0)
            {
                IToken next = tokens.Peek();
                if (next.TokenType == Token.LEFTBRACKET)
                {
                    bool comma = false, semicolon = false;
                    List<ISparqlExpression> args = new List<ISparqlExpression>();
                    args.Add(TryParseBrackettedExpression(tokens, true, out comma, out semicolon));

                    while (comma && !semicolon)
                    {
                        args.Add(TryParseBrackettedExpression(tokens, false, out comma, out semicolon));
                    }

                    // If there are no arguments (one null argument) then discard
                    if (args.Count == 1 && args.First() == null) args.Clear();

                    // Check whether we need to parse Scalar Arguments
                    Dictionary<String, ISparqlExpression> scalarArgs = null;
                    if (semicolon)
                    {
                        if (_syntax != SparqlQuerySyntax.Extended) throw new RdfParseException("Arguments List terminated by a Semicolon - Arbitrary Scalar Arguments for Extension Functions/Aggregates are not permitted in SPARQL 1.1");
                        scalarArgs = TryParseScalarArguments(first, tokens);
                    }

                    // Return an Extension Function expression
                    ISparqlExpression expr = SparqlExpressionFactory.CreateExpression(u, args, _factories);
                    if (expr is AggregateTerm)
                    {
                        if (!_allowAggregates) throw new RdfParseException("Aggregate Expression '" + expr.ToString() + "' encountered but aggregates are not permitted in this Expression");
                    }
                    return expr;
                }
                else
                {
                    // Just an IRIRef
                    return new ConstantTerm(new UriNode(null, u));
                }
            }
            else
            {
                // Just an IRIRef
                return new ConstantTerm(new UriNode(null, u));
            }
        }

        private ISparqlExpression TryParseRdfLiteral(Queue<IToken> tokens)
        {
            // First Token will be the String value of this RDF Literal
            IToken str = tokens.Dequeue();

            // Might have a Language Specifier/DataType afterwards
            if (tokens.Count > 0)
            {
                IToken next = tokens.Peek();
                if (next.TokenType == Token.LANGSPEC)
                {
                    tokens.Dequeue();
                    return new ConstantTerm(new LiteralNode(null, str.Value, next.Value));
                }
                else if (next.TokenType == Token.HATHAT)
                {
                    tokens.Dequeue();

                    // Should be a DataTypeToken afterwards
                    next = tokens.Dequeue();
                    LiteralWithDataTypeToken dtlit = new LiteralWithDataTypeToken(str, (DataTypeToken)next); ;
                    Uri u;

                    if (next.Value.StartsWith("<"))
                    {
                        u = UriFactory.Create(next.Value.Substring(1, next.Value.Length - 2));
                    }
                    else
                    {
                        // Resolve the QName
                        u = UriFactory.Create(Tools.ResolveQName(next.Value, _nsmapper, _baseUri));
                    }

                    if (SparqlSpecsHelper.GetNumericTypeFromDataTypeUri(u) != SparqlNumericType.NaN)
                    {
                        // Should be a Number
                        return TryParseNumericLiteral(dtlit, tokens, false);
                    }
                    else if (XmlSpecsHelper.XmlSchemaDataTypeBoolean.Equals(u.AbsoluteUri))
                    {
                        // Appears to be a Boolean
                        bool b;
                        if (Boolean.TryParse(dtlit.Value, out b))
                        {
                            return new ConstantTerm(new BooleanNode(null, b));
                        }
                        else
                        {
                            return new ConstantTerm(new StringNode(null, dtlit.Value, dtlit.DataType));
                        }
                    }
                    else
                    {
                        // Just a datatyped Literal Node
                        return new ConstantTerm(new LiteralNode(null, str.Value, u));
                    }
                }
                else
                {
                    return new ConstantTerm(new LiteralNode(null, str.Value));
                }
            }
            else
            {
                return new ConstantTerm(new LiteralNode(null, str.Value));
            }

        }

        private ISparqlExpression TryParseBooleanOrNumericLiteral(Queue<IToken> tokens)
        {
            // First Token must be a Plain Literal
            IToken lit = tokens.Dequeue();

            if (lit.Value.Equals("true"))
            {
                return new ConstantTerm(new BooleanNode(null, true));
            }
            else if (lit.Value.Equals("false"))
            {
                return new ConstantTerm(new BooleanNode(null, false));
            }
            else
            {
                return TryParseNumericLiteral(lit, tokens, true);
            }
        }

        private ISparqlExpression TryParseNumericLiteral(IToken literal, Queue<IToken> tokens, bool requireValidLexicalForm)
        {
            switch (literal.TokenType)
            {
                case Token.PLAINLITERAL:
                    // Use Regular Expressions to see what type it is
                    if (SparqlSpecsHelper.IsInteger(literal.Value))
                    {
                        return new ConstantTerm(new LongNode(null, Int64.Parse(literal.Value)));
                    }
                    else if (SparqlSpecsHelper.IsDecimal(literal.Value))
                    {
                        return new ConstantTerm(new DecimalNode(null, Decimal.Parse(literal.Value, NumberStyles.Any, CultureInfo.InvariantCulture)));
                    }
                    else if (SparqlSpecsHelper.IsDouble(literal.Value))
                    {
                        return new ConstantTerm(new DoubleNode(null, Double.Parse(literal.Value, NumberStyles.Any, CultureInfo.InvariantCulture)));
                    }
                    else
                    {
                        throw Error("The Plain Literal '" + literal.Value + "' is not a valid Integer, Decimal or Double", literal);
                    }
                    
                case Token.LITERALWITHDT:
                    // Get the Data Type Uri
                    String dt = ((LiteralWithDataTypeToken)literal).DataType;
                    String dtUri;
                    if (dt.StartsWith("<"))
                    {
                        String baseUri = (_baseUri == null) ? String.Empty : _baseUri.AbsoluteUri;
                        dtUri = Tools.ResolveUri(dt.Substring(1, dt.Length - 2), baseUri);
                    }
                    else
                    {
                        dtUri = Tools.ResolveQName(dt, _nsmapper, _baseUri);
                    }

                    // Try to return a numeric expression, enforce the need for a valid numeric value where relevant
                    LiteralNode lit = new LiteralNode(null, literal.Value, UriFactory.Create(dtUri));
                    IValuedNode value = lit.AsValuedNode();
                    if (requireValidLexicalForm && value.NumericType == SparqlNumericType.NaN)
                    {
                        throw Error("The Literal '" + literal.Value + "' with Datatype URI '" + dtUri + "' is not a valid Integer, Decimal or Double", literal);
                    }
                    else
                    {
                        return new ConstantTerm(value);
                    }
                    
                case Token.LITERAL:
                    // Check if there's a Datatype following the Literal
                    if (tokens.Count > 0)
                    {
                        IToken next = tokens.Peek();
                        if (next.TokenType == Token.HATHAT)
                        {
                            tokens.Dequeue();
                            // Should now see a DataTypeToken
                            DataTypeToken datatype = (DataTypeToken)tokens.Dequeue();
                            LiteralWithDataTypeToken dtlit = new LiteralWithDataTypeToken(literal, datatype);

                            // Self-recurse to save replicating code
                            return TryParseNumericLiteral(dtlit, tokens, true);
                        }
                    }
                    // Use Regex to see if it's a Integer/Decimal/Double
                    if (SparqlSpecsHelper.IsInteger(literal.Value))
                    {
                        return new ConstantTerm(new LongNode(null, Int64.Parse(literal.Value)));
                    }
                    else if (SparqlSpecsHelper.IsDecimal(literal.Value))
                    {
                        return new ConstantTerm(new DecimalNode(null, Decimal.Parse(literal.Value)));
                    }
                    else if (SparqlSpecsHelper.IsDouble(literal.Value))
                    {
                        return new ConstantTerm(new DoubleNode(null, Double.Parse(literal.Value)));
                    }
                    else
                    {
                        // Otherwise treat as a Node Expression
                        throw Error("The Literal '" + literal.Value + "' is not a valid Integer, Decimal or Double", literal);
                    }

                default:
                    throw Error("Unexpected Token '" + literal.GetType().ToString() + "' encountered while trying to parse a Numeric Literal", literal);
            }
        }

        private ISparqlExpression TryParseAggregateExpression(Queue<IToken> tokens)
        {
            if (_syntax == SparqlQuerySyntax.Sparql_1_0) throw new RdfParseException("Aggregates are not permitted in SPARQL 1.0");

            IToken agg = tokens.Dequeue();
            ISparqlExpression aggExpr = null;
            bool distinct = false, all = false;
            bool scalarArgs = false;

            // Turn off aggregate allowance since aggregates may not be nested
            bool aggsAllowed = _allowAggregates;
            _allowAggregates = false;

            // Expect a Left Bracket next
            IToken next = tokens.Dequeue();
            if (next.TokenType != Token.LEFTBRACKET)
            {
                throw Error("Unexpected Token '" + next.GetType().ToString() + "', expected a Left Bracket after an Aggregate Keyword", next);
            }

            // Then a possible DISTINCT/ALL
            next = tokens.Peek();
            if (next.TokenType == Token.DISTINCT)
            {
                distinct = true;
                tokens.Dequeue();
            }
            next = tokens.Peek();
            if (next.TokenType == Token.ALL || next.TokenType == Token.MULTIPLY)
            {
                all = true;
                tokens.Dequeue();
            }
            next = tokens.Peek();

            // If we've seen an ALL then we need the closing bracket
            if (all && next.TokenType != Token.RIGHTBRACKET)
            {
                throw Error("Unexpected Token '" + next.GetType().ToString() + "', expected a Right Bracket after the * specifier in an aggregate to terminate the aggregate", next);
            }
            else if (all && agg.TokenType != Token.COUNT)
            {
                throw new RdfQueryException("Cannot use the * specifier in aggregates other than COUNT");
            }
            else if (!all)
            {
                // If it's not an all then we expect some expression(s)
                // Gather the Tokens and parse the Expression
                Queue<IToken> subtokens = new Queue<IToken>();

                int openBrackets = 1;
                List<ISparqlExpression> expressions = new List<ISparqlExpression>();

                while (openBrackets > 0)
                {
                    subtokens = new Queue<IToken>();
                    next = tokens.Dequeue();
                    do
                    {
                        if (next.TokenType == Token.LEFTBRACKET)
                        {
                            openBrackets++;
                        }
                        else if (next.TokenType == Token.RIGHTBRACKET)
                        {
                            openBrackets--;
                        }
                        else if (next.TokenType == Token.COMMA)
                        {
                            // If we see a comma when we only have 1 bracket open then it is separating argument expressions
                            if (openBrackets == 1)
                            {
                                break;
                            }
                        }
                        else if (next.TokenType == Token.SEMICOLON)
                        {
                            // If we see a semicolon when we only have 1 bracket open then this indicates we have scalar arguments in-use
                            if (openBrackets == 1)
                            {
                                scalarArgs = true;
                                break;
                            }
                        }

                        if (openBrackets > 0)
                        {
                            subtokens.Enqueue(next);
                            next = tokens.Dequeue();
                        }
                    } while (openBrackets > 0);

                    // Parse this expression and add to the list of expressions we're concatenating
                    expressions.Add(Parse(subtokens));

                    // Once we've hit the ; for the scalar arguments then we can stop looking for expressions
                    if (scalarArgs) break;

                    // If we've hit a , then openBrackets will still be one and we'll go around again looking for another expression
                    // Otherwise we've reached the end of the aggregate and there was no ; for scalar arguments
                }

                if (expressions.Count == 0) throw new RdfParseException("Aggregate must have at least one argument expression unless they are a COUNT(*)");
                if (expressions.Count > 1) throw new RdfParseException("The " + agg.Value + " aggregate does not support more than one argument expression");
                aggExpr = expressions.First();
            }
            else
            {
                tokens.Dequeue();
            }

            // If the aggregate uses scalar arguments then we'll parse them here
            Dictionary<String, ISparqlExpression> scalarArguments = new Dictionary<string, ISparqlExpression>();
            if (scalarArgs)
            {
                scalarArguments = TryParseScalarArguments(agg, tokens);
            }

            // Reset Aggregate Allowance
            _allowAggregates = aggsAllowed;

            // Now we need to generate the actual expression
            switch (agg.TokenType)
            {
                case Token.AVG:
                    // AVG Aggregate
                    if (aggExpr is VariableTerm)
                    {
                        return new AggregateTerm(new AverageAggregate((VariableTerm)aggExpr, distinct));
                    }
                    else
                    {
                        return new AggregateTerm(new AverageAggregate(aggExpr, distinct));
                    }

                case Token.COUNT:
                    // COUNT Aggregate
                    if (all)
                    {
                        if (distinct)
                        {
                            return new AggregateTerm(new CountAllDistinctAggregate());
                        }
                        else
                        {
                            return new AggregateTerm(new CountAllAggregate());
                        }
                    }
                    else if (aggExpr is VariableTerm)
                    {
                        if (distinct)
                        {
                            return new AggregateTerm(new CountDistinctAggregate((VariableTerm)aggExpr));
                        }
                        else
                        {
                            return new AggregateTerm(new CountAggregate((VariableTerm)aggExpr));
                        }
                    }
                    else
                    {
                        if (distinct)
                        {
                            return new AggregateTerm(new CountDistinctAggregate(aggExpr));
                        }
                        else
                        {
                            return new AggregateTerm(new CountAggregate(aggExpr));
                        }
                    }
                case Token.GROUPCONCAT:
                    if (scalarArgs)
                    {
                        if (!scalarArguments.ContainsKey(SparqlSpecsHelper.SparqlKeywordSeparator)) throw new RdfParseException("The GROUP_CONCAT aggregate has Scalar Arguments but does not have the expected SEPARATOR argument");
                        return new AggregateTerm(new GroupConcatAggregate(aggExpr, scalarArguments[SparqlSpecsHelper.SparqlKeywordSeparator], distinct));
                    }
                    else
                    {
                        return new AggregateTerm(new GroupConcatAggregate(aggExpr, distinct));
                    }

                case Token.MAX:
                    // MAX Aggregate
                    if (aggExpr is VariableTerm)
                    {
                        return new AggregateTerm(new MaxAggregate((VariableTerm)aggExpr, distinct));
                    }
                    else
                    {
                        return new AggregateTerm(new MaxAggregate(aggExpr, distinct));
                    }

                case Token.MEDIAN:
                    // MEDIAN Aggregate
                    if (_syntax != SparqlQuerySyntax.Extended) throw new RdfParseException("The MEDIAN aggregate is only supported when the Syntax is set to Extended.");
                    if (aggExpr is VariableTerm)
                    {
                        return new AggregateTerm(new MedianAggregate((VariableTerm)aggExpr, distinct));
                    }
                    else
                    {
                        return new AggregateTerm(new MedianAggregate(aggExpr, distinct));
                    }

                case Token.MIN:
                    // MIN Aggregate
                    if (aggExpr is VariableTerm)
                    {
                        return new AggregateTerm(new MinAggregate((VariableTerm)aggExpr, distinct));
                    }
                    else
                    {
                        return new AggregateTerm(new MinAggregate(aggExpr, distinct));
                    }

                case Token.MODE:
                    // MODE Aggregate
                    if (_syntax != SparqlQuerySyntax.Extended) throw new RdfParseException("The MODE aggregate is only supported when the Syntax is set to Extended.");
                    if (aggExpr is VariableTerm)
                    {
                        return new AggregateTerm(new ModeAggregate((VariableTerm)aggExpr, distinct));
                    }
                    else
                    {
                        return new AggregateTerm(new ModeAggregate(aggExpr, distinct));
                    }

                case Token.NMAX:
                    // NMAX Aggregate
                    if (_syntax != SparqlQuerySyntax.Extended) throw new RdfParseException("The NMAX (Numeric Maximum) aggregate is only supported when the Syntax is set to Extended.  To achieve an equivalent result in SPARQL 1.0/1.1 apply a FILTER to your query so the aggregated variable is only literals of the desired numeric type");
                    if (aggExpr is VariableTerm)
                    {
                        return new AggregateTerm(new NumericMaxAggregate((VariableTerm)aggExpr, distinct));
                    }
                    else
                    {
                        return new AggregateTerm(new NumericMaxAggregate(aggExpr, distinct));
                    }

                case Token.NMIN:
                    // NMIN Aggregate
                    if (_syntax != SparqlQuerySyntax.Extended) throw new RdfParseException("The NMIN (Numeric Minimum) aggregate is only supported when the Syntax is set to Extended.  To achieve an equivalent result in SPARQL 1.0/1.1 apply a FILTER to your query so the aggregated variable is only literals of the desired numeric type");
                    if (aggExpr is VariableTerm)
                    {
                        return new AggregateTerm(new NumericMinAggregate((VariableTerm)aggExpr, distinct));
                    }
                    else
                    {
                        return new AggregateTerm(new NumericMinAggregate(aggExpr, distinct));
                    }

                case Token.SAMPLE:
                    // SAMPLE Aggregate
                    if (distinct) throw new RdfParseException("DISTINCT modifier is not valid for the SAMPLE aggregate");
                    return new AggregateTerm(new SampleAggregate(aggExpr));

                case Token.SUM:
                    // SUM Aggregate
                    if (aggExpr is VariableTerm)
                    {
                        return new AggregateTerm(new SumAggregate((VariableTerm)aggExpr, distinct));
                    }
                    else
                    {
                        return new AggregateTerm(new SumAggregate(aggExpr, distinct));
                    }

                default:
                    // Should have already handled this but have to have it to keep the compiler happy
                    throw Error("Cannot parse an Aggregate since '" + agg.GetType().ToString() + "' is not an Aggregate Keyword Token", agg);
            }
        }

        private Dictionary<String, ISparqlExpression> TryParseScalarArguments(IToken funcToken, Queue<IToken> tokens)
        {
            // Parse the Scalar Arguments
            Dictionary<String, ISparqlExpression> scalarArguments = new Dictionary<string, ISparqlExpression>();
            IToken next;
            Queue<IToken> subtokens = new Queue<IToken>();
            int openBrackets = 1;

            while (openBrackets > 0)
            {
                // First expect a Keyword/QName/URI for the Scalar Argument Name
                String argName;
                next = tokens.Peek();
                switch (next.TokenType)
                {
                    case Token.SEPARATOR:
                        if (funcToken.TokenType == Token.GROUPCONCAT)
                        {
                            // OK
                            argName = SparqlSpecsHelper.SparqlKeywordSeparator;
                        }
                        else
                        {
                            throw Error("The SEPARATOR scalar argument is only valid with the GROUP_CONCAT aggregate", next);
                        }
                        break;

                    case Token.QNAME:
                    case Token.URI:
                        if (_syntax != SparqlQuerySyntax.Extended) throw new RdfParseException("Arbitrary Scalar Arguments for Aggregates are not permitted in SPARQL 1.1");

                        // Resolve QName/URI
                        if (next.TokenType == Token.QNAME)
                        {
                            argName = Tools.ResolveQName(next.Value, _nsmapper, _baseUri);
                        }
                        else
                        {
                            argName = Tools.ResolveUri(next.Value, _baseUri.ToSafeString());
                        }
                        break;

                    default:
                        throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Keyword/QName/URI for the Scalar Argument Name", next);
                }
                tokens.Dequeue();

                // After the Argument Name need an =
                next = tokens.Peek();
                if (next.TokenType != Token.EQUALS)
                {
                    throw Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a = after a Scalar Argument name in an aggregate", next);
                }
                tokens.Dequeue();

                // Get the subtokens for the Argument Expression
                next = tokens.Dequeue();
                do
                {
                    if (next.TokenType == Token.LEFTBRACKET)
                    {
                        openBrackets++;
                    }
                    else if (next.TokenType == Token.RIGHTBRACKET)
                    {
                        openBrackets--;
                    }
                    else if (next.TokenType == Token.COMMA)
                    {
                        // If we see a COMMA and there is only 1 bracket open then expect another argument
                        if (openBrackets == 1)
                        {
                            break;
                        }
                    }

                    // If not the end bracket then add it to the subtokens
                    if (openBrackets > 0)
                    {
                        subtokens.Enqueue(next);
                        next = tokens.Dequeue();
                    }
                } while (openBrackets > 0);

                // Parse the Subtokens into the Argument Expression
                if (scalarArguments.ContainsKey(argName))
                {
                    scalarArguments[argName] = Parse(subtokens);
                }
                else
                {
                    scalarArguments.Add(argName, Parse(subtokens));
                }
            }

            return scalarArguments;
        }

        private ISparqlExpression TryParseSetExpression(ISparqlExpression expr, Queue<IToken> tokens)
        {
            IToken next = tokens.Dequeue();
            bool inSet = (next.TokenType == Token.IN);
            List<ISparqlExpression> expressions = new List<ISparqlExpression>();

            // Expecting a ( afterwards
            next = tokens.Dequeue();
            if (next.TokenType == Token.LEFTBRACKET)
            {
                next = tokens.Peek();

                if (next.TokenType == Token.RIGHTBRACKET)
                {
                    tokens.Dequeue();
                } 
                else
                {
                    bool comma = false;
                    expressions.Add(TryParseBrackettedExpression(tokens, false, out comma));
                    while (comma)
                    {
                        expressions.Add(TryParseBrackettedExpression(tokens, false, out comma));
                    }
                }
            }
            else
            {
                throw Error("Expected a left bracket to start the set of values for an IN/NOT IN expression", next);
            }

            if (inSet)
            {
                return new InFunction(expr, expressions);
            }
            else
            {
                return new NotInFunction(expr, expressions);
            }
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
            output.Append(t.GetType().ToString());
            output.Append(" at Line ");
            output.Append(t.StartLine);
            output.Append(" Column ");
            output.Append(t.StartPosition);
            output.Append(" to Line ");
            output.Append(t.EndLine);
            output.Append(" Column ");
            output.Append(t.EndPosition);
            output.Append("]\n");
            output.Append(msg);

            return new RdfParseException(output.ToString(), t);
        }
    }
}
