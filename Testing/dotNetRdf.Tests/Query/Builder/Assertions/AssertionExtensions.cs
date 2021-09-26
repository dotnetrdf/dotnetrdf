using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder.Assertions
{
    public static class AssertionExtensions
    {
        public static BindingsPatternAssertions Should(this BindingsPattern pattern)
        {
            return new BindingsPatternAssertions(pattern);
        }
    }
}