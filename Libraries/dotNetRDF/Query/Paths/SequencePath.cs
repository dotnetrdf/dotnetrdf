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
            output.Append(_lhs.ToString());
            output.Append(" / ");
            output.Append(_rhs.ToString());
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

            // The Object becomes a temporary variable then we transform the LHS of the path
            context.Object = context.GetNextTemporaryVariable();
            context.Top = false;
            context.AddTriplePattern(context.GetTriplePattern(context.Subject, _lhs, context.Object));

            // The Subject is then the Object that results from the LHS transform since the
            // Transform may adjust the Object
            context.Subject = context.Object;

            // We then reset the Object to be the target Object so that if the RHS is the last part
            // of the Path then it will complete the path transformation
            // If it isn't the last part of the path it will be set to a new temporary variable
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
            context.AddTriplePattern(context.GetTriplePattern(context.Subject, _rhs, context.Object));

            return context.ToAlgebra();
        }
    }
}
