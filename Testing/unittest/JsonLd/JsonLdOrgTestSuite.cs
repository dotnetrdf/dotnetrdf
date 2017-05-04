using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Xunit;
using Newtonsoft.Json.Linq;

namespace VDS.RDF.JsonLd
{
    public class JsonLdOrgTestSuite
    {
        [Theory]
        [MemberData("ExpandTests", MemberType =typeof(JsonLdTestSuiteDataSource))]
        public void ExpandTests(string inputPath, string expectedOutputPath)
        {
            var processor = new JsonLdProcessor(new JsonLdProcessorOptions());
            processor.BaseIri = new Uri(inputPath);
            var inputJson = File.ReadAllText(inputPath);
            var expectedOutputJson = File.ReadAllText(expectedOutputPath);
            var inputElement = JToken.Parse(inputJson);
            var expectedOutputElement = JToken.Parse(expectedOutputJson);
            var actualOutputElement = processor.Expand(new JsonLdContext(), null, inputElement);
            Assert.True(JToken.DeepEquals(actualOutputElement, expectedOutputElement),
                String.Format(
                "Error processing expand test {0}.\nActual output does not match expected output.\nExpected:\n{1}\n\nActual:\n{2}",
                Path.GetFileName(inputPath),
                expectedOutputElement.ToString(),
                actualOutputElement.ToString()));
        }
    }

    public static class JsonLdTestSuiteDataSource
    {
        public static IEnumerable<object[]> ExpandTests
        {
            get
            {
                var resourceDir = new DirectoryInfo("resources\\jsonld");
                foreach(var inputFile in resourceDir.EnumerateFiles("expand-????-in.jsonld"))
                {
                    var inputPath = inputFile.FullName;
                    var outputPath = inputPath.Replace("-in.jsonld", "-out.jsonld");
                    yield return new object[] { inputPath, outputPath };
                }
            }
        }
    }
}
