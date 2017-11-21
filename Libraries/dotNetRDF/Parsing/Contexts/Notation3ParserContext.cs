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
    /// Parser Context for Notation 3 Parsers
    /// </summary>
    public class Notation3ParserContext : TokenisingParserContext
    {
        private bool _keywordsMode = false;
        private List<String> _keywords = new List<string>();
        private Stack<IGraph> _subgraphs = new Stack<IGraph>();
        private IGraph _g;
        private Stack<IRdfHandler> _handlers = new Stack<IRdfHandler>();
        private Stack<VariableContext> _varContexts = new Stack<VariableContext>();
        private VariableContext _varContext = new VariableContext(VariableContextType.None);
        
        /// <summary>
        /// Creates a new Notation 3 Parser Context with default settings
        /// </summary>
        /// <param name="g">Graph to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        public Notation3ParserContext(IGraph g, ITokeniser tokeniser)
            : base(g, tokeniser) { }

        /// <summary>
        /// Creates a new Notation 3 Parser Context with custom settings
        /// </summary>
        /// <param name="g">Graph to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        public Notation3ParserContext(IGraph g, ITokeniser tokeniser, TokenQueueMode queueMode)
            : base(g, tokeniser, queueMode) { }

        /// <summary>
        /// Creates a new Notation 3 Parser Context with custom settings
        /// </summary>
        /// <param name="g">Graph to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public Notation3ParserContext(IGraph g, ITokeniser tokeniser, bool traceParsing, bool traceTokeniser)
            : base(g, tokeniser, traceParsing, traceTokeniser) { }

        /// <summary>
        /// Creates a new Notation 3 Parser Context with custom settings
        /// </summary>
        /// <param name="g">Graph to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public Notation3ParserContext(IGraph g, ITokeniser tokeniser, TokenQueueMode queueMode, bool traceParsing, bool traceTokeniser)
            : base(g, tokeniser, queueMode, traceParsing, traceTokeniser) { }

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
                return _keywordsMode;
            }
            set
            {
                // Can only turn Keywords Mode on, once on it cannot be turned off for the current input
                if (!_keywordsMode)
                {
                    _keywordsMode = value;
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
                return _keywords;
            }
        }

        /// <summary>
        /// Gets the Variable Context for Triples
        /// </summary>
        public VariableContext VariableContext
        {
            get
            {
                return _varContext;
            }
            set
            {
                _varContext = value;
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
            h.NamespaceMap.Import(Namespaces);
            h.BaseUri = BaseUri;

            _handlers.Push(_handler);
            _handler = new GraphHandler(h);
            _handler.StartRdf();

            _subgraphs.Push(_g);
            _g = h;

            VariableContext v = new VariableContext(VariableContextType.None);
            _varContexts.Push(_varContext);
            _varContext = v;
        }

        /// <summary>
        /// Pops a Graph from the Graph stack to become the in-scope Graph
        /// </summary>
        /// <remarks>
        /// Used for Graph Literal parsing
        /// </remarks>
        public void PopGraph()
        {
            if (_handlers.Count > 0)
            {
                _g = _subgraphs.Pop();
                _handler.EndRdf(true);
                _handler = _handlers.Pop();
                _varContext = _varContexts.Pop();
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
                return _g;
            }
        }

        /// <summary>
        /// Gets whether the Context is currently for a Graph Literal
        /// </summary>
        public bool GraphLiteralMode
        {
            get
            {
                return (_handlers.Count > 0);
            }
        }
    }
}
