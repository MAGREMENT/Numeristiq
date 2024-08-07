using Model.Kakuros;
using Model.Kakuros.Strategies;
using Model.Nonograms.Solver;
using Model.Nonograms.Solver.Strategies;
using Model.Repositories;
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
    private IReadOnlyList<SudokuStrategy>? _sudokuStrategies;
    private bool _sudokuInstantiated;

    private ThemeMultiRepository? _themeRepository;

    public SudokuSolver InstantiateSudokuSolver()
    {
        if (!_sudokuInstantiated)
        {
            _sudokuStrategies = new SudokuStrategyJsonRepository("strategies.json", 
                !Program.IsForProduction, true).GetStrategies();
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
            new NakedDoubleStrategy(),
            new HiddenDoubleStrategy(),
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
            new UnreachableSquaresStrategy(),
            new Model.Nonograms.Solver.Strategies.BruteForceStrategy());
        
        return solver;
    }

    public ThemeMultiRepository InstantiateThemeRepository()
    {
        _themeRepository ??= new ThemeMultiRepository(new JsonThemeRepository("themes.json",
            !Program.IsForProduction, true), new HardCodedThemeRepository());
        return _themeRepository;
    }

    public ISudokuBankRepository InstantiateBankRepository() => new MySqlSudokuBankRepository();
}