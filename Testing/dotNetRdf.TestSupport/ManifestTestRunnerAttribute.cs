using System;
using VDS.RDF;

namespace dotNetRdf.TestSupport;

/// <summary>
/// Attribute used to decorate the methods of <see cref="RdfTestSuite"/> test class indicating the type of test that the test method handles.
/// </summary>
public class ManifestTestRunnerAttribute : Attribute
{
    /// <summary>
    /// The IRI identifier of the type of test handled by the test method.
    /// </summary>
    public readonly string TestType;

    public ManifestTestRunnerAttribute(string testType)
    {
        TestType = testType;
    }
}