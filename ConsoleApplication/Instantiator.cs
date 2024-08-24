using Model.Binairos;
using Model.Core;
using Model.Kakuros;
using Model.Nonograms.Solver;
using Model.Repositories;
using Model.Sudokus.Solver;
using Model.Tectonics.Solver;
using Repository;
using Repository.Files;
using Repository.Files.Types;
using Repository.HardCoded;
using Repository.MySql;

namespace ConsoleApplication;

public class Instantiator
{
    private readonly IStrategyRepository<SudokuStrategy> _sudokuRepository = new FileSudokuStrategiesRepository(
        "strategies", !Program.IsForProduction, true,
        new JsonType<List<StrategyDAO>>());
    private readonly IStrategyRepository<Strategy<ITectonicSolverData>> _tectonicRepository = 
        new HardCodedTectonicStrategyRepository();
    private readonly IStrategyRepository<Strategy<IKakuroSolverData>> _kakuroRepository = 
        new HardCodedKakuroStrategyRepository();
    private readonly IStrategyRepository<Strategy<INonogramSolverData>> _nonogramRepository = 
        new HardCodedNonogramStrategyRepository();
    private readonly IStrategyRepository<Strategy<IBinairoSolverData>> _binairoRepository =
        new HardCodedBinairoStrategyRepository();

    private ThemeMultiRepository? _themeRepository;

    public SudokuSolver InstantiateSudokuSolver()
    {
        var solver = new SudokuSolver();
        solver.StrategyManager.AddStrategies(_sudokuRepository.GetStrategies());

        return solver;
    }

    public TectonicSolver InstantiateTectonicSolver()
    {
        var solver = new TectonicSolver();
        solver.StrategyManager.AddStrategies(_tectonicRepository.GetStrategies());
        
        return solver;
    }

    public KakuroSolver InstantiateKakuroSolver()
    {
        var solver = new KakuroSolver(new RecursiveKakuroCombinationCalculator());
        solver.StrategyManager.AddStrategies(_kakuroRepository.GetStrategies());
        
        return solver;
    }

    public NonogramSolver InstantiateNonogramSolver()
    {
        var solver = new NonogramSolver();
        solver.StrategyManager.AddStrategies(_nonogramRepository.GetStrategies());
        
        return solver;
    }

    public BinairoSolver InstantiateBinairoSolver()
    {
        var solver = new BinairoSolver();
        solver.StrategyManager.AddStrategies(_binairoRepository.GetStrategies());
        
        return solver;
    }

    public ThemeMultiRepository InstantiateThemeRepository()
    {
        _themeRepository ??= new ThemeMultiRepository(new FileThemeRepository("themes",
            !Program.IsForProduction, true, new JsonType<List<ThemeDAO>>()),
            new HardCodedThemeRepository());
        return _themeRepository;
    }

    public ISudokuBankRepository InstantiateBankRepository() => new MySqlSudokuBankRepository();
}