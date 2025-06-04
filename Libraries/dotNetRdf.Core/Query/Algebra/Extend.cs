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
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Represents an Extend operation which is the formal algebraic form of the BIND operation.
/// </summary>
public class Extend
    : IUnaryOperator
{
    /// <summary>
    /// Creates a new Extend operator.
    /// </summary>
    /// <param name="pattern">Pattern.</param>
    /// <param name="expr">Expression.</param>
    /// <param name="var">Variable to bind to.</param>
    public Extend(ISparqlAlgebra pattern, ISparqlExpression expr, string var)
    {
        InnerAlgebra = pattern;
        AssignExpression = expr;
        VariableName = var;

        if (InnerAlgebra.Variables.Contains(VariableName))
        {
            throw new RdfQueryException("Cannot create an Extend() operator which extends the results of the inner algebra with a variable that is already used in the inner algebra");
        }
    }

    /// <summary>
    /// Gets the Variable Name to be bound.
    /// </summary>
    public string VariableName { get; }

    /// <summary>
    /// Gets the Assignment Expression.
    /// </summary>
    public ISparqlExpression AssignExpression { get; }

    /// <summary>
    /// Gets the Inner Algebra.
    /// </summary>
    public ISparqlAlgebra InnerAlgebra { get; }

    /// <summary>
    /// Transforms the Inner Algebra using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimiser.</param>
    /// <returns></returns>
    public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
    {
        if (optimiser is IExpressionTransformer)
        {
            return new Extend(optimiser.Optimise(InnerAlgebra), ((IExpressionTransformer)optimiser).Transform(AssignExpression), VariableName);
        }
        else
        {
            return new Extend(optimiser.Optimise(InnerAlgebra), AssignExpression, VariableName);
        }
    }

    /// <summary>
    /// Gets the variables used in the algebra.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get 
        {
            return InnerAlgebra.Variables.Concat(VariableName.AsEnumerable()); 
        }
    }

    /// <summary>
    /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FloatingVariables { get { return InnerAlgebra.FloatingVariables.Concat(VariableName.AsEnumerable()); } }

    /// <summary>
    /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FixedVariables { get { return InnerAlgebra.FixedVariables; } }

    /// <summary>
    /// Converts the Algebra to a Query.
    /// </summary>
    /// <returns></returns>
    public SparqlQuery ToQuery()
    {
        return new SparqlQuery {RootGraphPattern = ToGraphPattern()};
    }

    /// <summary>
    /// Converts the Algebra to a Graph Pattern.
    /// </summary>
    /// <returns></returns>
    public GraphPattern ToGraphPattern()
    {
        var gp = InnerAlgebra.ToGraphPattern();
        if (gp.HasModifier)
        {
            var p = new GraphPattern();
            p.AddGraphPattern(gp);
            p.AddAssignment(new BindPattern(VariableName, AssignExpression));
            return p;
        }
        else
        {
            gp.AddAssignment(new BindPattern(VariableName, AssignExpression));
            return gp;
        }
    }

    /// <summary>
    /// Gets the String representation of the Extend.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "Extend(" + InnerAlgebra.ToSafeString() + ", " + AssignExpression + " AS ?" + VariableName + ")";
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessExtend(this, context);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitExtend(this);
    }
}
