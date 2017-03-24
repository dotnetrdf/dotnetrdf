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
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.PropertyFunctions;

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Interface for Triple Patterns
    /// </summary>
    public interface ITriplePattern
        : IComparable<ITriplePattern>
    {
        /// <summary>
        /// Evaluates the Triple Pattern in the given Evaluation Context
        /// </summary>
        /// <param name="context">Query Evaluation Context</param>
        void Evaluate(SparqlEvaluationContext context);

        /// <summary>
        /// Gets the Pattern Type
        /// </summary>
        TriplePatternType PatternType
        {
            get;
        }

        /// <summary>
        /// Gets whether the Pattern accepts all
        /// </summary>
        /// <remarks>
        /// Indicates that a Pattern is of the form ?s ?p ?o
        /// </remarks>
        bool IsAcceptAll 
        {
            get;
        }

        /// <summary>
        /// Gets the List of Variables used in the Pattern
        /// </summary>
        List<string> Variables 
        { 
            get; 
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the pattern i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        IEnumerable<String> FloatingVariables { get; }

        /// <summary>
        /// Gets the enumeration of fixed variables in the pattern i.e. variables that are guaranteed to have a bound value
        /// </summary>
        IEnumerable<String> FixedVariables { get; }

        /// <summary>
        /// Gets whether a Triple Pattern uses the Default Dataset when evaluated
        /// </summary>
        /// <remarks>
        /// Almost all Triple Patterns use the Default Dataset unless they are sub-query patterns which themselves don't use the Default Dataset or they contain an expression (in the case of BIND/LET/FILTERs) which does not use the Default Dataset
        /// </remarks>
        bool UsesDefaultDataset
        {
            get;
        }

        /// <summary>
        /// Gets whether a Triple Pattern does not contain any Blank Variables
        /// </summary>
        bool HasNoBlankVariables
        {
            get;
        }
        
    }

    /// <summary>
    /// Interface for Triple Patterns that can be used in a CONSTRUCT pattern
    /// </summary>
    public interface IConstructTriplePattern
        : ITriplePattern
    {
        /// <summary>
        /// Constructs a Triple from a Set based on this Triple Pattern
        /// </summary>
        /// <param name="context">Construct Context</param>
        /// <returns></returns>
        Triple Construct(ConstructContext context);

        /// <summary>
        /// Gets the Subject of the Pattern
        /// </summary>
        PatternItem Subject
        {
            get;
        }

        /// <summary>
        /// Gets the Predicate of the Pattern
        /// </summary>
        PatternItem Predicate
        {
            get;
        }

        /// <summary>
        /// Gets the Object of the Pattern
        /// </summary>
        PatternItem Object
        {
            get;
        }

        /// <summary>
        /// Gets whether the Pattern contains no Variables of any kind
        /// </summary>
        bool HasNoVariables
        {
            get;
        }

        /// <summary>
        /// Gets whether the Pattern contains no Explicit Variables (i.e. Blank Node Variables are ignored)
        /// </summary>
        bool HasNoExplicitVariables
        {
            get;
        }
    }

    /// <summary>
    /// Inteface for Triple Patterns that do simple pattern matching
    /// </summary>
    public interface IMatchTriplePattern
        : ITriplePattern, IComparable<IMatchTriplePattern>
    {
        /// <summary>
        /// Gets the Index type that should be used in Pattern execution
        /// </summary>
        TripleIndexType IndexType
        {
            get;
        }

        /// <summary>
        /// Gets the Subject of the Pattern
        /// </summary>
        PatternItem Subject
        {
            get;
        }

        /// <summary>
        /// Gets the Predicate of the Pattern
        /// </summary>
        PatternItem Predicate
        {
            get;
        }

        /// <summary>
        /// Gets the Object of the Pattern
        /// </summary>
        PatternItem Object
        {
            get;
        }

        /// <summary>
        /// Gets the Triples that match this pattern
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        IEnumerable<Triple> GetTriples(SparqlEvaluationContext context);

        /// <summary>
        /// Gets whether a given triple is accepted by this pattern
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        bool Accepts(SparqlEvaluationContext context, Triple t);

        /// <summary>
        /// Creates a set from a Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        ISet CreateResult(Triple t);
    }

    /// <summary>
    /// Interface for Triple Patterns that apply filters
    /// </summary>
    public interface IFilterPattern
        : ITriplePattern, IComparable<IFilterPattern>
    {
        /// <summary>
        /// Gets the filter to apply
        /// </summary>
        ISparqlFilter Filter
        {
            get;
        }
    }

    /// <summary>
    /// Interface for Triple Patterns that represent Assignment operators
    /// </summary>
    public interface IAssignmentPattern
        : ITriplePattern, IComparable<IAssignmentPattern>
    {
        /// <summary>
        /// Gets the Assignment Expression that is used
        /// </summary>
        ISparqlExpression AssignExpression
        {
            get;
        }

        /// <summary>
        /// Name of the Variable which is assigned to
        /// </summary>
        String VariableName
        {
            get;
        }
    }

    /// <summary>
    /// Interface for Triple Patterns that do sub-queries
    /// </summary>
    public interface ISubQueryPattern
        : ITriplePattern, IComparable<ISubQueryPattern>
    {
        /// <summary>
        /// Gets the sub-query
        /// </summary>
        SparqlQuery SubQuery
        {
            get;
        }
    }

    /// <summary>
    /// Interface for Triple Patterns that do property paths
    /// </summary>
    public interface IPropertyPathPattern
        : ITriplePattern, IComparable<IPropertyPathPattern>
    {
        /// <summary>
        /// Gets the Subject of the Pattern
        /// </summary>
        PatternItem Subject
        {
            get;
        }

        /// <summary>
        /// Gets the property path
        /// </summary>
        ISparqlPath Path
        {
            get;
        }

        /// <summary>
        /// Gets the Object of the Pattern
        /// </summary>
        PatternItem Object
        {
            get;
        }
    }

    /// <summary>
    /// Interface for Triple Patterns that do property functions
    /// </summary>
    public interface IPropertyFunctionPattern
        : ITriplePattern, IComparable<IPropertyFunctionPattern>
    {
        /// <summary>
        /// Gets the Subject arguments of the function
        /// </summary>
        IEnumerable<PatternItem> SubjectArgs
        {
            get;
        }

        /// <summary>
        /// Gets the Object arguments of the function
        /// </summary>
        IEnumerable<PatternItem> ObjectArgs
        {
            get;
        }

        /// <summary>
        /// Gets the property function
        /// </summary>
        ISparqlPropertyFunction PropertyFunction
        {
            get;
        }

        /// <summary>
        /// Gets the original triple patterns that made up this pattern
        /// </summary>
        IEnumerable<ITriplePattern> OriginalPatterns
        {
            get;
        }
    }
}
