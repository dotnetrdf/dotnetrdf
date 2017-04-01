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

namespace VDS.RDF
{
    /// <summary>
    /// A Namespace Mapper which has an explicit notion of Nesting
    /// </summary>
    public interface INestedNamespaceMapper : INamespaceMapper
    {
        /// <summary>
        /// Gets the Nesting Level at which the given Namespace is definition is defined
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <returns></returns>
        int GetNestingLevel(string prefix);

        /// <summary>
        /// Increments the Nesting Level
        /// </summary>
        void IncrementNesting();

        /// <summary>
        /// Decrements the Nesting Level
        /// </summary>
        /// <remarks>
        /// When the Nesting Level is decremented any Namespaces defined at a greater Nesting Level are now out of scope and so are removed from the Mapper
        /// </remarks>
        void DecrementNesting();

        /// <summary>
        /// Gets the current Nesting Level
        /// </summary>
        int NestingLevel { get; }
    }
}