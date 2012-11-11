namespace VDS.RDF.Query.Builder.Expressions
{
    public partial class VariableExpression
    {
        public static NumericExpression operator -(VariableExpression left, VariableExpression right)
        {
            return new NumericExpression(left.Expression) - new NumericExpression(right.Expression);
        }

        public static NumericExpression operator +(VariableExpression left, VariableExpression right)
        {
            return new NumericExpression(left.Expression) + new NumericExpression(right.Expression);
        }

        public static NumericExpression operator /(VariableExpression left, VariableExpression right)
        {
            return new NumericExpression(left.Expression) / new NumericExpression(right.Expression);
        }

        public static NumericExpression operator *(VariableExpression left, VariableExpression right)
        {
            return new NumericExpression(left.Expression) * new NumericExpression(right.Expression);
        }

        public static NumericExpression operator -(int left, VariableExpression right)
        {
            return new NumericExpression<int>(left) - new NumericExpression(right.Expression);
        }

        public static NumericExpression operator -(VariableExpression left, int right)
        {
            return new NumericExpression(left.Expression) - new NumericExpression<int>(right);
        }

        public static NumericExpression operator *(int left, VariableExpression right)
        {
            return new NumericExpression<int>(left) * new NumericExpression(right.Expression);
        }

        public static NumericExpression operator *(VariableExpression left, int right)
        {
            return new NumericExpression(left.Expression) * new NumericExpression<int>(right);
        }

        public static NumericExpression operator /(int left, VariableExpression right)
        {
            return new NumericExpression<int>(left) / new NumericExpression(right.Expression);
        }

        public static NumericExpression operator /(VariableExpression left, int right)
        {
            return new NumericExpression(left.Expression) / new NumericExpression<int>(right);
        }

        public static NumericExpression operator +(int left, VariableExpression right)
        {
            return new NumericExpression<int>(left) + new NumericExpression(right.Expression);
        }

        public static NumericExpression operator +(VariableExpression left, int right)
        {
            return new NumericExpression(left.Expression) + new NumericExpression<int>(right);
        }

        public static NumericExpression operator -(short left, VariableExpression right)
        {
            return new NumericExpression<short>(left) - new NumericExpression(right.Expression);
        }

        public static NumericExpression operator -(VariableExpression left, short right)
        {
            return new NumericExpression(left.Expression) - new NumericExpression<short>(right);
        }

        public static NumericExpression operator *(short left, VariableExpression right)
        {
            return new NumericExpression<short>(left) * new NumericExpression(right.Expression);
        }

        public static NumericExpression operator *(VariableExpression left, short right)
        {
            return new NumericExpression(left.Expression) * new NumericExpression<short>(right);
        }

        public static NumericExpression operator /(short left, VariableExpression right)
        {
            return new NumericExpression<short>(left) / new NumericExpression(right.Expression);
        }

        public static NumericExpression operator /(VariableExpression left, short right)
        {
            return new NumericExpression(left.Expression) / new NumericExpression<short>(right);
        }

        public static NumericExpression operator +(short left, VariableExpression right)
        {
            return new NumericExpression<short>(left) + new NumericExpression(right.Expression);
        }

        public static NumericExpression operator +(VariableExpression left, short right)
        {
            return new NumericExpression(left.Expression) + new NumericExpression<short>(right);
        }

        public static NumericExpression operator -(long left, VariableExpression right)
        {
            return new NumericExpression<long>(left) - new NumericExpression(right.Expression);
        }

        public static NumericExpression operator -(VariableExpression left, long right)
        {
            return new NumericExpression(left.Expression) - new NumericExpression<long>(right);
        }

        public static NumericExpression operator *(long left, VariableExpression right)
        {
            return new NumericExpression<long>(left) * new NumericExpression(right.Expression);
        }

        public static NumericExpression operator *(VariableExpression left, long right)
        {
            return new NumericExpression(left.Expression) * new NumericExpression<long>(right);
        }

        public static NumericExpression operator /(long left, VariableExpression right)
        {
            return new NumericExpression<long>(left) / new NumericExpression(right.Expression);
        }

        public static NumericExpression operator /(VariableExpression left, long right)
        {
            return new NumericExpression(left.Expression) / new NumericExpression<long>(right);
        }

        public static NumericExpression operator +(long left, VariableExpression right)
        {
            return new NumericExpression<long>(left) + new NumericExpression(right.Expression);
        }

        public static NumericExpression operator +(VariableExpression left, long right)
        {
            return new NumericExpression(left.Expression) + new NumericExpression<long>(right);
        }

        public static NumericExpression operator -(byte left, VariableExpression right)
        {
            return new NumericExpression<byte>(left) - new NumericExpression(right.Expression);
        }

        public static NumericExpression operator -(VariableExpression left, byte right)
        {
            return new NumericExpression(left.Expression) - new NumericExpression<byte>(right);
        }

        public static NumericExpression operator *(byte left, VariableExpression right)
        {
            return new NumericExpression<byte>(left) * new NumericExpression(right.Expression);
        }

        public static NumericExpression operator *(VariableExpression left, byte right)
        {
            return new NumericExpression(left.Expression) * new NumericExpression<byte>(right);
        }

        public static NumericExpression operator /(byte left, VariableExpression right)
        {
            return new NumericExpression<byte>(left) / new NumericExpression(right.Expression);
        }

        public static NumericExpression operator /(VariableExpression left, byte right)
        {
            return new NumericExpression(left.Expression) / new NumericExpression<byte>(right);
        }

        public static NumericExpression operator +(byte left, VariableExpression right)
        {
            return new NumericExpression<byte>(left) + new NumericExpression(right.Expression);
        }

        public static NumericExpression operator +(VariableExpression left, byte right)
        {
            return new NumericExpression(left.Expression) + new NumericExpression<byte>(right);
        }

        public static NumericExpression operator -(sbyte left, VariableExpression right)
        {
            return new NumericExpression<sbyte>(left) - new NumericExpression(right.Expression);
        }

        public static NumericExpression operator -(VariableExpression left, sbyte right)
        {
            return new NumericExpression(left.Expression) - new NumericExpression<sbyte>(right);
        }

        public static NumericExpression operator *(sbyte left, VariableExpression right)
        {
            return new NumericExpression<sbyte>(left) * new NumericExpression(right.Expression);
        }

        public static NumericExpression operator *(VariableExpression left, sbyte right)
        {
            return new NumericExpression(left.Expression) * new NumericExpression<sbyte>(right);
        }

        public static NumericExpression operator /(sbyte left, VariableExpression right)
        {
            return new NumericExpression<sbyte>(left) / new NumericExpression(right.Expression);
        }

        public static NumericExpression operator /(VariableExpression left, sbyte right)
        {
            return new NumericExpression(left.Expression) / new NumericExpression<sbyte>(right);
        }

        public static NumericExpression operator +(sbyte left, VariableExpression right)
        {
            return new NumericExpression<sbyte>(left) + new NumericExpression(right.Expression);
        }

        public static NumericExpression operator +(VariableExpression left, sbyte right)
        {
            return new NumericExpression(left.Expression) + new NumericExpression<sbyte>(right);
        }

        public static NumericExpression operator -(float left, VariableExpression right)
        {
            return new NumericExpression<float>(left) - new NumericExpression(right.Expression);
        }

        public static NumericExpression operator -(VariableExpression left, float right)
        {
            return new NumericExpression(left.Expression) - new NumericExpression<float>(right);
        }

        public static NumericExpression operator *(float left, VariableExpression right)
        {
            return new NumericExpression<float>(left) * new NumericExpression(right.Expression);
        }

        public static NumericExpression operator *(VariableExpression left, float right)
        {
            return new NumericExpression(left.Expression) * new NumericExpression<float>(right);
        }

        public static NumericExpression operator /(float left, VariableExpression right)
        {
            return new NumericExpression<float>(left) / new NumericExpression(right.Expression);
        }

        public static NumericExpression operator /(VariableExpression left, float right)
        {
            return new NumericExpression(left.Expression) / new NumericExpression<float>(right);
        }

        public static NumericExpression operator +(float left, VariableExpression right)
        {
            return new NumericExpression<float>(left) + new NumericExpression(right.Expression);
        }

        public static NumericExpression operator +(VariableExpression left, float right)
        {
            return new NumericExpression(left.Expression) + new NumericExpression<float>(right);
        }

        public static NumericExpression operator -(double left, VariableExpression right)
        {
            return new NumericExpression<double>(left) - new NumericExpression(right.Expression);
        }

        public static NumericExpression operator -(VariableExpression left, double right)
        {
            return new NumericExpression(left.Expression) - new NumericExpression<double>(right);
        }

        public static NumericExpression operator *(double left, VariableExpression right)
        {
            return new NumericExpression<double>(left) * new NumericExpression(right.Expression);
        }

        public static NumericExpression operator *(VariableExpression left, double right)
        {
            return new NumericExpression(left.Expression) * new NumericExpression<double>(right);
        }

        public static NumericExpression operator /(double left, VariableExpression right)
        {
            return new NumericExpression<double>(left) / new NumericExpression(right.Expression);
        }

        public static NumericExpression operator /(VariableExpression left, double right)
        {
            return new NumericExpression(left.Expression) / new NumericExpression<double>(right);
        }

        public static NumericExpression operator +(double left, VariableExpression right)
        {
            return new NumericExpression<double>(left) + new NumericExpression(right.Expression);
        }

        public static NumericExpression operator +(VariableExpression left, double right)
        {
            return new NumericExpression(left.Expression) + new NumericExpression<double>(right);
        }

        public static NumericExpression operator -(decimal left, VariableExpression right)
        {
            return new NumericExpression<decimal>(left) - new NumericExpression(right.Expression);
        }

        public static NumericExpression operator -(VariableExpression left, decimal right)
        {
            return new NumericExpression(left.Expression) - new NumericExpression<decimal>(right);
        }

        public static NumericExpression operator *(decimal left, VariableExpression right)
        {
            return new NumericExpression<decimal>(left) * new NumericExpression(right.Expression);
        }

        public static NumericExpression operator *(VariableExpression left, decimal right)
        {
            return new NumericExpression(left.Expression) * new NumericExpression<decimal>(right);
        }

        public static NumericExpression operator /(decimal left, VariableExpression right)
        {
            return new NumericExpression<decimal>(left) / new NumericExpression(right.Expression);
        }

        public static NumericExpression operator /(VariableExpression left, decimal right)
        {
            return new NumericExpression(left.Expression) / new NumericExpression<decimal>(right);
        }

        public static NumericExpression operator +(decimal left, VariableExpression right)
        {
            return new NumericExpression<decimal>(left) + new NumericExpression(right.Expression);
        }

        public static NumericExpression operator +(VariableExpression left, decimal right)
        {
            return new NumericExpression(left.Expression) + new NumericExpression<decimal>(right);
        }
    }
}