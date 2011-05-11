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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System.Text;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Paths
{
    /// <summary>
    /// Represents a standard forwards path
    /// </summary>
    public class SequencePath : BaseBinaryPath
    {
        /// <summary>
        /// Creates a new Sequence Path
        /// </summary>
        /// <param name="lhs">LHS Path</param>
        /// <param name="rhs">RHS Path</param>
        public SequencePath(ISparqlPath lhs, ISparqlPath rhs)
            : base(lhs, rhs) { }

        /// <summary>
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append(this._lhs.ToString());
            output.Append(" / ");
            output.Append(this._rhs.ToString());
            return output.ToString();
        }

        /// <summary>
        /// Converts a Path into its Algebra Form
        /// </summary>
        /// <param name="context">Path Transformation Context</param>
        /// <returns></returns>
        public override ISparqlAlgebra ToAlgebra(PathTransformContext context)
        {
            bool top = context.Top;

            //The Object becomes a temporary variable then we transform the LHS of the path
            context.Object = context.GetNextTemporaryVariable();
            context.Top = false;
            context.AddTriplePattern(context.GetTriplePattern(context.Subject, this._lhs, context.Object));

            //The Subject is then the Object that results from the LHS transform since the
            //Transform may adjust the Object
            context.Subject = context.Object;

            //We then reset the Object to be the target Object so that if the RHS is the last part
            //of the Path then it will complete the path transformation
            //If it isn't the last part of the path it will be set to a new temporary variable
            context.Top = top;
            if (context.Top)
            {
                context.ResetObject();
            }
            else
            {
                context.Object = context.GetNextTemporaryVariable();
            }
            context.Top = top;
            context.AddTriplePattern(context.GetTriplePattern(context.Subject, this._rhs, context.Object));

            return context.ToAlgebra();
        }
    }
}
