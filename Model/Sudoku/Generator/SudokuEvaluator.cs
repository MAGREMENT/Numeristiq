using System.Collections.Generic;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Trackers;

namespace Model.Sudoku.Generator;

public class SudokuEvaluator
{
    private readonly SudokuSolver _solver;
    private readonly RatingTracker _rTracker = new();
    private readonly HardestStrategyTracker _hsTracker = new();
    private readonly UsedStrategiesTracker _usTracker = new();

    private readonly List<IEvaluationCriteria> _criterias = new();

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

    public void AddCriteria(IEvaluationCriteria criteria)
    {
        _criterias.Add(criteria);
    }
}

public interface IEvaluationCriteria
{
    bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker usedStrategiesTracker);
}