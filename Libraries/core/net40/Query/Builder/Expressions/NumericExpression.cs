using System;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Arithmetic;
using VDS.RDF.Query.Expressions.Functions.XPath.Cast;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    public class NumericExpression<T> : TypedLiteralExpression<T>
    {
        public NumericExpression(T numericValue)
            : base(new ConstantTerm(CreateLiteralNode(numericValue)))
        {
        }

        internal NumericExpression(ISparqlExpression expression)
            : base(expression)
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

        public NumericExpression<T> Multiply(T right)
        {
            return Multiply(new NumericExpression<T>(right));
        }

        public NumericExpression<T> Multiply(NumericExpression<T> right)
        {
            var multiplication = new MultiplicationExpression(Expression, right.Expression);
            return new NumericExpression<T>(multiplication);
        }

        public NumericExpression Multiply(NumericExpression right)
        {
            return new NumericExpression(Expression).Multiply(right);
        }

        public NumericExpression<T> Divide(T right)
        {
            return Divide(new NumericExpression<T>(right));
        }

        public NumericExpression<T> Divide(NumericExpression<T> right)
        {
            var division = new DivisionExpression(Expression, right.Expression);
            return new NumericExpression<T>(division);
        }

        public NumericExpression Divide(NumericExpression right)
        {
            return new NumericExpression(Expression).Divide(right);
        }

        public NumericExpression<T> Add(T right)
        {
            return Add(new NumericExpression<T>(right));
        }

        public NumericExpression<T> Add(NumericExpression<T> right)
        {
            var addition = new AdditionExpression(Expression, right.Expression);
            return new NumericExpression<T>(addition);
        }

        public NumericExpression Add(NumericExpression right)
        {
            return new NumericExpression(Expression).Add(right);
        }

        public NumericExpression<T> Subtract(T right)
        {
            return this.Subtract(new NumericExpression<T>(right));
        }

        public NumericExpression<T> Subtract(NumericExpression<T> right)
        {
            var subtraction = new SubtractionExpression(Expression, right.Expression);
            return new NumericExpression<T>(subtraction);
        }

        public NumericExpression Subtract(NumericExpression right)
        {
            return new NumericExpression(Expression).Subtract(right);
        }
    }

    public class NumericExpression : LiteralExpression
    {
        internal NumericExpression(ISparqlExpression expression)
            : base(expression)
        {
        }

        public NumericExpression Multiply(NumericExpression right)
        {
            return new NumericExpression(new MultiplicationExpression(Expression, right.Expression));
        }

        public NumericExpression Multiply(int right)
        {
            return Multiply(new NumericExpression<int>(right));
        }

        public NumericExpression Divide(NumericExpression right)
        {
            return new NumericExpression(new DivisionExpression(Expression, right.Expression));
        }

        public NumericExpression Divide(int right)
        {
            return Divide(new NumericExpression<int>(right));
        }

        public NumericExpression Add(NumericExpression right)
        {
            return new NumericExpression(new AdditionExpression(Expression, right.Expression));
        }

        public NumericExpression Add(int right)
        {
            return Add(new NumericExpression<int>(right));
        }

        public NumericExpression Subtract(NumericExpression right)
        {
            return new NumericExpression(new SubtractionExpression(Expression, right.Expression));
        }

        public NumericExpression Subtract(int right)
        {
            return Subtract(new NumericExpression<int>(right));
        }
    }
}