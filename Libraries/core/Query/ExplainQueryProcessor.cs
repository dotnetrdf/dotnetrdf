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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query
{
    public class ExplainQueryProcessor : LeviathanQueryProcessor
    {
        private ThreadSafeValue<int> _depthCounter;
        private TextWriter _writer;

        public ExplainQueryProcessor(ISparqlDataset dataset, TextWriter output)
            : base(dataset)
        {
            this._depthCounter = new ThreadSafeValue<int>(() => 0);
            this._writer = output;
        }

        public ExplainQueryProcessor(IInMemoryQueryableStore store, TextWriter output)
            : this(new InMemoryDataset(store), output) { }

        private void ExplainEvaluationStart(ISparqlAlgebra algebra, SparqlEvaluationContext context)
        {
            this._depthCounter.Value++;

            StringBuilder output = new StringBuilder();
            output.Append("[Thread ID " + Thread.CurrentThread.ManagedThreadId + "]");
            output.Append(" Depth " + this._depthCounter.Value);
            output.Append(" " + algebra.GetType().FullName);
            output.Append(" Start");

            this._writer.WriteLine(output);
        }

        private void ExplainEvaluationEnd(ISparqlAlgebra algebra, SparqlEvaluationContext context)
        {
            this._depthCounter.Value--;

            StringBuilder output = new StringBuilder();
            output.Append("[Thread ID " + Thread.CurrentThread.ManagedThreadId + "]");
            output.Append(" Depth " + this._depthCounter.Value);
            output.Append(" " + algebra.GetType().FullName);
            output.Append(" End");

            this._writer.WriteLine(output);
        }

        private BaseMultiset ExplainAndEvaluate<T>(T algebra, SparqlEvaluationContext context, Func<T, SparqlEvaluationContext, BaseMultiset> evaluator)
            where T : ISparqlAlgebra
        {
            this.ExplainEvaluationStart(algebra, context);
            BaseMultiset results = evaluator(algebra, context);
            this.ExplainEvaluationEnd(algebra, context);
            return results;
        }

        public override BaseMultiset ProcessAsk(Ask ask, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Ask>(ask, context, base.ProcessAsk);
        }

        public override BaseMultiset ProcessBgp(IBgp bgp, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<IBgp>(bgp, context, base.ProcessBgp);
        }

        public override BaseMultiset ProcessBindings(Bindings b, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Bindings>(b, context, base.ProcessBindings);
        }

        public override BaseMultiset ProcessDistinct(Distinct distinct, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Distinct>(distinct, context, base.ProcessDistinct);
        }

        public override BaseMultiset ProcessExistsJoin(IExistsJoin existsJoin, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<IExistsJoin>(existsJoin, context, base.ProcessExistsJoin);
        }

        public override BaseMultiset ProcessFilter(Filter filter, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Filter>(filter, context, base.ProcessFilter);
        }

        public override BaseMultiset ProcessGraph(VDS.RDF.Query.Algebra.Graph graph, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Algebra.Graph>(graph, context, base.ProcessGraph);
        }

        public override BaseMultiset ProcessGroupBy(GroupBy groupBy, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<GroupBy>(groupBy, context, base.ProcessGroupBy);
        }

        public override BaseMultiset ProcessHaving(Having having, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Having>(having, context, base.ProcessHaving);
        }

        public override BaseMultiset ProcessJoin(IJoin join, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<IJoin>(join, context, base.ProcessJoin);
        }

        public override BaseMultiset ProcessLeftJoin(ILeftJoin leftJoin, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<ILeftJoin>(leftJoin, context, base.ProcessLeftJoin);
        }

        public override BaseMultiset ProcessMinus(IMinus minus, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<IMinus>(minus, context, base.ProcessMinus);
        }

        public override BaseMultiset ProcessNegatedPropertySet(NegatedPropertySet negPropSet, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<NegatedPropertySet>(negPropSet, context, base.ProcessNegatedPropertySet);
        }

        public override BaseMultiset ProcessOneOrMorePath(OneOrMorePath path, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<OneOrMorePath>(path, context, base.ProcessOneOrMorePath);
        }

        public override BaseMultiset ProcessOrderBy(OrderBy orderBy, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<OrderBy>(orderBy, context, base.ProcessOrderBy);
        }

        public override BaseMultiset ProcessProject(Project project, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Project>(project, context, base.ProcessProject);
        }

        public override BaseMultiset ProcessReduced(Reduced reduced, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Reduced>(reduced, context, base.ProcessReduced);
        }

        public override BaseMultiset ProcessSelect(Select select, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Select>(select, context, base.ProcessSelect);
        }

        public override BaseMultiset ProcessSelectDistinctGraphs(SelectDistinctGraphs selDistGraphs, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<SelectDistinctGraphs>(selDistGraphs, context, base.ProcessSelectDistinctGraphs);
        }

        public override BaseMultiset ProcessService(Service service, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Service>(service, context, base.ProcessService);
        }

        public override BaseMultiset ProcessSlice(Slice slice, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<Slice>(slice, context, base.ProcessSlice);
        }

        public override BaseMultiset ProcessUnion(IUnion union, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<IUnion>(union, context, base.ProcessUnion);
        }

        public override BaseMultiset ProcessZeroLengthPath(ZeroLengthPath path, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<ZeroLengthPath>(path, context, base.ProcessZeroLengthPath);
        }

        public override BaseMultiset ProcessZeroOrMorePath(ZeroOrMorePath path, SparqlEvaluationContext context)
        {
            return this.ExplainAndEvaluate<ZeroOrMorePath>(path, context, base.ProcessZeroOrMorePath);
        }
    }
}
