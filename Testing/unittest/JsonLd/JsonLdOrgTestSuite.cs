using System;
using System.Collections.Generic;
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
        public void ExpandTests(string inputPath, string contextPath, string expectedOutputPath, string baseIri, string processorMode, string expandContextPath, bool compactArrays)
        {
            var processorOptions = MakeProcessorOptions(inputPath, baseIri, processorMode, expandContextPath,
                compactArrays);
            var processor = new JsonLdProcessor(processorOptions);
            var inputJson = File.ReadAllText(inputPath);
            var expectedOutputJson = File.ReadAllText(expectedOutputPath);
            var inputElement = JToken.Parse(inputJson);
            var expectedOutputElement = JToken.Parse(expectedOutputJson);

            // Expand tests should not have a context parameter
            Assert.Null(contextPath);
            
            var actualOutputElement = processor.Expand(inputElement, processorOptions);
            Assert.True(JToken.DeepEquals(actualOutputElement, expectedOutputElement),
                $"Error processing expand test {Path.GetFileName(inputPath)}.\nActual output does not match expected output.\nExpected:\n{expectedOutputElement}\n\nActual:\n{actualOutputElement}");
        }

        [Theory]
        [MemberData("CompactTests", MemberType = typeof(JsonLdTestSuiteDataSource))]
        public void CompactTests(string inputPath, string contextPath, string expectedOutputPath, string baseIri, string processorMode, string expandContextPath, bool compactArrays)
        {
            var processorOptions = MakeProcessorOptions(inputPath, baseIri, processorMode, expandContextPath,
                compactArrays);
            var processor = new JsonLdProcessor(processorOptions);
            var inputJson = File.ReadAllText(inputPath);
            var contextJson = contextPath == null ? null : File.ReadAllText(contextPath);
            var expectedOutputJson = File.ReadAllText(expectedOutputPath);
            var inputElement = JToken.Parse(inputJson);
            var contextElement = contextJson == null ? new JObject() : JToken.Parse(contextJson);
            var expectedOutputElement = JToken.Parse(expectedOutputJson);

            var actualOutputElement = processor.Compact(inputElement, contextElement, processorOptions);
            Assert.True(JToken.DeepEquals(actualOutputElement, expectedOutputElement),
                $"Error processing compact test {Path.GetFileName(inputPath)}.\nActual output does not match expected output.\nExpected:\n{expectedOutputElement}\n\nActual:\n{actualOutputElement}");
        }

        [Theory]
        [MemberData("FlattenTests", MemberType = typeof(JsonLdTestSuiteDataSource))]
        public void FlattenTests(string inputPath, string contextPath, string expectedOutputPath, string baseIri, string processorMode, string expandContextPath, bool compactArrays)
        {
            var processorOptions = MakeProcessorOptions(inputPath, baseIri, processorMode, expandContextPath,
                compactArrays);
            var processor = new JsonLdProcessor(processorOptions);
            var inputJson = File.ReadAllText(inputPath);
            var contextJson = contextPath == null ? null : File.ReadAllText(contextPath);
            var expectedOutputJson = File.ReadAllText(expectedOutputPath);
            var inputElement = JToken.Parse(inputJson);
            var contextElement = contextJson == null ? null : JToken.Parse(contextJson);
            var expectedOutputElement = JToken.Parse(expectedOutputJson);

            var actualOutputElement = processor.Flatten(inputElement, contextElement, processorOptions);
            Assert.True(JToken.DeepEquals(actualOutputElement, expectedOutputElement),
                $"Error processing flatten test {Path.GetFileName(inputPath)}.\nActual output does not match expected output.\nExpected:\n{expectedOutputElement}\n\nActual:\n{actualOutputElement}");
        }


        private static JsonLdProcessorOptions MakeProcessorOptions(string inputPath, string baseIri, string processorMode,
            string expandContextPath, bool compactArrays)
        {
            var processorOptions = new JsonLdProcessorOptions
            {
                Base = baseIri != null
                    ? new Uri(baseIri)
                    : new Uri("http://json-ld.org/test-suite/tests/" + Path.GetFileName(inputPath))
            };
            if (processorMode != null) processorOptions.Syntax = processorMode.Equals("json-ld-1.1") ? JsonLdSyntax.JsonLd11 : JsonLdSyntax.JsonLd10;
            if (expandContextPath != null)
            {
                var expandContextJson = File.ReadAllText(expandContextPath);
                processorOptions.ExpandContext = JObject.Parse(expandContextJson);
            }
            processorOptions.CompactArrays = compactArrays;
            return processorOptions;
        }
    }


    public static class JsonLdTestSuiteDataSource
    {
        public static IEnumerable<object[]> ExpandTests => ProcessManifest("expand-manifest.jsonld");

        public static IEnumerable<object[]> CompactTests => ProcessManifest("compact-manifest.jsonld");

        public static IEnumerable<object[]> FlattenTests => ProcessManifest("flatten-manifest.jsonld");

        private static IEnumerable<object[]> ProcessManifest(string manifestPath)
        {
            var resourceDir = new DirectoryInfo("resources\\jsonld");
            manifestPath = Path.Combine(resourceDir.FullName, manifestPath);
            var manifestJson = File.ReadAllText(manifestPath);
            var manifest = JObject.Parse(manifestJson);
            var sequence = manifest.Property("sequence").Value as JArray;
            foreach (var testConfiguration in sequence.OfType<JObject>())
            {
                // For now ignore type as everything in this manifest is a positive test
                var input = testConfiguration.Property("input").Value.Value<string>();
                var context = testConfiguration.Property("context")?.Value.Value<string>();
                var expect = testConfiguration.Property("expect").Value.Value<string>();
                var optionsProperty = testConfiguration.Property("option");
                string baseIri = null,
                    processorMode = null,
                    expandContext = null;
                bool compactArrays = true;
                var options = optionsProperty?.Value as JObject;
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
                            case "compactArrays":
                                compactArrays = p.Value.Value<bool>();
                                break;
                        }
                    }
                }
                yield return new object[] {
                    Path.Combine(resourceDir.FullName, input),
                    context == null ? null:Path.Combine(resourceDir.FullName, context),
                    Path.Combine(resourceDir.FullName, expect),
                    baseIri,
                    processorMode,
                    expandContext,
                    compactArrays
                };
            }
        }
    }
}
