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
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A SPARQL Results Handler which loads directly into a <see cref="Multiset">Multiset</see>
    /// </summary>
    /// <remarks>
    /// Primarily intended for internal usage for future optimisation of some SPARQL evaluation
    /// </remarks>
    public class MultisetHandler 
        : BaseResultsHandler
    {
        private Multiset _mset;

        /// <summary>
        /// Creates a new Multiset Handler
        /// </summary>
        /// <param name="mset">Multiset</param>
        public MultisetHandler(Multiset mset)
        {
            if (mset == null) throw new ArgumentNullException("mset", "Multiset to load into cannot be null");
            _mset = mset;
        }

        /// <summary>
        /// Handles a Boolean Result by doing nothing
        /// </summary>
        /// <param name="result">Boolean Result</param>
        protected override void HandleBooleanResultInternal(bool result)
        {
            // Does Nothing
        }

        /// <summary>
        /// Handles a Variable by adding it to the Multiset
        /// </summary>
        /// <param name="var">Variable</param>
        /// <returns></returns>
        protected override bool HandleVariableInternal(string var)
        {
            _mset.AddVariable(var);
            return true;
        }

        /// <summary>
        /// Handles a Result by adding it to the Multiset
        /// </summary>
        /// <param name="result">Result</param>
        /// <returns></returns>
        protected override bool HandleResultInternal(SparqlResult result)
        {
            _mset.Add(new Set(result));
            return true;
        }
    }
}
