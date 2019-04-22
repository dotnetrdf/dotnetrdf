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
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query;
using VDS.RDF.Query.Aggregates;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Ordering;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Available Query Syntaxes
    /// </summary>
    public enum SparqlQuerySyntax
    {
        /// <summary>
        /// Use SPARQL 1.0
        /// </summary>
        Sparql_1_0,
        /// <summary>
        /// Use SPARQL 1.1
        /// </summary>
        Sparql_1_1,
        /// <summary>
        /// Use the latest SPARQL specification supported by the library (currently SPARQL 1.1) with some extensions
        /// </summary>
        /// <remarks>
        /// <para>
        /// Extensions include the following:
        /// </para>
        /// <ul>
        /// <li><strong>LET</strong> assignments (we recommend using the SPARQL 1.1 standards BIND instead)</li>
        /// <li>Additional aggregates - <strong>NMAX</strong>, <strong>NMIN</strong>, <strong>MEDIAN</strong> and <strong>MODE</strong> (we recommend using the Leviathan Function Library URIs for these instead to make them usable in SPARQL 1.1 mode)</li>
        /// <li><strong>UNSAID</strong> alias for <strong>NOT EXISTS</strong> (we recommend using the SPARQL 1.1 standard NOT EXISTS instead</li>
        /// <li><strong>EXISTS</strong> and <strong>NOT EXISTS</strong> are permitted as Graph Patterns (only allowed in FILTERs in SPARQL 1.1)</li>
        /// </ul>
        /// </remarks>
        Extended
    }

    /// <summary>
    /// Class for parsing SPARQL Queries into <see cref="SparqlQuery">SparqlQuery</see> objects that can be used to query a Graph or Triple Store
    /// </summary>
    public class SparqlQueryParser
        : ITraceableTokeniser, IObjectParser<SparqlQuery>
    {
        private readonly TokenQueueMode _queuemode = TokenQueueMode.QueueAllBeforeParsing;
        private bool _tracetokeniser = false;
        private Uri _defaultBaseUri = null;
        private SparqlQuerySyntax _syntax = Options.QueryDefaultSyntax;
        private IEnumerable<ISparqlCustomExpressionFactory> _factories = Enumerable.Empty<ISparqlCustomExpressionFactory>();
        private IQueryOptimiser _optimiser = null;

        #region Constructors and Properties

        /// <summary>
        /// Creates a new instance of the SPARQL Query Parser
        /// </summary>
        public SparqlQueryParser()
            : this(TokenQueueMode.QueueAllBeforeParsing) { }

        /// <summary>
        /// Creates a new instance of the SPARQL Query Parser which supports the given SPARQL Syntax
        /// </summary>
        /// <param name="syntax">SPARQL Syntax</param>
        public SparqlQueryParser(SparqlQuerySyntax syntax)
            : this(TokenQueueMode.QueueAllBeforeParsing, syntax) { }

        /// <summary>
        /// Creates a new instance of the SPARQL Query Parser using the given Tokeniser Queue Mode
        /// </summary>
        /// <param name="queueMode">Token Queue Mode</param>
        public SparqlQueryParser(TokenQueueMode queueMode)
            : this(queueMode, Options.QueryDefaultSyntax) { }

        /// <summary>
        /// Creates a new instance of the SPARQL Query Parser using the given Tokeniser which supports the given SPARQL Syntax
        /// </summary>
        /// <param name="queueMode">Token Queue Mode</param>
        /// <param name="syntax">SPARQL Syntax</param>
        public SparqlQueryParser(TokenQueueMode queueMode, SparqlQuerySyntax syntax)
        {
            _queuemode = queueMode;
            _syntax = syntax;
        }

        /// <summary>
        /// Gets/Sets whether Tokeniser progress is Traced to the Console
        /// </summary>
        public bool TraceTokeniser
        {
            get
            {
                return _tracetokeniser;
            }
            set
            {
                _tracetokeniser = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Default Base URI for Queries parsed by this Parser instance
        /// </summary>
        public Uri DefaultBaseUri
        {
            get
            {
                return _defaultBaseUri;
            }
            set
            {
                _defaultBaseUri = value;
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
                else
                {
                    _factories = Enumerable.Empty<ISparqlCustomExpressionFactory>();
                }
            }
        }

        /// <summary>
        /// Gets/Sets the locally scoped Query Optimiser applied to queries at the end of the parsing process
        /// </summary>
        /// <remarks>
        /// <para>
        /// May be null if no locally scoped optimiser is set in which case the globally scoped optimiser will be used
        /// </para>
        /// </remarks>
        public IQueryOptimiser QueryOptimiser
        {
            get
            {
                return _optimiser;
            }
            set
            {
                _optimiser = value;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Helper Method which raises the Warning event when a non-fatal issue with the SPARQL Query being parsed is detected
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            SparqlWarning d = Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event raised when a non-fatal issue with the SPARQL Query being parsed is detected
        /// </summary>
        public event SparqlWarning Warning;

        #endregion

        #region Public Parser Methods

        /// <summary>
        /// Parses a SPARQL Query from a File
        /// </summary>
        /// <param name="queryFile">File containing the Query</param>
        /// <returns></returns>
        public SparqlQuery ParseFromFile(String queryFile)
        {
            if (queryFile == null) throw new RdfParseException("Cannot parse a SPARQL Query from a null File");
            StreamReader reader = new StreamReader(File.OpenRead(queryFile), Encoding.UTF8);
            return ParseInternal(reader);
        }

        /// <summary>
        /// Parses a SPARQL Query from an arbitrary Input Stream
        /// </summary>
        /// <param name="input">Input Stream</param>
        /// <returns></returns>
        public SparqlQuery Parse(StreamReader input)
        {
            if (input == null) throw new RdfParseException("Cannot parse a SPARQL Query from a null Stream");

            // Issue a Warning if the Encoding of the Stream is not UTF-8
            if (!input.CurrentEncoding.Equals(Encoding.UTF8))
            {
                RaiseWarning("Expected Input Stream to be encoded as UTF-8 but got a Stream encoded as " + input.CurrentEncoding.EncodingName + " - Please be aware that parsing errors may occur as a result");
            }

            return ParseInternal(input);
        }

        /// <summary>
        /// Parses a SPARQL Query from an arbitrary Input
        /// </summary>
        /// <param name="input">Input</param>
        /// <returns></returns>
        public SparqlQuery Parse(TextReader input)
        {
            if (input == null) throw new RdfParseException("Cannot parse a SPARQL Query from a null TextReader");
            return ParseInternal(input);
        }

        /// <summary>
        /// Parses a SPARQL Query from a String
        /// </summary>
        /// <param name="queryString">A SPARQL Query</param>
        /// <returns></returns>
        public SparqlQuery ParseFromString(String queryString)
        {
            if (queryString == null) throw new RdfParseException("Cannot parse a SPARQL Query from a null String");
            return ParseInternal(new StringReader(queryString));
        }

        /// <summary>
        /// Parses a SPARQL Query from a SPARQL Parameterized String
        /// </summary>
        /// <param name="queryString">A SPARQL Parameterized String</param>
        /// <returns></returns>
        /// <remarks>
        /// The <see cref="SparqlParameterizedString">SparqlParameterizedString</see> class allows you to use parameters in a String in a manner similar to SQL Commands in the ADO.Net model.  See the documentation for <see cref="SparqlParameterizedString">SparqlParameterizedString</see> for details of this.
        /// </remarks>
        public SparqlQuery ParseFromString(SparqlParameterizedString queryString)
        {
            if (queryString == null) throw new RdfParseException("Cannot parse a SPARQL Query from a null String");
            return ParseFromString(queryString.ToString());
        }

        #endregion

        #region Internal Parsing Logic

        private SparqlQuery ParseInternal(TextReader input)
        {
            try
            {
                // Create the Parser Context
                SparqlQueryParserContext context = new SparqlQueryParserContext(new SparqlTokeniser(input, _syntax), _queuemode, false, _tracetokeniser);
                context.SyntaxMode = _syntax;
                context.ExpressionParser.SyntaxMode = context.SyntaxMode;
                context.ExpressionFactories = _factories;

                return ParseInternal(context);
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
                    // No catch actions just trying to clean up the stream
                }
            }
        }

        private SparqlQuery ParseInternal(SparqlQueryParserContext context)
        {
            IToken temp = null;

            // Initialise Context with relevant data
            context.DefaultBaseUri = _defaultBaseUri;
            context.ExpressionParser.NamespaceMap = context.Query.NamespaceMap;
            context.ExpressionParser.QueryParser = this;
            context.ExpressionParser.ExpressionFactories = context.ExpressionFactories;
            context.Tokens.InitialiseBuffer();
            context.SyntaxMode = _syntax;
            context.Query.ExpressionFactories = context.ExpressionFactories.ToList();

            do
            {
                temp = context.Tokens.Dequeue();

                switch (temp.TokenType)
                {
                    case Token.BOF:
                    case Token.COMMENT:
                    case Token.EOF:
                        // Discardable Tokens
                        break;

                    case Token.BASEDIRECTIVE:
                        TryParseBaseDeclaration(context);
                        break;
                    case Token.PREFIXDIRECTIVE:
                        TryParsePrefixDeclaration(context);
                        break;

                    case Token.ASK:
                    case Token.CONSTRUCT:
                    case Token.DESCRIBE:
                    case Token.SELECT:
                        TryParseQueryVerb(context, temp);

                        // Get Variables for a Select or Describe or Construct
                        if (context.Query.QueryType == SparqlQueryType.Select)
                        {
                            TryParseSelectVariables(context);
                        }
                        else if (context.Query.QueryType == SparqlQueryType.Describe)
                        {
                            TryParseDescribeVariables(context);
                        }
                        else if (context.Query.QueryType == SparqlQueryType.Construct)
                        {
                            TryParseConstructTemplate(context);
                        }

                        // Get Datasets (FROM Clauses)
                        temp = context.Tokens.Peek();
                        while (temp.TokenType == Token.FROM)
                        {
                            TryParseFrom(context);
                            temp = context.Tokens.Peek();
                        }

                        // Unless a SHORT Form CONSTRUCT then we need to check for a WHERE { } clause
                        // If the Query is a DESCRIBE then if there is no WHERE keyword the WHERE clause is not required
                        if (context.Query.QueryType != SparqlQueryType.Construct || (context.Query.QueryType == SparqlQueryType.Construct && context.Query.RootGraphPattern == null))
                        {
                            // Check for Optional WHERE and Discard
                            temp = context.Tokens.Peek();
                            bool whereSeen = false;
                            if (temp.TokenType == Token.WHERE)
                            {
                                context.Tokens.Dequeue();
                                whereSeen = true;
                            }

                            // Unless it's a DESCRIBE we must now see a Graph Pattern 
                            // OR if the next Token is a { then it must be a Graph Pattern regardless
                            // OR we saw a WHERE in which case we must have a Graph Pattern
                            temp = context.Tokens.Peek();
                            if (whereSeen || context.Query.QueryType != SparqlQueryType.Describe || temp.TokenType == Token.LEFTCURLYBRACKET)
                            {
                                TryParseGraphPatterns(context);
                            }
                        }

                        // If we're an ASK then we shouldn't have any Solution Modifier for SPARQL 1.0
                        // SPARQL 1.1 allows modifiers so we don't continue if using SPARQL 1.1
                        if (context.Query.QueryType == SparqlQueryType.Ask && context.SyntaxMode == SparqlQuerySyntax.Sparql_1_0) continue;

                        // Otherwise we can now have a Solution Modifier

                        // Firstly a possible GROUP BY clause
                        temp = context.Tokens.Peek();
                        if (temp.TokenType == Token.GROUPBY)
                        {
                            context.Tokens.Dequeue();
                            TryParseGroupByClause(context);

                            // Then a possible HAVING clause
                            temp = context.Tokens.Peek();
                            if (temp.TokenType == Token.HAVING)
                            {
                                context.Tokens.Dequeue();
                                TryParseHavingClause(context);
                            }
                        }
                        else if (temp.TokenType == Token.HAVING)
                        {
                            // Can have a HAVING without a GROUP BY
                            context.Tokens.Dequeue();
                            TryParseHavingClause(context);
                        }

                        // Check that either there are no Aggregates used or only Aggregates used
                        if (SparqlSpecsHelper.IsSelectQuery(context.Query.QueryType) && (context.Query.IsAggregate && context.Query.GroupBy == null))
                        {
                            // CORE-446
                            // Cope with the case where aggregates are used inside other functions
                            foreach (SparqlVariable var in context.Query.Variables)
                            {
                                if (!var.IsResultVariable) continue;
                                if (var.IsAggregate) continue;

                                if (var.IsProjection)
                                {
                                    // Maybe OK if the projection operates over aggregates or group keys
                                    ISparqlExpression expr = var.Projection;
                                    Queue<ISparqlExpression> exprs = new Queue<ISparqlExpression>(expr.Arguments);
                                    while (exprs.Count > 0)
                                    {
                                        expr = exprs.Dequeue();

                                        // Aggregates are OK
                                        if (expr is AggregateTerm) continue;

                                        // Other primary expressions are OK
                                        if (expr is ConstantTerm) continue;
                                        if (expr is AllModifier) continue;
                                        if (expr is DistinctModifier) continue;

                                        // Variables may 
                                        if (expr is VariableTerm)
                                        {
                                            String exprVar = expr.Variables.First();
                                            if (!context.Query.Variables.Any(v => v.IsAggregate && v.Name.Equals(exprVar)))
                                            {
                                                throw new RdfParseException("The Select Query is invalid since it contains both Aggregates and Variables in the SELECT Clause but it does not contain a GROUP BY clause");
                                            }
                                        }

                                        // Anything else need to check its arguments
                                        foreach (ISparqlExpression arg in expr.Arguments)
                                        {
                                            exprs.Enqueue(arg);
                                        }
                                    }
                                }
                                else
                                {
                                    throw new RdfParseException("The Select Query is invalid since it contains both Aggregates and Variables in the SELECT Clause but it does not contain a GROUP BY clause");
                                }
                            }
                            
                        }

                        // Then a possible ORDER BY clause
                        temp = context.Tokens.Peek();
                        if (temp.TokenType == Token.ORDERBY)
                        {
                            context.Tokens.Dequeue();
                            TryParseOrderByClause(context);
                        }

                        // Then a possible LIMIT/OFFSET clause
                        temp = context.Tokens.Peek();
                        if (temp.TokenType == Token.LIMIT || temp.TokenType == Token.OFFSET)
                        {
                            TryParseLimitOffsetClause(context);
                        }

                        // Finally if we're a SELECT then we can have a BINDINGS clause
                        if (SparqlSpecsHelper.IsSelectQuery(context.Query.QueryType))
                        {
                            temp = context.Tokens.Peek();
                            if (temp.TokenType == Token.BINDINGS || temp.TokenType == Token.VALUES)
                            {
                                if (temp.TokenType == Token.BINDINGS && context.SyntaxMode != SparqlQuerySyntax.Extended) throw ParserHelper.Error("The BINDINGS keyword is the old name for the VALUES keyword, use the VALUES keyword instead", temp);
                                if (context.SyntaxMode == SparqlQuerySyntax.Sparql_1_0) throw ParserHelper.Error("Inline Data blocks (VALUES clauses) are not permitted in SPARQL 1.0", temp);
                                context.Tokens.Dequeue();
                                context.Query.Bindings = TryParseInlineData(context);
                            }
                        }

                        break;

                    default:
                        if (context.Query.QueryType == SparqlQueryType.Ask)
                        {
                            throw ParserHelper.Error("Unexpected Token encountered, a valid ASK query has been parsed but additional invalid tokens are present after the Graph pattern", temp);
                        }
                        else
                        {
                            throw ParserHelper.Error("Unexpected Token encountered - expected a BASE/PREFIX directive or a Query Keyword to start a Query", temp);
                        }
                }
            } while (temp.TokenType != Token.EOF);

            // If not SPARQL 1.0 then do additional post parsing checks
            if (_syntax != SparqlQuerySyntax.Sparql_1_0)
            {
                switch (context.Query.QueryType)
                {
                    case SparqlQueryType.Select:
                    case SparqlQueryType.SelectDistinct:
                    case SparqlQueryType.SelectReduced:
                    case SparqlQueryType.Describe:
                        // Check Variable Usage
                        List<String> projectedSoFar = new List<string>();
                        List<String> mainBodyVars = (context.Query.RootGraphPattern != null ? context.Query.RootGraphPattern.Variables : Enumerable.Empty<String>()).Distinct().ToList();
                        foreach (SparqlVariable var in context.Query.Variables)
                        {
                            if (!var.IsResultVariable) continue;

                            if (projectedSoFar.Contains(var.Name) && (var.IsAggregate || var.IsProjection))
                            {
                                throw new RdfParseException("Cannot assign the results of an Aggregate/Project Expression to the variable " + var.ToString() + " as this variable is already Projected to earlier in the SELECT");
                            }

                            if (var.IsProjection)
                            {
                                if (mainBodyVars.Contains(var.Name)) throw new RdfParseException("Cannot project an expression to the variable " + var.Name + " as this variable is a bound variable from the main body of the query");
                                if (context.Query.GroupBy != null)
                                {
                                    if (!IsProjectableExpression(context, var.Projection, projectedSoFar))
                                    {
                                        throw new RdfParseException("Your SELECT uses the Project Expression " + var.Projection.ToString() + " which uses one/more variables which are either not projectable from the GROUP BY or not projected earlier in the SELECT.  All Variables used must be projectable from the GROUP BY, projected earlier in the SELECT or within an aggregate");
                                    }
                                }
                            }
                            else if (var.IsAggregate)
                            {
                                if (mainBodyVars.Contains(var.Name)) throw new RdfParseException("Cannot project an aggregate to the variable " + var.Name + " as this variable is a bound variable from the main body of the query");
                                if (context.Query.GroupBy != null)
                                {
                                    // Q: Does ISparqlAggregate needs to expose a Variables property?
                                    // if (!var.Aggregate.Var
                                }
                            }
                            else
                            {
                                if (context.Query.GroupBy != null)
                                {
                                    // If there is a GROUP BY then the Variable must either be projectable from there
                                    if (!context.Query.GroupBy.ProjectableVariables.Contains(var.Name))
                                    {
                                        throw new RdfParseException("Your SELECT/DESCRIBE query tries to project the variable " + var.ToString() + " but this Variable is not Grouped By");
                                    }
                                }
                            }

                            projectedSoFar.Add(var.Name);
                        }
                        break;

                    case SparqlQueryType.DescribeAll:
                    case SparqlQueryType.SelectAll:
                    case SparqlQueryType.SelectAllDistinct:
                    case SparqlQueryType.SelectAllReduced:
                        // Check that a GROUP BY has not been used
                        if (context.Query.GroupBy != null)
                        {
                            throw new RdfParseException("SELECT/DESCRIBE * is not permitted when a GROUP BY is used");
                        }
                        break;
                }
            }

            // Optimise the Query if the global option is enabled
            if (Options.QueryOptimisation)
            {
                // If a locally scoped optimiser is available use that
                if (_optimiser != null)
                {
                    context.Query.Optimise(_optimiser);
                }
                else
                {
                    context.Query.Optimise();
                }
            }

            return context.Query;
        }

        private void TryParseBaseDeclaration(SparqlQueryParserContext context)
        {
            if (context.SubQueryMode) throw new RdfQueryException("BASE Directives are not supported in Sub-queries");

            // Get the next Token which should be a Uri Token
            IToken next = context.Tokens.Dequeue();
            if (next.TokenType == Token.URI)
            {
                context.Query.BaseUri = UriFactory.Create(next.Value);
                context.ExpressionParser.BaseUri = context.Query.BaseUri;
            }
            else
            {
                throw ParserHelper.Error("Expected a URI Token to follow the BASE Verb in a Query", next);
            }
        }

        private void TryParsePrefixDeclaration(SparqlQueryParserContext context)
        {
            if (context.SubQueryMode) throw new RdfQueryException("PREFIX Directives are not supported in Sub-queries");

            // Get the next Two Tokens which should be a Prefix and a Uri
            IToken prefix = context.Tokens.Dequeue();
            IToken uri = context.Tokens.Dequeue();

            if (prefix.TokenType == Token.PREFIX)
            {
                if (uri.TokenType == Token.URI)
                {
                    String baseUri = (context.Query.BaseUri != null) ? context.Query.BaseUri.AbsoluteUri : String.Empty;
                    Uri u = UriFactory.Create(Tools.ResolveUri(uri.Value, baseUri));
                    if (prefix.Value.Length == 1)
                    {
                        // Defining prefix for Default Namespace
                        context.Query.NamespaceMap.AddNamespace("", u);
                    }
                    else
                    {
                        // Defining prefix for some other Namespace
                        context.Query.NamespaceMap.AddNamespace(prefix.Value.Substring(0, prefix.Value.Length - 1), u);
                    }
                }
                else
                {
                    throw ParserHelper.Error("Expected a URI Token to follow a Prefix Token to follow the PREFIX Verb in a Query", uri);
                }
            }
            else
            {
                throw ParserHelper.Error("Expected a Prefix Token to follow the PREFIX Verb in a Query", prefix);
            }
        }

        private void TryParseQueryVerb(SparqlQueryParserContext context, IToken t)
        {
            if (context.VerbSeen)
            {
                throw ParserHelper.Error("Only 1 Query Verb can occur in a Query", t);
            }
            else
            {
                context.VerbSeen = true;

                switch (t.TokenType) {
                    case Token.ASK:
                        if (context.SubQueryMode) throw ParserHelper.Error("ASK is not supported in Sub-queries",t);
                        context.Query.QueryType = SparqlQueryType.Ask;
                        break;
                    case Token.CONSTRUCT:
                        if (context.SubQueryMode) throw ParserHelper.Error("CONSTRUCT is not supported in Sub-queries",t);
                        context.Query.QueryType = SparqlQueryType.Construct;
                        break;
                    case Token.DESCRIBE:
                        if (context.SubQueryMode) throw ParserHelper.Error("DESCRIBE is not supported in Sub-queries", t);
                        context.Query.QueryType = SparqlQueryType.Describe;
                        break;
                    case Token.SELECT:
                        context.Query.QueryType = SparqlQueryType.Select;
                        break;
                }
            }
        }

        private void TryParseSelectVariables(SparqlQueryParserContext context)
        {
            IToken next; 
            bool firstToken = true;
            ISparqlExpression expr;

            // Any Expression we parse from the Select Variables segment may be an aggregate
            context.ExpressionParser.AllowAggregates = true;

            do {
                next = context.Tokens.Peek();

                switch (next.TokenType)
                {
                    case Token.ALL:
                        if (context.Query.Variables.Count() > 0)
                        {
                            throw ParserHelper.Error("Can't use the * symbol to specify Select All and specify Variables in the SELECT Clause", next);
                        }
                        // Change the Query Type to a Select All
                        switch (context.Query.QueryType)
                        {
                            case SparqlQueryType.Select:
                                context.Query.QueryType = SparqlQueryType.SelectAll;
                                break;
                            case SparqlQueryType.SelectDistinct:
                                context.Query.QueryType = SparqlQueryType.SelectAllDistinct;
                                break;
                            case SparqlQueryType.SelectReduced:
                                context.Query.QueryType = SparqlQueryType.SelectAllReduced;
                                break;
                        }
                        break;

                    case Token.VARIABLE:
                        if ((int)context.Query.QueryType >= (int)SparqlQueryType.SelectAll)
                        {
                            throw ParserHelper.Error("Can't use the * symbol to specify Select All and specify Variables in the SELECT Clause", next);
                        }

                        context.Query.AddVariable(next.Value, true);
                        break;

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
                        if ((int)context.Query.QueryType >= (int)SparqlQueryType.SelectAll)
                        {
                            throw ParserHelper.Error("Can't use the * symbol to specify Select All and specify an Aggregate in the SELECT Clause", next);
                        }

                        context.Tokens.Dequeue();
                        SparqlVariable aggVar = TryParseAggregate(context, next);
                        context.Query.AddVariable(aggVar);
                        firstToken = false;
                        continue;

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
                        if (context.SyntaxMode == SparqlQuerySyntax.Sparql_1_0) throw new RdfParseException("Project Expressions are not supported in SPARQL 1.0");

                        expr = TryParseFunctionExpression(context);

                        // Need to see the Alias
                        next = context.Tokens.Dequeue();
                        if (next.TokenType != Token.AS)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected an AS Keyword after a Projection Expression", next);
                        }
                        next = context.Tokens.Dequeue();
                        if (next.TokenType != Token.VARIABLE)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Variable as an alias after an AS Keyword", next);
                        }

                        context.Query.AddVariable(new SparqlVariable(next.Value.Substring(1), expr));
                        firstToken = false;
                        continue;

                    case Token.QNAME:
                    case Token.URI:
                        if (context.SyntaxMode == SparqlQuerySyntax.Sparql_1_0) throw new RdfParseException("Project Expressions are not supported in SPARQL 1.0");

                        // Try and parse a Project Expression which is a naked function call
                        // Resolve the URI
                        Uri u = UriFactory.Create(Tools.ResolveUriOrQName(next, context.Query.NamespaceMap, context.Query.BaseUri));
                        context.Tokens.Dequeue();

                        // Ensure we then see a Open Bracket
                        if (context.Tokens.Peek().TokenType != Token.LEFTBRACKET)
                        {
                            throw ParserHelper.Error("Expected a Left Bracket after a URI/QName in Select Variables for the arguments of a function call", context.Tokens.Peek());
                        }
                        context.Tokens.Dequeue();

                        // Then get the arguments (if any)
                        List<ISparqlExpression> args = new List<ISparqlExpression>();
                        if (context.Tokens.Peek().TokenType == Token.RIGHTBRACKET)
                        {
                            context.Tokens.Dequeue();
                        }
                        else
                        {
                            bool comma = false;
                            do
                            {
                                args.Add(TryParseExpression(context, true));
                                comma = (context.Tokens.LastTokenType == Token.COMMA || context.Tokens.LastTokenType == Token.DISTINCT);

                            } while (comma);
                        }

                        // If there are no arguments (one null argument) then discard
                        if (args.Count == 1 && args.First() == null) args.Clear();

                        // Then try and create an Expression
                        expr = SparqlExpressionFactory.CreateExpression(u, args, _factories);

                        // Need to see the Alias
                        next = context.Tokens.Dequeue();
                        if (next.TokenType != Token.AS)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected an AS Keyword after a Projection Expression", next);
                        }
                        next = context.Tokens.Dequeue();
                        if (next.TokenType != Token.VARIABLE)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Variable as an alias after an AS Keyword", next);
                        }

                        // Turn into the appropriate type of Variable
                        if (expr is AggregateTerm)
                        {
                            context.Query.AddVariable(new SparqlVariable(next.Value.Substring(1), ((AggregateTerm)expr).Aggregate));
                        }
                        else
                        {
                            context.Query.AddVariable(new SparqlVariable(next.Value.Substring(1), expr));
                        }
                        
                        firstToken = false;
                        continue;

                    case Token.LEFTBRACKET:
                        if ((int)context.Query.QueryType >= (int)SparqlQueryType.SelectAll)
                        {
                            throw ParserHelper.Error("Can't use the * symbol to specify Select All and specify a Projection Expression in the SELECT Clause", next);
                        }

                        if (context.SyntaxMode == SparqlQuerySyntax.Sparql_1_0) throw new RdfParseException("Project Expressions are not supported in SPARQL 1.0");

                        // Parse the Expression
                        context.Tokens.Dequeue();
                        expr = TryParseExpression(context, false, true);

                        // Require an alias for a Projection Expression
                        bool asTerminated = (context.Tokens.LastTokenType == Token.AS);
                        if (!asTerminated)
                        {
                            // Still need to see an AS
                            next = context.Tokens.Dequeue();
                            if (next.TokenType != Token.AS)
                            {
                                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected an AS Keyword after a Projection Expression", next);
                            }
                        }
                        next = context.Tokens.Dequeue();
                        if (next.TokenType != Token.VARIABLE)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Variable as an alias after an AS Keyword", next);
                        }

                        // Turn into the appropriate type of Variable
                        if (expr is AggregateTerm)
                        {
                            context.Query.AddVariable(new SparqlVariable(next.Value.Substring(1), ((AggregateTerm)expr).Aggregate));
                        }
                        else
                        {
                            context.Query.AddVariable(new SparqlVariable(next.Value.Substring(1), expr));
                        }

                        firstToken = false;
                        if (asTerminated)
                        {
                            // Still need a Right Bracket to terminate the expression since the alias was within the outer brackets
                            next = context.Tokens.Dequeue();
                            if (next.TokenType != Token.RIGHTBRACKET)
                            {
                                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Right Bracket to terminate the Projection Expression after the alias", next);
                            }
                        }
                        continue;

                    case Token.DISTINCT:
                        if (firstToken)
                        {
                            context.Query.QueryType = SparqlQueryType.SelectDistinct;
                        }
                        else
                        {
                            throw ParserHelper.Error("The DISTINCT Keyword must occur immediately after the SELECT Verb in a Query", next);
                        }
                        break;

                    case Token.REDUCED:
                        if (firstToken)
                        {
                            context.Query.QueryType = SparqlQueryType.SelectReduced;
                        }
                        else
                        {
                            throw ParserHelper.Error("The REDUCED Keyword must occur immediately after the SELECT Verb in a Query", next);
                        }
                        break;

                    case Token.COMMENT:
                        // Discard Comments
                        context.Tokens.Dequeue();
                        continue;

                    default:
                        if (firstToken)
                        {
                            throw ParserHelper.Error("The SELECT Keyword must be followed by a list of one/more variables or a * to specify all variables", next);
                        }
                        context.ExpressionParser.AllowAggregates = false;
                        return;
                }

                context.Tokens.Dequeue();
                firstToken = false;
            } while (true);
        }

        private SparqlVariable TryParseAggregate(SparqlQueryParserContext context, IToken agg)
        {
            if (context.SyntaxMode == SparqlQuerySyntax.Sparql_1_0) throw ParserHelper.Error("Aggregates are not supported in SPARQL 1.0", agg);

            IToken next;
            SparqlVariable var;
            ISparqlAggregate aggregate;

            // Check that the Token is an Aggregate Keyword Token
            switch (agg.TokenType)
            {
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
                    // OK
                    break;

                default:
                    throw ParserHelper.Error("Cannot parse an Aggregate since '" + agg.GetType().ToString() + "' is not an Aggregate Keyword Token", agg);
            }

            // Gather up the Tokens and call into the Expression Parser to get this parsed
            Queue<IToken> tokens = new Queue<IToken>();
            tokens.Enqueue(agg);
            int openBrackets = 0;
            do
            {
                next = context.Tokens.Dequeue();
                if (next.TokenType == Token.LEFTBRACKET)
                {
                    openBrackets++;
                }
                else if (next.TokenType == Token.RIGHTBRACKET)
                {
                    openBrackets--;
                }

                tokens.Enqueue(next);
            } while (openBrackets > 0);

            context.ExpressionParser.AllowAggregates = true;
            ISparqlExpression aggExpr = context.ExpressionParser.Parse(tokens);
            context.ExpressionParser.AllowAggregates = false;

            if (aggExpr is AggregateTerm)
            {
                aggregate = ((AggregateTerm)aggExpr).Aggregate;
            }
            else
            {
                throw new RdfParseException("Unexpected expression was parsed when an Aggregate was expected: " + aggExpr.ToString());
            }

            // See if there is an alias
            String alias = "Result";
            next = context.Tokens.Peek();
            if (next.TokenType == Token.AS)
            {
                context.Tokens.Dequeue();
                next = context.Tokens.Dequeue();
                if (next.TokenType != Token.VARIABLE)
                {
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "', expected a Variable Token after an AS Keyword to act as an aliased name for the Aggregate", next);
                }
                alias = next.Value.Substring(1);
            }
            else
            {
                if (context.SyntaxMode != SparqlQuerySyntax.Extended) throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected an AS keyword after an Aggregate", next);

                int nextID = context.NextAliasID;
                if (nextID > 0) alias += nextID.ToString();
                while (context.Query.Variables.Any(v => v.Name.Equals(alias)))
                {
                    alias = "Result" + context.NextAliasID;
                }
                RaiseWarning("No AS ?variable given for the Aggregate " + aggregate.ToString() + " so assigning alias '" + alias + "'");
            }


            var = new SparqlVariable(alias, aggregate);

            return var;
        }

        private void TryParseDescribeVariables(SparqlQueryParserContext context)
        {
            if (context.SubQueryMode) throw new RdfQueryException("DESCRIBE not permitted as a sub-query");

            IToken next;
            bool firstToken = true;

            do {
                next = context.Tokens.Peek();

                switch (next.TokenType)
                {
                    case Token.ALL:
                        if (context.Query.DescribeVariables.Count() > 0)
                        {
                            throw ParserHelper.Error("Can't use the * symbol to specify Describe All and specify Variables/URIs/QNames in the DESCRIBE Clause", next);
                        }
                        // Change the Query Type to a Describe All
                        context.Query.QueryType = SparqlQueryType.DescribeAll;
                        break;

                    case Token.VARIABLE:
                        if (context.Query.QueryType == SparqlQueryType.DescribeAll)
                        {
                            throw ParserHelper.Error("Can't use the * symbol to specify Describe All and specify Variables/URIs/QNames in the DESCRIBE Clause", next);
                        }
                        context.Query.AddVariable(next.Value, true);
                        context.Query.AddDescribeVariable(next);
                        break;

                    case Token.QNAME:
                    case Token.URI:
                        context.Query.AddDescribeVariable(next);
                        break;

                    case Token.COMMENT:
                        // Discard Comments
                        context.Tokens.Dequeue();
                        continue;

                    default:
                        if (firstToken)
                        {
                            throw ParserHelper.Error("The DESCRIBE keyword must be followed by a list of one/more variables/IRI References or a * to specify all variables", next);
                        }
                        return;
                }

                context.Tokens.Dequeue();
                firstToken = false;
            } while (true);
        }

        private void TryParseConstructTemplate(SparqlQueryParserContext context)
        {
            if (context.SubQueryMode) throw new RdfQueryException("CONSTRUCT not permitted as a sub-query");

            bool shortForm = (context.Tokens.Peek().TokenType == Token.WHERE || context.Tokens.Peek().TokenType == Token.FROM);
            if (shortForm && context.SyntaxMode == SparqlQuerySyntax.Sparql_1_0)
            {
                throw ParserHelper.Error("Short Form CONSTRUCT queries are not permitted in SPARQL 1.0", context.Tokens.Peek());
            }
            else if (shortForm)
            {
                IToken temp = context.Tokens.Peek();
                if (context.Tokens.Peek().TokenType == Token.FROM)
                {
                    while (temp.TokenType == Token.FROM)
                    {
                        TryParseFrom(context);
                        temp = context.Tokens.Peek();
                    }
                }
                if (temp.TokenType == Token.WHERE)
                {
                    // For Short Form CONSTRUCT discard the WHERE
                    context.Tokens.Dequeue();
                }
                else
                {
                    throw ParserHelper.Error("Unexpected Token '" + temp.GetType().Name + " encountered, expected the WHERE of a Short Form Construct to come after the FROM/FROM NAMED clauses of a Short Form Construct", temp);
                }
            }

            // Discard the opening {
            IToken next = context.Tokens.Dequeue();
            if (next.TokenType != Token.LEFTCURLYBRACKET)
            {
                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Left Curly Bracket to start a CONSTRUCT Template", next);
            }

            // Use a Graph Pattern for the Construct Template
            GraphPattern constructTemplate = TryParseGraphPattern(context, false);

            // Check it doesn't contain anything other than Triple Patterns
            if (constructTemplate.IsFiltered)
            {
                throw new RdfParseException("A FILTER Clause cannot occur in a CONSTRUCT Template");
            }
            else if (constructTemplate.IsGraph)
            {
                throw new RdfParseException("A GRAPH Clause cannot occur in a CONSTRUCT Template");
            }
            else if (constructTemplate.IsOptional || constructTemplate.IsMinus || constructTemplate.IsExists || constructTemplate.IsNotExists  || constructTemplate.IsService || constructTemplate.IsSubQuery)
            {
                throw new RdfParseException("Graph Clauses (e.g. OPTIONAL, MINUS etc.) cannot occur in a CONSTRUCT Template");
            }
            else if (constructTemplate.IsUnion)
            {
                throw new RdfParseException("A UNION Clause cannot occur in a CONSTRUCT Template");
            }
            else if (constructTemplate.HasChildGraphPatterns)
            {
                throw new RdfParseException("Nested Graph Patterns cannot occur in a CONSTRUCT Template");
            }
            else if (!constructTemplate.TriplePatterns.All(p => p is IConstructTriplePattern))
            {
                throw new RdfParseException("A Construct Template may only be composed of Triple Patterns - Assignments, Property Paths, Sub-queries etc. are not permitted");
            }
            else if (constructTemplate.UnplacedAssignments.Any())
            {
                throw new RdfParseException("A Construct Template may not contain any Assignments");
            }
            else
            {
                // OK
                context.Query.ConstructTemplate = constructTemplate;
                if (shortForm) context.Query.RootGraphPattern = constructTemplate;
            }
        }

        private void TryParseFrom(SparqlQueryParserContext context)
        {
            if (context.SubQueryMode) throw new RdfQueryException("Dataset Descriptions are not permitted in Sub-queries");

            IToken next = context.Tokens.Dequeue();

            // Should be a FROM
            if (next.TokenType == Token.FROM)
            {
                // Default Graph/Named Graph Specified

                next = context.Tokens.Peek();
                if (next.TokenType == Token.URI)
                {
                    // Default Graph Specified
                    String baseUri = (context.Query.BaseUri == null) ? String.Empty : context.Query.BaseUri.AbsoluteUri;
                    context.Query.AddDefaultGraph(UriFactory.Create(Tools.ResolveUri(next.Value, baseUri)));
                    context.Tokens.Dequeue();
                }
                else if (next.TokenType == Token.QNAME)
                {
                    // Default Graph Specified
                    context.Query.AddDefaultGraph(ResolveQName(context, next.Value));
                    context.Tokens.Dequeue();
                }
                else if (next.TokenType == Token.NAMED)
                {
                    // Named Graph Specified
                    context.Tokens.Dequeue();
                    next = context.Tokens.Peek();
                    if (next.TokenType == Token.URI) 
                    {
                        String baseUri = (context.Query.BaseUri == null) ? String.Empty : context.Query.BaseUri.AbsoluteUri;
                        context.Query.AddNamedGraph(UriFactory.Create(Tools.ResolveUri(next.Value, baseUri)));
                        context.Tokens.Dequeue();
                    }
                    else if (next.TokenType == Token.QNAME)
                    {
                        context.Query.AddNamedGraph(ResolveQName(context, next.Value));
                        context.Tokens.Dequeue();
                    }
                    else
                    {
                        throw ParserHelper.Error("Expected a QName/URI Token to occur after a FROM NAMED Keyword to specify a Named Graph URI", next);
                    }
                }
                else
                {
                    throw ParserHelper.Error("Expected a QName/URI Token to occur after a FROM Keyword to specify a Default Graph URI", next);
                }
            }
        }

        private void TryParseGraphPatterns(SparqlQueryParserContext context)
        {
            // Parse a Graph Pattern Object
            context.Query.RootGraphPattern = TryParseGraphPattern(context);
        }

        /// <summary>
        /// Tries to parse a Graph Pattern from the given Parser Context
        /// </summary>
        /// <param name="context">Parser Context</param>
        /// <param name="requireOpeningLeftBracket">Whether the opening Left Curly Bracket is required</param>
        /// <returns></returns>
        protected internal GraphPattern TryParseGraphPattern(SparqlQueryParserContext context, bool requireOpeningLeftBracket)
        {
            context.GraphPatternID++;
            IToken next;

            if (requireOpeningLeftBracket)
            {
                // Discard the opening {
                next = context.Tokens.Dequeue();
                if (next.TokenType != Token.LEFTCURLYBRACKET)
                {
                    throw ParserHelper.Error("Unexpected Token encountered, expected the start of a Graph Pattern", next);
                }
            }

            next = context.Tokens.Peek();

            if (next.TokenType == Token.RIGHTCURLYBRACKET)
            {
                // Empty Graph Pattern - Selects nothing
                context.Tokens.Dequeue();
                GraphPattern pattern = new GraphPattern();
                return pattern;
            }
            else if (next.TokenType == Token.LEFTCURLYBRACKET)
            {
                // Nested Graph Pattern
                GraphPattern pattern = new GraphPattern();
                GraphPattern child;// = new GraphPattern();

                child = TryParseGraphPattern(context, true);
                // this.TryParseTriplePatterns(context, child);
                pattern.AddGraphPattern(child);
                child = new GraphPattern();
                IToken lastToken = null;

                // Keep Parsing Graph Patterns until we hit a Right Curly Bracket
                do
                {
                    next = context.Tokens.Peek();
                    if (next.TokenType == Token.RIGHTCURLYBRACKET)
                    {
                        // This isn't in the switch as we want to break out of the loop when we get here
                        context.Tokens.Dequeue();
                        break;
                    }
                    else
                    {
                        switch (next.TokenType)
                        {
                            case Token.UNION:
                                // UNION Clause
                                if (!child.IsEmpty)
                                {
                                    pattern.AddGraphPattern(child);
                                    child = new GraphPattern();
                                }
                                lastToken = context.Tokens.Dequeue();
                                TryParseUnionClause(context, pattern);
                                break;

                            case Token.FILTER:
                                // FILTER Clause
                                lastToken = context.Tokens.Dequeue();
                                TryParseFilterClause(context, pattern);
                                break;

                            case Token.BIND:
                                // BIND Clause
                                lastToken = context.Tokens.Dequeue();
                                TryParseBindAssignment(context, pattern);
                                break;

                            case Token.OPTIONAL:
                                // OPTIONAL Clause
                                if (!child.IsEmpty)
                                {
                                    pattern.AddGraphPattern(child);
                                    child = new GraphPattern();
                                }
                                lastToken = context.Tokens.Dequeue();
                                TryParseOptionalClause(context, pattern);
                                break;

                            case Token.GRAPH:
                                // GRAPH Clause
                                if (!child.IsEmpty)
                                {
                                    pattern.AddGraphPattern(child);
                                    child = new GraphPattern();
                                }
                                lastToken = context.Tokens.Dequeue();
                                TryParseGraphClause(context, pattern);
                                break;

                            case Token.EXISTS:
                            case Token.NOTEXISTS:
                            case Token.UNSAID:
                                // EXISTS/NOT EXISTS/UNSAID Clause
                                if (next.TokenType == Token.UNSAID && context.SyntaxMode != SparqlQuerySyntax.Extended) throw new RdfParseException("The UNSAID Keyword is only supported when syntax is set to Extended.  It is an alias for NOT EXISTS which can be used when the syntax is set to SPARQL 1.1/Extended");
                                if (!child.IsEmpty)
                                {
                                    pattern.AddGraphPattern(child);
                                    child = new GraphPattern();
                                }
                                lastToken = context.Tokens.Dequeue();
                                TryParseExistsClause(context, pattern, (next.TokenType == Token.EXISTS));
                                break;

                            case Token.MINUS_P:
                                // MINUS Clause
                                if (!child.IsEmpty)
                                {
                                    pattern.AddGraphPattern(child);
                                    child = new GraphPattern();
                                }
                                lastToken = context.Tokens.Dequeue();
                                TryParseMinusClause(context, pattern);
                                break;

                            case Token.SERVICE:
                                // SERVICE clause
                                if (!child.IsEmpty)
                                {
                                    pattern.AddGraphPattern(child);
                                    child = new GraphPattern();
                                }
                                lastToken = context.Tokens.Dequeue();
                                TryParseServiceClause(context, pattern);
                                break;

                            case Token.VALUES:
                                // VALUES clause
                                if (context.SyntaxMode == SparqlQuerySyntax.Sparql_1_0) throw ParserHelper.Error("Inline Data blocks (VALUES clauses) are not permitted in SPARQL 1.0", next);
                                if (!child.IsEmpty)
                                {
                                    pattern.AddGraphPattern(child);
                                    child = new GraphPattern();
                                }
                                lastToken = context.Tokens.Dequeue();
                                pattern.AddInlineData(TryParseInlineData(context));
                                break;

                            case Token.DOT:
                                // Allowed after non-triple patterns
                                if (lastToken == null)
                                {
                                    throw ParserHelper.Error("Unexpected DOT in graph pattern.", next);
                                }
                                context.Tokens.Dequeue();
                                lastToken = null;
                                break;

                            case Token.VARIABLE:
                            case Token.URI:
                            case Token.QNAME:
                            case Token.LITERAL:
                            case Token.LONGLITERAL:
                            case Token.PLAINLITERAL:
                            case Token.BLANKNODE:
                            case Token.BLANKNODEWITHID:
                            case Token.LET:
                            case Token.LEFTSQBRACKET:
                            case Token.LEFTBRACKET:
                                // Start of some Triple Patterns
                                context.GraphPatternID++;
                                TryParseTriplePatterns(context, child);
                                lastToken = null;
                                break;

                            default:
                                // Otherwise we'll expect a new Graph Pattern
                                pattern.AddGraphPattern(TryParseGraphPattern(context, true));
                                break;
                        }
                    }
                } while (true);

                if (!child.IsEmpty)
                {
                    pattern.AddGraphPattern(child);
                }
                return pattern;
            }
            else
            {
                // Non-Empty Graph Pattern
                GraphPattern pattern = new GraphPattern();
                TryParseTriplePatterns(context, pattern);

                // Keep parsing Triple Patterns until we hit a Right Curly Bracket
                do
                {
                    next = context.Tokens.Peek();
                    if (next.TokenType == Token.RIGHTCURLYBRACKET)
                    {
                        break;
                    }
                    else
                    {
                        TryParseTriplePatterns(context, pattern);
                    }
                } while (true);

                // Discard the Right Curly Bracket
                context.Tokens.Dequeue();
                return pattern;
            }
        }

        private GraphPattern TryParseGraphPattern(SparqlQueryParserContext context)
        {
            return TryParseGraphPattern(context, true);
        }

        private void TryParseTriplePatterns(SparqlQueryParserContext context, GraphPattern p)
        {
            int lasttoken = context.Tokens.LastTokenType;
            IToken next = context.Tokens.Dequeue();

            // Allowed a Variable/RDF Term/Collection
            // OR we might go straight to a OPTIONAL/GRAPH/UNION/FILTER/EXISTS/NOT EXISTS/LET

            switch (next.TokenType)
            {
                case Token.COMMENT:
                    // Comments are discardable
                    TryParseTriplePatterns(context, p);
                    break;

                case Token.VARIABLE:
                    // Variable
                    context.LocalTokens.Push(next);
                    context.Query.AddVariable(next.Value);
                    TryParsePredicateObjectList(context, p,2);
                    break;

                case Token.URI:
                case Token.QNAME:
                case Token.LITERAL:
                case Token.LONGLITERAL:
                case Token.PLAINLITERAL:
                    // Must then be followed be a non-empty Property List
                    context.LocalTokens.Push(next);
                    TryParsePredicateObjectList(context, p,2);
                    break;

                case Token.BLANKNODE:
                case Token.BLANKNODEWITHID:
                    // Check list of Blank Node usages
                    if (context.BlankNodeIDUsages.ContainsKey(next.Value))
                    {
                        if (context.CheckBlankNodeScope && context.BlankNodeIDUsages[next.Value] != context.GraphPatternID)
                        {
                            throw ParserHelper.Error("Invalid use of Blank Node Label '" + next.Value + "', this Label has already been used in a different Graph Pattern", next);
                        }
                    }
                    else
                    {
                        context.BlankNodeIDUsages.Add(next.Value, context.GraphPatternID);
                    }

                    // Must then be followed be a non-empty Property List
                    context.LocalTokens.Push(next);
                    TryParsePredicateObjectList(context, p, 2);
                    break;

                case Token.LET:
                    // LET assignment
                    TryParseLetAssignment(context, p);
                    break;

                case Token.BIND:
                    // BIND assignment
                    TryParseBindAssignment(context, p);
                    break;

                case Token.LEFTSQBRACKET:
                    // Start of Blank Node Collection
                    // Create a new Blank Node Token
                    BlankNodeWithIDToken bnode = new BlankNodeWithIDToken(context.GetNewBlankNodeID(), 0, 0, 0);

                    // Push twice, once for Subject of Collection
                    context.LocalTokens.Push(bnode);

                    next = context.Tokens.Peek();
                    if (next.TokenType == Token.RIGHTSQBRACKET)
                    {
                        // Single anonymous blank node
                        context.Tokens.Dequeue();

                        // Parse as Subject of Triples
                        TryParsePredicateObjectList(context, p, 2);
                    }
                    else
                    {

                        // Parse the Collection
                        TryParsePredicateObjectList(context, p, 2);

                        // Push again for subject of Triples
                        context.LocalTokens.Push(bnode);
                        TryParsePredicateObjectList(context, p, 2);
                    }
                    break;

                case Token.LEFTBRACKET:
                    // Collection
                    TryParseCollection(context, p, false);
                    TryParsePredicateObjectList(context, p, 2);
                    break;

                case Token.FILTER:
                    // FILTER Pattern
                    TryParseFilterClause(context, p);
                    break;

                case Token.OPTIONAL:
                    // OPTIONAL Clause
                    TryParseOptionalClause(context, p);
                    break;

                case Token.EXISTS:
                case Token.NOTEXISTS:
                case Token.UNSAID:
                    // EXISTS/NOT EXISTS clause
                    if (next.TokenType == Token.UNSAID && context.SyntaxMode != SparqlQuerySyntax.Extended) throw new RdfParseException("The UNSAID Keyword is only supported when syntax is set to Extended.  It is an alias for NOT EXISTS which can be used when the syntax is set to SPARQL 1.1/Extended");
                    TryParseExistsClause(context, p, (next.TokenType == Token.EXISTS));
                    break;

                case Token.MINUS_P:
                    // MINUS clause
                    TryParseMinusClause(context, p);
                    break;

                case Token.SERVICE:
                    // SERVICE clause
                    TryParseServiceClause(context, p);
                    break;

                case Token.SELECT:
                    // Sub-query
                    TryParseSubquery(context, p);
                    break;

                case Token.GRAPH:
                    // GRAPH Clause
                    TryParseGraphClause(context, p);
                    break;

                case Token.UNION:
                    // UNION Clause
                    TryParseUnionClause(context, p);
                    break;

                case Token.VALUES:
                    // VALUES Clause
                    if (context.SyntaxMode == SparqlQuerySyntax.Sparql_1_0) throw ParserHelper.Error("Inline Data blocks (VALUES clauses) are not permitted in SPARQL 1.0", next);
                    p.AddInlineData(TryParseInlineData(context));
                    break;

                case Token.LEFTCURLYBRACKET:
                    // Nested Graph Pattern
                    p.AddGraphPattern(TryParseGraphPattern(context, false));

                    // Simplify Subqueries
                    if (p.ChildGraphPatterns.Last().IsSubQuery)
                    {
                        GraphPattern temp = p.LastChildPattern();
                        p.AddTriplePattern(temp.TriplePatterns.First());
                    }
                    break;

                case Token.DOT:
                    // Can Discard this if last character was the end of a nested Graph pattern
                    if (lasttoken == Token.RIGHTCURLYBRACKET || lasttoken == Token.RIGHTBRACKET)
                    {
                        // Can Discard this if the next character is not another DOT
                        next = context.Tokens.Peek();
                        if (next.TokenType != Token.DOT)
                        {
                            if (next.TokenType != Token.RIGHTCURLYBRACKET)
                            {
                                TryParseTriplePatterns(context, p);
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            throw ParserHelper.Error("A DOT Token cannot follow another DOT Token within a Graph Pattern", next);
                        }
                    }
                    else if (lasttoken == Token.SEMICOLON)
                    {
                        // Allow Trailing Semicolon
                        return;
                    }
                    else
                    {
                        throw ParserHelper.Error("A DOT Token can only be used to terminate a Triple Pattern or a Nested Graph Pattern", next);
                    }
                    break;

                default:
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered when the start of a Triple Pattern was expected", next);
            }
        }

        private void TryParsePredicateObjectList(SparqlQueryParserContext context, GraphPattern p, int expectedCount)
        {
            PatternItem subj, pred, obj;
            
            // Subject is first thing on the Stack
            subj = TryCreatePatternItem(context, context.LocalTokens.Pop());

            // Start grabbing other stuff off the Stack and Parsing
            IToken next, lit, temp;
            ISparqlPath path;

            do
            {
                // Peek at the Next Token
                next = context.Tokens.Peek();
                switch (next.TokenType)
                {
                    case Token.COMMENT:
                        // Ignore Comments
                        context.Tokens.Dequeue();
                        break;

                    case Token.VARIABLE:
                        context.LocalTokens.Push(next);
                        context.Query.AddVariable(next.Value);
                        context.Tokens.Dequeue();
                        break;

                    case Token.URI:
                    case Token.QNAME:
                    case Token.LITERAL:
                    case Token.LONGLITERAL:
                    case Token.PLAINLITERAL:
                    case Token.KEYWORDA:
                        context.LocalTokens.Push(next);
                        context.Tokens.Dequeue();
                        break;

                    case Token.HAT:
                    case Token.DIVIDE:
                    case Token.BITWISEOR:
                    case Token.MULTIPLY:
                    case Token.PLUS:
                    case Token.QUESTION:
                    case Token.NEGATION:
                        // If we see any of these Tokens then it's a Property Path
                        if (context.SyntaxMode == SparqlQuerySyntax.Sparql_1_0) throw new RdfParseException("Property Paths are not permitted in SPARQL 1.0");

                        if (context.LocalTokens.Count == expectedCount - 1)
                        {
                            path = context.PathParser.Parse(context, context.LocalTokens.Pop());
                            PathToken pathToken = new PathToken(path);
                            context.LocalTokens.Push(pathToken);
                        }
                        else if ((next.TokenType == Token.HAT || next.TokenType == Token.NEGATION) && context.LocalTokens.Count == expectedCount - 2)
                        {
                            // ^ and ! may be used to start a pattern
                            context.Tokens.Dequeue();
                            path = context.PathParser.Parse(context, next);
                            PathToken pathToken = new PathToken(path);
                            context.LocalTokens.Push(pathToken);
                        }
                        else
                        {
                            throw ParserHelper.Error("Encountered a '" + next.GetType().ToString() + "' Token which is valid only after a Predicate to indicate Path Cardinality", next);
                        }
                        break;

                    case Token.BLANKNODE:
                    case Token.BLANKNODEWITHID:
                        // Generate a new Blank Node ID if required
                        if (next.TokenType == Token.BLANKNODE)
                        {
                            next = new BlankNodeWithIDToken(context.GetNewBlankNodeID(), 0, 0, 0);
                        }

                        // Check list of Blank Node usages
                        if (context.BlankNodeIDUsages.ContainsKey(next.Value))
                        {
                            if (context.CheckBlankNodeScope && context.BlankNodeIDUsages[next.Value] != context.GraphPatternID)
                            {
                                throw ParserHelper.Error("Invalid use of Blank Node Label '" + next.Value + "', this Label has already been used in a different Graph Pattern", next);
                            }
                        }
                        else
                        {
                            context.BlankNodeIDUsages.Add(next.Value, context.GraphPatternID);
                        }

                        context.LocalTokens.Push(next);
                        context.Tokens.Dequeue();
                        break;

                    case Token.HATHAT:
                        // Get the next Token which should be a Datatype Token
                        context.Tokens.Dequeue();
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.DATATYPE)
                        {
                            // Get the previous Token off the Stack and ensure it's a Literal
                            lit = context.LocalTokens.Pop();
                            if (lit.TokenType == Token.LITERAL || lit.TokenType == Token.LONGLITERAL)
                            {
                                // Create a DataTyped Literal
                                context.LocalTokens.Push(new LiteralWithDataTypeToken(lit, (DataTypeToken)next));
                            }
                            else
                            {
                                throw ParserHelper.Error("Unexpected Datatype Token, a Datatype may only be specified after a quoted Literal/Long Literal", lit);
                            }
                        }
                        else
                        {
                            throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Datatype Token to follow a ^^ Token to specify the Datatype of a previous Literal Token", next);
                        }
                        context.Tokens.Dequeue();
                        break;

                    case Token.LEFTSQBRACKET:
                        // Start of Blank Node Collection
                        // Create a new Blank Node Token
                        BlankNodeWithIDToken bnode = new BlankNodeWithIDToken(context.GetNewBlankNodeID(), 0, 0, 0);

                        // Push twice, once for Object of the current Triple
                        context.LocalTokens.Push(bnode);

                        context.Tokens.Dequeue();
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.RIGHTSQBRACKET)
                        {
                            // Single anonymous blank node
                            context.Tokens.Dequeue();
                            break;
                        }

                        // Blank Node Collection
                        // Push again for Subject of new Triple
                        context.LocalTokens.Push(bnode);

                        // Recursively call self to parse the new Triple list
                        TryParsePredicateObjectList(context, p, expectedCount + 2);
                        break;

                    case Token.RIGHTSQBRACKET:
                        // End of Blank Node Collection

                        // Allow for trailing semicolon
                        if (context.LocalTokens.Count == expectedCount - 2 && context.Tokens.LastTokenType == Token.SEMICOLON)
                        {
                            context.Tokens.Dequeue();
                            return;
                        }

                        // Check length of Stack
                        if (context.LocalTokens.Count < expectedCount)
                        {
                            throw ParserHelper.Error("Encountered a Right Square Bracket Token to terminate a Blank Node Collection within a Triple Pattern but there are not enough Tokens to form a valid Triple Pattern", next);
                        }
                        else if (context.LocalTokens.Count > expectedCount)
                        {
                            throw ParserHelper.Error("Encountered a Right Square Bracket Token to terminate a Blank Node Collection within a Triple Pattern but there are too many Tokens to form a valid Triple Pattern - " + ExcessTokensString(context, expectedCount), next);
                        }
                        obj = TryCreatePatternItem(context, context.LocalTokens.Pop());

                        if (context.LocalTokens.Peek() is PathToken)
                        {
                            PathToken pathToken = context.LocalTokens.Pop() as PathToken;
                            p.AddTriplePattern(new PropertyPathPattern(subj, pathToken.Path, obj));
                        }
                        else
                        {
                            pred = TryCreatePatternItem(context, context.LocalTokens.Pop());
                            p.AddTriplePattern(new TriplePattern(subj, pred, obj));
                        }
                        context.Tokens.Dequeue();
                        return;

                    case Token.LEFTBRACKET:
                        // Property Path if it's the Predicate or Collection if it's the Object
                        if (context.LocalTokens.Count == expectedCount - 2)
                        {
                            // Property Path
                            if (context.SyntaxMode == SparqlQuerySyntax.Sparql_1_0) throw new RdfParseException("Property Paths are not permitted in SPARQL 1.0");
 
                            path = context.PathParser.Parse(context, context.Tokens.Dequeue());
                            PathToken pathToken = new PathToken(path);
                            context.LocalTokens.Push(pathToken);
                        }
                        else
                        {
                            // Collection
                            context.Tokens.Dequeue();
                            TryParseCollection(context, p, false);
                        }
                        break;

                    case Token.LANGSPEC:
                        // Get the previous Token off the Stack and ensure it's a Literal
                        lit = context.LocalTokens.Pop();
                        if (lit.TokenType == Token.LITERAL || lit.TokenType == Token.LONGLITERAL)
                        {
                            // Create a Language Specified Literal
                            context.LocalTokens.Push(new LiteralWithLanguageSpecifierToken(lit, (LanguageSpecifierToken)next));
                        }
                        else
                        {
                            throw ParserHelper.Error("Unexpected Language Specifier Token, a Language Specifier may only be specified after a quoted Literal/Long Literal", lit);
                        }
                        context.Tokens.Dequeue();
                        break;

                    case Token.COMMA:
                        // End of a Triple Pattern

                        // Check length of stack
                        if (context.LocalTokens.Count < expectedCount)
                        {
                            throw ParserHelper.Error("Encountered a Comma Token to terminate a Triple Pattern but there are not enough Tokens to form a valid Triple Pattern", next);
                        }
                        else if (context.LocalTokens.Count > expectedCount)
                        {
                            throw ParserHelper.Error("Encountered a Comma Token to terminate a Triple Pattern but there are too many Tokens to form a valid Triple Pattern - " + ExcessTokensString(context, expectedCount), next);
                        }
                        obj = TryCreatePatternItem(context, context.LocalTokens.Pop());
                        temp = context.LocalTokens.Pop();
                        if (temp.TokenType == Token.PATH)
                        {
                            path = ((PathToken)temp).Path;
                            p.AddTriplePattern(new PropertyPathPattern(subj, path, obj));
                        }
                        else
                        {
                            pred = TryCreatePatternItem(context, temp);

                            // Add Pattern to the Graph Pattern
                            p.AddTriplePattern(new TriplePattern(subj, pred, obj));
                        }

                        // Push Predicate back on Stack
                        context.LocalTokens.Push(temp);

                        context.Tokens.Dequeue();
                        break;

                    case Token.SEMICOLON:
                        // End of a Triple Pattern

                        // Check length of stack
                        if (context.LocalTokens.Count < expectedCount)
                        {
                            throw ParserHelper.Error("Encountered a Semicolon Token to terminate a Triple Pattern but there are not enough Tokens to form a valid Triple Pattern", next);
                        }
                        else if (context.LocalTokens.Count > expectedCount)
                        {
                            throw ParserHelper.Error("Encountered a Semicolon Token to terminate a Triple Pattern but there are too many Tokens to form a valid Triple Pattern - " + ExcessTokensString(context, expectedCount), next);
                        }
                        obj = TryCreatePatternItem(context, context.LocalTokens.Pop());
                        temp = context.LocalTokens.Pop();
                        if (temp.TokenType == Token.PATH)
                        {
                            path = ((PathToken)temp).Path;
                            p.AddTriplePattern(new PropertyPathPattern(subj, path, obj));
                        }
                        else
                        {
                            pred = TryCreatePatternItem(context, temp);

                            // Add Pattern to the Graph Pattern
                            p.AddTriplePattern(new TriplePattern(subj, pred, obj));
                        }

                        context.Tokens.Dequeue();
                        break;

                    case Token.DOT:
                        // End of the Triple Patterns

                        // Allow for trailing semicolon and Blank Node Collection lists
                        if (context.LocalTokens.Count == expectedCount - 2 && (context.Tokens.LastTokenType == Token.SEMICOLON || (context.Tokens.LastTokenType == Token.RIGHTSQBRACKET && p.TriplePatterns.Count > 0)))
                        {
                            if (context.Tokens.LastTokenType == Token.RIGHTSQBRACKET)
                            {
                                context.Tokens.Dequeue();
                            }
                            return;
                        }

                        // Check length of Stack
                        if (context.LocalTokens.Count < expectedCount)
                        {
                            throw ParserHelper.Error("Encountered a DOT Token to terminate a Triple Pattern but there are not enough Tokens to form a valid Triple Pattern", next);
                        }
                        else if (context.LocalTokens.Count > expectedCount)
                        {
                            throw ParserHelper.Error("Encountered a DOT Token to terminate a Triple Pattern but there are too many Tokens to form a valid Triple Pattern - " + ExcessTokensString(context, expectedCount), next);
                        }
                        obj = TryCreatePatternItem(context, context.LocalTokens.Pop());
                        temp = context.LocalTokens.Pop();
                        if (temp.TokenType == Token.PATH)
                        {
                            path = ((PathToken)temp).Path;
                            p.AddTriplePattern(new PropertyPathPattern(subj, path, obj));
                        }
                        else
                        {
                            pred = TryCreatePatternItem(context, temp);

                            // Add Pattern to the Graph Pattern
                            p.AddTriplePattern(new TriplePattern(subj, pred, obj));
                        }
                        context.Tokens.Dequeue();
                        return;

                    case Token.LEFTCURLYBRACKET:
                    case Token.RIGHTCURLYBRACKET:
                    case Token.OPTIONAL:
                    case Token.EXISTS:
                    case Token.NOTEXISTS:
                    case Token.UNSAID:
                    case Token.MINUS_P:
                    case Token.SERVICE:
                    case Token.GRAPH:
                    case Token.FILTER:
                    case Token.VALUES:
                        // End of the Triple Patterns

                        // Allow for trailing semicolon and Blank Node Collection lists
                        if (context.LocalTokens.Count == expectedCount - 2 && (context.Tokens.LastTokenType == Token.SEMICOLON || ((context.Tokens.LastTokenType == Token.RIGHTSQBRACKET || context.Tokens.LastTokenType == Token.RIGHTBRACKET) && p.TriplePatterns.Count > 0)))
                        {
                            return;
                        }

                        // Check length of Stack
                        if (context.LocalTokens.Count < expectedCount)
                        {
                            temp = context.LocalTokens.Peek();
                            if (next.TokenType == Token.LEFTCURLYBRACKET && context.SyntaxMode != SparqlQuerySyntax.Sparql_1_0 && context.LocalTokens.Count == expectedCount - 1 && (temp.TokenType == Token.QNAME || temp.TokenType == Token.URI || temp.TokenType == Token.KEYWORDA))
                            {
                                // In this case this should be a Cardinality Modifier on a path (we hope)
                                path = context.PathParser.Parse(context, context.LocalTokens.Pop());
                                IToken pathToken = new PathToken(path);
                                context.LocalTokens.Push(pathToken);
                                continue;
                            }
                            else
                            {
                                throw ParserHelper.Error("Encountered a Token which terminates a Triple Pattern but there are not enough Tokens to form a valid Triple Pattern", next);
                            }
                        }
                        else if (context.LocalTokens.Count > expectedCount)
                        {
                            throw ParserHelper.Error("Encountered a Token which terminates a Triple Pattern but there are too many Tokens to form a valid Triple Pattern - " + ExcessTokensString(context, expectedCount), next);
                        }
                        obj = TryCreatePatternItem(context, context.LocalTokens.Pop());
                        temp = context.LocalTokens.Pop();
                        if (temp.TokenType == Token.PATH)
                        {
                            path = ((PathToken)temp).Path;
                            p.AddTriplePattern(new PropertyPathPattern(subj, path, obj));
                        }
                        else
                        {
                            pred = TryCreatePatternItem(context, temp);

                            // Add Pattern to the Graph Pattern
                            p.AddTriplePattern(new TriplePattern(subj, pred, obj));
                        }
                        return;
                    
                    default:
                        throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' while trying to Parse Triple Patterns", next);
                }
            } while (true);

        }

        private void TryParseCollection(SparqlQueryParserContext context, GraphPattern p, bool nested)
        {

            // Check the next Token
            IToken next = context.Tokens.Peek();
            if (next.TokenType == Token.RIGHTBRACKET)
            {
                // Empty Collection
                context.Tokens.Dequeue();

                if (!nested)
                {
                    // Push an rdf:nil Uri on the Stack
                    context.LocalTokens.Push(new UriToken("<" + NamespaceMapper.RDF + "nil>", next.StartLine, next.StartPosition, next.EndPosition));
                }
            }
            else
            {
                // Push a Blank Node Token onto the stack for the start of the collection
                BlankNodeWithIDToken blank; 
                if (!nested)
                {
                    blank = new BlankNodeWithIDToken(context.GetNewBlankNodeID(), next.StartLine, next.StartPosition, next.EndPosition);
                    context.LocalTokens.Push(blank);
                } 
                else 
                {
                    blank = new BlankNodeWithIDToken("_:sparql-autos" + context.BlankNodeID, next.StartLine, next.StartPosition, next.EndPosition);
                }

                bool first = true;

                IUriNode rdfFirst, rdfRest, rdfNil;
                rdfFirst = new UriNode(null, UriFactory.Create(NamespaceMapper.RDF + "first"));
                rdfRest = new UriNode(null, UriFactory.Create(NamespaceMapper.RDF + "rest"));
                rdfNil = new UriNode(null, UriFactory.Create(NamespaceMapper.RDF + "nil"));

                do
                {
                    next = context.Tokens.Peek();

                    switch (next.TokenType)
                    {
                        case Token.BLANKNODE:
                        case Token.BLANKNODEWITHID:
                        case Token.KEYWORDA:
                        case Token.LITERAL:
                        case Token.LONGLITERAL:
                        case Token.PLAINLITERAL:
                        case Token.QNAME:
                        case Token.URI:
                        case Token.VARIABLE:
                            // Create the Triple pattern

                            if (first)
                            {
                                // rdf:first Pattern
                                p.AddTriplePattern(new TriplePattern(TryCreatePatternItem(context, blank), new NodeMatchPattern(rdfFirst), TryCreatePatternItem(context, next)));
                                first = false;
                            }
                            else
                            {
                                // Get new Blank Node ID
                                BlankNodeWithIDToken blank2 = new BlankNodeWithIDToken(context.GetNewBlankNodeID(), next.StartLine,next.StartPosition,next.EndPosition);

                                // rdf:rest Pattern
                                p.AddTriplePattern(new TriplePattern(TryCreatePatternItem(context, blank), new NodeMatchPattern(rdfRest), TryCreatePatternItem(context, blank2)));

                                blank = blank2;

                                // rdf:first Pattern
                                p.AddTriplePattern(new TriplePattern(TryCreatePatternItem(context, blank), new NodeMatchPattern(rdfFirst), TryCreatePatternItem(context, next)));
                            }

                            break;

                        case Token.LEFTSQBRACKET:
                            // Is the next token a Right Square Bracket?
                            // ie. a [] for an anonymous blank node
                            context.Tokens.Dequeue();
                            next = context.Tokens.Peek();

                            if (next.TokenType == Token.RIGHTSQBRACKET)
                            {
                                BlankNodeWithIDToken anon = new BlankNodeWithIDToken(context.GetNewBlankNodeID(), next.StartLine, next.StartPosition, next.EndPosition);

                                if (first)
                                {
                                    // rdf:first Pattern
                                    p.AddTriplePattern(new TriplePattern(TryCreatePatternItem(context, blank), new NodeMatchPattern(rdfFirst), TryCreatePatternItem(context, anon)));
                                    first = false;
                                }
                                else
                                {
                                    // Get new Blank Node ID
                                    BlankNodeWithIDToken blank2 = new BlankNodeWithIDToken(context.GetNewBlankNodeID(), next.StartLine, next.StartPosition, next.EndPosition);

                                    // rdf:rest Pattern
                                    p.AddTriplePattern(new TriplePattern(TryCreatePatternItem(context, blank), new NodeMatchPattern(rdfRest), TryCreatePatternItem(context, blank2)));

                                    blank = blank2;

                                    // rdf:first Pattern
                                    p.AddTriplePattern(new TriplePattern(TryCreatePatternItem(context, blank), new NodeMatchPattern(rdfFirst), TryCreatePatternItem(context, anon)));
                                }
                            }
                            else
                            {
                                BlankNodeWithIDToken anon = new BlankNodeWithIDToken(context.GetNewBlankNodeID(), next.StartLine, next.StartPosition, next.EndPosition);

                                if (first)
                                {
                                    // rdf:first Pattern
                                    p.AddTriplePattern(new TriplePattern(TryCreatePatternItem(context, blank), new NodeMatchPattern(rdfFirst), TryCreatePatternItem(context, anon)));
                                    first = false;
                                }
                                else
                                {
                                    // Get new Blank Node ID
                                    BlankNodeWithIDToken blank2 = new BlankNodeWithIDToken(context.GetNewBlankNodeID(), next.StartLine, next.StartPosition, next.EndPosition);

                                    // rdf:rest Pattern
                                    p.AddTriplePattern(new TriplePattern(TryCreatePatternItem(context, blank), new NodeMatchPattern(rdfRest), TryCreatePatternItem(context, blank2)));

                                    blank = blank2;

                                    // rdf:first Pattern
                                    p.AddTriplePattern(new TriplePattern(TryCreatePatternItem(context, blank), new NodeMatchPattern(rdfFirst), TryCreatePatternItem(context, anon)));
                                }

                                // Parse the Blank Node Collection
                                context.LocalTokens.Push(anon);
                                TryParsePredicateObjectList(context, p, context.LocalTokens.Count + 1);
                                continue;
                            }
                            break;

                        case Token.LEFTBRACKET:

                            BlankNodeWithIDToken innerCollection = new BlankNodeWithIDToken(context.GetNewBlankNodeID(), next.StartLine, next.StartPosition, next.EndPosition);

                            if (first)
                            {
                                // rdf:first Pattern
                                p.AddTriplePattern(new TriplePattern(TryCreatePatternItem(context, blank), new NodeMatchPattern(rdfFirst), TryCreatePatternItem(context, innerCollection)));
                                first = false;
                            }
                            else
                            {
                                // Get new Blank Node ID
                                BlankNodeWithIDToken blank2 = new BlankNodeWithIDToken(context.GetNewBlankNodeID(), next.StartLine, next.StartPosition, next.EndPosition);

                                // rdf:rest Pattern
                                p.AddTriplePattern(new TriplePattern(TryCreatePatternItem(context, blank), new NodeMatchPattern(rdfRest), TryCreatePatternItem(context, blank2)));

                                blank = blank2;

                                // rdf:first Pattern
                                p.AddTriplePattern(new TriplePattern(TryCreatePatternItem(context, blank), new NodeMatchPattern(rdfFirst), TryCreatePatternItem(context, innerCollection)));
                            }

                            context.Tokens.Dequeue();
                            TryParseCollection(context, p, true);
                            continue;

                        case Token.RIGHTBRACKET:
                            // End of Collection

                            // rdf:rest Pattern
                            p.AddTriplePattern(new TriplePattern(TryCreatePatternItem(context, blank), new NodeMatchPattern(rdfRest), new NodeMatchPattern(rdfNil)));
                            break;

                        default:
                            throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered while trying to parse a Collection", next);
                    }

                    context.Tokens.Dequeue();
                } while (next.TokenType != Token.RIGHTBRACKET);
            }
        }

        private void TryParseFilterClause(SparqlQueryParserContext context, GraphPattern p)
        {
            // TODO: Refactor entire function to just rely on SparqlExpressionParser instead

            IToken next = context.Tokens.Dequeue();
            switch (next.TokenType)
            {
                case Token.LEFTBRACKET:
                    TryParseFilterExpression(context, p);
                    break;

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
                case Token.MINUTES:
                case Token.MONTH:
                case Token.NOW:
                case Token.RAND:
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
                    // Built-in functions
                    TryParseFilterBuiltInCall(context, next, p);
                    break;

                case Token.EXISTS:
                case Token.NOTEXISTS:
                    // EXISTS/NOT EXISTS
                    TryParseFilterExists(context, p, (next.TokenType == Token.EXISTS));
                    break;
                
                case Token.REGEX:
                    // Regular Expression
                    TryParseFilterRegex(context, next, p);
                    break;

                case Token.URI:
                case Token.QNAME:
                    // Extension function
                    TryParseFilterFunctionCall(context, next, p);
                    break;

                default:
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered while trying to parse a FILTER Clause", next);
            }
        }

        private void TryParseFilterExpression(SparqlQueryParserContext context, GraphPattern p)
        {
            UnaryExpressionFilter filter = new UnaryExpressionFilter(TryParseExpression(context, false));
            p.IsFiltered = true;
            p.AddFilter(filter);
        }

        private void TryParseFilterBuiltInCall(SparqlQueryParserContext context, IToken t, GraphPattern p)
        {
            ISparqlFilter filter;
            IToken next = context.Tokens.Dequeue();

            // Should get a LeftBracket next
            if (next.TokenType != Token.LEFTBRACKET)
            {
                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered while trying to parse a Built-in Function call", next);
            }

            // Gather tokens for the FILTER expression
            Queue<IToken> subtokens = new Queue<IToken>();
            subtokens.Enqueue(t);
            subtokens.Enqueue(next);
            int openBrackets = 1;
            while (openBrackets > 0)
            {
                next = context.Tokens.Dequeue();
                if (next.TokenType == Token.LEFTBRACKET)
                {
                    openBrackets++;
                }
                else if (next.TokenType == Token.RIGHTBRACKET)
                {
                    openBrackets--;
                }

                subtokens.Enqueue(next);
            }

            ISparqlExpression expr = context.ExpressionParser.Parse(subtokens);
            if (expr is BoundFunction)
            {
                filter = new BoundFilter((VariableTerm)expr.Arguments.First());
            }
            else
            {
                filter = new UnaryExpressionFilter(expr);
            }

            p.IsFiltered = true;
            p.AddFilter(filter);
        }

        private void TryParseFilterRegex(SparqlQueryParserContext context, IToken t, GraphPattern p)
        {
            // Gather all the Tokens that make up the Regex
            IToken next = context.Tokens.Peek();
            Queue<IToken> regexTokens = new Queue<IToken>();
            regexTokens.Enqueue(t);

            int openBrackets = 0;
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

                regexTokens.Enqueue(next);
                context.Tokens.Dequeue();
                next = context.Tokens.Peek();

            } while (openBrackets > 0);

            UnaryExpressionFilter filter = new UnaryExpressionFilter(context.ExpressionParser.Parse(regexTokens));
            p.IsFiltered = true;
            p.AddFilter(filter);
        }

        private void TryParseFilterExists(SparqlQueryParserContext context, GraphPattern p, bool exists)
        {
            if (_syntax == SparqlQuerySyntax.Sparql_1_0) throw new RdfParseException("EXISTS/NOT EXISTS is not supported in SPARQL 1.0");

            // EXISTS/NOT EXISTS generate a new Graph Pattern
            GraphPattern existsClause = TryParseGraphPattern(context);

            UnaryExpressionFilter filter = new UnaryExpressionFilter(new ExistsFunction(existsClause, exists));
            p.IsFiltered = true;
            p.AddFilter(filter);
        }

        private void TryParseFilterFunctionCall(SparqlQueryParserContext context, IToken t, GraphPattern p)
        {
            // Gather the Terms of the Function Call
            Queue<IToken> funcTokens = new Queue<IToken>();
            funcTokens.Enqueue(t);

            int openBrackets = 0;
            IToken next = context.Tokens.Peek();
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

                funcTokens.Enqueue(next);
                context.Tokens.Dequeue();
                next = context.Tokens.Peek();

            } while (openBrackets > 0);

            UnaryExpressionFilter filter = new UnaryExpressionFilter(context.ExpressionParser.Parse(funcTokens));
            p.IsFiltered = true;
            p.AddFilter(filter);
        }

        private void TryParseOptionalClause(SparqlQueryParserContext context, GraphPattern p)
        {
            // Optional Clauses generate a child Graph Pattern
            GraphPattern child = TryParseGraphPattern(context);
            child.IsOptional = true;

            p.AddGraphPattern(child);
        }

        private void TryParseGraphClause(SparqlQueryParserContext context, GraphPattern p)
        {
            // Graph Clauses generate a child Graph Pattern

            // Get the Graph Specifier - must be a Variable/IRIRef
            IToken graphspec = context.Tokens.Dequeue();
            if (graphspec.TokenType != Token.URI && graphspec.TokenType != Token.QNAME && graphspec.TokenType != Token.VARIABLE)
            {
                throw ParserHelper.Error("Unexpected Token '" + graphspec.GetType().ToString() + "' encountered, expected a URI/QName/Variable Token to specify the active Graph for a GRAPH Clause", graphspec);
            }

            // Convert a QName or Relative Uri to a Absolute Uri
            if (graphspec.TokenType != Token.VARIABLE)
            {
                String u = Tools.ResolveUriOrQName(graphspec, context.Query.NamespaceMap, context.Query.BaseUri);
                if (!u.Equals(graphspec.Value))
                {
                    graphspec = new UriToken("<" + u + ">", graphspec.StartLine, graphspec.StartPosition, graphspec.EndPosition);
                }
            }
            else
            {
                context.Query.AddVariable(graphspec.Value);
            }

            GraphPattern child = TryParseGraphPattern(context);
            child.IsGraph = true;
            child.GraphSpecifier = graphspec;

            p.AddGraphPattern(child);

        }

        private void TryParseUnionClause(SparqlQueryParserContext context, GraphPattern p)
        {
            // Create a new Pattern which will hold the UNION
            GraphPattern union = new GraphPattern();
            union.IsUnion = true;

            // Add the Last Child Pattern of the Parent as that is the start of the UNION
            GraphPattern lastchild = p.LastChildPattern();
            if (lastchild.IsSimplifiable)
            {
                union.AddGraphPattern(lastchild.LastChildPattern());
            }
            else
            {
                union.AddGraphPattern(lastchild);
            }

            GraphPattern child = TryParseGraphPattern(context, true);
            union.AddGraphPattern(child);

            // Check for multiple
            IToken next = context.Tokens.Peek();
            while (next.TokenType == Token.UNION)
            {
                context.Tokens.Dequeue();
                union.AddGraphPattern(TryParseGraphPattern(context, true));
                next = context.Tokens.Peek();
            }

            p.AddGraphPattern(union);
        }

        private ISparqlExpression TryParseExpression(SparqlQueryParserContext context, bool commasTerminate)
        {
            return TryParseExpression(context, commasTerminate, false);
        }

        private ISparqlExpression TryParseExpression(SparqlQueryParserContext context, bool commasTerminate, bool asTerminates)
        {
            // Opening Bracket ( has already been discarded
            int openBrackets = 1;
            Queue<IToken> exprTerms = new Queue<IToken>();

            IToken next;
            while (openBrackets > 0)
            {
                // Get next Token
                next = context.Tokens.Peek();

                // Take account of nesting
                if (next.TokenType == Token.LEFTBRACKET)
                {
                    openBrackets++;
                }
                else if (next.TokenType == Token.RIGHTBRACKET)
                {
                    openBrackets--;
                }
                else if (next.TokenType == Token.COMMA && openBrackets == 1 && commasTerminate)
                {
                    // Comma can terminate the Tokens that make an expression when only 1 bracket is open
                    openBrackets--;
                }
                else if (next.TokenType == Token.AS && openBrackets == 1 && asTerminates)
                {
                    // AS can terminate the Tokens that make an expression when only 1 bracket is open
                    openBrackets--;
                }
                else if (next.TokenType == Token.DISTINCT && openBrackets == 1 && commasTerminate)
                {
                    // DISTINCT can terminate the Tokens that make an expression if it occurs as the first thing and only 1 bracket is open
                    if (exprTerms.Count == 0)
                    {
                        context.Tokens.Dequeue();
                        return new DistinctModifier();
                    }
                    else
                    {
                        throw ParserHelper.Error("Unexpected DISTINCT Keyword Token encountered, DISTINCT modifier keyword may only occur as the first argument to an aggregate function", next);
                    }
                }

                if (openBrackets > 0)
                {
                    exprTerms.Enqueue(next);
                }
                context.Tokens.Dequeue();
            }

            // Use the internal Expression Parser
            return context.ExpressionParser.Parse(exprTerms);
        }

        private ISparqlExpression TryParseFunctionExpression(SparqlQueryParserContext context)
        {
            // Gather the Terms of the Function Call
            // We've already encountered a function keyword/QName/URI which is the start point
            Queue<IToken> funcTokens = new Queue<IToken>();
            funcTokens.Enqueue(context.Tokens.Dequeue());

            int openBrackets = 0;
            IToken next = context.Tokens.Peek();
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

                funcTokens.Enqueue(next);
                context.Tokens.Dequeue();
                next = context.Tokens.Peek();

            } while (openBrackets > 0);

            return context.ExpressionParser.Parse(funcTokens);
        }

        private void TryParseOrderByClause(SparqlQueryParserContext context)
        {
            // ORDER BY has already been discarded
            IToken next = context.Tokens.Peek();

            // If SPARQL 1.1 then aggregates are permitted in ORDER BY
            if (context.SyntaxMode != SparqlQuerySyntax.Sparql_1_0) context.ExpressionParser.AllowAggregates = true;

            ISparqlOrderBy first, last;
            first = last = null;
            int termsSeen = 0;
            bool exit = false;

            while (true)
            {
                switch (next.TokenType)
                {
                    case Token.VARIABLE:
                        // Simple Variable Order By
                        if (first == null)
                        {
                            first = new OrderByVariable(next.Value);
                            last = first;
                        }
                        else
                        {
                            last.Child = new OrderByVariable(next.Value);
                            last = last.Child;
                        }
                        context.Tokens.Dequeue();
                        break;

                    case Token.ASC:
                    case Token.DESC:
                        // Ascending/Descending Expression Order By

                        // Discard the ASC/DESC token
                        context.Tokens.Dequeue();
                        bool desc = (next.TokenType == Token.DESC);

                        // Discard the ( token
                        next = context.Tokens.Peek();
                        if (next.TokenType != Token.LEFTBRACKET)
                        {
                            throw new RdfParseException("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Left Bracket Token to start a Bracketted Expression after an ASC/DESC Token");
                        }
                        context.Tokens.Dequeue();

                        if (first == null)
                        {
                            first = new OrderByExpression(TryParseExpression(context, false));
                            first.Descending = desc;
                            last = first;
                        }
                        else
                        {
                            last.Child = new OrderByExpression(TryParseExpression(context, false));
                            last.Child.Descending = desc;
                            last = last.Child;
                        }
                        break;

                    case Token.LEFTBRACKET:
                        // Ascending Expression Order By

                        // Discard the Left Bracket
                        context.Tokens.Dequeue();

                        if (first == null)
                        {
                            first = new OrderByExpression(TryParseExpression(context, false));
                            last = first;
                        }
                        else
                        {
                            last.Child = new OrderByExpression(TryParseExpression(context, false));
                            last = last.Child;
                        }
                        break;

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
                    case Token.QNAME:
                    case Token.URI:
                        // Built-in/Extension Function Call Order By
                        ISparqlExpression expr = TryParseFunctionExpression(context);

                        if (first == null)
                        {
                            first = new OrderByExpression(expr);
                            last = first;
                        }
                        else
                        {
                            last.Child = new OrderByExpression(expr);
                            last = last.Child;
                        }
                        break;

                    case Token.AVG:
                    case Token.COUNT:
                    case Token.GROUPCONCAT:
                    case Token.MAX:
                    case Token.MIN:
                    case Token.SUM:
                    case Token.SAMPLE:
                        if (context.SyntaxMode == SparqlQuerySyntax.Sparql_1_0) throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToSafeString() + "' encountered, aggregates are not permitted in an ORDER BY in SPARQL 1.0", next);

                        // Built-in/Extension Function Call Order By
                        ISparqlExpression aggExpr = TryParseFunctionExpression(context);

                        if (first == null)
                        {
                            first = new OrderByExpression(aggExpr);
                            last = first;
                        }
                        else
                        {
                            last.Child = new OrderByExpression(aggExpr);
                            last = last.Child;
                        }
                        break;

                    default:
                        if (termsSeen == 0)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a valid ORDER BY clause term", next);
                        }
                        else
                        {
                            exit = true;
                        }
                        break;
                }
                if (exit) break;

                termsSeen++;
                next = context.Tokens.Peek();
            }

            context.ExpressionParser.AllowAggregates = false;

            // Set to Query
            context.Query.OrderBy = first;
        }

        private void TryParseGroupByClause(SparqlQueryParserContext context)
        {
            if (_syntax == SparqlQuerySyntax.Sparql_1_0) throw new RdfParseException("GROUP BY clauses are not supported in SPARQL 1.0");

            // GROUP BY has already been discarded
            IToken next = context.Tokens.Peek();

            ISparqlGroupBy first, last, current;
            ISparqlExpression expr;
            first = last = current = null;
            int termsSeen = 0;
            bool exit = false;
            bool terminateExpression = false;

            while (true)
            {
                switch (next.TokenType)
                {
                    case Token.VARIABLE:
                        // Simple Variable Group By

                        String name = next.Value.Substring(1);
                        terminateExpression = (context.Tokens.Peek().TokenType == Token.AS);
                        if (!terminateExpression)
                        {
                            current = new GroupByVariable(name, name);
                        }
                        else
                        {
                            current = new GroupByVariable(name);
                        }
                        if (first == null)
                        {
                            first = current;
                            last = first;
                        }
                        else
                        {
                            last.Child = current;
                            last = last.Child;
                        }
                        context.Tokens.Dequeue();
                        break;

                    case Token.LEFTBRACKET:
                        // Bracketted Expression Group By
                        context.Tokens.Dequeue();
                        expr = TryParseExpression(context, false, true);
                        terminateExpression = (context.Tokens.LastTokenType == Token.AS);
                        if (!terminateExpression && expr is VariableTerm)
                        {
                            current = new GroupByVariable(expr.Variables.First());
                        }
                        else
                        {
                            current = new GroupByExpression(expr);
                        }
                        if (first == null)
                        {
                            first = current;
                            last = first;
                        }
                        else
                        {
                            last.Child = current;
                            last = last.Child;
                        }
                        break;

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
                    case Token.YEAR:
                    case Token.URI:
                    case Token.UUID:
                    case Token.QNAME:
                        // Function Expression Group By
                        expr = TryParseFunctionExpression(context);
                        terminateExpression = (context.Tokens.Peek().TokenType == Token.AS);
                        current = new GroupByExpression(expr);
                        if (first == null)
                        {
                            first = current;
                            last = first;
                        }
                        else
                        {
                            last.Child = current;
                            last = last.Child;
                        }
                        break;

                    default:
                        if (termsSeen == 0)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a valid GROUP BY clause term", next);
                        }
                        else
                        {
                            exit = true;
                        }
                        break;
                }
                if (exit) break;

                termsSeen++;
                next = context.Tokens.Peek();

                // Allow an AS ?var after an expression
                if (next.TokenType == Token.AS || terminateExpression)
                {
                    if (!terminateExpression) context.Tokens.Dequeue();
                    next = context.Tokens.Peek();
                    if (next.TokenType == Token.VARIABLE)
                    {
                        context.Tokens.Dequeue();
                        if (current != null) current.AssignVariable = next.Value.Substring(1);
                        next = context.Tokens.Peek();

                        // Find the terminating right bracket if required
                        if (terminateExpression)
                        {
                            if (next.TokenType != Token.RIGHTBRACKET)
                            {
                                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a ) to terminate the AS clause in a bracketted expression", next);
                            }
                            context.Tokens.Dequeue();
                            next = context.Tokens.Peek();
                        }
                    }
                    else
                    {
                        throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Variable Token after an AS token in a GROUP BY clause to specify the value to assign the GROUPed value to", next);
                    }
                }

                terminateExpression = false;
            }

            // Set to Query
            context.Query.GroupBy = first;
        }

        private void TryParseHavingClause(SparqlQueryParserContext context)
        {
            if (_syntax == SparqlQuerySyntax.Sparql_1_0) throw new RdfParseException("HAVING clauses are not supported in SPARQL 1.0");
            // HAVING Keyword has already been discarded
            IToken next = context.Tokens.Peek();
            ISparqlExpression havingExpr;

            switch (next.TokenType)
            {
                case Token.LEFTBRACKET:
                    // Find and parse the Expression
                    context.Tokens.Dequeue();
                    int openBrackets = 1;
                    Queue<IToken> exprTerms = new Queue<IToken>();
                    while (openBrackets > 0)
                    {
                        // Get next Token
                        next = context.Tokens.Peek();

                        // Take account of nesting
                        if (next.TokenType == Token.LEFTBRACKET)
                        {
                            openBrackets++;
                        }
                        else if (next.TokenType == Token.RIGHTBRACKET)
                        {
                            openBrackets--;
                        }

                        if (openBrackets > 0)
                        {
                            exprTerms.Enqueue(next);
                        }
                        context.Tokens.Dequeue();
                    }

                    // Permit aggregates in the Expression
                    context.ExpressionParser.AllowAggregates = true;
                    havingExpr = context.ExpressionParser.Parse(exprTerms);
                    context.ExpressionParser.AllowAggregates = false;
                    break;

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
                case Token.YEAR:
                case Token.URI:
                case Token.UUID:
                case Token.QNAME:
                    // Built-in function/expression
                    context.ExpressionParser.AllowAggregates = true;
                    havingExpr = TryParseFunctionExpression(context);
                    context.ExpressionParser.AllowAggregates = false;
                    break;

                default:
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Left Bracket to start a bracketted expression in a HAVING Clause", next);
            }

            // Set the Having Clause of the Group By
            // For Leviathan we can just wrap in a standard Unary Expression Filter
            context.Query.Having = new UnaryExpressionFilter(havingExpr);
       }

        private void TryParseLimitOffsetClause(SparqlQueryParserContext context)
        {
            IToken next = context.Tokens.Dequeue();

            int limit, offset;
            limit = offset = 0;

            if (next.TokenType == Token.LIMIT)
            {
                // Expect a Plain Literal which can be parsed to an Integer
                next = context.Tokens.Dequeue();
                if (next.TokenType == Token.PLAINLITERAL)
                {
                    if (Int32.TryParse(next.Value, out limit))
                    {
                        context.Query.Limit = limit;

                        // Is there a subsequent OFFSET?
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.OFFSET)
                        {
                            context.Tokens.Dequeue();
                            next = context.Tokens.Dequeue();
                            if (next.TokenType == Token.PLAINLITERAL)
                            {
                                if (Int32.TryParse(next.Value, out offset))
                                {
                                    context.Query.Offset = offset;
                                }
                                else
                                {
                                    throw ParserHelper.Error("Unable to convert string '" + next.Value + "' into an Integer to use as the results offset for a OFFSET Clause", next);
                                }
                            }
                            else
                            {
                                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Plain Literal containing an Integer value as part of the OFFSET Clause", next);
                            }
                        }
                    }
                    else
                    {
                        throw ParserHelper.Error("Unable to convert string '" + next.Value + "' into an Integer to use as the results limit for a LIMIT Clause", next);
                    }
                }
                else
                {
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Plain Literal containing an Integer value as part of the LIMIT Clause", next);
                }
            }
            else if (next.TokenType == Token.OFFSET)
            {
                // Expect a Plain Literal which can be parsed to an Integer
                next = context.Tokens.Dequeue();
                if (next.TokenType == Token.PLAINLITERAL)
                {
                    if (Int32.TryParse(next.Value, out offset))
                    {
                        context.Query.Offset = offset;

                        // Is there a subsequent LIMIT?
                        next = context.Tokens.Peek();
                        if (next.TokenType == Token.LIMIT)
                        {
                            context.Tokens.Dequeue();
                            next = context.Tokens.Dequeue();
                            if (next.TokenType == Token.PLAINLITERAL)
                            {
                                if (Int32.TryParse(next.Value, out limit))
                                {
                                    context.Query.Limit = limit;
                                }
                                else
                                {
                                    throw ParserHelper.Error("Unable to convert string '" + next.Value + "' into an Integer to use as the results limit for a LIMIT Clause", next);
                                }
                            }
                            else
                            {
                                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Plain Literal containing an Integer value as part of the LIMIT Clause", next);
                            }
                        }
                    }
                    else
                    {
                        throw ParserHelper.Error("Unable to convert string '" + next.Value + "' into an Integer to use as the results offset for a OFFSET Clause", next);
                    }
                }
                else
                {
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Plain Literal containing an Integer value as part of the OFFSET Clause", next);
                }
            }
            else
            {
                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Limit/Offset Token to start a Limit Offset Clause", next);
            }
        }

        private void TryParseExistsClause(SparqlQueryParserContext context, GraphPattern p, bool exists)
        {
            if (context.SyntaxMode == SparqlQuerySyntax.Sparql_1_0) throw new RdfParseException("EXISTS and NOT EXISTS clauses are not supported in SPARQL 1.0");
            if (context.SyntaxMode == SparqlQuerySyntax.Sparql_1_1) throw new RdfParseException("EXISTS and NOT EXISTS clauses can only be used inside FILTER clauses in SPARQL 1.1");

            // EXISTS and NOT EXISTS generate a new Child Graph Pattern
            GraphPattern child = TryParseGraphPattern(context);

            if (exists)
            {
                child.IsExists = true;
            }
            else
            {
                child.IsNotExists = true;
            }

            p.AddGraphPattern(child);
        }

        private void TryParseMinusClause(SparqlQueryParserContext context, GraphPattern p)
        {
            if (context.SyntaxMode == SparqlQuerySyntax.Sparql_1_0) throw new RdfParseException("MINUS clauses are not supported in SPARQL 1.0");

            // MINUS generates a new child graph pattern
            GraphPattern child = TryParseGraphPattern(context);
            child.IsMinus = true;
            p.AddGraphPattern(child);
        }

        private void TryParseLetAssignment(SparqlQueryParserContext context, GraphPattern p)
        {
            if (context.SyntaxMode == SparqlQuerySyntax.Sparql_1_0) throw new RdfParseException("LET assignment is not supported in SPARQL 1.0");
            if (context.SyntaxMode == SparqlQuerySyntax.Sparql_1_1) throw new RdfParseException("LET assignment is not supported in SPARQL 1.1 - use BIND assignment instead");

            IToken variable;
            ISparqlExpression expr;

            // Firstly we expect an opening bracket, a variable and then an assignment operator
            IToken next = context.Tokens.Dequeue();
            if (next.TokenType == Token.LEFTBRACKET)
            {
                next = context.Tokens.Dequeue();
                if (next.TokenType == Token.VARIABLE)
                {
                    variable = next;
                    context.Query.AddVariable(variable.Value, false);
                    next = context.Tokens.Dequeue();
                    if (next.TokenType == Token.ASSIGNMENT)
                    {
                        // See if there is a valid expression for the right hand side of the assignment
                        next = context.Tokens.Peek();
                        switch (next.TokenType)
                        {
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
                            case Token.URI:
                            case Token.QNAME:
                                expr = TryParseFunctionExpression(context);
                                break;
                            case Token.LEFTBRACKET:
                                context.Tokens.Dequeue();
                                expr = TryParseExpression(context, false);
                                break;
                            case Token.VARIABLE:
                                context.Tokens.Dequeue();
                                expr = new VariableTerm(next.Value);
                                break;
                            default:
                                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Token which was valid as the start of an expression for the right hand side of a LET assignment", next);
                        }

                        // Finally expect a Right Bracket to terminate the LET
                        next = context.Tokens.Dequeue();
                        if (next.TokenType != Token.RIGHTBRACKET)
                        {
                            throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Right Bracket to terminate the LET assignment", next);
                        }
                        
                        // Create a Let Pattern and add to the Query appropriately
                        LetPattern let = new LetPattern(variable.Value.Substring(1), expr);
                        if (Options.QueryOptimisation)
                        {
                            p.AddAssignment(let);
                        }
                        else
                        {
                            // When Optimisation is turned off we'll just stick the Let in the Triples Pattern where it occurs
                            // since we're not going to do any Triple Pattern ordering, Assignment or FILTER placement
                            p.AddTriplePattern(let);
                        }
                    }
                    else
                    {
                        throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected an Assignment operator as part of a LET assignment", next);
                    }
                }
                else
                {
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Variable as the first item in a LET assignment", next);
                }
            }
            else
            {
                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Left Bracket to start a LET assignment after a LET Keyword", next);
            }
        }

        private void TryParseBindAssignment(SparqlQueryParserContext context, GraphPattern p)
        {
            if (context.SyntaxMode == SparqlQuerySyntax.Sparql_1_0) throw new RdfParseException("BIND assignment is not supported in SPARQL 1.0");

            // First need to discard opening (
            IToken next = context.Tokens.Dequeue();
            if (next.TokenType != Token.LEFTBRACKET) throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a ( to start a BIND assignment after a BIND keyword", next);

            // Expect a bracketted expression terminated by an AS
            ISparqlExpression expr = TryParseExpression(context, false, true);
            if (context.Tokens.LastTokenType != Token.AS)
            {
                throw ParserHelper.Error("A BIND assignment did not end with an AS ?var as expected, BIND assignment must be of the general form BIND(expr AS ?var)", next);
            }

            // Ensure there is a Variable after the AS
            next = context.Tokens.Dequeue();
            if (next.TokenType == Token.VARIABLE)
            {
                BindPattern bind = new BindPattern(next.Value.Substring(1), expr);

                // Check that the Variable has not already been used
                if (context.Query.RootGraphPattern != null && context.Query.RootGraphPattern.Variables.Contains(bind.VariableName))
                {
                    throw ParserHelper.Error("A BIND assignment is attempting to bind to the variable ?" + bind.VariableName + " but this variable is already in use in the query", next);
                }
                else if (p.Variables.Contains(bind.VariableName))
                {
                    throw ParserHelper.Error("A BIND assignment is attempting to bind to the variable ?" + bind.VariableName + " but this variable is already in use earlier in the Graph pattern", next);
                }

                if (Options.QueryOptimisation)
                {
                    p.AddAssignment(bind);
                }
                else
                {
                    // When Optimisation is turned off we'll just stick the BIND in the Triples Pattern where it occurs
                    // since we're not going to do any Triple Pattern ordering, Assignment or FILTER placement
                    p.AddTriplePattern(bind);
                    // In this case the BIND must break the BGP since using AddTriplePattern will not do it automatically
                    p.BreakBGP();
                }

                // Ensure the BIND assignment is terminated with a )
                next = context.Tokens.Dequeue();
                if (next.TokenType != Token.RIGHTBRACKET) throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a ) to terminate a BIND assignment", next);
            }
            else
            {
                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Variable after the AS in a BIND assignment", next);
            }
        }

        private void TryParseSubquery(SparqlQueryParserContext context, GraphPattern p)
        {
            if (context.SyntaxMode == SparqlQuerySyntax.Sparql_1_0) throw new RdfParseException("Sub-queries are not supported in SPARQL 1.0");

            // We're going to make a temporary Token Queue which we will populate and then
            // use to create a new SPARQL Query Parser Context
            NonTokenisedTokenQueue tokens = new NonTokenisedTokenQueue();

            // Assume we've already seen a SELECT
            tokens.Enqueue(new BOFToken());
            tokens.Enqueue(new SelectKeywordToken(1, 1));

            // Now collect Tokens until we hit the closing Right Bracket
            int openBrackets = 1;
            do
            {
                IToken next = context.Tokens.Peek();

                if (next.TokenType == Token.LEFTCURLYBRACKET)
                {
                    openBrackets++;
                }
                else if (next.TokenType == Token.RIGHTCURLYBRACKET)
                {
                    openBrackets--;
                }
                else if (next.TokenType == Token.EOF)
                {
                    throw ParserHelper.Error("Unexpected End of File encountered while trying to gather Tokens to parse a Sub-Query from", next);
                }

                if (openBrackets > 0)
                {
                    tokens.Enqueue(context.Tokens.Dequeue());
                }
            } while (openBrackets > 0);

            tokens.Enqueue(new EOFToken(0, 0));

            // Create a Sub-query Parser Context
            SparqlQueryParserContext subcontext = new SparqlQueryParserContext(context, tokens);
            subcontext.Query.NamespaceMap.Import(context.Query.NamespaceMap);
            SparqlQuery subquery = ParseInternal(subcontext);
            foreach (SparqlVariable var in subquery.Variables)
            {
                if (var.IsResultVariable) context.Query.AddVariable("?" + var.Name, false);
            }
            SubQueryPattern subqueryPattern = new SubQueryPattern(subquery);
            GraphPattern p2 = new GraphPattern();
            p2.AddTriplePattern(subqueryPattern);
            p.AddGraphPattern(p2);
        }

        private void TryParseServiceClause(SparqlQueryParserContext context, GraphPattern p)
        {
            if (context.SyntaxMode == SparqlQuerySyntax.Sparql_1_0) throw new RdfParseException("SERVICE clauses are not supported in SPARQL 1.0");

            // May allow an optional SILENT keyword
            bool silent = false;
            if (context.Tokens.Peek().TokenType == Token.SILENT)
            {
                context.Tokens.Dequeue();
                silent = true;
            }

            // SERVICE first has a URI/Variable service specifier
            IToken specifier = context.Tokens.Dequeue();
            if (specifier.TokenType != Token.URI && specifier.TokenType != Token.VARIABLE)
            {
                throw ParserHelper.Error("Unexpected Token '" + specifier.GetType().ToString() + "' encountered, expected a URI/Variable after a SERVICE keyword", specifier);
            }

            // Then a Graph Pattern
            GraphPattern child = TryParseGraphPattern(context);
            child.IsService = true;
            child.GraphSpecifier = specifier;
            child.IsSilent = silent;
            p.AddGraphPattern(child);
        }

        private BindingsPattern TryParseInlineData(SparqlQueryParserContext context)
        {
            // First expect either a single variable or a sequence of variables enclosed in ( )
            IToken next = context.Tokens.Peek();
            List<String> vars = new List<String>();
            bool simpleForm = false;
            if (next.TokenType == Token.LEFTBRACKET)
            {
                context.Tokens.Dequeue();
                next = context.Tokens.Peek();
                while (next.TokenType == Token.VARIABLE)
                {
                    vars.Add(next.Value.Substring(1));
                    context.Tokens.Dequeue();
                    next = context.Tokens.Peek();
                }
                if (next.TokenType != Token.RIGHTBRACKET) throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' ecountered, expected a ) to terminate the variables list for a VALUES clause", next);
                context.Tokens.Dequeue();
            }
            else if (next.TokenType == Token.VARIABLE)
            {
                // Using the simplified form of the syntax
                simpleForm = true;
                vars.Add(next.Value.Substring(1));
                context.Tokens.Dequeue();
            }
            else
            {
                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected variables list for a VALUES clause", next);
            }

            // Then expect a Left Curly Bracket
            next = context.Tokens.Peek();
            if (next.TokenType == Token.LEFTCURLYBRACKET)
            {
                context.Tokens.Dequeue();
                BindingsPattern bindings = new BindingsPattern(vars);

                // Each Binding tuple must start with a ( unless using simplified single variable syntax form
                next = context.Tokens.Peek();
                while ((simpleForm && next.TokenType != Token.RIGHTCURLYBRACKET) || next.TokenType == Token.LEFTBRACKET)
                {
                    if (!simpleForm)
                    {
                        // Discard the ( and peek the next token
                        context.Tokens.Dequeue();
                        next = context.Tokens.Peek();
                    }

                    // Expect a sequence of values in the tuple
                    List<PatternItem> values = new List<PatternItem>();
                    while (next.TokenType != Token.RIGHTBRACKET)
                    {
                        next = context.Tokens.Dequeue();

                        // Get the value
                        switch (next.TokenType)
                        {
                            case Token.URI:
                            case Token.QNAME:
                            case Token.LITERALWITHDT:
                            case Token.LITERALWITHLANG:
                            case Token.PLAINLITERAL:
                                values.Add(TryCreatePatternItem(context, next));
                                break;

                            case Token.LONGLITERAL:
                            case Token.LITERAL:
                                // Need to check for subsequent datatype or language declaration
                                IToken lit = next;
                                next = context.Tokens.Peek();
                                if (next.TokenType == Token.HATHAT)
                                {
                                    context.Tokens.Dequeue();
                                    next = context.Tokens.Dequeue();
                                    if (next.TokenType == Token.DATATYPE)
                                    {
                                        LiteralWithDataTypeToken dtlit = new LiteralWithDataTypeToken(lit, (DataTypeToken)next);
                                        values.Add(TryCreatePatternItem(context, dtlit));
                                    }
                                    else
                                    {
                                        throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Datatype Token to specify the datatype for a Literal", next);
                                    }
                                }
                                else if (next.TokenType == Token.LANGSPEC)
                                {
                                    context.Tokens.Dequeue();
                                    LiteralWithLanguageSpecifierToken langlit = new LiteralWithLanguageSpecifierToken(lit, (LanguageSpecifierToken)next);
                                    values.Add(TryCreatePatternItem(context, langlit));
                                }
                                else
                                {
                                    values.Add(TryCreatePatternItem(context, lit));
                                }
                                break;

                            case Token.UNDEF:
                                // UNDEF indicates an unbound variable which equates to a null
                                values.Add(null);
                                break;

                            default:
                                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Token for a URI/Literal or an UNDEF keyword as part of a tuple in a VALUES clause", next);
                        }

                        next = context.Tokens.Peek();

                        // For simplified syntax just break after each value
                        if (simpleForm) break;
                    }

                    if (vars.Count != values.Count)
                    {
                        throw new RdfParseException("Invalid tuple in the VALUES clause, each Binding should contain " + vars.Count + " values but got a tuple containing " + values.Count + " values");
                    }

                    // Generate a representation of this possible solution and add it to our Bindings object
                    bindings.AddTuple(new BindingTuple(vars, values));

                    // Discard the ) and peek the next token
                    if (!simpleForm) context.Tokens.Dequeue();
                    next = context.Tokens.Peek();
                }

                // Finally we need to see a Right Curly Bracket
                if (next.TokenType != Token.RIGHTCURLYBRACKET)
                {
                    throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Right Curly Bracket to terminate the VALUES clause", next);
                }
                context.Tokens.Dequeue();

                return bindings;
            }
            else
            {
                throw ParserHelper.Error("Unexpected Token '" + next.GetType().ToString() + "' encountered, expected a Left Curly Bracket after the list of variables as part of a VALUES clause", next);
            }
        }

        private PatternItem TryCreatePatternItem(SparqlQueryParserContext context, IToken t)
        {
            String baseUri;
            Uri u;

            switch (t.TokenType)
            {
                case Token.VARIABLE:
                    // Variables accept any Node as a substitution
                    return new VariablePattern(t.Value);

                case Token.URI:
                    // Uri uses a Node Match
                    if (t.Value.StartsWith("_:"))
                    {
                        return new FixedBlankNodePattern(t.Value);
                    }
                    else
                    {
                        String uri = Tools.ResolveUri(t.Value, context.Query.BaseUri.ToSafeString());
                        u = UriFactory.Create(uri);
                        return new NodeMatchPattern(new UriNode(null, u));
                    }

                case Token.QNAME:
                    // QName uses a Node Match
                    return new NodeMatchPattern(new UriNode(null, ResolveQName(context, t.Value)));

                case Token.LITERAL:
                case Token.LONGLITERAL:
                    // Literals use Node Matches
                    return new NodeMatchPattern(new NonNormalizedLiteralNode(null, t.Value));

                case Token.PLAINLITERAL:
                    // Plain Literals either use an inferred Literal Node Match
                    // We know it must be one of the inferrable types or the Parser would have failed at the Tokenisation stage for the Literal
                    if (TurtleSpecsHelper.IsValidDouble(t.Value))
                    {
                        // Double - Check first since to be considered a double must contain an exponent so is unique compared to 
                        // the other two numeric types
                        return new NodeMatchPattern(new LiteralNode(null, t.Value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDouble)));
                    }
                    else if (TurtleSpecsHelper.IsValidInteger(t.Value))
                    {
                        // Integer - Check before decimal as any valid integer is a valid decimal
                        return new NodeMatchPattern(new LiteralNode(null, t.Value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger)));
                    }
                    else if (TurtleSpecsHelper.IsValidDecimal(t.Value))
                    {
                        // Decimal - Check last since any valid integer is also a valid decimal
                        return new NodeMatchPattern(new LiteralNode(null, t.Value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDecimal)));
                    }
                    else
                    {
                        // Boolean
                        return new NodeMatchPattern(new LiteralNode(null, t.Value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeBoolean)));
                    }


                case Token.LITERALWITHDT:
                    // Literal with Datatype use Node Matches
                    LiteralWithDataTypeToken litdt = (LiteralWithDataTypeToken)t;
                    if (litdt.DataType.StartsWith("<"))
                    {
                        baseUri = (context.Query.BaseUri == null) ? String.Empty : context.Query.BaseUri.AbsoluteUri;
                        u = UriFactory.Create(Tools.ResolveUri(litdt.DataType.Substring(1, litdt.DataType.Length - 2), baseUri));
                        return new NodeMatchPattern(new NonNormalizedLiteralNode(null, litdt.Value, u));
                    }
                    else
                    {
                        // Resolve the QName                       
                        return new NodeMatchPattern(new NonNormalizedLiteralNode(null, litdt.Value, ResolveQName(context, litdt.DataType)));
                    }

                case Token.LITERALWITHLANG:
                    // Literal with Lang Spec use Node Matches
                    LiteralWithLanguageSpecifierToken litls = (LiteralWithLanguageSpecifierToken)t;
                    return new NodeMatchPattern(new NonNormalizedLiteralNode(null, litls.Value, litls.Language));

                case Token.BLANKNODEWITHID:
                    // Blanks accept any Blank
                    return new BlankNodePattern(t.Value.Substring(2));

                case Token.KEYWORDA:
                    return new NodeMatchPattern(new UriNode(null, UriFactory.Create(NamespaceMapper.RDF + "type")));

                default:
                    throw ParserHelper.Error("Unable to Convert a '" + t.GetType().ToString() + "' to a Pattern Item in a Triple Pattern", t);
            }
        }

        private Uri ResolveQName(SparqlQueryParserContext context, String qname)
        {
            return UriFactory.Create(Tools.ResolveQName(qname, context.Query.NamespaceMap, context.Query.BaseUri));
        }

        private bool IsProjectableExpression(SparqlQueryParserContext context, ISparqlExpression expr, List<String> projectedSoFar)
        {
            if (expr.Type == SparqlExpressionType.Aggregate) return true;
            if (expr.Type == SparqlExpressionType.Primary)
            {
                return expr.Variables.All(v => context.Query.GroupBy.ProjectableVariables.Contains(v) || projectedSoFar.Contains(v));
            }
            else
            {
                return expr.Arguments.All(arg => IsProjectableExpression(context, arg, projectedSoFar));
            }
        }

        /// <summary>
        /// Constructs an error message that informs the user about unexpected excess tokens in a SPARQL qery
        /// </summary>
        /// <param name="context">Current parser context</param>
        /// <param name="expectedCount">The expected number of tokens</param>
        /// <returns></returns>
        public string ExcessTokensString(SparqlQueryParserContext context, int expectedCount)
        {
            var builder = new StringBuilder();
            builder.Append("The following excess tokens were ecountered from Line ");

            List<IToken> excessTokens = new List<IToken>();
            while (context.LocalTokens.Count > expectedCount)
            {
                excessTokens.Add(context.LocalTokens.Pop());
            }
            excessTokens.Reverse();

            IToken first = excessTokens[0];
            builder.AppendLine(first.StartLine + " Column " + first.StartPosition + " onwards:");
            for (int i = 0; i < excessTokens.Count; i++)
            {
                builder.Append(excessTokens[i].Value);
                builder.Append(' ');
            }
            builder.AppendLine();
            builder.Append("You may be missing some syntax to divide these tokens into multiple triple patterns");
            return builder.ToString();
        }
        
        #endregion
    }
}