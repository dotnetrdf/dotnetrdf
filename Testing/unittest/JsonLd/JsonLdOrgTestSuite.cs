using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Xunit;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace VDS.RDF.JsonLd
{
    public class JsonLdOrgTestSuite
    {
        [Theory]
        [MemberData("ExpandTests", MemberType = typeof(JsonLdTestSuiteDataSource))]
        public void ExpandTests(string inputPath, string expectedOutputPath, string baseIri, string processorMode, string expandContextPath)
        {
            var processorOptions = new JsonLdProcessorOptions();
            if (baseIri != null)
            {
                processorOptions.Base = new Uri(baseIri);
            }
            else
            {
                processorOptions.Base = new Uri("http://json-ld.org/test-suite/tests/" + Path.GetFileName(inputPath));
            }
            if (processorMode != null) processorOptions.Syntax = processorMode.Equals("json-ld-1.1") ? JsonLdSyntax.JsonLd11 : JsonLdSyntax.JsonLd10;
            var processor = new JsonLdProcessor(processorOptions);
            var inputJson = File.ReadAllText(inputPath);
            var expectedOutputJson = File.ReadAllText(expectedOutputPath);
            var inputElement = JToken.Parse(inputJson);
            var expectedOutputElement = JToken.Parse(expectedOutputJson);
            if (expandContextPath != null)
            {
                var expandContextJson = File.ReadAllText(expandContextPath);
                processorOptions.ExpandContext = JObject.Parse(expandContextJson);
            }
            
            var actualOutputElement = processor.Expand(inputElement, processorOptions);
            Assert.True(JToken.DeepEquals(actualOutputElement, expectedOutputElement),
                String.Format(
                "Error processing expand test {0}.\nActual output does not match expected output.\nExpected:\n{1}\n\nActual:\n{2}",
                Path.GetFileName(inputPath),
                expectedOutputElement,
                actualOutputElement));
        }
    }


    public static class JsonLdTestSuiteDataSource
    {
        public static IEnumerable<object[]> ExpandTests
        {
            get
            {
                var resourceDir = new DirectoryInfo("resources\\jsonld");
                var manifestPath = Path.Combine(resourceDir.FullName, "expand-manifest.jsonld");
                var manifestJson = File.ReadAllText(manifestPath);
                var manifest = JObject.Parse(manifestJson);
                var sequence = manifest.Property("sequence").Value as JArray;
                foreach(var testConfiguration in sequence.OfType<JObject>())
                {
                    // For now ignore type as everything in this manifest is a positive test
                    var input = testConfiguration.Property("input").Value.Value<string>();
                    var expect = testConfiguration.Property("expect").Value.Value<string>();
                    var optionsProperty = testConfiguration.Property("option");
                    string baseIri = null, 
                        processorMode = null, 
                        expandContext = null;
                    if (optionsProperty != null)
                    {
                        var options = optionsProperty.Value as JObject;
                        if (options != null)
                        {
                            foreach (var p in options.Properties())
                            {
                                switch (p.Name)
                                {
                                    case "base":
                                        baseIri = p.Value.Value<string>();
                                        break;
                                    case "processingMode":
                                        processorMode = p.Value.Value<string>();
                                        break;
                                    case "expandContext":
                                        expandContext = Path.Combine(resourceDir.FullName, p.Value.Value<string>());
                                        break;
                                }
                            }
                        }
                    }
                    yield return new object[] {
                        Path.Combine(resourceDir.FullName, input),
                        Path.Combine(resourceDir.FullName, expect),
                        baseIri,
                        processorMode,
                        expandContext
                    };
                }
            }
        }
    }
}
