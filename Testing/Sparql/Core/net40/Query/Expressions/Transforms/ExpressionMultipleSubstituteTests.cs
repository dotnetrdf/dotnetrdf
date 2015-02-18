using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions.Aggregates.Sparql;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions.Transforms
{
    [TestFixture]
    public class ExpressionMultipleSubstituteTests
        : AbstractExpressionTransformTests
    {
        private IDictionary<IExpression, IExpression> Substitutions { get; set; }

        protected override IExpressionTransform CreateInstance()
        {
            return new ExprTransformMultipleSubstitute(this.Substitutions);
        }

        [SetUp]
        public void Setup()
        {
            this.Substitutions = new Dictionary<IExpression, IExpression>();
        }
        
        [Test]
        public void ExpressionMultipleSubstitute1()
        {
            IExpression x = new VariableTerm("x");
            IExpression y = new VariableTerm("y");

            this.Substitutions.Add(x, y);

            this.CheckTransform(x, y);
        }

        [Test]
        public void ExpressionMultipleSubstitute2()
        {
            IExpression agg = new CountAllAggregate();
            IExpression threshold = new ConstantTerm(new LongNode(100));
            IExpression condition = new GreaterThanExpression(agg, threshold);
            IExpression tempVar = new VariableTerm(".0");
            IExpression transformedCondition = new GreaterThanExpression(tempVar, threshold);

            this.Substitutions.Add(agg, tempVar);

            this.CheckTransform(condition, transformedCondition);
        }

    }
}
