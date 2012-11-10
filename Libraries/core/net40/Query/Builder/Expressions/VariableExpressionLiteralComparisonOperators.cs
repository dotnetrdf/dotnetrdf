using System;

namespace VDS.RDF.Query.Builder.Expressions
{
    public partial class VariableExpression 
    {
        public static BooleanExpression operator >(int left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) > right;
        }

        public static BooleanExpression operator <(int left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) < right;
        }

        public static BooleanExpression operator >(VariableExpression left, int right)
        {
            return left > new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator <(VariableExpression left, int right)
        {
            return left < new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator >(decimal left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) > right;
        }

        public static BooleanExpression operator <(decimal left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) < right;
        }

        public static BooleanExpression operator >(VariableExpression left, decimal right)
        {
            return left > new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator <(VariableExpression left, decimal right)
        {
            return left < new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator >(float left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) < right;
        }

        public static BooleanExpression operator <(float left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) < right;
        }

        public static BooleanExpression operator >(VariableExpression left, float right)
        {
            return left > new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator <(VariableExpression left, float right)
        {
            return left < new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator >(double left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) < right;
        }

        public static BooleanExpression operator <(double left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) < right;
        }

        public static BooleanExpression operator >(VariableExpression left, double right)
        {
            return left > new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator <(VariableExpression left, double right)
        {
            return left < new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator >(string left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) > right;
        }

        public static BooleanExpression operator <(string left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) < right;
        }

        public static BooleanExpression operator >(VariableExpression left, string right)
        {
            return left > new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator <(VariableExpression left, string right)
        {
            return left < new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator >(bool left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) > right;
        }

        public static BooleanExpression operator <(bool left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) < right;
        }

        public static BooleanExpression operator >(VariableExpression left, bool right)
        {
            return left > new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator <(VariableExpression left, bool right)
        {
            return left < new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator >(DateTime left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) > right;
        }

        public static BooleanExpression operator <(DateTime left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) < right;
        }

        public static BooleanExpression operator >(VariableExpression left, DateTime right)
        {
            return left > new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator <(VariableExpression left, DateTime right)
        {
            return left < new LiteralExpression(CreateConstantTerm(right));
        }
        
        public static BooleanExpression operator >=(int left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) >= right;
        }

        public static BooleanExpression operator <=(int left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) <= right;
        }

        public static BooleanExpression operator >=(VariableExpression left, int right)
        {
            return left >= new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator <=(VariableExpression left, int right)
        {
            return left <= new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator >=(decimal left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) >= right;
        }

        public static BooleanExpression operator <=(decimal left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) <= right;
        }

        public static BooleanExpression operator >=(VariableExpression left, decimal right)
        {
            return left >= new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator <=(VariableExpression left, decimal right)
        {
            return left <= new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator >=(float left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) <= right;
        }

        public static BooleanExpression operator <=(float left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) <= right;
        }

        public static BooleanExpression operator >=(VariableExpression left, float right)
        {
            return left >= new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator <=(VariableExpression left, float right)
        {
            return left <= new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator >=(double left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) <= right;
        }

        public static BooleanExpression operator <=(double left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) <= right;
        }

        public static BooleanExpression operator >=(VariableExpression left, double right)
        {
            return left >= new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator <=(VariableExpression left, double right)
        {
            return left <= new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator >=(string left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) >= right;
        }

        public static BooleanExpression operator <=(string left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) <= right;
        }

        public static BooleanExpression operator >=(VariableExpression left, string right)
        {
            return left >= new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator <=(VariableExpression left, string right)
        {
            return left <= new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator >=(bool left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) >= right;
        }

        public static BooleanExpression operator <=(bool left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) <= right;
        }

        public static BooleanExpression operator >=(VariableExpression left, bool right)
        {
            return left >= new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator <=(VariableExpression left, bool right)
        {
            return left <= new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator >=(DateTime left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) >= right;
        }

        public static BooleanExpression operator <=(DateTime left, VariableExpression right)
        {
            return new LiteralExpression(CreateConstantTerm(left)) <= right;
        }

        public static BooleanExpression operator >=(VariableExpression left, DateTime right)
        {
            return left >= new LiteralExpression(CreateConstantTerm(right));
        }

        public static BooleanExpression operator <=(VariableExpression left, DateTime right)
        {
            return left <= new LiteralExpression(CreateConstantTerm(right));
        }
    }
}