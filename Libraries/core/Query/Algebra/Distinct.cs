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
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a Distinct modifier on a SPARQL Query
    /// </summary>
    public class Distinct : IUnaryOperator
    {
        private ISparqlAlgebra _pattern;

        /// <summary>
        /// Creates a new Distinct Modifier
        /// </summary>
        /// <param name="pattern">Pattern</param>
        public Distinct(ISparqlAlgebra pattern)
        {
            this._pattern = pattern;
        }

        /// <summary>
        /// Evaluates the Distinct Modifier
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            context.InputMultiset = context.Evaluate(this._pattern);//this._pattern.Evaluate(context);

            if (context.InputMultiset is IdentityMultiset || context.InputMultiset is NullMultiset)
            {
                context.OutputMultiset = context.InputMultiset;
                return context.OutputMultiset;
            }
            else
            {
                context.OutputMultiset = new Multiset(context.InputMultiset.Variables);
                IEnumerable<ISet> sets = context.InputMultiset.Sets.Distinct();
                foreach (ISet s in context.InputMultiset.Sets.Distinct())
                {
                    context.OutputMultiset.Add(s);
                }
                return context.OutputMultiset;
            }
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return this._pattern.Variables;
            }
        }

        /// <summary>
        /// Gets the Inner Algebra
        /// </summary>
        public ISparqlAlgebra InnerAlgebra
        {
            get
            {
                return this._pattern;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Distinct(" + this._pattern.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = this._pattern.ToQuery();
            switch (q.QueryType)
            {
                case SparqlQueryType.Select:
                    q.QueryType = SparqlQueryType.SelectDistinct;
                    break;
                case SparqlQueryType.SelectAll:
                    q.QueryType = SparqlQueryType.SelectAllDistinct;
                    break;
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    throw new NotSupportedException("Cannot convert a Distinct() to a SPARQL Query when the Inner Algebra converts to a Query that already has a DISTINCT/REDUCED modifier applied");
                case SparqlQueryType.Ask:
                case SparqlQueryType.Construct:
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    throw new NotSupportedException("Cannot convert a Distinct() to a SPARQL Query when the Inner Algebra converts to a Query that is not a SELECT query");
                case SparqlQueryType.Unknown:
                    q.QueryType = SparqlQueryType.SelectDistinct;
                    break;
                default:
                    throw new NotSupportedException("Cannot convert a Distinct() to a SPARQL Query when the Inner Algebra converts to a Query with an unexpected Query Type");
            }
            return q;
        }

        /// <summary>
        /// Throws an exception since a Distinct() cannot be converted back to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown since a Distinct() cannot be converted to a Graph Pattern</exception>
        public GraphPattern ToGraphPattern()
        {
            throw new NotSupportedException("A Distinct() cannot be converted to a GraphPattern");
        }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new Distinct(this._pattern);
        }
    }

    /// <summary>
    /// Represents a Reduced modifier on a SPARQL Query
    /// </summary>
    public class Reduced : IUnaryOperator
    {
        private ISparqlAlgebra _pattern;

        /// <summary>
        /// Creates a new Reduced Modifier
        /// </summary>
        /// <param name="pattern">Pattern</param>
        public Reduced(ISparqlAlgebra pattern)
        {
            this._pattern = pattern;
        }

        /// <summary>
        /// Evaluates the Reduced Modifier
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            context.InputMultiset = context.Evaluate(this._pattern);//this._pattern.Evaluate(context);

            if (context.InputMultiset is IdentityMultiset || context.InputMultiset is NullMultiset)
            {
                context.OutputMultiset = context.InputMultiset;
                return context.OutputMultiset;
            }
            else
            {
                if (context.Query.Limit > 0)
                {
                    context.OutputMultiset = new Multiset(context.InputMultiset.Variables);
                    foreach (ISet s in context.InputMultiset.Sets.Distinct())
                    {
                        context.OutputMultiset.Add(s);
                    }
                }
                else
                {
                    context.OutputMultiset = context.InputMultiset;
                }
                return context.OutputMultiset;
            }
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return this._pattern.Variables.Distinct();
            }
        }

        /// <summary>
        /// Gets the Inner Algebra
        /// </summary>
        public ISparqlAlgebra InnerAlgebra
        {
            get
            {
                return this._pattern;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Reduced(" + this._pattern.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = this._pattern.ToQuery();
            switch (q.QueryType)
            {
                case SparqlQueryType.Select:
                    q.QueryType = SparqlQueryType.SelectReduced;
                    break;
                case SparqlQueryType.SelectAll:
                    q.QueryType = SparqlQueryType.SelectAllReduced;
                    break;
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    throw new NotSupportedException("Cannot convert a Reduced() to a SPARQL Query when the Inner Algebra converts to a Query that already has a DISTINCT/REDUCED modifier applied");
                case SparqlQueryType.Ask:
                case SparqlQueryType.Construct:
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    throw new NotSupportedException("Cannot convert a Reduced() to a SPARQL Query when the Inner Algebra converts to a Query that is not a SELECT query");
                case SparqlQueryType.Unknown:
                    q.QueryType = SparqlQueryType.SelectReduced;
                    break;
                default:
                    throw new NotSupportedException("Cannot convert a Reduced() to a SPARQL Query when the Inner Algebra converts to a Query with an unexpected Query Type");
            }
            return q;
        }

        /// <summary>
        /// Throws an exception since a Reduced() cannot be converted back to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown since a Reduced() cannot be converted to a Graph Pattern</exception>
        public GraphPattern ToGraphPattern()
        {
            throw new NotSupportedException("A Reduced() cannot be converted to a GraphPattern");
        }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new Reduced(this._pattern);
        }
    }
}
