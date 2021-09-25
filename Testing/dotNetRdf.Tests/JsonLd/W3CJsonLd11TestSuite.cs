using System.Collections.Generic;
using Xunit;

namespace VDS.RDF.JsonLd
{
    public class W3CJsonLd11TestSuite : JsonLdTestSuiteBase
    {

        [Theory]
        [MemberData(nameof(JsonLdTestSuiteDataSource.W3CExpandTests), MemberType = typeof(JsonLdTestSuiteDataSource))]
        public override void ExpandTests(string testId, JsonLdTestType testType, string inputPath, string contextPath,
            string expectedOutputPath, JsonLdErrorCode expectErrorCode, string baseIri,
            string processorMode, string expandContextPath, bool compactArrays, string rdfDirection)
        {
            base.ExpandTests(testId, testType, inputPath, contextPath, expectedOutputPath, expectErrorCode,
                baseIri, processorMode, expandContextPath, compactArrays, rdfDirection);
        }

        [Theory]
        [MemberData(nameof(JsonLdTestSuiteDataSource.W3CCompactTests), MemberType = typeof(JsonLdTestSuiteDataSource))]
        public override void CompactTests(string testId, JsonLdTestType testType, string inputPath, string contextPath,
            string expectedOutputPath, JsonLdErrorCode expectedErrorCode, string baseIri,
            string processorMode, string expandContextPath, bool compactArrays, string rdfDirection)
        {
            base.CompactTests(testId, testType, inputPath, contextPath, expectedOutputPath, expectedErrorCode,
                baseIri, processorMode, expandContextPath, compactArrays, rdfDirection);
        }


        [Theory]
        [MemberData(nameof(JsonLdTestSuiteDataSource.W3CFlattenTests), MemberType = typeof(JsonLdTestSuiteDataSource))]
        public override void FlattenTests(string testId, JsonLdTestType testType, string inputPath, string contextPath,
            string expectedOutputPath,
            JsonLdErrorCode expectedErrorCode, string baseIri, string processorMode, string expandContextPath,
            bool compactArrays, string rdfDirection)
        {
            base.FlattenTests(testId, testType, inputPath, contextPath, expectedOutputPath, expectedErrorCode,
                baseIri, processorMode, expandContextPath, compactArrays, rdfDirection);
        }

        private readonly Dictionary<string, string> _skippedParserTests = new Dictionary<string, string>
        {
            {"#t0120", "Test fails due to .NET URI parsing library"},
            {"#t0121", "Test fails due to .NET URI parsing library"},
            {"#t0122", "Test fails due to .NET URI parsing library"},
            {"#t0123", "Test fails due to .NET URI parsing library"},
            {"#t0124", "Test fails due to .NET URI parsing library"},
            {"#t0125", "Test fails due to .NET URI parsing library"},
            {"#t0126", "Test fails due to .NET URI parsing library"},
            {"#te075", "Test uses a blank-node property"},
            {"#tjs12", "Test depends on decimal representation of a float"},
        };

        [SkippableTheory(typeof(SkipException))]
        //[Theory]
        [MemberData(nameof(JsonLdTestSuiteDataSource.W3CToRdfTests), MemberType = typeof(JsonLdTestSuiteDataSource))]
        public override void JsonLdParserTests(string testId, JsonLdTestType testType, string inputPath, string contextPath,
            string expectedOutputPath, JsonLdErrorCode expectedErrorCode, string baseIri, string processorMode,
            string expandContextPath, bool compactArrays, string rdfDirection)
        {
            if (_skippedParserTests.ContainsKey(testId)) throw new SkipException(_skippedParserTests[testId]);
            base.JsonLdParserTests(testId, testType, inputPath, contextPath, expectedOutputPath,
                expectedErrorCode, baseIri, processorMode, expandContextPath, compactArrays, rdfDirection);
        }

        [Theory]
        [MemberData(nameof(JsonLdTestSuiteDataSource.W3CFromRdfTests), MemberType = typeof(JsonLdTestSuiteDataSource))]
        public override void JsonLdWriterTests(string testId, JsonLdTestType testType, string inputPath, string contextPath,
            string expectedOutputPath, JsonLdErrorCode expectErrorCode, bool useNativeTypes, bool useRdfType, bool ordered, string rdfDirection)
        {
            base.JsonLdWriterTests(testId, testType, inputPath, contextPath, expectedOutputPath, expectErrorCode, useNativeTypes, useRdfType, ordered, rdfDirection);
        }

        [Theory]
        [MemberData(nameof(JsonLdTestSuiteDataSource.W3CFrameTests), MemberType = typeof(JsonLdTestSuiteDataSource))]
        public override void JsonLdFramingTests(string testId, JsonLdTestType testType, string inputPath, string framePath,
            string expectedOutputPath, JsonLdErrorCode expectErrorCode, string processingMode, 
            bool pruneBlankNodeIdentifiers, bool? omitGraph, bool ordered)
        {
            base.JsonLdFramingTests(testId, testType, inputPath, framePath, expectedOutputPath, expectErrorCode, processingMode, 
                pruneBlankNodeIdentifiers, omitGraph, ordered);
        }
    }
}
