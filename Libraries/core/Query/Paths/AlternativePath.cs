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

namespace VDS.RDF.Query.Paths
{
    /// <summary>
    /// Represents Alternative Paths
    /// </summary>
    public class AlternativePath : BaseBinaryPath
    {
        /// <summary>
        /// Creates a new Alternative Path
        /// </summary>
        /// <param name="lhs">LHS Path</param>
        /// <param name="rhs">RHS Path</param>
        public AlternativePath(ISparqlPath lhs, ISparqlPath rhs)
            : base(lhs, rhs) { }

        ///// <summary>
        ///// Alternative paths are complex
        ///// </summary>
        //public override bool IsSimple
        //{
        //    get
        //    {
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// Evaluates the Path in the given Context
        ///// </summary>
        ///// <param name="context">Path Evaluation Context</param>
        //public override void Evaluate(PathEvaluationContext context)
        //{
        //    bool first = context.IsFirst;

        //    //Evaluate the LHS using the new context
        //    PathEvaluationContext newContext = new PathEvaluationContext(context);
        //    this._lhs.Evaluate(newContext);

        //    //Evaluate the RHS using the original context
        //    context.IsFirst = first;
        //    this._rhs.Evaluate(context);

        //    //Union the results of the two alternatives together
        //    context.Paths.UnionWith(newContext.Paths);
        //    context.CompletePaths.UnionWith(newContext.CompletePaths);
        //}

        /// <summary>
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('(');
            output.Append(this._lhs.ToString());
            output.Append(" | ");
            output.Append(this._rhs.ToString());
            output.Append(')');
            return output.ToString();
        }

        ///// <summary>
        ///// Throws an error since a Path with alternatives is not transformable to an Algebra expression
        ///// </summary>
        ///// <param name="context">Transform Context</param>
        //public override void ToAlgebra(PathTransformContext context)
        //{
        //    throw new RdfQueryException("Cannot transform a non-simple Path to an Algebra expression");
        //}

        public override ISparqlAlgebra ToAlgebraOperator(PathTransformContext context)
        {
            PathTransformContext lhsContext = new PathTransformContext(context);
            PathTransformContext rhsContext = new PathTransformContext(context);
            ISparqlAlgebra lhs = this._lhs.ToAlgebraOperator(lhsContext);
            ISparqlAlgebra rhs = this._rhs.ToAlgebraOperator(rhsContext);
            return new Union(lhs, rhs);
        }
    }
}
