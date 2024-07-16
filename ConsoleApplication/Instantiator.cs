using Model.Kakuros;
using Model.Kakuros.Strategies;
using Model.Nonograms.Solver;
using Model.Nonograms.Solver.Strategies;
using Model.Sudokus.Solver;
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
            _sudokuStrategies = new SudokuStrategyJSONRepository("strategies.json", 
                !IsForProduction, true).GetStrategies();
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
            new Model.Tectonics.Solver.Strategies.BruteForceStrategy());
        
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
        solver.StrategyManager.AddStrategies(new PerfectRemainingSpaceStrategy(),
            new NotEnoughSpaceStrategy(),
            new EdgeValueStrategy(),
            new PerfectValueSpaceStrategy(),
            new ValueCompletionStrategy(),
            new BridgingStrategy(),
            new SplittingStrategy(),
            new ValueOverlayStrategy(),
            new UnreachableSquareStrategy(),
            new Model.Nonograms.Solver.Strategies.BruteForceStrategy());
        
        return solver;
    }
}