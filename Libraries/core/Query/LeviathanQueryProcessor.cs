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
using System.Threading;
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
    /// <para>
    /// In future releases much of the Leviathan Query engine logic will be moved into this class to make it possible for implementors to override specific bits of the algebra processing but this is not possible at this time
    /// </para>
    /// </remarks>
    public class LeviathanQueryProcessor : ISparqlQueryProcessor, ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext>
    {
        private ISparqlDataset _dataset;
#if !NO_RWLOCK
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
#endif

        /// <summary>
        /// Creates a new Leviathan Query Processor
        /// </summary>
        /// <param name="store">Triple Store</param>
        public LeviathanQueryProcessor(IInMemoryQueryableStore store)
            : this(new InMemoryDataset(store)) { }

        /// <summary>
        /// Creates a new Leviathan Query Processor
        /// </summary>
        /// <param name="data">SPARQL Dataset</param>
        public LeviathanQueryProcessor(ISparqlDataset data)
        {
            this._dataset = data;

            if (!this._dataset.UsesUnionDefaultGraph)
            {
                if (!this._dataset.HasGraph(null))
                {
                    //Create the Default unnamed Graph if it doesn't exist and then Flush() the change
                    this._dataset.AddGraph(new Graph());
                    this._dataset.Flush();
                }
            }
        }


        /// <summary>
        /// Processes a SPARQL Query
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        public Object ProcessQuery(SparqlQuery query)
        {
            //Handle the Thread Safety of the Query Evaluation
#if !NO_RWLOCK
            ReaderWriterLockSlim currLock = (this._dataset is IThreadSafeDataset) ? ((IThreadSafeDataset)this._dataset).Lock : this._lock;
            try
            {
                currLock.EnterReadLock();
#endif
                return query.Evaluate(this._dataset);
#if !NO_RWLOCK
            }
            finally
            {
                currLock.ExitReadLock();
            }
#endif
        }

        private SparqlEvaluationContext GetContext()
        {
            return this.GetContext(null);
        }

        private SparqlEvaluationContext GetContext(SparqlQuery q)
        {
            return new SparqlEvaluationContext(q, this._dataset, this.GetProcessorForContext());
        }

        private ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> GetProcessorForContext()
        {
            if (this.GetType().Equals(typeof(LeviathanQueryProcessor)))
            {
                return null;
            }
            else
            {
                return this;
            }
        }

        #region Algebra Processor Implementation

        /// <summary>
        /// Processes SPARQL Algebra
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessAlgebra(ISparqlAlgebra algebra, SparqlEvaluationContext context)
        {
            if (algebra is Ask)
            {
                return this.ProcessAsk((Ask)algebra, context);
            }
            else if (algebra is IBgp)
            {
                return this.ProcessBgp((IBgp)algebra, context);
            }
            else if (algebra is Bindings)
            {
                return this.ProcessBindings((Bindings)algebra, context);
            }
            else if (algebra is Distinct)
            {
                return this.ProcessDistinct((Distinct)algebra, context);
            }
            else if (algebra is IExistsJoin)
            {
                return this.ProcessExistsJoin((IExistsJoin)algebra, context);
            }
            else if (algebra is Filter)
            {
                return this.ProcessFilter((Filter)algebra, context);
            }
            else if (algebra is Algebra.Graph)
            {
                return this.ProcessGraph((Algebra.Graph)algebra, context);
            }
            else if (algebra is GroupBy)
            {
                return this.ProcessGroupBy((GroupBy)algebra, context);
            }
            else if (algebra is Having)
            {
                return this.ProcessHaving((Having)algebra, context);
            }
            else if (algebra is IJoin)
            {
                return this.ProcessJoin((IJoin)algebra, context);
            }
            else if (algebra is ILeftJoin)
            {
                return this.ProcessLeftJoin((ILeftJoin)algebra, context);
            }
            else if (algebra is IMinus)
            {
                return this.ProcessMinus((IMinus)algebra, context);
            }
            else if (algebra is NegatedPropertySet)
            {
                return this.ProcessNegatedPropertySet((NegatedPropertySet)algebra, context);
            }
            else if (algebra is OneOrMorePath)
            {
                return this.ProcessOneOrMorePath((OneOrMorePath)algebra, context);
            }
            else if (algebra is OrderBy)
            {
                return this.ProcessOrderBy((OrderBy)algebra, context);
            }
            else if (algebra is Project)
            {
                return this.ProcessProject((Project)algebra, context);
            } 
            else if (algebra is Reduced)
            {
                return this.ProcessReduced((Reduced)algebra, context);
            }
            else if (algebra is Select)
            {
                return this.ProcessSelect((Select)algebra, context);
            }
            else if (algebra is SelectDistinctGraphs)
            {
                return this.ProcessSelectDistinctGraphs((SelectDistinctGraphs)algebra, context);
            }
            else if (algebra is Service)
            {
                return this.ProcessService((Service)algebra, context);
            }
            else if (algebra is Slice)
            {
                return this.ProcessSlice((Slice)algebra, context);
            }
            else if (algebra is IUnion)
            {
                return this.ProcessUnion((IUnion)algebra, context);
            }
            else if (algebra is ZeroLengthPath)
            {
                return this.ProcessZeroLengthPath((ZeroLengthPath)algebra, context);
            }
            else if (algebra is ZeroOrMorePath)
            {
                return this.ProcessZeroOrMorePath((ZeroOrMorePath)algebra, context);
            }
            else
            {
                //Unknown Algebra
                return algebra.Evaluate(context);
            }
        }

        /// <summary>
        /// Processes an Ask
        /// </summary>
        /// <param name="ask">Ask</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessAsk(Ask ask, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return ask.Evaluate(context);
        }

        /// <summary>
        /// Processes a BGP
        /// </summary>
        /// <param name="bgp">BGP</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessBgp(IBgp bgp, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return bgp.Evaluate(context);
        }

        /// <summary>
        /// Processes a Bindings modifier
        /// </summary>
        /// <param name="b">Bindings</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessBindings(Bindings b, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return b.Evaluate(context);
        }

        /// <summary>
        /// Processes a Distinct modifier
        /// </summary>
        /// <param name="distinct">Distinct modifier</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessDistinct(Distinct distinct, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return distinct.Evaluate(context);
        }

        /// <summary>
        /// Processes an Exists Join
        /// </summary>
        /// <param name="existsJoin">Exists Join</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessExistsJoin(IExistsJoin existsJoin, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return existsJoin.Evaluate(context);
        }

        /// <summary>
        /// Processes a Filter
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessFilter(Filter filter, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return filter.Evaluate(context);
        }

        /// <summary>
        /// Processes a Graph
        /// </summary>
        /// <param name="graph">Graph</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessGraph(Algebra.Graph graph, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return graph.Evaluate(context);
        }

        /// <summary>
        /// Processes a Group By
        /// </summary>
        /// <param name="groupBy">Group By</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessGroupBy(GroupBy groupBy, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return groupBy.Evaluate(context);
        }

        /// <summary>
        /// Processes a Having
        /// </summary>
        /// <param name="having">Having</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessHaving(Having having, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return having.Evaluate(context);
        }

        /// <summary>
        /// Processes a Join
        /// </summary>
        /// <param name="join">Join</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessJoin(IJoin join, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return join.Evaluate(context);
        }

        /// <summary>
        /// Processes a LeftJoin
        /// </summary>
        /// <param name="leftJoin">Left Join</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessLeftJoin(ILeftJoin leftJoin, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return leftJoin.Evaluate(context);
        }

        /// <summary>
        /// Processes a Minus
        /// </summary>
        /// <param name="minus">Minus</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessMinus(IMinus minus, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return minus.Evaluate(context);
        }

        public BaseMultiset ProcessNegatedPropertySet(NegatedPropertySet negPropSet, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return negPropSet.Evaluate(context);
        }

        public BaseMultiset ProcessOneOrMorePath(OneOrMorePath path, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return path.Evaluate(context);
        }

        /// <summary>
        /// Processes an Order By
        /// </summary>
        /// <param name="orderBy"></param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessOrderBy(OrderBy orderBy, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return orderBy.Evaluate(context);
        }

        /// <summary>
        /// Processes a Projection
        /// </summary>
        /// <param name="project">Projection</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessProject(Project project, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return project.Evaluate(context);
        }

        /// <summary>
        /// Processes a Reduced modifier
        /// </summary>
        /// <param name="reduced">Reduced modifier</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessReduced(Reduced reduced, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return reduced.Evaluate(context);
        }

        /// <summary>
        /// Processes a Select
        /// </summary>
        /// <param name="select">Select</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessSelect(Select select, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return select.Evaluate(context);
        }

        /// <summary>
        /// Processes a Select Distinct Graphs
        /// </summary>
        /// <param name="selDistGraphs">Select Distinct Graphs</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessSelectDistinctGraphs(SelectDistinctGraphs selDistGraphs, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return selDistGraphs.Evaluate(context);
        }

        /// <summary>
        /// Processes a Service
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessService(Service service, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return service.Evaluate(context);
        }

        /// <summary>
        /// Processes a Slice modifier
        /// </summary>
        /// <param name="slice">Slice modifier</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessSlice(Slice slice, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return slice.Evaluate(context);
        }

        /// <summary>
        /// Processes a Union
        /// </summary>
        /// <param name="union">Union</param>
        /// <param name="context">SPARQL Evaluation Context</param>
        public BaseMultiset ProcessUnion(IUnion union, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return union.Evaluate(context);
        }

        public BaseMultiset ProcessZeroLengthPath(ZeroLengthPath path, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return path.Evaluate(context);
        }

        public BaseMultiset ProcessZeroOrMorePath(ZeroOrMorePath path, SparqlEvaluationContext context)
        {
            if (context == null) context = this.GetContext();
            return path.Evaluate(context);
        }

        #endregion
    }
}
