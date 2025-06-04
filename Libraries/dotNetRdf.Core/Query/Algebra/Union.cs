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
/// Represents a Union.
/// </summary>
public class Union 
    : IUnion
{
    private readonly ISparqlAlgebra _lhs, _rhs;

    /// <summary>
    /// Creates a new Union.
    /// </summary>
    /// <param name="lhs">LHS Pattern.</param>
    /// <param name="rhs">RHS Pattern.</param>
    public Union(ISparqlAlgebra lhs, ISparqlAlgebra rhs)
    {
        _lhs = lhs;
        _rhs = rhs;
    }

    /// <summary>
    /// Gets the Variables used in the Algebra.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get
        {
            return (_lhs.Variables.Concat(_rhs.Variables)).Distinct();
        }
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
            return _lhs.FixedVariables.Intersect(_rhs.FixedVariables);
        }
    }

    /// <summary>
    /// Gets the LHS of the Join.
    /// </summary>
    public ISparqlAlgebra Lhs
    {
        get
        {
            return _lhs;
        }
    }

    /// <summary>
    /// Gets the RHS of the Join.
    /// </summary>
    public ISparqlAlgebra Rhs
    {
        get
        {
            return _rhs;
        }
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
    /// Gets the String representation of the Algebra.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "Union(" + _lhs + ", " + _rhs + ")";
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
        var p = new GraphPattern();
        p.IsUnion = true;
        p.AddGraphPattern(_lhs.ToGraphPattern());
        p.AddGraphPattern(_rhs.ToGraphPattern());
        return p;
    }

    /// <summary>
    /// Transforms both sides of the Join using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimser.</param>
    /// <returns></returns>
    public virtual ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
    {
        return new Union(optimiser.Optimise(_lhs), optimiser.Optimise(_rhs));
    }

    /// <summary>
    /// Transforms the LHS of the Join using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimser.</param>
    /// <returns></returns>
    public virtual ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
    {
        return new Union(optimiser.Optimise(_lhs), _rhs);
    }

    /// <summary>
    /// Transforms the RHS of the Join using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimser.</param>
    /// <returns></returns>
    public virtual ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
    {
        return new Union(_lhs, optimiser.Optimise(_rhs));
    }
}