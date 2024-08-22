using System.Text;

namespace Repository.Files;

public class FileRepository<T> where T : class
{
    private readonly string? _filePath;
    private readonly IFileType<T> _type;
    
    protected FileRepository(string fileName, bool searchParentDirectories, bool createIfNotFound, IFileType<T> type)
    {
        _type = type;
        
        var directory = Directory.GetCurrentDirectory();
        var fullName = fileName + type.Extension;
        var initialPath = $@"{directory}\{fullName}";
        _filePath = initialPath;
        if (File.Exists(initialPath)) return;
        
        if (searchParentDirectories)
        {
            var buffer = Directory.GetParent(_filePath);
            while (buffer is not null)
            {
                var p = $@"{buffer.FullName}\{fullName}";
                if (File.Exists(p))
                {
                    _filePath = p;
                    return;
                }

                buffer = Directory.GetParent(buffer.FullName);
            }
        }

        if (createIfNotFound)
        {
            try
            {
                using var stream = File.Create(initialPath);
                return;
            }
            catch
            {
                //ignored
            }
        }

        _filePath = null;
    }

    public void TearDown()
    {
        if (_filePath is null) return;

        try
        {
            File.Delete(_filePath);
        }
        catch
        {
            //ignored
        }
    }
    
    protected T? Download()
    {
        if (_filePath is null) return null;
        
        try
        {
            using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
           
            return _type.Read(stream);
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    protected bool Upload(T DAO)
    {
        if (_filePath is null) return false;
        
        try
        {
            using var stream = new FileStream(_filePath, FileMode.Truncate, FileAccess.Write, FileShare.None);

            _type.Write(stream, DAO);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

public interface IFileType<T> where T : class
{
    string Extension { get; }
    void Write(Stream stream, T DAO);
    T? Read(Stream stream);
}