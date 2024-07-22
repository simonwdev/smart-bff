namespace SmartBff.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Concatenates the members of a <see cref="IEnumerable{T}"/>
    /// collection of type String, using the specified separator between each member.
    /// </summary>
    public static string Join(this IEnumerable<string> values, string separator = ",")
    {
        return string.Join(separator, values);
    }
}