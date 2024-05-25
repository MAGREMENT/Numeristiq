using System.Text;
using System.Text.Json;
using Model;

namespace Repository;

public class JSONRepository<T> : IRepository<T> where T : class?
{
    private readonly string _filePath;

    public JSONRepository(string filePath)
    {
        _filePath = filePath;
    }

    public virtual T? Download()
    {
        return InternalDownload<T>();
    }

    protected TDownload? InternalDownload<TDownload>() where TDownload : class?
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

    protected TDownload? InternalDownload<TDownload>(Stream stream) where TDownload : class?
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

    public virtual bool Upload(T DAO)
    {
        return InternalUpload(DAO);
    }

    protected bool InternalUpload<TUpload>(TUpload DAO)
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
    
    protected bool InternalUpload<TUpload>(TUpload DAO, Stream stream)
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
}