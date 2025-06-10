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

using System;
using System.Collections.Generic;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Represents a BINDINGS modifier on a SPARQL Query.
/// </summary>
public class Bindings
    : ITerminalOperator
{
    /// <summary>
    /// Creates a new BINDINGS modifier.
    /// </summary>
    /// <param name="bindings">Bindings.</param>
    public Bindings(BindingsPattern bindings)
    {
        BindingsPattern = bindings ?? throw new ArgumentNullException(nameof(bindings), "Bindings cannot be null");
    }

    /// <summary>
    /// Gets the Variables used in the Algebra.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get
        {
            return BindingsPattern.Variables;
        }
    }

    /// <summary>
    /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FloatingVariables { get { return BindingsPattern.FloatingVariables; } }

    /// <summary>
    /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FixedVariables { get { return BindingsPattern.FixedVariables; } } 

    /// <summary>
    /// Gets the Bindings. 
    /// </summary>
    public BindingsPattern BindingsPattern { get; }

    /// <summary>
    /// Gets the String representation of the Algebra.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "Bindings()";
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessBindings(this, context);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitBindings(this);
    }

    /// <summary>
    /// Converts the Algebra back to a SPARQL Query.
    /// </summary>
    /// <returns></returns>
    public SparqlQuery ToQuery()
    {
        GraphPattern gp = ToGraphPattern();
        var q = new SparqlQuery {RootGraphPattern = gp};
        return q;
    }

    /// <summary>
    /// Converts the Algebra back to a Graph Pattern.
    /// </summary>
    /// <returns></returns>
    public GraphPattern ToGraphPattern()
    {
        var gp = new GraphPattern();
        gp.AddInlineData(BindingsPattern);
        return gp;
    }

    ///// <summary>
    ///// Transforms the Inner Algebra using the given Optimiser
    ///// </summary>
    ///// <param name="optimiser">Optimiser</param>
    ///// <returns></returns>
    // public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
    // {
    //    return this;
    // }
}
