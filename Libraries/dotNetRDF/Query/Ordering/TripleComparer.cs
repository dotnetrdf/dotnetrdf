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

namespace VDS.RDF.Query.Ordering
{
    class TripleComparer : IComparer<Triple>
    {
        private IComparer<Triple> _child;
        private Func<Triple, Triple, int> _compareFunc;
        private int _modifier = 1;

        public TripleComparer(Func<Triple, Triple, int> compareFunc, bool descending, IComparer<Triple> child)
        {
            _compareFunc = compareFunc;
            _child = child;
            if (descending)
            {
                _modifier = -1;
            }
        }

        public TripleComparer(Func<Triple, Triple, int> compareFunc, IComparer<Triple> child)
            : this(compareFunc, false, child) { }

        public TripleComparer(Func<Triple, Triple, int> compareFunc)
            : this(compareFunc, null) { }

        public int Compare(Triple x, Triple y)
        {
            if (_compareFunc == null)
            {
                return 0;
            }
            else
            {
                int c = _compareFunc(x, y);
                if (c == 0)
                {
                    if (_child != null)
                    {
                        return _modifier * _child.Compare(x, y);
                    }
                    else
                    {
                        return _modifier * c;
                    }
                }
                else
                {
                    return _modifier * c;
                }
            }
        }
    }
}
