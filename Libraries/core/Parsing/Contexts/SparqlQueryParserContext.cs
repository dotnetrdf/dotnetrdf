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
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Parser Context for SPARQL Query parser
    /// </summary>
    public class SparqlQueryParserContext : TokenisingParserContext
    {
        private SparqlQuery _query = new SparqlQuery();
        private bool _verbSeen = false;
        private SparqlExpressionParser _exprParser = new SparqlExpressionParser();
        private SparqlPathParser _pathParser = new SparqlPathParser();
        private Dictionary<String, int> _bnodeLabelUsages = new Dictionary<string, int>();
        private int _blankNodeID = 1;
        private int _graphPatternID = 0;
        private Uri _defaultBaseUri = null;
        private bool _subqueryMode = false;
        private SparqlQuerySyntax _syntax = Options.QueryDefaultSyntax;
        private int _nextAliasID = 0;
        private IEnumerable<ISparqlCustomExpressionFactory> _factories = Enumerable.Empty<ISparqlCustomExpressionFactory>();

        /// <summary>
        /// Creates a new SPARQL Query Parser Context with default settings
        /// </summary>
        /// <param name="tokeniser">Tokeniser to use</param>
        public SparqlQueryParserContext(ITokeniser tokeniser)
            : base(new NullHandler(), tokeniser) { }

        /// <summary>
        /// Creates a new SPARQL Query Parser Context with custom settings
        /// </summary>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        public SparqlQueryParserContext(ITokeniser tokeniser, TokenQueueMode queueMode)
            : base(new NullHandler(), tokeniser, queueMode) { }

        /// <summary>
        /// Creates a new SPARQL Query Parser Context with custom settings
        /// </summary>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public SparqlQueryParserContext(ITokeniser tokeniser, bool traceParsing, bool traceTokeniser)
            : base(new NullHandler(), tokeniser, traceParsing, traceTokeniser) { }

        /// <summary>
        /// Creates a new SPARQL Query Parser Context with custom settings
        /// </summary>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public SparqlQueryParserContext(ITokeniser tokeniser, TokenQueueMode queueMode, bool traceParsing, bool traceTokeniser)
            : base(new NullHandler(), tokeniser, queueMode, traceParsing, traceTokeniser) { }

        /// <summary>
        /// Creates a new SPARQL Query Parser Context for parsing sub-queries
        /// </summary>
        /// <param name="parent">Parent Query Parser Context</param>
        /// <param name="tokens">Tokens that need parsing to form a subquery</param>
        protected internal SparqlQueryParserContext(SparqlQueryParserContext parent, ITokenQueue tokens)
            : base(new NullHandler(), null)
        {
            this._traceParsing = parent.TraceParsing;
            this._traceTokeniser = parent.TraceTokeniser;
            this._queue = tokens;
            this._subqueryMode = true;
            this._query = new SparqlQuery(true);
            this._factories = parent.ExpressionFactories;
            this._syntax = parent.SyntaxMode;
            this._exprParser.SyntaxMode = this._syntax;
        }

        /// <summary>
        /// Creates a new Query Parser Context from the given Token Queue
        /// </summary>
        /// <param name="tokens">Token Queue</param>
        protected internal SparqlQueryParserContext(ITokenQueue tokens)
            : base(new NullHandler(), null)
        {
            this._queue = tokens;
            this._query = new SparqlQuery(true);
        }

        /// <summary>
        /// Gets the Query that this Parser Context is populating
        /// </summary>
        public SparqlQuery Query
        {
            get
            {
                return this._query;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Query Verb has been seen
        /// </summary>
        public bool VerbSeen
        {
            get
            {
                return this._verbSeen;
            }
            set
            {
                if (value)
                {
                    this._verbSeen = value;
                }
            }
        }

        /// <summary>
        /// Returns whether this Parser Context is for a sub-query
        /// </summary>
        public bool SubQueryMode
        {
            get
            {
                return this._subqueryMode;
            }
        }

        /// <summary>
        /// Gets/Sets the Syntax that should be supported
        /// </summary>
        public SparqlQuerySyntax SyntaxMode
        {
            get
            {
                return this._syntax;
            }
            set
            {
                this._syntax = value;
                this._exprParser.SyntaxMode = value;
            }
        }

        /// <summary>
        /// Gets/Sets the default Base Uri to resolve relative URIs against
        /// </summary>
        public Uri DefaultBaseUri
        {
            get
            {
                return this._defaultBaseUri;
            }
            set
            {
                this._defaultBaseUri = value;
                this._exprParser.BaseUri = value;
                this._query.BaseUri = value;
            }
        }

        /// <summary>
        /// Gets the Expression Parser
        /// </summary>
        internal SparqlExpressionParser ExpressionParser
        {
            get
            {
                return this._exprParser;
            }
        }

        /// <summary>
        /// Gets the Property Path Parser
        /// </summary>
        internal SparqlPathParser PathParser
        {
            get
            {
                return this._pathParser;
            }
        }

        /// <summary>
        /// Gets/Sets the current Graph Pattern ID
        /// </summary>
        public int GraphPatternID
        {
            get
            {
                return this._graphPatternID;
            }
            set
            {
                this._graphPatternID = value;
            }
        }

        /// <summary>
        /// Gets a new Blank Node ID
        /// </summary>
        /// <returns></returns>
        public String GetNewBlankNodeID()
        {
            String id = "_:sparql-autos" + this._blankNodeID;
            while (this._bnodeLabelUsages.ContainsKey(id))
            {
                this._blankNodeID++;
                id = "_:sparql-autos" + this._blankNodeID;
            }
            this._bnodeLabelUsages.Add(id, this._graphPatternID);

            return id;
        }

        /// <summary>
        /// Gets the mapping of in use Blank Nodes IDs
        /// </summary>
        public Dictionary<String, int> BlankNodeIDUsages
        {
            get
            {
                return this._bnodeLabelUsages;
            }
        }

        /// <summary>
        /// Gets the last Blank Node ID that was issued
        /// </summary>
        public int BlankNodeID
        {
            get
            {
                return this._blankNodeID;
            }
        }

        /// <summary>
        /// Gets the Next Available Alias ID for aliasing Project Expressions and Aggregates which don't have an Aggregate Specified
        /// </summary>
        public int NextAliasID
        {
            get
            {
                int temp = this._nextAliasID;
                this._nextAliasID++;
                return temp;
            }
        }

        /// <summary>
        /// Gets the Custom Expression Factories valid for this Parser
        /// </summary>
        public IEnumerable<ISparqlCustomExpressionFactory> ExpressionFactories
        {
            get
            {
                return this._factories;
            }
            set
            {
                if (value != null)
                {
                    this._factories = value;
                }
            }
        }
    }
}
