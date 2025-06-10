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
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.PropertyFunctions;

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Algebra Operator which provides full text query capabilities for a query.
/// </summary>
/// <remarks>
/// The evaluation of this operator simply registers the search provider with the Evaluation Context such that any <see cref="FullTextMatchPropertyFunction"/> instances are honoured.
/// </remarks>
public class FullTextQuery
    : IUnaryOperator, ILeviathanAlgebraExtension
{
    private readonly IFullTextSearchProvider _provider;

    /// <summary>
    /// Creates a new Full Text Query algebra.
    /// </summary>
    /// <param name="searchProvider">Search Provider.</param>
    /// <param name="algebra">Inner Algebra.</param>
    public FullTextQuery(IFullTextSearchProvider searchProvider, ISparqlAlgebra algebra)
    {
        _provider = searchProvider;
        InnerAlgebra = algebra;
    }

    /// <summary>
    /// Gets the Inner Algebra.
    /// </summary>
    public ISparqlAlgebra InnerAlgebra
    {
        get;
    }

    /// <summary>
    /// Transforms the algebra.
    /// </summary>
    /// <param name="optimiser">Optimiser.</param>
    /// <returns></returns>
    public ISparqlAlgebra Transform(Optimisation.IAlgebraOptimiser optimiser)
    {
        return new FullTextQuery(_provider, optimiser.Optimise(InnerAlgebra));
    }

    /// <summary>
    /// Evaluates the algebra.
    /// </summary>
    /// <param name="context">Evaluation Context.</param>
    /// <returns></returns>
    public BaseMultiset Evaluate(SparqlEvaluationContext context)
    {
        context[FullTextHelper.ContextKey] = _provider;
        return context.Evaluate(InnerAlgebra);
    }

    /// <summary>
    /// Gets the variables used in the algebra.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get 
        {
            return InnerAlgebra.Variables;
        }
    }

    /// <summary>
    /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FloatingVariables { get { return InnerAlgebra.FloatingVariables; } }

    /// <summary>
    /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FixedVariables { get { return InnerAlgebra.FixedVariables; } } 

    /// <summary>
    /// Converts the algebra into a query.
    /// </summary>
    /// <returns></returns>
    public SparqlQuery ToQuery()
    {
        return InnerAlgebra.ToQuery();
    }

    /// <summary>
    /// Converts the algebra into a Graph Pattern.
    /// </summary>
    /// <returns></returns>
    public Patterns.GraphPattern ToGraphPattern()
    {
        return InnerAlgebra.ToGraphPattern();
    }

    /// <summary>
    /// Gets the string representation of the algebra.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "FullTextQuery(" + InnerAlgebra + ")";
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitUnknownOperator(this);
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessUnknownOperator(this, context);
    }
}
