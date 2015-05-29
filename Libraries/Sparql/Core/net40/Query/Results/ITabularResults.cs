/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;

namespace VDS.RDF.Query.Results
{
    /// <summary>
    /// Represents tabular results
    /// </summary>
    public interface ITabularResults
        : IEnumerable<IResultRow>, IDisposable
    {
        /// <summary>
        /// Gets whether the table of results is streaming i.e. single use
        /// </summary>
        /// <remarks>
        /// If this returns true then the enumerator returned by these results is only guaranteed to be valid once and further attempt to obtain an enumerator may result in an error.  If this is false then the results may be enumerated as many times as desired
        /// </remarks>
        bool IsStreaming { get; }

        /// <summary>
        /// Gets the enumeration of variables present in these results
        /// </summary>
        /// <remarks>
        /// If <see cref="IsStreaming"/> returns true then the enumerator returned here is only guaranteed to be valid once and subsequent attempts to obtain and enumerate may result in an error, if this is false then this enumerator may be enumerated as many times as desired.
        /// </remarks>
        IEnumerable<String> Variables { get; } 
    }
}
