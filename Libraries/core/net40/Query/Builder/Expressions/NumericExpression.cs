using System;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Arithmetic;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    public class NumericExpression<T> : TypedLiteralExpression<T>
    {
        public NumericExpression(T numericValue)
            : base(new ConstantTerm(CreateLiteralNode(numericValue)))
        {
        }

        internal NumericExpression(ISparqlExpression expression) : base(expression)
        {
        }

        private static ILiteralNode CreateLiteralNode(T numericValue)
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

            throw new ArgumentException(string.Format("Unsupported type for numeric node: {0}", typeof(T)));
        }

        public static implicit operator NumericExpression(NumericExpression<T> expression)
        {
            return new NumericExpression(expression.Expression);
        }
    }

    public class NumericExpression : LiteralExpression
    {
        internal NumericExpression(ISparqlExpression expression)
            : base(expression)
        {
        }
    }
}