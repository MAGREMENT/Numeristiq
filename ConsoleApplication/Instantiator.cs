using Model.Core;
using Model.Kakuros;
using Model.Nonograms.Solver;
using Model.Repositories;
using Model.Sudokus.Solver;
using Model.Tectonics.Solver;
using Repository;

namespace ConsoleApplication;

public class Instantiator
{
    private readonly IStrategyRepository<SudokuStrategy> sudokuRepository = new SudokuStrategyJsonRepository("strategies.json", 
        !Program.IsForProduction, true);
    private readonly IStrategyRepository<Strategy<ITectonicSolverData>> tectonicRepository = new HardCodedTectonicStrategyRepository();
    private readonly IStrategyRepository<Strategy<IKakuroSolverData>> kakuroRepository = new HardCodedKakuroStrategyRepository();
    private readonly IStrategyRepository<Strategy<INonogramSolverData>> nonogramRepository = new HardCodedNonogramStrategyRepository();

    private ThemeMultiRepository? _themeRepository;

    public SudokuSolver InstantiateSudokuSolver()
    {
        var solver = new SudokuSolver();
        solver.StrategyManager.AddStrategies(sudokuRepository.GetStrategies());

        return solver;
    }

    public TectonicSolver InstantiateTectonicSolver()
    {
        var solver = new TectonicSolver();
        solver.StrategyManager.AddStrategies(tectonicRepository.GetStrategies());
        
        return solver;
    }

    public KakuroSolver InstantiateKakuroSolver()
    {
        var solver = new KakuroSolver(new RecursiveKakuroCombinationCalculator());
        solver.StrategyManager.AddStrategies(kakuroRepository.GetStrategies());
        
        return solver;
    }

    public NonogramSolver InstantiateNonogramSolver()
    {
        var solver = new NonogramSolver();
        solver.StrategyManager.AddStrategies(nonogramRepository.GetStrategies());
        
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