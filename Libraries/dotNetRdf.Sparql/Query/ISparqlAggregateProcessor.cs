using System.Collections.Generic;
using VDS.RDF.Query.Aggregates.Leviathan;
using VDS.RDF.Query.Aggregates.Sparql;
using VDS.RDF.Query.Aggregates.XPath;

namespace VDS.RDF.Query
{
    public interface ISparqlAggregateProcessor<out TResult, in TContext, in TBinding>
    {
        TResult ProcessAverage(AverageAggregate average, TContext context, IEnumerable<TBinding> bindings);
        TResult ProcessCount(CountAggregate count, TContext context, IEnumerable<TBinding> bindings);
        TResult ProcessCountAll(CountAllAggregate countAll, TContext context, IEnumerable<TBinding> bindings);
        TResult ProcessCountAllDistinct(CountAllDistinctAggregate countAllDistinct, TContext context, IEnumerable<TBinding> bindings);
        TResult ProcessCountDistinct(CountDistinctAggregate countDistinct, TContext context, IEnumerable<TBinding> bindings);
        TResult ProcessGroupConcat(GroupConcatAggregate groupConcat, TContext context, IEnumerable<TBinding> bindings);
        TResult ProcessMax(MaxAggregate max, TContext context, IEnumerable<TBinding> bindings);
        TResult ProcessMin(MinAggregate min, TContext context, IEnumerable<TBinding> bindings);
        TResult ProcessSample(SampleAggregate sample, TContext context, IEnumerable<TBinding> bindings);
        TResult ProcessSum(SumAggregate sum, TContext context, IEnumerable<TBinding> bindings);
        
        // XPath Aggregates
        TResult ProcessStringJoin(StringJoinAggregate stringJoin, TContext context, IEnumerable<TBinding> bindings);
        
        // DotNetRDF-specific aggregates
        TResult ProcessAll(AllAggregate all, TContext context, IEnumerable<TBinding> bindings);
        TResult ProcessAny(AnyAggregate any, TContext context, IEnumerable<TBinding> bindings);
        TResult ProcessMedian(MedianAggregate median,TContext context, IEnumerable<TBinding> bindings);
        TResult ProcessMode(ModeAggregate mode, TContext context, IEnumerable<TBinding> bindings);
        TResult ProcessNone(NoneAggregate none, TContext context, IEnumerable<TBinding> bindings);
        TResult ProcessNumericMax(NumericMaxAggregate numericMax, TContext context, IEnumerable<TBinding> bindings);
        TResult ProcessNumericMin(NumericMinAggregate numericMin, TContext context, IEnumerable<TBinding> bindings);

    }
}
