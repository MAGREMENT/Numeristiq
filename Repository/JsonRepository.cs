using System.Text;
using System.Text.Json;
using Model;

namespace Repository;

public abstract class JsonRepository
{
    private readonly string _filePath;

    protected JsonRepository(string filePath, bool searchParentDirectories, bool createIfNotFound)
    {
        _filePath = Instantiate(filePath, searchParentDirectories, createIfNotFound);
    }

    protected TDownload? Download<TDownload>() where TDownload : class?
    {
        try
        {
            using var reader = new StreamReader(_filePath, Encoding.UTF8);
            return JsonSerializer.Deserialize<TDownload>(reader.ReadToEnd());
        }
        catch (Exception)
        {
            return null;
        }
    }

    protected TDownload? Download<TDownload>(Stream stream) where TDownload : class?
    {
        try
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return JsonSerializer.Deserialize<TDownload>(reader.ReadToEnd());
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    protected bool Upload<TUpload>(TUpload DAO)
    {
        try
        {
            using var writer = new StreamWriter(_filePath, new FileStreamOptions
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
    
    protected bool Upload<TUpload>(TUpload DAO, Stream stream)
    {
        try
        {
            using var writer = new StreamWriter(stream);

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
    
    private static string Instantiate(string fileName, bool searchParentDirectories, bool createIfNotFound)
    {
        var directory = Directory.GetCurrentDirectory();
        var path = $@"{directory}\{fileName}";
        bool exists = File.Exists(path);

        if (searchParentDirectories && !exists)
        {
            var buffer = Directory.GetParent(path);
            while (buffer is not null)
            {
                var p = $@"{buffer.FullName}\{fileName}";
                if (File.Exists(p))
                {
                    path = p;
                    exists = true;
                    break;
                }

                buffer = Directory.GetParent(buffer.FullName);
            }
        }

        if (createIfNotFound && !exists)
        {
            using var stream = File.Create(path);
        }

        return path;
    }
}