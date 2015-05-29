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

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Effective numeric type for a Node
    /// </summary>
    /// <remarks>
    /// Used to represent what kind of numeric term a node is considered to represent and thus how its value may be retrieved
    /// </remarks>
    public enum EffectiveNumericType : int
    {
        /// <summary>
        /// Not a Number
        /// </summary>
        NaN = -1,
        /// <summary>
        /// An Integer
        /// </summary>
        Integer = 0,
        /// <summary>
        /// A Decimal
        /// </summary>
        Decimal = 1,
        /// <summary>
        /// A Single precision Floating Point
        /// </summary>
        Float = 2,
        /// <summary>
        /// A Double precision Floating Point
        /// </summary>
        Double = 3
    }
}
