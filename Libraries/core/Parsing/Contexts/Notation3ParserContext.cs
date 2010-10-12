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
    /// Parser Context for Notation 3 Parsers
    /// </summary>
    public class Notation3ParserContext : TokenisingParserContext
    {
        private bool _keywordsMode = false;
        private List<String> _keywords = new List<string>();
        private Stack<IGraph> _graphs = new Stack<IGraph>();
        private VariableContext _varContext = null;
        
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
        /// Gets the Variable Context for Triples
        /// </summary>
        public VariableContext VariableContext
        {
            get
            {
                return this._varContext;
            }
            set
            {
                this._varContext = value;
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
            h.NamespaceMap.Import(this._g.NamespaceMap);
            h.BaseUri = this._g.BaseUri;
            this._graphs.Push(this._g);
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
            if (this._graphs.Count > 0)
            {
                this._g = this._graphs.Pop();
            }
            else
            {
                throw new RdfParseException("Cannot pop a Graph from the Parser Context to become the in-scope Graph since there are no Graphs on the stack");
            }
        }

        /// <summary>
        /// Gets whether the Context is currently for a Graph Literal
        /// </summary>
        public bool GraphLiteralMode
        {
            get
            {
                return (this._graphs.Count > 0);
            }
        }
    }
}
