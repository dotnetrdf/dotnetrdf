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
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Paths
{
    /// <summary>
    /// Represents a Path expression in SPARQL
    /// </summary>
    public interface ISparqlPath
    {
        ///// <summary>
        ///// Gets whether the Path is a Simple Path
        ///// </summary>
        //bool IsSimple
        //{
        //    get;
        //}

        ///// <summary>
        ///// Gets whether the Path allows for zero-length Paths
        ///// </summary>
        //bool AllowsZeroLength
        //{
        //    get;
        //}

        ///// <summary>
        ///// Evaluates the Path in the given Path Evaluation Context
        ///// </summary>
        ///// <param name="context">Evaluation Context</param>
        //void Evaluate(PathEvaluationContext context);

        ///// <summary>
        ///// Converts the Path to an Algebra expression
        ///// </summary>
        ///// <param name="context">Transform Context</param>
        ///// <returns></returns>
        ///// <remarks>
        ///// If the Path is not simple then this function will throw an error since there is no Algebra transform for the Path
        ///// </remarks>
        ///// <exception cref="RdfQueryException">Thrown if the path is non-simple and therefore there is no Algebra transform</exception>
        //void ToAlgebra(PathTransformContext context);

        ISparqlAlgebra ToAlgebraOperator(PathTransformContext context);
    }
}
