using System.Text;
using System.Text.Json;

namespace Repository.Files.Types;

public class JsonType<T> : IFileType<T> where T : class
{
    public string Extension => ".json";
    public void Write(Stream stream, T DAO)
    {
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        writer.Write(JsonSerializer.Serialize(DAO, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    public T? Read(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return JsonSerializer.Deserialize<T>(reader.ReadToEnd());
    }
}