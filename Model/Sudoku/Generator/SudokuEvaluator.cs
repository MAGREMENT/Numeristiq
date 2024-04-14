using Model.Sudoku.Generator.Criterias;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Trackers;
using Model.Utility.Collections;

namespace Model.Sudoku.Generator;

public class SudokuEvaluator
{
    private static readonly IEvaluationCriteria[] AllCriterias =
    {
        new CantUseStrategyCriteria(),
        new MustUseStrategyCriteria(),
        new MaximumRatingCriteria(),
        new MinimumRatingCriteria(),
        new MaximumHardestDifficultyCriteria(),
        new MinimumHardestDifficultyCriteria()
    };
    
    private readonly SudokuSolver _solver;
    private readonly RatingTracker _rTracker = new();
    private readonly HardestStrategyTracker _hsTracker = new();
    private readonly UsedStrategiesTracker _usTracker = new();

    private readonly UniqueList<IEvaluationCriteria> _criterias = new();

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
        _criterias.Add(criteria, i =>
        {
            _criterias.RemoveAt(i);
            _criterias.InsertAt(criteria, i);
        });
    }

    public void RemoveCriteria(int i)
    {
        _criterias.RemoveAt(i);
    }
}

public interface IEvaluationCriteria
{
    string Name { get; }
    bool IsValid(GeneratedSudokuPuzzle puzzle, UsedStrategiesTracker usedStrategiesTracker);
}