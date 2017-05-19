using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Newtonsoft.Json.Linq;
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.JsonLd
{
    public class JsonLdOrgTestSuite
    {
        [Theory]
        [MemberData("ExpandTests", MemberType = typeof(JsonLdTestSuiteDataSource))]
        public void ExpandTests(string inputPath, string contextPath, string expectedOutputPath, string baseIri,
            string processorMode, string expandContextPath, bool compactArrays)
        {
            var processorOptions = MakeProcessorOptions(inputPath, baseIri, processorMode, expandContextPath,
                compactArrays);
            var inputJson = File.ReadAllText(inputPath);
            var expectedOutputJson = File.ReadAllText(expectedOutputPath);
            var inputElement = JToken.Parse(inputJson);
            var expectedOutputElement = JToken.Parse(expectedOutputJson);

            // Expand tests should not have a context parameter
            Assert.Null(contextPath);

            var actualOutputElement = JsonLdProcessor.Expand(inputElement, processorOptions);
            Assert.True(JToken.DeepEquals(actualOutputElement, expectedOutputElement),
                $"Error processing expand test {Path.GetFileName(inputPath)}.\nActual output does not match expected output.\nExpected:\n{expectedOutputElement}\n\nActual:\n{actualOutputElement}");
        }

        [Theory]
        [MemberData("CompactTests", MemberType = typeof(JsonLdTestSuiteDataSource))]
        public void CompactTests(string inputPath, string contextPath, string expectedOutputPath, string baseIri,
            string processorMode, string expandContextPath, bool compactArrays)
        {
            var processorOptions = MakeProcessorOptions(inputPath, baseIri, processorMode, expandContextPath,
                compactArrays);
            var inputJson = File.ReadAllText(inputPath);
            var contextJson = contextPath == null ? null : File.ReadAllText(contextPath);
            var expectedOutputJson = File.ReadAllText(expectedOutputPath);
            var inputElement = JToken.Parse(inputJson);
            var contextElement = contextJson == null ? new JObject() : JToken.Parse(contextJson);
            var expectedOutputElement = JToken.Parse(expectedOutputJson);

            var actualOutputElement = JsonLdProcessor.Compact(inputElement, contextElement, processorOptions);
            Assert.True(JToken.DeepEquals(actualOutputElement, expectedOutputElement),
                $"Error processing compact test {Path.GetFileName(inputPath)}.\nActual output does not match expected output.\nExpected:\n{expectedOutputElement}\n\nActual:\n{actualOutputElement}");
        }

        [Theory]
        [MemberData("FlattenTests", MemberType = typeof(JsonLdTestSuiteDataSource))]
        public void FlattenTests(string inputPath, string contextPath, string expectedOutputPath, string baseIri,
            string processorMode, string expandContextPath, bool compactArrays)
        {
            var processorOptions = MakeProcessorOptions(inputPath, baseIri, processorMode, expandContextPath,
                compactArrays);
            var inputJson = File.ReadAllText(inputPath);
            var contextJson = contextPath == null ? null : File.ReadAllText(contextPath);
            var expectedOutputJson = File.ReadAllText(expectedOutputPath);
            var inputElement = JToken.Parse(inputJson);
            var contextElement = contextJson == null ? null : JToken.Parse(contextJson);
            var expectedOutputElement = JToken.Parse(expectedOutputJson);

            var actualOutputElement = JsonLdProcessor.Flatten(inputElement, contextElement, processorOptions);
            Assert.True(JToken.DeepEquals(actualOutputElement, expectedOutputElement),
                $"Error processing flatten test {Path.GetFileName(inputPath)}.\nActual output does not match expected output.\nExpected:\n{expectedOutputElement}\n\nActual:\n{actualOutputElement}");
        }

        [Theory]
        [MemberData("ToRdfTests", MemberType = typeof(JsonLdTestSuiteDataSource))]
        public void JsonLdParserTests(string inputPath, string contextPath, string expectedOutputPath, string baseIri,
            string processorMode, string expandContextPath, bool compactArrays)
        {
            var processorOptions = MakeProcessorOptions(inputPath, baseIri, processorMode, expandContextPath,
                compactArrays);
            var contextJson = contextPath == null ? null : File.ReadAllText(contextPath);
            var contextElement = contextJson == null ? null : JToken.Parse(contextJson);
            var nqParser = new NQuadsParser(NQuadsSyntax.Rdf11);
            var expectedStore = new TripleStore();
            nqParser.Load(expectedStore, expectedOutputPath);
            FixStringLiterals(expectedStore);
            var jsonldParser = new JsonLdParser(processorOptions);
            var actualStore = new TripleStore();
            jsonldParser.Load(actualStore, inputPath);
            Assert.True(expectedStore.Graphs.Count.Equals(actualStore.Graphs.Count),
                $"Test failed for input {Path.GetFileName(inputPath)}.\r\nActual graph count {actualStore.Graphs.Count} does not match expected graph count {expectedStore.Graphs.Count}.");
            foreach (var expectGraph in expectedStore.Graphs)
            {
                Assert.True(actualStore.HasGraph(expectGraph.BaseUri),
                    $"Test failed for input {Path.GetFileName(inputPath)}.\r\nCould not find expected graph {expectGraph.BaseUri}");
                var actualGraph = actualStore.Graphs[expectGraph.BaseUri];
                var bNodeMapping = new Dictionary<INode, INode>();
                var graphsEqual = actualGraph.Equals(expectGraph, out bNodeMapping);
                if (!graphsEqual)
                {
                    var ser = new NQuadsWriter();
                    string expectedLines = MakeNQuadsList(expectedStore);
                    string actualLines = MakeNQuadsList(actualStore);
                    Assert.True(graphsEqual,
                        $"Test failed for input {Path.GetFileName(inputPath)}.\r\nGraph {expectGraph.BaseUri} differs in actual output from expected output.\r\nExpected:\r\n{expectedLines}\r\nActual:\r\n{actualLines}");
                }
            }
        }

        [Theory]
        [MemberData("FromRdfTests", MemberType = typeof(JsonLdTestSuiteDataSource))]
        public void JsonLdWriterTests(string inputPath, string contextPath, string expectedOutputPath, string baseIri,
            string processorMode, string expandContextPath, bool compactArrays)
        {
            var nqParser = new NQuadsParser(NQuadsSyntax.Rdf11);
            var input = new TripleStore();
            nqParser.Load(input, inputPath);
            FixStringLiterals(input);
            var expectedOutputJson = File.ReadAllText(expectedOutputPath);
            var expectedOutput = JToken.Parse(expectedOutputJson);
            var jsonLdWriter = new JsonLdWriter();
            var actualOutput = jsonLdWriter.SerializeStore(input);
            Assert.True(DeepEquals(expectedOutput, actualOutput, true),
                $"Test failed for input {Path.GetFileName(inputPath)}\nExpected:\n{expectedOutput}\nActual:\n{actualOutput}");
        }

        private static bool DeepEquals(JToken token1, JToken token2, bool ignoreArrayOrder)
        {
            if (token1.Type != token2.Type) return false;
            switch (token1.Type)
            {
                case JTokenType.Object:
                    var o1 = token1 as JObject;
                    var o2 = token2 as JObject;
                    foreach (var p in o1.Properties())
                    {
                        if (o2[p.Name] == null) return false;
                        if (!DeepEquals(o1[p.Name], o2[p.Name], ignoreArrayOrder)) return false;
                    }
                    if (o2.Properties().Any(p2 => o1.Property(p2.Name) == null)) return false;
                    return true;
                case JTokenType.Array:
                    var a1 = token1 as JArray;
                    var a2 = token2 as JArray;
                    if (a1.Count != a2.Count) return false;
                    if (!ignoreArrayOrder)
                    {
                        for (int i = 0; i < a1.Count; i++)
                        {
                            if (!DeepEquals(a1[i], a2[i], ignoreArrayOrder)) return false;
                        }
                        return true;
                    }
                    var unmatchedItems = (token2 as JArray).ToList();
                    foreach (var item1 in a1)
                    {
                        if (unmatchedItems.Count == 0) return false;
                        var match = unmatchedItems.FindIndex(x => DeepEquals(item1, x, ignoreArrayOrder));
                        if (match >= 0)
                        {
                            unmatchedItems.RemoveAt(match);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return unmatchedItems.Count == 0;
                default:
                    return JToken.DeepEquals(token1, token2);
            }
        }


        private static void FixStringLiterals(TripleStore store)
        {
            var xsdString = new Uri("http://www.w3.org/2001/XMLSchema#string");
            foreach (var t in store.Triples.ToList())
            {
                var literalNode = t.Object as ILiteralNode;
                if (literalNode != null && String.IsNullOrEmpty(literalNode.Language) && literalNode.DataType == null)
                {
                    var graphToUpdate = t.Graph;
                    graphToUpdate.Retract(t);
                    graphToUpdate.Assert(
                        new Triple(t.Subject, t.Predicate,
                            graphToUpdate.CreateLiteralNode(literalNode.Value, xsdString),
                            graphToUpdate.BaseUri));
                }
            }
        }

        private static string MakeNQuadsList(TripleStore store)
        {
            var ser = new NQuadsWriter();
            using (var expectedTextWriter = new System.IO.StringWriter())
            {
                ser.Save(store, expectedTextWriter);
                var lines = expectedTextWriter.ToString().Split('\n').Select(x => x.Trim()).ToList();
                lines.Sort();
                return String.Join(Environment.NewLine, lines);
            }
        }

        private static JsonLdProcessorOptions MakeProcessorOptions(string inputPath, string baseIri,
            string processorMode,
            string expandContextPath, bool compactArrays)
        {
            var processorOptions = new JsonLdProcessorOptions
            {
                Base = baseIri != null
                    ? new Uri(baseIri)
                    : new Uri("http://json-ld.org/test-suite/tests/" + Path.GetFileName(inputPath))
            };
            if (processorMode != null)
                processorOptions.Syntax = processorMode.Equals("json-ld-1.1")
                    ? JsonLdSyntax.JsonLd11
                    : JsonLdSyntax.JsonLd10;
            if (expandContextPath != null)
            {
                var expandContextJson = File.ReadAllText(expandContextPath);
                processorOptions.ExpandContext = JObject.Parse(expandContextJson);
            }
            processorOptions.CompactArrays = compactArrays;
            return processorOptions;
        }

        [Fact]
        public void TestDataSource()
        {
            Assert.Equal(111, JsonLdTestSuiteDataSource.ToRdfTests.Count());
        }
    }


    public static class JsonLdTestSuiteDataSource
    {
        public static IEnumerable<object[]> ExpandTests => ProcessManifest("expand-manifest.jsonld");

        public static IEnumerable<object[]> CompactTests => ProcessManifest("compact-manifest.jsonld");

        public static IEnumerable<object[]> FlattenTests => ProcessManifest("flatten-manifest.jsonld");

        public static IEnumerable<object[]> ToRdfTests => ProcessManifest("toRdf-manifest.jsonld", "toRdf-skip.txt");

        public static IEnumerable<object[]> FromRdfTests => ProcessManifest("fromRdf-manifest.jsonld");

        private static IEnumerable<object[]> ProcessManifest(string manifestPath, string skipTestsPath = null)
        {
            var resourceDir = new DirectoryInfo("resources\\jsonld");
            manifestPath = Path.Combine(resourceDir.FullName, manifestPath);
            var manifestJson = File.ReadAllText(manifestPath);
            var manifest = JObject.Parse(manifestJson);
            var skipTests = new List<string>();
            if (skipTestsPath != null)
            {
                skipTestsPath = Path.Combine(resourceDir.FullName, skipTestsPath);
                skipTests = File.ReadAllLines(skipTestsPath).ToList();
            }
            var sequence = manifest.Property("sequence").Value as JArray;
            foreach (var testConfiguration in sequence.OfType<JObject>())
            {
                // For now ignore type as everything in this manifest is a positive test
                var input = testConfiguration.Property("input").Value.Value<string>();
                if (skipTests.Contains(input)) continue;
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
