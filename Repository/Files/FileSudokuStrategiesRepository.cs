using Model.Core;
using Model.Core.Settings;
using Model.Repositories;
using Model.Sudokus.Solver;

namespace Repository.Files;

public class FileSudokuStrategiesRepository : FileRepository<List<StrategyDAO>>, IStrategyRepository<SudokuStrategy>
{
    private List<StrategyDAO>? _buffer;
    
    public FileSudokuStrategiesRepository(string fileName, bool searchParentDirectories, bool createIfNotFound,
        IFileType<List<StrategyDAO>> type) : base(fileName, searchParentDirectories, createIfNotFound, type)
    {
    }
    
    public void SetStrategies(IReadOnlyList<SudokuStrategy> list)
    {
        _buffer = DAOManager.To(list);
        Upload(_buffer);
    }

    public IEnumerable<SudokuStrategy> GetStrategies()
    {
        _buffer ??= Download();
        return _buffer is null ? Enumerable.Empty<SudokuStrategy>() : DAOManager.To(_buffer);
    }

    public void UpdateStrategy(SudokuStrategy strategy)
    {
        _buffer ??= Download();
        if (_buffer is null) return;

        var index = IndexOf(strategy.Name);
        if (index != -1)
        {
            _buffer[index] = DAOManager.To(strategy);
            Upload(_buffer);
        }
    }

    private int IndexOf(string name)
    {
        for (int i = 0; i < _buffer!.Count; i++)
        {
            if (_buffer[i].Name.Equals(name)) return i;
        }

        return -1;
    }
}

