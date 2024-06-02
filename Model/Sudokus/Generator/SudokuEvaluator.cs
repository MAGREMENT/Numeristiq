using System.Collections.Generic;
using System.Text;
using Model.Core.Trackers;
using Model.Helpers.Settings;
using Model.Sudokus.Solver;
using Model.Utility.Collections;

namespace Model.Sudokus.Generator;

public class SudokuEvaluator : IStrategiesContext
{
    private readonly SudokuSolver _solver;
    private readonly RatingTracker<SudokuStrategy, ISudokuSolveResult> _rTracker = new();
    private readonly HardestStrategyTracker<SudokuStrategy, ISudokuSolveResult> _hsTracker = new();
    private readonly UsedStrategiesTracker<SudokuStrategy, ISudokuSolveResult> _usTracker = new();

    private UniqueList<EvaluationCriteria> _criterias = new();

    public SudokuEvaluator(SudokuSolver solver)
    {
        _solver = solver;

        _rTracker.Attach(solver);
        _hsTracker.Attach(solver);
        _usTracker.Attach(solver);
    }

    public GeneratedSudokuPuzzle? Evaluate(GeneratedSudokuPuzzle puzzle)
    {
        _solver.SetSudoku(puzzle.Sudoku.Copy());
        _solver.Solve();
        
        puzzle.SetEvaluation(_rTracker.Rating, _hsTracker.Hardest);
        foreach (var criteria in _criterias)
        {
            if (!criteria.IsValid(puzzle, _usTracker)) return null;
        }

        return puzzle;
    }

    public UniqueList<EvaluationCriteria> GetCriteriasCopy()
    {
        return _criterias.Copy();
    }

    public void SetCriterias(UniqueList<EvaluationCriteria> criteriaList)
    {
        _criterias = criteriaList.Copy();
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
    
    public abstract bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker<SudokuStrategy, ISudokuSolveResult> usedStrategiesTracker);
    public void Set(int index, SettingValue value, bool checkValidity)
    {
        if (index < 0 || index >= _settings.Length) return;

        var setting = _settings[index];
        setting.Set(value, checkValidity);
        SettingUpdated?.Invoke(setting);
    }

    public override string ToString()
    {
        if (_settings.Length == 0) return Name;
        
        var builder = new StringBuilder($"{Name} : {_settings[0]}");
        for (int i = 1; i < _settings.Length; i++)
        {
            builder.Append($", {_settings[i]}");
        }

        return builder.ToString();
    }
}

public delegate void OnSettingUpdate(IReadOnlySetting setting);