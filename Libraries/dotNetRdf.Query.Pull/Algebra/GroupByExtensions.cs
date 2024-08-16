using VDS.RDF.Query.Grouping;

namespace dotNetRdf.Query.Pull.Algebra;

internal static class GroupByExtensions {
    public static IEnumerable<string?> GroupingKeyNames(this ISparqlGroupBy grouping)
    {
        if (grouping.Expression is not null)
        {
            yield return grouping.AssignVariable;
        }
        else
        {
            foreach (var v in grouping.Variables.Where(x => x != grouping.AssignVariable)) { yield return v; }
        }

        if (grouping.Child != null)
        {
            foreach (var v in grouping.Child.GroupingKeyNames()) yield return v;
        }
    }
}