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
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Parser Context for SPARQL Query parser
    /// </summary>
    public class SparqlQueryParserContext : TokenisingParserContext
    {
        private SparqlQuery _query = new SparqlQuery();
        private bool _verbSeen = false;
        private SparqlExpressionParser _exprParser = new SparqlExpressionParser();
        private SparqlPathParser _pathParser = new SparqlPathParser();
        private Dictionary<String, int> _bnodeLabelUsages = new Dictionary<string, int>();
        private int _blankNodeID = 1;
        private int _graphPatternID = 0;
        private Uri _defaultBaseUri = null;
        private bool _subqueryMode = false;
        private SparqlQuerySyntax _syntax = Options.QueryDefaultSyntax;
        private int _nextAliasID = 0;
        private IEnumerable<ISparqlCustomExpressionFactory> _factories = Enumerable.Empty<ISparqlCustomExpressionFactory>();
        private bool _checkBNodeScope = true;

        /// <summary>
        /// Creates a new SPARQL Query Parser Context with default settings
        /// </summary>
        /// <param name="tokeniser">Tokeniser to use</param>
        public SparqlQueryParserContext(ITokeniser tokeniser)
            : base(new NullHandler(), tokeniser) { }

        /// <summary>
        /// Creates a new SPARQL Query Parser Context with custom settings
        /// </summary>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        public SparqlQueryParserContext(ITokeniser tokeniser, TokenQueueMode queueMode)
            : base(new NullHandler(), tokeniser, queueMode) { }

        /// <summary>
        /// Creates a new SPARQL Query Parser Context with custom settings
        /// </summary>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public SparqlQueryParserContext(ITokeniser tokeniser, bool traceParsing, bool traceTokeniser)
            : base(new NullHandler(), tokeniser, traceParsing, traceTokeniser) { }

        /// <summary>
        /// Creates a new SPARQL Query Parser Context with custom settings
        /// </summary>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public SparqlQueryParserContext(ITokeniser tokeniser, TokenQueueMode queueMode, bool traceParsing, bool traceTokeniser)
            : base(new NullHandler(), tokeniser, queueMode, traceParsing, traceTokeniser) { }

        /// <summary>
        /// Creates a new SPARQL Query Parser Context for parsing sub-queries
        /// </summary>
        /// <param name="parent">Parent Query Parser Context</param>
        /// <param name="tokens">Tokens that need parsing to form a subquery</param>
        protected internal SparqlQueryParserContext(SparqlQueryParserContext parent, ITokenQueue tokens)
            : base(new NullHandler(), null)
        {
            _traceParsing = parent.TraceParsing;
            _traceTokeniser = parent.TraceTokeniser;
            _queue = tokens;
            _subqueryMode = true;
            _query = new SparqlQuery(true);
            _factories = parent.ExpressionFactories;
            _syntax = parent.SyntaxMode;
            _exprParser.SyntaxMode = _syntax;
        }

        /// <summary>
        /// Creates a new Query Parser Context from the given Token Queue
        /// </summary>
        /// <param name="tokens">Token Queue</param>
        protected internal SparqlQueryParserContext(ITokenQueue tokens)
            : base(new NullHandler(), null)
        {
            _queue = tokens;
            _query = new SparqlQuery(true);
        }

        /// <summary>
        /// Gets the Query that this Parser Context is populating
        /// </summary>
        public SparqlQuery Query
        {
            get
            {
                return _query;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Query Verb has been seen
        /// </summary>
        public bool VerbSeen
        {
            get
            {
                return _verbSeen;
            }
            set
            {
                if (value)
                {
                    _verbSeen = value;
                }
            }
        }

        /// <summary>
        /// Returns whether this Parser Context is for a sub-query
        /// </summary>
        public bool SubQueryMode
        {
            get
            {
                return _subqueryMode;
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
                _exprParser.SyntaxMode = value;
            }
        }

        /// <summary>
        /// Gets/Sets the default Base Uri to resolve relative URIs against
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
                _exprParser.BaseUri = value;
                _query.BaseUri = value;
            }
        }

        /// <summary>
        /// Gets the Expression Parser
        /// </summary>
        internal SparqlExpressionParser ExpressionParser
        {
            get
            {
                return _exprParser;
            }
        }

        /// <summary>
        /// Gets the Property Path Parser
        /// </summary>
        internal SparqlPathParser PathParser
        {
            get
            {
                return _pathParser;
            }
        }

        /// <summary>
        /// Gets/Sets the current Graph Pattern ID
        /// </summary>
        public int GraphPatternID
        {
            get
            {
                return _graphPatternID;
            }
            set
            {
                _graphPatternID = value;
            }
        }

        /// <summary>
        /// Gets a new Blank Node ID
        /// </summary>
        /// <returns></returns>
        public String GetNewBlankNodeID()
        {
            String id = "_:sparql-autos" + _blankNodeID;
            while (_bnodeLabelUsages.ContainsKey(id))
            {
                _blankNodeID++;
                id = "_:sparql-autos" + _blankNodeID;
            }
            _bnodeLabelUsages.Add(id, _graphPatternID);

            return id;
        }

        /// <summary>
        /// Gets the mapping of in use Blank Nodes IDs
        /// </summary>
        public Dictionary<String, int> BlankNodeIDUsages
        {
            get
            {
                return _bnodeLabelUsages;
            }
        }

        /// <summary>
        /// Gets the last Blank Node ID that was issued
        /// </summary>
        public int BlankNodeID
        {
            get
            {
                return _blankNodeID;
            }
        }

        /// <summary>
        /// Gets/Sets whether Blank Node scoping must be checked
        /// </summary>
        /// <remarks>
        /// If false then only name tracking will be done to prevent auto-generated IDs colliding with user allocated IDs
        /// </remarks>
        public bool CheckBlankNodeScope
        {
            get
            {
                return _checkBNodeScope;
            }
            set
            {
                _checkBNodeScope = value;
            }
        }

        /// <summary>
        /// Gets the Next Available Alias ID for aliasing Project Expressions and Aggregates which don't have an Aggregate Specified
        /// </summary>
        public int NextAliasID
        {
            get
            {
                int temp = _nextAliasID;
                _nextAliasID++;
                return temp;
            }
        }

        /// <summary>
        /// Gets the Custom Expression Factories valid for this Parser
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
    }
}
