using Model.Repositories;

namespace Repository.Files;

public class FilePuzzlePresetRepository : FileRepository<Dictionary<string, string>>, IPuzzlePresetRepository
{
    private Dictionary<string, string>? _buffer;
    
    public FilePuzzlePresetRepository(string name, bool searchParentDirectories, bool createIfNotFound, 
        IFileType<Dictionary<string, string>> type) : base(name, searchParentDirectories, createIfNotFound, type)
    {
    }

    public IEnumerable<(string, string)> GetPresets()
    {
        _buffer ??= Download();
        return _buffer is null ? Enumerable.Empty<(string, string)>() : Enumerate(_buffer);
    }

    private static IEnumerable<(string, string)> Enumerate(Dictionary<string, string> dic)
    {
        foreach (var entry in dic)
        {
            yield return (entry.Key, entry.Value);
        }
    }
}