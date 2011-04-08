/*

Copyright Robert Vesse 2009-11
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
            new AskBgpOptimiser(),
            new LazyBgpOptimiser()
        };

        public const String OptimiserStatsNamespace = "http://www.dotnetrdf.org/optimiserStats#";

        /// <summary>
        /// Gets/Sets the global Query Optimiser that is used by default
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> May be overridden by the Optimiser setting on a SparqlQueryParser
        /// </para>
        /// <para>
        /// Unlike previous releases a Query may not be reoptimised using a different optimiser if desired by calling the <see cref="SparqlQuery.Optimise">Optimise()</see> method again and providing a different Optimiser
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
        /// Unlike Query Optimisation multiple Algebra Optimisations may apply.  Algebra optimisers may also be specified and apply locally by the use of the relevant properties on the <see cref="SparqlQueryParser">SparqlQueryParser</see> and <see cref="SparqlQuery">SparqlQuery</see> classes.  Those specified on a parser will automatically be passed through to all queries parsed by the parser.  Locally specified optimisers apply prior to globally specified ones.
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
                new LazyBgpOptimiser()
            };
        }
    }
}
