/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Interface for algebra operators
    /// </summary>
    public interface IAlgebra
        : IEquatable<IAlgebra>
    {
        /// <summary>
        /// Gets the variables that are projected from this algebra i.e. those visible outside it
        /// </summary>
        IEnumerable<String> ProjectedVariables { get;  }

        /// <summary>
        /// Gets the variables that are fixed for this algebra i.e. those projected and guaranteed to have a value
        /// </summary>
        IEnumerable<String> FixedVariables { get;  }

        /// <summary>
        /// Gets the variables that are floating for this algebra i.e. those projected and not guaranteed to have a value
        /// </summary>
        IEnumerable<String> FloatingVariables { get; }

        /// <summary>
        /// Accepts an algebra visitor
        /// </summary>
        /// <param name="visitor">Visitor</param>
        void Accept(IAlgebraVisitor visitor);

        /// <summary>
        /// Executes the algebra
        /// </summary>
        /// <param name="executor">Algebra executor</param>
        /// <param name="context">Execution Context</param>
        /// <returns></returns>
        /// <remarks>
        /// The default implementations provide this visitor pattern style so they simply call back to the appropriate method of the provided algebra executor.  This is therefore primarily intended for use as an extension point, custom algebra operators can be introduced which provide their own execution semantics
        /// </remarks>
        IEnumerable<ISolution> Execute(IAlgebraExecutor executor, IExecutionContext context);

        /// <summary>
        /// Gets the string form of the algebra
        /// </summary>
        /// <returns></returns>
        String ToString();

        /// <summary>
        /// Gets the string form of the algeba using the given formatter
        /// </summary>
        /// <param name="formatter">Formatter</param>
        /// <returns></returns>
        String ToString(IAlgebraFormatter formatter);

        /// <summary>
        /// Creates an exact copy of the algebra
        /// </summary>
        /// <returns></returns>
        IAlgebra Copy();
    }

    /// <summary>
    /// Interface for unary algebra operators
    /// </summary>
    public interface IUnaryAlgebra
        : IAlgebra
    {
        /// <summary>
        /// Gets the inner algebra
        /// </summary>
        IAlgebra InnerAlgebra { get; }

        /// <summary>
        /// Creates a copy of the operator replacing the inner algebra with the given algebra
        /// </summary>
        /// <param name="innerAlgebra">Inner Algebra</param>
        /// <returns></returns>
        IAlgebra Copy(IAlgebra innerAlgebra);
    }

    /// <summary>
    /// Interface for binary algebra operators
    /// </summary>
    public interface IBinaryAlgebra
        : IAlgebra
    {
        /// <summary>
        /// Gets the LHS algebra
        /// </summary>
        IAlgebra Lhs { get; }

        /// <summary>
        /// Gets the RHS algebra
        /// </summary>
        IAlgebra Rhs { get; }

        /// <summary>
        /// Creates a copy of the operator replacing the inner algebras with the given algebras
        /// </summary>
        /// <param name="lhs">LHS Algebra</param>
        /// <param name="rhs">RHS Algebra</param>
        /// <returns></returns>
        IAlgebra Copy(IAlgebra lhs, IAlgebra rhs);
    }
}
