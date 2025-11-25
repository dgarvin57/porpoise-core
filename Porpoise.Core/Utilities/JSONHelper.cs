#nullable enable

using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Porpoise.Core.Utilities;

/// <summary>
/// Legacy JSON serialization helper using DataContractJsonSerializer (pre-System.Text.Json era).
/// Used throughout the original Porpoise codebase for saving/loading objects to JSON.
/// </summary>
public static class JSONHelper
{
    public static string ToJSON<T>(T obj)
    {
        if (obj is null)
            throw new ArgumentNullException(nameof(obj));
        var serializer = new DataContractJsonSerializer(obj.GetType());
        using var ms = new MemoryStream();
        serializer.WriteObject(ms, obj);
        string retVal = Encoding.Default.GetString(ms.ToArray());
        return retVal;
    }

    public static T FromJSON<T>(string json) where T : new()
    {
        ArgumentNullException.ThrowIfNull(json);
        T obj = new();
        byte[] bytes = Encoding.Unicode.GetBytes(json);
        using var ms = new MemoryStream(bytes);
        var serializer = new DataContractJsonSerializer(obj.GetType());
        obj = (T)serializer.ReadObject(ms)!;
        return obj;
    }
}