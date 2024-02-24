using Model.Helpers.Changes.Buffers;
using Model.Sudoku.Solver;
using Repository;

namespace ConsoleApplication;

public class Instantiator
{
    private SudokuSolver? _solver;

    public bool InstantiateSudokuSolver(out SudokuSolver solver)
    {
        if (_solver is not null)
        {
            solver = _solver;
            return true;
        }
        
        var repo = new SudokuStrategiesJSONRepository("strategies.json");
        if (!repo.Initialize(true))
        {
            Console.WriteLine("Exception while initializing repository");
            solver = null!;
            return false;
        }

        _solver = new SudokuSolver();
        _solver.StrategyManager.AddStrategies(repo.Download());

        solver = _solver;
        return true;
    }
}