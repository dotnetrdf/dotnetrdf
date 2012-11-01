using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;

namespace VDS.RDF.Query.Builder
{
    public static class ExpressionBuilderEqualityExtensions
    {
        public static BooleanExpression Eq(this VariableExpression leftExpression, VariableExpression rightExpression)
        {
            return Eq(leftExpression.Expression, rightExpression.Expression);
        }

        public static BooleanExpression Eq(this RdfTermExpression leftExpression, VariableExpression rightExpression)
        {
            return Eq(leftExpression.Expression, rightExpression.Expression);
        }

        public static BooleanExpression Eq(this VariableExpression leftExpression, RdfTermExpression rightExpression)
        {
            return Eq(leftExpression.Expression, rightExpression.Expression);
        }

        public static BooleanExpression Eq(this RdfTermExpression leftExpression, RdfTermExpression rightExpression)
        {
            return Eq(leftExpression.Expression, rightExpression.Expression);
        }


        public static BooleanExpression Gt(this VariableExpression leftExpression, VariableExpression rightExpression)
        {
            return Gt(leftExpression.Expression, rightExpression.Expression);
        }

        public static BooleanExpression Gt(this RdfTermExpression leftExpression, VariableExpression rightExpression)
        {
            return Gt(leftExpression.Expression, rightExpression.Expression);
        }

        public static BooleanExpression Gt(this VariableExpression leftExpression, RdfTermExpression rightExpression)
        {
            return Gt(leftExpression.Expression, rightExpression.Expression);
        }

        public static BooleanExpression Gt(this RdfTermExpression leftExpression, RdfTermExpression rightExpression)
        {
            return Gt(leftExpression.Expression, rightExpression.Expression);
        }


        public static BooleanExpression Lt(this VariableExpression leftExpression, VariableExpression rightExpression)
        {
            return Lt(leftExpression.Expression, rightExpression.Expression);
        }

        public static BooleanExpression Lt(this RdfTermExpression leftExpression, VariableExpression rightExpression)
        {
            return Lt(leftExpression.Expression, rightExpression.Expression);
        }

        public static BooleanExpression Lt(this VariableExpression leftExpression, RdfTermExpression rightExpression)
        {
            return Lt(leftExpression.Expression, rightExpression.Expression);
        }

        public static BooleanExpression Lt(this RdfTermExpression leftExpression, RdfTermExpression rightExpression)
        {
            return Lt(leftExpression.Expression, rightExpression.Expression);
        }


        public static BooleanExpression Ge(this VariableExpression leftExpression, VariableExpression rightExpression)
        {
            return Ge(leftExpression.Expression, rightExpression.Expression);
        }

        public static BooleanExpression Ge(this RdfTermExpression leftExpression, VariableExpression rightExpression)
        {
            return Ge(leftExpression.Expression, rightExpression.Expression);
        }

        public static BooleanExpression Ge(this VariableExpression leftExpression, RdfTermExpression rightExpression)
        {
            return Ge(leftExpression.Expression, rightExpression.Expression);
        }

        public static BooleanExpression Ge(this RdfTermExpression leftExpression, RdfTermExpression rightExpression)
        {
            return Ge(leftExpression.Expression, rightExpression.Expression);
        }


        public static BooleanExpression Le(this VariableExpression leftExpression, VariableExpression rightExpression)
        {
            return Le(leftExpression.Expression, rightExpression.Expression);
        }

        public static BooleanExpression Le(this RdfTermExpression leftExpression, VariableExpression rightExpression)
        {
            return Le(leftExpression.Expression, rightExpression.Expression);
        }

        public static BooleanExpression Le(this VariableExpression leftExpression, RdfTermExpression rightExpression)
        {
            return Le(leftExpression.Expression, rightExpression.Expression);
        }

        public static BooleanExpression Le(this RdfTermExpression leftExpression, RdfTermExpression rightExpression)
        {
            return Le(leftExpression.Expression, rightExpression.Expression);
        }


        private static BooleanExpression Eq(ISparqlExpression left, ISparqlExpression right)
        {
            ISparqlExpression equalsExpression = new EqualsExpression(left, right);
            return new BooleanExpression(equalsExpression);
        }

        private static BooleanExpression Gt(ISparqlExpression left, ISparqlExpression right)
        {
            ISparqlExpression equalsExpression = new GreaterThanExpression(left, right);
            return new BooleanExpression(equalsExpression);
        }

        private static BooleanExpression Lt(ISparqlExpression left, ISparqlExpression right)
        {
            ISparqlExpression equalsExpression = new LessThanExpression(left, right);
            return new BooleanExpression(equalsExpression);
        }

        private static BooleanExpression Ge(ISparqlExpression left, ISparqlExpression right)
        {
            ISparqlExpression equalsExpression = new GreaterThanOrEqualToExpression(left, right);
            return new BooleanExpression(equalsExpression);
        }

        private static BooleanExpression Le(ISparqlExpression left, ISparqlExpression right)
        {
            ISparqlExpression equalsExpression = new LessThanOrEqualToExpression(left, right);
            return new BooleanExpression(equalsExpression);
        } 
    }
}