using Xunit;
using Xunit.Sdk;

namespace VDS.RDF.XunitExtensions
{
    [XunitTestCaseDiscoverer("DynamicSkipExample.XunitExtensions.SkippableFactDiscoverer", "DynamicSkipExample")]
    public class SkippableFactAttribute : FactAttribute { }
}
