using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    public partial class VariableExpression : SparqlExpression
    {
        public VariableExpression(string variable)
            : base(new VariableTerm(variable))
        {
        }

        public new VariableTerm Expression
        {
            get
            {
                return (VariableTerm)base.Expression;
            }
        }

        public static BooleanExpression operator >(VariableExpression left, VariableExpression right)
        {
            return Gt(left.Expression, right.Expression);
        }

        public static BooleanExpression operator <(VariableExpression left, VariableExpression right)
        {
            return Lt(left.Expression, right.Expression);
        }

        public static BooleanExpression operator >=(VariableExpression left, VariableExpression right)
        {
            return Ge(left.Expression, right.Expression);
        }

        public static BooleanExpression operator <=(VariableExpression left, VariableExpression right)
        {
            return Le(left.Expression, right.Expression);
        }
    }
}