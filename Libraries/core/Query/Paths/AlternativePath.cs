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

        /// <summary>
        /// Converts a Path into its Algebra Form
        /// </summary>
        /// <param name="context">Path Transformation Context</param>
        /// <returns></returns>
        public override ISparqlAlgebra ToAlgebra(PathTransformContext context)
        {
            PathTransformContext lhsContext = new PathTransformContext(context);
            PathTransformContext rhsContext = new PathTransformContext(context);
            ISparqlAlgebra lhs = this._lhs.ToAlgebra(lhsContext);
            ISparqlAlgebra rhs = this._rhs.ToAlgebra(rhsContext);
            return new Union(lhs, rhs);
        }
    }
}
