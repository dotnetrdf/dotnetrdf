using Xunit;
using Xunit.Sdk;

namespace VDS.RDF.XunitExtensions
{
    [XunitTestCaseDiscoverer("VDS.RDF.XunitExtensions.SkippableTheoryDiscoverer", "DynamicSkipExample")]
    public class SkippableTheoryAttribute : TheoryAttribute { }
}
