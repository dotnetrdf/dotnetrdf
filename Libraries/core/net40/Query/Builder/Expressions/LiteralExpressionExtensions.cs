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