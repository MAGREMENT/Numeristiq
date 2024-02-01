using System.Text;
using System.Text.Json;
using Model;
using Model.Sudoku;

namespace Repository;

public class JSONRepository<T> : IRepository<T>
{
    private string _path = "";
    private bool _pathFound;

    private readonly string _fileName;

    public JSONRepository(string fileName)
    {
        _fileName = fileName;
    }

    public bool UploadAllowed { get; set; } = true;
    
    public void Initialize()
    {
        if (_pathFound) return;

        try
        {
            _path = SearchPathToJSONFile();
            _pathFound = true;
        }
        catch (Exception)
        {
            throw new RepositoryInitializationException("Couldn't find json file");
        }
    }

    public T? Download()
    {
        using var reader = new StreamReader(_path, Encoding.UTF8);

        var result = JsonSerializer.Deserialize<T>(reader.ReadToEnd());

        return result;
    }

    public void Upload(T DAO)
    {
        if (!UploadAllowed) return;
        
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
    }

    public void New(T DAO)
    {
        if (!_pathFound)
        {
            _path = $@"{Directory.GetCurrentDirectory()}\{_fileName}";
            _pathFound = true;
        }
        
        using var writer = new StreamWriter(_path, new FileStreamOptions
        {
            Mode = FileMode.CreateNew,
            Access = FileAccess.Write,
            Share = FileShare.None
        });
        
        writer.Write(JsonSerializer.Serialize(DAO, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    private string SearchPathToJSONFile()
    {
        var current = Directory.GetCurrentDirectory();
        while (!File.Exists($@"{current}\{_fileName}"))
        {
            var buffer = Directory.GetParent(current);
            if (buffer is null) throw new Exception();

            current = buffer.FullName;
        }

        return $@"{current}\{_fileName}";
    }
}