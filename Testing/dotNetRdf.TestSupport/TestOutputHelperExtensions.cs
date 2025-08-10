using Xunit;

namespace VDS.RDF;

public static class TestOutputHelperExtensions
{
    public static void WriteLine(this ITestOutputHelper output)
    {
        output.WriteLine(string.Empty);
    }
}
