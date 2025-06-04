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
/// Represents a LeftJoin predicated on the existence/non-existence of joinable sets on the RHS for each item on the LHS.
/// </summary>
public class ExistsJoin 
    : IExistsJoin
{
    private readonly ISparqlAlgebra _lhs, _rhs;
    private readonly bool _mustExist;

    /// <summary>
    /// Creates a new Exists Join.
    /// </summary>
    /// <param name="lhs">LHS Pattern.</param>
    /// <param name="rhs">RHS Pattern.</param>
    /// <param name="mustExist">Whether a joinable set must exist on the RHS for the LHS set to be preserved.</param>
    public ExistsJoin(ISparqlAlgebra lhs, ISparqlAlgebra rhs, bool mustExist)
    {
        _lhs = lhs;
        _rhs = rhs;
        _mustExist = mustExist;
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
        get { return _lhs.FloatingVariables; }
    }

    /// <summary>
    /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FixedVariables
    {
        get { return _lhs.FixedVariables; }
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

    /// <summary>
    /// Gets whether this is an EXISTS join.
    /// </summary>
    public bool MustExist
    {
        get
        {
            return _mustExist;
        }
    }

    /// <summary>
    /// Gets the String representation of the Algebra.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "ExistsJoin(" + _lhs + ", " + _rhs + ", " + _mustExist + ")";
    }

    TResult IProcessable.Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessExistsJoin(this, context);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitExistsJoin(this);
    }


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
        var p = _lhs.ToGraphPattern();
        var opt = _rhs.ToGraphPattern();
        if (_mustExist)
        {
            opt.IsExists = true;
        }
        else
        {
            opt.IsNotExists = true;
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
        return new ExistsJoin(optimiser.Optimise(_lhs), optimiser.Optimise(_rhs), _mustExist);
    }

    /// <summary>
    /// Transforms the LHS of the Join using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimser.</param>
    /// <returns></returns>
    public ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
    {
        return new ExistsJoin(optimiser.Optimise(_lhs), _rhs, _mustExist);
    }

    /// <summary>
    /// Transforms the RHS of the Join using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimser.</param>
    /// <returns></returns>
    public ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
    {
        return new ExistsJoin(_lhs, optimiser.Optimise(_rhs), _mustExist);
    }
}