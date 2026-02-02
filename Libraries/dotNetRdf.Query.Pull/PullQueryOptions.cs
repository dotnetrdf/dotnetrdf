/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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

using System.Globalization;
using VDS.RDF.Query.Describe;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Utils.Describe;

namespace VDS.RDF.Query.Pull;

/// <summary>
/// A class encapsulation the runtime options that can be configured for a <see cref="PullQueryProcessor"/>.
/// </summary>
public class PullQueryOptions
{
    /// <summary>
    /// The default query timeout in milliseconds.
    /// </summary>
    public static ulong DefaultQueryExecutionTimeout = 180000;
    
    /// <summary>
    /// Get or set the node comparer to use in evaluation. 
    /// </summary>
    public ISparqlNodeComparer NodeComparer = new SparqlNodeComparer(
        CultureInfo.InvariantCulture, CompareOptions.None);
    
    /// <summary>
    /// Get or set the hard timeout limit for SPARQL query execution (in milliseconds).
    /// </summary>
    /// <remarks>Defaults to <see cref="DefaultQueryExecutionTimeout"/>. When set to 0, no timeout limit is applied to query evaluation.</remarks>
    public ulong QueryExecutionTimeout { get; set; } = DefaultQueryExecutionTimeout;

    /// <summary>
    /// Get or set the optimisers to be applied to a query algebra prior to evaluation.
    /// </summary>
    public IEnumerable<IAlgebraOptimiser> AlgebraOptimisers { get; set; } = new List<IAlgebraOptimiser>();
    
    /// <summary>
    /// Get or set the URI factory to use during query evaluation.
    /// </summary>
    /// <remarks>When querying an in-memory dataset, using the same URI factory as the dataset being queried, or a child of the URI factory of the dataset being queried will save memory.</remarks>
    public IUriFactory UriFactory { get; set; } = VDS.RDF.UriFactory.Root;
    
    /// <summary>
    /// Gets/Sets the <see cref="ISparqlDescribe">ISparqlDescribe</see> which provides the Describe algorithm you wish to use.
    /// </summary>
    /// <remarks>
    /// By default, this will be the <see cref="ConciseBoundedDescription">ConciseBoundedDescription</see> (CBD) algorithm.
    /// </remarks>
    public ISparqlDescribe Describer { get; set; } = new SparqlDescriber(new ConciseBoundedDescription());
    
    /// <summary>
    /// Gets/Sets the <see cref="ISparqlResultFactory"/> which creates the <see cref="ISparqlResult"/> instances
    /// that are passed to a <see cref="ISparqlResultsHandler"/>.
    /// </summary>
    public ISparqlResultFactory SparqlResultFactory { get; set; } = new SparqlResultFactory();
    
    /// <summary>
    /// Get/Sets whether the processor treats the union of all graphs in the store as the default graph.
    /// </summary>
    /// <remarks>If this option is set to <code>true</code> then the default graph for the query will be the union
    /// of all graphs in the store. If this option is set to <code>false</code> (the default) then the default graph
    /// of the query will be the list of graphs named by <see cref="DefaultGraphNames"/>, or the unnamed graph if
    /// <see cref="DefaultGraphNames"/> is null or empty.</remarks>
    public bool UnionDefaultGraph { get; set; }
    
    /// <summary>
    /// Get/Sets the list of names of graphs whose union is the default graph for the query, unless overridden by the
    /// query.
    /// </summary>
    /// <remarks>Any FROM clauses in the processed query will always take precedence over the value specified here.
    /// Specifying the value <code>true</code> for <see cref="UnionDefaultGraph"/> will also take precedence
    /// over this list.</remarks>
    public IEnumerable<IRefNode>? DefaultGraphNames { get; set; }
}