using Model.Sudokus.Solver;
using Model.Tectonics;
using Repository;

namespace ConsoleApplication;

public class Instantiator
{
    private const bool IsForProduction = false;
    
    private IReadOnlyList<SudokuStrategy>? _strategies;
    private bool _sudokuInstantiated;

    public SudokuSolver InstantiateSudokuSolver()
    {
        if (!_sudokuInstantiated)
        {
            var pInstantiator = new PathInstantiator(!IsForProduction, true);
            _strategies = new SudokuStrategiesJSONRepository(pInstantiator.Instantiate("strategies.json"))
                .Download();
            _sudokuInstantiated = true;
        }

        var solver = new SudokuSolver();
        solver.StrategyManager.AddStrategies(_strategies);

        return solver;
    }

    public TectonicSolver InstantiateTectonicSolver()
    {
        return new TectonicSolver();
    }
}