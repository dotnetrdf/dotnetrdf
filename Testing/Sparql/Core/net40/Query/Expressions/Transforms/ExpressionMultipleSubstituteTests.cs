using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions.Transforms
{
    [TestFixture]
    public class ExpressionMultipleSubstituteTests
        : AbstractExpressionTransformTests
    {
        private IDictionary<IExpression, IExpression> Substitutions { get; set; }
        
        [Test]
        public void ExpressionMultipleSubstitute1()
        {
            IExpression x = new VariableTerm("x");
            IExpression y = new VariableTerm("y");

            this.Substitutions = new Dictionary<IExpression, IExpression>();
            this.Substitutions.Add(x, y);

            this.CheckTransform(x, y);
        }

        protected override IExpressionTransform CreateInstance()
        {
            return new ExprTransformMultipleSubstitute(this.Substitutions);
        }
    }
}
