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
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Base Class for Store Parser Contexts
    /// </summary>
    public abstract class BaseStoreParserContext
    {
        /// <summary>
        /// Store being parsed into
        /// </summary>
        protected ITripleStore _store;
        /// <summary>
        /// Is Parsing Traced?
        /// </summary>
        protected bool _traceParsing = false;

        /// <summary>
        /// Creates a new Base Store Parser Context
        /// </summary>
        /// <param name="store">Triple Store</param>
        public BaseStoreParserContext(ITripleStore store)
        {
            this._store = store;
        }

        /// <summary>
        /// Creates a new Base Parser Context
        /// </summary>
        /// <param name="store">Triple Store</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        public BaseStoreParserContext(ITripleStore store, bool traceParsing)
            : this(store)
        {
            this._traceParsing = traceParsing;
        }

        /// <summary>
        /// Gets the Store being parsed into
        /// </summary>
        public ITripleStore Store
        {
            get
            {
                return this._store;
            }
        }

        /// <summary>
        /// Gets/Sets whether to trace parsing
        /// </summary>
        public bool TraceParsing
        {
            get
            {
                return this._traceParsing;
            }
            set
            {
                this._traceParsing = value;
            }
        }
    }

    /// <summary>
    /// Class for Store Parser Contexts for Tokeniser based Parsing
    /// </summary>
    public class TokenisingStoreParserContext : BaseStoreParserContext
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
        /// Creates a new Tokenising Store Parser Context with default settings
        /// </summary>
        /// <param name="store">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        public TokenisingStoreParserContext(ITripleStore store, ITokeniser tokeniser)
            : base(store)
        {
            this._queue = new TokenQueue(tokeniser);
        }

        /// <summary>
        /// Creates a new Tokenising Store Parser Context with custom settings
        /// </summary>
        /// <param name="store">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        public TokenisingStoreParserContext(ITripleStore store, ITokeniser tokeniser, TokenQueueMode queueMode)
            : base(store)
        {
            switch (queueMode)
            {
                case TokenQueueMode.AsynchronousBufferDuringParsing:
                    this._queue = new AsynchronousBufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.SynchronousBufferDuringParsing:
                    this._queue = new BufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.QueueAllBeforeParsing:
                default:
                    this._queue = new TokenQueue(tokeniser);
                    break;
            }
        }

        /// <summary>
        /// Creates a new Tokenising Store Parser Context with custom settings
        /// </summary>
        /// <param name="store">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TokenisingStoreParserContext(ITripleStore store, ITokeniser tokeniser, bool traceParsing, bool traceTokeniser)
            : this(store, tokeniser)
        {
            this._traceParsing = traceParsing;
            this._traceTokeniser = traceTokeniser;
            this._queue.Tracing = this._traceTokeniser;
        }

        /// <summary>
        /// Creates a new Tokenising Store Parser Context with custom settings
        /// </summary>
        /// <param name="store">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TokenisingStoreParserContext(ITripleStore store, ITokeniser tokeniser, TokenQueueMode queueMode, bool traceParsing, bool traceTokeniser)
            : base(store, traceParsing)
        {
            switch (queueMode)
            {
                case TokenQueueMode.AsynchronousBufferDuringParsing:
                    this._queue = new AsynchronousBufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.SynchronousBufferDuringParsing:
                    this._queue = new BufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.QueueAllBeforeParsing:
                default:
                    this._queue = new TokenQueue(tokeniser);
                    break;
            }
            this._traceTokeniser = traceTokeniser;
            this._queue.Tracing = this._traceTokeniser;
        }

        /// <summary>
        /// Gets the Token Queue
        /// </summary>
        public ITokenQueue Tokens
        {
            get
            {
                return this._queue;
            }
        }

        /// <summary>
        /// Gets the Local Tokens stack
        /// </summary>
        public Stack<IToken> LocalTokens
        {
            get
            {
                if (this._localTokens == null) this._localTokens = new Stack<IToken>();
                return this._localTokens;
            }
        }

        /// <summary>
        /// Gets/Sets whether tokeniser tracing is used
        /// </summary>
        public bool TraceTokeniser
        {
            get
            {
                return this._traceTokeniser;
            }
            set
            {
                this._traceTokeniser = value;
            }
        }
    }
}
