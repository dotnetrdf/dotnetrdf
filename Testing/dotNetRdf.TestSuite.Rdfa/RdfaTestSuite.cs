using dotNetRdf.TestSupport;
using Xunit.Abstractions;

namespace dotNetRdf.TestSuite.Rdfa
{
    public class RdfaTestSuite : RdfTestSuite
    {
        private readonly ITestOutputHelper _output;

        public static ManifestTestDataProvider RdfaTests = new ManifestTestDataProvider(
            new Uri("http://rdfa.info/test-suite/manifest.jsonld"),
            Path.Combine("resources", "test-suite", "manifest.jsonld"));

        public RdfaTestSuite(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [MemberData(nameof(RdfaTests))]
        public void RunTest(ManifestTestData t)
        {
            _output.WriteLine($"{t.Id}: {t.Name} is a {t.Type}");
        }
    }
}