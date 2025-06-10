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

// unset

using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Represents a LeftJoin predicated on an arbitrary filter expression.
/// </summary>
public class LeftJoin 
    : ILeftJoin
{
    /// <summary>
    /// Creates a new LeftJoin where there is no Filter over the join.
    /// </summary>
    /// <param name="lhs">LHS Pattern.</param>
    /// <param name="rhs">RHS Pattern.</param>
    public LeftJoin(ISparqlAlgebra lhs, ISparqlAlgebra rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    /// <summary>
    /// Creates a new LeftJoin where there is a Filter over the join.
    /// </summary>
    /// <param name="lhs">LHS Pattern.</param>
    /// <param name="rhs">RHS Pattern.</param>
    /// <param name="filter">Filter to decide which RHS solutions are valid.</param>
    public LeftJoin(ISparqlAlgebra lhs, ISparqlAlgebra rhs, ISparqlFilter filter)
        : this(lhs, rhs)
    {
        Filter = filter;
    }


    /// <summary>
    /// Gets the Variables used in the Algebra.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get
        {
            return (Lhs.Variables.Concat(Rhs.Variables)).Distinct();
        }
    }

    /// <summary>
    /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FloatingVariables
    {
        get
        {
            // Floating variables are those fixed on RHS or floating on either side and not fixed on LHS
            IEnumerable<string> floating = Lhs.FloatingVariables.Concat(Rhs.FloatingVariables).Concat(Rhs.FixedVariables).Distinct();
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
            // Fixed variables are those fixed on LHS
            return Lhs.FixedVariables;
        }
    }

    /// <summary>
    /// Gets the Filter that applies across the Join.
    /// </summary>
    public ISparqlFilter Filter { get; } = new UnaryExpressionFilter(new ConstantTerm(new BooleanNode(true)));

    /// <summary>
    /// Gets the LHS of the Join.
    /// </summary>
    public ISparqlAlgebra Lhs { get; }

    /// <summary>
    /// Gets the RHS of the Join.
    /// </summary>
    public ISparqlAlgebra Rhs { get; }

    /// <summary>
    /// Gets the String representation of the Algebra.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var filter = Filter.ToString().TrimEnd();
        filter = filter.Substring(7, filter.Length - 8);
        return "LeftJoin(" + Lhs + ", " + Rhs + ", " + filter + ")";
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessLeftJoin(this, context);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitLeftJoin(this);
    }

    /// <summary>
    /// Converts the Algebra back to a SPARQL Query.
    /// </summary>
    /// <returns></returns>
    public SparqlQuery ToQuery()
    {
        var q = new SparqlQuery();
        q.RootGraphPattern = ToGraphPattern();
        q.Optimise();
        return q;
    }

    /// <summary>
    /// Converts the Algebra back to a SPARQL Query.
    /// </summary>
    /// <returns></returns>
    public GraphPattern ToGraphPattern()
    {
        var p = Lhs.ToGraphPattern();
        var opt = Rhs.ToGraphPattern();
        opt.IsOptional = true;
        if (Filter.Expression is ConstantTerm ct)
        {
            try
            {
                if (!ct.Node.AsSafeBoolean())
                {
                    opt.Filter = Filter;
                }
            }
            catch
            {
                opt.Filter = Filter;
            }
        }
        else
        {
            opt.Filter = Filter;
        }
        p.AddGraphPattern(opt);
        return p;
    }

    /// <summary>
    /// Transforms both sides of the Join using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimser.</param>
    /// <returns></returns>
    public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
    {
        if (optimiser is IExpressionTransformer)
        {
            return new LeftJoin(optimiser.Optimise(Lhs), optimiser.Optimise(Rhs), new UnaryExpressionFilter(((IExpressionTransformer)optimiser).Transform(Filter.Expression)));
        }
        else
        {
            return new LeftJoin(optimiser.Optimise(Lhs), optimiser.Optimise(Rhs), Filter);
        }
    }

    /// <summary>
    /// Transforms the LHS of the Join using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimser.</param>
    /// <returns></returns>
    public ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
    {
        if (optimiser is IExpressionTransformer)
        {
            return new LeftJoin(optimiser.Optimise(Lhs), Rhs, new UnaryExpressionFilter(((IExpressionTransformer)optimiser).Transform(Filter.Expression)));
        }
        else
        {
            return new LeftJoin(optimiser.Optimise(Lhs), Rhs, Filter);
        }
    }

    /// <summary>
    /// Transforms the RHS of the Join using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimser.</param>
    /// <returns></returns>
    public ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
    {
        if (optimiser is IExpressionTransformer)
        {
            return new LeftJoin(Lhs, optimiser.Optimise(Rhs), new UnaryExpressionFilter(((IExpressionTransformer)optimiser).Transform(Filter.Expression)));
        }
        else
        {
            return new LeftJoin(Lhs, optimiser.Optimise(Rhs), Filter);
        }
    }
}