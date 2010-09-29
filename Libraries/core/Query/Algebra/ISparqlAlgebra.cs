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
using System.Linq;
using System.Text;
using VDS.RDF.Query.Filters;
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
    }

    /// <summary>
    /// Represents an Algebra construct which is a BGP
    /// </summary>
    public interface IBgp : ISparqlAlgebra
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
    /// Represents an Algebra construct which is an Abstract Join
    /// </summary>
    public interface IAbstractJoin : ISparqlAlgebra
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
    }

    /// <summary>
    /// Represents an Algebra construct which is a Join
    /// </summary>
    public interface IJoin : IAbstractJoin
    {

    }

    /// <summary>
    /// Represents an Algebra construct which is a Left Join
    /// </summary>
    public interface ILeftJoin : IAbstractJoin
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
    public interface IUnion : IAbstractJoin
    {

    }

    /// <summary>
    /// Represents an Algebra construct which is a Minus
    /// </summary>
    public interface IMinus : IAbstractJoin
    {

    }

    /// <summary>
    /// Represents an Algebra construct which is an Exists Join
    /// </summary>
    public interface IExistsJoin : IAbstractJoin
    {

    }
}
