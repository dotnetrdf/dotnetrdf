using System.Linq;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Primitives;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder.Assertions
{
    public class BindingsPatternAssertions : ReferenceTypeAssertions<BindingsPattern, BindingsPatternAssertions>
    {
        private readonly PatternItemFactory _patternItemFactory = new PatternItemFactory();

        public BindingsPatternAssertions(BindingsPattern pattern)
        {
            Subject = pattern;
        }

        public AndConstraint<BindingsPatternAssertions> DeclareVariables(string variable)
        {
            Subject.Variables.Should().Contain(variable);

            return new AndConstraint<BindingsPatternAssertions>(this);
        }

        public AndConstraint<BindingsPatternAssertions> ContainTuple(object tuple)
        {
            var type = tuple.GetType();
            var properties = type.GetProperties().ToList();
            var variables = properties.Select(prop => prop.Name).ToList();
            var values = properties.Select(prop => _patternItemFactory.CreateLiteralNodeMatchPattern(prop.GetValue(tuple))).ToList();
            var bindingTuple = new BindingTuple(variables, values);

            Subject.Tuples.Should()
                .Contain(t => variables.All(v => t[v].Equals(bindingTuple[v])));

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