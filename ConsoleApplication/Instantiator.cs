using Model.Kakuros;
using Model.Kakuros.Strategies;
using Model.Nonograms;
using Model.Nonograms.Solver;
using Model.Nonograms.Solver.Strategies;
using Model.Sudokus.Solver;
using Model.Tectonics;
using Model.Tectonics.Solver;
using Model.Tectonics.Solver.Strategies;
using Model.Tectonics.Solver.Strategies.AlternatingInference;
using Model.Tectonics.Solver.Strategies.AlternatingInference.Types;
using Repository;
using NakedSingleStrategy = Model.Tectonics.Solver.Strategies.NakedSingleStrategy;

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

    public KakuroSolver InstantiateKakuroSolver()
    {
        var solver = new KakuroSolver(new RecursiveKakuroCombinationCalculator());
        solver.StrategyManager.AddStrategies(new Model.Kakuros.Strategies.NakedSingleStrategy(),
            new AmountCoherencyStrategy(),
            new CombinationCoherencyStrategy());
        
        return solver;
    }

    public NonogramSolver InstantiateNonogramSolver()
    {
        var solver = new NonogramSolver();
        solver.StrategyManager.AddStrategies(new PerfectSpaceStrategy(),
            new NotEnoughSpaceStrategy(),
            new BridgingStrategy());
        
        return solver;
    }
}