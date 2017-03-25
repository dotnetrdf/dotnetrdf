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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Aggregates
{
    /// <summary>
    /// Interface for SPARQL Aggregates which can be used to calculate aggregates over Results
    /// </summary>
    public interface ISparqlAggregate
    {
        /// <summary>
        /// Applies the Aggregate to the Result Binder and returns a single Node as a Result
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        IValuedNode Apply(SparqlEvaluationContext context);

        /// <summary>
        /// Applies the Aggregate to the Result Binder and returns a single Node as a Result
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Enumerable of Binding IDs which the aggregate is applied over</param>
        IValuedNode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs);

        /// <summary>
        /// Gets the Expression that the Aggregate is applied to
        /// </summary>
        ISparqlExpression Expression
        {
            get;
        }

        /// <summary>
        /// Gets the Type of the Aggregate
        /// </summary>
        SparqlExpressionType Type
        {
            get;
        }

        /// <summary>
        /// Gets the URI/Keyword of the Aggregate
        /// </summary>
        String Functor
        {
            get;
        }

        /// <summary>
        /// Gets the Arguments of the Aggregate
        /// </summary>
        IEnumerable<ISparqlExpression> Arguments
        {
            get;
        }
    }
}
