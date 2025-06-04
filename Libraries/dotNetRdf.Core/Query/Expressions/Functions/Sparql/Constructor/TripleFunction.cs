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

using System;
using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;

/// <summary>
/// Class representing the SPARQL-Star TRIPLE() function.
/// </summary>
public class TripleFunction : ISparqlExpression
{
    /// <summary>
    /// Get the first argument to this function that specifies the subject of the triple.
    /// </summary>
    public ISparqlExpression SubjectExpression { get; }
    /// <summary>
    /// Get the second argument to this function that specifies the predicate of the triple.
    /// </summary>
    public ISparqlExpression PredicateExpression { get; }
    /// <summary>
    /// Get the third argument to this function that specifies the object of the triple.
    /// </summary>
    public ISparqlExpression ObjectExpression { get; }

    /// <summary>
    /// Create a new TRIPLE function expression.
    /// </summary>
    /// <param name="subjectExpression">Subject argument expression.</param>
    /// <param name="predicateExpression">Predicate argument expression.</param>
    /// <param name="objectExpression">Object argument expression.</param>
    public TripleFunction(ISparqlExpression subjectExpression, ISparqlExpression predicateExpression,
        ISparqlExpression objectExpression)
    {
        SubjectExpression = subjectExpression;
        PredicateExpression = predicateExpression;
        ObjectExpression = objectExpression;
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlExpressionVisitor<T> visitor)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public IEnumerable<string> Variables => SubjectExpression.Variables.Concat(PredicateExpression.Variables)
        .Concat(ObjectExpression.Variables);

    /// <inheritdoc />
    public SparqlExpressionType Type => SparqlExpressionType.Function;

    /// <inheritdoc />
    public string Functor => SparqlSpecsHelper.SparqlStarKeywordTriple;

    /// <inheritdoc />
    public IEnumerable<ISparqlExpression> Arguments =>
        new[] { SubjectExpression, PredicateExpression, ObjectExpression };

    /// <inheritdoc />
    public ISparqlExpression Transform(IExpressionTransformer transformer)
    {
        return new TripleFunction(transformer.Transform(SubjectExpression),
            transformer.Transform(PredicateExpression),
            transformer.Transform(ObjectExpression));
    }

    /// <inheritdoc />
    public bool CanParallelise => false;
}
