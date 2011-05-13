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
    public class SparqlUpdateParserContext : TokenisingParserContext
    {
        private SparqlUpdateCommandSet _commandSet = new SparqlUpdateCommandSet();
        private SparqlQueryParser _queryParser = new SparqlQueryParser();
        private SparqlExpressionParser _exprParser = new SparqlExpressionParser();
        private SparqlPathParser _pathParser = new SparqlPathParser();
        //private Uri _baseUri;
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
                return this._commandSet;
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
        /// Gets the Path Parser
        /// </summary>
        internal SparqlPathParser PathParser
        {
            get
            {
                return this._pathParser;
            }
        }

        /// <summary>
        /// Gets the Query Parser
        /// </summary>
        internal SparqlQueryParser QueryParser
        {
            get
            {
                return this._queryParser;
            }
        }

        /// <summary>
        /// Gets the Namespace Map
        /// </summary>
        public NamespaceMapper NamespaceMap
        {
            get
            {
                return this._commandSet.NamespaceMap;
            }
        }

        /// <summary>
        /// Gets/Sets the locally scoped custom expression factories
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
