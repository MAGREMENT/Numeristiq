using System.Collections.Generic;
using Model.Core.Settings;
using Model.Core.Trackers;
using Model.Sudokus.Solver;
using Model.Utility.Collections;

namespace Model.Sudokus.Generator;

public class SudokuEvaluator
{
    private readonly SudokuSolver _solver;
    private readonly RatingTracker _rTracker = new();
    private readonly HardestStrategyTracker _hsTracker = new();
    private readonly UsedStrategiesTracker _usTracker = new();
    
    private UniqueList<EvaluationCriteria>? _snapshot;

    public UniqueList<EvaluationCriteria> Criterias { get; private set; } = new();

    public SudokuEvaluator(SudokuSolver solver)
    {
        _solver = solver;

        _rTracker.AttachTo(solver);
        _hsTracker.AttachTo(solver);
        _usTracker.AttachTo(solver);
    }

    public GeneratedSudokuPuzzle? Evaluate(GeneratedSudokuPuzzle puzzle)
    {
        _solver.SetSudoku(puzzle.Puzzle.Copy());
        _solver.Solve();
        
        puzzle.SetEvaluation(_rTracker.Rating, _hsTracker.Hardest);
        foreach (var criteria in Criterias)
        {
            if (!criteria.IsValid(puzzle, _usTracker)) return null;
        }

        return puzzle;
    }

    public void TakeSnapShot()
    {
        _snapshot = Criterias.Copy();
    }

    public void RestoreSnapShot()
    {
        if (_snapshot is null) return;

        Criterias = _snapshot;
        _snapshot = null;
    }
    
    public IReadOnlyList<string> GetUsedStrategiesName()
    {
        string[] result = new string[_solver.StrategyManager.Strategies.Count];
        for (int i = 0; i < _solver.StrategyManager.Strategies.Count; i++)
        {
            result[i] = _solver.StrategyManager.Strategies[i].Name;
        }

        return result;
    }
}

public abstract class EvaluationCriteria : ISettingCollection
{
    protected readonly ISetting[] _settings;
    
    public string Name { get; }
    public IReadOnlyList<IReadOnlySetting> Settings => _settings;

    public event OnSettingUpdate? SettingUpdated;

    protected EvaluationCriteria(string name, params ISetting[] settings)
    {
        Name = name;
        _settings = settings;
    }
    
    public abstract bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker usedStrategiesTracker);
    public void Set(int index, SettingValue value, bool checkValidity)
    {
        if (index < 0 || index >= _settings.Length) return;

        var setting = _settings[index];
        setting.Set(value, checkValidity);
        SettingUpdated?.Invoke(setting);
    }

    public override string ToString()
    {
        return $"{Name} : {_settings.ToStringSequence(", ")}";
    }
}

public delegate void OnSettingUpdate(IReadOnlySetting setting);