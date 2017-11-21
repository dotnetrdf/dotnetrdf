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
            output.Append(_lhs.ToString());
            output.Append(" | ");
            output.Append(_rhs.ToString());
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
            ISparqlAlgebra lhs = _lhs.ToAlgebra(lhsContext);
            ISparqlAlgebra rhs = _rhs.ToAlgebra(rhsContext);
            return new Union(lhs, rhs);
        }
    }
}
