/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2023 dotNetRDF Project (http://dotnetrdf.org/)
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
    /// An Algebra Optimiser is a class that can transform a SPARQL algebra from one form to another typically for optimisation purposes.
    /// </summary>
    public interface IAlgebraOptimiser
    {
        /// <summary>
        /// Optimises the given Algebra.
        /// </summary>
        /// <param name="algebra">Algebra to optimise.</param>
        /// <returns></returns>
        /// <remarks>
        /// <strong>Important:</strong> An Algebra Optimiser must guarantee to return an equivalent algebra to the given algebra.  In the event of any error the optimiser <em>should</em> still return a valid algebra (or at least the original algebra).
        /// </remarks>
        ISparqlAlgebra Optimise(ISparqlAlgebra algebra);

        /// <summary>
        /// Determines whether an Optimiser is applicable based on the Query whose Algebra is being optimised.
        /// </summary>
        /// <param name="q">SPARQL Query.</param>
        /// <returns></returns>
        bool IsApplicable(SparqlQuery q);

        /// <summary>
        /// Determines whether an Optimiser is applicable based on the Update Command Set being optimised.
        /// </summary>
        /// <param name="cmds">Update Command Set.</param>
        /// <returns></returns>
        bool IsApplicable(SparqlUpdateCommandSet cmds);

        /// <summary>
        /// Determines whether an Optimiser will perform algebra optimizations that are potentially unsafe at execution time.
        /// </summary>
        bool UnsafeOptimisation { get; set; }
    }
}
