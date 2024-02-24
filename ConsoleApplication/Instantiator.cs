using Model.Helpers.Changes.Buffers;
using Model.Sudoku.Solver;
using Model.Tectonic;
using Repository;

namespace ConsoleApplication;

public class Instantiator
{
    private SudokuSolver? _ssolver;
    private TectonicSolver? _tsolver;

    public bool InstantiateSudokuSolver(out SudokuSolver solver)
    {
        if (_ssolver is not null)
        {
            solver = _ssolver;
            return true;
        }
        
        var repo = new SudokuStrategiesJSONRepository("strategies.json");
        if (!repo.Initialize(true))
        {
            Console.WriteLine("Exception while initializing repository");
            solver = null!;
            return false;
        }

        _ssolver = new SudokuSolver();
        _ssolver.StrategyManager.AddStrategies(repo.Download());

        solver = _ssolver;
        return true;
    }

    public bool InstantiateTectonicSolver(out TectonicSolver solver)
    {
        _tsolver ??= new TectonicSolver();
        solver = _tsolver;
        return true;
    }
}