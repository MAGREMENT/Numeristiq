using System.Collections.Generic;
using Model.Sudoku.Solver.Trackers;

namespace Model.Sudoku.Generator.Criterias;

public class MustUseStrategiesCriteria : IEvaluationCriteria
{
    private readonly List<string> _names = new();

    public void Add(string name) => _names.Add(name);
    
    public bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker usedStrategiesTracker)
    {
        foreach (var name in _names)
        {
            if (!usedStrategiesTracker.WasUsed(name)) return false;
        }

        return true;
    }
}