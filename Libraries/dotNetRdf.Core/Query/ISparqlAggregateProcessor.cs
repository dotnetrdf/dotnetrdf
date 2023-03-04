/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2023 dotNetRDF Project (http://dotnetrdf.org/)
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
