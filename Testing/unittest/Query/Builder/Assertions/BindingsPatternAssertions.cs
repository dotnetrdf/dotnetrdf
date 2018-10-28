using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder.Assertions
{
    public class BindingsPatternAssertions : ReferenceTypeAssertions<BindingsPattern, BindingsPatternAssertions>
    {
        public BindingsPatternAssertions(BindingsPattern pattern)
        {
            Subject = pattern;
        }

        public AndConstraint<BindingsPatternAssertions> BeEquivalentTo(BindingsPattern expected)
        {
            return Subject.Should()
                .DeclareVariables(expected.Variables)
                .And.HasTuples(expected.Tuples.Count())
                .And.ContainTuples(expected.Tuples);
        }

        public AndConstraint<BindingsPatternAssertions> DeclareVariables(IEnumerable<string> variables)
        {
            Subject.Variables.Should().ContainInOrder(variables);

            return new AndConstraint<BindingsPatternAssertions>(this);
        }

        public AndConstraint<BindingsPatternAssertions> ContainTuples(IEnumerable<BindingTuple> tuples)
        {
            var variables = Subject.Variables;

            foreach (var tuple in tuples)
            {
                Execute.Assertion
                    .Given(() => Subject.Tuples)
                    .ForCondition(actual => actual.Any(t => variables.All(v => Equals(t[v], tuple[v]))))
                    .FailWith("{0} should contain tuple {1}", Subject, tuple);
            }

            return new AndConstraint<BindingsPatternAssertions>(this);
        }

        protected override string Identifier => "BindingsPattern";

        public AndConstraint<BindingsPatternAssertions> HasTuples(int count)
        {
            Subject.Tuples.Should().HaveCount(count);

            return new AndConstraint<BindingsPatternAssertions>(this);
        }
    }
}