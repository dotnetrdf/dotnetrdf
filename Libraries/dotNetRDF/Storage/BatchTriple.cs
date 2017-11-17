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

namespace VDS.RDF.Storage
{

    /// <summary>
    /// Structure for representing Triples that are waiting to be Batch written to the Database
    /// </summary>
    public struct BatchTriple
    {
        private Triple _t;
        private String _graphID;

        /// <summary>
        /// Creates a new Batch Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <param name="graphID">Graph ID to store Triple for</param>
        public BatchTriple(Triple t, String graphID)
        {
            _t = t;
            _graphID = graphID;
        }

        /// <summary>
        /// Triple
        /// </summary>
        public Triple Triple
        {
            get
            {
                return _t;
            }
        }

        /// <summary>
        /// Graph ID
        /// </summary>
        public String GraphID
        {
            get
            {
                return _graphID;
            }
        }

        /// <summary>
        /// Equality for Batch Triples
        /// </summary>
        /// <param name="obj">Object to test</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is BatchTriple)
            {
                BatchTriple other = (BatchTriple)obj;
                return _graphID == other.GraphID && _t.Equals(other.Triple);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Hash Code for Batch Triples
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (_graphID + _t.GetHashCode()).GetHashCode();
        }
    }
}