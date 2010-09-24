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
using VDS.RDF.Storage;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Abstract Base Class for SPARQL Query processors which are not based on the SPARQL Algebra
    /// </summary>
    public abstract class NonAlgebraQueryProcessor : ISparqlQueryProcessor
    {
        /// <summary>
        /// Processes a SPARQL Query
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        public abstract Object ProcessQuery(SparqlQuery query);

        /// <summary>
        /// Processes SPARQL Algebra
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessAlgebra(ISparqlAlgebra algebra)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes an Ask
        /// </summary>
        /// <param name="ask">Ask</param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessAsk(Ask ask)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes a BGP
        /// </summary>
        /// <param name="bgp">BGP</param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessBgp(Bgp bgp)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes a Bindings modifier
        /// </summary>
        /// <param name="b">Bindings</param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessBindings(Bindings b)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes a Distinct modifier
        /// </summary>
        /// <param name="distinct">Distinct modifier</param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessDistinct(Distinct distinct)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes an Exists Join
        /// </summary>
        /// <param name="existsJoin">Exists Join</param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessExistsJoin(ExistsJoin existsJoin)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes a Filter
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessFilter(Filter filter)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes a Graph
        /// </summary>
        /// <param name="graph">Graph</param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessGraph(Algebra.Graph graph)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes a Group By
        /// </summary>
        /// <param name="groupBy">Group By</param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessGroupBy(GroupBy groupBy)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes a Having
        /// </summary>
        /// <param name="having">Having</param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessHaving(Having having)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes a Join
        /// </summary>
        /// <param name="join">Join</param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessJoin(Join join)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes a LeftJoin
        /// </summary>
        /// <param name="leftJoin">Left Join</param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessLeftJoin(LeftJoin leftJoin)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes a Minus
        /// </summary>
        /// <param name="minus">Minus</param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessMinus(Minus minus)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes an Order By
        /// </summary>
        /// <param name="orderBy"></param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessOrderBy(OrderBy orderBy)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes a Projection
        /// </summary>
        /// <param name="project">Projection</param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessProject(Project project)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes a Reduced modifier
        /// </summary>
        /// <param name="reduced">Reduced modifier</param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessReduced(Reduced reduced)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes a Select
        /// </summary>
        /// <param name="select">Select</param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessSelect(Select select)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes a Select Distinct Graphs
        /// </summary>
        /// <param name="selDistGraphs">Select Distinct Graphs</param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessSelectDistinctGraphs(SelectDistinctGraphs selDistGraphs)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes a Service
        /// </summary>
        /// <param name="service">Service</param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessService(Service service)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes a Slice modifier
        /// </summary>
        /// <param name="slice">Slice modifier</param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessSlice(Slice slice)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }

        /// <summary>
        /// Processes a Union
        /// </summary>
        /// <param name="union"></param>
        /// <exception cref="NotSupportedException">Thrown since this Query Processor is not an algebra supporting processor</exception>
        public void ProcessUnion(Union union)
        {
            throw new NotSupportedException("This Query Processor is not an algebra supporting processor");
        }
    }

    /// <summary>
    /// A SPARQL Query Processor where the query is processed by passing it to the <see cref="INativelyQueryableStore.ExecuteQuery">ExecuteQuery()</see> method of an <see cref="INativelyQueryableStore">INativelyQueryableStore</see>
    /// </summary>
    public class SimpleQueryProcessor : NonAlgebraQueryProcessor
    {
        private INativelyQueryableStore _store;

        /// <summary>
        /// Creates a new Simple Query Processor
        /// </summary>
        /// <param name="store">Triple Store</param>
        public SimpleQueryProcessor(INativelyQueryableStore store)
        {
            this._store = store;
        }

        /// <summary>
        /// Processes a SPARQL Query
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        public override object ProcessQuery(SparqlQuery query)
        {
            return _store.ExecuteQuery(query.ToString());
        }
    }

#if !NO_STORAGE

    /// <summary>
    /// A SPARQL Query Processor where the query is processed by passing it to the <see cref="IGenericIOManager.Query">Query()</see> method of an <see cref="IQueryableGenericIOManager">IQueryableGenericIOManager</see>
    /// </summary>
    public class GenericQueryProcessor : NonAlgebraQueryProcessor
    {
        private IQueryableGenericIOManager _manager;

        /// <summary>
        /// Creates a new Generic Query Processor
        /// </summary>
        /// <param name="manager">Generic IO Manager</param>
        public GenericQueryProcessor(IQueryableGenericIOManager manager)
        {
            this._manager = manager;
        }

        /// <summary>
        /// Processes a SPARQL Query
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        public override object ProcessQuery(SparqlQuery query)
        {
            return this._manager.Query(query.ToString());
        }
    }

#endif

    /// <summary>
    /// A SPARQL Query Processor where the query is processed by passing it to a remote SPARQL endpoint
    /// </summary>
    public class RemoteQueryProcessor : NonAlgebraQueryProcessor
    {
        private SparqlRemoteEndpoint _endpoint;

        /// <summary>
        /// Creates a new Remote Query Processor
        /// </summary>
        /// <param name="endpoint">SPARQL Endpoint</param>
        public RemoteQueryProcessor(SparqlRemoteEndpoint endpoint)
        {
            this._endpoint = endpoint;
        }

        /// <summary>
        /// Processes a SPARQL Query
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        public override object ProcessQuery(SparqlQuery query)
        {
            switch (query.QueryType)
            {
                case SparqlQueryType.Ask:
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    return this._endpoint.QueryWithResultSet(query.ToString());
                case SparqlQueryType.Construct:
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    return this._endpoint.QueryWithResultGraph(query.ToString());
                default:
                    throw new RdfQueryException("Unable to execute an unknown query type against a Remote Endpoint");
            }
        }
    }
}
