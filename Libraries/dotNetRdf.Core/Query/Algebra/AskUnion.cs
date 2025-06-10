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

using VDS.RDF.Query.Optimisation;

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Represents a Union.
/// </summary>
/// <remarks>
/// <para>
/// An Ask Union differs from a standard Union in that if it finds a solution on the LHS it has no need to evaluate the RHS.
/// </para>
/// </remarks>
public class AskUnion : Union
{
    private readonly ISparqlAlgebra _lhs, _rhs;

    /// <summary>
    /// Creates a new Ask Union.
    /// </summary>
    /// <param name="lhs">LHS Pattern.</param>
    /// <param name="rhs">RHS Pattern.</param>
    public AskUnion(ISparqlAlgebra lhs, ISparqlAlgebra rhs):base(lhs, rhs)
    {
        _lhs = lhs;
        _rhs = rhs;
    }


    /// <summary>
    /// Gets the String representation of the Algebra.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "AskUnion(" + _lhs + ", " + _rhs + ")";
    }


    /// <summary>
    /// Transforms both sides of the Join using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimser.</param>
    /// <returns></returns>
    public override ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
    {
        return new AskUnion(optimiser.Optimise(_lhs), optimiser.Optimise(_rhs));
    }

    /// <summary>
    /// Transforms the LHS of the Join using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimser.</param>
    /// <returns></returns>
    public override ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
    {
        return new AskUnion(optimiser.Optimise(_lhs), _rhs);
    }

    /// <summary>
    /// Transforms the RHS of the Join using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimser.</param>
    /// <returns></returns>
    public override ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
    {
        return new AskUnion(_lhs, optimiser.Optimise(_rhs));
    }
}