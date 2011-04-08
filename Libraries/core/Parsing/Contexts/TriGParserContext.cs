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
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Parser Context class for TriG Parsers
    /// </summary>
    public class TriGParserContext : TokenisingStoreParserContext
    {
        private bool _defaultGraphExists = false;

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
                return this._defaultGraphExists;
            }
            set
            {
                if (!this._defaultGraphExists && value)
                {
                    this._defaultGraphExists = value;
                }
            }
        }
    }
}
