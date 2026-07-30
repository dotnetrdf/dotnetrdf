using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using VDS.RDF.JsonLd.Syntax;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using Xunit;

namespace VDS.RDF.JsonLd;

public class JsonLdTestSuiteBase
{
    private readonly ITestOutputHelper _output;

    public JsonLdTestSuiteBase(ITestOutputHelper output)
    {
        _output = output;
        // Ensure that we are using modern TLS2 for HTTPS connections (required to access the GitHub-hosted context files)
        //ServicePointManager.Expect100Continue = true;
        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
    }

    public virtual void ExpandTests(string testId, JsonLdTestType testType, string inputPath, string contextPath,
        string expectedOutputPath, JsonLdErrorCode expectErrorCode, string baseIri,
        string processorMode, string expandContextPath, bool compactArrays, string rdfDirection)
    {
        JsonLdProcessorOptions processorOptions = MakeProcessorOptions(inputPath, baseIri, processorMode, expandContextPath,
            compactArrays, rdfDirection);
        var inputJson = File.ReadAllText(inputPath);
        var inputElement = JsonNode.Parse(inputJson);

        // Expand tests should not have a context parameter
        Assert.Null(contextPath);

        switch (testType)
        {
            case JsonLdTestType.PositiveEvaluationTest:
                JsonArray actualOutputElement = JsonLdProcessor.Expand(inputElement, processorOptions);
                var expectedOutputJson = File.ReadAllText(expectedOutputPath);
                var expectedOutputElement = JsonNode.Parse(expectedOutputJson);
                Assert.True(DeepEquals(actualOutputElement, expectedOutputElement),
                    $"Error processing expand test {Path.GetFileName(inputPath)}.\nActual output does not match expected output.\nExpected:\n{expectedOutputElement}\n\nActual:\n{actualOutputElement}");
                break;
            case JsonLdTestType.NegativeEvaluationTest:
                JsonLdProcessorException exception =
                    Assert.Throws<JsonLdProcessorException>(
                        () => JsonLdProcessor.Expand(inputElement, processorOptions));
                Assert.Equal(expectErrorCode, exception.ErrorCode);
                break;
            case JsonLdTestType.PositiveSyntaxTest:
                // Expect test to run without throwing processing errors
                JsonArray _ = JsonLdProcessor.Expand(inputElement, processorOptions);
                break;
            case JsonLdTestType.NegativeSyntaxTest:
                Assert.ThrowsAny<JsonLdProcessorException>(() => JsonLdProcessor.Expand(inputElement, processorOptions));
                break;
        }
    }

    
    public virtual void CompactTests(string testId, JsonLdTestType testType, string inputPath, string contextPath, 
        string expectedOutputPath, JsonLdErrorCode expectedErrorCode, string baseIri,
        string processorMode, string expandContextPath, bool compactArrays, string rdfDirection)
    {
        JsonLdProcessorOptions processorOptions = MakeProcessorOptions(inputPath, baseIri, processorMode, expandContextPath,
            compactArrays, rdfDirection);
        var inputJson = File.ReadAllText(inputPath);
        var contextJson = contextPath == null ? null : File.ReadAllText(contextPath);
        var inputElement = JsonNode.Parse(inputJson);
        JsonNode contextElement = contextJson == null ? JsonNode.Parse("{}") : JsonNode.Parse(contextJson);
        switch (testType)
        {
            case JsonLdTestType.PositiveEvaluationTest:
                var expectedOutputJson = File.ReadAllText(expectedOutputPath);
                var expectedOutputElement = JsonNode.Parse(expectedOutputJson);
                JsonNode actualOutputElement = JsonLdProcessor.Compact(inputElement, contextElement, processorOptions);
                Assert.True(DeepEquals(actualOutputElement, expectedOutputElement),
                    $"Error processing compact test {Path.GetFileName(inputPath)}.\nActual output does not match expected output.\nExpected:\n{expectedOutputElement}\n\nActual:\n{actualOutputElement}");
                break;
            case JsonLdTestType.NegativeEvaluationTest:
                JsonLdProcessorException exception = Assert.Throws<JsonLdProcessorException>(() =>
                    JsonLdProcessor.Compact(inputElement, contextElement, processorOptions));
                Assert.Equal(expectedErrorCode, exception.ErrorCode);
                break;
            default:
                Assert.Fail($"Test type {testType} has not been implemented for Compact tests");
                break;
        }
    }

    public virtual void FlattenTests(string testId, JsonLdTestType testType, string inputPath, string contextPath, 
        string expectedOutputPath, JsonLdErrorCode expectedErrorCode, string baseIri,
        string processorMode, string expandContextPath, bool compactArrays, string rdfDirection)
    {
        JsonLdProcessorOptions processorOptions = MakeProcessorOptions(inputPath, baseIri, processorMode, expandContextPath,
            compactArrays, rdfDirection);
        var inputJson = File.ReadAllText(inputPath);
        var contextJson = contextPath == null ? null : File.ReadAllText(contextPath);
        var inputElement = JsonNode.Parse(inputJson);
        JsonNode contextElement = contextJson == null ? JsonValue.Create<string>(null) : JsonNode.Parse(contextJson);

        switch (testType)
        {
            case JsonLdTestType.PositiveEvaluationTest:
                var expectedOutputJson = File.ReadAllText(expectedOutputPath);
                var expectedOutputElement = JsonNode.Parse(expectedOutputJson);

                JsonNode actualOutputElement = JsonLdProcessor.Flatten(inputElement, contextElement, processorOptions);
                Assert.True(DeepEquals(actualOutputElement, expectedOutputElement),
                    $"Error processing flatten test {Path.GetFileName(inputPath)}.\nActual output does not match expected output.\nExpected:\n{expectedOutputElement}\n\nActual:\n{actualOutputElement}");
                break;
            case JsonLdTestType.NegativeEvaluationTest:
                JsonLdProcessorException exception = Assert.Throws<JsonLdProcessorException>(() =>
                    JsonLdProcessor.Flatten(inputElement, contextElement, processorOptions));
                Assert.Equal(expectedErrorCode, exception.ErrorCode);
                break;
            default:
                Assert.Fail($"Test type {testType} has not been implemented for Flatten tests");
                break;
        }
    }

    public virtual void JsonLdParserTests(string testId, JsonLdTestType testType, string inputPath, string contextPath, 
        string expectedOutputPath, JsonLdErrorCode expectedErrorCode, string baseIri,
        string processorMode, string expandContextPath, bool compactArrays, string rdfDirection)
    {
        JsonLdProcessorOptions processorOptions = MakeProcessorOptions(inputPath, baseIri, processorMode, expandContextPath,
            compactArrays, rdfDirection);
        var jsonldParser = new JsonLdParser(processorOptions);
        var actualStore = new TripleStore();

        switch (testType)
        {
            case JsonLdTestType.PositiveEvaluationTest:
                var nqParser = new NQuadsParser(NQuadsSyntax.Rdf11);
                var expectedStore = new TripleStore();
                nqParser.Load(expectedStore, expectedOutputPath);
                FixStringLiterals(expectedStore);
                jsonldParser.Load(actualStore, inputPath);
                Assert.True(expectedStore.Graphs.Count.Equals(actualStore.Graphs.Count) ||
                            (expectedStore.Graphs.Count == 0 && actualStore.Graphs.Count == 1 &&
                             actualStore.Graphs[(IRefNode)null].IsEmpty),
                    $"Test failed for input {Path.GetFileName(inputPath)}.\r\nActual graph count {actualStore.Graphs.Count} does not match expected graph count {expectedStore.Graphs.Count}.");
                //AssertStoresEqual(expectedStore, actualStore, Path.GetFileName(inputPath));
                TestTools.AssertEqual(expectedStore, actualStore, _output);
                break;

            case JsonLdTestType.NegativeEvaluationTest:
                JsonLdProcessorException exception =
                    Assert.Throws<JsonLdProcessorException>(() => jsonldParser.Load(actualStore, inputPath));
                Assert.Equal(expectedErrorCode, exception.ErrorCode);
                break;

            case JsonLdTestType.PositiveSyntaxTest:
                // Positive syntax test should load input file without raising any exceptions
                jsonldParser.Load(actualStore, inputPath);
                break;

            default:
                Assert.Fail($"Test type {testType} is not currently supported for the JSON-LD Parser tests");
                break;
        }
    }

#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
    public virtual void JsonLdWriterTests(string testId, JsonLdTestType testType, string inputPath, string contextPath, 
        string expectedOutputPath, JsonLdErrorCode expectErrorCode, bool useNativeTypes, bool useRdfType, bool ordered, string rdfDirection)
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
    {
        var nqParser = new NQuadsParser(NQuadsSyntax.Rdf11);
        var input = new TripleStore();
        nqParser.Load(input, inputPath);
        FixStringLiterals(input);
        var writerOptions = new JsonLdWriterOptions
            {UseNativeTypes = useNativeTypes, UseRdfType = useRdfType, Ordered = ordered};
        if (rdfDirection != null)
        {
            switch (rdfDirection)
            {
                case "i18n-datatype":
                    writerOptions.RdfDirection = JsonLdRdfDirectionMode.I18NDatatype;
                    break;
                case "compound-literal":
                    writerOptions.RdfDirection = JsonLdRdfDirectionMode.CompoundLiteral;
                    break;
                default:
                    throw new Exception($"Test {testId} specifies an unrecognized value for the rdfDirection option: {rdfDirection}.");
            }
        }
        var jsonLdWriter = new JsonLdWriter(writerOptions);

        switch (testType)
        {
            case JsonLdTestType.PositiveEvaluationTest:
            {
                JsonArray actualOutput = jsonLdWriter.SerializeStore(input);
                var expectedOutputJson = File.ReadAllText(expectedOutputPath);
                var expectedOutput = JsonNode.Parse(expectedOutputJson);

                try
                {
                    Assert.True(DeepEquals(expectedOutput, actualOutput, true, true),
                        $"Test failed for input {Path.GetFileName(inputPath)}\nExpected:\n{expectedOutput}\nActual:\n{actualOutput}");
                }
                catch (DeepEqualityFailure ex)
                {
                    Assert.Fail($"Test failed for input {Path.GetFileName(inputPath)}\nExpected:\n{expectedOutput}\nActual:\n{actualOutput}\nMatch Failured: {ex}");
                }

                break;
            }
            case JsonLdTestType.NegativeEvaluationTest:
            {
                JsonLdProcessorException exception = Assert.Throws<JsonLdProcessorException>(() => jsonLdWriter.SerializeStore(input));
                Assert.Equal(expectErrorCode, exception.ErrorCode);
                break;
            }
            case JsonLdTestType.PositiveSyntaxTest:
                JsonArray _ = jsonLdWriter.SerializeStore(input);
                break;
            case JsonLdTestType.NegativeSyntaxTest:
                Assert.ThrowsAny<JsonLdProcessorException>(() => jsonLdWriter.SerializeStore(input));
                break;
        }
    }

    public virtual void JsonLdFramingTests(string testId, JsonLdTestType testType, string inputPath, string framePath, 
        string expectedOutputPath, JsonLdErrorCode expectErrorCode, string processingMode, 
        bool pruneBlankNodeIdentifiers, bool? omitGraph, bool ordered)
    {
        var inputJson = File.ReadAllText(inputPath);
        var frameJson = File.ReadAllText(framePath);
        JsonLdProcessingMode jsonLdProcessingMode = "json-ld-1.0".Equals(processingMode)
            ? JsonLdProcessingMode.JsonLd10
            : JsonLdProcessingMode.JsonLd11FrameExpansion;
        var options = new JsonLdProcessorOptions
        {
            ProcessingMode = jsonLdProcessingMode,
            PruneBlankNodeIdentifiers = pruneBlankNodeIdentifiers,
            Ordered = ordered,
        };
        if (omitGraph.HasValue) options.OmitGraph = omitGraph.Value;
        var inputElement = JsonNode.Parse(inputJson);
        var frameElement = JsonNode.Parse(frameJson);
        
        switch (testType)
        {
            case JsonLdTestType.PositiveEvaluationTest:
                var expectedOutputJson = File.ReadAllText(expectedOutputPath);
                var expectedOutputElement = JsonNode.Parse(expectedOutputJson);
                JsonObject actualOutput = JsonLdProcessor.Frame(inputElement, frameElement, options);
                Assert.True(DeepEquals(expectedOutputElement, actualOutput),
                    $"Test failed for input {Path.GetFileName(inputPath)}\nExpected:\n{expectedOutputElement}\nActual:\n{actualOutput}");
                break;
            case JsonLdTestType.NegativeEvaluationTest:
                JsonLdProcessorException exception = Assert.ThrowsAny<JsonLdProcessorException>(() =>
                    JsonLdProcessor.Frame(inputElement, frameElement, options));
                Assert.Equal(expectErrorCode, exception.ErrorCode);
                break;
            case JsonLdTestType.PositiveSyntaxTest:
                JsonLdProcessor.Frame(inputElement, frameElement, options);
                break;
            case JsonLdTestType.NegativeSyntaxTest:
                Assert.ThrowsAny<JsonLdProcessorException>(() =>
                    JsonLdProcessor.Frame(inputElement, frameElement, options));
                break;
        }
        
    }

    private static bool DeepEquals(JsonNode t1, JsonNode t2, bool arraysAreOrdered = false)
    {
        if (t1 == null) return t2 == null;
        if (t2 == null) return false;
        var t1Kind = t1.GetValueKind();
        var t2Kind = t2.GetValueKind();
        if (t1Kind == JsonValueKind.Null && t2Kind == JsonValueKind.Null) return true;
        if (t1Kind == JsonValueKind.Null && t2Kind == JsonValueKind.String) return t2.GetValue<string>() == null;
        if (t2Kind == JsonValueKind.Null && t1Kind == JsonValueKind.String) return t1.GetValue<string>() == null;
        if (t1Kind != t2Kind) return false;
        switch (t1Kind)
        {
            case JsonValueKind.Array:
                return DeepEquals(t1.AsArray(), t2.AsArray(), arraysAreOrdered);
            case JsonValueKind.Object:
                return DeepEquals(t1.AsObject(), t2.AsObject(), arraysAreOrdered);
            default:
                return t1.Equals(t2);
        }
    }

    private static bool DeepEquals(JsonArray a1, JsonArray a2, bool arraysAreOrdered = false)
    {
        if (a1.Count != a2.Count) return false;
        if (arraysAreOrdered)
        {
            return !a1.Where((t, i) => !DeepEquals(t, a2[i], true)).Any();
        }
        JsonArray a2Clone = a2.DeepClone().AsArray();
        foreach (JsonNode item in a1)
        {
            var matched = false;
            for (var j = 0; j < a2Clone.Count; j++)
            {
                if (DeepEquals(item, a2Clone[j], false))
                {
                    a2Clone.RemoveAt(j);
                    matched = true;
                    break;
                }
            }
            if (!matched) return false;
        }
        return true;
    }

    private static bool DeepEquals(JsonObject o1, JsonObject o2, bool arraysAreOrdered = false)
    {
        if (o1.Count != o2.Count) return false;
        return o1.All(p => o2.ContainsKey(p.Key)) &&
               o1.All(p => DeepEquals(p.Value, o2[p.Key], arraysAreOrdered));
    }

    private static bool DeepEquals(JsonNode token1, JsonNode token2, bool ignoreArrayOrder, bool throwOnMismatch)
    {
        if (token1.GetValueKind() != token2.GetValueKind())
        {
            if (throwOnMismatch) throw new DeepEqualityFailure(token1, token2);
            return false;
        }
        switch (token1.GetValueKind())
        {
            case JsonValueKind.Object:
                if (!(token1 is JsonObject o1 && token2 is JsonObject o2)) return false;
                foreach (var p in o1)
                {
                    if (o2[p.Key] == null)
                    {
                        if (throwOnMismatch) throw new DeepEqualityFailure(token1, token2);
                        return false;
                    }
                    if (!DeepEquals(p.Value, o2[p.Key], ignoreArrayOrder, throwOnMismatch))
                    {
                        if (throwOnMismatch) throw new DeepEqualityFailure(p.Value, o2[p.Key]);
                        return false;
                    }
                }
                if (o2.Any(p2 => o1[p2.Key] == null))
                {
                    if (throwOnMismatch) throw new DeepEqualityFailure(token1, token2);
                    return false;
                }
                return true;
            case JsonValueKind.Array:
                if (!(token1 is JsonArray a1 && token2 is JsonArray a2)) return false;
                if (a1.Count != a2.Count)
                {
                    if (throwOnMismatch) throw new DeepEqualityFailure(token1, token2);
                    return false;
                }

                if (!ignoreArrayOrder)
                {
                    for (var i = 0; i < a1.Count; i++)
                    {
                        if (!DeepEquals(a1[i], a2[i], ignoreArrayOrder, throwOnMismatch))
                        {
                            if (throwOnMismatch) throw new DeepEqualityFailure(token1, token2);
                            return false;
                        }

                    }
                    return true;
                }
                var unmatchedItems = token2.DeepClone().AsArray().ToList();
                foreach (JsonNode item1 in a1)
                {
                    if (unmatchedItems.Count == 0)
                    {
                        if (throwOnMismatch) throw new DeepEqualityFailure(token1, token2);
                        return false;
                    }
                    var match = unmatchedItems.FindIndex(x => DeepEquals(item1, x, ignoreArrayOrder, false));
                    if (match >= 0)
                    {
                        unmatchedItems.RemoveAt(match);
                    }
                    else
                    {
                        if (throwOnMismatch) throw new DeepEqualityFailure(token1, token2);
                        return false;
                    }
                }
                return unmatchedItems.Count == 0;
            default:
                return JsonNode.DeepEquals(token1, token2);
        }
    }

    private class DeepEqualityFailure : Exception
    {
        public DeepEqualityFailure(JsonNode expected, JsonNode actual) : base(
            $"DeepEquality failed at {expected.GetPath()}.\nExpected: {expected}\nActual: {actual}")
        {
            
        }
    }

    private static void FixStringLiterals(ITripleStore store)
    {
        var xsdString = new Uri("http://www.w3.org/2001/XMLSchema#string");
        foreach (IGraph graphToUpdate in store.Graphs)
        {
            foreach (Triple t in graphToUpdate.Triples.ToList())
            {
                if (t.Object is ILiteralNode literalNode && string.IsNullOrEmpty(literalNode.Language) &&
                    literalNode.DataType == null)
                {
                    graphToUpdate.Retract(t);
                    graphToUpdate.Assert(
                        new Triple(t.Subject, t.Predicate,
                            graphToUpdate.CreateLiteralNode(literalNode.Value, xsdString)));
                }
            }
        }
    }

    private static JsonLdProcessorOptions MakeProcessorOptions(string inputPath, 
        string baseIri,
        string processorMode,
        string expandContextPath, 
        bool compactArrays,
        string rdfDirection)
    {
        var processorOptions = new JsonLdProcessorOptions
        {
            Base = baseIri != null
                ? new Uri(baseIri)
                : new Uri("http://json-ld.org/test-suite/tests/" + Path.GetFileName(inputPath))
        };
        if (processorMode != null)
            processorOptions.ProcessingMode = processorMode.Equals("json-ld-1.1")
                ? JsonLdProcessingMode.JsonLd11
                : JsonLdProcessingMode.JsonLd10;
        if (expandContextPath != null)
        {
            var expandContextJson = File.ReadAllText(expandContextPath);
            processorOptions.ExpandContext = JsonNode.Parse(expandContextJson);
        }
        processorOptions.CompactArrays = compactArrays;
        if (!string.IsNullOrEmpty(rdfDirection))
        {
            switch (rdfDirection)
            {
                case "i18n-datatype":
                    processorOptions.RdfDirection = JsonLdRdfDirectionMode.I18NDatatype;
                    break;
                case "compound-literal":
                    processorOptions.RdfDirection = JsonLdRdfDirectionMode.CompoundLiteral;
                    break;
                default:
                    throw new Exception($"Unexpected value for rdfDirection option in test {inputPath}");
            }
        }
        return processorOptions;
    }
}