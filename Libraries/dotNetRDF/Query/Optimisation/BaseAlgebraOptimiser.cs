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

using VDS.RDF.Query.Algebra;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation
{

    /// <summary>
    /// Abstract Base Class for Algebra Transformers where the Transformer may care about the depth of the Algebra in the Algebra Tree
    /// </summary>
    public abstract class BaseAlgebraOptimiser : IAlgebraOptimiser
    {
        /// <summary>
        /// Attempts to optimise an Algebra to another more optimal form
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <returns></returns>
        public virtual ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
        {
            return OptimiseInternal(algebra, 0);
        }

        /// <summary>
        /// Transforms the Algebra to another form tracking the depth in the Algebra tree
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="depth">Depth</param>
        /// <returns></returns>
        protected abstract ISparqlAlgebra OptimiseInternal(ISparqlAlgebra algebra, int depth);

        /// <summary>
        /// Determines whether the Optimiser can be applied to a given Query
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        public abstract bool IsApplicable(SparqlQuery q);

        /// <summary>
        /// Determines whether the Optimiser can be applied to a given Update Command Set
        /// </summary>
        /// <param name="cmds">Command Set</param>
        /// <returns></returns>
        public abstract bool IsApplicable(SparqlUpdateCommandSet cmds);
    }
}
