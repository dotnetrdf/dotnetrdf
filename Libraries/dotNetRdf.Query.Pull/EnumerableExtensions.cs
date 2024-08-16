namespace dotNetRdf.Query.Pull;

/// <summary>
/// Provides extensions to <see cref="IEnumerable{T}"/>
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Return an enumeration of the non-null items in an enumerable of possible null values.
    /// </summary>
    /// <param name="enumerable">The input enumerable to be filtered.</param>
    /// <typeparam name="T">The type of item which <paramref name="enumerable"/> contains.</typeparam>
    /// <returns>An enumerable of non-null <typeparamref name="T"/></returns>
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) where T : class
    {
        return enumerable.Where(t => t != null).Select(t => t!);
    }
}