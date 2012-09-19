using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Parser Context for Turtle parsing
    /// </summary>
    public class TurtleParserContext
        : TokenisingParserContext
    {
        private TurtleSyntax _syntax = TurtleSyntax.W3C;

        /// <summary>
        /// Creates a new Turtle Parser Context with default settings
        /// </summary>
        /// <param name="g">Graph to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        public TurtleParserContext(IGraph g, ITokeniser tokeniser, TurtleSyntax syntax)
            : this(g, tokeniser, syntax, TokenQueueMode.SynchronousBufferDuringParsing, false, false) { }

        /// <summary>
        /// Creates a new Turtle Parser Context with custom settings
        /// </summary>
        /// <param name="g">Graph to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        public TurtleParserContext(IGraph g, ITokeniser tokeniser, TurtleSyntax syntax, TokenQueueMode queueMode)
            : this(g, tokeniser, syntax, queueMode, false, false) { }

        /// <summary>
        /// Creates a new Turtle Parser Context with custom settings
        /// </summary>
        /// <param name="g">Graph to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TurtleParserContext(IGraph g, ITokeniser tokeniser, TurtleSyntax syntax, bool traceParsing, bool traceTokeniser)
            : this(g, tokeniser, syntax, TokenQueueMode.SynchronousBufferDuringParsing, traceParsing, traceTokeniser) { }

        /// <summary>
        /// Creates a new Turtle Parser Context with custom settings
        /// </summary>
        /// <param name="g">Graph to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TurtleParserContext(IGraph g, ITokeniser tokeniser, TurtleSyntax syntax, TokenQueueMode queueMode, bool traceParsing, bool traceTokeniser)
            : base(g, tokeniser, queueMode, traceParsing, traceTokeniser)
        {
            this._syntax = syntax;
        }

        /// <summary>
        /// Creates a new Turtle Parser Context with default settings
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        public TurtleParserContext(IRdfHandler handler, ITokeniser tokeniser, TurtleSyntax syntax)
            : this(handler, tokeniser, syntax, TokenQueueMode.SynchronousBufferDuringParsing, false, false) { }

        /// <summary>
        /// Creates a new Turtle Parser Context with custom settings
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        public TurtleParserContext(IRdfHandler handler, ITokeniser tokeniser, TurtleSyntax syntax, TokenQueueMode queueMode)
            : this(handler, tokeniser, syntax, queueMode, false, false) { }

        /// <summary>
        /// Creates a new Turtle Parser Context with custom settings
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TurtleParserContext(IRdfHandler handler, ITokeniser tokeniser, TurtleSyntax syntax, bool traceParsing, bool traceTokeniser)
            : this(handler, tokeniser, syntax, TokenQueueMode.SynchronousBufferDuringParsing, traceParsing, traceTokeniser) { }

        /// <summary>
        /// Creates a new Turtle Parser Context with custom settings
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TurtleParserContext(IRdfHandler handler, ITokeniser tokeniser, TurtleSyntax syntax, TokenQueueMode queueMode, bool traceParsing, bool traceTokeniser)
            : base(handler, tokeniser, queueMode, traceParsing, traceTokeniser)
        {
            this._syntax = syntax;
        }

        /// <summary>
        /// Gets the Turtle Syntax being used
        /// </summary>
        public TurtleSyntax Syntax
        {
            get
            {
                return this._syntax;
            }
        }
    }
}
