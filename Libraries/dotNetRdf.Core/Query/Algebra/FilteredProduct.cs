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
using System.Linq;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Optimisation;

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Algebra operator which combines a Filter and a Product into a single operation for improved performance and reduced memory usage.
/// </summary>
public class FilteredProduct
    : IAbstractJoin
{
    /// <summary>
    /// Creates a new Filtered Product.
    /// </summary>
    /// <param name="lhs">LHS Algebra.</param>
    /// <param name="rhs">RHS Algebra.</param>
    /// <param name="expr">Expression to filter with.</param>
    public FilteredProduct(ISparqlAlgebra lhs, ISparqlAlgebra rhs, ISparqlExpression expr)
    {
        Lhs = lhs;
        Rhs = rhs;
        FilterExpression = expr;
    }

    /// <summary>
    /// Gets the LHS Algebra.
    /// </summary>
    public ISparqlAlgebra Lhs { get; }

    /// <summary>
    /// Gets the RHS Algebra.
    /// </summary>
    public ISparqlAlgebra Rhs { get; }

    /// <summary>
    /// Get the filter expression to apply.
    /// </summary>
    public ISparqlExpression FilterExpression { get; }

    /// <summary>
    /// Transforms the inner algebra with the given optimiser.
    /// </summary>
    /// <param name="optimiser">Algebra Optimiser.</param>
    /// <returns></returns>
    public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
    {
        if (optimiser is IExpressionTransformer)
        {
            return new FilteredProduct(optimiser.Optimise(Lhs), optimiser.Optimise(Rhs), ((IExpressionTransformer)optimiser).Transform(FilterExpression));
        }
        else
        {
            return new FilteredProduct(optimiser.Optimise(Lhs), optimiser.Optimise(Rhs), FilterExpression);
        }
    }

    /// <summary>
    /// Transforms the LHS algebra only with the given optimiser.
    /// </summary>
    /// <param name="optimiser">Algebra Optimiser.</param>
    /// <returns></returns>
    public ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
    {
        return new FilteredProduct(optimiser.Optimise(Lhs), Rhs, FilterExpression);
    }

    /// <summary>
    /// Transforms the RHS algebra only with the given optimiser.
    /// </summary>
    /// <param name="optimiser">Algebra Optimiser.</param>
    /// <returns></returns>
    public ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
    {
        return new FilteredProduct(Lhs, optimiser.Optimise(Rhs), FilterExpression);
    }

    /// <summary>
    /// Gets the Variables used in the Algebra.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get
        {
            return Lhs.Variables.Concat(Rhs.Variables).Concat(FilterExpression.Variables).Distinct();
        }
    }

    /// <summary>
    /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FloatingVariables
    {
        get
        {
            // Floating variables are those floating on either side which are not fixed
            IEnumerable<string> floating = Lhs.FloatingVariables.Concat(Rhs.FloatingVariables).Distinct();
            var fixedVars = new HashSet<string>(FixedVariables);
            return floating.Where(v => !fixedVars.Contains(v));
        }
    }

    /// <summary>
    /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FixedVariables
    {
        get
        {
            // Fixed variables are those fixed on either side
            return Lhs.FixedVariables.Concat(Rhs.FixedVariables).Distinct();
        }
    }

    /// <summary>
    /// Converts the algebra back into a query.
    /// </summary>
    /// <returns></returns>
    public SparqlQuery ToQuery()
    {
        ISparqlAlgebra algebra = new Filter(new Join(Lhs, Rhs), new UnaryExpressionFilter(FilterExpression));
        return algebra.ToQuery();
    }

    /// <summary>
    /// Converts the algebra back into a Graph Pattern.
    /// </summary>
    /// <returns></returns>
    public Patterns.GraphPattern ToGraphPattern()
    {
        ISparqlAlgebra algebra = new Filter(new Join(Lhs, Rhs), new UnaryExpressionFilter(FilterExpression));
        return algebra.ToGraphPattern();
    }

    /// <summary>
    /// Gets the string representation of the algebra.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "FilteredProduct(" + Lhs + ", " + Rhs + ", " + FilterExpression + ")";
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessUnknownOperator(this, context);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitUnknownOperator(this);
    }
}
