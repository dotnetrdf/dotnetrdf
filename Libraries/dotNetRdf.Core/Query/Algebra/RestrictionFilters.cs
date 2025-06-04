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
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Abstract Base Class for specialised Filters which restrict the value of a variable to some values.
/// </summary>
public abstract class VariableRestrictionFilter 
    : IFilter
{
    /// <summary>
    /// Creates a new Variable Restriction Filter.
    /// </summary>
    /// <param name="pattern">Algebra the filter applies over.</param>
    /// <param name="var">Variable to restrict on.</param>
    /// <param name="filter">Filter to use.</param>
    public VariableRestrictionFilter(ISparqlAlgebra pattern, string var, ISparqlFilter filter)
    {
        InnerAlgebra = pattern;
        RestrictionVariable = var;
        SparqlFilter = filter;
    }

    /// <summary>
    /// Gets the Variable that this filter restricts the value of.
    /// </summary>
    public string RestrictionVariable { get; }

    /// <summary>
    /// Gets the Variables used in the Algebra.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get
        {
            return InnerAlgebra.Variables.Concat(SparqlFilter.Variables).Distinct();
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
    /// Gets the Filter to be used.
    /// </summary>
    public ISparqlFilter SparqlFilter { get; }

    /// <summary>
    /// Gets the Inner Algebra.
    /// </summary>
    public ISparqlAlgebra InnerAlgebra { get; }

    /// <summary>
    /// Gets the String representation of the FILTER.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var filter = SparqlFilter.ToString();
        filter = filter.Substring(7, filter.Length - 8);
        return GetType().Name + "(" + InnerAlgebra + ", " + filter + ")";
    }

    /// <inheritdoc />
    public abstract TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context);

    /// <inheritdoc />
    public abstract T Accept<T>(ISparqlAlgebraVisitor<T> visitor);

    /// <summary>
    /// Converts the Algebra back to a SPARQL Query.
    /// </summary>
    /// <returns></returns>
    public SparqlQuery ToQuery()
    {
        var q = new SparqlQuery { RootGraphPattern = ToGraphPattern() };
        q.Optimise();
        return q;
    }

    /// <summary>
    /// Converts the Algebra back to a Graph Pattern.
    /// </summary>
    /// <returns></returns>
    public GraphPattern ToGraphPattern()
    {
        var p = InnerAlgebra.ToGraphPattern();
        var f = new GraphPattern();
        f.AddFilter(SparqlFilter);
        p.AddGraphPattern(f);
        return p;
    }

    /// <summary>
    /// Transforms the Inner Algebra using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimiser.</param>
    /// <returns></returns>
    public abstract ISparqlAlgebra Transform(IAlgebraOptimiser optimiser);
}

/// <summary>
/// Abstract Base Class for specialised Filters which restrict the value of a variable to a single value.
/// </summary>
public abstract class SingleValueRestrictionFilter 
    : VariableRestrictionFilter
{
    /// <summary>
    /// Creates a new Single Value Restriction Filter.
    /// </summary>
    /// <param name="pattern">Algebra the filter applies over.</param>
    /// <param name="var">Variable to restrict on.</param>
    /// <param name="term">Value to restrict to.</param>
    /// <param name="filter">Filter to use.</param>
    public SingleValueRestrictionFilter(ISparqlAlgebra pattern, string var, ConstantTerm term, ISparqlFilter filter)
        : base(pattern, var, filter)
    {
        RestrictionValue = term;
    }

    /// <summary>
    /// Gets the Value Restriction which this filter applies.
    /// </summary>
    public ConstantTerm RestrictionValue { get; }

    /// <inheritdoc />
    public override TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessSingleValueRestrictionFilter(this, context);
    }

    /// <inheritdoc />
    public override T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitSingleValueRestrictionFilter(this);
    }
}

/// <summary>
/// Represents a special case Filter where the Filter restricts a variable to just one value i.e. FILTER(?x = &lt;value&gt;).
/// </summary>
public class IdentityFilter 
    : SingleValueRestrictionFilter
{
    /// <summary>
    /// Creates a new Identity Filter.
    /// </summary>
    /// <param name="pattern">Algebra the Filter applies over.</param>
    /// <param name="var">Variable to restrict on.</param>
    /// <param name="term">Expression Term.</param>
    public IdentityFilter(ISparqlAlgebra pattern, string var, ConstantTerm term)
        : base(pattern, var, term, new UnaryExpressionFilter(new EqualsExpression(new VariableTerm(var), term))) { }

    /// <summary>
    /// Transforms the Inner Algebra using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimiser.</param>
    /// <returns></returns>
    public override ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
    {
        if (optimiser is IExpressionTransformer expressionTransformer)
        {
            return new IdentityFilter(optimiser.Optimise(InnerAlgebra), RestrictionVariable, (ConstantTerm)expressionTransformer.Transform(RestrictionValue));
        }

        return new IdentityFilter(optimiser.Optimise(InnerAlgebra), RestrictionVariable, RestrictionValue);
    }
}

/// <summary>
/// Represents a special case Filter where the Filter is supposed to restrict a variable to just one value i.e. FILTER(SAMETERM(?x, &lt;value&gt;)).
/// </summary>
public class SameTermFilter
    : SingleValueRestrictionFilter
{
    /// <summary>
    /// Creates a new Same Term Filter.
    /// </summary>
    /// <param name="pattern">Algebra the Filter applies over.</param>
    /// <param name="var">Variable to restrict on.</param>
    /// <param name="term">Expression Term.</param>
    public SameTermFilter(ISparqlAlgebra pattern, string var, ConstantTerm term)
        : base(pattern, var, term, new UnaryExpressionFilter(new SameTermFunction(new VariableTerm(var), term))) { }

    /// <summary>
    /// Transforms the Inner Algebra using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimiser.</param>
    /// <returns></returns>
    public override ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
    {
        if (optimiser is IExpressionTransformer expressionTransformer)
        {
            return new SameTermFilter(optimiser.Optimise(InnerAlgebra), RestrictionVariable, (ConstantTerm)expressionTransformer.Transform(RestrictionValue));
        }

        return new SameTermFilter(optimiser.Optimise(InnerAlgebra), RestrictionVariable, RestrictionValue);
    }
}
