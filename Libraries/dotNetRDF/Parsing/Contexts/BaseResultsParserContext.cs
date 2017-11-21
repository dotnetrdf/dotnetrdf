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
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Base class for SPARQL Results Parser Contexts
    /// </summary>
    public class BaseResultsParserContext 
        : IResultsParserContext
    {
        private ISparqlResultsHandler _handler;
        private List<String> _variables = new List<string>();
        /// <summary>
        /// Controls parser tracing behaviour
        /// </summary>
        protected bool _traceParsing = false;

        /// <summary>
        /// Creates a new Results Parser Context
        /// </summary>
        /// <param name="results">Result Set</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        public BaseResultsParserContext(SparqlResultSet results, bool traceParsing)
            : this(new ResultSetHandler(results), traceParsing) { }

        /// <summary>
        /// Creates a new Results Parser Context
        /// </summary>
        /// <param name="results">Result Set</param>
        public BaseResultsParserContext(SparqlResultSet results)
            : this(results, false) { }

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="handler">Results Handler</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        public BaseResultsParserContext(ISparqlResultsHandler handler, bool traceParsing)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            _handler = handler;
            _traceParsing = traceParsing;
        }

        /// <summary>
        /// Creates a new Results Parser Context
        /// </summary>
        /// <param name="handler">Results Handler</param>
        public BaseResultsParserContext(ISparqlResultsHandler handler)
            : this(handler, false) { }

        /// <summary>
        /// Gets the Results Handler to be used
        /// </summary>
        public ISparqlResultsHandler Handler
        {
            get
            {
                return _handler;
            }
        }

        /// <summary>
        /// Gets the Variables that have been seen
        /// </summary>
        public List<String> Variables
        {
            get
            {
                return _variables;
            }
        }

        /// <summary>
        /// Gets/Sets whether Parser Tracing is used
        /// </summary>
        public bool TraceParsing
        {
            get
            {
                return _traceParsing;
            }
            set
            {
                _traceParsing = value;
            }
        }
    }

    /// <summary>
    /// Class for Tokenising SPARQL Results Parser Contexts
    /// </summary>
    public class TokenisingResultParserContext
        : BaseResultsParserContext
    {
        /// <summary>
        /// Tokeniser
        /// </summary>
        protected ITokenQueue _queue;
        /// <summary>
        /// Is Tokeniser traced?
        /// </summary>
        protected bool _traceTokeniser = false;
        /// <summary>
        /// Local Tokens
        /// </summary>
        protected Stack<IToken> _localTokens;

        /// <summary>
        /// Creates a new Tokenising Parser Context with default settings
        /// </summary>
        /// <param name="results">Result Set to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        public TokenisingResultParserContext(SparqlResultSet results, ITokeniser tokeniser)
            : base(results)
        {
            _queue = new BufferedTokenQueue(tokeniser);
        }

        /// <summary>
        /// Creates a new Tokenising Parser Context with custom settings
        /// </summary>
        /// <param name="results">Result Set to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        public TokenisingResultParserContext(SparqlResultSet results, ITokeniser tokeniser, TokenQueueMode queueMode)
            : base(results)
        {
            switch (queueMode)
            {
                case TokenQueueMode.AsynchronousBufferDuringParsing:
                    _queue = new AsynchronousBufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.SynchronousBufferDuringParsing:
                    _queue = new BufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.QueueAllBeforeParsing:
                default:
                    _queue = new TokenQueue(tokeniser);
                    break;
            }
        }

        /// <summary>
        /// Creates a new Tokenising Parser Context with custom settings
        /// </summary>
        /// <param name="results">Result Set to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TokenisingResultParserContext(SparqlResultSet results, ITokeniser tokeniser, bool traceParsing, bool traceTokeniser)
            : this(results, tokeniser)
        {
            _traceParsing = traceParsing;
            _traceTokeniser = traceTokeniser;
            _queue.Tracing = _traceTokeniser;
        }

        /// <summary>
        /// Creates a new Tokenising Parser Context with custom settings
        /// </summary>
        /// <param name="results">Result Set to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TokenisingResultParserContext(SparqlResultSet results, ITokeniser tokeniser, TokenQueueMode queueMode, bool traceParsing, bool traceTokeniser)
            : base(results, traceParsing)
        {
            switch (queueMode)
            {
                case TokenQueueMode.AsynchronousBufferDuringParsing:
                    _queue = new AsynchronousBufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.SynchronousBufferDuringParsing:
                    _queue = new BufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.QueueAllBeforeParsing:
                default:
                    _queue = new TokenQueue(tokeniser);
                    break;
            }
            _traceTokeniser = traceTokeniser;
            _queue.Tracing = _traceTokeniser;
        }

        /// <summary>
        /// Creates a new Tokenising Parser Context with default settings
        /// </summary>
        /// <param name="handler">Results Handler</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        public TokenisingResultParserContext(ISparqlResultsHandler handler, ITokeniser tokeniser)
            : base(handler)
        {
            _queue = new BufferedTokenQueue(tokeniser);
        }

        /// <summary>
        /// Creates a new Tokenising Parser Context with custom settings
        /// </summary>
        /// <param name="handler">Results Handler</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        public TokenisingResultParserContext(ISparqlResultsHandler handler, ITokeniser tokeniser, TokenQueueMode queueMode)
            : base(handler)
        {
            switch (queueMode)
            {
                case TokenQueueMode.AsynchronousBufferDuringParsing:
                    _queue = new AsynchronousBufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.SynchronousBufferDuringParsing:
                    _queue = new BufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.QueueAllBeforeParsing:
                default:
                    _queue = new TokenQueue(tokeniser);
                    break;
            }
        }

        /// <summary>
        /// Creates a new Tokenising Parser Context with custom settings
        /// </summary>
        /// <param name="handler">Results Handler</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TokenisingResultParserContext(ISparqlResultsHandler handler, ITokeniser tokeniser, bool traceParsing, bool traceTokeniser)
            : this(handler, tokeniser)
        {
            _traceParsing = traceParsing;
            _traceTokeniser = traceTokeniser;
            _queue.Tracing = _traceTokeniser;
        }

        /// <summary>
        /// Creates a new Tokenising Parser Context with custom settings
        /// </summary>
        /// <param name="handler">Results Handler</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TokenisingResultParserContext(ISparqlResultsHandler handler, ITokeniser tokeniser, TokenQueueMode queueMode, bool traceParsing, bool traceTokeniser)
            : base(handler, traceParsing)
        {
            switch (queueMode)
            {
                case TokenQueueMode.AsynchronousBufferDuringParsing:
                    _queue = new AsynchronousBufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.SynchronousBufferDuringParsing:
                    _queue = new BufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.QueueAllBeforeParsing:
                default:
                    _queue = new TokenQueue(tokeniser);
                    break;
            }
            _traceTokeniser = traceTokeniser;
            _queue.Tracing = _traceTokeniser;
        }

        /// <summary>
        /// Gets the Token Queue
        /// </summary>
        public ITokenQueue Tokens
        {
            get
            {
                return _queue;
            }
        }

        /// <summary>
        /// Gets the Local Tokens stack
        /// </summary>
        public Stack<IToken> LocalTokens
        {
            get
            {
                if (_localTokens == null) _localTokens = new Stack<IToken>();
                return _localTokens;
            }
        }

        /// <summary>
        /// Gets/Sets whether tokeniser tracing is used
        /// </summary>
        public bool TraceTokeniser
        {
            get
            {
                return _traceTokeniser;
            }
            set
            {
                _traceTokeniser = value;
            }
        }
    }
}
