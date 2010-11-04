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
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Default SPARQL Query Processor provided by the library's Leviathan SPARQL Engine
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Leviathan Query Processor simply invokes the <see cref="ISparqlAlgebra">Evaluate</see> method of the SPARQL Algebra it is asked to process
    /// </para>
    /// </remarks>
    public class LeviathanQueryProcessor : ISparqlQueryProcessor
    {
        private SparqlEvaluationContext _context;

        /// <summary>
        /// Creates a new Leviathan Query Processor
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="store">Triple Store</param>
        public LeviathanQueryProcessor(SparqlQuery query, IInMemoryQueryableStore store)
            : this(query, new InMemoryDataset(store)) { }

        public LeviathanQueryProcessor(SparqlQuery query, ISparqlDataset data)
        {
            this._context = new SparqlEvaluationContext(query, data);
        }

        /// <summary>
        /// Creates a new Leviathan Query Processor
        /// </summary>
        /// <param name="store">Triple Store</param>
        public LeviathanQueryProcessor(IInMemoryQueryableStore store)
            : this(new InMemoryDataset(store)) { }

        public LeviathanQueryProcessor(ISparqlDataset data)
        {
            this._context = new SparqlEvaluationContext(null, data);
        }

        /// <summary>
        /// Processes SPARQL Algebra
        /// </summary>
        /// <param name="algebra">Algebra</param>
        public void ProcessAlgebra(ISparqlAlgebra algebra)
        {
            algebra.Evaluate(this._context);
        }

        /// <summary>
        /// Processes an Ask
        /// </summary>
        /// <param name="ask">Ask</param>
        public void ProcessAsk(Ask ask)
        {
            ask.Evaluate(this._context);
        }

        /// <summary>
        /// Processes a BGP
        /// </summary>
        /// <param name="bgp">BGP</param>
        public void ProcessBgp(IBgp bgp)
        {
            bgp.Evaluate(this._context);
        }

        /// <summary>
        /// Processes a Bindings modifier
        /// </summary>
        /// <param name="b">Bindings</param>
        public void ProcessBindings(Bindings b)
        {
            b.Evaluate(this._context);
        }

        /// <summary>
        /// Processes a Distinct modifier
        /// </summary>
        /// <param name="distinct">Distinct modifier</param>
        public void ProcessDistinct(Distinct distinct)
        {
            distinct.Evaluate(this._context);
        }

        /// <summary>
        /// Processes an Exists Join
        /// </summary>
        /// <param name="existsJoin">Exists Join</param>
        public void ProcessExistsJoin(IExistsJoin existsJoin)
        {
            existsJoin.Evaluate(this._context);
        }

        /// <summary>
        /// Processes a Filter
        /// </summary>
        /// <param name="filter">Filter</param>
        public void ProcessFilter(Filter filter)
        {
            filter.Evaluate(this._context);
        }

        /// <summary>
        /// Processes a Graph
        /// </summary>
        /// <param name="graph">Graph</param>
        public void ProcessGraph(Algebra.Graph graph)
        {
            graph.Evaluate(this._context);
        }

        /// <summary>
        /// Processes a Group By
        /// </summary>
        /// <param name="groupBy">Group By</param>
        public void ProcessGroupBy(GroupBy groupBy)
        {
            groupBy.Evaluate(this._context);
        }

        /// <summary>
        /// Processes a Having
        /// </summary>
        /// <param name="having">Having</param>
        public void ProcessHaving(Having having)
        {
            having.Evaluate(this._context);
        }

        /// <summary>
        /// Processes a Join
        /// </summary>
        /// <param name="join">Join</param>
        public void ProcessJoin(IJoin join)
        {
            join.Evaluate(this._context);
        }

        /// <summary>
        /// Processes a LeftJoin
        /// </summary>
        /// <param name="leftJoin">Left Join</param>
        public void ProcessLeftJoin(ILeftJoin leftJoin)
        {
            leftJoin.Evaluate(this._context);
        }

        /// <summary>
        /// Processes a Minus
        /// </summary>
        /// <param name="minus">Minus</param>
        public void ProcessMinus(IMinus minus)
        {
            minus.Evaluate(this._context);
        }

        /// <summary>
        /// Processes an Order By
        /// </summary>
        /// <param name="orderBy"></param>
        public void ProcessOrderBy(OrderBy orderBy)
        {
            orderBy.Evaluate(this._context);
        }

        /// <summary>
        /// Processes a Projection
        /// </summary>
        /// <param name="project">Projection</param>
        public void ProcessProject(Project project)
        {
            project.Evaluate(this._context);
        }

        /// <summary>
        /// Processes a SPARQL Query
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        public Object ProcessQuery(SparqlQuery query)
        {
            return query.Evaluate(this._context.Data);
        }

        /// <summary>
        /// Processes a Reduced modifier
        /// </summary>
        /// <param name="reduced">Reduced modifier</param>
        public void ProcessReduced(Reduced reduced)
        {
            reduced.Evaluate(this._context);
        }

        /// <summary>
        /// Processes a Select
        /// </summary>
        /// <param name="select">Select</param>
        public void ProcessSelect(Select select)
        {
            select.Evaluate(this._context);
        }

        /// <summary>
        /// Processes a Select Distinct Graphs
        /// </summary>
        /// <param name="selDistGraphs">Select Distinct Graphs</param>
        public void ProcessSelectDistinctGraphs(SelectDistinctGraphs selDistGraphs)
        {
            selDistGraphs.Evaluate(this._context);
        }

        /// <summary>
        /// Processes a Service
        /// </summary>
        /// <param name="service">Service</param>
        public void ProcessService(Service service)
        {
            service.Evaluate(this._context);
        }

        /// <summary>
        /// Processes a Slice modifier
        /// </summary>
        /// <param name="slice">Slice modifier</param>
        public void ProcessSlice(Slice slice)
        {
            slice.Evaluate(this._context);
        }

        /// <summary>
        /// Processes a Union
        /// </summary>
        /// <param name="union"></param>
        public void ProcessUnion(IUnion union)
        {
            union.Evaluate(this._context);
        }
    }
}
