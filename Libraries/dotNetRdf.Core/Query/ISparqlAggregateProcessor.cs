/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.Query;

/// <summary>
/// The interface to be implemented by a processor for the SPARQL aggregate operations.
/// </summary>
/// <typeparam name="TResult">The type of result returned by a process method.</typeparam>
/// <typeparam name="TContext">The type of the context object provided to a process method.</typeparam>
/// <typeparam name="TBinding">The type of a the variable bindings provided to a process method.</typeparam>
public interface ISparqlAggregateProcessor<out TResult, in TContext, in TBinding>
{
    /// <summary>
    /// Return the average over the aggregated values.
    /// </summary>
    /// <param name="average"></param>
    /// <param name="context"></param>
    /// <param name="bindings"></param>
    /// <returns></returns>
    TResult ProcessAverage(AverageAggregate average, TContext context, IEnumerable<TBinding> bindings);
    /// <summary>
    /// Return the count of the aggregated values.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="context"></param>
    /// <param name="bindings"></param>
    /// <returns></returns>
    TResult ProcessCount(CountAggregate count, TContext context, IEnumerable<TBinding> bindings);
    /// <summary>
    /// Return the count of the number of bindings.
    /// </summary>
    /// <param name="countAll"></param>
    /// <param name="context"></param>
    /// <param name="bindings"></param>
    /// <returns></returns>
    TResult ProcessCountAll(CountAllAggregate countAll, TContext context, IEnumerable<TBinding> bindings);
    /// <summary>
    /// Return the count of the number of unique bindings.
    /// </summary>
    /// <param name="countAllDistinct"></param>
    /// <param name="context"></param>
    /// <param name="bindings"></param>
    /// <returns></returns>
    TResult ProcessCountAllDistinct(CountAllDistinctAggregate countAllDistinct, TContext context, IEnumerable<TBinding> bindings);
    /// <summary>
    /// Return the count of distinct values in the aggregation.
    /// </summary>
    /// <param name="countDistinct"></param>
    /// <param name="context"></param>
    /// <param name="bindings"></param>
    /// <returns></returns>
    TResult ProcessCountDistinct(CountDistinctAggregate countDistinct, TContext context, IEnumerable<TBinding> bindings);
    /// <summary>
    /// Return the grouped concatenation of the bindings.
    /// </summary>
    /// <param name="groupConcat"></param>
    /// <param name="context"></param>
    /// <param name="bindings"></param>
    /// <returns></returns>
    TResult ProcessGroupConcat(GroupConcatAggregate groupConcat, TContext context, IEnumerable<TBinding> bindings);
    /// <summary>
    /// Return the highest value in the aggregation.
    /// </summary>
    /// <param name="max"></param>
    /// <param name="context"></param>
    /// <param name="bindings"></param>
    /// <returns></returns>
    TResult ProcessMax(MaxAggregate max, TContext context, IEnumerable<TBinding> bindings);
    /// <summary>
    /// Return the lowest value in the aggregation.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="context"></param>
    /// <param name="bindings"></param>
    /// <returns></returns>
    TResult ProcessMin(MinAggregate min, TContext context, IEnumerable<TBinding> bindings);
    /// <summary>
    /// Return a sample value from the aggregation.
    /// </summary>
    /// <param name="sample"></param>
    /// <param name="context"></param>
    /// <param name="bindings"></param>
    /// <returns></returns>
    TResult ProcessSample(SampleAggregate sample, TContext context, IEnumerable<TBinding> bindings);
    /// <summary>
    /// Return the sum of the aggregated values.
    /// </summary>
    /// <param name="sum"></param>
    /// <param name="context"></param>
    /// <param name="bindings"></param>
    /// <returns></returns>
    TResult ProcessSum(SumAggregate sum, TContext context, IEnumerable<TBinding> bindings);
    
    // XPath Aggregates
    /// <summary>
    /// Return the string concatenation of the aggregated values.
    /// </summary>
    /// <param name="stringJoin"></param>
    /// <param name="context"></param>
    /// <param name="bindings"></param>
    /// <returns></returns>
    TResult ProcessStringJoin(StringJoinAggregate stringJoin, TContext context, IEnumerable<TBinding> bindings);
    
    // DotNetRDF-specific aggregates
    /// <summary>
    /// Return true if the inner expression is true for all bindings, false otherwise.
    /// </summary>
    /// <param name="all"></param>
    /// <param name="context"></param>
    /// <param name="bindings"></param>
    /// <returns></returns>
    TResult ProcessAll(AllAggregate all, TContext context, IEnumerable<TBinding> bindings);
    /// <summary>
    /// Return true if the inner expression is true for at least one binding, false otherwise.
    /// </summary>
    /// <param name="any"></param>
    /// <param name="context"></param>
    /// <param name="bindings"></param>
    /// <returns></returns>
    TResult ProcessAny(AnyAggregate any, TContext context, IEnumerable<TBinding> bindings);
    /// <summary>
    /// Return the median of the values returned by the inner expression.
    /// </summary>
    /// <param name="median"></param>
    /// <param name="context"></param>
    /// <param name="bindings"></param>
    /// <returns></returns>
    TResult ProcessMedian(MedianAggregate median,TContext context, IEnumerable<TBinding> bindings);
    /// <summary>
    /// Return the mathematical mode of the values returned by the inner expression.
    /// </summary>
    /// <param name="mode"></param>
    /// <param name="context"></param>
    /// <param name="bindings"></param>
    /// <returns></returns>
    TResult ProcessMode(ModeAggregate mode, TContext context, IEnumerable<TBinding> bindings);
    /// <summary>
    /// Return true if the inner expression returns false for all bindings, false otherwise.
    /// </summary>
    /// <param name="none"></param>
    /// <param name="context"></param>
    /// <param name="bindings"></param>
    /// <returns></returns>
    TResult ProcessNone(NoneAggregate none, TContext context, IEnumerable<TBinding> bindings);
    /// <summary>
    /// Convert the result of the inner expression to a numeric value and return the lowest value returned from all bindings.
    /// </summary>
    /// <param name="numericMax"></param>
    /// <param name="context"></param>
    /// <param name="bindings"></param>
    /// <returns></returns>
    TResult ProcessNumericMax(NumericMaxAggregate numericMax, TContext context, IEnumerable<TBinding> bindings);
    /// <summary>
    /// Convert the result of the inner expression to a numeric value and return the highest value returned from all bindings.
    /// </summary>
    /// <param name="numericMin"></param>
    /// <param name="context"></param>
    /// <param name="bindings"></param>
    /// <returns></returns>
    TResult ProcessNumericMin(NumericMinAggregate numericMin, TContext context, IEnumerable<TBinding> bindings);

}
