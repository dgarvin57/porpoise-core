public static class StringExtensions
{
    public static T XmlDeserializeFromString<T>(this string xml)
    {
        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
        using var reader = new StringReader(xml);
        return (T)serializer.Deserialize(reader)!;
    }
}
