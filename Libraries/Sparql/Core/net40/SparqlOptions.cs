using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF
{
    public static class SparqlOptions
    {
        private static long _queryExecutionTimeout = 180000, _updateExecutionTimeout = 180000;
        private static bool _queryOptimisation = true, _algebraOptimisation = true, _unsafeOptimisation = false;
        private static bool _queryAllowUnknownFunctions = true;
        private static bool _rigorousQueryEvaluation = false, _strictOperators = false;

        /// <summary>
        /// Gets/Sets the Hard Timeout limit for SPARQL Query Execution (in milliseconds)
        /// </summary>
        /// <remarks>
        /// This is used to stop SPARQL queries running away and never completing execution, it defaults to 3 mins (180,000 milliseconds)
        /// </remarks>
        public static long QueryExecutionTimeout
        {
            get
            {
                return _queryExecutionTimeout;
            }
            set
            {
                _queryExecutionTimeout = Math.Max(value, 0);
            }
        }

        /// <summary>
        /// Gets/Sets whether Query Optimisation should be used
        /// </summary>
        public static bool QueryOptimisation
        {
            get
            {
                return _queryOptimisation;
            }
            set
            {
                _queryOptimisation = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether Algebra Optimisation should be used
        /// </summary>
        public static bool AlgebraOptimisation
        {
            get
            {
                return _algebraOptimisation;
            }
            set
            {
                _algebraOptimisation = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether some Optimisations considered unsafe can be used
        /// </summary>
        /// <remarks>
        /// <para>
        /// The notion of unsafe optimisations refers to optimisations that can make significant performance improvements to some types of queries but are disabled normally because they may lead to behaviour which does not strictly align with the SPARQL specification.
        /// </para>
        /// <para>
        /// One example of such an optimisation is an implicit join where the optimiser cannot be sure that the variables involved don't represent literals.
        /// </para>
        /// </remarks>
        public static bool UnsafeOptimisation
        {
            get
            {
                return _unsafeOptimisation;
            }
            set
            {
                _unsafeOptimisation = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether functions that can't be parsed into Expressions should be represented by the <see cref="VDS.RDF.Query.Expressions.Functions.UnknownFunction">UnknownFunction</see>
        /// </summary>
        /// <remarks>When set to false a Parser Error will be thrown if the Function cannot be parsed into an Expression</remarks>
        public static bool QueryAllowUnknownFunctions
        {
            get
            {
                return _queryAllowUnknownFunctions;
            }
            set
            {
                _queryAllowUnknownFunctions = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether to use rigorous query evaluation
        /// </summary>
        /// <remarks>
        /// <para>
        /// Rigorous Query evaluation applies more checks to the triples produced by datasets to ensure they actually match the patterns being scanned.  If the underlying index structures are able to guarantee this then rigorous evaluation may be turned off for faster evaluation which it is by default since our default <see cref="TreeIndexedTripleCollection"/> and <see cref="TripleCollection"/> implementations will guarantee this.
        /// </para>
        /// </remarks>
        public static bool RigorousEvaluation
        {
            get
            {
                return _rigorousQueryEvaluation;
            }
            set
            {
                _rigorousQueryEvaluation = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether to use strict operators
        /// </summary>
        /// <remarks>
        /// <para>
        /// Strict Operators refers to the interpretation of certian operators like + and - in SPARQL expression evaluation.  If enabled then the operators will function only as specified in the SPARQL specification, if disabled (which is the default) then certain extensions (which the SPARQL specification allows an implementation to provide) will be allowed e.g. date time arithmetic.
        /// </para>
        /// <para>
        /// The only time you may want to disable this is if you are developing queries locally which you want to ensure are portable to other systems or when running the SPARQL compliance tests.
        /// </para>
        /// </remarks>
        public static bool StrictOperators
        {
            get
            {
                return _strictOperators;
            }
            set
            {
                _strictOperators = value;
            }
        }

#if NET40 && !SILVERLIGHT

        /// <summary>
        /// Gets/Sets whether the query engine will try to use PLinq where applicable to evaluate suitable SPARQL constructs in parallel
        /// </summary>
        /// <remarks>
        /// For the 0.6.1 release onwards this was an experimental feature and disabled by default, from 0.7.0 onwards this is enabled by default
        /// </remarks>
        public static bool UsePLinqEvaluation
        {
            get
            {
                return _usePLinq;
            }
            set
            {
                _usePLinq = value;
            }
        }

#endif

        /// <summary>
        /// Gets/Sets the Hard Timeout limit for SPARQL Update Execution (in milliseconds)
        /// </summary>
        /// <remarks>
        /// This is used to stop SPARQL Updates running away and never completing execution, it defaults to 3 mins (180,000 milliseconds)
        /// </remarks>
        public static long UpdateExecutionTimeout
        {
            get
            {
                return _updateExecutionTimeout;
            }
            set
            {
                _updateExecutionTimeout = Math.Max(0, value);
            }
        }
    }
}
