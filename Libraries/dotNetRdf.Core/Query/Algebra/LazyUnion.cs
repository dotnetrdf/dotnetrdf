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

using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Represents a Union.
/// </summary>
/// <remarks>
/// <para>
/// A Lazy Union differs from a standard Union in that if it finds sufficient solutions on the LHS it has no need to evaluate the RHS.
/// </para>
/// </remarks>
public class LazyUnion : IUnion
{
    /// <summary>
    /// Creates a new Lazy Union.
    /// </summary>
    /// <param name="lhs">LHS Pattern.</param>
    /// <param name="rhs">RHS Pattern.</param>
    public LazyUnion(ISparqlAlgebra lhs, ISparqlAlgebra rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    /// <summary>
    /// Creates a new Lazy Union.
    /// </summary>
    /// <param name="lhs">LHS Pattern.</param>
    /// <param name="rhs">RHS Pattern.</param>
    /// <param name="requiredResults">The number of results that the Union should attempt to return.</param>
    public LazyUnion(ISparqlAlgebra lhs, ISparqlAlgebra rhs, int requiredResults)
    {
        Lhs = lhs;
        Rhs = rhs;
        RequiredResults = requiredResults;
    }

    /// <summary>
    /// Evaluates the Lazy Union.
    /// </summary>
    /// <param name="context">Evaluation Context.</param>
    /// <returns></returns>
    public BaseMultiset Evaluate(SparqlEvaluationContext context)
    {
        BaseMultiset initialInput = context.InputMultiset;
        if (Lhs is Extend || Rhs is Extend) initialInput = new IdentityMultiset();

        context.InputMultiset = initialInput;
        BaseMultiset lhsResult = context.Evaluate(Lhs); //this._lhs.Evaluate(context);
        context.CheckTimeout();

        if (lhsResult.Count >= RequiredResults || RequiredResults == -1)
        {
            // Only evaluate the RHS if the LHS didn't yield sufficient results
            context.InputMultiset = initialInput;
            BaseMultiset rhsResult = context.Evaluate(Rhs); //this._rhs.Evaluate(context);
            context.CheckTimeout();

            context.OutputMultiset = lhsResult.Union(rhsResult);
            context.CheckTimeout();

            context.InputMultiset = context.OutputMultiset;
        }
        else
        {
            context.OutputMultiset = lhsResult;
        }
        return context.OutputMultiset;
    }

    /// <summary>
    /// Get/set the number of results the union should attempt to return.
    /// </summary>

    public int RequiredResults { get; } = -1;

    /// <summary>
    /// Gets the Variables used in the Algebra.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get { return Lhs.Variables.Concat(Rhs.Variables).Distinct(); }
    }

    /// <summary>
    /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FloatingVariables
    {
        get
        {
            // Floating variables are those not fixed
            var fixedVars = new HashSet<string>(FixedVariables);
            return Variables.Where(v => !fixedVars.Contains(v));
        }
    }

    /// <summary>
    /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FixedVariables
    {
        get
        {
            // Fixed variables are those fixed on both sides
            return Lhs.FixedVariables.Intersect(Rhs.FixedVariables);
        }
    }

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
        return "LazyUnion(" + Lhs + ", " + Rhs + ")";
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessUnion(this, context);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitUnion(this);
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
    /// Converts the Union back to Graph Patterns.
    /// </summary>
    /// <returns></returns>
    public GraphPattern ToGraphPattern()
    {
        var p = new GraphPattern();
        p.IsUnion = true;
        p.AddGraphPattern(Lhs.ToGraphPattern());
        p.AddGraphPattern(Rhs.ToGraphPattern());
        return p;
    }

    /// <summary>
    /// Transforms both sides of the Join using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimser.</param>
    /// <returns></returns>
    public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
    {
        return new LazyUnion(optimiser.Optimise(Lhs), optimiser.Optimise(Rhs));
    }

    /// <summary>
    /// Transforms the LHS of the Join using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimser.</param>
    /// <returns></returns>
    public ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
    {
        return new LazyUnion(optimiser.Optimise(Lhs), Rhs);
    }

    /// <summary>
    /// Transforms the RHS of the Join using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimser.</param>
    /// <returns></returns>
    public ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
    {
        return new LazyUnion(Lhs, optimiser.Optimise(Rhs));
    }
}