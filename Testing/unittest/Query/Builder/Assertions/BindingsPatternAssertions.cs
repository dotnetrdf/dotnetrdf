using System.Linq;
using System.Reflection;
using FluentAssertions;
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

        public AndConstraint<BindingsPatternAssertions> DeclareVariables(params string[] variables)
        {
            Subject.Variables.Should().ContainInOrder(variables);

            return new AndConstraint<BindingsPatternAssertions>(this);
        }

        public AndConstraint<BindingsPatternAssertions> ContainTuple(object tuple)
        {
            var type = tuple.GetType();
            var properties = type.GetProperties().ToList();
            var variables = properties.Select(prop => prop.Name).ToList();
            var values = properties.ToDictionary(
                prop => prop.Name,
                prop => (INode) prop.GetValue(tuple));

            Subject.Tuples.Should()
                .Contain(
                    t => variables.All(v => Equals(t[v], values[v])),
                    "VALUES should contain tuple ( {0} )",
                    string.Join(", ", values.Values.Select(v => v?.ToString() ?? "UNDEF")));

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