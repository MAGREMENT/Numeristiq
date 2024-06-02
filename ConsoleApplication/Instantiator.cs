using Model.Sudokus.Solver;
using Model.Tectonics;
using Model.Tectonics.Strategies;
using Model.Tectonics.Strategies.AlternatingInference;
using Model.Tectonics.Strategies.AlternatingInference.Types;
using Repository;

namespace ConsoleApplication;

public class Instantiator
{
    private const bool IsForProduction = false;
    
    private IReadOnlyList<SudokuStrategy>? _sudokuStrategies;
    private bool _sudokuInstantiated;

    public SudokuSolver InstantiateSudokuSolver()
    {
        if (!_sudokuInstantiated)
        {
            var pInstantiator = new PathInstantiator(!IsForProduction, true);
            _sudokuStrategies = new SudokuStrategiesJSONRepository(pInstantiator.Instantiate("strategies.json"))
                .Download();
            _sudokuInstantiated = true;
        }

        var solver = new SudokuSolver();
        solver.StrategyManager.AddStrategies(_sudokuStrategies);

        return solver;
    }

    public TectonicSolver InstantiateTectonicSolver()
    {
        var solver = new TectonicSolver();
        solver.StrategyManager.AddStrategies(new NakedSingleStrategy(),
            new HiddenSingleStrategy(),
            new ZoneInteractionStrategy(),
            new AlternatingInferenceGeneralization(new XChainType()),
            new GroupEliminationStrategy(),
            new AlternatingInferenceGeneralization(new AlternatingInferenceChainType()),
            new BruteForceStrategy());
        return solver;
    }
}