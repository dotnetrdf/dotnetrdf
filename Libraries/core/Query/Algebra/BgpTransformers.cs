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

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// An Algebra Transformer that optimises Algebra to use <see cref="LazyBgp">LazyBgp</see>'s wherever possible
    /// </summary>
    public class LazyBgpTransformer : BaseAlgebraTransformer
    {
        /// <summary>
        /// Transforms an Algebra to a form that uses <see cref="LazyBgp">LazyBgp</see> where possible
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// By transforming a query to use <see cref="LazyBgp">LazyBgp</see> we can achieve much more efficient processing of some forms of queries
        /// </para>
        /// </remarks>
        protected override ISparqlAlgebra TransformInternal(ISparqlAlgebra algebra, int depth)
        {
            try
            {
                ISparqlAlgebra temp;
                if (algebra is Bgp)
                {
                    //The use of -1 for required results means requirements will be detected from Query
                    temp = new LazyBgp(((Bgp)algebra).TriplePatterns);
                }
                else if (algebra is LeftJoin)
                {
                    IJoin join = (IJoin)algebra;
                    temp = new LeftJoin(this.TransformInternal(join.Lhs, depth + 1), join.Rhs, ((LeftJoin)algebra).Filter);
                }
                else if (algebra is Union)
                {
                    IJoin join = (IJoin)algebra;
                    temp = new LazyUnion(this.TransformInternal(join.Lhs, depth + 1), this.TransformInternal(join.Rhs, depth + 1));
                }
                else
                {
                    temp = algebra;
                }
                return temp;
            }
            catch
            {
                //If the Transform fails return the current algebra
                return algebra;
            }
        }
    }

    /// <summary>
    /// An Algebra Transformer that optimises Algebra to use <see cref="AskBgp">AskBgp</see>'s wherever possible
    /// </summary>
    public class AskBgpTransformer : BaseAlgebraTransformer
    {
        /// <summary>
        /// Transforms an Algebra to a form that uses <see cref="AskBgp">AskBgp</see> where possible
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="depth">Depth</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// By transforming a query to use <see cref="AskBgp">AskBgp</see> we can achieve much more efficient processing of some forms of queries
        /// </para>
        /// </remarks>
        protected override ISparqlAlgebra TransformInternal(ISparqlAlgebra algebra, int depth)
        {
            try
            {
                ISparqlAlgebra temp;
                if (algebra is Bgp)
                {
                    //The use of -1 for required results means requirements will be detected from Query
                    temp = new AskBgp(((Bgp)algebra).TriplePatterns);
                }
                else if (algebra is LeftJoin)
                {
                    IJoin join = (IJoin)algebra;
                    temp = new LeftJoin(this.TransformInternal(join.Lhs, depth + 1), join.Rhs, ((LeftJoin)algebra).Filter);
                }
                else if (algebra is Union)
                {
                    IJoin join = (IJoin)algebra;
                    temp = new AskUnion(this.TransformInternal(join.Lhs, depth + 1), this.TransformInternal(join.Rhs, depth + 1));
                }
                else
                {
                    temp = algebra;
                }
                return temp;
            }
            catch
            {
                //If the Transform fails return the current algebra
                return algebra;
            }
        }
    }
}
