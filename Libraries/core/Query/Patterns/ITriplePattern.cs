/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using VDS.RDF.Query.Construct;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Interface for Triple Patterns
    /// </summary>
    public interface ITriplePattern : IComparable<ITriplePattern>
    {
        /// <summary>
        /// Evaluates the Triple Pattern in the given Evaluation Context
        /// </summary>
        /// <param name="context">Query Evaluation Context</param>
        void Evaluate(SparqlEvaluationContext context);

        /// <summary>
        /// Gets the Index type that should be used in Pattern execution
        /// </summary>
        TripleIndexType IndexType 
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
        /// Gets whether a Triple Pattern uses the Default Dataset when evaluated
        /// </summary>
        /// <remarks>
        /// Almost all Triple Patterns use the Default Dataset unless they are sub-query patterns which themselves don't use the Default Dataset or they contain an expression (in the case of BIND/LET/FILTERs) which does not use the Default Dataset
        /// </remarks>
        bool UsesDefaultDataset
        {
            get;
        }
        
    }

    /// <summary>
    /// Interface for Triple Patterns that can be used in a CONSTRUCT pattern
    /// </summary>
    public interface IConstructTriplePattern : ITriplePattern
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

        /// <summary>
        /// Gets whether the Pattern contains no Blank Node Variables (i.e. Explicit Variables are ignored)
        /// </summary>
        bool HasNoBlankVariables
        {
            get;
        }
    }

    /// <summary>
    /// Interface for Triple Patterns that represent Assignment operators
    /// </summary>
    public interface IAssignmentPattern : ITriplePattern, IComparable<IAssignmentPattern>
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
}
