using Xunit;

namespace VDS.RDF.JsonLd;

public class DnrJsonLd11TestSuite : JsonLdTestSuiteBase
{
    public DnrJsonLd11TestSuite(ITestOutputHelper output): base(output) { }

    [Theory]
    [MemberData(nameof(JsonLdTestSuiteDataSource.DnrFrameTests), MemberType = typeof(JsonLdTestSuiteDataSource))]
    public override void JsonLdFramingTests(string testId, JsonLdTestType testType, string inputPath, string framePath,
        string expectedOutputPath, JsonLdErrorCode expectErrorCode, string processingMode,
        bool pruneBlankNodeIdentifiers, bool? omitGraph, bool ordered)
    {
        base.JsonLdFramingTests(testId, testType, inputPath, framePath, expectedOutputPath, expectErrorCode, processingMode,
            pruneBlankNodeIdentifiers, omitGraph, ordered);
    }
}
