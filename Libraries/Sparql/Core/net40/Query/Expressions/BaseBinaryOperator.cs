using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Specifications;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions
{
    public abstract class BaseBinaryOperator
        : BaseBinaryExpression
    {
        public BaseBinaryOperator(IExpression leftExpr, IExpression rightExpr) 
            : base(leftExpr, rightExpr) {}

        public override string ToString(IAlgebraFormatter formatter)
        {
            return String.Format("{0}({1}, {2})", this.Functor, this.FirstArgument.ToString(formatter), this.SecondArgument.ToString(formatter));
        }

        public override string ToPrefixString(IAlgebraFormatter formatter)
        {
            return String.Format("({0} {1} {2})", this.Functor, this.FirstArgument.ToPrefixString(formatter), this.SecondArgument.ToPrefixString(formatter));
        }
    }
}
