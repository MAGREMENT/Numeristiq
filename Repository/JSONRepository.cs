using System.Text;
using System.Text.Json;
using Model;

namespace Repository;

public class JSONRepository<T> : IRepository<T> where T : class?
{
    private readonly string _fileName;

    private string? _path;

    public JSONRepository(string fileName)
    {
        _fileName = fileName;
    }
    
    public bool Initialize(bool createNewOnNoneExistingFound)
    {
        if (_path is not null) return true;

        _path = SearchPathToJSONFile();
        if (_path is not null) return true;
        if (!createNewOnNoneExistingFound) return false;
        
        var buffer = $@"{Directory.GetCurrentDirectory()}\{_fileName}";
        try
        {
            using var stream = File.Create(buffer);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public virtual T? Download()
    {
        return InternalDownload<T>();
    }

    protected TDownload? InternalDownload<TDownload>() where TDownload : class?
    {
        if (_path is null) return null;
        
        using var reader = new StreamReader(_path, Encoding.UTF8);
        try
        {
            return JsonSerializer.Deserialize<TDownload>(reader.ReadToEnd());
        }
        catch (Exception)
        {
            return null;
        }
    }

    public virtual bool Upload(T DAO)
    {
        return InternalUpload(DAO);
    }

    protected bool InternalUpload<TUpload>(TUpload DAO)
    {
        if (_path is null) return false;

        try
        {
            using var writer = new StreamWriter(_path, new FileStreamOptions
            {
                Mode = FileMode.Truncate,
                Access = FileAccess.Write,
                Share = FileShare.None
            });

            writer.Write(JsonSerializer.Serialize(DAO, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private string? SearchPathToJSONFile()
    {
        var current = Directory.GetCurrentDirectory();
        while (!File.Exists($@"{current}\{_fileName}"))
        {
            var buffer = Directory.GetParent(current);
            if (buffer is null) return null;

            current = buffer.FullName;
        }

        return $@"{current}\{_fileName}";
    }
}