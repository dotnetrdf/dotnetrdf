namespace dotNetRdf.Query.PullEvaluation;

public static class EnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) where T : class
    {
        return enumerable.Where(t => t != null).Select(t => t!);
    }
}