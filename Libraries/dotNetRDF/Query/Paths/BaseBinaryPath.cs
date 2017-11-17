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
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Paths
{
    /// <summary>
    /// Abstract Base Class for Binary Path operators
    /// </summary>
    public abstract class BaseBinaryPath : ISparqlPath
    {
        /// <summary>
        /// Parts of the Path
        /// </summary>
        protected ISparqlPath _lhs, _rhs;

        /// <summary>
        /// Creates a new Binary Path
        /// </summary>
        /// <param name="lhs">LHS Path</param>
        /// <param name="rhs">RHS Path</param>
        public BaseBinaryPath(ISparqlPath lhs, ISparqlPath rhs)
        {
            _lhs = lhs;
            _rhs = rhs;
        }

        /// <summary>
        /// Gets the LHS Path component
        /// </summary>
        public ISparqlPath LhsPath
        {
            get
            {
                return _lhs;
            }
        }

        /// <summary>
        /// Gets the RHS Path component
        /// </summary>
        public ISparqlPath RhsPath
        {
            get
            {
                return _rhs;
            }
        }

        /// <summary>
        /// Converts a Path into its Algebra Form
        /// </summary>
        /// <param name="context">Path Transformation Context</param>
        /// <returns></returns>
        public abstract ISparqlAlgebra ToAlgebra(PathTransformContext context);

        /// <summary>
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public abstract override String ToString();
    }
}
