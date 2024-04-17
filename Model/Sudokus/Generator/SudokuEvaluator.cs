using System.Collections.Generic;
using System.Text;
using Model.Helpers.Settings;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Trackers;
using Model.Utility.Collections;

namespace Model.Sudokus.Generator;

public class SudokuEvaluator
{
    private readonly SudokuSolver _solver;
    private readonly RatingTracker _rTracker = new();
    private readonly HardestStrategyTracker _hsTracker = new();
    private readonly UsedStrategiesTracker _usTracker = new();

    private UniqueList<EvaluationCriteria> _criterias = new();

    public SudokuEvaluator(SudokuSolver solver)
    {
        _solver = solver;

        _solver.AddTracker(_rTracker);
        _solver.AddTracker(_hsTracker);
        _solver.AddTracker(_usTracker);
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