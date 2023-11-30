using System.Text;
using System.Text.Json;
using Model;

namespace Repository;

public class JSONStrategyRepository : IStrategyRepository
{
    private string _path = "";
    private bool _pathSearched;

    public bool UploadAllowed { get; set; } = true;
    
    public void Initialize()
    {
        if (_pathSearched) return;

        try
        {
            _path = SearchPathToJSONFile();
        }
        catch (Exception)
        {
            throw new StrategyRepositoryInitializationException("Couldn't find json file");
        }

        _pathSearched = true;
    }

    public List<StrategyDAO> DownloadStrategies()
    {
        using var reader = new StreamReader(_path, Encoding.UTF8);

        var result = JsonSerializer.Deserialize<List<StrategyDAO>>(reader.ReadToEnd());

        return result ?? new List<StrategyDAO>();
    }

    public void UploadStrategies(List<StrategyDAO> DAOs)
    {
        if (!UploadAllowed) return;
        
        using var writer = new StreamWriter(_path, new FileStreamOptions
        {
            Mode = FileMode.Truncate,
            Access = FileAccess.Write,
            Share = FileShare.None
        });
        
        writer.Write(JsonSerializer.Serialize(DAOs, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
    
    private static string SearchPathToJSONFile()
    {
        var current = Directory.GetCurrentDirectory();
        while (!File.Exists(current + @"\strategies.json"))
        {
            var buffer = Directory.GetParent(current);
            if (buffer is null) throw new Exception();

            current = buffer.FullName;
        }

        return current + @"\strategies.json";
    }
}