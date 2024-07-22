using System.Text.Json;
using IdentityModel.Client;

namespace SmartBff.Extensions;

public static class JsonDocumentExtensions
{
    /// <summary>
    /// Tries to get a string array from a JObject, returning null if the type
    /// is not an array.
    /// </summary>
    /// <param name="json">The json.</param>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public static List<string>? TryGetStringArrayOrNull(this JsonElement json, string name)
    {
        return json.TryGetValue(name).ValueKind == JsonValueKind.Array
            ? json.TryGetStringArray(name).ToList()
            : null;
    }

    /// <summary>
    /// Tries to get a strongly typed object array from a JObject, returning null if the type
    /// is not an array.
    /// </summary>
    /// <param name="json">The json.</param>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public static List<T>? TryGetObjectArrayOrNull<T>(this JsonElement json, string name, Func<JsonElement, T> processor)
        where T : class
    {
        var values = new List<T>();

        var array = json.TryGetValue(name);
        if (array.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in array.EnumerateArray())
            {
                var element = processor?.Invoke(item);
                if (element != null)
                    values.Add(element);
            }

            return values;
        }

        return null;
    }
}