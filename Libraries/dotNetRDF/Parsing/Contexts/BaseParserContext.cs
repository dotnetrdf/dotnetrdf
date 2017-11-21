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

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Base Class for Parser Contexts
    /// </summary>
    public abstract class BaseParserContext
        : IParserContext
    {
        /// <summary>
        /// RDF Handler used to handle the generated RDF
        /// </summary>
        protected IRdfHandler _handler;
        /// <summary>
        /// Is Parsing Traced?
        /// </summary>
        protected bool _traceParsing = false;

        private NestedNamespaceMapper _nsmap = new NestedNamespaceMapper(true);
        private Uri _baseUri;

        /// <summary>
        /// Creates a new Base Parser Context
        /// </summary>
        /// <param name="g">Graph to parse into</param>
        public BaseParserContext(IGraph g)
            : this(g, false) { }

        /// <summary>
        /// Creates a new Base Parser Context
        /// </summary>
        /// <param name="g">Graph to parse into</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        public BaseParserContext(IGraph g, bool traceParsing)
            : this(new GraphHandler(g), traceParsing) { }

        /// <summary>
        /// Creates a new Base Parser Context
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        public BaseParserContext(IRdfHandler handler)
            : this(handler, false) { }

        /// <summary>
        /// Creates a new Base Parser Context
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        public BaseParserContext(IRdfHandler handler, bool traceParsing)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            _handler = handler;
            _traceParsing = traceParsing;

            _baseUri = _handler.GetBaseUri();
        }

        /// <summary>
        /// Gets the Handler used to handle the generated RDF
        /// </summary>
        public IRdfHandler Handler
        {
            get
            {
                return _handler;
            }
        }

        /// <summary>
        /// Gets/Sets whether to trace parsing
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

        /// <summary>
        /// Gets the Namespace Map for the parsing context
        /// </summary>
        public INestedNamespaceMapper Namespaces
        {
            get
            {
                return _nsmap;
            }
        }

        /// <summary>
        /// Gets the Base URI for the parsing context
        /// </summary>
        public Uri BaseUri
        {
            get
            {
                return _baseUri;
            }
            set
            {
                _baseUri = value;
            }
        }
    }

    /// <summary>
    /// Class for Parser Contexts for Tokeniser based Parsing
    /// </summary>
    public class TokenisingParserContext
        : BaseParserContext, ITokenisingParserContext
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
        /// <param name="g">Graph to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        public TokenisingParserContext(IGraph g, ITokeniser tokeniser)
            : base(g)
        {
            _queue = new BufferedTokenQueue(tokeniser);
        }

        /// <summary>
        /// Creates a new Tokenising Parser Context with custom settings
        /// </summary>
        /// <param name="g">Graph to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        public TokenisingParserContext(IGraph g, ITokeniser tokeniser, TokenQueueMode queueMode)
            : base(g)
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
        /// <param name="g">Graph to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TokenisingParserContext(IGraph g, ITokeniser tokeniser, bool traceParsing, bool traceTokeniser)
            : this(g, tokeniser)
        {
            _traceParsing = traceParsing;
            _traceTokeniser = traceTokeniser;
            _queue.Tracing = _traceTokeniser;
        }

        /// <summary>
        /// Creates a new Tokenising Parser Context with custom settings
        /// </summary>
        /// <param name="g">Graph to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TokenisingParserContext(IGraph g, ITokeniser tokeniser, TokenQueueMode queueMode, bool traceParsing, bool traceTokeniser)
            : base(g, traceParsing)
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
        /// <param name="handler">RDF Handler</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        public TokenisingParserContext(IRdfHandler handler, ITokeniser tokeniser)
            : base(handler)
        {
            _queue = new BufferedTokenQueue(tokeniser);
        }

        /// <summary>
        /// Creates a new Tokenising Parser Context with custom settings
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        public TokenisingParserContext(IRdfHandler handler, ITokeniser tokeniser, TokenQueueMode queueMode)
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
        /// <param name="handler">RDF Handler</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TokenisingParserContext(IRdfHandler handler, ITokeniser tokeniser, bool traceParsing, bool traceTokeniser)
            : this(handler, tokeniser)
        {
            _traceParsing = traceParsing;
            _traceTokeniser = traceTokeniser;
            _queue.Tracing = _traceTokeniser;
        }

        /// <summary>
        /// Creates a new Tokenising Parser Context with custom settings
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TokenisingParserContext(IRdfHandler handler, ITokeniser tokeniser, TokenQueueMode queueMode, bool traceParsing, bool traceTokeniser)
            : base(handler, traceParsing)
        {
            switch (queueMode)
            {
                case TokenQueueMode.AsynchronousBufferDuringParsing:
                    _queue = new AsynchronousBufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.QueueAllBeforeParsing:
                    _queue = new TokenQueue(tokeniser);
                    break;
                case TokenQueueMode.SynchronousBufferDuringParsing:
                default:
                    _queue = new BufferedTokenQueue(tokeniser);
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
