/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Interface for classes that represent the SPARQL Algebra and are used to evaluate queries
    /// </summary>
    public interface ISparqlAlgebra
    {
        /// <summary>
        /// Evaluates the Algebra in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        BaseMultiset Evaluate(SparqlEvaluationContext context);

        /// <summary>
        /// Gets the enumeration of Variables used in the Algebra
        /// </summary>
        IEnumerable<String> Variables
        {
            get;
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        IEnumerable<String> FloatingVariables { get; }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        IEnumerable<String> FixedVariables { get; }
        
        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown if the given Algebra cannot be converted to a SPARQL Query</exception>
        SparqlQuery ToQuery();

        /// <summary>
        /// Converts the Algebra back to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown if the given Algebra cannot be converted to a Graph Pattern</exception>
        GraphPattern ToGraphPattern();
    }

    /// <summary>
    /// Interface for SPARQL Algebra constructs which are unary operators i.e. they apply over a single inner Algebra
    /// </summary>
    public interface IUnaryOperator 
        : ISparqlAlgebra
    {
        /// <summary>
        /// Gets the Inner Algebra
        /// </summary>
        ISparqlAlgebra InnerAlgebra
        {
            get;
        }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        /// <remarks>
        /// The operator should retain all it's existing properties and just return a new version of itself with the inner algebra having had the given optimiser applied to it
        /// </remarks>
        ISparqlAlgebra Transform(IAlgebraOptimiser optimiser);
    }

    /// <summary>
    /// Marker Interface for SPARQL Algebra constructs which are terminal operators i.e. they contain no inner algebra operators
    /// </summary>
    public interface ITerminalOperator 
        : ISparqlAlgebra
    {

    }

    /// <summary>
    /// Represents an Algebra construct which is a BGP
    /// </summary>
    public interface IBgp 
        : ISparqlAlgebra, ITerminalOperator
    {
        /// <summary>
        /// Gets the Number of Patterns in the BGP
        /// </summary>
        int PatternCount
        {
            get;
        }

        /// <summary>
        /// Gets the Triple Patterns in the BGP
        /// </summary>
        IEnumerable<ITriplePattern> TriplePatterns
        {
            get;
        }
    }

    /// <summary>
    /// Represents an Algebra construct which is a Filter
    /// </summary>
    public interface IFilter 
        : IUnaryOperator
    {
        /// <summary>
        /// Gets the Filter
        /// </summary>
        ISparqlFilter SparqlFilter 
        { 
            get;
        }
    }


    /// <summary>
    /// Represents an Algebra construct which is an Abstract Join (i.e. any kind of Join over two algebra operators)
    /// </summary>
    /// <remarks>
    /// Specific sub-interfaces are used to mark specific kinds of Join
    /// </remarks>
    public interface IAbstractJoin
        : ISparqlAlgebra
    {
        /// <summary>
        /// Gets the LHS of the Join
        /// </summary>
        ISparqlAlgebra Lhs
        {
            get;
        }

        /// <summary>
        /// Gets the RHS of the Join
        /// </summary>
        ISparqlAlgebra Rhs
        {
            get;
        }

        /// <summary>
        /// Transforms both sides of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        /// <remarks>
        /// The join should retain all it's existing properties and just return a new version of itself with the two sides of the join having had the given optimiser applied to them
        /// </remarks>
        ISparqlAlgebra Transform(IAlgebraOptimiser optimiser);

        /// <summary>
        /// Transforms the LHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        /// <remarks>
        /// The join should retain all it's existing properties and just return a new version of itself with LHS side of the join having had the given optimiser applied to them
        /// </remarks>
        ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser);

        /// <summary>
        /// Transforms the RHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        /// <remarks>
        /// The join should retain all it's existing properties and just return a new version of itself with RHS side of the join having had the given optimiser applied to them
        /// </remarks>
        ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser);
    }

    /// <summary>
    /// Represents an Algebra construct which is a Join
    /// </summary>
    public interface IJoin 
        : IAbstractJoin
    {

    }

    /// <summary>
    /// Represents an Algebra construct which is a Left Join
    /// </summary>
    public interface ILeftJoin
        : IAbstractJoin
    {
        /// <summary>
        /// Gets the Filter used on the Join
        /// </summary>
        ISparqlFilter Filter
        {
            get;
        }
    }

    /// <summary>
    /// Represents an Algebra construct which is a Union
    /// </summary>
    public interface IUnion 
        : IAbstractJoin
    {

    }

    /// <summary>
    /// Represents an Algebra construct which is a Minus
    /// </summary>
    public interface IMinus 
        : IAbstractJoin
    {

    }

    /// <summary>
    /// Represents an Algebra construct which is an Exists Join
    /// </summary>
    public interface IExistsJoin 
        : IAbstractJoin
    {
        /// <summary>
        /// Gets whether the Join requires compatible solutions to exist on the RHS
        /// </summary>
        bool MustExist
        {
            get;
        }
    }
}
