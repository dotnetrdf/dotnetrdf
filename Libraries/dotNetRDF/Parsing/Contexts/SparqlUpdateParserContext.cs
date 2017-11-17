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
using VDS.RDF.Update;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Parser Context for SPARQL Update Parser
    /// </summary>
    public class SparqlUpdateParserContext
        : TokenisingParserContext
    {
        private SparqlUpdateCommandSet _commandSet = new SparqlUpdateCommandSet();
        private SparqlQueryParser _queryParser = new SparqlQueryParser();
        private SparqlExpressionParser _exprParser = new SparqlExpressionParser();
        private SparqlPathParser _pathParser = new SparqlPathParser();
        private HashSet<String> _dataBNodes = new HashSet<string>();
        private IEnumerable<ISparqlCustomExpressionFactory> _factories = Enumerable.Empty<ISparqlCustomExpressionFactory>();

        /// <summary>
        /// Creates a new SPARQL Update Parser Context
        /// </summary>
        /// <param name="tokeniser">Tokeniser</param>
        public SparqlUpdateParserContext(ITokeniser tokeniser)
            : base(new NullHandler(), tokeniser) { }

        /// <summary>
        /// Creates a new SPARQL Update Parser Context with custom settings
        /// </summary>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        public SparqlUpdateParserContext(ITokeniser tokeniser, TokenQueueMode queueMode)
            : base(new NullHandler(), tokeniser, queueMode) { }

        /// <summary>
        /// Creates a new SPARQL Update Parser Context with custom settings
        /// </summary>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public SparqlUpdateParserContext(ITokeniser tokeniser, bool traceParsing, bool traceTokeniser)
            : base(new NullHandler(), tokeniser, traceParsing, traceTokeniser) { }

        /// <summary>
        /// Creates a new SPARQL Update Parser Context with custom settings
        /// </summary>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public SparqlUpdateParserContext(ITokeniser tokeniser, TokenQueueMode queueMode, bool traceParsing, bool traceTokeniser)
            : base(new NullHandler(), tokeniser, queueMode, traceParsing, traceTokeniser) { }

        /// <summary>
        /// Gets the Update Command Set that is being populated
        /// </summary>
        public SparqlUpdateCommandSet CommandSet
        {
            get
            {
                return _commandSet;
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
        /// Gets the Path Parser
        /// </summary>
        internal SparqlPathParser PathParser
        {
            get
            {
                return _pathParser;
            }
        }

        /// <summary>
        /// Gets the Query Parser
        /// </summary>
        internal SparqlQueryParser QueryParser
        {
            get
            {
                return _queryParser;
            }
        }

        /// <summary>
        /// Gets the Namespace Map
        /// </summary>
        public NamespaceMapper NamespaceMap
        {
            get
            {
                return _commandSet.NamespaceMap;
            }
        }

        /// <summary>
        /// Gets/Sets the locally scoped custom expression factories
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

        /// <summary>
        /// Gets the set of BNodes used in INSERT DATA commands so far
        /// </summary>
        public HashSet<String> DataBNodes
        {
            get
            {
                return _dataBNodes;
            }
        }
    }
}
