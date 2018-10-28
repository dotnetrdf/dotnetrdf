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
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    internal static class LiteralExpressionExtensions
    {
        private static readonly NodeFactory NodeFactory = new NodeFactory();

        internal static ILiteralNode ToLiteral(object value)
        {
            switch (value)
            {
                case int i:
                    return i.ToLiteral(NodeFactory);
                case decimal d:
                    return d.ToLiteral(NodeFactory);
                case short s:
                    return s.ToLiteral(NodeFactory);
                case long l:
                    return l.ToLiteral(NodeFactory);
                case float f:
                    return f.ToLiteral(NodeFactory);
                case double d:
                    return d.ToLiteral(NodeFactory);
                case byte b:
                    return b.ToLiteral(NodeFactory);
                case sbyte s:
                    return s.ToLiteral(NodeFactory);
                case string s:
                    return new LiteralNode(null, s);
                case DateTime d:
                    return d.ToLiteral(NodeFactory);
                case TimeSpan t:
                    return t.ToLiteral(NodeFactory);
                case bool b:
                    return b.ToLiteral(NodeFactory);
                default:
                    throw new ArgumentException(string.Format("Unsupported type for literal node: {0}", value.GetType()));
            }
        }

        /// <summary>
        /// Creates a typed literal term
        /// </summary>
        internal static ConstantTerm ToConstantTerm<T>(this T value)
        {
            return new ConstantTerm(ToLiteral(value));
        }

        /// <summary>
        /// Creates an untyped literal term (simple literal)
        /// </summary>
        internal static ConstantTerm ToSimpleLiteral(this string value)
        {
            return new ConstantTerm(new LiteralNode(null, value));
        }
    }
}