using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace VDS.RDF.JsonLd;

public static class JsonLdTestSuiteDataSource
{
    public static IEnumerable<object[]> ExpandTests => ProcessManifest(Path.Combine("resources","jsonld"), "expand-manifest.jsonld");

    public static IEnumerable<object[]> CompactTests => ProcessManifest(Path.Combine("resources", "jsonld"), "compact-manifest.jsonld");

    public static IEnumerable<object[]> FlattenTests => ProcessManifest(Path.Combine("resources", "jsonld"), "flatten-manifest.jsonld");

    public static IEnumerable<object[]> ToRdfTests => ProcessManifest(Path.Combine("resources", "jsonld"), "toRdf-manifest.jsonld", "toRdf-skip.txt");

    public static IEnumerable<object[]> FromRdfTests => ProcessFromRdfManifest(Path.Combine("resources", "jsonld"), "fromRdf-manifest.jsonld");

    public static IEnumerable<object[]> FrameTests => ProcessFrameManifest(Path.Combine("resources", "jsonld"), "frame-manifest.jsonld");

    public static IEnumerable<object[]> W3CExpandTests =>
        ProcessManifest(Path.Combine("resources", "jsonld11"), "expand-manifest.jsonld");

    public static IEnumerable<object[]> W3CCompactTests =>
        ProcessManifest(Path.Combine("resources", "jsonld11"), "compact-manifest.jsonld");

    public static IEnumerable<object[]> W3CFlattenTests =>
        ProcessManifest(Path.Combine("resources", "jsonld11"), "flatten-manifest.jsonld");

    public static IEnumerable<object[]> W3CToRdfTests =>
        ProcessManifest(Path.Combine("resources", "jsonld11"), "toRdf-manifest.jsonld");

    public static IEnumerable<object[]> W3CFromRdfTests =>
        ProcessFromRdfManifest(Path.Combine("resources", "jsonld11"), "fromRdf-manifest.jsonld");

    public static IEnumerable<object[]> W3CFrameTests =>
        ProcessFrameManifest(Path.Combine("resources", "jsonld-framing11"), "frame-manifest.jsonld");

    public static IEnumerable<object[]> DnrFrameTests =>
        ProcessFrameManifest(Path.Combine("resources", "dnr-jsonld-framing11"), "frame-manifest.jsonld");

    private static IEnumerable<object[]> ProcessManifest(string resourceDirPath, string manifestPath, string skipTestsPath = null)
    {
        var resourceDir = new DirectoryInfo(resourceDirPath);
        manifestPath = Path.Combine(resourceDir.FullName, manifestPath);
        var manifestJson = File.ReadAllText(manifestPath);
        var manifest = JObject.Parse(manifestJson);
        var skipTests = new List<string>();
        if (skipTestsPath != null)
        {
            skipTestsPath = Path.Combine(resourceDir.FullName, skipTestsPath);
            skipTests = File.ReadAllLines(skipTestsPath).ToList();
        }

        var testSuiteBaseIri = manifest.Property("baseIri").Value.Value<string>();
        var sequence = manifest.Property("sequence").Value as JArray;
        foreach (var testConfiguration in sequence.OfType<JObject>())
        {
            var testId = testConfiguration.Property("@id").Value.Value<string>();
            var testType = GetTestType(testConfiguration);
            // For now ignore type as everything in this manifest is a positive test
            var input = testConfiguration.Property("input").Value.Value<string>();
            var defaultBaseIri = testSuiteBaseIri + input;
            if (skipTests.Contains(input)) continue;
            var context = testConfiguration.Property("context")?.Value.Value<string>();

            var expect = testType == JsonLdTestType.PositiveEvaluationTest
                ? testConfiguration.Property("expect").Value.Value<string>()
                : null;
            var expectErrorCodeString = testType == JsonLdTestType.NegativeEvaluationTest
                ? testConfiguration.Property("expectErrorCode").Value.Value<string>()
                : null;
            var expectErrorCode = GetErrorCode(expectErrorCodeString);
            var optionsProperty = testConfiguration.Property("option");
            string baseIri = defaultBaseIri,
                processorMode = null,
                expandContext = null,
                specVersion = null,
                rdfDirection = null;
            var compactArrays = true;
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
                        case "rdfDirection":
                            rdfDirection = p.Value.Value<string>();
                            break;
                        case "specVersion":
                            specVersion = p.Value.Value<string>();
                            break;
                    }
                }
            }
            if ("json-ld-1.0".Equals(specVersion)) continue; // Tests that are specifically against the 1.0 version of the spec will not work with a 1.1 processor
            yield return new object[] {
                testId,
                testType,
                Path.Combine(resourceDir.FullName, input),
                context == null ? null : Path.Combine(resourceDir.FullName, context),
                expect == null ? null : Path.Combine(resourceDir.FullName, expect),
                expectErrorCode,
                baseIri,
                processorMode,
                expandContext,
                compactArrays,
                rdfDirection
            };
        }
    }

    private static JsonLdTestType GetTestType(JObject testConfiguration)
    {
        var types = testConfiguration.Property("@type")?.Value.Values<string>().ToArray();
        if (types == null) throw new ArgumentException("No @types property on test configuration");
        if (types.Contains("jld:PositiveEvaluationTest")) return JsonLdTestType.PositiveEvaluationTest;
        if (types.Contains("jld:NegativeEvaluationTest")) return JsonLdTestType.NegativeEvaluationTest;
        if (types.Contains("jld:PositiveSyntaxTest")) return JsonLdTestType.PositiveSyntaxTest;
        if (types.Contains("jld:NegativeSyntaxTest")) return JsonLdTestType.NegativeSyntaxTest;
        throw new ArgumentException($"Unable to determine test type from @type={types}");
    }

    private static JsonLdErrorCode? GetErrorCode(string errorCodeString)
    {
        if (string.IsNullOrEmpty(errorCodeString)) return null;
        var errorCodeName = string.Join("", errorCodeString.Split(new [] {' ', '@', '-'}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Capitalize()));
        if (Enum.IsDefined(typeof(JsonLdErrorCode), errorCodeName))
        {
            return (JsonLdErrorCode)Enum.Parse(typeof(JsonLdErrorCode), errorCodeName);
        }

        throw new ArgumentException($"Unable to process string '{errorCodeString}' as a JsonLdErrorCode enum value",
            nameof(errorCodeString));
    }

    public static string Capitalize(this string s)
    {
        return s.Substring(0, 1).ToUpper() + s.Substring(1).ToLower();
    }

    private static IEnumerable<object[]> ProcessFromRdfManifest(string resourceDirPath, string manifestPath, string skipTestsPath = null)
    {
        var resourceDir = new DirectoryInfo(resourceDirPath);
        manifestPath = Path.Combine(resourceDir.FullName, manifestPath);
        var manifestJson = File.ReadAllText(manifestPath);
        var manifest = JObject.Parse(manifestJson);
        var skipTests = ReadSkipTests(skipTestsPath, resourceDir);
        var sequence = manifest.Property("sequence").Value as JArray;
        foreach (var testConfiguration in sequence.OfType<JObject>())
        {
            var testId = testConfiguration.Property("@id").Value.Value<string>();
            var testType = GetTestType(testConfiguration);
            var input = testConfiguration.Property("input").Value.Value<string>();
            if (skipTests.Contains(input)) continue;
            var context = testConfiguration.Property("context")?.Value.Value<string>();
            var expect = testType == JsonLdTestType.PositiveEvaluationTest
                ? testConfiguration.Property("expect").Value.Value<string>()
                : null;
            var expectErrorCode = testType == JsonLdTestType.NegativeEvaluationTest
                ? GetErrorCode(testConfiguration.Property("expectErrorCode").Value.Value<string>())
                : null;
            var optionsProperty = testConfiguration.Property("option");
            bool useNativeTypes = false, useRdfType = false, ordered=false;
            string specVersion = null, rdfDirection = null;
            var options = optionsProperty?.Value as JObject;
            if (options != null)
            {
                foreach (var p in options.Properties())
                {
                    switch (p.Name)
                    {
                        case "useNativeTypes":
                            useNativeTypes = p.Value.Value<bool>();
                            break;
                        case "useRdfType":
                            useRdfType = p.Value.Value<bool>();
                            break;
                        case "rdfDirection":
                            rdfDirection = p.Value.Value<string>();
                            break;
                        case "specVersion":
                            specVersion = p.Value.Value<string>();
                            break;
                        case "ordered":
                            ordered = p.Value.Value<bool>();
                            break;
                    }
                }
            }
            if ("json-ld-1.0".Equals(specVersion)) continue; // Tests that are specifically against the 1.0 version of the spec will not work with a 1.1 processor
            yield return new object[] {
                testId,
                testType,
                Path.Combine(resourceDir.FullName, input),
                context == null ? null : Path.Combine(resourceDir.FullName, context),
                expect == null ? null : Path.Combine(resourceDir.FullName, expect),
                expectErrorCode,
                useNativeTypes,
                useRdfType,
                ordered,
                rdfDirection
            };
        }
    }

    private static List<string> ReadSkipTests(string skipTestsPath, DirectoryInfo resourceDir)
    {
        var skipTests = new List<string>();
        if (skipTestsPath != null)
        {
            skipTestsPath = Path.Combine(resourceDir.FullName, skipTestsPath);
            skipTests = File.ReadAllLines(skipTestsPath).ToList();
        }
        return skipTests;
    }

    private static IEnumerable<object[]> ProcessFrameManifest(string resourceDirPath, string manifestPath, string skipTestsPath = null)
    {
        var resourceDir = new DirectoryInfo(resourceDirPath);
        var skipTests = ReadSkipTests(skipTestsPath, resourceDir);
        manifestPath = Path.Combine(resourceDir.FullName, manifestPath);
        var manifestJson = File.ReadAllText(manifestPath);
        var manifest = JObject.Parse(manifestJson);
        var sequence = manifest.Property("sequence").Value as JArray;
        foreach (var testConfiguration in sequence.OfType<JObject>())
        {
            var testId = testConfiguration.Property("@id").Value.Value<string>();
            var testType = GetTestType(testConfiguration);
            var input = testConfiguration.Property("input").Value.Value<string>();
            if (skipTests.Contains(input)) continue;
            var frame = testConfiguration.Property("frame")?.Value.Value<string>();
            var expect = testType == JsonLdTestType.PositiveEvaluationTest
                ? testConfiguration.Property("expect").Value.Value<string>()
                : null;
            var expectErrorCode = testType == JsonLdTestType.NegativeEvaluationTest
                ? GetErrorCode(testConfiguration.Property("expectErrorCode").Value.Value<string>())
                : null;
            var optionsProperty = testConfiguration.Property("option");
            bool pruneBlankNodeIdentifiers = false, ordered = false;
            bool? omitGraph = null;
            string processingMode = null, specVersion = null;
            var options = optionsProperty?.Value as JObject;
            if (options != null)
            {
                foreach (var p in options.Properties())
                {
                    switch (p.Name)
                    {
                        case "processingMode":
                            processingMode = p.Value.Value<string>();
                            break;
                        case "specVersion":
                            specVersion = p.Value.Value<string>();
                            break;
                        case "pruneBlankNodeIdentifiers":
                            pruneBlankNodeIdentifiers = p.Value.Value<bool>();
                            break;
                        case "omitGraph":
                            omitGraph = p.Value.Value<bool>();
                            break;
                        case "ordered":
                            ordered = p.Value.Value<bool>();
                            break;
                    }
                }
            }
            if ("json-ld-1.0".Equals(specVersion)) continue; // Tests that are specifically against the 1.0 version of the spec will not work with a 1.1 processor
            yield return new object[] {
                testId,
                testType,
                Path.Combine(resourceDir.FullName, input),
                Path.Combine(resourceDir.FullName, frame),
                expect == null ? null : Path.Combine(resourceDir.FullName, expect),
                expectErrorCode,
                processingMode,
                pruneBlankNodeIdentifiers,
                omitGraph,
                ordered
            };
        }
    }
}