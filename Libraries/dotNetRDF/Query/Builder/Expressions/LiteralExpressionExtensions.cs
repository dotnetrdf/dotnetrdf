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

        private static ILiteralNode ToLiteral<T>(T numericValue)
        {
            if (typeof(T) == typeof(int))
            {
                return ((int)(object)numericValue).ToLiteral(NodeFactory);
            }
            if (typeof(T) == typeof(decimal))
            {
                return ((decimal)(object)numericValue).ToLiteral(NodeFactory);
            }
            if (typeof(T) == typeof(short))
            {
                return ((short)(object)numericValue).ToLiteral(NodeFactory);
            }
            if (typeof(T) == typeof(long))
            {
                return ((long)(object)numericValue).ToLiteral(NodeFactory);
            }
            if (typeof(T) == typeof(float))
            {
                return ((float)(object)numericValue).ToLiteral(NodeFactory);
            }
            if (typeof(T) == typeof(double))
            {
                return ((double)(object)numericValue).ToLiteral(NodeFactory);
            }
            if (typeof(T) == typeof(byte))
            {
                return ((byte)(object)numericValue).ToLiteral(NodeFactory);
            }
            if (typeof(T) == typeof(sbyte))
            {
                return ((sbyte)(object)numericValue).ToLiteral(NodeFactory);
            }
            if (typeof(T) == typeof(string))
            {
                return new LiteralNode(null, numericValue.ToString());
            }
            if (typeof(T) == typeof(DateTime))
            {
                return ((DateTime)(object)numericValue).ToLiteral(NodeFactory);
            }
            if (typeof(T) == typeof(TimeSpan))
            {
                return ((TimeSpan)(object)numericValue).ToLiteral(NodeFactory);
            }
            if (typeof(T) == typeof(bool))
            {
                return ((bool)(object)numericValue).ToLiteral(NodeFactory);
            }

            throw new ArgumentException(String.Format("Unsupported type for literal node: {0}", typeof(T)));
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