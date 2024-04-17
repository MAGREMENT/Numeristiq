using Model.Helpers.Changes.Buffers;
using Model.Sudokus.Solver;
using Model.Tectonics;
using Repository;

namespace ConsoleApplication;

public class Instantiator
{
    private SudokuStrategiesJSONRepository? _sRepo;

    public bool InstantiateSudokuSolver(out SudokuSolver solver)
    {
        if (_sRepo is null)
        {
            _sRepo = new SudokuStrategiesJSONRepository("strategies.json");
            if (!_sRepo.Initialize(true))
            {
                Console.WriteLine("Exception while initializing repository");
                solver = null!;
                return false;
            }
        }

        solver = new SudokuSolver();
        solver.StrategyManager.AddStrategies(_sRepo.Download());
        return true;
    }

    public bool InstantiateTectonicSolver(out TectonicSolver solver)
    {
        solver = new TectonicSolver();
        return true;
    }
}