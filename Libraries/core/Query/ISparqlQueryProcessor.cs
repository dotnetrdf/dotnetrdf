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
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Interface for SPARQL Query Processors
    /// </summary>
    /// <remarks>
    /// <para>
    /// A SPARQL Query Processor is a class that knows how to evaluate SPARQL queries against some data source to which the processor has access
    /// </para>
    /// <para>
    /// The point of this interface is to allow for end users to implement custom update processors or to extend and modify the behaviour of the default Leviathan engine as required.
    /// </para>
    /// <para>
    /// Implementations may choose to only implement the parts relevant to them and leave the rest throwing <see cref="NotImplementedException">NotImplementedException</see>'s or similar.  For example a query processor might just implement the <see cref="ProcessQuery">ProcessQuery</see> method and not implement any of the algebra processing methods as it may choose to evaluate the query in it's own way or simply farm the query out to some remote query engine and not need to make/use the algebra transform.
    /// </para>
    /// </remarks>
    public interface ISparqlQueryProcessor
    {
        /// <summary>
        /// Processes SPARQL Algebra
        /// </summary>
        /// <param name="algebra">Algebra</param>
        void ProcessAlgebra(ISparqlAlgebra algebra);

        /// <summary>
        /// Processes an Ask
        /// </summary>
        /// <param name="ask">Ask</param>
        void ProcessAsk(Ask ask);

        /// <summary>
        /// Processes a BGP
        /// </summary>
        /// <param name="bgp">BGP</param>
        void ProcessBgp(Bgp bgp);

        /// <summary>
        /// Processes a Bindings modifier
        /// </summary>
        /// <param name="b">Bindings</param>
        void ProcessBindings(Bindings b);

        /// <summary>
        /// Processes a Distinct modifier
        /// </summary>
        /// <param name="distinct">Distinct modifier</param>
        void ProcessDistinct(Distinct distinct);

        /// <summary>
        /// Processes an Exists Join
        /// </summary>
        /// <param name="existsJoin">Exists Join</param>
        void ProcessExistsJoin(ExistsJoin existsJoin);

        /// <summary>
        /// Processes a Filter
        /// </summary>
        /// <param name="filter">Filter</param>
        void ProcessFilter(Filter filter);

        /// <summary>
        /// Processes a Graph
        /// </summary>
        /// <param name="graph">Graph</param>
        void ProcessGraph(Algebra.Graph graph);

        /// <summary>
        /// Processes a Group By
        /// </summary>
        /// <param name="groupBy">Group By</param>
        void ProcessGroupBy(GroupBy groupBy);

        /// <summary>
        /// Processes a Having
        /// </summary>
        /// <param name="having">Having</param>
        void ProcessHaving(Having having);

        /// <summary>
        /// Processes a Join
        /// </summary>
        /// <param name="join">Join</param>
        void ProcessJoin(Join join);

        /// <summary>
        /// Processes a LeftJoin
        /// </summary>
        /// <param name="leftJoin">Left Join</param>
        void ProcessLeftJoin(LeftJoin leftJoin);

        /// <summary>
        /// Processes a Minus
        /// </summary>
        /// <param name="minus">Minus</param>
        void ProcessMinus(Minus minus);

        /// <summary>
        /// Processes an Order By
        /// </summary>
        /// <param name="orderBy"></param>
        void ProcessOrderBy(OrderBy orderBy);

        /// <summary>
        /// Processes a Projection
        /// </summary>
        /// <param name="project">Projection</param>
        void ProcessProject(Project project);

        /// <summary>
        /// Processes a SPARQL Query
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        Object ProcessQuery(SparqlQuery query);

        /// <summary>
        /// Processes a Reduced modifier
        /// </summary>
        /// <param name="reduced">Reduced modifier</param>
        void ProcessReduced(Reduced reduced);

        /// <summary>
        /// Processes a Select
        /// </summary>
        /// <param name="select">Select</param>
        void ProcessSelect(Select select);

        /// <summary>
        /// Processes a Select Distinct Graphs
        /// </summary>
        /// <param name="selDistGraphs">Select Distinct Graphs</param>
        void ProcessSelectDistinctGraphs(SelectDistinctGraphs selDistGraphs);

        /// <summary>
        /// Processes a Service
        /// </summary>
        /// <param name="service">Service</param>
        void ProcessService(Service service);

        /// <summary>
        /// Processes a Slice modifier
        /// </summary>
        /// <param name="slice">Slice modifier</param>
        void ProcessSlice(Slice slice);

        /// <summary>
        /// Processes a Union
        /// </summary>
        /// <param name="union"></param>
        void ProcessUnion(Union union);
    }
}
