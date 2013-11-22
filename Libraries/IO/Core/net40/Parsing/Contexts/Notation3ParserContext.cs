/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Graphs;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Parser Context for Notation 3 Parsers
    /// </summary>
    public class Notation3ParserContext 
        : TokenisingParserContext
    {
        private bool _keywordsMode = false;
        private readonly List<String> _keywords = new List<string>();
        private readonly Stack<IGraph> _subgraphs = new Stack<IGraph>();
        private IGraph _g;
        private readonly Stack<IRdfHandler> _handlers = new Stack<IRdfHandler>();
        
        /// <summary>
        /// Creates a new Notation 3 Parser Context with default settings
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        public Notation3ParserContext(IRdfHandler handler, ITokeniser tokeniser)
            : base(handler, tokeniser) { }

        /// <summary>
        /// Creates a new Notation 3 Parser Context with custom settings
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        public Notation3ParserContext(IRdfHandler handler, ITokeniser tokeniser, TokenQueueMode queueMode)
            : base(handler, tokeniser, queueMode) { }

        /// <summary>
        /// Creates a new Notation 3 Parser Context with custom settings
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public Notation3ParserContext(IRdfHandler handler, ITokeniser tokeniser, bool traceParsing, bool traceTokeniser)
            : base(handler, tokeniser, traceParsing, traceTokeniser) { }

        /// <summary>
        /// Creates a new Notation 3 Parser Context with custom settings
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public Notation3ParserContext(IRdfHandler handler, ITokeniser tokeniser, TokenQueueMode queueMode, bool traceParsing, bool traceTokeniser)
            : base(handler, tokeniser, queueMode, traceParsing, traceTokeniser) { }

        /// <summary>
        /// Gets/Sets whether Keywords Mode is in use
        /// </summary>
        public bool KeywordsMode
        {
            get
            {
                return this._keywordsMode;
            }
            set
            {
                //Can only turn Keywords Mode on, once on it cannot be turned off for the current input
                if (!this._keywordsMode)
                {
                    this._keywordsMode = value;
                }
            }
        }

        /// <summary>
        /// Gets the list of in-use Keywords
        /// </summary>
        public List<String> Keywords
        {
            get
            {
                return this._keywords;
            }
        }

        /// <summary>
        /// Pushes the current in-scope Graph onto the Graph stack and creates a new empty Graph to be the in-scope Graph
        /// </summary>
        /// <remarks>
        /// Used for Graph Literal parsing - Base Uri and Namespace Maps of the outermost Graph is propogated to the innermost Graph
        /// </remarks>
        public void PushGraph()
        {
            Graph h = new Graph();
            h.Namespaces.Import(this.Namespaces);

            this._handlers.Push(this.Handler);
            this.Handler = new GraphHandler(h);
            this.Handler.StartRdf();

            this._subgraphs.Push(this._g);
            this._g = h;
        }

        /// <summary>
        /// Pops a Graph from the Graph stack to become the in-scope Graph
        /// </summary>
        /// <remarks>
        /// Used for Graph Literal parsing
        /// </remarks>
        public void PopGraph()
        {
            if (this._handlers.Count > 0)
            {
                this._g = this._subgraphs.Pop();
                this.Handler.EndRdf(true);
                this.Handler = this._handlers.Pop();
            }
            else
            {
                throw new RdfParseException("Cannot pop a RDF Handler from the Parser Context to become the in-scope RDF Handler since there are no RDF Handlers on the stack");
            }
        }

        /// <summary>
        /// Gets the current sub-graph (if any)
        /// </summary>
        public IGraph SubGraph
        {
            get
            {
                return this._g;
            }
        }

        /// <summary>
        /// Gets whether the Context is currently for a Graph Literal
        /// </summary>
        public bool GraphLiteralMode
        {
            get
            {
                return (this._handlers.Count > 0);
            }
        }
    }
}
