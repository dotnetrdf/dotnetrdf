/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.IO;

namespace VDS.RDF
{
    /// <summary>
    /// Interface for parsers that generate objects of some sort
    /// </summary>
    /// <typeparam name="T">Generated Object Type</typeparam>
    /// <remarks>
    /// <para>
    /// Primarily used as a marker interface in relation to <see cref="IOManager" /> to provide a mechanism whereby parsers for arbitrary objects can be registered and associated with MIME Types and File Extensions
    /// </para>
    /// </remarks>
    public interface IObjectParser<out T>
    {
        /// <summary>
        /// Parses an Object from some input
        /// </summary>
        /// <param name="input">Text Stream</param>
        /// <returns>Parsed object</returns>
        T Parse(TextReader input);

        /// <summary>
        /// Parses an Object from a String
        /// </summary>
        /// <param name="data">String</param>
        /// <returns>Parsed object</returns>
        /// <remarks>
        /// In most cases the implementation of this method is likely to be purely for convinience, it will simply wrap the string in a <see cref="StringReader"/> and call the <see cref="Parse(TextReader)"/> method
        /// </remarks>
        T ParseFromString(String data);
    }
}
