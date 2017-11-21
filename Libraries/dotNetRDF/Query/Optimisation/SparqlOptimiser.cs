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

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// Static Helper class which provides global registry of Algebra Optimisers and the global Query Optimiser
    /// </summary>
    public static class SparqlOptimiser
    {
        private static IQueryOptimiser _queryOpt = new DefaultOptimiser();
        private static List<IAlgebraOptimiser> _algebraOpt = new List<IAlgebraOptimiser>()
        {
            // Optimise to insert Property Functions first - this is always a no-op if none registered
            new PropertyFunctionOptimiser(),
            // Optimise for Lazy Evaluation
            new AskBgpOptimiser(),
            new LazyBgpOptimiser(),
            // Optimise into Strict Algebra - makes further optimisations easier to do
            new StrictAlgebraOptimiser(),
            // Optimise ORDER BY + DISTINCT/REDUCED combinations
            new OrderByDistinctOptimiser(),
            // Optimise for special filter constructs which improve performance
            new IdentityFilterOptimiser(),
            new ImplicitJoinOptimiser(),
            new FilteredProductOptimiser(),
        };

        /// <summary>
        /// Namespace URI for the Optimiser Statistics vocabulary
        /// </summary>
        public const String OptimiserStatsNamespace = "http://www.dotnetrdf.org/optimiserStats#";

        /// <summary>
        /// Gets/Sets the global Query Optimiser that is used by default
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> May be overridden by the Optimiser setting on a SparqlQueryParser
        /// </para>
        /// <para>
        /// Unlike previous releases a Query may be reoptimised using a different optimiser if desired by calling the <see cref="SparqlQuery.Optimise()">Optimise()</see> method again and providing a different Optimiser.  This may not always fully reoptimise the query since the first optimisation will have caused any Filters and Assignments to be placed in the Triple Pattern
        /// </para>
        /// <para>
        /// <em>Warning:</em> Setting this to null has no effect, to disable automatic optimisation use the global property <see cref="Options.QueryOptimisation">Options.QueryOptimisation</see>.  Even with this option disabled a Query can still be optimised manually by calling its <see cref="SparqlQuery.Optimise()">Optimise()</see> method.
        /// </para>
        /// </remarks>
        public static IQueryOptimiser QueryOptimiser
        {
            get
            {
                return _queryOpt;
            }
            set
            {
                if (value != null)
                {
                    _queryOpt = value;
                }
            }
        }

        /// <summary>
        /// Gets the global Algebra Optimisers that are in use
        /// </summary>
        /// <remarks>
        /// <para>
        /// Unlike Query Optimisation multiple Algebra Optimisations may apply.  Algebra optimisers may also be specified and apply locally by the use of the relevant properties on the <see cref="VDS.RDF.Parsing.SparqlQueryParser">SparqlQueryParser</see> and <see cref="SparqlQuery">SparqlQuery</see> classes.  Those specified on a parser will automatically be passed through to all queries parsed by the parser.  Locally specified optimisers apply prior to globally specified ones.
        /// </para>
        /// </remarks>
        public static IEnumerable<IAlgebraOptimiser> AlgebraOptimisers
        {
            get
            {
                return _algebraOpt;
            }
        }

        /// <summary>
        /// Adds a new Algebra Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        public static void AddOptimiser(IAlgebraOptimiser optimiser)
        {
            _algebraOpt.Add(optimiser);
        }

        /// <summary>
        /// Removes an Algebra Optimiser
        /// </summary>
        /// <param name="optimiser"></param>
        public static void RemoveOptimiser(IAlgebraOptimiser optimiser)
        {
            _algebraOpt.Remove(optimiser);
        }

        /// <summary>
        /// Resets Optimisers to default settings
        /// </summary>
        public static void ResetOptimisers()
        {
            if (!_queryOpt.GetType().Equals(typeof(DefaultOptimiser)))
            {
                _queryOpt = new DefaultOptimiser();
            }
            _algebraOpt = new List<IAlgebraOptimiser>()
            {
                new AskBgpOptimiser(),
                new LazyBgpOptimiser(),
            };
        }
    }
}
