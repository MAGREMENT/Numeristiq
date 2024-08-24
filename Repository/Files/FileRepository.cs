using Model.Utility;

namespace Repository.Files;

public class FileRepository<T> where T : class
{
    private readonly string? _filePath;
    private readonly IFileType<T> _type;
    
    protected FileRepository(string name, bool searchParentDirectories, bool createIfNotFound, IFileType<T> type)
    {
        _type = type;
        _filePath = PathFinder.Find(name + type.Extension, searchParentDirectories, createIfNotFound);
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