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
using System.Collections.Generic;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.PropertyFunctions
{
    /// <summary>
    /// Represents information about a property function
    /// </summary>
    public class PropertyFunctionInfo
    {
        private Uri _funcUri;
        private List<IMatchTriplePattern> _patterns = new List<IMatchTriplePattern>();
        private List<PatternItem> _subjArgs = new List<PatternItem>();
        private List<PatternItem> _objArgs = new List<PatternItem>();

        /// <summary>
        /// Creates new function information
        /// </summary>
        /// <param name="u">Function URI</param>
        public PropertyFunctionInfo(Uri u)
        {
            _funcUri = u;
        }

        /// <summary>
        /// Gets the function URI
        /// </summary>
        public Uri FunctionUri
        {
            get
            {
                return _funcUri;
            }
        }

        /// <summary>
        /// Gets the triple patterns that compose the property function
        /// </summary>
        public List<IMatchTriplePattern> Patterns
        {
            get
            {
                return _patterns;
            }
        }

        /// <summary>
        /// Gets the subject arguments to the function
        /// </summary>
        public List<PatternItem> SubjectArgs
        {
            get
            {
                return _subjArgs;
            }
        }

        /// <summary>
        /// Gets the object arguments to the function
        /// </summary>
        public List<PatternItem> ObjectArgs
        {
            get
            {
                return _objArgs;
            }
        }
    }
}
