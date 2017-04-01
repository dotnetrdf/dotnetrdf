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
using VDS.RDF.Query.Algebra;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// An optimizer that handles a special case for ORDER BY + DISTINCT combinations which can significantly improve performance by eliminating duplicates prior to sorting when the default SPARQL behaviour is to do a potentially costly sort over many duplicates and then eliminate distincts.
    /// </summary>
    /// <remarks>
    /// Only applies to queries which meet the following criteria:
    /// <ul>
    /// <li>Has an ORDER BY and a DISTNCT on the same level of the query</li>
    /// <li>Selects a fixed list of variables i.e. not a SELECT DISTINCT *</li>
    /// <li>All variables used in the ORDER BY expressions also occur in the project list</li>
    /// </ul>
    /// </remarks>
    public class OrderByDistinctOptimiser
        : IAlgebraOptimiser
    {
        /// <summary>
        /// Optimizes the given algebra
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <returns>Optimized algebra</returns>
        public ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
        {
            if (algebra is Distinct)
            {
                Distinct distinct = (Distinct)algebra;
                if (distinct.InnerAlgebra is Select)
                {
                    Select select = (Select)distinct.InnerAlgebra;
                    if (!select.IsSelectAll)
                    {
                        if (select.InnerAlgebra is OrderBy)
                        {
                            bool ok = true;
                            OrderBy orderBy = (OrderBy)select.InnerAlgebra;
                            List<String> projectVars = select.SparqlVariables.Select(v => v.Name).ToList();
                            foreach (String var in orderBy.Ordering.Variables)
                            {
                                if (!projectVars.Contains(var))
                                {
                                    ok = false;
                                    break;
                                }
                            }

                            if (ok)
                            {
                                // Safe to apply the optimization
                                Select newSelect = new Select(orderBy.InnerAlgebra, false, select.SparqlVariables);
                                Distinct newDistinct = new Distinct(newSelect);
                                return new OrderBy(newDistinct, orderBy.Ordering);
                            }

                        }
                    }
                }

                // If we reach here than optimization is not applicable
                return ((Distinct)algebra).Transform(this);
            }
            else if (algebra is Reduced)
            {
                Reduced reduced = (Reduced)algebra;
                if (reduced.InnerAlgebra is Select)
                {
                    Select select = (Select)reduced.InnerAlgebra;
                    if (!select.IsSelectAll)
                    {
                        if (select.InnerAlgebra is OrderBy)
                        {
                            bool ok = true;
                            OrderBy orderBy = (OrderBy)select.InnerAlgebra;
                            List<String> projectVars = select.SparqlVariables.Select(v => v.Name).ToList();
                            foreach (String var in orderBy.Ordering.Variables)
                            {
                                if (!projectVars.Contains(var))
                                {
                                    ok = false;
                                    break;
                                }
                            }

                            if (ok)
                            {
                                // Safe to apply the optimization
                                Select newSelect = new Select(orderBy.InnerAlgebra, false, select.SparqlVariables);
                                Reduced newReduced = new Reduced(newSelect);
                                return new OrderBy(newReduced, orderBy.Ordering);
                            }

                        }
                    }
                }

                // If we reach here than optimization is not applicable
                return ((Reduced)algebra).Transform(this);
            }
            else if (algebra is ITerminalOperator)
            {
                return algebra;
            }
            else if (algebra is IUnaryOperator)
            {
                return ((IUnaryOperator)algebra).Transform(this);
            }
            else if (algebra is IAbstractJoin)
            {
                return ((IAbstractJoin)algebra).Transform(this);
            }
            else
            {
                return algebra;
            }
        }

        /// <summary>
        /// Returns true if the query is a SELECT DISTINCT or SELECT REDUCED and has an ORDER BY
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlQuery q)
        {
            switch (q.QueryType)
            {
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    return q.OrderBy != null;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns that this is not applicable to updates
        /// </summary>
        /// <param name="cmds">Update commands</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlUpdateCommandSet cmds)
        {
            return false;
        }
    }
}
