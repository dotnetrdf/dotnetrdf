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

using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Parser Context class for TriG Parsers
    /// </summary>
    public class TriGParserContext 
        : TokenisingStoreParserContext
    {
        private bool _defaultGraphExists = false;
        private TriGSyntax _syntax = TriGSyntax.MemberSubmission;

        /// <summary>
        /// Creates a new TriG Parser Context with default settings
        /// </summary>
        /// <param name="store">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        public TriGParserContext(ITripleStore store, ITokeniser tokeniser)
            : base(store,tokeniser) { }

        /// <summary>
        /// Creates a new TrigG Parser Context with custom settings
        /// </summary>
        /// <param name="store">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        public TriGParserContext(ITripleStore store, ITokeniser tokeniser, TokenQueueMode queueMode)
            : base(store, tokeniser, queueMode) { }

        /// <summary>
        /// Creates a new TriG Parser Context with custom settings
        /// </summary>
        /// <param name="store">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TriGParserContext(ITripleStore store, ITokeniser tokeniser, bool traceParsing, bool traceTokeniser)
            : this(store, tokeniser, TokenQueueMode.SynchronousBufferDuringParsing, traceParsing, traceTokeniser) { }

        /// <summary>
        /// Creates a new TriG Parser Context with custom settings
        /// </summary>
        /// <param name="store">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TriGParserContext(ITripleStore store, ITokeniser tokeniser, TokenQueueMode queueMode, bool traceParsing, bool traceTokeniser)
            : base(store, tokeniser, queueMode, traceParsing, traceTokeniser) { }

        /// <summary>
        /// Creates a new TriG Parser Context with default settings
        /// </summary>
        /// <param name="handler">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        public TriGParserContext(IRdfHandler handler, ITokeniser tokeniser)
            : base(handler, tokeniser) { }

        /// <summary>
        /// Creates a new TrigG Parser Context with custom settings
        /// </summary>
        /// <param name="handler">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        public TriGParserContext(IRdfHandler handler, ITokeniser tokeniser, TokenQueueMode queueMode)
            : base(handler, tokeniser, queueMode) { }

        /// <summary>
        /// Creates a new TriG Parser Context with custom settings
        /// </summary>
        /// <param name="handler">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TriGParserContext(IRdfHandler handler, ITokeniser tokeniser, bool traceParsing, bool traceTokeniser)
            : this(handler, tokeniser, TokenQueueMode.SynchronousBufferDuringParsing, traceParsing, traceTokeniser) { }

        /// <summary>
        /// Creates a new TriG Parser Context with custom settings
        /// </summary>
        /// <param name="handler">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TriGParserContext(IRdfHandler handler, ITokeniser tokeniser, TokenQueueMode queueMode, bool traceParsing, bool traceTokeniser)
            : base(handler, tokeniser, queueMode, traceParsing, traceTokeniser) { }

        /// <summary>
        /// Gets/Sets whether the Default Graph exists
        /// </summary>
        public bool DefaultGraphExists
        {
            get
            {
                return _defaultGraphExists;
            }
            set
            {
                if (!_defaultGraphExists && value)
                {
                    _defaultGraphExists = value;
                }
            }
        }

        /// <summary>
        /// Gets/Sets the Syntax to be used
        /// </summary>
        public TriGSyntax Syntax
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
    }
}
