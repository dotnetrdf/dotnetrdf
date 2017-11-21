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

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler which simply counts the Triples
    /// </summary>
    public class CountHandler 
        : BaseRdfHandler
    {
        private int _counter = 0;

        /// <summary>
        /// Creates a Handler which counts Triples
        /// </summary>
        public CountHandler()
            : base(new MockNodeFactory())
        { }

        /// <summary>
        /// Resets the current count to zero
        /// </summary>
        protected override void StartRdfInternal()
        {
            _counter = 0;
        }

        /// <summary>
        /// Handles the Triple by incrementing the Triple count
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            _counter++;
            return true;
        }

        /// <summary>
        /// Gets the Count of Triples handled in the most recent parsing operation
        /// </summary>
        /// <remarks>
        /// Note that each time you reuse the handler the count is reset to 0
        /// </remarks>
        public int Count
        {
            get
            {
                return _counter;
            }
        }

        /// <summary>
        /// Gets that the Handler accepts all Triples
        /// </summary>
        public override bool AcceptsAll
        {
            get 
            {
                return true; 
            }
        }
    }
}
