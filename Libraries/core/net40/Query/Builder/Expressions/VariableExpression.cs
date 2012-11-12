using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    /// <summary>
    /// Represents an expression, which evaluates to a variable
    /// </summary>
    public partial class VariableExpression : SparqlExpression
    {
        internal VariableExpression(string variable)
            : base(new VariableTerm(variable))
        {
        }

        /// <summary>
        /// Gets the <see cref="VariableTerm"/> represented by this variable expression
        /// </summary>
        public new VariableTerm Expression
        {
            get
            {
                return (VariableTerm)base.Expression;
            }
        }

#pragma warning disable 1591
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
#pragma warning restore 1591
    }
}