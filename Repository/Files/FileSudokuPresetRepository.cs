using Model.Repositories;
using Model.Sudokus.Solver;
using Repository.Files.Types;

namespace Repository.Files;

public class FileSudokuPresetRepository : IStrategyPresetRepository<SudokuStrategy>
{
    private readonly Stream _stream;
    private readonly JsonType<List<StrategyDAO>> _type = new();
    
    public FileSudokuPresetRepository(Stream stream)
    {
        _stream = stream;
    }

    public void SetStrategies(IReadOnlyList<SudokuStrategy> list)
    {
        _type.Write(_stream, DAOManager.To(list));
    }

    public IEnumerable<SudokuStrategy> GetStrategies()
    {
        var read = _type.Read(_stream);
        return read is null ? Enumerable.Empty<SudokuStrategy>() : DAOManager.To(read);
    }
}