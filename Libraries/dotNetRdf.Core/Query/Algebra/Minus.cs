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
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Represents the Minus join.
/// </summary>
public class Minus 
    : IMinus
{
    /// <summary>
    /// Creates a new Minus join.
    /// </summary>
    /// <param name="lhs">LHS Pattern.</param>
    /// <param name="rhs">RHS Pattern.</param>
    public Minus(ISparqlAlgebra lhs, ISparqlAlgebra rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
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
        get { return Lhs.FloatingVariables; }
    }

    /// <summary>
    /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FixedVariables
    {
        get { return Lhs.FixedVariables; }
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
    /// Gets the string representation of the Algebra.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "Minus(" + Lhs + ", " + Rhs + ")";
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessMinus(this, context);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitMinus(this);
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
    /// Converts the Minus() back to a MINUS Graph Pattern.
    /// </summary>
    /// <returns></returns>
    public GraphPattern ToGraphPattern()
    {
        var p = Lhs.ToGraphPattern();
        var opt = Rhs.ToGraphPattern();
        if (!opt.HasModifier)
        {
            opt.IsMinus = true;
            p.AddGraphPattern(opt);
        }
        else
        {
            var parent = new GraphPattern();
            parent.AddGraphPattern(opt);
            parent.IsMinus = true;
            p.AddGraphPattern(parent);
        }
        return p;
    }

    /// <summary>
    /// Transforms both sides of the Join using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimser.</param>
    /// <returns></returns>
    public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
    {
        return new Minus(optimiser.Optimise(Lhs), optimiser.Optimise(Rhs));
    }

    /// <summary>
    /// Transforms the LHS of the Join using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimser.</param>
    /// <returns></returns>
    public ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
    {
        return new Minus(optimiser.Optimise(Lhs), Rhs);
    }

    /// <summary>
    /// Transforms the RHS of the Join using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimser.</param>
    /// <returns></returns>
    public ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
    {
        return new Minus(Lhs, optimiser.Optimise(Rhs));
    }
}
